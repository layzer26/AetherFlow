using System.Collections.Generic;
using AetherFlow.Core;
using AetherFlow.Core.Enums;

namespace AetherFlow.Engine
{
    // Creates sample 5v5 teams for match demos and tests.
    public static class AgentFactory
    {
        public static List<Agent> CreateAttackers(ZoneGraph map)
        {
            var spawn = map.GetZone("Attackers Spawn");

            return new List<Agent>
            {
                new Agent("Phoenix", AgentRole.Duelist, 100, spawn, new List<Card>
                {
                    new Card("Curveball",    EffectType.Stun,   20, 1, TargetType.Single, false, false, range: 1),
                    new Card("Hot Hands",    EffectType.Damage, 30, 1, TargetType.Area,   false, false, range: 1),
                    new Card("Blaze",        EffectType.Damage, 25, 1, TargetType.Single, false, false, range: 1),
                    new Card("Run It Back",  EffectType.Heal,   50, 0, TargetType.Self,   true,  false, range: 0),
                }),

                new Agent("Brimstone", AgentRole.Controller, 90, spawn, new List<Card>
                {
                    new Card("Stim Beacon",  EffectType.Buff,   20, 1, TargetType.Self,   false, false, range: 0),
                    new Card("Incendiary",   EffectType.Damage, 25, 1, TargetType.Area,   false, true,  range: 2),
                    new Card("Sky Smoke",    EffectType.Smoke,  0,  1, TargetType.Single, false, false, range: 3),
                    new Card("Orbital Strike",EffectType.Damage,60, 0, TargetType.Area,   true,  false, range: 3),
                }),

                new Agent("Sova", AgentRole.Initiator, 95, spawn, new List<Card>
                {
                    new Card("Shock Bolt",   EffectType.Damage, 30, 1, TargetType.Single, false, false, range: 2),
                    new Card("Owl Drone",    EffectType.Stun,   15, 1, TargetType.Area,   false, false, range: 2),
                    new Card("Recon Bolt",   EffectType.Buff,   10, 1, TargetType.Self,   false, false, range: 0),
                    new Card("Hunter's Fury",EffectType.Damage, 45, 0, TargetType.Global, true,  false, range: 99),
                }),

                new Agent("Sage", AgentRole.Sentinel, 90, spawn, new List<Card>
                {
                    new Card("Slow Orb",     EffectType.Stun,   10, 1, TargetType.Area,   false, false, range: 2),
                    new Card("Barrier Orb",  EffectType.Smoke,  0,  1, TargetType.Single, false, false, range: 1),
                    new Card("Healing Orb",  EffectType.Heal,   40, 1, TargetType.Self,   false, false, range: 0),
                    new Card("Resurrection", EffectType.Heal,   100,0, TargetType.Self,   true,  false, range: 0),
                }),

                new Agent("Jett", AgentRole.Duelist, 100, spawn, new List<Card>
                {
                    new Card("Cloudburst",   EffectType.Smoke,  0,  1, TargetType.Single, false, false, range: 2),
                    new Card("Updraft",      EffectType.Buff,   15, 1, TargetType.Self,   false, false, range: 0),
                    new Card("Blade Storm",  EffectType.Damage, 40, 1, TargetType.Single, false, false, range: 1),
                    new Card("Blade Storm X",EffectType.Damage, 35, 0, TargetType.Area,   true,  false, range: 2),
                }),
            };
        }

        public static List<Agent> CreateDefenders(ZoneGraph map)
        {
            var aSite = map.GetZone("A Site");
            var bSite = map.GetZone("B Site");
            var defSpawn = map.GetZone("Defenders Spawn");

            return new List<Agent>
            {
                new Agent("Reyna", AgentRole.Duelist, 100, aSite, new List<Card>
                {
                    new Card("Leer",         EffectType.Stun,   20, 1, TargetType.Single, false, false, range: 1),
                    new Card("Devour",       EffectType.Heal,   50, 1, TargetType.Self,   false, false, range: 0),
                    new Card("Dismiss",      EffectType.Buff,   10, 1, TargetType.Self,   false, false, range: 0),
                    new Card("Empress",      EffectType.Damage, 50, 0, TargetType.Global, true,  false, range: 99),
                }),

                new Agent("Omen", AgentRole.Controller, 90, defSpawn, new List<Card>
                {
                    new Card("Shrouded Step",EffectType.Buff,   10, 1, TargetType.Self,   false, false, range: 0),
                    new Card("Paranoia",     EffectType.Stun,   15, 1, TargetType.Area,   false, false, range: 2),
                    new Card("Dark Cover",   EffectType.Smoke,  0,  1, TargetType.Single, false, false, range: 3),
                    new Card("From the Rift",EffectType.Damage, 55, 0, TargetType.Area,   true,  false, range: 3),
                }),

                new Agent("Fade", AgentRole.Initiator, 95, defSpawn, new List<Card>
                {
                    new Card("Seize",        EffectType.Stun,   20, 1, TargetType.Area,   false, false, range: 2),
                    new Card("Haunt",        EffectType.Buff,   10, 1, TargetType.Self,   false, false, range: 0),
                    new Card("Prowler",      EffectType.Damage, 25, 1, TargetType.Single, false, false, range: 2),
                    new Card("Nightfall",    EffectType.Stun,   30, 0, TargetType.Global, true,  false, range: 99),
                }),

                new Agent("Killjoy", AgentRole.Sentinel, 90, aSite, new List<Card>
                {
                    new Card("Nanoswarm",    EffectType.Damage, 30, 1, TargetType.Area,   false, false, range: 1),
                    new Card("Alarmbot",     EffectType.Stun,   15, 1, TargetType.Single, false, false, range: 1),
                    new Card("Turret",       EffectType.Buff,   15, 1, TargetType.Self,   false, false, range: 0),
                    new Card("Lockdown",     EffectType.Stun,   40, 0, TargetType.Global, true,  false, range: 99),
                }),

                new Agent("Chamber", AgentRole.Sentinel, 100, bSite, new List<Card>
                {
                    new Card("Trademark",    EffectType.Stun,   20, 1, TargetType.Single, false, false, range: 2),
                    new Card("Headhunter",   EffectType.Damage, 40, 1, TargetType.Single, false, false, range: 1),
                    new Card("Rendezvous",   EffectType.Buff,   10, 1, TargetType.Self,   false, false, range: 0),
                    new Card("Tour de Force",EffectType.Damage, 60, 0, TargetType.Area,   true,  false, range: 2),
                }),
            };
        }
    }
}
