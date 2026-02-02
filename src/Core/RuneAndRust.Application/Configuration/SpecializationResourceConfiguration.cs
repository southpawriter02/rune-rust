// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationResourceConfiguration.cs
// Root configuration record for deserializing specialization-resources.json.
// Version: 0.18.4f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Root configuration structure for specialization resources.
/// </summary>
/// <remarks>
/// <para>
/// Loaded from <c>config/specialization-resources.json</c> at service initialization.
/// Contains configuration for all three specialization-specific resource systems:
/// Rage (Berserker), Momentum (Storm Blade), and Coherence (Arcanist).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// {
///   "version": "1.0",
///   "rage": { ... },
///   "momentum": { ... },
///   "coherence": { ... }
/// }
/// </code>
/// </example>
public record SpecializationResourceConfiguration
{
    /// <summary>
    /// Gets the version of the configuration format.
    /// </summary>
    /// <remarks>
    /// Used for schema validation and migration support.
    /// Format: "Major.Minor" (e.g., "1.0").
    /// </remarks>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Gets the configuration metadata.
    /// </summary>
    /// <remarks>
    /// Contains creation date and description for documentation purposes.
    /// </remarks>
    public ConfigurationMetadata? Metadata { get; init; }

    /// <summary>
    /// Gets the Rage resource configuration (Berserker specialization).
    /// </summary>
    /// <remarks>
    /// Defines thresholds, sources, and decay rules for the Rage system.
    /// Rage is a volatile combat resource powered by violence and pain.
    /// </remarks>
    public RageConfigurationSection Rage { get; init; } = new();

    /// <summary>
    /// Gets the Momentum resource configuration (Storm Blade specialization).
    /// </summary>
    /// <remarks>
    /// Defines thresholds, sources, and decay rules for the Momentum system.
    /// Momentum rewards sustained aggression and penalizes interruption.
    /// </remarks>
    public MomentumConfigurationSection Momentum { get; init; } = new();

    /// <summary>
    /// Gets the Coherence resource configuration (Arcanist specialization).
    /// </summary>
    /// <remarks>
    /// Defines thresholds, sources, and cascade/apotheosis rules.
    /// Coherence is a reality-stability meter with power bonuses and instability risks.
    /// </remarks>
    public CoherenceConfigurationSection Coherence { get; init; } = new();
}

/// <summary>
/// Metadata for configuration files.
/// </summary>
public record ConfigurationMetadata
{
    /// <summary>
    /// Gets the creation date of the configuration.
    /// </summary>
    public string CreatedDate { get; init; } = string.Empty;

    /// <summary>
    /// Gets the description of the configuration.
    /// </summary>
    public string Description { get; init; } = string.Empty;
}
