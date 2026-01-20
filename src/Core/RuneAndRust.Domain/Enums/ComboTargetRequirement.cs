namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Specifies target requirements for combo steps.
/// </summary>
/// <remarks>
/// <para>Target requirements control how combo steps validate their targets relative to previous steps:</para>
/// <list type="bullet">
///   <item><description><see cref="Any"/> - No target restriction, any valid target is acceptable</description></item>
///   <item><description><see cref="SameTarget"/> - Must target the same entity as the previous step</description></item>
///   <item><description><see cref="DifferentTarget"/> - Must target a different entity than the previous step</description></item>
///   <item><description><see cref="Self"/> - Must target the caster (for buffs, self-heals, or vanish abilities)</description></item>
/// </list>
/// <para>
/// Target requirements are evaluated by <see cref="Definitions.ComboStep.Matches"/> during combo detection
/// to ensure ability sequences maintain proper targeting patterns.
/// </para>
/// </remarks>
public enum ComboTargetRequirement
{
    /// <summary>
    /// No target restriction - any valid target is acceptable.
    /// </summary>
    /// <remarks>
    /// <para>This is the default target requirement for combo steps.</para>
    /// <para>Use when the combo step can target any enemy or ally without affecting combo progress.</para>
    /// <para>Example: First step of most combos uses Any to allow flexibility in initiating the combo.</para>
    /// </remarks>
    Any,

    /// <summary>
    /// Must target the same entity as the previous step.
    /// </summary>
    /// <remarks>
    /// <para>Enforces focused damage on a single target across multiple steps.</para>
    /// <para>Common for combos that build up damage or effects on one enemy.</para>
    /// <para>Example: Elemental Burst requires Fire Bolt, Ice Shard, and Lightning all hit the same target.</para>
    /// </remarks>
    SameTarget,

    /// <summary>
    /// Must target a different entity than the previous step.
    /// </summary>
    /// <remarks>
    /// <para>Enforces spreading damage or effects across multiple targets.</para>
    /// <para>Useful for cleave combos or multi-target crowd control.</para>
    /// <para>Example: A "Whirlwind" combo might require hitting different enemies in sequence.</para>
    /// </remarks>
    DifferentTarget,

    /// <summary>
    /// Must target the caster (self-targeted ability).
    /// </summary>
    /// <remarks>
    /// <para>Used for defensive or utility abilities within a combo.</para>
    /// <para>Common for stealth abilities, self-buffs, or repositioning moves.</para>
    /// <para>Example: Assassin's Dance includes Vanish (self-targeted) between Backstab and Ambush.</para>
    /// </remarks>
    Self
}
