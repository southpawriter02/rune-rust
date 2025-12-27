namespace RuneAndRust.Core.Models.Magic
{
    public enum CastTimeType
    {
        Instant,
        Chant
    }

    public class Spell
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Cost { get; set; }
        public CastTimeType CastTimeType { get; set; }
        public int ChantTurns { get; set; } // Only if CastTimeType is Chant
        public MagicSchool School { get; set; }
        public string EffectJson { get; set; } = string.Empty; // JSON representation of the effect
    }
}
