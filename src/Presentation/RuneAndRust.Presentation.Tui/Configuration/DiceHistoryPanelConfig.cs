// ═══════════════════════════════════════════════════════════════════════════════
// DiceHistoryPanelConfig.cs
// Configuration for the dice history panel display settings.
// Version: 0.13.4d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration for the dice history panel display settings.
/// </summary>
/// <remarks>
/// <para>This configuration controls all visual aspects of the dice history panel:</para>
/// <list type="bullet">
///   <item><description>Panel dimensions and layout</description></item>
///   <item><description>Luck rating thresholds</description></item>
///   <item><description>Color settings for various elements</description></item>
///   <item><description>Bar chart formatting characters</description></item>
/// </list>
/// <para>
/// Configuration values can be loaded from <c>config/dice-history-panel.json</c>
/// or use default values for immediate usage.
/// </para>
/// </remarks>
public class DiceHistoryPanelConfig
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Panel Dimensions
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the total width of the panel in characters.
    /// </summary>
    /// <remarks>
    /// <para>Default: 72 characters</para>
    /// </remarks>
    public int PanelWidth { get; set; } = 72;

    /// <summary>
    /// Gets or sets the number of recent rolls to display.
    /// </summary>
    /// <remarks>
    /// <para>Default: 20 rolls</para>
    /// </remarks>
    public int RecentRollDisplayCount { get; set; } = 20;

    /// <summary>
    /// Gets or sets the width of distribution chart bars in characters.
    /// </summary>
    /// <remarks>
    /// <para>Default: 20 characters (matches d20 values)</para>
    /// </remarks>
    public int DistributionBarWidth { get; set; } = 20;

    // ═══════════════════════════════════════════════════════════════════════════
    // Luck Rating Thresholds
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the threshold for "very lucky" luck rating (percentage).
    /// </summary>
    /// <remarks>
    /// <para>Default: 10.0% - Luck deviation >= 10% shows "VERY LUCKY"</para>
    /// </remarks>
    public float VeryLuckyThreshold { get; set; } = 10.0f;

    /// <summary>
    /// Gets or sets the threshold for "lucky" luck rating (percentage).
    /// </summary>
    /// <remarks>
    /// <para>Default: 5.0% - Luck deviation >= 5% shows "LUCKY"</para>
    /// </remarks>
    public float LuckyThreshold { get; set; } = 5.0f;

    /// <summary>
    /// Gets or sets the threshold for "unlucky" luck rating (percentage).
    /// </summary>
    /// <remarks>
    /// <para>Default: -5.0% - Luck deviation is -5% or below shows "UNLUCKY"</para>
    /// </remarks>
    public float UnluckyThreshold { get; set; } = -5.0f;

    /// <summary>
    /// Gets or sets the threshold for "very unlucky" luck rating (percentage).
    /// </summary>
    /// <remarks>
    /// <para>Default: -10.0% - Luck deviation is -10% or below shows "VERY UNLUCKY"</para>
    /// </remarks>
    public float VeryUnluckyThreshold { get; set; } = -10.0f;

    // ═══════════════════════════════════════════════════════════════════════════
    // Critical Roll Colors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the color for natural 20 (critical success) display.
    /// </summary>
    /// <remarks>
    /// <para>Default: Yellow - Gold color for critical success</para>
    /// </remarks>
    public ConsoleColor CriticalSuccessColor { get; set; } = ConsoleColor.Yellow;

    /// <summary>
    /// Gets or sets the color for natural 1 (critical failure) display.
    /// </summary>
    /// <remarks>
    /// <para>Default: Red - Danger color for critical failure</para>
    /// </remarks>
    public ConsoleColor CriticalFailureColor { get; set; } = ConsoleColor.Red;

    // ═══════════════════════════════════════════════════════════════════════════
    // Streak Colors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the color for lucky streak indicator.
    /// </summary>
    /// <remarks>
    /// <para>Default: Green - Positive/lucky indicator</para>
    /// </remarks>
    public ConsoleColor LuckyStreakColor { get; set; } = ConsoleColor.Green;

    /// <summary>
    /// Gets or sets the color for unlucky streak indicator.
    /// </summary>
    /// <remarks>
    /// <para>Default: Red - Negative/unlucky indicator</para>
    /// </remarks>
    public ConsoleColor UnluckyStreakColor { get; set; } = ConsoleColor.Red;

    // ═══════════════════════════════════════════════════════════════════════════
    // Luck Rating Colors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the color for "very lucky" luck rating.
    /// </summary>
    /// <remarks>
    /// <para>Default: Green - Bright positive indicator</para>
    /// </remarks>
    public ConsoleColor VeryLuckyColor { get; set; } = ConsoleColor.Green;

    /// <summary>
    /// Gets or sets the color for "lucky" luck rating.
    /// </summary>
    /// <remarks>
    /// <para>Default: DarkGreen - Subtle positive indicator</para>
    /// </remarks>
    public ConsoleColor LuckyColor { get; set; } = ConsoleColor.DarkGreen;

    /// <summary>
    /// Gets or sets the color for "normal" luck rating.
    /// </summary>
    /// <remarks>
    /// <para>Default: White - Neutral indicator</para>
    /// </remarks>
    public ConsoleColor NormalLuckColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Gets or sets the color for "unlucky" luck rating.
    /// </summary>
    /// <remarks>
    /// <para>Default: DarkYellow - Warning indicator</para>
    /// </remarks>
    public ConsoleColor UnluckyColor { get; set; } = ConsoleColor.DarkYellow;

    /// <summary>
    /// Gets or sets the color for "very unlucky" luck rating.
    /// </summary>
    /// <remarks>
    /// <para>Default: Red - Danger indicator</para>
    /// </remarks>
    public ConsoleColor VeryUnluckyColor { get; set; } = ConsoleColor.Red;

    // ═══════════════════════════════════════════════════════════════════════════
    // Distribution Outlier Colors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the color for above-expected outliers in distribution.
    /// </summary>
    /// <remarks>
    /// <para>Default: Green - Positive outlier indicator</para>
    /// </remarks>
    public ConsoleColor AboveExpectedColor { get; set; } = ConsoleColor.Green;

    /// <summary>
    /// Gets or sets the color for below-expected outliers in distribution.
    /// </summary>
    /// <remarks>
    /// <para>Default: Red - Negative outlier indicator</para>
    /// </remarks>
    public ConsoleColor BelowExpectedColor { get; set; } = ConsoleColor.Red;

    // ═══════════════════════════════════════════════════════════════════════════
    // Bar Chart Characters
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the character for filled portion of distribution bars.
    /// </summary>
    /// <remarks>
    /// <para>Default: '#' - Standard fill character</para>
    /// </remarks>
    public char BarFilledChar { get; set; } = '#';

    /// <summary>
    /// Gets or sets the character for empty portion of distribution bars.
    /// </summary>
    /// <remarks>
    /// <para>Default: '.' - Visible empty space character</para>
    /// </remarks>
    public char BarEmptyChar { get; set; } = '.';

    // ═══════════════════════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the appropriate color for a luck deviation percentage.
    /// </summary>
    /// <param name="deviation">The luck deviation percentage.</param>
    /// <returns>The console color corresponding to the luck rating.</returns>
    public ConsoleColor GetLuckColor(float deviation)
    {
        if (deviation >= VeryLuckyThreshold)
            return VeryLuckyColor;
        if (deviation >= LuckyThreshold)
            return LuckyColor;
        if (deviation <= VeryUnluckyThreshold)
            return VeryUnluckyColor;
        if (deviation <= UnluckyThreshold)
            return UnluckyColor;
        return NormalLuckColor;
    }

    /// <summary>
    /// Gets the luck rating text for a deviation percentage.
    /// </summary>
    /// <param name="deviation">The luck deviation percentage.</param>
    /// <returns>The luck rating text (e.g., "VERY LUCKY", "LUCKY", "NORMAL").</returns>
    public string GetLuckRatingText(float deviation)
    {
        if (deviation >= VeryLuckyThreshold)
            return "VERY LUCKY";
        if (deviation >= LuckyThreshold)
            return "LUCKY";
        if (deviation <= VeryUnluckyThreshold)
            return "VERY UNLUCKY";
        if (deviation <= UnluckyThreshold)
            return "UNLUCKY";
        return "NORMAL";
    }
}
