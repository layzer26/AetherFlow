using AetherFlow.Core.Enums;

namespace AetherFlow.Core
{
    public class Card
    {
        public string Name { get; set; }
        public EffectType EffectType { get; set; }
        public int Power { get; set; }
        public int Cost { get; set; }
        public TargetType TargetType { get; set; }
        public bool IsUltimate { get; set; }
        public bool HasRngModifier { get; set; }

        public Card(string name, EffectType effectType, int power, int cost, TargetType targetType, bool isUltimate, bool hasRng)
        {
            Name = name;
            EffectType = effectType;
            Power = power;
            Cost = cost;
            TargetType = targetType;
            IsUltimate = isUltimate;
            HasRngModifier = hasRng;
        }
    }
}