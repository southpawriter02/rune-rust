// ═══════════════════════════════════════════════════════════════════════════════
// DiceStatisticsDto.cs
// Data transfer object for displaying dice roll statistics in the history panel.
// Version: 0.13.4d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for dice roll statistics display.
/// </summary>
/// <remarks>
/// <para>This DTO provides formatted statistics data for the statistics section
/// of the dice history panel, including:</para>
/// <list type="bullet">
///   <item><description>Total roll count</description></item>
///   <item><description>Natural 20/1 counts and percentages</description></item>
///   <item><description>Average roll and deviation from expected</description></item>
/// </list>
/// </remarks>
/// <param name="TotalRolls">Total number of rolls recorded.</param>
/// <param name="NaturalTwentyCount">Number of natural 20 rolls.</param>
/// <param name="NaturalTwentyPercentage">Percentage of rolls that were natural 20.</param>
/// <param name="NaturalOneCount">Number of natural 1 rolls.</param>
/// <param name="NaturalOnePercentage">Percentage of rolls that were natural 1.</param>
/// <param name="AverageRoll">Calculated average roll value.</param>
/// <param name="ExpectedAverage">Expected average for this die type.</param>
/// <param name="Deviation">Deviation from expected average.</param>
/// <param name="DeviationPercentage">Deviation as a percentage.</param>
public record DiceStatisticsDto(
    int TotalRolls,
    int NaturalTwentyCount,
    float NaturalTwentyPercentage,
    int NaturalOneCount,
    float NaturalOnePercentage,
    float AverageRoll,
    float ExpectedAverage,
    float Deviation,
    float DeviationPercentage)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Display Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the natural 20 count with percentage as a formatted string.
    /// </summary>
    /// <remarks>
    /// <para>Example: "47 (9.0%)"</para>
    /// </remarks>
    public string NaturalTwentyDisplay => $"{NaturalTwentyCount} ({NaturalTwentyPercentage:F1}%)";

    /// <summary>
    /// Gets the natural 1 count with percentage as a formatted string.
    /// </summary>
    /// <remarks>
    /// <para>Example: "31 (5.9%)"</para>
    /// </remarks>
    public string NaturalOneDisplay => $"{NaturalOneCount} ({NaturalOnePercentage:F1}%)";

    /// <summary>
    /// Gets the average roll as a formatted string.
    /// </summary>
    /// <remarks>
    /// <para>Example: "11.2"</para>
    /// </remarks>
    public string AverageRollDisplay => $"{AverageRoll:F1}";

    /// <summary>
    /// Gets the deviation percentage formatted with sign.
    /// </summary>
    /// <remarks>
    /// <para>Examples: "+6.7%", "-3.5%"</para>
    /// </remarks>
    public string DeviationDisplay => Deviation >= 0
        ? $"+{DeviationPercentage:F1}%"
        : $"{DeviationPercentage:F1}%";

    /// <summary>
    /// Gets whether statistics are above expected average.
    /// </summary>
    public bool IsAboveExpected => Deviation > 0;

    /// <summary>
    /// Gets whether statistics are below expected average.
    /// </summary>
    public bool IsBelowExpected => Deviation < 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // Static Factory
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets an empty statistics DTO representing no roll history.
    /// </summary>
    public static DiceStatisticsDto Empty => new(
        TotalRolls: 0,
        NaturalTwentyCount: 0,
        NaturalTwentyPercentage: 0f,
        NaturalOneCount: 0,
        NaturalOnePercentage: 0f,
        AverageRoll: 10.5f,
        ExpectedAverage: 10.5f,
        Deviation: 0f,
        DeviationPercentage: 0f);
}
