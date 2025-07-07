using Xunit;
using AetherFlow.Core;
using AetherFlow.Core.Enums;

namespace AetherFlow.Tests
{
    public class ZoneGraphTests
    {
        [Fact]
        public void BuildViennaMap_ShouldContainExpectedZones()
        {
            var map = Maps.BuildViennaMap();
            Assert.NotNull(map.GetZone("Mid"));
            Assert.NotNull(map.GetZone("A Site"));
            Assert.NotNull(map.GetZone("B Site"));
            Assert.NotNull(map.GetZone("Attackers Spawn"));
            Assert.NotNull(map.GetZone("Defenders Spawn"));
        }

        [Fact]
        public void A_Site_ShouldHaveCorrectNeighbours()
        {
            var map = Maps.BuildViennaMap();
            var aSite = map.GetZone("A Site");
            var neighbourNames = aSite.Neighbours.Select(n => n.ZoneName).ToList();

            Assert.Contains("A Long", neighbourNames);
            Assert.Contains("A Short", neighbourNames);
            Assert.Contains("Mid To A", neighbourNames);
        }

        [Fact]
        public void Mid_ShouldConnectTo_MidToA_And_MidToB()
        {
            var map = Maps.BuildViennaMap();
            var mid = map.GetZone("Mid");
            var neighbourNames = mid.Neighbours.Select(n => n.ZoneName).ToList();

            Assert.Contains("Mid To A", neighbourNames);
            Assert.Contains("Mid To B", neighbourNames);
        }
    }
}
