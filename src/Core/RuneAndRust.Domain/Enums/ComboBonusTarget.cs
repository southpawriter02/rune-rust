namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Specifies the target(s) for combo bonus effects.
/// </summary>
/// <remarks>
/// <para>Combo bonus targets determine who receives the bonus effect when a combo completes:</para>
/// <list type="bullet">
///   <item><description><see cref="LastTarget"/> - Apply to the target of the final ability</description></item>
///   <item><description><see cref="AllHitTargets"/> - Apply to all targets hit during the combo</description></item>
///   <item><description><see cref="Self"/> - Apply to the caster</description></item>
///   <item><description><see cref="Area"/> - Apply to an area around the final target</description></item>
/// </list>
/// <para>
/// This allows bonus effects to be precisely directed - damage bonuses typically target the enemy,
/// while healing effects target the caster.
/// </para>
/// </remarks>
public enum ComboBonusTarget
{
    /// <summary>
    /// Apply the bonus effect to the target of the final ability in the combo.
    /// </summary>
    /// <remarks>
    /// <para>This is the default target for most offensive bonus effects.</para>
    /// <para>Ideal for focused damage or status effects on a single enemy.</para>
    /// <para>Example: Elemental Burst's damage multiplier applies to the Lightning target.</para>
    /// </remarks>
    LastTarget,

    /// <summary>
    /// Apply the bonus effect to all targets hit during the entire combo sequence.
    /// </summary>
    /// <remarks>
    /// <para>Spreads effects across all enemies damaged during combo execution.</para>
    /// <para>Useful for cleave combos or spreading status effects.</para>
    /// <para>Example: A "Plague Strike" combo might apply poison to every enemy hit.</para>
    /// </remarks>
    AllHitTargets,

    /// <summary>
    /// Apply the bonus effect to the caster.
    /// </summary>
    /// <remarks>
    /// <para>Used for healing, buffs, or cooldown resets that benefit the player.</para>
    /// <para>Appropriate for sustain effects that reward combo execution.</para>
    /// <para>Example: Divine Judgment's heal effect targets the paladin (self).</para>
    /// </remarks>
    Self,

    /// <summary>
    /// Apply the bonus effect to an area around the final target.
    /// </summary>
    /// <remarks>
    /// <para>Expands single-target effects into area-of-effect damage or status.</para>
    /// <para>Radius is specified in <see cref="Definitions.ComboBonusEffect.Value"/>.</para>
    /// <para>Example: Divine Judgment expands to a 2-cell radius around the smite target.</para>
    /// </remarks>
    Area
}
