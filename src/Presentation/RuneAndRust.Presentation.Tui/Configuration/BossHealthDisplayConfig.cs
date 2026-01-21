namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration for boss health bar display settings.
/// </summary>
/// <remarks>
/// <para>Loaded from config/boss-health-display.json.</para>
/// <para>Provides customization for bar dimensions, symbols, colors,
/// health thresholds, and animation timing.</para>
/// </remarks>
public class BossHealthDisplayConfig
{
    /// <summary>
    /// The width of the health bar fill area in characters.
    /// </summary>
    public int BarWidth { get; set; } = 60;

    /// <summary>
    /// The total width of the display area.
    /// </summary>
    public int TotalWidth { get; set; } = 72;

    /// <summary>
    /// The total height of the display area in rows.
    /// </summary>
    public int TotalHeight { get; set; } = 6;

    /// <summary>
    /// Starting X coordinate for the display.
    /// </summary>
    public int StartX { get; set; } = 0;

    /// <summary>
    /// Starting Y coordinate for the display.
    /// </summary>
    public int StartY { get; set; } = 0;

    /// <summary>
    /// Indentation for text elements.
    /// </summary>
    public int TextIndent { get; set; } = 3;

    /// <summary>
    /// Symbol configuration for the health bar.
    /// </summary>
    public BossHealthBarSymbols Symbols { get; set; } = new();

    /// <summary>
    /// Color configuration for the health bar.
    /// </summary>
    public BossHealthBarColors Colors { get; set; } = new();

    /// <summary>
    /// Color thresholds for health percentage display.
    /// </summary>
    /// <remarks>
    /// Thresholds are evaluated in descending order by Percent.
    /// The first threshold where health percentage is greater than the threshold
    /// determines the color.
    /// </remarks>
    public List<HealthColorThreshold> ColorThresholds { get; set; } =
    [
        new() { Percent = 75, Color = ConsoleColor.Green },
        new() { Percent = 50, Color = ConsoleColor.Yellow },
        new() { Percent = 25, Color = ConsoleColor.DarkYellow },
        new() { Percent = 0, Color = ConsoleColor.Red }
    ];

    /// <summary>
    /// Animation timing configuration.
    /// </summary>
    public DamageAnimationConfig Animation { get; set; } = new();

    /// <summary>
    /// Creates a default configuration with standard values.
    /// </summary>
    /// <returns>Default configuration instance.</returns>
    public static BossHealthDisplayConfig CreateDefault() => new();
}

/// <summary>
/// Symbol characters for boss health bar rendering.
/// </summary>
/// <remarks>
/// Provides customizable characters for the health bar fill, borders,
/// phase markers, and boss name header decoration.
/// </remarks>
public class BossHealthBarSymbols
{
    /// <summary>
    /// Character for filled health bar portion.
    /// </summary>
    public char FilledChar { get; set; } = '#';

    /// <summary>
    /// Character for empty health bar portion.
    /// </summary>
    public char EmptyChar { get; set; } = '.';

    /// <summary>
    /// Character for bar border.
    /// </summary>
    public char BorderChar { get; set; } = '=';

    /// <summary>
    /// Character for bar corners.
    /// </summary>
    public char CornerChar { get; set; } = '+';

    /// <summary>
    /// Character for phase marker on bar.
    /// </summary>
    public char PhaseMarkerChar { get; set; } = '|';

    /// <summary>
    /// Prefix for boss name header.
    /// </summary>
    public string HeaderPrefix { get; set; } = "<<";

    /// <summary>
    /// Suffix for boss name header.
    /// </summary>
    public string HeaderSuffix { get; set; } = ">>";
}

/// <summary>
/// Color configuration for boss health bar elements.
/// </summary>
/// <remarks>
/// Defines console colors for various health bar UI elements
/// including the header, phase markers, and damage effects.
/// </remarks>
public class BossHealthBarColors
{
    /// <summary>
    /// Color for the boss name header.
    /// </summary>
    public ConsoleColor HeaderColor { get; set; } = ConsoleColor.Magenta;

    /// <summary>
    /// Color for phase markers.
    /// </summary>
    public ConsoleColor PhaseMarkerColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Color for damage flash effect.
    /// </summary>
    public ConsoleColor DamageFlashColor { get; set; } = ConsoleColor.Red;

    /// <summary>
    /// Color for damage delta text.
    /// </summary>
    public ConsoleColor DamageColor { get; set; } = ConsoleColor.Red;

    /// <summary>
    /// Color for critical health (0%).
    /// </summary>
    public ConsoleColor CriticalColor { get; set; } = ConsoleColor.DarkRed;

    /// <summary>
    /// Color for healing delta text.
    /// </summary>
    public ConsoleColor HealingColor { get; set; } = ConsoleColor.Green;
}

/// <summary>
/// Health color threshold configuration.
/// </summary>
/// <remarks>
/// Defines a health percentage threshold and the associated color.
/// When health is above this threshold, this color is used for the bar fill.
/// </remarks>
public class HealthColorThreshold
{
    /// <summary>
    /// The minimum health percentage for this color.
    /// </summary>
    /// <remarks>
    /// The color is used when health percentage is greater than this value.
    /// </remarks>
    public int Percent { get; set; }

    /// <summary>
    /// The color to use above this percentage.
    /// </summary>
    public ConsoleColor Color { get; set; }
}

/// <summary>
/// Damage animation timing configuration.
/// </summary>
/// <remarks>
/// Controls the timing of the damage flash effect and delta text display
/// during boss damage animation.
/// </remarks>
public class DamageAnimationConfig
{
    /// <summary>
    /// Duration of the damage flash effect in milliseconds.
    /// </summary>
    public int FlashDurationMs { get; set; } = 100;

    /// <summary>
    /// Duration to display the damage delta in milliseconds.
    /// </summary>
    public int DeltaDisplayMs { get; set; } = 500;
}
