
using System.Runtime.InteropServices;
using AetherFlow.Core.Enums;

namespace AetherFlow.Core
{

    public class MatchState
    {
        //Current status of the match 
        public Team TeamA { get; set; }
        public Team TeamB { get; set; }
        public ZoneGraph Map { get; set; }
        public int Turn { get; set; }
        public Team CurrentTeam { get; set; }
        public bool IsSpikePlanted { get; set; }
        public Zone SpikeZone { get; set; }

        public MatchState()
        {
            
                TeamA = new Teams(TeamNames.TeamA, new List<Agent>());
                TeamB = new Teams(TeamNames.TeamB, new List<Agent>());


                Map = new ZoneGraph();
                CurrentTeam = TeamA;
                SpikeZone = new Zone();
                Turn = 0;
                IsSpikePlanted = false;

            




        }//Match State class 
    }//namespace