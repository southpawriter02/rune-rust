namespace RuneAndRust.Presentation.Shared.ValueObjects;

/// <summary>
/// Represents a platform-agnostic color with RGB values.
/// </summary>
/// <remarks>
/// <para>ThemeColor provides a unified color representation that can be
/// converted to platform-specific formats by theme adapters:</para>
/// <list type="bullet">
///   <item><description>TUI: <see cref="System.ConsoleColor"/> or Spectre.Console.Color</description></item>
///   <item><description>GUI: Avalonia.Media.Color or ISolidColorBrush</description></item>
/// </list>
/// <para>Colors can be created from hex strings or RGB byte values.</para>
/// </remarks>
/// <param name="R">Red component (0-255).</param>
/// <param name="G">Green component (0-255).</param>
/// <param name="B">Blue component (0-255).</param>
/// <param name="Name">Optional descriptive name for the color.</param>
/// <example>
/// <code>
/// // Create from hex string
/// var forestGreen = ThemeColor.FromHex("#228B22", "Forest Green");
/// 
/// // Create from RGB values
/// var customColor = new ThemeColor(255, 128, 0, "Custom Orange");
/// 
/// // Get hex representation
/// Console.WriteLine(customColor.Hex); // "#FF8000"
/// </code>
/// </example>
public readonly record struct ThemeColor(
    byte R,
    byte G,
    byte B,
    string? Name = null)
{
    /// <summary>
    /// Gets the hexadecimal representation of the color.
    /// </summary>
    /// <example>#FF5500</example>
    public string Hex => $"#{R:X2}{G:X2}{B:X2}";

    /// <summary>
    /// Creates a ThemeColor from a hex string.
    /// </summary>
    /// <param name="hex">The hex color string (e.g., "#FF5500" or "FF5500").</param>
    /// <param name="name">Optional descriptive name for the color.</param>
    /// <returns>A new ThemeColor with the specified RGB values.</returns>
    /// <exception cref="ArgumentException">Thrown when hex string is invalid.</exception>
    /// <example>
    /// <code>
    /// var color = ThemeColor.FromHex("#228B22", "Forest Green");
    /// </code>
    /// </example>
    public static ThemeColor FromHex(string hex, string? name = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hex);

        // Remove leading # if present
        hex = hex.TrimStart('#');

        if (hex.Length != 6)
        {
            throw new ArgumentException(
                $"Hex color must be 6 characters (got {hex.Length}): {hex}",
                nameof(hex));
        }

        var r = Convert.ToByte(hex[0..2], 16);
        var g = Convert.ToByte(hex[2..4], 16);
        var b = Convert.ToByte(hex[4..6], 16);

        return new ThemeColor(r, g, b, name);
    }

    /// <summary>
    /// Returns a display string for debugging and logging.
    /// </summary>
    /// <returns>Color name with hex value, or just hex if no name.</returns>
    public override string ToString() =>
        Name is not null ? $"{Name} ({Hex})" : Hex;

    // ═══════════════════════════════════════════════════════════════════════════
    // Common Colors (for convenience)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Black color (0, 0, 0).</summary>
    public static ThemeColor Black => new(0, 0, 0, "Black");

    /// <summary>White color (255, 255, 255).</summary>
    public static ThemeColor White => new(255, 255, 255, "White");

    /// <summary>Transparent fallback (magenta for visibility during debugging).</summary>
    public static ThemeColor Fallback => new(255, 0, 255, "Fallback Magenta");
}
