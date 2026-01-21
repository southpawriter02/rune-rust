using Microsoft.Extensions.Logging;
using RuneAndRust.Presentation.Shared.Enums;
using RuneAndRust.Presentation.Shared.ValueObjects;

namespace RuneAndRust.Presentation.Shared.Utilities;

/// <summary>
/// Provides color transformation algorithms for color vision deficiency (color blindness) support.
/// </summary>
/// <remarks>
/// <para>Implements scientifically validated color transformation matrices based on the research of
/// Machado, Oliveira &amp; Fernandes (2009) for simulating and correcting color vision deficiencies.</para>
/// <para>The transformation matrices adjust RGB values to improve distinguishability for users with:</para>
/// <list type="bullet">
/// <item><description><b>Protanopia:</b> Red-blindness (~1% of males)</description></item>
/// <item><description><b>Deuteranopia:</b> Green-blindness (~6% of males, most common)</description></item>
/// <item><description><b>Tritanopia:</b> Blue-blindness (~0.01% of population)</description></item>
/// <item><description><b>Achromatopsia:</b> Complete color blindness (very rare)</description></item>
/// </list>
/// <para><b>Logging:</b> Color transformations are logged at Debug level.</para>
/// </remarks>
/// <example>
/// <code>
/// var originalRed = new ThemeColor(255, 0, 0, "Red");
/// var transformed = ColorBlindTransform.Transform(originalRed, ColorBlindMode.Protanopia);
/// // Result: Color shifted toward brown/gray range for red-blind users
/// </code>
/// </example>
public static class ColorBlindTransform
{
    // ═══════════════════════════════════════════════════════════════════════════
    // COLOR TRANSFORMATION MATRICES
    // Based on Machado, Oliveira & Fernandes (2009) research
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Protanopia (red-blind) transformation matrix.
    /// </summary>
    /// <remarks>
    /// Red appears as dark brown/black, green appears as tan.
    /// Primarily affects red-green color perception.
    /// </remarks>
    private static readonly double[,] ProtanopiaMatrix =
    {
        { 0.567, 0.433, 0.000 },
        { 0.558, 0.442, 0.000 },
        { 0.000, 0.242, 0.758 }
    };

    /// <summary>
    /// Deuteranopia (green-blind) transformation matrix.
    /// </summary>
    /// <remarks>
    /// Green appears as beige, red appears as brownish-yellow.
    /// Most common form of color blindness.
    /// </remarks>
    private static readonly double[,] DeuteranopiaMatrix =
    {
        { 0.625, 0.375, 0.000 },
        { 0.700, 0.300, 0.000 },
        { 0.000, 0.300, 0.700 }
    };

    /// <summary>
    /// Tritanopia (blue-blind) transformation matrix.
    /// </summary>
    /// <remarks>
    /// Blue appears as green, yellow appears as pink.
    /// Affects blue-yellow color perception.
    /// </remarks>
    private static readonly double[,] TritanopiaMatrix =
    {
        { 0.950, 0.050, 0.000 },
        { 0.000, 0.433, 0.567 },
        { 0.000, 0.475, 0.525 }
    };

    /// <summary>
    /// Achromatopsia (complete color blindness) transformation matrix.
    /// </summary>
    /// <remarks>
    /// Uses luminance values to convert to grayscale.
    /// Weights based on human eye sensitivity: Red 0.2126, Green 0.7152, Blue 0.0722.
    /// </remarks>
    private static readonly double[,] AchromatopsiaMatrix =
    {
        { 0.2126, 0.7152, 0.0722 },
        { 0.2126, 0.7152, 0.0722 },
        { 0.2126, 0.7152, 0.0722 }
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Transforms a color for the specified color blind mode.
    /// </summary>
    /// <param name="color">The original color to transform.</param>
    /// <param name="mode">The color blind mode to apply.</param>
    /// <param name="logger">Optional logger for debug output.</param>
    /// <returns>
    /// The transformed color appropriate for the specified color blind mode,
    /// or the original color if mode is <see cref="ColorBlindMode.None"/>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unsupported <paramref name="mode"/> is specified.
    /// </exception>
    /// <example>
    /// <code>
    /// // Transform red for protanopia
    /// var red = new ThemeColor(255, 0, 0, "Red");
    /// var result = ColorBlindTransform.Transform(red, ColorBlindMode.Protanopia);
    /// 
    /// // Transform for grayscale (achromatopsia)
    /// var blue = new ThemeColor(0, 0, 255, "Blue");
    /// var gray = ColorBlindTransform.Transform(blue, ColorBlindMode.Achromatopsia);
    /// </code>
    /// </example>
    public static ThemeColor Transform(
        ThemeColor color,
        ColorBlindMode mode,
        ILogger? logger = null)
    {
        // No transformation needed for normal vision
        if (mode == ColorBlindMode.None)
        {
            return color;
        }

        // Select the appropriate transformation matrix
        var matrix = mode switch
        {
            ColorBlindMode.Protanopia => ProtanopiaMatrix,
            ColorBlindMode.Deuteranopia => DeuteranopiaMatrix,
            ColorBlindMode.Tritanopia => TritanopiaMatrix,
            ColorBlindMode.Achromatopsia => AchromatopsiaMatrix,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported color blind mode.")
        };

        var transformed = ApplyMatrix(color, matrix, mode);

        // Log the transformation at debug level
        logger?.LogDebug(
            "Color transformed: {Original} -> {Transformed} ({Mode})",
            color.Hex,
            transformed.Hex,
            mode);

        return transformed;
    }

    /// <summary>
    /// Determines if a color blind mode requires color transformation.
    /// </summary>
    /// <param name="mode">The color blind mode to check.</param>
    /// <returns>
    /// <c>true</c> if the mode requires transformation; <c>false</c> for <see cref="ColorBlindMode.None"/>.
    /// </returns>
    public static bool RequiresTransformation(ColorBlindMode mode)
    {
        return mode != ColorBlindMode.None;
    }

    /// <summary>
    /// Gets a description of what the specified color blind mode affects.
    /// </summary>
    /// <param name="mode">The color blind mode to describe.</param>
    /// <returns>A human-readable description of the color vision deficiency.</returns>
    public static string GetModeDescription(ColorBlindMode mode)
    {
        return mode switch
        {
            ColorBlindMode.None => "Normal color vision",
            ColorBlindMode.Protanopia => "Red-blind (affects ~1% of males)",
            ColorBlindMode.Deuteranopia => "Green-blind (affects ~6% of males)",
            ColorBlindMode.Tritanopia => "Blue-blind (affects ~0.01% of population)",
            ColorBlindMode.Achromatopsia => "Complete color blindness (grayscale)",
            _ => "Unknown color blind mode"
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Applies a 3x3 transformation matrix to an RGB color.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="matrix">The 3x3 transformation matrix.</param>
    /// <param name="mode">The color blind mode (for naming the result).</param>
    /// <returns>The transformed color.</returns>
    private static ThemeColor ApplyMatrix(ThemeColor color, double[,] matrix, ColorBlindMode mode)
    {
        // Normalize RGB to 0-1 range for matrix multiplication
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;

        // Apply 3x3 matrix transformation:
        // [newR]   [m00 m01 m02]   [r]
        // [newG] = [m10 m11 m12] × [g]
        // [newB]   [m20 m21 m22]   [b]
        double newR = (matrix[0, 0] * r) + (matrix[0, 1] * g) + (matrix[0, 2] * b);
        double newG = (matrix[1, 0] * r) + (matrix[1, 1] * g) + (matrix[1, 2] * b);
        double newB = (matrix[2, 0] * r) + (matrix[2, 1] * g) + (matrix[2, 2] * b);

        // Clamp values to valid range and convert back to byte (0-255)
        byte finalR = (byte)Math.Clamp(newR * 255.0, 0.0, 255.0);
        byte finalG = (byte)Math.Clamp(newG * 255.0, 0.0, 255.0);
        byte finalB = (byte)Math.Clamp(newB * 255.0, 0.0, 255.0);

        // Create new color with "(CB)" suffix to indicate color-blind adjusted
        var newName = string.IsNullOrEmpty(color.Name)
            ? $"CB-{mode}"
            : $"{color.Name} (CB)";

        return new ThemeColor(finalR, finalG, finalB, newName);
    }
}
