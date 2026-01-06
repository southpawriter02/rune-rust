using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// A reusable text fragment for descriptor composition (Tier 3).
/// Fragments are selected via weighted random with biome/tag filtering
/// to fill placeholder tokens in base templates.
/// </summary>
public class DescriptorFragment : IEntity
{
    public Guid Id { get; private set; }
    public FragmentCategory Category { get; private set; }
    public string? Subcategory { get; private set; }
    public string Text { get; private set; }
    public Biome? BiomeAffinity { get; private set; }
    public int Weight { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly HashSet<string> _tags = [];
    public IReadOnlySet<string> Tags => _tags;

    private DescriptorFragment()
    {
        Text = null!;
    } // For EF Core

    public DescriptorFragment(
        FragmentCategory category,
        string text,
        string? subcategory = null,
        Biome? biomeAffinity = null,
        int weight = 1,
        IEnumerable<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be empty", nameof(text));
        if (weight < 1)
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be at least 1");

        Id = Guid.NewGuid();
        Category = category;
        Subcategory = subcategory;
        Text = text;
        BiomeAffinity = biomeAffinity;
        Weight = weight;
        CreatedAt = DateTime.UtcNow;

        if (tags != null)
        {
            foreach (var tag in tags)
            {
                _tags.Add(tag);
            }
        }
    }

    /// <summary>
    /// Constructor for seeding with known Id.
    /// </summary>
    public DescriptorFragment(
        Guid id,
        FragmentCategory category,
        string text,
        string? subcategory = null,
        Biome? biomeAffinity = null,
        int weight = 1,
        IEnumerable<string>? tags = null)
        : this(category, text, subcategory, biomeAffinity, weight, tags)
    {
        Id = id;
    }

    // Factory methods for each category

    /// <summary>
    /// Creates a spatial descriptor fragment (room size/feel).
    /// </summary>
    public static DescriptorFragment CreateSpatial(
        string text,
        Biome? biomeAffinity = null,
        int weight = 1,
        params string[] tags) =>
        new(FragmentCategory.Spatial, text, null, biomeAffinity, weight, tags);

    /// <summary>
    /// Creates an architectural feature fragment (wall/ceiling/floor).
    /// </summary>
    public static DescriptorFragment CreateArchitectural(
        string text,
        ArchitecturalSubcategory subcategory,
        Biome? biomeAffinity = null,
        int weight = 1,
        params string[] tags) =>
        new(FragmentCategory.Architectural, text, subcategory.ToString(), biomeAffinity, weight, tags);

    /// <summary>
    /// Creates a detail fragment (decay, runes, activity, ominous, loot).
    /// </summary>
    public static DescriptorFragment CreateDetail(
        string text,
        DetailSubcategory subcategory,
        Biome? biomeAffinity = null,
        int weight = 1,
        params string[] tags) =>
        new(FragmentCategory.Detail, text, subcategory.ToString(), biomeAffinity, weight, tags);

    /// <summary>
    /// Creates an atmospheric fragment (smell, sound, light, temperature).
    /// </summary>
    public static DescriptorFragment CreateAtmospheric(
        string text,
        AtmosphericSubcategory subcategory,
        Biome? biomeAffinity = null,
        int weight = 1,
        params string[] tags) =>
        new(FragmentCategory.Atmospheric, text, subcategory.ToString(), biomeAffinity, weight, tags);

    /// <summary>
    /// Creates a direction descriptor fragment.
    /// </summary>
    public static DescriptorFragment CreateDirection(
        string text,
        Biome? biomeAffinity = null,
        int weight = 1,
        params string[] tags) =>
        new(FragmentCategory.Direction, text, null, biomeAffinity, weight, tags);

    /// <summary>
    /// Adds a tag to this fragment.
    /// </summary>
    public void AddTag(string tag)
    {
        if (!string.IsNullOrWhiteSpace(tag))
            _tags.Add(tag);
    }

    /// <summary>
    /// Checks if this fragment has a specific tag.
    /// </summary>
    public bool HasTag(string tag) => _tags.Contains(tag);

    /// <summary>
    /// Checks if this fragment matches the given biome (null affinity matches all).
    /// </summary>
    public bool MatchesBiome(Biome biome) =>
        BiomeAffinity == null || BiomeAffinity == biome;

    /// <summary>
    /// Checks if this fragment has any of the specified tags (or has no tags).
    /// </summary>
    public bool MatchesTags(IEnumerable<string>? requiredTags)
    {
        if (requiredTags == null || !requiredTags.Any())
            return true;
        if (_tags.Count == 0)
            return true; // Fragments with no tags match everything
        return _tags.Intersect(requiredTags).Any();
    }

    public override string ToString() => $"{Category}/{Subcategory ?? "General"}: {Text[..Math.Min(50, Text.Length)]}...";
}
