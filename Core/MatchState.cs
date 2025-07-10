using System.Collections.Generic;

namespace AetherFlow.Core
{
    public class MatchState
    {
        public List<Agent> TeamA { get; set; }
        public List<Agent> TeamB { get; set; }
        public ZoneGraph Map { get; set; }
        public int TurnNumber { get; set; }
        public List<Agent> CurrentTeam { get; set; }
        public bool IsSpikePlanted { get; set; }
        public Zone? SpikeZone { get; set; }


        public MatchState(List<Agent> teamA, List<Agent> teamB, ZoneGraph map)
        {
            TeamA = teamA;
            TeamB = teamB;
            Map = map;
            TurnNumber = 0;
            CurrentTeam = TeamA;
            IsSpikePlanted = false;
            SpikeZone = null;
        }
    }
}
