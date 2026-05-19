using System.Collections.Generic;
using AetherFlow.Core.Enums;

namespace AetherFlow.Core
{
    public class Agent
    {
        public string Name { get; set; }
        public AgentRole Role { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public Zone CurrentZone { get; set; }
        public List<Card> AgentDeck { get; set; }

        public bool IsAlive => Health > 0;
        public bool IsStunned { get; set; }
        public int UltimateCharge { get; set; }  // 0–100; plays ult at 100
        public int PowerBuff { get; set; }        // bonus damage on next damage card

        public Agent(string name, AgentRole role, int health, Zone startingZone, List<Card> agentDeck)
        {
            Name = name;
            Role = role;
            Health = health;
            MaxHealth = health;
            CurrentZone = startingZone;
            AgentDeck = agentDeck;
        }
    }
}