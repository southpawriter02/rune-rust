using RuneAndRust.Presentation.Shared.Enums;

namespace RuneAndRust.Presentation.Shared.ValueObjects;

/// <summary>
/// Contains the complete color palette for the game theme.
/// </summary>
/// <remarks>
/// <para>ColorPalette is the single source of truth for all colors used in the game.
/// Colors are organized by category and accessed via <see cref="ColorKey"/>.</para>
/// <para>The default palette provides a dark fantasy aesthetic with:</para>
/// <list type="bullet">
///   <item><description>Dark backgrounds (#1A1A2E, #16213E)</description></item>
///   <item><description>Vibrant health/resource bars</description></item>
///   <item><description>Distinct entity colors for gameplay clarity</description></item>
/// </list>
/// </remarks>
public class ColorPalette
{
    private readonly Dictionary<ColorKey, ThemeColor> _colors;

    /// <summary>
    /// Initializes a new empty ColorPalette.
    /// </summary>
    /// <remarks>Use <see cref="CreateDefault"/> for the standard game palette.</remarks>
    private ColorPalette()
    {
        _colors = new Dictionary<ColorKey, ThemeColor>();
    }

    /// <summary>
    /// Initializes a ColorPalette from an existing dictionary.
    /// </summary>
    /// <param name="colors">The color dictionary to use.</param>
    private ColorPalette(Dictionary<ColorKey, ThemeColor> colors)
    {
        _colors = colors;
    }

    /// <summary>
    /// Creates the default game color palette.
    /// </summary>
    /// <returns>A new ColorPalette with the standard game colors.</returns>
    /// <remarks>
    /// <para>The default palette includes all <see cref="ColorKey"/> values with
    /// carefully selected colors for the dark fantasy theme.</para>
    /// <para>Color values are based on common web color names for readability.</para>
    /// </remarks>
    public static ColorPalette CreateDefault()
    {
        var colors = new Dictionary<ColorKey, ThemeColor>
        {
            // ═══════════════════════════════════════════════════════════════════
            // Core UI Colors
            // ═══════════════════════════════════════════════════════════════════
            [ColorKey.Primary] = ThemeColor.FromHex("#4A90D9", "Primary Blue"),
            [ColorKey.Secondary] = ThemeColor.FromHex("#2ECC71", "Secondary Green"),
            [ColorKey.Accent] = ThemeColor.FromHex("#E74C3C", "Accent Red"),
            [ColorKey.Warning] = ThemeColor.FromHex("#F39C12", "Warning Orange"),
            [ColorKey.Error] = ThemeColor.FromHex("#E74C3C", "Error Red"),
            [ColorKey.Success] = ThemeColor.FromHex("#2ECC71", "Success Green"),
            [ColorKey.Background] = ThemeColor.FromHex("#1A1A2E", "Background"),
            [ColorKey.Surface] = ThemeColor.FromHex("#16213E", "Surface"),
            [ColorKey.Text] = ThemeColor.FromHex("#EAEAEA", "Text"),
            [ColorKey.Muted] = ThemeColor.FromHex("#7F8C8D", "Muted"),

            // ═══════════════════════════════════════════════════════════════════
            // Health Colors
            // ═══════════════════════════════════════════════════════════════════
            [ColorKey.HealthFull] = ThemeColor.FromHex("#228B22", "Forest Green"),
            [ColorKey.HealthGood] = ThemeColor.FromHex("#32CD32", "Lime Green"),
            [ColorKey.HealthLow] = ThemeColor.FromHex("#FFD700", "Gold"),
            [ColorKey.HealthCritical] = ThemeColor.FromHex("#DC143C", "Crimson"),

            // ═══════════════════════════════════════════════════════════════════
            // Resource Colors
            // ═══════════════════════════════════════════════════════════════════
            [ColorKey.Mana] = ThemeColor.FromHex("#4169E1", "Royal Blue"),
            [ColorKey.Rage] = ThemeColor.FromHex("#DC143C", "Crimson"),
            [ColorKey.Energy] = ThemeColor.FromHex("#FFD700", "Gold"),
            [ColorKey.Focus] = ThemeColor.FromHex("#00CED1", "Dark Turquoise"),
            [ColorKey.Stamina] = ThemeColor.FromHex("#32CD32", "Lime Green"),

            // ═══════════════════════════════════════════════════════════════════
            // Status Effect Colors
            // ═══════════════════════════════════════════════════════════════════
            [ColorKey.Buff] = ThemeColor.FromHex("#32CD32", "Lime Green"),
            [ColorKey.Debuff] = ThemeColor.FromHex("#DC143C", "Crimson"),
            [ColorKey.Fire] = ThemeColor.FromHex("#FF4500", "Orange Red"),
            [ColorKey.Ice] = ThemeColor.FromHex("#00BFFF", "Deep Sky Blue"),
            [ColorKey.Poison] = ThemeColor.FromHex("#9400D3", "Dark Violet"),
            [ColorKey.Lightning] = ThemeColor.FromHex("#FFD700", "Gold"),
            [ColorKey.Holy] = ThemeColor.FromHex("#FFFACD", "Lemon Chiffon"),
            [ColorKey.Shadow] = ThemeColor.FromHex("#4B0082", "Indigo"),

            // ═══════════════════════════════════════════════════════════════════
            // Terrain Colors
            // ═══════════════════════════════════════════════════════════════════
            [ColorKey.Floor] = ThemeColor.FromHex("#696969", "Dim Gray"),
            [ColorKey.Wall] = ThemeColor.FromHex("#2F4F4F", "Dark Slate Gray"),
            [ColorKey.Water] = ThemeColor.FromHex("#1E90FF", "Dodger Blue"),
            [ColorKey.Lava] = ThemeColor.FromHex("#FF4500", "Orange Red"),
            [ColorKey.Grass] = ThemeColor.FromHex("#228B22", "Forest Green"),
            [ColorKey.Door] = ThemeColor.FromHex("#8B4513", "Saddle Brown"),

            // ═══════════════════════════════════════════════════════════════════
            // Entity Colors
            // ═══════════════════════════════════════════════════════════════════
            [ColorKey.Player] = ThemeColor.FromHex("#00FF00", "Lime"),
            [ColorKey.Enemy] = ThemeColor.FromHex("#FF0000", "Red"),
            [ColorKey.Npc] = ThemeColor.FromHex("#FFFF00", "Yellow"),
            [ColorKey.Boss] = ThemeColor.FromHex("#FF00FF", "Magenta"),
            [ColorKey.Ally] = ThemeColor.FromHex("#00FFFF", "Cyan"),

            // ═══════════════════════════════════════════════════════════════════
            // UI State Colors
            // ═══════════════════════════════════════════════════════════════════
            [ColorKey.Selected] = ThemeColor.FromHex("#4A90D9", "Primary Blue"),
            [ColorKey.Disabled] = ThemeColor.FromHex("#555555", "Dark Gray"),
            [ColorKey.Border] = ThemeColor.FromHex("#7F8C8D", "Gray")
        };

        return new ColorPalette(colors);
    }

    /// <summary>
    /// Gets a color by its key.
    /// </summary>
    /// <param name="key">The color key to retrieve.</param>
    /// <returns>
    /// The theme color for the specified key, or a magenta fallback color
    /// if the key is not defined.
    /// </returns>
    public ThemeColor GetColor(ColorKey key) =>
        _colors.TryGetValue(key, out var color)
            ? color
            : ThemeColor.Fallback;

    /// <summary>
    /// Checks if the palette contains a color for the specified key.
    /// </summary>
    /// <param name="key">The color key to check.</param>
    /// <returns>True if the color is defined; otherwise false.</returns>
    public bool ContainsColor(ColorKey key) => _colors.ContainsKey(key);

    /// <summary>
    /// Gets the total number of colors in the palette.
    /// </summary>
    public int Count => _colors.Count;

    /// <summary>
    /// Gets all defined color keys.
    /// </summary>
    public IEnumerable<ColorKey> Keys => _colors.Keys;
}
