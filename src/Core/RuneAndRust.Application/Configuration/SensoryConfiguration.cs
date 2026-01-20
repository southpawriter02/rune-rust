namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Configuration for sensory descriptor pools.
/// </summary>
public class SensoryConfiguration
{
    /// <summary>
    /// Schema version for configuration validation.
    /// </summary>
    public string Version { get; init; } = "1.0";

    /// <summary>
    /// Light source definitions.
    /// </summary>
    public IReadOnlyDictionary<string, LightSourceDefinition> LightSources { get; init; } =
        new Dictionary<string, LightSourceDefinition>();

    /// <summary>
    /// Darkness level definitions.
    /// </summary>
    public IReadOnlyDictionary<string, DarknessLevelDefinition> DarknessLevels { get; init; } =
        new Dictionary<string, DarknessLevelDefinition>();

    /// <summary>
    /// Weather condition definitions.
    /// </summary>
    public IReadOnlyDictionary<string, WeatherDefinition> WeatherConditions { get; init; } =
        new Dictionary<string, WeatherDefinition>();

    /// <summary>
    /// Time of day definitions.
    /// </summary>
    public IReadOnlyDictionary<string, TimeOfDayDefinition> TimesOfDay { get; init; } =
        new Dictionary<string, TimeOfDayDefinition>();
}

/// <summary>
/// Defines a light source type with visual characteristics.
/// </summary>
public class LightSourceDefinition
{
    /// <summary>
    /// Light source identifier (e.g., "torch", "crystal").
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Light color/quality (e.g., "warm orange", "cold blue").
    /// </summary>
    public string LightQuality { get; init; } = string.Empty;

    /// <summary>
    /// Descriptor pool path for this light source.
    /// </summary>
    public string DescriptorPool { get; init; } = string.Empty;

    /// <summary>
    /// Atmospheric mood this light creates.
    /// </summary>
    public string Atmosphere { get; init; } = string.Empty;

    /// <summary>
    /// Biomes where this light source is common.
    /// </summary>
    public IReadOnlyList<string> CommonBiomes { get; init; } = [];

    /// <summary>
    /// Whether this light source flickers/moves.
    /// </summary>
    public bool IsFlickering { get; init; }
}

/// <summary>
/// Defines a darkness/visibility level.
/// </summary>
public class DarknessLevelDefinition
{
    /// <summary>
    /// Darkness level identifier (e.g., "dim", "bright").
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Numeric visibility level (0 = pitch black, 100 = blinding).
    /// </summary>
    public int VisibilityLevel { get; init; }

    /// <summary>
    /// Descriptor pool path for this darkness level.
    /// </summary>
    public string DescriptorPool { get; init; } = string.Empty;

    /// <summary>
    /// Tags applied when at this darkness level.
    /// </summary>
    public IReadOnlyList<string> ImpliedTags { get; init; } = [];
}

/// <summary>
/// Defines a weather condition.
/// </summary>
public class WeatherDefinition
{
    /// <summary>
    /// Weather condition identifier (e.g., "rain", "storm").
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Descriptor pool for indoor effects.
    /// </summary>
    public string IndoorPool { get; init; } = string.Empty;

    /// <summary>
    /// Descriptor pool for outdoor effects.
    /// </summary>
    public string OutdoorPool { get; init; } = string.Empty;

    /// <summary>
    /// Climate values where this weather can occur.
    /// </summary>
    public IReadOnlyList<string> ValidClimates { get; init; } = [];

    /// <summary>
    /// Whether this weather affects visibility.
    /// </summary>
    public bool AffectsVisibility { get; init; }

    /// <summary>
    /// Whether this weather affects sound (muffling, etc.).
    /// </summary>
    public bool AffectsSound { get; init; }
}

/// <summary>
/// Defines a time of day period.
/// </summary>
public class TimeOfDayDefinition
{
    /// <summary>
    /// Time period identifier (e.g., "dawn", "night").
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Descriptor pool for outdoor effects.
    /// </summary>
    public string OutdoorPool { get; init; } = string.Empty;

    /// <summary>
    /// Light quality during this time period.
    /// </summary>
    public string LightQuality { get; init; } = string.Empty;

    /// <summary>
    /// Default darkness level for outdoor areas.
    /// </summary>
    public string DefaultDarknessLevel { get; init; } = string.Empty;

    /// <summary>
    /// Tags applied during this time.
    /// </summary>
    public IReadOnlyList<string> ImpliedTags { get; init; } = [];
}
