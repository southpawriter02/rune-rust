using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Defines connection rules between two biomes.
/// </summary>
public class BiomeTransition : IEntity
{
    /// <summary>
    /// Gets the unique identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the source biome ID.
    /// </summary>
    public string SourceBiomeId { get; private set; }

    /// <summary>
    /// Gets the target biome ID.
    /// </summary>
    public string TargetBiomeId { get; private set; }

    /// <summary>
    /// Gets the transition style.
    /// </summary>
    public TransitionStyle Style { get; private set; }

    /// <summary>
    /// Gets whether this transition is allowed.
    /// </summary>
    public bool IsAllowed { get; private set; }

    /// <summary>
    /// Gets whether this transition works in both directions.
    /// </summary>
    public bool IsBidirectional { get; private set; }

    /// <summary>
    /// Gets the minimum depth at which this transition can occur.
    /// </summary>
    public int MinDepth { get; private set; }

    /// <summary>
    /// Gets the maximum depth at which this transition can occur (null = no limit).
    /// </summary>
    public int? MaxDepth { get; private set; }

    /// <summary>
    /// Gets the number of rooms in the transition zone (for Gradual style).
    /// </summary>
    public int TransitionLength { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private BiomeTransition()
    {
        SourceBiomeId = null!;
        TargetBiomeId = null!;
    }

    /// <summary>
    /// Creates a new biome transition.
    /// </summary>
    public static BiomeTransition Create(
        string sourceBiomeId,
        string targetBiomeId,
        TransitionStyle style = TransitionStyle.Gradual,
        bool isAllowed = true,
        bool isBidirectional = true,
        int minDepth = 0,
        int? maxDepth = null,
        int transitionLength = 3)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceBiomeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetBiomeId);
        ArgumentOutOfRangeException.ThrowIfNegative(minDepth);
        ArgumentOutOfRangeException.ThrowIfLessThan(transitionLength, 1);

        return new BiomeTransition
        {
            Id = Guid.NewGuid(),
            SourceBiomeId = sourceBiomeId.ToLowerInvariant(),
            TargetBiomeId = targetBiomeId.ToLowerInvariant(),
            Style = style,
            IsAllowed = isAllowed,
            IsBidirectional = isBidirectional,
            MinDepth = minDepth,
            MaxDepth = maxDepth,
            TransitionLength = transitionLength
        };
    }

    /// <summary>
    /// Checks if this transition applies to the given biome pair.
    /// </summary>
    public bool AppliesTo(string source, string target)
    {
        if (SourceBiomeId.Equals(source, StringComparison.OrdinalIgnoreCase) &&
            TargetBiomeId.Equals(target, StringComparison.OrdinalIgnoreCase))
            return true;

        if (IsBidirectional &&
            SourceBiomeId.Equals(target, StringComparison.OrdinalIgnoreCase) &&
            TargetBiomeId.Equals(source, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    /// <summary>
    /// Checks if this transition is valid at the given depth.
    /// </summary>
    public bool IsValidAtDepth(int depth)
    {
        if (depth < MinDepth) return false;
        if (MaxDepth.HasValue && depth > MaxDepth.Value) return false;
        return true;
    }
}
