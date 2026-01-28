namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the result of a drop probability roll.
/// </summary>
/// <remarks>
/// <para>
/// A drop roll result is either a successful drop with a specific quality
/// tier, or a "no drop" result where the enemy dropped nothing.
/// </para>
/// <para>
/// The <see cref="RolledValue"/> property is included for debugging and
/// logging purposes to understand the random distribution over time.
/// </para>
/// </remarks>
/// <param name="DroppedTier">The quality tier that was rolled, or null for no-drop.</param>
/// <param name="IsNoDrop">Whether this result is a no-drop.</param>
/// <param name="RolledValue">The random value that was rolled (0.0-1.0) for debugging.</param>
public readonly record struct DropRollResult(
    QualityTier? DroppedTier,
    bool IsNoDrop,
    decimal RolledValue)
{
    /// <summary>
    /// Gets whether a valid tier was rolled.
    /// </summary>
    public bool HasDrop => !IsNoDrop && DroppedTier.HasValue;

    /// <summary>
    /// Gets the dropped tier, throwing if no drop occurred.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when IsNoDrop is true.</exception>
    public QualityTier Tier =>
        DroppedTier ?? throw new InvalidOperationException(
            "Cannot access Tier on a no-drop result.");

    /// <summary>
    /// Creates a successful drop result.
    /// </summary>
    /// <param name="tier">The quality tier that was rolled.</param>
    /// <param name="rolledValue">The random value for debugging.</param>
    /// <returns>A drop result with the specified tier.</returns>
    public static DropRollResult Dropped(QualityTier tier, decimal rolledValue)
    {
        return new DropRollResult(tier, false, rolledValue);
    }

    /// <summary>
    /// Creates a no-drop result.
    /// </summary>
    /// <param name="rolledValue">The random value for debugging.</param>
    /// <returns>A no-drop result.</returns>
    public static DropRollResult NoDrop(decimal rolledValue)
    {
        return new DropRollResult(null, true, rolledValue);
    }

    /// <summary>
    /// Creates a display string for debug/logging purposes.
    /// </summary>
    public override string ToString()
    {
        if (IsNoDrop)
        {
            return $"No Drop (rolled {RolledValue:F3})";
        }
        return $"{DroppedTier} (rolled {RolledValue:F3})";
    }
}
