// ==============================================================================
// v0.38.13: Ambient Environmental Descriptors
// AmbientSoundDescriptor.cs
// ==============================================================================
// Purpose: Periodic ambient sounds that fire to make the world feel alive
// Pattern: Follows CombatManeuverDescriptor structure from v0.38.12
// Integration: Used by AmbientEnvironmentService to generate periodic ambient events
// ==============================================================================

namespace RuneAndRust.Core.AmbientFlavor;

/// <summary>
/// Represents an ambient sound descriptor for periodic environmental audio events.
/// Describes sounds that fire periodically to make the world feel alive and immersive.
/// v0.38.13: Ambient Environmental Descriptors
/// </summary>
public class AmbientSoundDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this descriptor.
    /// </summary>
    public int DescriptorId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Biome where this sound occurs.
    /// Values: The_Roots, Muspelheim, Niflheim, Alfheim, Jötunheim, Generic
    /// </summary>
    public string Biome { get; set; } = string.Empty;

    /// <summary>
    /// Category of sound.
    /// Values: Mechanical, Decay, Eerie, Creature, Fire, Ice, Wind, Glitch, Industrial, Elemental
    /// </summary>
    public string SoundCategory { get; set; } = string.Empty;

    /// <summary>
    /// Subcategory of sound for more specific classification.
    /// Values: ActiveMachinery, DecayingSystems, SmallCreatures, DistantThreats,
    ///         Lava, Creaking, Howling, RealityDistortion, CursedChoir,
    ///         TitanicMachinery, EmptySpaces, OppressiveSilence
    /// </summary>
    public string SoundSubcategory { get; set; } = string.Empty;

    // ==================== CONTEXTUAL MODIFIERS ====================

    /// <summary>
    /// Time of day when this sound is most likely (optional).
    /// Values: Day, Night, NULL (any time)
    /// </summary>
    public string? TimeOfDay { get; set; }

    /// <summary>
    /// Intensity of the sound (optional).
    /// Values: Subtle, Moderate, Oppressive, NULL (any intensity)
    /// </summary>
    public string? Intensity { get; set; }

    /// <summary>
    /// Location context where sound occurs (optional).
    /// Values: Corridor, Chamber, Exterior, Underground, NULL
    /// </summary>
    public string? LocationContext { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// The ambient sound description text with {Variable} placeholders.
    ///
    /// Available variables:
    /// - LOCATION: {Biome}, {LocationName}, {TimeOfDay}
    /// - DESCRIPTION: {DistanceDescriptor}, {IntensityModifier}
    ///
    /// Example: "The distant thrum of still-functioning machinery echoes through the halls."
    /// Example: "A scream cuts through the silence, distant but unmistakable. Then nothing."
    /// Example: "Ice cracks with sharp snap sounds that echo for miles."
    /// </summary>
    public string DescriptorText { get; set; } = string.Empty;

    // ==================== METADATA ====================

    /// <summary>
    /// Probability weight for random selection (1.0 = default).
    /// Higher values increase selection chance.
    /// </summary>
    public float Weight { get; set; } = 1.0f;

    /// <summary>
    /// Whether this descriptor is active and can be selected.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON array of tags for categorization and filtering.
    /// Example: ["Mechanical", "Background", "Ominous"]
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid biome values.
    /// </summary>
    public static class Biomes
    {
        public const string The_Roots = "The_Roots";
        public const string Muspelheim = "Muspelheim";
        public const string Niflheim = "Niflheim";
        public const string Alfheim = "Alfheim";
        public const string Jötunheim = "Jötunheim";
        public const string Generic = "Generic";
    }

    /// <summary>
    /// Valid sound category values.
    /// </summary>
    public static class SoundCategories
    {
        public const string Mechanical = "Mechanical";
        public const string Decay = "Decay";
        public const string Eerie = "Eerie";
        public const string Creature = "Creature";
        public const string Fire = "Fire";
        public const string Ice = "Ice";
        public const string Wind = "Wind";
        public const string Glitch = "Glitch";
        public const string Industrial = "Industrial";
        public const string Elemental = "Elemental";
    }

    /// <summary>
    /// Valid intensity values.
    /// </summary>
    public static class Intensities
    {
        public const string Subtle = "Subtle";
        public const string Moderate = "Moderate";
        public const string Oppressive = "Oppressive";
    }

    /// <summary>
    /// Valid time of day values.
    /// </summary>
    public static class TimesOfDay
    {
        public const string Day = "Day";
        public const string Night = "Night";
    }
}
