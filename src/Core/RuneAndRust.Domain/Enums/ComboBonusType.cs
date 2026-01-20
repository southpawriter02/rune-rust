namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of bonus effects applied when a combo completes successfully.
/// </summary>
/// <remarks>
/// <para>Combo bonus effects reward players for executing ability sequences correctly:</para>
/// <list type="bullet">
///   <item><description><see cref="ExtraDamage"/> - Deal additional damage using dice notation</description></item>
///   <item><description><see cref="DamageMultiplier"/> - Multiply the final ability's damage</description></item>
///   <item><description><see cref="ApplyStatus"/> - Apply a status effect to the target</description></item>
///   <item><description><see cref="Heal"/> - Heal the caster using dice notation</description></item>
///   <item><description><see cref="ResetCooldown"/> - Reset a specific ability's cooldown</description></item>
///   <item><description><see cref="RefundResource"/> - Refund resource cost of abilities used</description></item>
///   <item><description><see cref="AreaEffect"/> - Expand the final effect to an area</description></item>
/// </list>
/// <para>
/// Each bonus type uses a different value format - see individual members for details.
/// Multiple bonuses can be applied from a single combo completion.
/// </para>
/// </remarks>
public enum ComboBonusType
{
    /// <summary>
    /// Deal additional flat damage on combo completion.
    /// </summary>
    /// <remarks>
    /// <para>Value format: Dice notation (e.g., "4d6", "2d8+4")</para>
    /// <para>Damage type specified via <see cref="Definitions.ComboBonusEffect.DamageType"/></para>
    /// <para>Example: Assassin's Dance grants +4d6 piercing damage on completion.</para>
    /// </remarks>
    ExtraDamage,

    /// <summary>
    /// Multiply the final ability's damage by a factor.
    /// </summary>
    /// <remarks>
    /// <para>Value format: Decimal multiplier (e.g., "2.0", "1.5")</para>
    /// <para>Applied to the final ability's total damage after other modifiers.</para>
    /// <para>Example: Elemental Burst multiplies final Lightning damage by 2.0.</para>
    /// </remarks>
    DamageMultiplier,

    /// <summary>
    /// Apply a status effect to the target.
    /// </summary>
    /// <remarks>
    /// <para>Value format: Empty string (status ID in <see cref="Definitions.ComboBonusEffect.StatusEffectId"/>)</para>
    /// <para>Status effects can be debuffs (stunned, bleeding) or buffs (empowered).</para>
    /// <para>Example: Warrior's Onslaught applies "stunned" and "bleeding" to the target.</para>
    /// </remarks>
    ApplyStatus,

    /// <summary>
    /// Heal the caster on combo completion.
    /// </summary>
    /// <remarks>
    /// <para>Value format: Dice notation (e.g., "2d8", "3d6+2")</para>
    /// <para>Healing is always applied to the caster regardless of target setting.</para>
    /// <para>Example: Divine Judgment heals the paladin for 2d8 HP on completion.</para>
    /// </remarks>
    Heal,

    /// <summary>
    /// Reset a specific ability's cooldown.
    /// </summary>
    /// <remarks>
    /// <para>Value format: Ability ID (e.g., "vanish", "charge")</para>
    /// <para>The specified ability's cooldown is reset to 0, making it immediately available.</para>
    /// <para>Example: Assassin's Dance resets the Vanish cooldown for continued stealth play.</para>
    /// </remarks>
    ResetCooldown,

    /// <summary>
    /// Refund resource cost of abilities used in the combo.
    /// </summary>
    /// <remarks>
    /// <para>Value format: Percentage as string (e.g., "50", "100") or flat amount</para>
    /// <para>Refunds mana, stamina, or other resources spent during the combo.</para>
    /// <para>Example: A "Sustained Assault" combo might refund 50% of stamina spent.</para>
    /// </remarks>
    RefundResource,

    /// <summary>
    /// Expand the final effect to an area.
    /// </summary>
    /// <remarks>
    /// <para>Value format: Radius in cells (e.g., "2", "3")</para>
    /// <para>Converts a single-target ability into an area effect centered on the target.</para>
    /// <para>Example: Divine Judgment expands Holy Light to a 2-cell radius around the target.</para>
    /// </remarks>
    AreaEffect
}
