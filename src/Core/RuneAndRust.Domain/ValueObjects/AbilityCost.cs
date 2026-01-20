namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the resource cost to use an ability.
/// </summary>
/// <remarks>
/// AbilityCost is an immutable value object that defines what resource type
/// and amount is required to use an ability. Zero-cost abilities can use
/// the <see cref="None"/> static property.
/// </remarks>
public readonly record struct AbilityCost
{
    /// <summary>
    /// Gets the resource type required (e.g., "mana", "rage", "energy").
    /// </summary>
    public string ResourceTypeId { get; init; }

    /// <summary>
    /// Gets the amount of resource required.
    /// </summary>
    public int Amount { get; init; }

    /// <summary>
    /// Gets whether this cost requires any resource.
    /// </summary>
    public bool HasCost => Amount > 0 && !string.IsNullOrEmpty(ResourceTypeId);

    /// <summary>
    /// Gets a zero-cost instance for abilities with no resource cost.
    /// </summary>
    public static AbilityCost None => new() { ResourceTypeId = string.Empty, Amount = 0 };

    /// <summary>
    /// Creates a new ability cost.
    /// </summary>
    /// <param name="resourceTypeId">The resource type ID (e.g., "mana", "rage").</param>
    /// <param name="amount">The amount of resource required (cannot be negative).</param>
    /// <returns>A new AbilityCost instance.</returns>
    /// <exception cref="ArgumentException">Thrown when resourceTypeId is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when amount is negative.</exception>
    public static AbilityCost Create(string resourceTypeId, int amount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceTypeId);
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        return new AbilityCost
        {
            ResourceTypeId = resourceTypeId.ToLowerInvariant(),
            Amount = amount
        };
    }

    /// <summary>
    /// Returns a string representation of this cost.
    /// </summary>
    /// <returns>A formatted string showing the cost amount and resource type, or "Free" for zero-cost.</returns>
    public override string ToString() => HasCost ? $"{Amount} {ResourceTypeId}" : "Free";
}
