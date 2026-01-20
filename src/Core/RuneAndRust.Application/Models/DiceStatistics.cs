// ═══════════════════════════════════════════════════════════════════════════════
// DiceStatistics.cs
// Aggregated dice roll statistics record for display in the statistics view.
// Version: 0.12.0b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Models;

/// <summary>
/// Aggregated dice roll statistics for display in the statistics view.
/// </summary>
/// <remarks>
/// <para>This record provides a complete snapshot of all dice-related statistics:</para>
/// <list type="bullet">
///   <item><description>Total roll counts and natural 20/1 counts</description></item>
///   <item><description>Actual vs expected averages and rates</description></item>
///   <item><description>Luck percentage and rating</description></item>
///   <item><description>Current and longest streak information</description></item>
/// </list>
/// <para>
/// This record is created by the DiceHistoryService and consumed by the
/// statistics view (v0.12.0c) for rendering to the player.
/// </para>
/// </remarks>
/// <param name="TotalRolls">Total number of d20 rolls recorded.</param>
/// <param name="TotalNat20s">Total natural 20s rolled.</param>
/// <param name="TotalNat1s">Total natural 1s rolled.</param>
/// <param name="AverageD20">Actual average d20 result.</param>
/// <param name="ExpectedD20">Expected d20 average (10.5).</param>
/// <param name="Nat20Rate">Actual natural 20 rate (0.0 to 1.0).</param>
/// <param name="ExpectedNat20Rate">Expected natural 20 rate (0.05).</param>
/// <param name="Nat1Rate">Actual natural 1 rate (0.0 to 1.0).</param>
/// <param name="ExpectedNat1Rate">Expected natural 1 rate (0.05).</param>
/// <param name="LuckPercentage">Deviation from expected average as percentage.</param>
/// <param name="Rating">Overall luck rating based on luck percentage.</param>
/// <param name="CurrentStreak">Current lucky (+) or unlucky (-) streak count.</param>
/// <param name="LongestLuckyStreak">Longest recorded consecutive lucky streak.</param>
/// <param name="LongestUnluckyStreak">Longest recorded consecutive unlucky streak.</param>
public record DiceStatistics(
    int TotalRolls,
    int TotalNat20s,
    int TotalNat1s,
    double AverageD20,
    double ExpectedD20,
    double Nat20Rate,
    double ExpectedNat20Rate,
    double Nat1Rate,
    double ExpectedNat1Rate,
    double LuckPercentage,
    LuckRating Rating,
    int CurrentStreak,
    int LongestLuckyStreak,
    int LongestUnluckyStreak)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Display Helper Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the natural 20 rate as a formatted percentage string.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Example: "5.00%" for a rate of 0.05.
    /// </para>
    /// </remarks>
    public string Nat20RateDisplay => $"{Nat20Rate * 100:F2}%";

    /// <summary>
    /// Gets the natural 1 rate as a formatted percentage string.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Example: "5.00%" for a rate of 0.05.
    /// </para>
    /// </remarks>
    public string Nat1RateDisplay => $"{Nat1Rate * 100:F2}%";

    /// <summary>
    /// Gets the luck percentage as a formatted string with sign.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Examples: "+6.67%", "-3.50%", "0.00%"
    /// </para>
    /// </remarks>
    public string LuckPercentageDisplay => LuckPercentage >= 0
        ? $"+{LuckPercentage:F2}%"
        : $"{LuckPercentage:F2}%";

    /// <summary>
    /// Gets the average d20 result as a formatted string.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Example: "11.23" for an average of 11.234.
    /// </para>
    /// </remarks>
    public string AverageD20Display => $"{AverageD20:F2}";

    /// <summary>
    /// Gets the current streak as a formatted string with direction indicator.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Examples: "🔥 +5" for lucky streak, "❄️ -3" for unlucky streak, "— 0" for no streak.
    /// </para>
    /// </remarks>
    public string CurrentStreakDisplay => CurrentStreak switch
    {
        > 0 => $"🔥 +{CurrentStreak}",
        < 0 => $"❄️ {CurrentStreak}",
        _ => "— 0"
    };

    /// <summary>
    /// Gets whether the player has any d20 roll history.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns false for new players who haven't made any d20 rolls yet.
    /// This can be used to show placeholder text in the statistics view.
    /// </para>
    /// </remarks>
    public bool HasRollHistory => TotalRolls > 0;

    /// <summary>
    /// Gets whether the player is currently on a lucky streak.
    /// </summary>
    public bool IsOnLuckyStreak => CurrentStreak > 0;

    /// <summary>
    /// Gets whether the player is currently on an unlucky streak.
    /// </summary>
    public bool IsOnUnluckyStreak => CurrentStreak < 0;

    /// <summary>
    /// Gets the absolute value of the current streak (for display purposes).
    /// </summary>
    public int CurrentStreakLength => Math.Abs(CurrentStreak);

    // ═══════════════════════════════════════════════════════════════════════════
    // Static Factory
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets an empty DiceStatistics instance representing no roll history.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This static property provides default values for players who have
    /// not yet made any dice rolls. The expected values (10.5 average, 5% rates)
    /// are used as placeholders.
    /// </para>
    /// </remarks>
    public static DiceStatistics Empty => new(
        TotalRolls: 0,
        TotalNat20s: 0,
        TotalNat1s: 0,
        AverageD20: 10.5,
        ExpectedD20: 10.5,
        Nat20Rate: 0.05,
        ExpectedNat20Rate: 0.05,
        Nat1Rate: 0.05,
        ExpectedNat1Rate: 0.05,
        LuckPercentage: 0,
        Rating: LuckRating.Average,
        CurrentStreak: 0,
        LongestLuckyStreak: 0,
        LongestUnluckyStreak: 0);
}
