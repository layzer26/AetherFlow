using AetherFlow.Core.Enums;

namespace AetherFlow.Core
{
    public class ZoneEffect
    {
        public Zone Zone { get; set; }
        public EffectType EffectType { get; set; }
        public int TurnsRemaining { get; set; }

        public ZoneEffect(Zone zone, EffectType effectType, int duration)
        {
            Zone = zone;
            EffectType = effectType;
            TurnsRemaining = duration;
        }
    }
}
