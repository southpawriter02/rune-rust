// ==============================================================================
// v0.38.13: Ambient Environmental Descriptors
// AmbientBackgroundActivityDescriptor.cs
// ==============================================================================
// Purpose: Background activity that suggests a larger world
// Pattern: Follows CombatManeuverDescriptor structure from v0.38.12
// Integration: Used by AmbientEnvironmentService for background world events
// ==============================================================================

namespace RuneAndRust.Core.AmbientFlavor;

/// <summary>
/// Represents an ambient background activity descriptor for distant world events.
/// Describes background activity that suggests a larger world beyond the player's immediate location.
/// v0.38.13: Ambient Environmental Descriptors
/// </summary>
public class AmbientBackgroundActivityDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this descriptor.
    /// </summary>
    public int DescriptorId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Biome where this activity occurs.
    /// Values: The_Roots, Muspelheim, Niflheim, Alfheim, Jötunheim, Generic
    /// </summary>
    public string Biome { get; set; } = string.Empty;

    /// <summary>
    /// Category of background activity.
    /// Values: DistantCombat, EnvironmentalEvent, OtherSurvivors, CreatureActivity,
    ///         StructuralEvent, RealityEvent
    /// </summary>
    public string ActivityCategory { get; set; } = string.Empty;

    /// <summary>
    /// Subcategory of activity for more specific classification.
    /// Values: Weapons, Screams, Explosions, Collapse, Tremor, RealityTear,
    ///         Voices, Caravan, Singing, Predators, Movement
    /// </summary>
    public string ActivitySubcategory { get; set; } = string.Empty;

    // ==================== CONTEXTUAL MODIFIERS ====================

    /// <summary>
    /// Time of day when this activity occurs (optional).
    /// Values: Day, Night, NULL
    /// </summary>
    public string? TimeOfDay { get; set; }

    /// <summary>
    /// Distance of the activity from the player (optional).
    /// Values: Near, Medium, Far, Uncertain, NULL
    /// </summary>
    public string? Distance { get; set; }

    /// <summary>
    /// Threat level of the activity (optional).
    /// Values: Safe, Concerning, Dangerous, NULL
    /// </summary>
    public string? ThreatLevel { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// The background activity description text with {Variable} placeholders.
    ///
    /// Available variables:
    /// - LOCATION: {Distance}, {Direction}, {ThreatLevel}, {TimeOfDay}, {Biome}
    ///
    /// Example: "The clash of weapons echoes from somewhere distant."
    /// Example: "A scream, abruptly cut off. Someone didn't make it."
    /// Example: "Voices carry from an adjacent chamber. Other survivors."
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
    /// Example: ["Ominous", "Combat", "Distant"]
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
    /// Valid activity category values.
    /// </summary>
    public static class ActivityCategories
    {
        public const string DistantCombat = "DistantCombat";
        public const string EnvironmentalEvent = "EnvironmentalEvent";
        public const string OtherSurvivors = "OtherSurvivors";
        public const string CreatureActivity = "CreatureActivity";
        public const string StructuralEvent = "StructuralEvent";
        public const string RealityEvent = "RealityEvent";
    }

    /// <summary>
    /// Valid distance values.
    /// </summary>
    public static class Distances
    {
        public const string Near = "Near";
        public const string Medium = "Medium";
        public const string Far = "Far";
        public const string Uncertain = "Uncertain";
    }

    /// <summary>
    /// Valid threat level values.
    /// </summary>
    public static class ThreatLevels
    {
        public const string Safe = "Safe";
        public const string Concerning = "Concerning";
        public const string Dangerous = "Dangerous";
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
