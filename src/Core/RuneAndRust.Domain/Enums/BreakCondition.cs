// ═══════════════════════════════════════════════════════════════════════════════
// BreakCondition.cs
// Enumerates the conditions under which a Shadow Snare can be broken or ended.
// Version: 0.20.4c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the conditions under which a <see cref="MyrkgengrAbilityId.ShadowSnare"/>
/// effect is broken or ended.
/// </summary>
/// <remarks>
/// <para>
/// A Shadow Snare roots a target in place for its duration. The snare can
/// end naturally (duration expires) or be broken early through saves,
/// caster movement, dispelling, or caster incapacitation.
/// </para>
/// </remarks>
/// <seealso cref="MyrkgengrAbilityId"/>
public enum BreakCondition
{
    /// <summary>
    /// The target successfully rolled against the snare's save DC.
    /// </summary>
    SaveSucceeded = 1,

    /// <summary>
    /// The caster moved away, breaking concentration on the snare.
    /// </summary>
    CasterMoved = 2,

    /// <summary>
    /// The snare's duration expired naturally.
    /// </summary>
    DurationExpired = 3,

    /// <summary>
    /// The snare was removed by a dispel or purge effect.
    /// </summary>
    Dispelled = 4,

    /// <summary>
    /// The caster was incapacitated, ending all maintained effects.
    /// </summary>
    CasterIncapacitated = 5
}
