namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38: Tier 1 - Base Descriptor Template
/// Represents a biome-agnostic archetype (e.g., Pillar_Base, Corridor_Base)
/// These templates are combined with ThematicModifiers to create final descriptors
/// </summary>
public class DescriptorBaseTemplate
{
    // Identity
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;  // Room, Feature, Object, Atmospheric, Loot
    public string Archetype { get; set; } = string.Empty;  // Cover, Hazard, Container, Ambient, etc.

    // Mechanical properties (JSON)
    public string? BaseMechanics { get; set; }

    // Description templates (use placeholders: {Modifier}, {Modifier_Adj}, {Modifier_Detail})
    public string NameTemplate { get; set; } = string.Empty;
    public string DescriptionTemplate { get; set; } = string.Empty;

    // Metadata
    public string? Tags { get; set; }  // JSON array
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Parses base mechanics JSON into a dictionary
    /// </summary>
    public Dictionary<string, object>? GetBaseMechanics()
    {
        if (string.IsNullOrEmpty(BaseMechanics))
            return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(BaseMechanics);
        }
        catch
        {
            return null;
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
    /// Validates that the template has required fields
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(TemplateName)) return false;
        if (string.IsNullOrEmpty(Category)) return false;
        if (string.IsNullOrEmpty(Archetype)) return false;
        if (string.IsNullOrEmpty(NameTemplate)) return false;
        if (string.IsNullOrEmpty(DescriptionTemplate)) return false;

        // Validate category
        var validCategories = new[] { "Room", "Feature", "Object", "Atmospheric", "Loot" };
        if (!validCategories.Contains(Category))
            return false;

        return true;
    }
}
