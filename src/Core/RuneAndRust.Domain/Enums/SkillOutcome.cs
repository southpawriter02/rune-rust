namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the degree of success or failure for a skill check using success-counting mechanics.
/// </summary>
/// <remarks>
/// <para>
/// Provides 6 tiers of outcomes for nuanced narrative and mechanical effects:
/// <list type="bullet">
///   <item><description>CriticalFailure: Fumble - catastrophic failure with consequences</description></item>
///   <item><description>Failure: Did not meet the difficulty class</description></item>
///   <item><description>MarginalSuccess: Barely succeeded - met DC exactly</description></item>
///   <item><description>FullSuccess: Clear success - exceeded DC by 1-2</description></item>
///   <item><description>ExceptionalSuccess: Outstanding - exceeded DC by 3-4</description></item>
///   <item><description>CriticalSuccess: Masterful - exceeded DC by 5+</description></item>
/// </list>
/// </para>
/// <para>
/// Outcome is determined by:
/// <list type="bullet">
///   <item><description>Fumble detection (0 successes AND ≥1 botch) → CriticalFailure</description></item>
///   <item><description>Margin calculation (NetSuccesses - DC) → other outcomes</description></item>
/// </list>
/// </para>
/// </remarks>
public enum SkillOutcome
{
    /// <summary>
    /// Fumble - catastrophic failure with additional consequences.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Triggered when the dice roll is a fumble (0 successes AND ≥1 botch).
    /// This outcome occurs regardless of difficulty class.
    /// </para>
    /// <para>
    /// Examples of consequences:
    /// <list type="bullet">
    ///   <item><description>Lockpicking: Pick breaks, alerting guards</description></item>
    ///   <item><description>Climbing: Fall and take damage</description></item>
    ///   <item><description>Persuasion: Target becomes hostile</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    CriticalFailure = 0,

    /// <summary>
    /// Failure - did not meet the difficulty class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Net successes were less than the difficulty class (margin &lt; 0).
    /// The attempt fails but without additional negative consequences.
    /// </para>
    /// <para>
    /// May allow retry in some circumstances.
    /// </para>
    /// </remarks>
    Failure = 1,

    /// <summary>
    /// Marginal Success - barely succeeded.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Net successes exactly matched the difficulty class (margin = 0).
    /// The task is accomplished but with complications or delays.
    /// </para>
    /// <para>
    /// Examples of complications:
    /// <list type="bullet">
    ///   <item><description>Lockpicking: Success but took extra time</description></item>
    ///   <item><description>Climbing: Made it but exhausted</description></item>
    ///   <item><description>Persuasion: Agreed but reluctantly</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    MarginalSuccess = 2,

    /// <summary>
    /// Full Success - clear success.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Net successes exceeded the difficulty class by 1-2 (margin 1-2).
    /// The task is accomplished cleanly as expected.
    /// </para>
    /// <para>
    /// This is the "normal" successful outcome.
    /// </para>
    /// </remarks>
    FullSuccess = 3,

    /// <summary>
    /// Exceptional Success - outstanding performance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Net successes exceeded the difficulty class by 3-4 (margin 3-4).
    /// The task is accomplished with notable excellence.
    /// </para>
    /// <para>
    /// May provide minor bonus effects:
    /// <list type="bullet">
    ///   <item><description>Lockpicking: Discovered hidden compartment</description></item>
    ///   <item><description>Climbing: Found faster route for others</description></item>
    ///   <item><description>Persuasion: Gained additional favor</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    ExceptionalSuccess = 4,

    /// <summary>
    /// Critical Success - masterful achievement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Net successes exceeded the difficulty class by 5+ (margin ≥ 5).
    /// The task is accomplished with extraordinary skill.
    /// </para>
    /// <para>
    /// Provides significant bonus effects:
    /// <list type="bullet">
    ///   <item><description>Lockpicking: Disabled alarm, found secret</description></item>
    ///   <item><description>Climbing: Reached destination instantly</description></item>
    ///   <item><description>Persuasion: Target becomes loyal ally</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    CriticalSuccess = 5
}
