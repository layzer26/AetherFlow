using System.Collections.Generic;
using AetherFlow.Core.Enums;

namespace AetherFlow.Core
{
    public class Agent
    {
        public string Name { get; set; }
        public AgentRole Role { get; set; }
        public int Health { get; set; }
        public Zone CurrentZone { get; set; }
        public List<Card> AgentDeck { get; set; }

        public Agent(string name, AgentRole role, int health, Zone startingZone, List<Card> agentDeck)
        {
            Name = name;
            Role = role;
            Health = health;
            CurrentZone = startingZone;
            AgentDeck = agentDeck;
        }
    }
}