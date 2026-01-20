namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the type of resource change that occurred.
/// </summary>
public enum ResourceChangeType
{
    /// <summary>Resource gained from regeneration.</summary>
    Regeneration,

    /// <summary>Resource lost from decay.</summary>
    Decay,

    /// <summary>Resource spent on an ability.</summary>
    Spent,

    /// <summary>Resource gained from an action.</summary>
    Gained,

    /// <summary>Resource gained from dealing damage.</summary>
    BuildOnDamageDealt,

    /// <summary>Resource gained from taking damage.</summary>
    BuildOnDamageTaken,

    /// <summary>Resource gained from healing.</summary>
    BuildOnHeal
}

/// <summary>
/// Represents a change to a resource pool.
/// </summary>
public record ResourceChange(
    string ResourceTypeId,
    int PreviousValue,
    int NewValue,
    ResourceChangeType ChangeType)
{
    /// <summary>Gets the amount of change (positive for gain, negative for loss).</summary>
    public int Delta => NewValue - PreviousValue;

    /// <summary>Gets whether this was a gain (positive change).</summary>
    public bool IsGain => Delta > 0;

    /// <summary>Gets whether this was a loss (negative change).</summary>
    public bool IsLoss => Delta < 0;
}

/// <summary>
/// Contains all resource changes from a game action or turn end.
/// </summary>
public record ResourceChangeResult(IReadOnlyList<ResourceChange> Changes)
{
    /// <summary>Gets whether any changes occurred.</summary>
    public bool HasChanges => Changes.Count > 0;

    /// <summary>Gets an empty result with no changes.</summary>
    public static ResourceChangeResult Empty => new([]);
}
