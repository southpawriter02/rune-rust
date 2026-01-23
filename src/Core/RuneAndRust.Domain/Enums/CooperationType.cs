namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines how multiple participants combine their efforts in a cooperative skill check.
/// </summary>
/// <remarks>
/// <para>
/// Each cooperation type represents a different mechanical approach to group skill checks:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="WeakestLink"/>: Party stealth where one failure fails all</description></item>
///   <item><description><see cref="BestAttempt"/>: Multiple attempts where best result wins</description></item>
///   <item><description><see cref="Combined"/>: Physical tasks where effort literally adds</description></item>
///   <item><description><see cref="Assisted"/>: Primary roller gets bonus from helpers</description></item>
/// </list>
/// </remarks>
public enum CooperationType
{
    /// <summary>
    /// The participant with the lowest dice pool makes the check.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used when the group can only succeed if everyone succeeds. The weakest
    /// member determines the outcome because they're most likely to fail.
    /// </para>
    /// <para>
    /// Common uses:
    /// <list type="bullet">
    ///   <item><description>Party stealth (one spotted = all spotted)</description></item>
    ///   <item><description>Group climbing (one falls = rope pulls others)</description></item>
    ///   <item><description>Synchronized actions (one mistake ruins timing)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    WeakestLink = 0,

    /// <summary>
    /// Each participant rolls independently; the best result is used.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used when only one participant needs to succeed for the group to benefit.
    /// All participants roll, and the highest net successes determines the outcome.
    /// </para>
    /// <para>
    /// Common uses:
    /// <list type="bullet">
    ///   <item><description>Searching a room (anyone can find the clue)</description></item>
    ///   <item><description>Spotting ambush (one alert person saves all)</description></item>
    ///   <item><description>Recalling lore (anyone might remember)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    BestAttempt = 1,

    /// <summary>
    /// All participants roll; their net successes are summed together.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used when effort literally combines, such as physical tasks where more
    /// people means more force. The total net successes from all participants
    /// is compared against the DC.
    /// </para>
    /// <para>
    /// Common uses:
    /// <list type="bullet">
    ///   <item><description>Lifting heavy objects (more people = more force)</description></item>
    ///   <item><description>Group foraging (more searchers = more yield)</description></item>
    ///   <item><description>Pushing/pulling obstacles (combined strength)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Combined = 2,

    /// <summary>
    /// One primary participant rolls with bonus dice from helpers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used when one person does the actual work while others provide support.
    /// Each helper rolls independently; helpers who achieve 2+ net successes
    /// grant +1d10 to the primary's pool.
    /// </para>
    /// <para>
    /// Common uses:
    /// <list type="bullet">
    ///   <item><description>Lockpicking with lookout (picker + spotters)</description></item>
    ///   <item><description>Surgery with assistants (surgeon + nurses)</description></item>
    ///   <item><description>Hacking with distraction (hacker + decoys)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Assisted = 3
}
