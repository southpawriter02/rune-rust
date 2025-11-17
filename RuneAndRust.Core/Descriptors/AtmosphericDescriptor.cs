using System.Text.Json;

namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38.4: Atmospheric descriptor model
/// Represents a single sensory descriptor for environmental atmosphere
/// </summary>
public class AtmosphericDescriptor
{
    /// <summary>
    /// Unique identifier for this descriptor
    /// </summary>
    public int DescriptorId { get; set; }

    /// <summary>
    /// Atmospheric category (Lighting, Sound, Smell, Temperature, PsychicPresence)
    /// </summary>
    public AtmosphericCategory Category { get; set; }

    /// <summary>
    /// Intensity level (Subtle, Moderate, Oppressive)
    /// </summary>
    public AtmosphericIntensity Intensity { get; set; }

    /// <summary>
    /// The descriptive text for this atmospheric element
    /// </summary>
    public string DescriptorText { get; set; } = string.Empty;

    /// <summary>
    /// Biome affinity (null for generic descriptors, specific biome name otherwise)
    /// </summary>
    public string? BiomeAffinity { get; set; }

    /// <summary>
    /// Tags for filtering and classification (JSON array)
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Gets tags as a list
    /// </summary>
    public List<string> GetTags()
    {
        if (string.IsNullOrEmpty(Tags))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(Tags) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Checks if this descriptor is suitable for a specific biome
    /// </summary>
    public bool IsSuitableForBiome(string? biomeName)
    {
        // Generic descriptors (null affinity) work for any biome
        if (BiomeAffinity == null)
            return true;

        // Biome-specific descriptors must match
        return BiomeAffinity.Equals(biomeName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if this descriptor has a specific tag
    /// </summary>
    public bool HasTag(string tag)
    {
        var tags = GetTags();
        return tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Validates that this descriptor has required properties
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(DescriptorText))
            return false;

        return true;
    }
}
