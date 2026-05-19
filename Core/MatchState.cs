using System.Collections.Generic;

namespace AetherFlow.Core
{
    public class MatchState
    {
        public List<Agent> TeamA { get; set; }   // Attackers
        public List<Agent> TeamB { get; set; }   // Defenders
        public ZoneGraph Map { get; set; }
        public int TurnNumber { get; set; }
        public List<Agent> CurrentTeam { get; set; }
        public bool IsSpikePlanted { get; set; }
        public Zone? SpikeZone { get; set; }
        public int SpikeTimer { get; set; }       // rounds until detonation (counts down from 4)
        public int DefuseProgress { get; set; }   // rounds a defender has been defusing
        public List<ZoneEffect> ActiveZoneEffects { get; set; }
        public string? Winner { get; set; }       // "Attackers", "Defenders", or null

        public MatchState(List<Agent> teamA, List<Agent> teamB, ZoneGraph map)
        {
            TeamA = teamA;
            TeamB = teamB;
            Map = map;
            TurnNumber = 0;
            CurrentTeam = TeamA;
            IsSpikePlanted = false;
            SpikeZone = null;
            SpikeTimer = 0;
            DefuseProgress = 0;
            ActiveZoneEffects = new List<ZoneEffect>();
            Winner = null;
        }
    }
}
