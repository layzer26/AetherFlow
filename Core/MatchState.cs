
using System.Runtime.InteropServices;   

namespace AetherFlow.Core
{

    public class MatchState
    {
        //Current status of the match 
        public List<Agent> TeamA { get; set; }
        public List<Agent> TeamB { get; set; }
        public ZoneGraph Map { get; set; }
        public int turn;
        public string CurrentTeam { get; set; }
        public bool IsSpikePlanted { get; set; }
        public Zone SpikeZone { get; set; }
        



    }//Match State class 
}//namespace