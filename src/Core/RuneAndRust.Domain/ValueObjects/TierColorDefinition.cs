namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines color representation for a quality tier across different rendering contexts.
/// </summary>
/// <remarks>
/// <para>
/// This value object provides color data in multiple formats:
/// - HexColor: For GUI and web rendering ("#RRGGBB" format)
/// - ConsoleColor: For TUI terminal rendering (System.ConsoleColor)
/// - RGB: Individual byte values for custom color mixing
/// </para>
/// <para>
/// Colors are loaded from configuration to allow theme customization.
/// </para>
/// </remarks>
/// <param name="HexColor">Hex color code in "#RRGGBB" format.</param>
/// <param name="ConsoleColor">Closest matching System.ConsoleColor for terminal display.</param>
/// <param name="RgbRed">Red component (0-255).</param>
/// <param name="RgbGreen">Green component (0-255).</param>
/// <param name="RgbBlue">Blue component (0-255).</param>
public readonly record struct TierColorDefinition(
    string HexColor,
    ConsoleColor ConsoleColor,
    byte RgbRed,
    byte RgbGreen,
    byte RgbBlue)
{
    /// <summary>
    /// Gets the color as a tuple for easy destructuring.
    /// </summary>
    public (byte R, byte G, byte B) RgbTuple => (RgbRed, RgbGreen, RgbBlue);

    /// <summary>
    /// Gets the hex color without the leading '#'.
    /// </summary>
    public string HexColorRaw => HexColor.TrimStart('#');

    /// <summary>
    /// Creates a TierColorDefinition from a hex color string.
    /// </summary>
    /// <param name="hexColor">Hex color in "#RRGGBB" format.</param>
    /// <param name="consoleColor">Closest matching ConsoleColor.</param>
    /// <returns>A new TierColorDefinition instance.</returns>
    /// <exception cref="ArgumentException">Thrown when hexColor is null, empty, or invalid format.</exception>
    public static TierColorDefinition FromHex(string hexColor, ConsoleColor consoleColor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hexColor);

        var hex = hexColor.TrimStart('#');
        if (hex.Length != 6)
        {
            throw new ArgumentException(
                $"Invalid hex color format: '{hexColor}'. Expected '#RRGGBB' format.",
                nameof(hexColor));
        }

        // Validate hex characters
        if (!IsValidHexString(hex))
        {
            throw new ArgumentException(
                $"Invalid hex color format: '{hexColor}'. Contains non-hexadecimal characters.",
                nameof(hexColor));
        }

        var red = Convert.ToByte(hex[..2], 16);
        var green = Convert.ToByte(hex[2..4], 16);
        var blue = Convert.ToByte(hex[4..6], 16);

        return new TierColorDefinition(
            hexColor.StartsWith('#') ? hexColor : $"#{hexColor}",
            consoleColor,
            red,
            green,
            blue);
    }

    /// <summary>
    /// Creates a TierColorDefinition from RGB components.
    /// </summary>
    /// <param name="red">Red component (0-255).</param>
    /// <param name="green">Green component (0-255).</param>
    /// <param name="blue">Blue component (0-255).</param>
    /// <param name="consoleColor">Closest matching ConsoleColor.</param>
    /// <returns>A new TierColorDefinition instance.</returns>
    public static TierColorDefinition FromRgb(byte red, byte green, byte blue, ConsoleColor consoleColor)
    {
        var hexColor = $"#{red:X2}{green:X2}{blue:X2}";
        return new TierColorDefinition(hexColor, consoleColor, red, green, blue);
    }

    // Standard tier color definitions

    /// <summary>
    /// Gray color for Jury-Rigged tier.
    /// </summary>
    public static TierColorDefinition JuryRiggedDefault =>
        FromHex("#808080", ConsoleColor.Gray);

    /// <summary>
    /// White color for Scavenged tier.
    /// </summary>
    public static TierColorDefinition ScavengedDefault =>
        FromHex("#FFFFFF", ConsoleColor.White);

    /// <summary>
    /// Green color for Clan-Forged tier.
    /// </summary>
    public static TierColorDefinition ClanForgedDefault =>
        FromHex("#00FF00", ConsoleColor.Green);

    /// <summary>
    /// Purple color for Optimized tier.
    /// </summary>
    public static TierColorDefinition OptimizedDefault =>
        FromHex("#800080", ConsoleColor.Magenta);

    /// <summary>
    /// Gold color for Myth-Forged tier.
    /// </summary>
    public static TierColorDefinition MythForgedDefault =>
        FromHex("#FFD700", ConsoleColor.Yellow);

    /// <summary>
    /// Creates a display string for debug/logging purposes.
    /// </summary>
    public override string ToString() => $"{HexColor} ({ConsoleColor})";

    /// <summary>
    /// Validates that a string contains only valid hexadecimal characters.
    /// </summary>
    /// <param name="hex">The string to validate (without '#' prefix).</param>
    /// <returns>True if all characters are valid hexadecimal digits.</returns>
    private static bool IsValidHexString(string hex)
    {
        foreach (var c in hex)
        {
            if (!char.IsAsciiHexDigit(c))
            {
                return false;
            }
        }
        return true;
    }
}
