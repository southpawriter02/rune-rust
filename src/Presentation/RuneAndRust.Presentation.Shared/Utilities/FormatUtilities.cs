// ═══════════════════════════════════════════════════════════════════════════════
// FormatUtilities.cs
// Shared formatting utilities for TUI and GUI presentation layers.
// Version: 0.13.5e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Shared.Utilities;

/// <summary>
/// Provides static utility methods for formatting values consistently across
/// TUI and GUI presentation layers.
/// </summary>
/// <remarks>
/// <para>
/// This utility class centralizes common formatting operations that were previously
/// duplicated across multiple TUI and GUI components. By consolidating these methods,
/// we ensure consistent output formatting throughout the application.
/// </para>
/// <para>Formatting categories include:</para>
/// <list type="bullet">
///   <item><description>Percentage formatting for health bars, progress indicators</description></item>
///   <item><description>Duration formatting for status effects, cooldowns</description></item>
///   <item><description>Number formatting with thousands separators and compact notation</description></item>
///   <item><description>Delta formatting with sign prefixes for stat changes</description></item>
///   <item><description>Text formatting for truncation and centering</description></item>
/// </list>
/// <para>
/// All methods are static and thread-safe, with no internal state.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Format a health bar percentage
/// var healthDisplay = FormatUtilities.FormatPercentage(75, 100); // "75%"
/// 
/// // Format a buff duration
/// var duration = FormatUtilities.FormatDuration(TimeSpan.FromMinutes(2.5)); // "2m 30s"
/// 
/// // Format a damage delta
/// var delta = FormatUtilities.FormatDelta(-15); // "-15"
/// </code>
/// </example>
public static class FormatUtilities
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PERCENTAGE FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a current/max value pair as a percentage string.
    /// </summary>
    /// <param name="current">The current value (numerator).</param>
    /// <param name="max">The maximum value (denominator).</param>
    /// <param name="format">
    /// The format string for the percentage. Defaults to "P0" (whole number percentage).
    /// Use "P1" for one decimal place, "P2" for two decimal places, etc.
    /// </param>
    /// <returns>
    /// A formatted percentage string (e.g., "75%"). Returns "0%" if max is zero or negative.
    /// Values are clamped to the 0-100% range.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method handles edge cases gracefully:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>If max ≤ 0, returns "0%" to prevent division by zero</description></item>
    ///   <item><description>If current exceeds max, the percentage is clamped to 100%</description></item>
    ///   <item><description>If current is negative, the percentage is clamped to 0%</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// FormatUtilities.FormatPercentage(50, 100);    // "50%"
    /// FormatUtilities.FormatPercentage(0, 100);     // "0%"
    /// FormatUtilities.FormatPercentage(100, 100);   // "100%"
    /// FormatUtilities.FormatPercentage(150, 100);   // "100%" (clamped)
    /// FormatUtilities.FormatPercentage(50, 0);      // "0%" (max is zero)
    /// </code>
    /// </example>
    public static string FormatPercentage(int current, int max, string format = "P0")
    {
        // Handle edge case: zero or negative max
        if (max <= 0)
        {
            return "0%";
        }

        // Calculate percentage and clamp to valid range
        var percentage = Math.Clamp((double)current / max, 0, 1);
        return percentage.ToString(format);
    }

    /// <summary>
    /// Formats a percentage value (0.0-1.0) as a string.
    /// </summary>
    /// <param name="percentage">
    /// The percentage as a decimal value (0.0 to 1.0, where 1.0 = 100%).
    /// </param>
    /// <param name="format">
    /// The format string for the percentage. Defaults to "P0" (whole number percentage).
    /// </param>
    /// <returns>
    /// A formatted percentage string (e.g., "75%"). Values are clamped to 0-100%.
    /// </returns>
    /// <example>
    /// <code>
    /// FormatUtilities.FormatPercentage(0.75);   // "75%"
    /// FormatUtilities.FormatPercentage(0.0);    // "0%"
    /// FormatUtilities.FormatPercentage(1.5);    // "100%" (clamped)
    /// </code>
    /// </example>
    public static string FormatPercentage(double percentage, string format = "P0")
    {
        var clamped = Math.Clamp(percentage, 0, 1);
        return clamped.ToString(format);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DURATION FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats a <see cref="TimeSpan"/> as a human-readable duration string.
    /// </summary>
    /// <param name="duration">The duration to format.</param>
    /// <param name="compact">
    /// If <c>true</c> (default), uses compact format ("45s", "2m 30s").
    /// If <c>false</c>, uses verbose format ("45 seconds", "2 minutes 30 seconds").
    /// </param>
    /// <returns>
    /// A formatted duration string appropriate for the magnitude of the duration.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The format automatically adapts to the duration magnitude:
    /// </para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>Duration Range</term>
    ///     <description>Compact Format</description>
    ///   </listheader>
    ///   <item>
    ///     <term>&lt;1 second</term>
    ///     <description>"&lt;1s"</description>
    ///   </item>
    ///   <item>
    ///     <term>1-59 seconds</term>
    ///     <description>"45s"</description>
    ///   </item>
    ///   <item>
    ///     <term>1-59 minutes</term>
    ///     <description>"2m 30s" or "5m"</description>
    ///   </item>
    ///   <item>
    ///     <term>1+ hours</term>
    ///     <description>"1h 30m" or "2h"</description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// FormatUtilities.FormatDuration(TimeSpan.FromSeconds(45));       // "45s"
    /// FormatUtilities.FormatDuration(TimeSpan.FromMinutes(2.5));      // "2m 30s"
    /// FormatUtilities.FormatDuration(TimeSpan.FromHours(1.5));        // "1h 30m"
    /// FormatUtilities.FormatDuration(TimeSpan.FromSeconds(45), false); // "45 seconds"
    /// </code>
    /// </example>
    public static string FormatDuration(TimeSpan duration, bool compact = true)
    {
        // Handle sub-second durations
        if (duration.TotalSeconds < 1)
        {
            return compact ? "<1s" : "less than 1 second";
        }

        // Seconds only (< 1 minute)
        if (duration.TotalSeconds < 60)
        {
            return compact
                ? $"{duration.Seconds}s"
                : $"{duration.Seconds} second{(duration.Seconds != 1 ? "s" : "")}";
        }

        // Minutes and seconds (< 1 hour)
        if (duration.TotalMinutes < 60)
        {
            var mins = (int)duration.TotalMinutes;
            var secs = duration.Seconds;

            if (secs == 0)
            {
                return compact
                    ? $"{mins}m"
                    : $"{mins} minute{(mins != 1 ? "s" : "")}";
            }

            return compact
                ? $"{mins}m {secs}s"
                : $"{mins} minute{(mins != 1 ? "s" : "")} {secs} second{(secs != 1 ? "s" : "")}";
        }

        // Hours and minutes (1+ hours)
        var hours = (int)duration.TotalHours;
        var minutes = duration.Minutes;

        if (minutes == 0)
        {
            return compact
                ? $"{hours}h"
                : $"{hours} hour{(hours != 1 ? "s" : "")}";
        }

        return compact
            ? $"{hours}h {minutes}m"
            : $"{hours} hour{(hours != 1 ? "s" : "")} {minutes} minute{(minutes != 1 ? "s" : "")}";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NUMBER FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats an integer with thousands separators.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>
    /// A formatted string with thousands separators (e.g., "1,234,567").
    /// Uses the current culture's number format.
    /// </returns>
    /// <example>
    /// <code>
    /// FormatUtilities.FormatNumber(1234567);  // "1,234,567"
    /// FormatUtilities.FormatNumber(42);       // "42"
    /// FormatUtilities.FormatNumber(-1000);    // "-1,000"
    /// </code>
    /// </example>
    public static string FormatNumber(int value)
    {
        return value.ToString("N0");
    }

    /// <summary>
    /// Formats a large number in compact notation (K, M, B suffixes).
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>
    /// A compact formatted string:
    /// <list type="bullet">
    ///   <item><description>Values ≥ 1 billion: "1.2B"</description></item>
    ///   <item><description>Values ≥ 1 million: "3.4M"</description></item>
    ///   <item><description>Values ≥ 1 thousand: "5.6K"</description></item>
    ///   <item><description>Values &lt; 1 thousand: exact value</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// FormatUtilities.FormatCompactNumber(1_234_567_890);  // "1.2B"
    /// FormatUtilities.FormatCompactNumber(1_234_567);      // "1.2M"
    /// FormatUtilities.FormatCompactNumber(1_234);          // "1.2K"
    /// FormatUtilities.FormatCompactNumber(123);            // "123"
    /// </code>
    /// </example>
    public static string FormatCompactNumber(long value)
    {
        return value switch
        {
            >= 1_000_000_000 => $"{value / 1_000_000_000.0:F1}B",
            >= 1_000_000 => $"{value / 1_000_000.0:F1}M",
            >= 1_000 => $"{value / 1_000.0:F1}K",
            _ => value.ToString()
        };
    }

    /// <summary>
    /// Formats a delta (change) value with a sign prefix.
    /// </summary>
    /// <param name="oldValue">The previous value.</param>
    /// <param name="newValue">The new value.</param>
    /// <returns>
    /// A formatted delta string with sign prefix (e.g., "+10", "-5", "+0").
    /// Positive changes include a "+" prefix; negative changes show the "-" naturally.
    /// </returns>
    /// <example>
    /// <code>
    /// FormatUtilities.FormatDelta(50, 60);   // "+10"
    /// FormatUtilities.FormatDelta(60, 50);   // "-10"
    /// FormatUtilities.FormatDelta(50, 50);   // "+0"
    /// </code>
    /// </example>
    public static string FormatDelta(int oldValue, int newValue)
    {
        var delta = newValue - oldValue;
        return FormatDelta(delta);
    }

    /// <summary>
    /// Formats a delta value with a sign prefix.
    /// </summary>
    /// <param name="delta">The delta (change) value.</param>
    /// <returns>
    /// A formatted delta string with sign prefix (e.g., "+10", "-5", "+0").
    /// Positive values include a "+" prefix; negative values show the "-" naturally.
    /// </returns>
    /// <example>
    /// <code>
    /// FormatUtilities.FormatDelta(10);   // "+10"
    /// FormatUtilities.FormatDelta(-10);  // "-10"
    /// FormatUtilities.FormatDelta(0);    // "+0"
    /// </code>
    /// </example>
    public static string FormatDelta(int delta)
    {
        return delta >= 0 ? $"+{delta}" : delta.ToString();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TEXT FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Truncates text to a maximum length, appending an ellipsis if truncated.
    /// </summary>
    /// <param name="text">The text to truncate.</param>
    /// <param name="maxLength">
    /// The maximum length of the output string, including ellipsis.
    /// Must be greater than 0.
    /// </param>
    /// <param name="ellipsis">
    /// The ellipsis string to append when truncating. Defaults to "...".
    /// </param>
    /// <returns>
    /// The original text if it fits within maxLength, or a truncated version
    /// with the ellipsis appended. Returns empty string for null/empty input.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The method handles edge cases:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Null or empty text returns empty string</description></item>
    ///   <item><description>Text shorter than maxLength is returned unchanged</description></item>
    ///   <item><description>Text exactly at maxLength is returned unchanged</description></item>
    ///   <item><description>If maxLength is less than or equal to ellipsis length, 
    ///         returns truncated ellipsis</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// FormatUtilities.TruncateText("Hello World", 8);        // "Hello..."
    /// FormatUtilities.TruncateText("Hello", 10);             // "Hello"
    /// FormatUtilities.TruncateText("Hello World", 5, "…");   // "Hell…"
    /// FormatUtilities.TruncateText("", 10);                  // ""
    /// </code>
    /// </example>
    public static string TruncateText(string text, int maxLength, string ellipsis = "...")
    {
        // Handle null or empty input
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        // No truncation needed if text fits
        if (text.Length <= maxLength)
        {
            return text;
        }

        // Handle edge case: maxLength too small for ellipsis
        if (maxLength <= ellipsis.Length)
        {
            return ellipsis[..maxLength];
        }

        // Truncate and append ellipsis
        return text[..(maxLength - ellipsis.Length)] + ellipsis;
    }

    /// <summary>
    /// Centers text within a specified width by adding padding on both sides.
    /// </summary>
    /// <param name="text">The text to center.</param>
    /// <param name="width">The total width of the output string.</param>
    /// <returns>
    /// The text centered within the specified width. If the text is longer than
    /// the width, returns the original text unchanged. For odd padding, the extra
    /// space is added to the right.
    /// </returns>
    /// <example>
    /// <code>
    /// FormatUtilities.CenterText("Hi", 10);      // "    Hi    "
    /// FormatUtilities.CenterText("Hello", 10);   // "  Hello   "
    /// FormatUtilities.CenterText("Too Long", 5); // "Too Long" (unchanged)
    /// </code>
    /// </example>
    public static string CenterText(string text, int width)
    {
        // Handle null or empty input
        if (string.IsNullOrEmpty(text))
        {
            return new string(' ', width);
        }

        // No centering needed if text already fills or exceeds width
        if (text.Length >= width)
        {
            return text;
        }

        // Calculate left and right padding
        var totalPadding = width - text.Length;
        var leftPadding = totalPadding / 2;

        // PadLeft adds left padding, PadRight adds right padding
        return text.PadLeft(leftPadding + text.Length).PadRight(width);
    }
}
