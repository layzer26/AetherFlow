using System;
using System.Collections.Generic;

namespace AetherFlow.Core
{
    public class ZoneGraph
    {
        private Dictionary<string, Zone> zones;

        public ZoneGraph()
        {
            zones = new Dictionary<string, Zone>();
        }

        public void AddZone(string name)
        {
            if (!zones.ContainsKey(name))
            {
                zones[name] = new Zone(name);
            }
        }

        public void ConnectZones(string zoneA, string zoneB)
        {
            if (zones.ContainsKey(zoneA) && zones.ContainsKey(zoneB))
            {
                zones[zoneA].AddNeighbor(zones[zoneB]);
            }
        }

        public Zone GetZone(string name)
        {
            return zones.TryGetValue(name, out var zone) ? zone : null;
        }

        public IEnumerable<Zone> Zones => zones.Values;
    }
}