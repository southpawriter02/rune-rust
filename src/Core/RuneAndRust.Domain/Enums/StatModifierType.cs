namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines how a stat modifier is applied to a base value.
/// </summary>
/// <remarks>
/// <para>Application order: Flat modifiers first, then Percentage, then Override.</para>
/// <para>Override takes complete precedence and ignores other modifiers.</para>
/// </remarks>
public enum StatModifierType
{
    /// <summary>
    /// Adds a flat value to the stat.
    /// Example: +2 Attack, -3 Defense.
    /// Applied first in calculation order.
    /// </summary>
    Flat,

    /// <summary>
    /// Multiplies the stat by a percentage.
    /// Example: +30% Attack (value = 0.3), -50% Speed (value = -0.5).
    /// Applied after flat modifiers.
    /// </summary>
    Percentage,

    /// <summary>
    /// Sets the stat to a specific value, ignoring all other modifiers.
    /// Rarely used, for special effects that set stats absolutely.
    /// </summary>
    Override
}
