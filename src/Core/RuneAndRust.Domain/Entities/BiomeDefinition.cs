using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a biome definition loaded from configuration.
/// </summary>
/// <remarks>
/// Biomes define themed areas within dungeons with unique descriptors,
/// gameplay rules, and depth constraints for spawn distribution.
/// </remarks>
public class BiomeDefinition : IEntity
{
    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the string identifier for the biome (e.g., "fungal-caverns").
    /// </summary>
    public string BiomeId { get; private set; }

    /// <summary>
    /// Gets the display name of the biome.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the biome description for codex/UI.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the minimum depth (Z-level) where this biome can appear.
    /// </summary>
    public int MinDepth { get; private set; }

    /// <summary>
    /// Gets the maximum depth (Z-level) where this biome can appear.
    /// </summary>
    /// <remarks>
    /// Null means no maximum depth (can appear at any depth below MinDepth).
    /// </remarks>
    public int? MaxDepth { get; private set; }

    /// <summary>
    /// Gets the base selection weight for weighted random selection.
    /// </summary>
    public int BaseWeight { get; private set; }

    /// <summary>
    /// Gets the tags for filtering (e.g., "natural", "underground", "aquatic").
    /// </summary>
    public IReadOnlyList<string> Tags { get; private set; }

    /// <summary>
    /// Gets the biome-themed descriptors.
    /// </summary>
    public BiomeDescriptors Descriptors { get; private set; }

    /// <summary>
    /// Gets the gameplay rules for this biome.
    /// </summary>
    public BiomeRules Rules { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private BiomeDefinition()
    {
        BiomeId = null!;
        Name = null!;
        Description = null!;
        Tags = [];
        Descriptors = BiomeDescriptors.Empty;
        Rules = BiomeRules.Default;
    }

    /// <summary>
    /// Creates a new biome definition.
    /// </summary>
    public static BiomeDefinition Create(
        string biomeId,
        string name,
        string description,
        int minDepth = 0,
        int? maxDepth = null,
        int baseWeight = 100,
        IReadOnlyList<string>? tags = null,
        BiomeDescriptors? descriptors = null,
        BiomeRules? rules = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(biomeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(minDepth);

        if (maxDepth.HasValue && maxDepth.Value < minDepth)
            throw new ArgumentException("MaxDepth cannot be less than MinDepth", nameof(maxDepth));

        return new BiomeDefinition
        {
            Id = Guid.NewGuid(),
            BiomeId = biomeId.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            MinDepth = minDepth,
            MaxDepth = maxDepth,
            BaseWeight = baseWeight,
            Tags = tags ?? [],
            Descriptors = descriptors ?? BiomeDescriptors.Empty,
            Rules = rules ?? BiomeRules.Default
        };
    }

    /// <summary>
    /// Checks if this biome is valid for the specified depth.
    /// </summary>
    public bool IsValidForDepth(int depth)
    {
        if (depth < MinDepth)
            return false;

        if (MaxDepth.HasValue && depth > MaxDepth.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Checks if this biome has the specified tag.
    /// </summary>
    public bool HasTag(string tag) =>
        Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Checks if this biome has all the specified tags.
    /// </summary>
    public bool HasAllTags(IEnumerable<string> requiredTags) =>
        requiredTags.All(HasTag);
}
