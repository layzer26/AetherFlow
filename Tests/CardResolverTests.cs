using System.Collections.Generic;
using System.Linq;
using AetherFlow.Core;
using AetherFlow.Core.Enums;
using AetherFlow.Engine;
using Xunit;

namespace AetherFlow.Tests
{
    public class CardResolverTests
    {
        private static ZoneGraph BuildTwoZoneMap()
        {
            var graph = new ZoneGraph();
            graph.AddZone("Alpha");
            graph.AddZone("Beta");
            graph.ConnectZones("Alpha", "Beta");
            return graph;
        }

        private static Agent MakeAgent(string name, Zone zone, int health = 100)
        {
            return new Agent(name, AgentRole.Duelist, health, zone, new List<Card>());
        }

        [Fact]
        public void Damage_Card_Reduces_Enemy_Health()
        {
            var map = BuildTwoZoneMap();
            var alpha = map.GetZone("Alpha");
            var beta = map.GetZone("Beta");
            var state = new MatchState(new List<Agent>(), new List<Agent>(), map);

            var attacker = MakeAgent("Jett", alpha);
            var defender = MakeAgent("Reyna", beta, health: 100);

            var card = new Card("Blade Storm", EffectType.Damage, 40, 1, TargetType.Single, false, false, range: 1);

            CardResolver.Resolve(card, attacker, beta, new List<Agent> { defender }, state);

            Assert.Equal(60, defender.Health);
        }

        [Fact]
        public void Damage_Card_Eliminates_Enemy_When_Health_Reaches_Zero()
        {
            var map = BuildTwoZoneMap();
            var alpha = map.GetZone("Alpha");
            var beta = map.GetZone("Beta");
            var state = new MatchState(new List<Agent>(), new List<Agent>(), map);

            var attacker = MakeAgent("Jett", alpha);
            var defender = MakeAgent("Reyna", beta, health: 30);

            var card = new Card("Blade Storm", EffectType.Damage, 40, 1, TargetType.Single, false, false, range: 1);

            CardResolver.Resolve(card, attacker, beta, new List<Agent> { defender }, state);

            Assert.Equal(0, defender.Health);
            Assert.False(defender.IsAlive);
        }

        [Fact]
        public void Heal_Card_Restores_Health_Up_To_Max()
        {
            var map = BuildTwoZoneMap();
            var alpha = map.GetZone("Alpha");
            var state = new MatchState(new List<Agent>(), new List<Agent>(), map);

            var agent = MakeAgent("Sage", alpha); // MaxHealth = 100
            agent.Health = 50;                    // simulate damage taken
            var card = new Card("Healing Orb", EffectType.Heal, 40, 1, TargetType.Self, false, false);

            CardResolver.Resolve(card, agent, null, new List<Agent>(), state);

            Assert.Equal(90, agent.Health);
        }

        [Fact]
        public void Heal_Card_Does_Not_Exceed_MaxHealth()
        {
            var map = BuildTwoZoneMap();
            var alpha = map.GetZone("Alpha");
            var state = new MatchState(new List<Agent>(), new List<Agent>(), map);

            var agent = MakeAgent("Sage", alpha); // MaxHealth = 100
            agent.Health = 90;                    // simulate damage taken
            var card = new Card("Healing Orb", EffectType.Heal, 40, 1, TargetType.Self, false, false);

            CardResolver.Resolve(card, agent, null, new List<Agent>(), state);

            Assert.Equal(100, agent.Health);
        }

        [Fact]
        public void Smoke_Card_Adds_ZoneEffect_To_State()
        {
            var map = BuildTwoZoneMap();
            var alpha = map.GetZone("Alpha");
            var beta = map.GetZone("Beta");
            var state = new MatchState(new List<Agent>(), new List<Agent>(), map);

            var agent = MakeAgent("Brimstone", alpha);
            var card = new Card("Sky Smoke", EffectType.Smoke, 0, 1, TargetType.Single, false, false, range: 2);

            CardResolver.Resolve(card, agent, beta, new List<Agent>(), state);

            Assert.Single(state.ActiveZoneEffects);
            Assert.Equal(beta, state.ActiveZoneEffects[0].Zone);
            Assert.Equal(EffectType.Smoke, state.ActiveZoneEffects[0].EffectType);
        }

        [Fact]
        public void Stun_Card_Marks_Enemy_As_Stunned()
        {
            var map = BuildTwoZoneMap();
            var alpha = map.GetZone("Alpha");
            var beta = map.GetZone("Beta");
            var state = new MatchState(new List<Agent>(), new List<Agent>(), map);

            var attacker = MakeAgent("Sova", alpha);
            var defender = MakeAgent("Omen", beta);

            var card = new Card("Slow Orb", EffectType.Stun, 0, 1, TargetType.Area, false, false, range: 1);

            CardResolver.Resolve(card, attacker, beta, new List<Agent> { defender }, state);

            Assert.True(defender.IsStunned);
        }

        [Fact]
        public void Buff_Card_Increases_PowerBuff_On_Caster()
        {
            var map = BuildTwoZoneMap();
            var alpha = map.GetZone("Alpha");
            var state = new MatchState(new List<Agent>(), new List<Agent>(), map);

            var agent = MakeAgent("Jett", alpha);
            var card = new Card("Updraft", EffectType.Buff, 15, 1, TargetType.Self, false, false);

            CardResolver.Resolve(card, agent, null, new List<Agent>(), state);

            Assert.Equal(15, agent.PowerBuff);
        }

        [Fact]
        public void Buff_Is_Consumed_On_Next_Damage_Card()
        {
            var map = BuildTwoZoneMap();
            var alpha = map.GetZone("Alpha");
            var beta = map.GetZone("Beta");
            var state = new MatchState(new List<Agent>(), new List<Agent>(), map);

            var attacker = MakeAgent("Jett", alpha);
            attacker.PowerBuff = 15;

            var defender = MakeAgent("Reyna", beta, health: 100);

            var damageCard = new Card("Blade Storm", EffectType.Damage, 40, 1, TargetType.Single, false, false, range: 1);

            CardResolver.Resolve(damageCard, attacker, beta, new List<Agent> { defender }, state);

            Assert.Equal(45, defender.Health);   // 40 + 15 buff = 55 damage
            Assert.Equal(0, attacker.PowerBuff); // consumed
        }

        [Fact]
        public void Damage_Card_Builds_Ultimate_Charge()
        {
            var map = BuildTwoZoneMap();
            var alpha = map.GetZone("Alpha");
            var beta = map.GetZone("Beta");
            var state = new MatchState(new List<Agent>(), new List<Agent>(), map);

            var attacker = MakeAgent("Phoenix", alpha);
            var defender = MakeAgent("Reyna", beta);

            var card = new Card("Hot Hands", EffectType.Damage, 30, 1, TargetType.Single, false, false, range: 1);

            CardResolver.Resolve(card, attacker, beta, new List<Agent> { defender }, state);

            Assert.True(attacker.UltimateCharge > 0);
        }

        [Fact]
        public void Global_Damage_Card_Hits_All_Enemies()
        {
            var map = BuildTwoZoneMap();
            var alpha = map.GetZone("Alpha");
            var beta = map.GetZone("Beta");
            var state = new MatchState(new List<Agent>(), new List<Agent>(), map);

            var attacker = MakeAgent("Sova", alpha);
            var d1 = MakeAgent("Reyna", beta, health: 100);
            var d2 = MakeAgent("Omen", alpha, health: 100);

            var card = new Card("Hunter's Fury", EffectType.Damage, 45, 0, TargetType.Global, true, false, range: 99);

            CardResolver.Resolve(card, attacker, null, new List<Agent> { d1, d2 }, state);

            Assert.Equal(55, d1.Health);
            Assert.Equal(55, d2.Health);
        }
    }
}
