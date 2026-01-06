namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a pool of a specific resource with current and maximum values.
/// </summary>
/// <remarks>
/// ResourcePool is a mutable class that tracks the current state of a resource.
/// It provides methods for spending and gaining resource amounts.
/// </remarks>
public class ResourcePool
{
    /// <summary>Gets the ID of the resource type this pool represents.</summary>
    public string ResourceTypeId { get; }

    /// <summary>Gets the current amount of the resource.</summary>
    public int Current { get; private set; }

    /// <summary>Gets the maximum amount of the resource.</summary>
    public int Maximum { get; private set; }

    /// <summary>Gets the current fill percentage (0.0 to 1.0).</summary>
    public float Percentage => Maximum > 0 ? (float)Current / Maximum : 0f;

    /// <summary>Gets whether the pool is at maximum capacity.</summary>
    public bool IsFull => Current >= Maximum;

    /// <summary>Gets whether the pool is empty (zero).</summary>
    public bool IsEmpty => Current <= 0;

    /// <summary>
    /// Creates a new resource pool.
    /// </summary>
    /// <param name="resourceTypeId">The resource type ID.</param>
    /// <param name="maximum">The maximum capacity.</param>
    /// <param name="startAtZero">If true, starts at 0; otherwise starts at max.</param>
    public ResourcePool(string resourceTypeId, int maximum, bool startAtZero = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceTypeId);
        ArgumentOutOfRangeException.ThrowIfNegative(maximum);

        ResourceTypeId = resourceTypeId.ToLowerInvariant();
        Maximum = maximum;
        Current = startAtZero ? 0 : maximum;
    }

    /// <summary>
    /// Attempts to spend the specified amount of resource.
    /// </summary>
    /// <param name="amount">The amount to spend.</param>
    /// <returns>True if the resource was spent; false if insufficient.</returns>
    public bool Spend(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        if (Current < amount)
            return false;

        Current -= amount;
        return true;
    }

    /// <summary>
    /// Gains the specified amount of resource, up to the maximum.
    /// </summary>
    /// <param name="amount">The amount to gain.</param>
    /// <returns>The actual amount gained (may be less if capped at max).</returns>
    public int Gain(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        var previousValue = Current;
        Current = Math.Min(Maximum, Current + amount);
        return Current - previousValue;
    }

    /// <summary>
    /// Loses the specified amount of resource, down to zero.
    /// </summary>
    /// <param name="amount">The amount to lose.</param>
    /// <returns>The actual amount lost.</returns>
    public int Lose(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        var previousValue = Current;
        Current = Math.Max(0, Current - amount);
        return previousValue - Current;
    }

    /// <summary>Sets the current value to maximum.</summary>
    public void SetToMax() => Current = Maximum;

    /// <summary>Sets the current value to zero.</summary>
    public void SetToZero() => Current = 0;

    /// <summary>Sets the current value directly (clamped to valid range).</summary>
    public void SetCurrent(int value) => Current = Math.Clamp(value, 0, Maximum);

    /// <summary>
    /// Modifies the maximum value, optionally adjusting current proportionally.
    /// </summary>
    public void SetMaximum(int newMaximum, bool adjustCurrentProportionally = false)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(newMaximum);

        if (adjustCurrentProportionally && Maximum > 0)
        {
            var ratio = (float)Current / Maximum;
            Current = (int)(newMaximum * ratio);
        }

        Maximum = newMaximum;
        Current = Math.Min(Current, Maximum);
    }

    public override string ToString() => $"{ResourceTypeId}: {Current}/{Maximum}";
}
