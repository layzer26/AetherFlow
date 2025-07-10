using System;
using System.Collections.Generic;   // for List<T>
using System.Linq;                  // for .Select, .Any(), etc.
using Xunit;                        // for [Fact], FactAttribute, Assert
using Xunit.Abstractions;           // for ITestOutputHelper (if you use it)
using AetherFlow.Core;              // for Agent, Zone, Maps…
using AetherFlow.Core.Enums;        // for AgentRole, EffectType, TargetType…


namespace AetherFlow.Tests
{
    public class ZoneGraphVisualizationTests
    {
        private readonly ITestOutputHelper _output;

        public ZoneGraphVisualizationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void PrintGraph_ShouldShowEachZoneAndItsNeighbours()
        {
            // Arrange
            var map = Maps.BuildViennaMap();

            // Act & Log
            foreach (var zone in map.Zones)
            {
                var neighbourNames = string.Join(", ", zone.Neighbours.Select(n => n.ZoneName));
                _output.WriteLine($"{zone.ZoneName} → [{neighbourNames}]");
            }

            // Assert (trivial, just ensure map isn’t empty)
            Assert.NotEmpty(map.Zones);
        }
    }
}
