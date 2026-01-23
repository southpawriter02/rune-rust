namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Classifies the type of effect a master ability provides.
/// </summary>
/// <remarks>
/// <para>
/// Master abilities are Rank 5 capstone abilities that provide powerful bonuses
/// to skilled characters. Each type has different mechanics:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="AutoSucceed"/>: Skip the check entirely below threshold DC</description></item>
///   <item><description><see cref="DiceBonus"/>: Permanent bonus dice to specific checks</description></item>
///   <item><description><see cref="DamageReduction"/>: Reduce or negate damage from specific sources</description></item>
///   <item><description><see cref="DistanceBonus"/>: Increase effective range or distance</description></item>
///   <item><description><see cref="RerollFailure"/>: Re-roll a failed check once per period</description></item>
///   <item><description><see cref="SpecialAction"/>: Unique capabilities not tied to check mechanics</description></item>
/// </list>
/// </remarks>
public enum MasterAbilityType
{
    /// <summary>
    /// Automatically succeed on checks at or below a threshold DC.
    /// </summary>
    /// <remarks>
    /// When the check's DC is at or below the ability's <c>AutoSucceedDc</c>,
    /// the check automatically succeeds with <see cref="SkillOutcome.FullSuccess"/>
    /// without rolling dice. The result includes <c>WasAutoSucceed: true</c>.
    /// </remarks>
    /// <example>
    /// Spider Climb: Auto-succeed climbing checks DC ≤ 12.
    /// Ghost Walk: Auto-succeed stealth checks DC ≤ 14.
    /// </example>
    AutoSucceed = 0,

    /// <summary>
    /// Provides a permanent bonus to the dice pool for specific check types.
    /// </summary>
    /// <remarks>
    /// The ability's <c>DiceBonus</c> is added to the dice pool during context
    /// building. This stacks with other modifiers.
    /// </remarks>
    /// <example>
    /// Fearsome Reputation: +2d10 to all intimidation checks.
    /// Glitch Reader: Reduces [Glitched] DC penalty by 2 (effectively +2d10 equivalent).
    /// </example>
    DiceBonus = 1,

    /// <summary>
    /// Reduces or negates damage from specific sources.
    /// </summary>
    /// <remarks>
    /// The ability specifies a damage threshold and reduction amount.
    /// Damage at or below the threshold is reduced by the specified amount.
    /// </remarks>
    /// <example>
    /// Cat's Grace: No damage from falls of 30ft or less.
    /// </example>
    DamageReduction = 2,

    /// <summary>
    /// Increases effective range, distance, or duration for skill effects.
    /// </summary>
    /// <remarks>
    /// The ability's <c>DistanceBonus</c> is added to distance calculations,
    /// time limits, or similar numeric bounds.
    /// </remarks>
    /// <example>
    /// Death-Defying Leap: +10ft maximum leap distance.
    /// Master Tracker: Track trails up to 48 hours old (vs normal 24 hours).
    /// </example>
    DistanceBonus = 3,

    /// <summary>
    /// Allows re-rolling a failed check once per specified period.
    /// </summary>
    /// <remarks>
    /// When a check fails, the player may invoke this ability to re-roll.
    /// The ability tracks usage per <c>RerollPeriod</c> (conversation, scene, day).
    /// </remarks>
    /// <example>
    /// Master Negotiator: Re-roll one failed negotiation check per conversation.
    /// </example>
    RerollFailure = 4,

    /// <summary>
    /// Grants unique capabilities not tied to standard check mechanics.
    /// </summary>
    /// <remarks>
    /// The ability's <c>SpecialEffect</c> string describes the unique capability.
    /// Interpretation is handled by the specific skill subsystem (e.g., Acrobatics,
    /// System Bypass).
    /// </remarks>
    /// <example>
    /// Salvage Expertise: Always recover trap components on successful disarmament.
    /// Silent Bypass: No noise generated on successful bypass.
    /// Apex Scavenger: Never find empty containers, +50% foraging yield.
    /// </example>
    SpecialAction = 5
}
