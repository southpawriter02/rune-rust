using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Interfaces;

/// <summary>
/// Base interface for all skill check modifiers.
/// </summary>
/// <remarks>
/// <para>
/// Defines the common contract for modifiers that affect skill checks,
/// including equipment, situational, environmental, and target-based factors.
/// </para>
/// <para>
/// All modifier implementations should be immutable value objects.
/// </para>
/// </remarks>
public interface ISkillModifier
{
    /// <summary>
    /// Gets the bonus or penalty to the dice pool.
    /// </summary>
    /// <remarks>
    /// Positive values add dice, negative values remove dice.
    /// The final pool is clamped to a minimum of 1 die.
    /// </remarks>
    int DiceModifier { get; }

    /// <summary>
    /// Gets the bonus or penalty to the difficulty class.
    /// </summary>
    /// <remarks>
    /// Positive values increase difficulty, negative values decrease it.
    /// The final DC is clamped to a minimum of 0.
    /// </remarks>
    int DcModifier { get; }

    /// <summary>
    /// Gets the category of this modifier.
    /// </summary>
    ModifierCategory Category { get; }

    /// <summary>
    /// Returns a short description suitable for UI display.
    /// </summary>
    /// <returns>A formatted string describing the modifier and its effects.</returns>
    /// <example>
    /// "Tinker's Toolkit (+2d10)"
    /// "Dim Lighting (DC +1)"
    /// "Friendly Disposition (+2d10)"
    /// </example>
    string ToShortDescription();
}
