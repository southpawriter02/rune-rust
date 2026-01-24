namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the duration of a foraging search, affecting bonus dice and time investment.
/// </summary>
/// <remarks>
/// <para>
/// Longer searches provide more bonus dice but have diminishing returns per hour.
/// This creates meaningful player choice based on time pressure vs. resource needs.
/// </para>
/// <para>
/// Economy balance analysis:
/// <list type="bullet">
///   <item><description>Quick: ~48 value/hour (efficient but low chance for rare finds)</description></item>
///   <item><description>Thorough: ~20 value/hour (balanced, good cache chance)</description></item>
///   <item><description>Complete: ~15 value/hour (best for rare components, highest cache chance)</description></item>
/// </list>
/// </para>
/// <para>
/// Duration summary:
/// <list type="bullet">
///   <item><description>Quick: 10 minutes, +0 bonus dice</description></item>
///   <item><description>Thorough: 1 hour, +2 bonus dice</description></item>
///   <item><description>Complete: 4 hours, +4 bonus dice</description></item>
/// </list>
/// </para>
/// </remarks>
public enum SearchDuration
{
    /// <summary>
    /// A quick 10-minute search of the immediate area.
    /// </summary>
    /// <remarks>
    /// <para>
    /// No bonus dice, but time-efficient for common salvage.
    /// Best for: Quick scrap gathering when time is critical.
    /// </para>
    /// <para>
    /// Duration: 10 minutes.
    /// Bonus Dice: +0.
    /// Average Value/Hour: ~48.
    /// </para>
    /// </remarks>
    Quick = 0,

    /// <summary>
    /// A thorough 1-hour search of the surrounding area.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Adds +2d10 to the dice pool. Provides a balanced approach
    /// with reasonable cache discovery chances.
    /// Best for: Balanced searching when you have some time.
    /// </para>
    /// <para>
    /// Duration: 1 hour (60 minutes).
    /// Bonus Dice: +2d10.
    /// Average Value/Hour: ~20.
    /// </para>
    /// </remarks>
    Thorough = 1,

    /// <summary>
    /// A complete 4-hour search of the entire accessible area.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Adds +4d10 to the dice pool. Maximum yields but lowest
    /// efficiency per hour. Highest chance for cache discovery.
    /// Best for: Maximum yields when time is not a constraint.
    /// </para>
    /// <para>
    /// Duration: 4 hours (240 minutes).
    /// Bonus Dice: +4d10.
    /// Average Value/Hour: ~15.
    /// </para>
    /// </remarks>
    Complete = 2
}
