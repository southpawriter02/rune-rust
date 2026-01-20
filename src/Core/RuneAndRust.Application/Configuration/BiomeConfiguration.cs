namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Configuration for biome definitions loaded from JSON.
/// </summary>
public class BiomeConfiguration
{
    /// <summary>
    /// Schema version for configuration validation.
    /// </summary>
    public string Version { get; init; } = "1.0";

    /// <summary>
    /// Available biome definitions keyed by biome ID.
    /// </summary>
    public IReadOnlyDictionary<string, BiomeConfigurationDto> Biomes { get; init; } =
        new Dictionary<string, BiomeConfigurationDto>();
}

/// <summary>
/// DTO for biome definition loaded from configuration.
/// </summary>
/// <remarks>
/// Biomes provide sensible defaults for environment categories and specify
/// which descriptor pools should be preferred for atmosphere generation.
/// </remarks>
public class BiomeConfigurationDto
{
    /// <summary>
    /// Biome identifier (e.g., "cave", "dungeon", "volcanic").
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Display name for this biome.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description of this biome type.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Default category values for this biome.
    /// Key is category ID, value is the default value ID.
    /// </summary>
    public IReadOnlyDictionary<string, string> DefaultCategoryValues { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Tags automatically applied when in this biome.
    /// </summary>
    public IReadOnlyList<string> ImpliedTags { get; init; } = [];

    /// <summary>
    /// Preferred descriptor pools for this biome.
    /// Key is pool path, value is biome-specific pool path override.
    /// </summary>
    public IReadOnlyDictionary<string, string> DescriptorPoolOverrides { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Terms to emphasize in this biome (higher weight).
    /// </summary>
    public IReadOnlyList<string> EmphasizedTerms { get; init; } = [];

    /// <summary>
    /// Terms to exclude from this biome.
    /// </summary>
    public IReadOnlyList<string> ExcludedTerms { get; init; } = [];
}
