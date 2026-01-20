using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents an architectural style for dungeon rooms.
/// </summary>
public class ArchitecturalStyle : IEntity
{
    /// <summary>
    /// Gets the unique identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the style string ID.
    /// </summary>
    public string StyleId { get; private set; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets compatible biome IDs (empty = all biomes).
    /// </summary>
    public IReadOnlyList<string> CompatibleBiomes { get; private set; }

    /// <summary>
    /// Gets the minimum depth where this style can appear.
    /// </summary>
    public int MinDepth { get; private set; }

    /// <summary>
    /// Gets the maximum depth where this style can appear (null = no limit).
    /// </summary>
    public int? MaxDepth { get; private set; }

    /// <summary>
    /// Gets the style descriptors.
    /// </summary>
    public StyleDescriptors Descriptors { get; private set; }

    /// <summary>
    /// Gets the style rules.
    /// </summary>
    public StyleRules Rules { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private ArchitecturalStyle()
    {
        StyleId = null!;
        Name = null!;
        Description = string.Empty;
        CompatibleBiomes = [];
        Descriptors = StyleDescriptors.Empty;
        Rules = StyleRules.Default;
    }

    /// <summary>
    /// Creates a new architectural style.
    /// </summary>
    public static ArchitecturalStyle Create(
        string styleId,
        string name,
        string description,
        IReadOnlyList<string>? compatibleBiomes = null,
        int minDepth = 0,
        int? maxDepth = null,
        StyleDescriptors? descriptors = null,
        StyleRules? rules = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(styleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new ArchitecturalStyle
        {
            Id = Guid.NewGuid(),
            StyleId = styleId.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            CompatibleBiomes = compatibleBiomes ?? [],
            MinDepth = minDepth,
            MaxDepth = maxDepth,
            Descriptors = descriptors ?? StyleDescriptors.Empty,
            Rules = rules ?? StyleRules.Default
        };
    }

    /// <summary>
    /// Checks if this style is compatible with a biome.
    /// </summary>
    public bool IsCompatibleWith(string biomeId) =>
        CompatibleBiomes.Count == 0 ||
        CompatibleBiomes.Any(b => b.Equals(biomeId, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Checks if this style is valid at the given depth.
    /// </summary>
    public bool IsValidAtDepth(int depth)
    {
        if (depth < MinDepth) return false;
        if (MaxDepth.HasValue && depth > MaxDepth.Value) return false;
        return true;
    }
}
