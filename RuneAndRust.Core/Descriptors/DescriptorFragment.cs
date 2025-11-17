namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38.1: Descriptor fragment model
/// Represents reusable text fragments for room descriptions
/// </summary>
public class DescriptorFragment
{
    // Identity
    public int FragmentId { get; set; }
    public string Category { get; set; } = string.Empty;  // SpatialDescriptor, ArchitecturalFeature, Detail, Atmospheric, Direction
    public string? Subcategory { get; set; }  // Wall, Ceiling, Floor, Decay, Runes, etc.
    public string FragmentText { get; set; } = string.Empty;

    // Filtering
    public string? Tags { get; set; }  // JSON array
    public float Weight { get; set; } = 1.0f;

    // Status
    public bool IsActive { get; set; } = true;

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

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
    /// Checks if this fragment matches all required tags
    /// </summary>
    public bool MatchesTags(List<string> requiredTags)
    {
        if (requiredTags == null || requiredTags.Count == 0)
            return true;

        var fragmentTags = GetTags();
        if (fragmentTags.Count == 0)
            return false;

        return requiredTags.Any(requiredTag => fragmentTags.Contains(requiredTag));
    }
}

/// <summary>
/// Fragment categories enumeration
/// </summary>
public enum FragmentCategory
{
    SpatialDescriptor,
    ArchitecturalFeature,
    Detail,
    Atmospheric,
    Direction
}
