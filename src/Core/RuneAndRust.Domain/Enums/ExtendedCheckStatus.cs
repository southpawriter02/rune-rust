namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the current status of an extended (multi-round) skill check.
/// </summary>
/// <remarks>
/// <para>
/// Extended checks accumulate successes over multiple rounds toward a target threshold.
/// The check ends when the target is reached, time runs out, or a catastrophic failure occurs.
/// </para>
/// <para>
/// Status transitions:
/// <list type="bullet">
///   <item><description>InProgress → Succeeded: accumulated ≥ target</description></item>
///   <item><description>InProgress → Failed: rounds exhausted</description></item>
///   <item><description>InProgress → CatastrophicFailure: 3 consecutive fumbles</description></item>
///   <item><description>InProgress → Abandoned: player chose to stop</description></item>
/// </list>
/// </para>
/// </remarks>
public enum ExtendedCheckStatus
{
    /// <summary>
    /// The extended check is still in progress.
    /// </summary>
    /// <remarks>
    /// The character has not yet accumulated enough successes,
    /// and there are rounds remaining.
    /// </remarks>
    InProgress = 0,

    /// <summary>
    /// The extended check succeeded.
    /// </summary>
    /// <remarks>
    /// The accumulated successes met or exceeded the target threshold
    /// within the allowed number of rounds.
    /// </remarks>
    Succeeded = 1,

    /// <summary>
    /// The extended check failed due to running out of time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All available rounds were used without reaching the target threshold.
    /// </para>
    /// <para>
    /// Regular failure may allow retry with fresh state, depending on context.
    /// </para>
    /// </remarks>
    Failed = 2,

    /// <summary>
    /// The extended check ended in catastrophic failure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Three consecutive fumbles occurred, triggering a disaster.
    /// </para>
    /// <para>
    /// Catastrophic failure typically has severe consequences:
    /// <list type="bullet">
    ///   <item><description>Lock picking: lock jams permanently</description></item>
    ///   <item><description>Terminal hacking: lockout + alarm triggered</description></item>
    ///   <item><description>Tracking: quarry escapes permanently</description></item>
    ///   <item><description>Climbing: character falls from current height</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    CatastrophicFailure = 3,

    /// <summary>
    /// The extended check was abandoned by the player.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The player chose to stop the extended check before completion.
    /// </para>
    /// <para>
    /// Abandoning may have consequences depending on context:
    /// <list type="bullet">
    ///   <item><description>Stealth infiltration: may trigger alert on retreat</description></item>
    ///   <item><description>Terminal hacking: may leave trace of intrusion</description></item>
    ///   <item><description>Negotiation: may damage reputation</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Abandoned = 4
}
