namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines a range for randomized attribute bonuses on equipment.
/// </summary>
/// <remarks>
/// <para>
/// Attribute bonus ranges allow equipment to have variable bonuses
/// within tier-appropriate bounds. For example, Tier 3 weapons have
/// a range of +1 to +2, while Tier 4 weapons have +2 to +4.
/// </para>
/// <para>
/// The RollBonus method uses inclusive bounds (both min and max are
/// possible results).
/// </para>
/// </remarks>
/// <param name="MinBonus">Minimum possible attribute bonus (inclusive).</param>
/// <param name="MaxBonus">Maximum possible attribute bonus (inclusive).</param>
public readonly record struct AttributeBonusRange(int MinBonus, int MaxBonus)
{
    /// <summary>
    /// Gets whether this is a fixed (non-random) bonus.
    /// </summary>
    public bool IsFixed => MinBonus == MaxBonus;

    /// <summary>
    /// Gets the range span (difference between max and min, plus 1).
    /// </summary>
    public int RangeSpan => MaxBonus - MinBonus + 1;

    /// <summary>
    /// Rolls a random bonus within the range.
    /// </summary>
    /// <param name="random">Random number generator.</param>
    /// <returns>A random value between MinBonus and MaxBonus (inclusive).</returns>
    /// <exception cref="ArgumentNullException">Thrown when random is null.</exception>
    public int RollBonus(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);

        if (IsFixed)
        {
            return MinBonus;
        }

        // Random.Next upper bound is exclusive, so add 1
        return random.Next(MinBonus, MaxBonus + 1);
    }

    /// <summary>
    /// Creates a new AttributeBonusRange with validation.
    /// </summary>
    /// <param name="minBonus">Minimum bonus value.</param>
    /// <param name="maxBonus">Maximum bonus value.</param>
    /// <returns>A new AttributeBonusRange instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when min > max or values are negative.</exception>
    public static AttributeBonusRange Create(int minBonus, int maxBonus)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minBonus);
        ArgumentOutOfRangeException.ThrowIfNegative(maxBonus);

        if (minBonus > maxBonus)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minBonus),
                $"MinBonus ({minBonus}) cannot be greater than MaxBonus ({maxBonus}).");
        }

        return new AttributeBonusRange(minBonus, maxBonus);
    }

    /// <summary>
    /// Creates a fixed (non-random) attribute bonus.
    /// </summary>
    /// <param name="bonus">The fixed bonus value.</param>
    /// <returns>An AttributeBonusRange with min == max.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when bonus is negative.</exception>
    public static AttributeBonusRange Fixed(int bonus)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bonus);
        return new AttributeBonusRange(bonus, bonus);
    }

    // Standard tier bonus ranges

    /// <summary>
    /// Fixed +1 bonus for Tier 2 (ClanForged) items.
    /// </summary>
    public static AttributeBonusRange Tier2Default => Fixed(1);

    /// <summary>
    /// +1 to +2 range for Tier 3 (Optimized) items.
    /// </summary>
    public static AttributeBonusRange Tier3Default => Create(1, 2);

    /// <summary>
    /// +2 to +4 range for Tier 4 (MythForged) items.
    /// </summary>
    public static AttributeBonusRange Tier4Default => Create(2, 4);

    /// <summary>
    /// Creates a display string for debug/logging purposes.
    /// </summary>
    public override string ToString() =>
        IsFixed ? $"+{MinBonus}" : $"+{MinBonus} to +{MaxBonus}";
}
