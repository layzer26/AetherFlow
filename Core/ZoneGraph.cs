using System;
using System.Collections.Generic;
using System.Linq;

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
                zones[name] = new Zone(name);
        }

        public void ConnectZones(string zoneA, string zoneB)
        {
            if (zones.ContainsKey(zoneA) && zones.ContainsKey(zoneB))
                zones[zoneA].AddNeighbor(zones[zoneB]);
        }

        public Zone GetZone(string name)
        {
            if (!zones.ContainsKey(name))
                throw new KeyNotFoundException($"Zone '{name}' not found.");
            return zones[name];
        }

        public IEnumerable<Zone> Zones => zones.Values;

        // BFS: all zones reachable within `range` hops (excluding origin).
        public IEnumerable<Zone> GetZonesWithinRange(Zone origin, int range)
        {
            var visited = new HashSet<Zone> { origin };
            var frontier = new List<Zone> { origin };

            for (int hop = 0; hop < range; hop++)
            {
                var next = new List<Zone>();
                foreach (var z in frontier)
                    foreach (var n in z.Neighbours.Where(n => !visited.Contains(n)))
                    {
                        visited.Add(n);
                        next.Add(n);
                    }
                frontier = next;
            }

            visited.Remove(origin);
            return visited;
        }

        // BFS: returns the first zone to step into when travelling from origin toward target.
        public Zone? GetNextZoneToward(Zone origin, Zone target)
        {
            if (origin == target) return null;

            var visited = new HashSet<Zone> { origin };
            // queue stores (currentZone, firstStep taken from origin)
            var queue = new Queue<(Zone current, Zone firstStep)>();

            foreach (var n in origin.Neighbours)
            {
                if (!visited.Contains(n))
                {
                    visited.Add(n);
                    queue.Enqueue((n, n));
                }
            }

            while (queue.Count > 0)
            {
                var (current, firstStep) = queue.Dequeue();
                if (current == target) return firstStep;

                foreach (var n in current.Neighbours)
                {
                    if (!visited.Contains(n))
                    {
                        visited.Add(n);
                        queue.Enqueue((n, firstStep));
                    }
                }
            }

            return null; // disconnected graph
        }
    }
}
