namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Represents an NPC's disposition toward the player as a numeric value.
/// </summary>
/// <remarks>
/// <para>
/// Disposition values range from -100 (maximum hostility) to +100 (maximum alliance).
/// The value is converted to <see cref="NpcDisposition"/> categories for modifier
/// calculation and interaction restrictions.
/// </para>
/// <para>
/// Disposition changes from social interactions, quests, and faction events.
/// Some effects can lock disposition at a certain level until recovery conditions are met.
/// </para>
/// </remarks>
/// <param name="Value">The numeric disposition value, clamped to -100 to +100.</param>
/// <param name="IsLocked">Whether disposition changes are blocked (e.g., after [Trust Shattered]).</param>
/// <param name="LockedReason">The reason disposition is locked, if applicable.</param>
public readonly record struct DispositionLevel(
    int Value,
    bool IsLocked = false,
    string? LockedReason = null)
{
    /// <summary>
    /// Minimum possible disposition value.
    /// </summary>
    public const int MinValue = -100;

    /// <summary>
    /// Maximum possible disposition value.
    /// </summary>
    public const int MaxValue = 100;

    /// <summary>
    /// Default neutral disposition value.
    /// </summary>
    public const int DefaultValue = 0;

    /// <summary>
    /// Gets the clamped disposition value within valid range.
    /// </summary>
    public int ClampedValue => Math.Clamp(Value, MinValue, MaxValue);

    /// <summary>
    /// Gets the disposition category derived from the numeric value.
    /// </summary>
    /// <remarks>
    /// The category determines the dice modifier and available interactions.
    /// </remarks>
    public NpcDisposition Category => NpcDispositionExtensions.FromValue(ClampedValue);

    /// <summary>
    /// Gets the dice modifier for social checks based on disposition.
    /// </summary>
    /// <remarks>
    /// Returns values from +3d10 (Ally) to -2d10 (Hostile).
    /// </remarks>
    public int DiceModifier => Category.GetDiceModifier();

    /// <summary>
    /// Gets whether this disposition level blocks certain interactions.
    /// </summary>
    /// <remarks>
    /// Hostile disposition may prevent certain social options entirely.
    /// </remarks>
    public bool BlocksSomeInteractions => Category.BlocksSomeInteractions();

    /// <summary>
    /// Gets whether this disposition allows forming alliances.
    /// </summary>
    /// <remarks>
    /// Requires at least Friendly disposition for alliance formation.
    /// </remarks>
    public bool AllowsAlliance => Category.AllowsAlliance();

    /// <summary>
    /// Gets whether the disposition is currently in a positive range.
    /// </summary>
    public bool IsPositive => Category.IsPositive();

    /// <summary>
    /// Gets whether the disposition is currently in a negative range.
    /// </summary>
    public bool IsNegative => Category.IsNegative();

    /// <summary>
    /// Creates a new disposition level with default neutral value.
    /// </summary>
    /// <returns>A neutral disposition level (value 0).</returns>
    public static DispositionLevel CreateNeutral() => new(DefaultValue);

    /// <summary>
    /// Creates a new disposition level at the specified value.
    /// </summary>
    /// <param name="value">The disposition value.</param>
    /// <returns>A new disposition level clamped to valid range.</returns>
    public static DispositionLevel Create(int value) => new(Math.Clamp(value, MinValue, MaxValue));

    /// <summary>
    /// Creates a new disposition level from a category.
    /// </summary>
    /// <param name="category">The disposition category.</param>
    /// <returns>A disposition level at the midpoint of the category range.</returns>
    /// <remarks>
    /// Useful for initializing NPCs at a specific disposition category.
    /// </remarks>
    public static DispositionLevel FromCategory(NpcDisposition category)
    {
        var midpoint = category.GetMidpointValue();
        return new DispositionLevel(midpoint);
    }

    /// <summary>
    /// Modifies the disposition by the specified amount.
    /// </summary>
    /// <param name="change">The amount to change (positive or negative).</param>
    /// <returns>A new disposition level with the modified value, or unchanged if locked.</returns>
    /// <remarks>
    /// If the disposition is locked (e.g., due to a fumble consequence),
    /// no modification occurs and the current level is returned.
    /// </remarks>
    public DispositionLevel Modify(int change)
    {
        if (IsLocked)
            return this;

        return this with { Value = Math.Clamp(ClampedValue + change, MinValue, MaxValue) };
    }

    /// <summary>
    /// Sets the disposition to a specific value, ignoring the current value.
    /// </summary>
    /// <param name="newValue">The new disposition value.</param>
    /// <returns>A new disposition level at the specified value, or unchanged if locked.</returns>
    public DispositionLevel SetValue(int newValue)
    {
        if (IsLocked)
            return this;

        return this with { Value = Math.Clamp(newValue, MinValue, MaxValue) };
    }

    /// <summary>
    /// Locks the disposition at its current value.
    /// </summary>
    /// <param name="reason">The reason for locking (e.g., fumble consequence).</param>
    /// <returns>A locked disposition level.</returns>
    /// <remarks>
    /// Locked dispositions cannot be modified until unlocked.
    /// This is typically used for fumble consequences like [Trust Shattered].
    /// </remarks>
    public DispositionLevel Lock(string reason)
    {
        return this with { IsLocked = true, LockedReason = reason };
    }

    /// <summary>
    /// Unlocks the disposition, allowing future changes.
    /// </summary>
    /// <returns>An unlocked disposition level.</returns>
    /// <remarks>
    /// Unlocking typically requires completing a recovery quest or
    /// waiting for a significant time period.
    /// </remarks>
    public DispositionLevel Unlock()
    {
        return this with { IsLocked = false, LockedReason = null };
    }

    /// <summary>
    /// Calculates the distance to reach a target disposition category.
    /// </summary>
    /// <param name="target">The target disposition category.</param>
    /// <returns>
    /// The minimum positive or negative change required to reach the target category.
    /// Positive value means disposition must increase, negative means decrease.
    /// </returns>
    public int DistanceTo(NpcDisposition target)
    {
        if (Category == target)
            return 0;

        if (target > Category)
        {
            // Need to increase disposition
            return target.GetMinThreshold() - ClampedValue;
        }
        else
        {
            // Need to decrease disposition (return negative distance)
            return target.GetMaxThreshold() - ClampedValue;
        }
    }

    /// <summary>
    /// Gets a human-readable description of the disposition.
    /// </summary>
    /// <returns>A formatted string for UI display.</returns>
    public string ToDescription()
    {
        var modifierStr = DiceModifier >= 0 ? $"+{DiceModifier}d10" : $"{DiceModifier}d10";
        var lockedStr = IsLocked ? $" [LOCKED: {LockedReason}]" : "";
        return $"{Category.GetDescription()} ({ClampedValue}, {modifierStr}){lockedStr}";
    }

    /// <summary>
    /// Gets a compact representation of the disposition.
    /// </summary>
    /// <returns>A short string showing category and modifier.</returns>
    public string ToShortDescription()
    {
        var modifierStr = DiceModifier >= 0 ? $"+{DiceModifier}" : $"{DiceModifier}";
        return $"{Category.GetDescription()} ({modifierStr}d10)";
    }

    /// <inheritdoc/>
    public override string ToString() => ToDescription();
}
