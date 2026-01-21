// ═══════════════════════════════════════════════════════════════════════════════
// RollDistributionDto.cs
// Data transfer object for roll distribution chart display.
// Version: 0.13.4d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for roll distribution chart display.
/// </summary>
/// <remarks>
/// <para>This DTO provides data and helper methods for rendering an ASCII bar chart
/// showing the frequency distribution of roll values from 1-20.</para>
/// <list type="bullet">
///   <item><description>Roll counts indexed by (rollValue - 1)</description></item>
///   <item><description>Percentage calculations per value</description></item>
///   <item><description>Outlier detection for values above/below expected</description></item>
/// </list>
/// </remarks>
/// <param name="RollCounts">Array of counts indexed by (rollValue - 1). Index 0 = roll 1, Index 19 = roll 20.</param>
/// <param name="TotalRolls">Total number of rolls for percentage calculation.</param>
public record RollDistributionDto(int[] RollCounts, int TotalRolls)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Computed Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the expected count per roll value for uniform distribution.
    /// </summary>
    /// <remarks>
    /// <para>For d20 with uniform distribution, each value should appear 5% of the time.</para>
    /// </remarks>
    public float ExpectedCountPerValue => TotalRolls / 20f;

    /// <summary>
    /// Gets the maximum count across all roll values (for bar scaling).
    /// </summary>
    public int MaxCount => RollCounts.Length > 0 ? RollCounts.Max() : 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // Query Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the count for a specific roll value.
    /// </summary>
    /// <param name="rollValue">The roll value (1-20).</param>
    /// <returns>The count of occurrences for this value, or 0 if invalid.</returns>
    public int GetCount(int rollValue)
    {
        if (rollValue < 1 || rollValue > 20 || RollCounts.Length < rollValue)
            return 0;
        return RollCounts[rollValue - 1];
    }

    /// <summary>
    /// Gets the percentage of total rolls for a specific value.
    /// </summary>
    /// <param name="rollValue">The roll value (1-20).</param>
    /// <returns>Percentage as a float (e.g., 5.5 for 5.5%).</returns>
    /// <remarks>
    /// <para>Returns 0 if rollValue is invalid or TotalRolls is 0.</para>
    /// </remarks>
    public float GetPercentage(int rollValue)
    {
        if (rollValue < 1 || rollValue > 20 || TotalRolls == 0)
            return 0f;
        return (RollCounts[rollValue - 1] / (float)TotalRolls) * 100f;
    }

    /// <summary>
    /// Determines if a roll value is significantly above expected frequency.
    /// </summary>
    /// <param name="rollValue">The roll value (1-20).</param>
    /// <returns>True if count exceeds 150% of expected.</returns>
    /// <remarks>
    /// <para>
    /// Outlier threshold: count > (expectedCount * 1.5)
    /// This indicates the value has appeared significantly more often than random chance.
    /// </para>
    /// </remarks>
    public bool IsAboveExpected(int rollValue)
    {
        if (rollValue < 1 || rollValue > 20 || RollCounts.Length < rollValue)
            return false;
        return RollCounts[rollValue - 1] > ExpectedCountPerValue * 1.5f;
    }

    /// <summary>
    /// Determines if a roll value is significantly below expected frequency.
    /// </summary>
    /// <param name="rollValue">The roll value (1-20).</param>
    /// <returns>True if count is below 50% of expected.</returns>
    /// <remarks>
    /// <para>
    /// Outlier threshold: count is less than (expectedCount * 0.5)
    /// This indicates the value has appeared significantly less often than random chance.
    /// </para>
    /// </remarks>
    public bool IsBelowExpected(int rollValue)
    {
        if (rollValue < 1 || rollValue > 20 || RollCounts.Length < rollValue)
            return false;
        return RollCounts[rollValue - 1] < ExpectedCountPerValue * 0.5f;
    }

    /// <summary>
    /// Determines if a roll value is an outlier (either above or below expected).
    /// </summary>
    /// <param name="rollValue">The roll value (1-20).</param>
    /// <returns>True if the value is either above or below expected.</returns>
    public bool IsOutlier(int rollValue)
    {
        return IsAboveExpected(rollValue) || IsBelowExpected(rollValue);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Static Factory
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets an empty distribution DTO representing no roll history.
    /// </summary>
    public static RollDistributionDto Empty => new(new int[20], 0);
}
