using AetherFlow.Core.Enums;

namespace AetherFlow.Core
{
    public class Teams
    {
        public TeamNames Name { get; set; }
        public List<Agent> Agents { get; set; } = new();
        public bool isAttacking { get; set; }


        public Team(string name, List<Agent> agents)
        {
            this.Name = name;
            this.Agents = agents;
        }
            
        }// Team class
}//namespace