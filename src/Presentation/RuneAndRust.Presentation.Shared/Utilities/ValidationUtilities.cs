// ═══════════════════════════════════════════════════════════════════════════════
// ValidationUtilities.cs
// Shared validation utilities for TUI and GUI presentation layers.
// Version: 0.13.5e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Shared.Utilities;

/// <summary>
/// Provides static utility methods for value validation and clamping used
/// consistently across TUI and GUI presentation layers.
/// </summary>
/// <remarks>
/// <para>
/// This utility class centralizes validation operations that were previously
/// duplicated across multiple UI components. By consolidating these methods,
/// we ensure consistent validation behavior throughout the application.
/// </para>
/// <para>Validation categories include:</para>
/// <list type="bullet">
///   <item><description>Percentage validation (0-1 range checking)</description></item>
///   <item><description>Range validation and clamping</description></item>
///   <item><description>Value normalization (to 0-1 range)</description></item>
///   <item><description>Grid dimension and position validation</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Validate percentage
/// bool isValid = ValidationUtilities.ValidatePercentage(0.75); // true
/// 
/// // Clamp value to range
/// int clamped = ValidationUtilities.ClampValue(150, 0, 100); // 100
/// 
/// // Normalize value to 0-1
/// double normalized = ValidationUtilities.NormalizeValue(50, 0, 100); // 0.5
/// </code>
/// </example>
public static class ValidationUtilities
{
    // ═══════════════════════════════════════════════════════════════════════════
    // VALUE VALIDATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates that a percentage value is between 0 and 1 (inclusive).
    /// </summary>
    /// <param name="percentage">The percentage value to validate (0.0 to 1.0).</param>
    /// <returns>
    /// <c>true</c> if 0 ≤ percentage ≤ 1; otherwise, <c>false</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// ValidationUtilities.ValidatePercentage(0.5);   // true
    /// ValidationUtilities.ValidatePercentage(1.0);   // true
    /// ValidationUtilities.ValidatePercentage(-0.1);  // false
    /// ValidationUtilities.ValidatePercentage(1.5);   // false
    /// </code>
    /// </example>
    public static bool ValidatePercentage(double percentage)
    {
        return percentage >= 0 && percentage <= 1;
    }

    /// <summary>
    /// Validates that current ≤ max and both values are non-negative.
    /// </summary>
    /// <param name="current">The current value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>
    /// <c>true</c> if current ≥ 0 AND max ≥ 0 AND current ≤ max; otherwise, <c>false</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// ValidationUtilities.ValidateCurrentMax(50, 100);  // true
    /// ValidationUtilities.ValidateCurrentMax(100, 100); // true
    /// ValidationUtilities.ValidateCurrentMax(150, 100); // false
    /// ValidationUtilities.ValidateCurrentMax(-1, 100);  // false
    /// </code>
    /// </example>
    public static bool ValidateCurrentMax(int current, int max)
    {
        return current >= 0 && max >= 0 && current <= max;
    }

    /// <summary>
    /// Checks if an integer value is within a specified range (inclusive).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <returns>
    /// <c>true</c> if min ≤ value ≤ max; otherwise, <c>false</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// ValidationUtilities.IsInRange(5, 0, 10);  // true
    /// ValidationUtilities.IsInRange(0, 0, 10);  // true
    /// ValidationUtilities.IsInRange(11, 0, 10); // false
    /// </code>
    /// </example>
    public static bool IsInRange(int value, int min, int max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Checks if a double value is within a specified range (inclusive).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <returns>
    /// <c>true</c> if min ≤ value ≤ max; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsInRange(double value, double min, double max)
    {
        return value >= min && value <= max;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALUE CLAMPING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Clamps an integer value to be within the specified range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>
    /// The original value if within range, min if below, or max if above.
    /// </returns>
    /// <example>
    /// <code>
    /// ValidationUtilities.ClampValue(50, 0, 100);   // 50 (unchanged)
    /// ValidationUtilities.ClampValue(-10, 0, 100);  // 0 (clamped to min)
    /// ValidationUtilities.ClampValue(150, 0, 100);  // 100 (clamped to max)
    /// </code>
    /// </example>
    public static int ClampValue(int value, int min, int max)
    {
        return Math.Clamp(value, min, max);
    }

    /// <summary>
    /// Clamps a double value to be within the specified range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>
    /// The original value if within range, min if below, or max if above.
    /// </returns>
    public static double ClampValue(double value, double min, double max)
    {
        return Math.Clamp(value, min, max);
    }

    /// <summary>
    /// Clamps a percentage to the valid [0, 1] range.
    /// </summary>
    /// <param name="percentage">The percentage to clamp.</param>
    /// <returns>
    /// The percentage clamped to [0, 1].
    /// </returns>
    /// <example>
    /// <code>
    /// ValidationUtilities.ClampPercentage(0.5);   // 0.5
    /// ValidationUtilities.ClampPercentage(-0.1);  // 0.0
    /// ValidationUtilities.ClampPercentage(1.5);   // 1.0
    /// </code>
    /// </example>
    public static double ClampPercentage(double percentage)
    {
        return Math.Clamp(percentage, 0, 1);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VALUE NORMALIZATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Normalizes an integer value to a 0-1 range.
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <param name="min">The minimum of the input range.</param>
    /// <param name="max">The maximum of the input range.</param>
    /// <returns>
    /// A normalized value in the [0, 1] range. Returns 0 if min ≥ max (invalid range).
    /// </returns>
    /// <remarks>
    /// The result is clamped to [0, 1] to handle out-of-range input values.
    /// </remarks>
    /// <example>
    /// <code>
    /// ValidationUtilities.NormalizeValue(50, 0, 100);   // 0.5
    /// ValidationUtilities.NormalizeValue(0, 0, 100);    // 0.0
    /// ValidationUtilities.NormalizeValue(100, 0, 100);  // 1.0
    /// ValidationUtilities.NormalizeValue(150, 0, 100);  // 1.0 (clamped)
    /// ValidationUtilities.NormalizeValue(50, 100, 100); // 0.0 (invalid range)
    /// </code>
    /// </example>
    public static double NormalizeValue(int value, int min, int max)
    {
        // Handle edge case: zero or negative range
        if (max <= min)
        {
            return 0;
        }

        return Math.Clamp((double)(value - min) / (max - min), 0, 1);
    }

    /// <summary>
    /// Normalizes a double value to a 0-1 range.
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <param name="min">The minimum of the input range.</param>
    /// <param name="max">The maximum of the input range.</param>
    /// <returns>
    /// A normalized value in the [0, 1] range. Returns 0 if min ≥ max (invalid range).
    /// </returns>
    public static double NormalizeValue(double value, double min, double max)
    {
        // Handle edge case: zero or negative range
        if (max <= min)
        {
            return 0;
        }

        return Math.Clamp((value - min) / (max - min), 0, 1);
    }

    /// <summary>
    /// Denormalizes a 0-1 value back to a specified integer range.
    /// </summary>
    /// <param name="normalized">The normalized value (0.0 to 1.0).</param>
    /// <param name="min">The minimum of the target range.</param>
    /// <param name="max">The maximum of the target range.</param>
    /// <returns>
    /// An integer in the [min, max] range. The normalized value is clamped to [0, 1]
    /// before conversion, and the result is rounded to the nearest integer.
    /// </returns>
    /// <example>
    /// <code>
    /// ValidationUtilities.DenormalizeValue(0.5, 0, 100);  // 50
    /// ValidationUtilities.DenormalizeValue(0.0, 0, 100);  // 0
    /// ValidationUtilities.DenormalizeValue(1.0, 0, 100);  // 100
    /// ValidationUtilities.DenormalizeValue(0.33, 50, 80); // 60 (rounded)
    /// </code>
    /// </example>
    public static int DenormalizeValue(double normalized, int min, int max)
    {
        var clamped = Math.Clamp(normalized, 0, 1);
        return (int)Math.Round(min + clamped * (max - min));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GRID VALIDATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates that grid dimensions are positive.
    /// </summary>
    /// <param name="width">The grid width to validate.</param>
    /// <param name="height">The grid height to validate.</param>
    /// <returns>
    /// <c>true</c> if width &gt; 0 AND height &gt; 0; otherwise, <c>false</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// ValidationUtilities.ValidateGridDimensions(8, 8);   // true
    /// ValidationUtilities.ValidateGridDimensions(0, 8);   // false
    /// ValidationUtilities.ValidateGridDimensions(-1, 8);  // false
    /// </code>
    /// </example>
    public static bool ValidateGridDimensions(int width, int height)
    {
        return width > 0 && height > 0;
    }

    /// <summary>
    /// Validates that a grid position is within bounds.
    /// </summary>
    /// <param name="x">The X coordinate to validate.</param>
    /// <param name="y">The Y coordinate to validate.</param>
    /// <param name="width">The grid width.</param>
    /// <param name="height">The grid height.</param>
    /// <returns>
    /// <c>true</c> if 0 ≤ x &lt; width AND 0 ≤ y &lt; height; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This is equivalent to <see cref="GridUtilities.IsValidPosition(int, int, int, int)"/>
    /// but provided here for convenience when not using the full GridUtilities.
    /// </remarks>
    public static bool ValidateGridPosition(int x, int y, int width, int height)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}
