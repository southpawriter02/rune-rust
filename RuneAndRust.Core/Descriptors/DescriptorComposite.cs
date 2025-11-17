namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38: Tier 3 - Composite Descriptor
/// Generated combination of a BaseTemplate + ThematicModifier
/// Example: Pillar_Base + Scorched = "Scorched Support Pillar"
/// </summary>
public class DescriptorComposite
{
    // Identity
    public int CompositeId { get; set; }
    public int BaseTemplateId { get; set; }
    public int? ModifierId { get; set; }  // NULL for unmodified base templates

    // Generated properties (computed from base + modifier)
    public string FinalName { get; set; } = string.Empty;
    public string FinalDescription { get; set; } = string.Empty;
    public string? FinalMechanics { get; set; }  // Merged base + modifier mechanics (JSON)

    // Spawn rules
    public string? BiomeRestrictions { get; set; }  // JSON array: ["Muspelheim", "The_Roots"]
    public float SpawnWeight { get; set; } = 1.0f;
    public string? SpawnRules { get; set; }  // JSON

    // Metadata
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties (populated by repository/service)
    public DescriptorBaseTemplate? BaseTemplate { get; set; }
    public ThematicModifier? Modifier { get; set; }

    /// <summary>
    /// Parses final mechanics JSON into a dictionary
    /// </summary>
    public Dictionary<string, object>? GetFinalMechanics()
    {
        if (string.IsNullOrEmpty(FinalMechanics))
            return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(FinalMechanics);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses biome restrictions JSON into a list
    /// </summary>
    public List<string> GetBiomeRestrictions()
    {
        if (string.IsNullOrEmpty(BiomeRestrictions))
            return new List<string>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(BiomeRestrictions) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Parses spawn rules JSON into a dictionary
    /// </summary>
    public Dictionary<string, object>? GetSpawnRules()
    {
        if (string.IsNullOrEmpty(SpawnRules))
            return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(SpawnRules);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks if this descriptor can spawn in a given biome
    /// </summary>
    public bool CanSpawnInBiome(string biome)
    {
        var restrictions = GetBiomeRestrictions();
        if (restrictions.Count == 0)
            return true;  // No restrictions = can spawn anywhere

        return restrictions.Contains(biome);
    }

    /// <summary>
    /// Validates that the composite has required fields
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(FinalName)) return false;
        if (string.IsNullOrEmpty(FinalDescription)) return false;
        if (BaseTemplateId <= 0) return false;
        if (SpawnWeight < 0) return false;

        return true;
    }
}
