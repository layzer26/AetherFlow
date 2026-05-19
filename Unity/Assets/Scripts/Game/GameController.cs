using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AetherFlow.Core;
using AetherFlow.Core.Enums;
using AetherFlow.Engine;

namespace AetherFlow.Unity
{
    // What the player is currently expected to do.
    public enum InputMode { Idle, SelectCard, SelectTargetZone, SelectMoveZone }

    // Singleton that owns the MatchState and drives the game loop via coroutines.
    // Player controls TeamA (Attackers). AI controls TeamB (Defenders).
    public class GameController : MonoBehaviour
    {
        public static GameController Instance { get; private set; }

        public MatchState State        { get; private set; }
        public InputMode  Input        { get; private set; } = InputMode.Idle;
        public Agent?     ActiveAgent  { get; private set; }

        // Valid zones highlighted for current input mode (targets or moves).
        public HashSet<Zone> HighlightedZones { get; private set; } = new();

        // Events – UI components subscribe to these.
        public event Action         OnStateChanged;
        public event Action<string> OnLog;
        public event Action<string> OnMatchOver;

        private const int MaxRounds            = 25;
        private const int SpikeDetonationRounds = 4;
        private const int DefuseRoundsRequired  = 2;
        private static readonly string[] PlantZones = { "A Site", "B Site" };

        // Pending input signals (set by external callers via the public API below).
        private Card?  _chosenCard;
        private Zone?  _chosenZone;
        private bool   _inputReady;

        // ── Lifecycle ────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void StartMatch(MatchState state)
        {
            State = state;
            StartCoroutine(MatchLoop());
        }

        // ── Match Loop ───────────────────────────────────────────────────────

        private IEnumerator MatchLoop()
        {
            Log("=== MATCH START — You are the Attackers ===");
            NotifyStateChanged();

            while (State.Winner == null && State.TurnNumber < MaxRounds)
            {
                State.TurnNumber++;
                Log($"\n— Round {State.TurnNumber} —");

                yield return StartCoroutine(PlayerTurn());
                if (State.Winner != null) break;

                yield return StartCoroutine(AiTurn());
                if (State.Winner != null) break;

                TickSpike();
                TickZoneEffects();
                CheckWin();
                NotifyStateChanged();
            }

            if (State.Winner == null) State.Winner = "Defenders";
            OnMatchOver?.Invoke(State.Winner);
        }

        // ── Player Turn ──────────────────────────────────────────────────────

        private IEnumerator PlayerTurn()
        {
            Log("YOUR TURN");
            var enemies = AliveEnemies(State.TeamA);

            foreach (var agent in State.TeamA.Where(a => a.IsAlive).ToList())
            {
                if (HandleStun(agent)) continue;

                ActiveAgent = agent;
                Log($"{agent.Name} ({agent.Role}) — {agent.Health}/{agent.MaxHealth} HP  [{agent.CurrentZone.ZoneName}]");

                yield return StartCoroutine(PhaseSelectCard(agent, enemies));
                NotifyStateChanged();
                if (State.Winner != null) yield break;

                yield return StartCoroutine(PhaseSelectMove(agent));
                TryPlant(agent);
                CheckWin();
                NotifyStateChanged();
                if (State.Winner != null) yield break;

                yield return new WaitForSeconds(0.2f);
            }

            ActiveAgent = null;
            Input = InputMode.Idle;
        }

        // Waits for the player to tap a card (or skip).
        private IEnumerator PhaseSelectCard(Agent agent, List<Agent> enemies)
        {
            Input = InputMode.SelectCard;
            HighlightedZones.Clear();
            _chosenCard = null;
            _inputReady = false;
            NotifyStateChanged();

            while (!_inputReady) yield return null;

            if (_chosenCard == null) { Input = InputMode.Idle; yield break; }

            // If the card needs a zone target, wait for zone selection.
            Zone? targetZone = null;
            if (NeedsZoneTarget(_chosenCard))
            {
                HighlightedZones = ValidCardTargets(_chosenCard, agent);
                Input = InputMode.SelectTargetZone;
                _chosenZone = null;
                _inputReady = false;
                NotifyStateChanged();

                while (!_inputReady) yield return null;

                targetZone = _chosenZone;
                HighlightedZones.Clear();
            }

            var result = CardResolver.Resolve(_chosenCard, agent, targetZone, enemies, State);
            Log(result.Description);
            foreach (var entry in result.LogEntries) Log(entry);

            Input = InputMode.Idle;
        }

        // Waits for the player to tap a destination zone (or skip).
        private IEnumerator PhaseSelectMove(Agent agent)
        {
            HighlightedZones = ValidMoveZones(agent);
            Input = InputMode.SelectMoveZone;
            _chosenZone = null;
            _inputReady = false;
            NotifyStateChanged();

            while (!_inputReady) yield return null;

            HighlightedZones.Clear();
            Input = InputMode.Idle;

            if (_chosenZone == null || _chosenZone == agent.CurrentZone) yield break;

            if (!IsSmoked(_chosenZone) && agent.CurrentZone.Neighbours.Contains(_chosenZone))
            {
                Log($"{agent.Name} moves → {_chosenZone.ZoneName}");
                agent.CurrentZone = _chosenZone;
            }
            else if (IsSmoked(_chosenZone))
            {
                Log($"{_chosenZone.ZoneName} is smoked — can't move there.");
            }
        }

        // ── AI Turn ──────────────────────────────────────────────────────────

        private IEnumerator AiTurn()
        {
            Log("AI TURN (Defenders)");
            var enemies = AliveEnemies(State.TeamB);

            foreach (var agent in State.TeamB.Where(a => a.IsAlive).ToList())
            {
                if (HandleStun(agent)) continue;

                // Card.
                var (card, targetZone) = AiSelectCard(agent, enemies);
                if (card != null)
                {
                    var result = CardResolver.Resolve(card, agent, targetZone, enemies, State);
                    Log(result.Description);
                    foreach (var e in result.LogEntries) Log(e);
                }

                // Move toward spike (to defuse) or toward A Site.
                var dest = State.IsSpikePlanted && State.SpikeZone != null
                    ? State.SpikeZone
                    : State.Map.GetZone("A Site");

                var next = State.Map.GetNextZoneToward(agent.CurrentZone, dest);
                if (next != null && !IsSmoked(next))
                {
                    agent.CurrentZone = next;
                    Log($"[AI] {agent.Name} moves → {next.ZoneName}");
                }

                TryDefuse(agent);
                CheckWin();
                NotifyStateChanged();
                yield return new WaitForSeconds(0.7f);
                if (State.Winner != null) yield break;
            }
        }

        // Mirrors the CLI AI logic — picks the best card for this agent.
        private (Card? card, Zone? targetZone) AiSelectCard(Agent agent, List<Agent> enemies)
        {
            var deck = agent.AgentDeck;

            if (agent.UltimateCharge >= 100)
            {
                var ult = deck.FirstOrDefault(c => c.IsUltimate);
                if (ult != null) { agent.UltimateCharge = 0; return (ult, null); }
            }

            if (agent.Health < agent.MaxHealth * 0.4f)
            {
                var heal = deck.FirstOrDefault(c => c.EffectType == EffectType.Heal && !c.IsUltimate);
                if (heal != null) return (heal, null);
            }

            var dmg = deck.FirstOrDefault(c => c.EffectType == EffectType.Damage && !c.IsUltimate);
            if (dmg != null)
            {
                var z = BestEnemyZone(dmg, agent, enemies);
                if (z != null) return (dmg, z);
            }

            var stun = deck.FirstOrDefault(c => c.EffectType == EffectType.Stun && !c.IsUltimate);
            if (stun != null)
            {
                var z = BestEnemyZone(stun, agent, enemies);
                if (z != null) return (stun, z);
            }

            var buff = deck.FirstOrDefault(c => c.EffectType == EffectType.Buff && !c.IsUltimate);
            if (buff != null) return (buff, null);

            return (null, null);
        }

        private Zone? BestEnemyZone(Card card, Agent caster, List<Agent> enemies)
        {
            var reachable = card.Range >= 99
                ? State.Map.Zones.ToList()
                : State.Map.GetZonesWithinRange(caster.CurrentZone, card.Range)
                          .Append(caster.CurrentZone)
                          .ToList();

            return reachable
                .Where(z => enemies.Any(e => e.CurrentZone == z))
                .OrderByDescending(z => enemies.Count(e => e.CurrentZone == z))
                .FirstOrDefault();
        }

        // ── Public Input API (called by UI) ──────────────────────────────────

        // Called by the card hand when the player taps a card.
        public void PlayerSelectCard(Card card)
        {
            if (Input != InputMode.SelectCard) return;
            _chosenCard  = card;
            _inputReady  = true;
        }

        // Called when player taps "Skip Card" or "Skip Move".
        public void PlayerSkip()
        {
            if (Input == InputMode.Idle) return;
            _chosenCard  = null;
            _chosenZone  = null;
            _inputReady  = true;
        }

        // Called by MapView when the player taps a zone.
        public void PlayerSelectZone(Zone zone)
        {
            if (Input == InputMode.SelectTargetZone || Input == InputMode.SelectMoveZone)
            {
                _chosenZone = zone;
                _inputReady = true;
            }
        }

        // ── Spike ────────────────────────────────────────────────────────────

        private void TryPlant(Agent agent)
        {
            if (State.IsSpikePlanted) return;
            if (!PlantZones.Contains(agent.CurrentZone.ZoneName)) return;
            State.IsSpikePlanted = true;
            State.SpikeZone      = agent.CurrentZone;
            State.SpikeTimer     = SpikeDetonationRounds;
            State.DefuseProgress = 0;
            Log($"*** SPIKE PLANTED at {agent.CurrentZone.ZoneName}! {SpikeDetonationRounds} rounds left ***");
        }

        private void TryDefuse(Agent agent)
        {
            if (!State.IsSpikePlanted || State.SpikeZone == null) return;
            if (agent.CurrentZone != State.SpikeZone) return;
            State.DefuseProgress++;
            Log($"[AI] {agent.Name} defusing… ({State.DefuseProgress}/{DefuseRoundsRequired})");
            if (State.DefuseProgress >= DefuseRoundsRequired)
            {
                State.Winner = "Defenders";
                Log("*** SPIKE DEFUSED — Defenders win! ***");
            }
        }

        private void TickSpike()
        {
            if (!State.IsSpikePlanted || State.Winner != null) return;
            State.SpikeTimer--;
            Log($"[Spike] {State.SpikeTimer} round(s) until detonation.");
            if (State.SpikeTimer <= 0)
            {
                State.Winner = "Attackers";
                Log("*** BOOM — Attackers win! ***");
            }
        }

        private void TickZoneEffects()
        {
            foreach (var e in State.ActiveZoneEffects) e.TurnsRemaining--;
            State.ActiveZoneEffects.RemoveAll(e => e.TurnsRemaining <= 0);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private bool HandleStun(Agent agent)
        {
            if (!agent.IsStunned) return false;
            Log($"{agent.Name} is stunned — skipping.");
            agent.IsStunned = false;
            return true;
        }

        private void CheckWin()
        {
            if (State.Winner != null) return;
            if (!State.TeamB.Any(a => a.IsAlive)) State.Winner = "Attackers";
            else if (!State.TeamA.Any(a => a.IsAlive)) State.Winner = "Defenders";
        }

        private bool IsSmoked(Zone z) =>
            State.ActiveZoneEffects.Any(e => e.Zone == z && e.EffectType == EffectType.Smoke);

        private static bool NeedsZoneTarget(Card card) =>
            card.TargetType == TargetType.Single ||
            card.TargetType == TargetType.Area   ||
            card.EffectType == EffectType.Smoke;

        private HashSet<Zone> ValidCardTargets(Card card, Agent agent)
        {
            if (card.Range >= 99) return new HashSet<Zone>(State.Map.Zones);
            return new HashSet<Zone>(
                State.Map.GetZonesWithinRange(agent.CurrentZone, card.Range)
                         .Append(agent.CurrentZone));
        }

        private HashSet<Zone> ValidMoveZones(Agent agent) =>
            new HashSet<Zone>(agent.CurrentZone.Neighbours.Where(n => !IsSmoked(n)));

        private List<Agent> AliveEnemies(List<Agent> ownTeam) =>
            (ownTeam == State.TeamA ? State.TeamB : State.TeamA)
            .Where(a => a.IsAlive).ToList();

        private void NotifyStateChanged() => OnStateChanged?.Invoke();
        private void Log(string msg)       => OnLog?.Invoke(msg);
    }
}
