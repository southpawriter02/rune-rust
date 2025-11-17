using System.Text.Json;

namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38.4: Biome atmosphere profile model
/// Defines atmospheric composition for a specific biome
/// </summary>
public class BiomeAtmosphereProfile
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
    /// Lighting descriptor IDs (JSON array)
    /// </summary>
    public string LightingDescriptors { get; set; } = string.Empty;

    /// <summary>
    /// Sound descriptor IDs (JSON array)
    /// </summary>
    public string SoundDescriptors { get; set; } = string.Empty;

    /// <summary>
    /// Smell descriptor IDs (JSON array)
    /// </summary>
    public string SmellDescriptors { get; set; } = string.Empty;

    /// <summary>
    /// Temperature descriptor IDs (JSON array)
    /// </summary>
    public string TemperatureDescriptors { get; set; } = string.Empty;

    /// <summary>
    /// Psychic presence descriptor IDs (JSON array)
    /// </summary>
    public string PsychicDescriptors { get; set; } = string.Empty;

    /// <summary>
    /// Composite template with placeholders ({Lighting}, {Sound}, etc.)
    /// </summary>
    public string CompositeTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Default intensity for this biome
    /// </summary>
    public AtmosphericIntensity DefaultIntensity { get; set; } = AtmosphericIntensity.Moderate;

    /// <summary>
    /// Gets lighting descriptor IDs as a list
    /// </summary>
    public List<int> GetLightingDescriptorIds()
    {
        return DeserializeIds(LightingDescriptors);
    }

    /// <summary>
    /// Gets sound descriptor IDs as a list
    /// </summary>
    public List<int> GetSoundDescriptorIds()
    {
        return DeserializeIds(SoundDescriptors);
    }

    /// <summary>
    /// Gets smell descriptor IDs as a list
    /// </summary>
    public List<int> GetSmellDescriptorIds()
    {
        return DeserializeIds(SmellDescriptors);
    }

    /// <summary>
    /// Gets temperature descriptor IDs as a list
    /// </summary>
    public List<int> GetTemperatureDescriptorIds()
    {
        return DeserializeIds(TemperatureDescriptors);
    }

    /// <summary>
    /// Gets psychic descriptor IDs as a list
    /// </summary>
    public List<int> GetPsychicDescriptorIds()
    {
        return DeserializeIds(PsychicDescriptors);
    }

    /// <summary>
    /// Helper method to deserialize JSON arrays of descriptor IDs
    /// </summary>
    private List<int> DeserializeIds(string json)
    {
        if (string.IsNullOrEmpty(json))
            return new List<int>();

        try
        {
            return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }
        catch
        {
            return new List<int>();
        }
    }

    /// <summary>
    /// Validates that this profile has required properties
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(BiomeName))
            return false;

        if (string.IsNullOrEmpty(CompositeTemplate))
            return false;

        // Check that we have at least some descriptors for each category
        if (GetLightingDescriptorIds().Count == 0)
            return false;
        if (GetSoundDescriptorIds().Count == 0)
            return false;
        if (GetSmellDescriptorIds().Count == 0)
            return false;
        if (GetTemperatureDescriptorIds().Count == 0)
            return false;
        if (GetPsychicDescriptorIds().Count == 0)
            return false;

        return true;
    }

    /// <summary>
    /// Gets a summary of this profile for display
    /// </summary>
    public string GetSummary()
    {
        return $"Biome: {BiomeName}, Default Intensity: {DefaultIntensity}, " +
               $"Descriptors: {GetLightingDescriptorIds().Count} lighting, " +
               $"{GetSoundDescriptorIds().Count} sound, " +
               $"{GetSmellDescriptorIds().Count} smell, " +
               $"{GetTemperatureDescriptorIds().Count} temperature, " +
               $"{GetPsychicDescriptorIds().Count} psychic";
    }
}
