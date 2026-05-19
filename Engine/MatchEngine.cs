using System;
using System.Collections.Generic;
using System.Linq;
using AetherFlow.Core;
using AetherFlow.Core.Enums;

namespace AetherFlow.Engine
{
    public class MatchEngine
    {
        private readonly MatchState _state;
        private const int MaxRounds = 25;
        private const int SpikeDetonationRounds = 4;
        private const int DefuseRoundsRequired = 2;

        // Site zones where attackers may plant the spike.
        private static readonly string[] PlantZones = { "A Site", "B Site" };

        public MatchEngine(MatchState state)
        {
            _state = state;
        }

        // Runs the match to completion and returns the winner string.
        public string RunMatch(Action<string> log)
        {
            log("=== AETHERFLOW MATCH START ===");
            log($"Attackers: {string.Join(", ", _state.TeamA.Select(a => a.Name))}");
            log($"Defenders: {string.Join(", ", _state.TeamB.Select(a => a.Name))}");

            while (_state.Winner == null && _state.TurnNumber < MaxRounds)
            {
                _state.TurnNumber++;
                log($"\n{'=',-60}");
                log($"  ROUND {_state.TurnNumber}");
                log($"{'=',-60}");

                ProcessTeamTurn(_state.TeamA, _state.TeamB, isAttackers: true, log);
                if (_state.Winner != null) break;

                ProcessTeamTurn(_state.TeamB, _state.TeamA, isAttackers: false, log);
                if (_state.Winner != null) break;

                TickSpikeTimer(log);
                TickZoneEffects();
                CheckWinConditions(log);
            }

            if (_state.Winner == null)
            {
                _state.Winner = "Defenders"; // time ran out — defenders hold
                log("\nRound limit reached — Defenders hold the map!");
            }

            log($"\n=== MATCH OVER — {_state.Winner.ToUpper()} WIN ===");
            return _state.Winner;
        }

        private void ProcessTeamTurn(List<Agent> team, List<Agent> opponents, bool isAttackers, Action<string> log)
        {
            string label = isAttackers ? "ATTACKERS" : "DEFENDERS";
            log($"\n--- {label} TURN ---");

            foreach (var agent in team.Where(a => a.IsAlive).ToList())
            {
                if (agent.IsStunned)
                {
                    log($"  [{agent.Name}] is stunned — skipping turn.");
                    agent.IsStunned = false;
                    continue;
                }

                log($"\n  [{agent.Name} | {agent.Role} | {agent.Health}/{agent.MaxHealth} HP | Zone: {agent.CurrentZone.ZoneName}]");
                ProcessAgentTurn(agent, opponents.Where(o => o.IsAlive).ToList(), isAttackers, log);
            }
        }

        private void ProcessAgentTurn(Agent agent, List<Agent> enemies, bool isAttackers, Action<string> log)
        {
            // 1. Play a card (if one is available and useful).
            var (card, targetZone) = SelectCard(agent, enemies, isAttackers);
            if (card != null)
            {
                var result = CardResolver.Resolve(card, agent, targetZone, enemies, _state);
                log($"    {result.Description}");
                foreach (var entry in result.LogEntries)
                    log(entry);
            }
            else
            {
                log("    No card played this turn.");
            }

            // 2. Move one zone toward the objective.
            var destination = GetObjectiveZone(agent, isAttackers, enemies);
            if (destination != null && destination != agent.CurrentZone)
            {
                var next = _state.Map.GetNextZoneToward(agent.CurrentZone, destination);
                if (next != null && !IsSmoked(next))
                {
                    log($"    {agent.Name} moves: {agent.CurrentZone.ZoneName} → {next.ZoneName}");
                    agent.CurrentZone = next;
                }
                else if (next != null && IsSmoked(next))
                {
                    log($"    {agent.Name} cannot move — {next.ZoneName} is smoked.");
                }
            }

            // 3. Plant or defuse the spike if conditions are met.
            if (isAttackers)
                TryPlant(agent, log);
            else
                TryDefuse(agent, log);
        }

        // ── Card AI ──────────────────────────────────────────────────────────────

        private (Card? card, Zone? targetZone) SelectCard(Agent agent, List<Agent> enemies, bool isAttackers)
        {
            var deck = agent.AgentDeck;

            // Try ultimate if charged.
            if (agent.UltimateCharge >= 100)
            {
                var ult = deck.FirstOrDefault(c => c.IsUltimate);
                if (ult != null)
                {
                    agent.UltimateCharge = 0;
                    return (ult, BestTargetZone(ult, agent, enemies));
                }
            }

            // Heal if below 40% health.
            if (agent.Health < agent.MaxHealth * 0.4)
            {
                var heal = deck.FirstOrDefault(c => c.EffectType == EffectType.Heal && !c.IsUltimate);
                if (heal != null)
                    return (heal, null);
            }

            // Damage if an enemy is in range.
            var damageCard = deck.FirstOrDefault(c => c.EffectType == EffectType.Damage && !c.IsUltimate);
            if (damageCard != null)
            {
                var targetZone = BestTargetZone(damageCard, agent, enemies);
                if (targetZone != null)
                    return (damageCard, targetZone);
            }

            // Stun if an enemy is in range.
            var stunCard = deck.FirstOrDefault(c => c.EffectType == EffectType.Stun && !c.IsUltimate);
            if (stunCard != null)
            {
                var targetZone = BestTargetZone(stunCard, agent, enemies);
                if (targetZone != null)
                    return (stunCard, targetZone);
            }

            // Smoke a chokepoint (controllers).
            if (agent.Role == AgentRole.Controller)
            {
                var smoke = deck.FirstOrDefault(c => c.EffectType == EffectType.Smoke && !c.IsUltimate);
                if (smoke != null)
                {
                    var chokepoint = ChokeZoneToSmoke(agent, enemies);
                    if (chokepoint != null)
                        return (smoke, chokepoint);
                }
            }

            // Buff self.
            var buff = deck.FirstOrDefault(c => c.EffectType == EffectType.Buff && !c.IsUltimate);
            if (buff != null)
                return (buff, null);

            return (null, null);
        }

        private Zone? BestTargetZone(Card card, Agent caster, List<Agent> enemies)
        {
            if (card.TargetType == TargetType.Self || card.TargetType == TargetType.Global)
                return null;

            var reachable = card.Range >= 99
                ? _state.Map.Zones.ToList()
                : _state.Map.GetZonesWithinRange(caster.CurrentZone, card.Range)
                         .Concat(new[] { caster.CurrentZone })
                         .ToList();

            // Pick zone with the most enemies.
            return reachable
                .Where(z => enemies.Any(e => e.CurrentZone == z))
                .OrderByDescending(z => enemies.Count(e => e.CurrentZone == z))
                .FirstOrDefault();
        }

        private Zone? ChokeZoneToSmoke(Agent agent, List<Agent> enemies)
        {
            // Smoke the zone nearest to the bulk of enemies.
            var reachable = _state.Map.GetZonesWithinRange(agent.CurrentZone, 3)
                                      .Concat(new[] { agent.CurrentZone });
            return reachable
                .Where(z => !IsSmoked(z))
                .OrderByDescending(z => enemies.Count(e =>
                    e.CurrentZone == z || e.CurrentZone.Neighbours.Contains(z)))
                .FirstOrDefault();
        }

        // ── Movement AI ───────────────────────────────────────────────────────────

        private Zone? GetObjectiveZone(Agent agent, bool isAttackers, List<Agent> enemies)
        {
            if (isAttackers)
            {
                // If spike is planted, push onto it to defend the detonation.
                if (_state.IsSpikePlanted && _state.SpikeZone != null)
                    return _state.SpikeZone;

                // Otherwise push toward A Site (all attackers converge for simplicity).
                return _state.Map.GetZone("A Site");
            }
            else
            {
                // Defenders: if spike planted, rush to defuse.
                if (_state.IsSpikePlanted && _state.SpikeZone != null)
                    return _state.SpikeZone;

                // Otherwise defend A Site.
                return _state.Map.GetZone("A Site");
            }
        }

        // ── Spike ────────────────────────────────────────────────────────────────

        private void TryPlant(Agent agent, Action<string> log)
        {
            if (_state.IsSpikePlanted) return;
            if (!PlantZones.Contains(agent.CurrentZone.ZoneName)) return;

            _state.IsSpikePlanted = true;
            _state.SpikeZone = agent.CurrentZone;
            _state.SpikeTimer = SpikeDetonationRounds;
            _state.DefuseProgress = 0;
            log($"    *** {agent.Name} PLANTS THE SPIKE on {agent.CurrentZone.ZoneName}! ({SpikeDetonationRounds} rounds to detonation) ***");
        }

        private void TryDefuse(Agent agent, Action<string> log)
        {
            if (!_state.IsSpikePlanted || _state.SpikeZone == null) return;
            if (agent.CurrentZone != _state.SpikeZone) return;

            _state.DefuseProgress++;
            log($"    {agent.Name} is defusing the spike... ({_state.DefuseProgress}/{DefuseRoundsRequired})");

            if (_state.DefuseProgress >= DefuseRoundsRequired)
            {
                _state.Winner = "Defenders";
                log($"    *** SPIKE DEFUSED by {agent.Name}! Defenders win! ***");
            }
        }

        // ── Win Conditions ───────────────────────────────────────────────────────

        private void CheckWinConditions(Action<string> log)
        {
            if (_state.Winner != null) return;

            if (!_state.TeamB.Any(a => a.IsAlive))
            {
                _state.Winner = "Attackers";
                log("\n  *** All defenders eliminated — Attackers win! ***");
            }
            else if (!_state.TeamA.Any(a => a.IsAlive))
            {
                _state.Winner = "Defenders";
                log("\n  *** All attackers eliminated — Defenders win! ***");
            }
        }

        private void TickSpikeTimer(Action<string> log)
        {
            if (!_state.IsSpikePlanted || _state.Winner != null) return;

            _state.SpikeTimer--;
            log($"\n  [Spike] {_state.SpikeTimer} round(s) until detonation.");

            if (_state.SpikeTimer <= 0)
            {
                _state.Winner = "Attackers";
                log("  *** BOOM — SPIKE DETONATED! Attackers win! ***");
            }
        }

        private void TickZoneEffects()
        {
            foreach (var effect in _state.ActiveZoneEffects)
                effect.TurnsRemaining--;

            _state.ActiveZoneEffects.RemoveAll(e => e.TurnsRemaining <= 0);
        }

        private bool IsSmoked(Zone zone) =>
            _state.ActiveZoneEffects.Any(e => e.Zone == zone && e.EffectType == EffectType.Smoke);
    }
}
