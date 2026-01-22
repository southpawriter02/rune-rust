namespace RuneAndRust.Domain.Constants;

/// <summary>
/// Constants for the extended check system.
/// </summary>
/// <remarks>
/// These constants define the core mechanics for extended (multi-round) skill checks,
/// including fumble penalties and catastrophic failure thresholds.
/// </remarks>
public static class ExtendedCheckConstants
{
    /// <summary>
    /// Number of accumulated successes lost per fumble.
    /// </summary>
    /// <remarks>
    /// When a fumble occurs during an extended check, the character loses 
    /// this many accumulated successes (minimum 0 - cannot go negative).
    /// </remarks>
    public const int FumblePenalty = 2;

    /// <summary>
    /// Number of consecutive fumbles required to trigger catastrophic failure.
    /// </summary>
    /// <remarks>
    /// Three fumbles in a row ends the extended check with catastrophic failure,
    /// regardless of accumulated successes or remaining rounds. This represents
    /// a disastrous sequence of errors that makes the task impossible to complete.
    /// </remarks>
    public const int CatastrophicFumbleThreshold = 3;

    /// <summary>
    /// Default maximum rounds for an extended check if not specified.
    /// </summary>
    /// <remarks>
    /// Most extended checks allow 10 rounds to complete the task.
    /// Specific contexts (e.g., time pressure, combat) may override this.
    /// </remarks>
    public const int DefaultMaxRounds = 10;

    /// <summary>
    /// Minimum target successes for an extended check.
    /// </summary>
    /// <remarks>
    /// An extended check must require at least 1 success to complete.
    /// This prevents degenerate checks that auto-succeed.
    /// </remarks>
    public const int MinimumTargetSuccesses = 1;

    /// <summary>
    /// Minimum rounds for an extended check.
    /// </summary>
    /// <remarks>
    /// An extended check must allow at least 1 round for the character to attempt it.
    /// </remarks>
    public const int MinimumRounds = 1;
}
