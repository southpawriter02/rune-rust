namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents an entry in a spawn table with weight and constraints.
/// </summary>
public readonly record struct SpawnEntry
{
    /// <summary>
    /// Gets the entity ID to spawn (monster ID or item ID).
    /// </summary>
    public string EntityId { get; init; }

    /// <summary>
    /// Gets the selection weight (higher = more common).
    /// </summary>
    public int Weight { get; init; }

    /// <summary>
    /// Gets the minimum depth where this entity can spawn.
    /// </summary>
    public int MinDepth { get; init; }

    /// <summary>
    /// Gets the maximum depth where this entity can spawn (null = no limit).
    /// </summary>
    public int? MaxDepth { get; init; }

    /// <summary>
    /// Gets the tags required for this entry to be selectable.
    /// </summary>
    public IReadOnlyList<string> RequiredTags { get; init; }

    /// <summary>
    /// Creates a spawn entry.
    /// </summary>
    public static SpawnEntry Create(
        string entityId,
        int weight = 100,
        int minDepth = 0,
        int? maxDepth = null,
        IReadOnlyList<string>? requiredTags = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityId);
        ArgumentOutOfRangeException.ThrowIfNegative(weight);

        if (maxDepth.HasValue && maxDepth.Value < minDepth)
            throw new ArgumentException("MaxDepth cannot be less than MinDepth", nameof(maxDepth));

        return new SpawnEntry
        {
            EntityId = entityId.ToLowerInvariant(),
            Weight = weight,
            MinDepth = minDepth,
            MaxDepth = maxDepth,
            RequiredTags = requiredTags ?? []
        };
    }

    /// <summary>
    /// Checks if this entry is valid for the specified depth.
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
    /// Checks if this entry has all required tags satisfied by the context.
    /// </summary>
    public bool HasRequiredTags(IEnumerable<string> contextTags)
    {
        if (RequiredTags.Count == 0)
            return true;

        var tagSet = contextTags.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return RequiredTags.All(t => tagSet.Contains(t));
    }
}
