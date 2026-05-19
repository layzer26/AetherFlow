using System;
using System.Collections.Generic;
using System.Linq;
using AetherFlow.Core;
using AetherFlow.Core.Enums;

namespace AetherFlow.Engine
{
    public static class CardResolver
    {
        // Resolves a card played by `caster` against `enemies` (opposing team, alive only).
        // `targetZone` is the zone the card is aimed at (may be null for Self/Global cards).
        public static ActionResult Resolve(Card card, Agent caster, Zone? targetZone, List<Agent> enemies, MatchState state)
        {
            var result = new ActionResult($"{caster.Name} plays {card.Name}", true);

            switch (card.EffectType)
            {
                case EffectType.Damage:
                    ResolveDamage(card, caster, targetZone, enemies, result);
                    break;
                case EffectType.Heal:
                    ResolveHeal(card, caster, result);
                    break;
                case EffectType.Smoke:
                    ResolveSmoke(card, targetZone, state, result);
                    break;
                case EffectType.Stun:
                    ResolveStun(card, targetZone, enemies, result);
                    break;
                case EffectType.Buff:
                    ResolveBuff(card, caster, result);
                    break;
                default:
                    result.LogEntries.Add($"  Unknown effect type: {card.EffectType}");
                    break;
            }

            // Regular cards build ultimate charge; ultimates don't charge themselves.
            if (!card.IsUltimate)
                caster.UltimateCharge = Math.Min(100, caster.UltimateCharge + 15);

            return result;
        }

        private static void ResolveDamage(Card card, Agent caster, Zone? targetZone, List<Agent> enemies, ActionResult result)
        {
            int baseDamage = card.Power + caster.PowerBuff;
            caster.PowerBuff = 0;

            var targets = GetEnemiesInZone(targetZone, enemies, card.TargetType);
            if (!targets.Any())
            {
                result.LogEntries.Add("  No enemies in range.");
                return;
            }

            foreach (var target in targets)
            {
                target.Health = Math.Max(0, target.Health - baseDamage);
                result.LogEntries.Add(target.IsAlive
                    ? $"  {target.Name} took {baseDamage} damage — {target.Health} HP remaining."
                    : $"  {target.Name} took {baseDamage} damage — eliminated!");

                if (!target.IsAlive)
                    caster.UltimateCharge = Math.Min(100, caster.UltimateCharge + 20); // kill bonus
            }
        }

        private static void ResolveHeal(Card card, Agent caster, ActionResult result)
        {
            int before = caster.Health;
            caster.Health = Math.Min(caster.MaxHealth, caster.Health + card.Power);
            result.LogEntries.Add($"  {caster.Name} healed {caster.Health - before} HP — {caster.Health}/{caster.MaxHealth} HP.");
        }

        private static void ResolveSmoke(Card card, Zone? targetZone, MatchState state, ActionResult result)
        {
            if (targetZone == null)
            {
                result.Success = false;
                result.LogEntries.Add("  Smoke needs a target zone.");
                return;
            }

            // Refresh or add smoke effect (2-turn duration).
            var existing = state.ActiveZoneEffects.FirstOrDefault(e => e.Zone == targetZone && e.EffectType == EffectType.Smoke);
            if (existing != null)
                existing.TurnsRemaining = 2;
            else
                state.ActiveZoneEffects.Add(new ZoneEffect(targetZone, EffectType.Smoke, 2));

            result.LogEntries.Add($"  {targetZone.ZoneName} is smoked for 2 turns — movement blocked.");
        }

        private static void ResolveStun(Card card, Zone? targetZone, List<Agent> enemies, ActionResult result)
        {
            var targets = GetEnemiesInZone(targetZone, enemies, card.TargetType);
            if (!targets.Any())
            {
                result.LogEntries.Add("  No enemies to stun.");
                return;
            }

            foreach (var target in targets)
            {
                target.IsStunned = true;
                result.LogEntries.Add($"  {target.Name} is stunned — loses next action.");
            }
        }

        private static void ResolveBuff(Card card, Agent caster, ActionResult result)
        {
            caster.PowerBuff += card.Power;
            result.LogEntries.Add($"  {caster.Name} gains +{card.Power} power — stacked for next attack.");
        }

        // Returns enemies in the target zone based on card targeting mode.
        private static List<Agent> GetEnemiesInZone(Zone? targetZone, List<Agent> enemies, TargetType targeting)
        {
            return targeting switch
            {
                TargetType.Global => enemies.Where(e => e.IsAlive).ToList(),
                TargetType.Area   => enemies.Where(e => e.IsAlive && e.CurrentZone == targetZone).ToList(),
                TargetType.Single => enemies.Where(e => e.IsAlive && e.CurrentZone == targetZone).Take(1).ToList(),
                _                 => new List<Agent>()
            };
        }
    }
}
