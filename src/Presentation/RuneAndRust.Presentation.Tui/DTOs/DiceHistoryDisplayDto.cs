// ═══════════════════════════════════════════════════════════════════════════════
// DiceHistoryDisplayDto.cs
// Data transfer object containing all information needed to render the dice history panel.
// Version: 0.13.4d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object containing all information needed to render
/// the dice history panel.
/// </summary>
/// <remarks>
/// <para>This DTO aggregates all dice history data from the Application layer services:</para>
/// <list type="bullet">
///   <item><description>Recent roll values for display</description></item>
///   <item><description>Roll statistics (totals, averages, criticals)</description></item>
///   <item><description>Luck rating and deviation</description></item>
///   <item><description>Streak information (current and longest)</description></item>
///   <item><description>Distribution data for bar chart visualization</description></item>
/// </list>
/// <para>
/// This is a presentation-layer DTO created by mapping domain data
/// from DiceStatistics and DiceRollRecord in the Application/Domain layers.
/// </para>
/// </remarks>
/// <param name="RecentRolls">List of recent roll values in chronological order (oldest first).</param>
/// <param name="DieType">Type of die rolled (e.g., "d20").</param>
/// <param name="TotalRolls">Total number of rolls recorded.</param>
/// <param name="NaturalTwenties">Count of natural 20 rolls.</param>
/// <param name="NaturalOnes">Count of natural 1 rolls.</param>
/// <param name="AverageRoll">Calculated average roll value.</param>
/// <param name="ExpectedAverage">Expected average for this die type (10.5 for d20).</param>
/// <param name="LuckDeviation">Percentage deviation from expected average.</param>
/// <param name="CurrentStreak">Current streak count (positive = lucky, negative = unlucky).</param>
/// <param name="LongestLuckyStreak">Longest recorded lucky streak.</param>
/// <param name="LongestUnluckyStreak">Longest recorded unlucky streak.</param>
/// <param name="Distribution">Array of counts for each roll value (index 0 = roll 1, index 19 = roll 20).</param>
public record DiceHistoryDisplayDto(
    IReadOnlyList<int> RecentRolls,
    string DieType,
    int TotalRolls,
    int NaturalTwenties,
    int NaturalOnes,
    float AverageRoll,
    float ExpectedAverage,
    float LuckDeviation,
    int CurrentStreak,
    int LongestLuckyStreak,
    int LongestUnluckyStreak,
    int[] Distribution)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Computed Display Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the natural 20 percentage of total rolls.
    /// </summary>
    /// <remarks>
    /// <para>Example: 9.0 for 47 nat 20s out of 523 rolls.</para>
    /// </remarks>
    public float NaturalTwentyPercentage => TotalRolls > 0
        ? (NaturalTwenties / (float)TotalRolls) * 100f
        : 0f;

    /// <summary>
    /// Gets the natural 1 percentage of total rolls.
    /// </summary>
    /// <remarks>
    /// <para>Example: 5.9 for 31 nat 1s out of 523 rolls.</para>
    /// </remarks>
    public float NaturalOnePercentage => TotalRolls > 0
        ? (NaturalOnes / (float)TotalRolls) * 100f
        : 0f;

    /// <summary>
    /// Gets whether there is any roll history to display.
    /// </summary>
    /// <remarks>
    /// <para>Returns false for new players who haven't made any rolls yet.</para>
    /// </remarks>
    public bool HasHistory => TotalRolls > 0;

    /// <summary>
    /// Gets whether the current streak is lucky (above average).
    /// </summary>
    public bool IsLuckyStreak => CurrentStreak > 0;

    /// <summary>
    /// Gets whether the current streak is unlucky (below average).
    /// </summary>
    public bool IsUnluckyStreak => CurrentStreak < 0;

    /// <summary>
    /// Gets the luck deviation formatted with sign and percentage.
    /// </summary>
    /// <remarks>
    /// <para>Examples: "+6.7%", "-3.5%", "0.0%"</para>
    /// </remarks>
    public string LuckDeviationDisplay => LuckDeviation >= 0
        ? $"+{LuckDeviation:F1}%"
        : $"{LuckDeviation:F1}%";

    // ═══════════════════════════════════════════════════════════════════════════
    // Static Factory
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets an empty display DTO representing no roll history.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This static property provides default values for players who have
    /// not yet made any dice rolls.
    /// </para>
    /// </remarks>
    public static DiceHistoryDisplayDto Empty => new(
        RecentRolls: Array.Empty<int>(),
        DieType: "d20",
        TotalRolls: 0,
        NaturalTwenties: 0,
        NaturalOnes: 0,
        AverageRoll: 10.5f,
        ExpectedAverage: 10.5f,
        LuckDeviation: 0f,
        CurrentStreak: 0,
        LongestLuckyStreak: 0,
        LongestUnluckyStreak: 0,
        Distribution: new int[20]);
}
