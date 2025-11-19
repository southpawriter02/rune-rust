// ==============================================================================
// v0.38.13: Ambient Environmental Descriptors
// AmbientAtmosphericDetailDescriptor.cs
// ==============================================================================
// Purpose: Air quality, weather effects, and environmental conditions
// Pattern: Follows CombatManeuverDescriptor structure from v0.38.12
// Integration: Used by AmbientEnvironmentService for atmospheric changes
// ==============================================================================

namespace RuneAndRust.Core.AmbientFlavor;

/// <summary>
/// Represents an ambient atmospheric detail descriptor for environmental conditions.
/// Describes air quality, temperature, and weather effects that change periodically.
/// v0.38.13: Ambient Environmental Descriptors
/// </summary>
public class AmbientAtmosphericDetailDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this descriptor.
    /// </summary>
    public int DescriptorId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Biome where this atmospheric detail occurs.
    /// Values: The_Roots, Muspelheim, Niflheim, Alfheim, Jötunheim, Generic
    /// </summary>
    public string Biome { get; set; } = string.Empty;

    /// <summary>
    /// Category of atmospheric detail.
    /// Values: AirQuality, Temperature, Humidity, Visibility, TimeOfDay, WeatherEffect
    /// </summary>
    public string DetailCategory { get; set; } = string.Empty;

    /// <summary>
    /// Subcategory of detail for more specific classification.
    /// Values: Thick, Breathable, Suffocating, Hot, Cold, Dry, Saturated,
    ///         Clear, Obscured, DayTransition, NightTransition, StormEffect, CalmEffect
    /// </summary>
    public string DetailSubcategory { get; set; } = string.Empty;

    // ==================== CONTEXTUAL MODIFIERS ====================

    /// <summary>
    /// Time of day when this detail applies (optional).
    /// Values: Day, Night, Transition, NULL
    /// </summary>
    public string? TimeOfDay { get; set; }

    /// <summary>
    /// Intensity of the atmospheric condition (optional).
    /// Values: Subtle, Moderate, Oppressive, NULL
    /// </summary>
    public string? Intensity { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// The atmospheric detail description text with {Variable} placeholders.
    ///
    /// Available variables:
    /// - LOCATION: {Biome}, {TimeOfDay}
    /// - DESCRIPTION: {IntensityModifier}, {TemperatureValue}, {VisibilityDistance}
    ///
    /// Example: "The air is thick with humidity. Every breath is heavy."
    /// Example: "The heat is suffocating. Each breath burns your lungs."
    /// Example: "Your breath crystallizes instantly in the frigid air."
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
    /// Example: ["Oppressive", "Physical", "Environmental"]
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
    /// Valid detail category values.
    /// </summary>
    public static class DetailCategories
    {
        public const string AirQuality = "AirQuality";
        public const string Temperature = "Temperature";
        public const string Humidity = "Humidity";
        public const string Visibility = "Visibility";
        public const string TimeOfDay = "TimeOfDay";
        public const string WeatherEffect = "WeatherEffect";
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
        public const string Transition = "Transition";
    }
}
