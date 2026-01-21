// ═══════════════════════════════════════════════════════════════════════════════
// ProgressBarRenderer.cs
// Renders progress bars for achievement progress visualization.
// Version: 0.13.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Renders progress bars for achievement progress visualization.
/// </summary>
/// <remarks>
/// <para>
/// Progress bars use '#' for filled portions and '.' for empty portions,
/// providing a clear visual representation of completion status:
/// </para>
/// <list type="bullet">
///   <item><description>Complete: <c>[########################################]</c></description></item>
///   <item><description>Half: <c>[####################....................]</c></description></item>
///   <item><description>Empty: <c>[........................................]</c></description></item>
/// </list>
/// <para>
/// The renderer also provides percentage formatting and color coding based
/// on completion level.
/// </para>
/// </remarks>
public class ProgressBarRenderer
{
    /// <summary>
    /// The character used for filled portions of the progress bar.
    /// </summary>
    private const char FilledChar = '#';

    /// <summary>
    /// The character used for empty portions of the progress bar.
    /// </summary>
    private const char EmptyChar = '.';

    /// <summary>
    /// Renders a progress bar string.
    /// </summary>
    /// <param name="current">The current progress value.</param>
    /// <param name="target">The target progress value.</param>
    /// <param name="width">The width of the bar in characters (excluding brackets).</param>
    /// <returns>
    /// A formatted progress bar string like <c>[##########..........]</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// If <paramref name="target"/> is zero or negative, returns an empty progress bar.
    /// Progress is capped at 100% (current values exceeding target still show full bar).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var renderer = new ProgressBarRenderer();
    /// var bar = renderer.RenderProgressBar(47, 100, 40);
    /// // Returns: "[##################......................]"
    /// </code>
    /// </example>
    public string RenderProgressBar(int current, int target, int width)
    {
        // Handle invalid target values
        if (target <= 0)
        {
            return $"[{new string(EmptyChar, width)}]";
        }

        // Calculate the percentage, capped at 100%
        var percentage = Math.Min((double)current / target, 1.0);

        // Calculate filled and empty character counts
        var filledWidth = (int)(width * percentage);
        var emptyWidth = width - filledWidth;

        // Build the progress bar string
        var filled = new string(FilledChar, filledWidth);
        var empty = new string(EmptyChar, emptyWidth);

        return $"[{filled}{empty}]";
    }

    /// <summary>
    /// Formats the completion percentage for display.
    /// </summary>
    /// <param name="current">The current progress value.</param>
    /// <param name="target">The target progress value.</param>
    /// <returns>
    /// A formatted percentage string like <c>47%</c>.
    /// Returns <c>0%</c> if target is zero or negative.
    /// </returns>
    /// <example>
    /// <code>
    /// var renderer = new ProgressBarRenderer();
    /// var pct = renderer.FormatPercentage(47, 100);
    /// // Returns: "47%"
    /// </code>
    /// </example>
    public string FormatPercentage(int current, int target)
    {
        if (target <= 0)
        {
            return "0%";
        }

        // Calculate percentage, capped at 100%
        var percentage = Math.Min((current * 100) / target, 100);
        return $"{percentage}%";
    }

    /// <summary>
    /// Gets the display color for a progress bar based on completion percentage.
    /// </summary>
    /// <param name="percentage">
    /// The completion percentage as a decimal (0.0 to 1.0).
    /// </param>
    /// <returns>
    /// A <see cref="ConsoleColor"/> appropriate for the progress level:
    /// <list type="bullet">
    ///   <item><description>Green - 100% complete</description></item>
    ///   <item><description>Cyan - 75-99% complete</description></item>
    ///   <item><description>Yellow - 50-74% complete</description></item>
    ///   <item><description>DarkYellow - 25-49% complete</description></item>
    ///   <item><description>Red - 0-24% complete</description></item>
    /// </list>
    /// </returns>
    public ConsoleColor GetProgressColor(float percentage)
    {
        return percentage switch
        {
            >= 1.0f => ConsoleColor.Green,
            >= 0.75f => ConsoleColor.Cyan,
            >= 0.5f => ConsoleColor.Yellow,
            >= 0.25f => ConsoleColor.DarkYellow,
            _ => ConsoleColor.Red
        };
    }

    /// <summary>
    /// Renders a progress bar with current/target display for incremental achievements.
    /// </summary>
    /// <param name="current">The current progress value.</param>
    /// <param name="target">The target progress value.</param>
    /// <param name="width">The width of the bar in characters.</param>
    /// <returns>
    /// A formatted string with progress bar and count like 
    /// <c>[####################....................] 47/100 (47%)</c>.
    /// </returns>
    public string RenderProgressBarWithCount(int current, int target, int width)
    {
        var bar = RenderProgressBar(current, target, width);
        var pct = FormatPercentage(current, target);

        // For single-value achievements (target = 1), just show percentage
        if (target <= 1)
        {
            return $"{bar} {pct}";
        }

        // For incremental achievements, show count and percentage
        return $"{bar} {current}/{target} ({pct})";
    }
}
