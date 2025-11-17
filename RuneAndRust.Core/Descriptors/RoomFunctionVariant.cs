namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38.1: Room function variant model
/// Represents functional descriptors for chamber rooms (e.g., "Pumping Station")
/// </summary>
public class RoomFunctionVariant
{
    // Identity
    public int FunctionId { get; set; }
    public string FunctionName { get; set; } = string.Empty;
    public string FunctionDetail { get; set; } = string.Empty;

    // Biome affinity (JSON array)
    public string? BiomeAffinity { get; set; }

    // Archetype
    public string Archetype { get; set; } = string.Empty;

    // Tags (JSON array)
    public string? Tags { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Parses biome affinity JSON into a list
    /// </summary>
    public List<string> GetBiomeAffinity()
    {
        if (string.IsNullOrEmpty(BiomeAffinity))
            return new List<string>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(BiomeAffinity) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Parses tags JSON into a list
    /// </summary>
    public List<string> GetTags()
    {
        if (string.IsNullOrEmpty(Tags))
            return new List<string>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(Tags) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Checks if this function variant is suitable for a specific biome
    /// </summary>
    public bool IsSuitableForBiome(string biome)
    {
        var affinities = GetBiomeAffinity();
        if (affinities.Count == 0)
            return true;  // No restrictions

        return affinities.Contains(biome);
    }
}
