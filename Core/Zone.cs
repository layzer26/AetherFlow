using System.Collections.Generic;
using AetherFlow.Core.Enums;

namespace AetherFlow.Core
{
    public class Zone
    {
        public string ZoneName { get; set; }
        public List<Zone> Neighbours { get; set; }
        public ZoneType ControlStatus { get; set; }

        public Zone(string zoneName)
        {
            ZoneName = zoneName;
            Neighbours = new List<Zone>();
            ControlStatus = ZoneType.Neutral;
        }

        public void AddNeighbor(Zone neighbor)
        {
            if (!Neighbours.Contains(neighbor))
            {
                Neighbours.Add(neighbor);
                neighbor.Neighbours.Add(this);
            }
        }
    }
}