using System.Collections.Generic;      // for List<T>
using Xunit;                           // for [Fact], FactAttribute, Assert
using Xunit.Abstractions;              // for ITestOutputHelper
using AetherFlow.Core;                 // for Agent, Zone, etc.
using AetherFlow.Core.Enums;           // for AgentRole, EffectType, TargetType

namespace AetherFlow.Tests
{
    public class AgentTests
    {
        private readonly ITestOutputHelper _output;

        public AgentTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Agent_ShouldInitialize_WithCorrectProperties()
        {
            // Arrange
            var zone = new Zone("A Site");
            var deck = new List<Card>
            {
                new Card("Flashbang", EffectType.Stun,    0, 1, TargetType.Area,   false, false),
                new Card("Smoke",     EffectType.Smoke,   0, 1, TargetType.Area,   false, true),
                new Card("Heal",      EffectType.Heal,    15,2, TargetType.Single, false, false),
                new Card("Ult",       EffectType.Damage,  50,0, TargetType.Area,   true,  false)
            };

            // Act
            var agent = new Agent("Phoenix", AgentRole.Duelist, 100, zone, deck);

            // Log diagnostic info
            _output.WriteLine($"Agent name: {agent.Name}");
            _output.WriteLine($"Health = {agent.Health}, Zone = {agent.CurrentZone.ZoneName}");
            _output.WriteLine("Deck contains:");
            foreach (var card in agent.AgentDeck)
            {
                _output.WriteLine($" - {card.Name} (Ultimate? {card.IsUltimate})");
            }

            // Assert
            Assert.Equal("Phoenix",         agent.Name);
            Assert.Equal(AgentRole.Duelist, agent.Role);
            Assert.Equal(100,               agent.Health);
            Assert.Equal(zone,              agent.CurrentZone);
            Assert.Equal(4,                 agent.AgentDeck.Count);
            Assert.True(agent.AgentDeck.Exists(c => c.IsUltimate));
        }
    }
}
