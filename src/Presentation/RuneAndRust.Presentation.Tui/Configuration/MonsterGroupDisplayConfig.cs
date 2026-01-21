namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration for monster group display settings.
/// </summary>
/// <remarks>
/// <para>Loaded from config/monster-group-display.json.</para>
/// <para>Provides customization for position, size, colors, and symbols
/// for the monster group UI components.</para>
/// </remarks>
public class MonsterGroupDisplayConfig
{
    /// <summary>
    /// The total width of the display area.
    /// </summary>
    public int TotalWidth { get; set; } = 72;

    /// <summary>
    /// The total height of the display area in rows.
    /// </summary>
    public int TotalHeight { get; set; } = 16;

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
    /// Row offset for member cards (from StartY).
    /// </summary>
    public int MemberCardStartRow { get; set; } = 2;

    /// <summary>
    /// Width of each member card.
    /// </summary>
    public int MemberCardWidth { get; set; } = 15;

    /// <summary>
    /// Spacing between member cards.
    /// </summary>
    public int CardSpacing { get; set; } = 2;

    /// <summary>
    /// Row offset for tactic indicator (from StartY).
    /// </summary>
    public int TacticIndicatorRow { get; set; } = 9;

    /// <summary>
    /// Row offset for morale hint (from StartY).
    /// </summary>
    public int MoraleHintRow { get; set; } = 13;

    /// <summary>
    /// Row offset for morale effect display (from StartY).
    /// </summary>
    public int MoraleEffectRow { get; set; } = 14;

    /// <summary>
    /// Symbol configuration for the group display.
    /// </summary>
    public MonsterGroupSymbols Symbols { get; set; } = new();

    /// <summary>
    /// Color configuration for the group display.
    /// </summary>
    public MonsterGroupColors Colors { get; set; } = new();

    /// <summary>
    /// Health color thresholds for member display.
    /// </summary>
    /// <remarks>
    /// Thresholds are checked in descending order by percent.
    /// Color is applied when health is greater than the threshold percent.
    /// </remarks>
    public List<HealthColorThreshold> HealthColorThresholds { get; set; } = new()
    {
        new HealthColorThreshold { Percent = 75, Color = ConsoleColor.Green },
        new HealthColorThreshold { Percent = 50, Color = ConsoleColor.Yellow },
        new HealthColorThreshold { Percent = 25, Color = ConsoleColor.DarkYellow },
        new HealthColorThreshold { Percent = 0, Color = ConsoleColor.Red }
    };

    /// <summary>
    /// Effect box configuration for notifications.
    /// </summary>
    public EffectBoxConfig EffectBox { get; set; } = new();

    /// <summary>
    /// Creates a default configuration with standard values.
    /// </summary>
    /// <returns>Default configuration instance.</returns>
    public static MonsterGroupDisplayConfig CreateDefault() => new();
}

/// <summary>
/// Symbol characters for monster group display.
/// </summary>
/// <remarks>
/// Provides configurable symbols for card borders, leader badges,
/// and tree-style role assignment prefixes.
/// </remarks>
public class MonsterGroupSymbols
{
    /// <summary>Symbol for leader badge displayed on leader's card.</summary>
    public string LeaderBadge { get; set; } = "[*]";

    /// <summary>Character for card top-left corner.</summary>
    public char CardTopLeft { get; set; } = '┌';

    /// <summary>Character for card top-right corner.</summary>
    public char CardTopRight { get; set; } = '┐';

    /// <summary>Character for card bottom-left corner.</summary>
    public char CardBottomLeft { get; set; } = '└';

    /// <summary>Character for card bottom-right corner.</summary>
    public char CardBottomRight { get; set; } = '┘';

    /// <summary>Character for card horizontal border.</summary>
    public char CardHorizontal { get; set; } = '─';

    /// <summary>Character for card vertical border.</summary>
    public char CardVertical { get; set; } = '│';

    /// <summary>Prefix for tree branch (non-last item in list).</summary>
    public string TreeBranch { get; set; } = "|-- ";

    /// <summary>Prefix for tree end (last item in list).</summary>
    public string TreeEnd { get; set; } = "+-- ";
}

/// <summary>
/// Color configuration for monster group elements.
/// </summary>
/// <remarks>
/// Provides customizable colors for group headers, leader badges,
/// tactic indicators, synergies, and effect boxes.
/// </remarks>
public class MonsterGroupColors
{
    /// <summary>Color for the group name header.</summary>
    public ConsoleColor HeaderColor { get; set; } = ConsoleColor.Cyan;

    /// <summary>Color for leader badge and name.</summary>
    public ConsoleColor LeaderColor { get; set; } = ConsoleColor.Yellow;

    /// <summary>Color for tactic title.</summary>
    public ConsoleColor TacticColor { get; set; } = ConsoleColor.Yellow;

    /// <summary>Color for highlighted/active tactic.</summary>
    public ConsoleColor TacticHighlightColor { get; set; } = ConsoleColor.Cyan;

    /// <summary>Color for synergy notifications.</summary>
    public ConsoleColor SynergyColor { get; set; } = ConsoleColor.Green;

    /// <summary>Color for morale hint text.</summary>
    public ConsoleColor HintColor { get; set; } = ConsoleColor.DarkYellow;

    /// <summary>Color for critical health (0%).</summary>
    public ConsoleColor CriticalHealthColor { get; set; } = ConsoleColor.DarkRed;

    /// <summary>Default text color.</summary>
    public ConsoleColor DefaultColor { get; set; } = ConsoleColor.Gray;

    /// <summary>Color for effect box border.</summary>
    public ConsoleColor EffectBoxBorderColor { get; set; } = ConsoleColor.Yellow;

    /// <summary>Color for effect box title.</summary>
    public ConsoleColor EffectBoxTitleColor { get; set; } = ConsoleColor.Red;

    /// <summary>Color for effect box content text.</summary>
    public ConsoleColor EffectBoxContentColor { get; set; } = ConsoleColor.White;

    /// <summary>Color for negative effect text.</summary>
    public ConsoleColor NegativeEffectColor { get; set; } = ConsoleColor.Red;
}

/// <summary>
/// Configuration for effect notification boxes.
/// </summary>
/// <remarks>
/// Effect boxes are used for dramatic notifications like leader defeat.
/// They appear centered in the group display area.
/// </remarks>
public class EffectBoxConfig
{
    /// <summary>Width of the effect box.</summary>
    public int Width { get; set; } = 53;

    /// <summary>Height of the effect box.</summary>
    public int Height { get; set; } = 10;

    /// <summary>Starting row for the effect box (offset from group display StartY).</summary>
    public int StartRow { get; set; } = 3;
}
