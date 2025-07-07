using Xunit;
using AetherFlow.Core;
using AetherFlow.Core.Enums;

namespace AetherFlow.Tests
{
    public class AgentTests
    {
        [Fact]
        public void Agent_ShouldInitialize_WithCorrectProperties()
        {
            // Arrange
            var zone = new Zone("A Site");
            var deck = new List<Card>
            {
                new Card("Ability 1", EffectType.Stun, 0, 1, TargetType.Area, false, false),
                new Card("Ability 2 ", EffectType.Smoke, 0, 1, TargetType.Area, false, true),
                new Card("Ability 3", EffectType.Heal, 15, 2, TargetType.Single, false, false),
                new Card("Ultimate Ability", EffectType.Damage, 50, 0, TargetType.Area, true, false)
            };


            // Act
            var agent = new Agent("Agent Name:", AgentRole.Duelist, 100, zone, deck);

            // Assert
            Assert.Equal("Agent Name: ", agent.Name);
            Assert.Equal(AgentRole.Duelist, agent.Role);
            Assert.Equal(100, agent.Health);
            Assert.Equal(zone, agent.CurrentZone);
            Assert.Equal(4, agent.AgentDeck.Count);
            Assert.True(agent.AgentDeck.Exists(c => c.IsUltimate));

        }
    }
}