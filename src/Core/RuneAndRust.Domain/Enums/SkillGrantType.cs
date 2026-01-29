// ═══════════════════════════════════════════════════════════════════════════════
// SkillGrantType.cs
// Enum defining how a skill grant from a background is applied to a character.
// Version: 0.17.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines how a skill grant from a background is applied to a character.
/// </summary>
/// <remarks>
/// <para>
/// SkillGrantType determines the mechanical effect of skill bonuses granted by
/// backgrounds. Different grant types allow backgrounds to provide various
/// forms of professional expertise accumulated during the character's
/// pre-Silence occupation.
/// </para>
/// <para>
/// The three grant types serve distinct purposes:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <see cref="Permanent"/>: Core professional skills that persist throughout the game
///     and stack with other bonuses from training, equipment, or abilities.
///   </description></item>
///   <item><description>
///     <see cref="StartingBonus"/>: Initial advantage that provides a head start but
///     doesn't stack with later skill development.
///   </description></item>
///   <item><description>
///     <see cref="Proficiency"/>: Removes the untrained penalty for specialized skills,
///     allowing the character to attempt checks without disadvantage.
///   </description></item>
/// </list>
/// <para>
/// All standard background skill grants use the <see cref="Permanent"/> type,
/// but the enum supports future flexibility for backgrounds with different
/// grant mechanics.
/// </para>
/// </remarks>
/// <seealso cref="RuneAndRust.Domain.ValueObjects.BackgroundSkillGrant"/>
/// <seealso cref="Background"/>
public enum SkillGrantType
{
    /// <summary>
    /// Bonus added permanently to the base skill value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Permanent grants represent core professional expertise that never fades.
    /// The bonus is added to all skill checks using this skill and stacks with
    /// other bonuses from training, equipment, or abilities.
    /// </para>
    /// <para>
    /// Example: Village Smith's Craft +2 applies to all Craft checks permanently.
    /// </para>
    /// </remarks>
    Permanent = 0,

    /// <summary>
    /// Bonus applied only at character creation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// StartingBonus grants represent initial training that provides a head
    /// start but doesn't stack with later skill development. If the character
    /// later trains the skill beyond the starting bonus, the bonus is subsumed.
    /// </para>
    /// <para>
    /// Example: Starting with 2 ranks in a skill instead of 0.
    /// </para>
    /// </remarks>
    StartingBonus = 1,

    /// <summary>
    /// Unlocks skill use without the untrained penalty.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Proficiency grants remove the -2 untrained penalty for skills that
    /// normally require training. The character doesn't gain bonus ranks,
    /// but can attempt the skill without disadvantage.
    /// </para>
    /// <para>
    /// Example: A Ruin Delver can attempt Traps checks without the -2 penalty.
    /// </para>
    /// </remarks>
    Proficiency = 2
}
