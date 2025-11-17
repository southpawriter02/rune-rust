using System.Text.Json;

namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38.5: Biome resource profile model
/// Defines resource distribution for a specific biome
/// </summary>
public class BiomeResourceProfile
{
    /// <summary>
    /// Unique identifier for this profile
    /// </summary>
    public int ProfileId { get; set; }

    /// <summary>
    /// Biome name this profile applies to
    /// </summary>
    public string BiomeName { get; set; } = string.Empty;

    /// <summary>
    /// Common resource distribution (JSON array)
    /// </summary>
    public string CommonResources { get; set; } = string.Empty;

    /// <summary>
    /// Uncommon resource distribution (JSON array)
    /// </summary>
    public string UncommonResources { get; set; } = string.Empty;

    /// <summary>
    /// Rare resource distribution (JSON array)
    /// </summary>
    public string RareResources { get; set; } = string.Empty;

    /// <summary>
    /// Legendary resource distribution (JSON array, optional)
    /// </summary>
    public string? LegendaryResources { get; set; }

    /// <summary>
    /// Spawn density for small rooms
    /// </summary>
    public int SpawnDensitySmall { get; set; }

    /// <summary>
    /// Spawn density for medium rooms
    /// </summary>
    public int SpawnDensityMedium { get; set; }

    /// <summary>
    /// Spawn density for large rooms
    /// </summary>
    public int SpawnDensityLarge { get; set; }

    /// <summary>
    /// Unique resources for this biome (JSON array, optional)
    /// </summary>
    public string? UniqueResources { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets common resource definitions as a list
    /// </summary>
    public List<ResourceDefinition> GetCommonResourceDefinitions()
    {
        return DeserializeResourceDefinitions(CommonResources);
    }

    /// <summary>
    /// Gets uncommon resource definitions as a list
    /// </summary>
    public List<ResourceDefinition> GetUncommonResourceDefinitions()
    {
        return DeserializeResourceDefinitions(UncommonResources);
    }

    /// <summary>
    /// Gets rare resource definitions as a list
    /// </summary>
    public List<ResourceDefinition> GetRareResourceDefinitions()
    {
        return DeserializeResourceDefinitions(RareResources);
    }

    /// <summary>
    /// Gets legendary resource definitions as a list
    /// </summary>
    public List<ResourceDefinition> GetLegendaryResourceDefinitions()
    {
        if (string.IsNullOrEmpty(LegendaryResources))
            return new List<ResourceDefinition>();

        return DeserializeResourceDefinitions(LegendaryResources);
    }

    /// <summary>
    /// Gets unique resources as a list
    /// </summary>
    public List<string> GetUniqueResources()
    {
        if (string.IsNullOrEmpty(UniqueResources))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(UniqueResources) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Gets spawn density based on room size
    /// </summary>
    public int GetSpawnDensity(string roomSize)
    {
        return roomSize?.ToLower() switch
        {
            "small" => SpawnDensitySmall,
            "medium" => SpawnDensityMedium,
            "large" => SpawnDensityLarge,
            _ => SpawnDensityMedium // Default to medium
        };
    }

    /// <summary>
    /// Helper method to deserialize resource definitions from JSON
    /// </summary>
    private List<ResourceDefinition> DeserializeResourceDefinitions(string json)
    {
        if (string.IsNullOrEmpty(json))
            return new List<ResourceDefinition>();

        try
        {
            return JsonSerializer.Deserialize<List<ResourceDefinition>>(json) ?? new List<ResourceDefinition>();
        }
        catch
        {
            return new List<ResourceDefinition>();
        }
    }

    /// <summary>
    /// Validates that this profile has required properties
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(BiomeName))
            return false;

        // Check that we have at least common resources
        if (GetCommonResourceDefinitions().Count == 0)
            return false;

        return true;
    }

    /// <summary>
    /// Gets a summary of this profile for display
    /// </summary>
    public string GetSummary()
    {
        return $"Biome: {BiomeName}, " +
               $"Resources: {GetCommonResourceDefinitions().Count} common, " +
               $"{GetUncommonResourceDefinitions().Count} uncommon, " +
               $"{GetRareResourceDefinitions().Count} rare, " +
               $"Spawn Density: {SpawnDensitySmall}/{SpawnDensityMedium}/{SpawnDensityLarge}";
    }
}

/// <summary>
/// v0.38.5: Resource definition for biome distribution tables
/// Defines a resource type with template, resource name, and spawn weight
/// </summary>
public class ResourceDefinition
{
    /// <summary>
    /// Base template name (e.g., "Ore_Vein_Base", "Salvage_Wreckage_Base")
    /// </summary>
    public string Template { get; set; } = string.Empty;

    /// <summary>
    /// Resource type (e.g., "Iron", "Star_Metal", "Luminous_Fungus")
    /// </summary>
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// Spawn weight (0.0-1.0, relative probability within tier)
    /// </summary>
    public float Weight { get; set; }

    /// <summary>
    /// Validates that this definition has required properties
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(Template))
            return false;

        if (string.IsNullOrEmpty(Resource))
            return false;

        if (Weight < 0 || Weight > 1)
            return false;

        return true;
    }
}
