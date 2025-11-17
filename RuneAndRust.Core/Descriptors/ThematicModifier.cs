namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38: Tier 2 - Thematic Modifier
/// Represents a biome-specific variation (e.g., Scorched, Frozen, Rusted)
/// Modifiers are combined with BaseTemplates to create themed descriptors
/// </summary>
public class ThematicModifier
{
    // Identity
    public int ModifierId { get; set; }
    public string ModifierName { get; set; } = string.Empty;
    public string PrimaryBiome { get; set; } = string.Empty;  // The_Roots, Muspelheim, Niflheim, Alfheim, Jotunheim

    // Descriptive properties
    public string Adjective { get; set; } = string.Empty;  // "scorched", "ice-covered", "corroded"
    public string DetailFragment { get; set; } = string.Empty;  // "radiates intense heat"

    // Mechanical modifiers (JSON)
    public string? StatModifiers { get; set; }
    public string? StatusEffects { get; set; }

    // Visual/atmospheric properties
    public string? ColorPalette { get; set; }  // "red-orange-black"
    public string? AmbientSounds { get; set; }  // JSON array
    public string? ParticleEffects { get; set; }  // JSON array

    // Metadata
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Parses stat modifiers JSON into a dictionary
    /// </summary>
    public Dictionary<string, object>? GetStatModifiers()
    {
        if (string.IsNullOrEmpty(StatModifiers))
            return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(StatModifiers);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses status effects JSON into a list
    /// </summary>
    public List<Dictionary<string, object>>? GetStatusEffects()
    {
        if (string.IsNullOrEmpty(StatusEffects))
            return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(StatusEffects);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses ambient sounds JSON into a list
    /// </summary>
    public List<string> GetAmbientSounds()
    {
        if (string.IsNullOrEmpty(AmbientSounds))
            return new List<string>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(AmbientSounds) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Validates that the modifier has required fields
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(ModifierName)) return false;
        if (string.IsNullOrEmpty(PrimaryBiome)) return false;
        if (string.IsNullOrEmpty(Adjective)) return false;
        if (string.IsNullOrEmpty(DetailFragment)) return false;

        // Validate biome
        var validBiomes = new[] { "The_Roots", "Muspelheim", "Niflheim", "Alfheim", "Jotunheim" };
        if (!validBiomes.Contains(PrimaryBiome))
            return false;

        return true;
    }
}
