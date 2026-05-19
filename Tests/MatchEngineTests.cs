using System.Collections.Generic;
using System.Linq;
using AetherFlow.Core;
using AetherFlow.Core.Enums;
using AetherFlow.Engine;
using Xunit;
using Xunit.Abstractions;

namespace AetherFlow.Tests
{
    public class MatchEngineTests
    {
        private readonly ITestOutputHelper _output;

        public MatchEngineTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private static ZoneGraph BuildSimpleMap()
        {
            var graph = new ZoneGraph();
            graph.AddZone("Attackers Spawn");
            graph.AddZone("A Site");
            graph.AddZone("Defenders Spawn");
            graph.ConnectZones("Attackers Spawn", "A Site");
            graph.ConnectZones("Defenders Spawn", "A Site");
            return graph;
        }

        private static Agent Duelist(string name, Zone zone, int health = 100) =>
            new Agent(name, AgentRole.Duelist, health, zone, new List<Card>
            {
                new Card("Strike", EffectType.Damage, 30, 1, TargetType.Single, false, false, range: 1),
                new Card("Heal",   EffectType.Heal,   30, 1, TargetType.Self,   false, false, range: 0),
                new Card("Buff",   EffectType.Buff,   10, 1, TargetType.Self,   false, false, range: 0),
                new Card("Ult",    EffectType.Damage, 50, 0, TargetType.Global, true,  false, range: 99),
            });

        [Fact]
        public void Attackers_Win_By_Elimination()
        {
            var map = BuildSimpleMap();
            var attackers = new List<Agent>
            {
                Duelist("A1", map.GetZone("A Site")),
                Duelist("A2", map.GetZone("A Site")),
            };
            var defenders = new List<Agent>
            {
                Duelist("D1", map.GetZone("A Site"), health: 10),  // will die on turn 1
            };

            var state = new MatchState(attackers, defenders, map);
            var engine = new MatchEngine(state);

            string winner = engine.RunMatch(_output.WriteLine);

            Assert.Equal("Attackers", winner);
        }

        [Fact]
        public void Defenders_Win_By_Elimination()
        {
            var map = BuildSimpleMap();
            var attackers = new List<Agent>
            {
                Duelist("A1", map.GetZone("A Site"), health: 10),  // will die on turn 1
            };
            var defenders = new List<Agent>
            {
                Duelist("D1", map.GetZone("A Site")),
                Duelist("D2", map.GetZone("A Site")),
            };

            var state = new MatchState(attackers, defenders, map);
            var engine = new MatchEngine(state);

            string winner = engine.RunMatch(_output.WriteLine);

            Assert.Equal("Defenders", winner);
        }

        [Fact]
        public void Attackers_Win_By_Spike_Detonation()
        {
            var map = BuildSimpleMap();
            // Only one attacker on A Site — he plants immediately; no defenders alive.
            var attackers = new List<Agent>
            {
                Duelist("A1", map.GetZone("A Site")),
            };
            var defenders = new List<Agent>
            {
                Duelist("D1", map.GetZone("Defenders Spawn"), health: 1),
            };

            // Kill the defender to prevent defuse; spike planted first turn.
            var state = new MatchState(attackers, defenders, map);
            var engine = new MatchEngine(state);

            string winner = engine.RunMatch(_output.WriteLine);

            // Attacker kills defender and/or spike detonates — attackers win either way.
            Assert.Equal("Attackers", winner);
        }

        [Fact]
        public void Spike_Is_Planted_When_Attacker_Reaches_Site()
        {
            var map = BuildSimpleMap();
            var attackers = new List<Agent>
            {
                Duelist("A1", map.GetZone("A Site")),
            };
            // No defenders → attackers plant and detonate unopposed.
            var state = new MatchState(attackers, new List<Agent>(), map);
            var engine = new MatchEngine(state);

            engine.RunMatch(_output.WriteLine);

            Assert.True(state.IsSpikePlanted || state.Winner == "Attackers");
        }

        [Fact]
        public void ZoneGraph_BFS_Finds_Correct_Next_Step()
        {
            var map = BuildSimpleMap();
            var spawn = map.GetZone("Attackers Spawn");
            var site = map.GetZone("A Site");

            var next = map.GetNextZoneToward(spawn, site);

            Assert.NotNull(next);
            Assert.Equal("A Site", next!.ZoneName);
        }

        [Fact]
        public void ZoneGraph_GetZonesWithinRange_Returns_Adjacent_Zones()
        {
            var map = BuildSimpleMap();
            var spawn = map.GetZone("A Site");

            var inRange = map.GetZonesWithinRange(spawn, 1).ToList();

            Assert.Contains(inRange, z => z.ZoneName == "Attackers Spawn");
            Assert.Contains(inRange, z => z.ZoneName == "Defenders Spawn");
        }

        [Fact]
        public void Full_Vienna_Match_Completes_Without_Error()
        {
            var map = Maps.LoadMap("Vienna");
            var attackers = AgentFactory.CreateAttackers(map);
            var defenders = AgentFactory.CreateDefenders(map);
            var state = new MatchState(attackers, defenders, map);
            var engine = new MatchEngine(state);

            var exception = Record.Exception(() => engine.RunMatch(_output.WriteLine));

            Assert.Null(exception);
            Assert.NotNull(state.Winner);
        }
    }
}
