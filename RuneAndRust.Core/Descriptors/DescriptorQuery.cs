namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38: Query object for filtering descriptors
/// Used by DescriptorService to search for descriptors based on criteria
/// </summary>
public class DescriptorQuery
{
    // Category filter (Room, Feature, Object, Atmospheric, Loot)
    public string? Category { get; set; }

    // Archetype filter (Cover, Hazard, Container, Ambient, etc.)
    public string? Archetype { get; set; }

    // Biome filter (The_Roots, Muspelheim, Niflheim, Alfheim, Jotunheim)
    public string? Biome { get; set; }

    // Tag filters
    public List<string>? RequiredTags { get; set; }
    public List<string>? ExcludedTags { get; set; }

    // Spawn rules filters
    public string? MinRoomSize { get; set; }  // Small, Medium, Large, XLarge
    public string? MaxRoomSize { get; set; }

    // Modifier filter
    public string? ModifierName { get; set; }
    public string? BaseTemplateName { get; set; }

    // Active filter
    public bool OnlyActive { get; set; } = true;

    // Limit
    public int? Limit { get; set; }

    /// <summary>
    /// Validates that the query has at least one filter
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Category) ||
               !string.IsNullOrEmpty(Archetype) ||
               !string.IsNullOrEmpty(Biome) ||
               !string.IsNullOrEmpty(ModifierName) ||
               !string.IsNullOrEmpty(BaseTemplateName) ||
               (RequiredTags != null && RequiredTags.Count > 0);
    }
}

/// <summary>
/// Result object for descriptor queries
/// </summary>
public class DescriptorQueryResult
{
    public List<DescriptorComposite> Descriptors { get; set; } = new();
    public int TotalCount { get; set; }
    public DescriptorQuery Query { get; set; } = new();
}
