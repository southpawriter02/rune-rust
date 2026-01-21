// ═══════════════════════════════════════════════════════════════════════════════
// LeaderboardViewConfig.cs
// Configuration settings for the Leaderboard View display.
// Version: 0.13.4c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration settings for the leaderboard view display.
/// </summary>
/// <remarks>
/// <para>
/// Provides customizable settings for panel dimensions, column widths,
/// maximum displayed entries, and color schemes for rank highlighting.
/// </para>
/// <para>Default column layout:</para>
/// <code>
/// Rank  Name          Class     Level    Score        Date
/// (6)   (14)          (10)      (8)      (12)         (12)
/// </code>
/// </remarks>
public class LeaderboardViewConfig
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PANEL DIMENSIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the total width of the leaderboard panel in characters.
    /// </summary>
    public int PanelWidth { get; set; } = 72;

    /// <summary>
    /// Gets or sets the total height of the leaderboard panel in lines.
    /// </summary>
    public int PanelHeight { get; set; } = 35;

    // ═══════════════════════════════════════════════════════════════════════════
    // COLUMN WIDTHS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the width of the rank column.
    /// </summary>
    public int RankColumnWidth { get; set; } = 6;

    /// <summary>
    /// Gets or sets the width of the name column.
    /// </summary>
    public int NameColumnWidth { get; set; } = 14;

    /// <summary>
    /// Gets or sets the width of the class column.
    /// </summary>
    public int ClassColumnWidth { get; set; } = 10;

    /// <summary>
    /// Gets or sets the width of the level column.
    /// </summary>
    public int LevelColumnWidth { get; set; } = 8;

    /// <summary>
    /// Gets or sets the width of the score column.
    /// </summary>
    public int ScoreColumnWidth { get; set; } = 12;

    /// <summary>
    /// Gets or sets the width of the date column.
    /// </summary>
    public int DateColumnWidth { get; set; } = 12;

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY SETTINGS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the maximum number of entries to display.
    /// </summary>
    public int MaxDisplayEntries { get; set; } = 10;

    // ═══════════════════════════════════════════════════════════════════════════
    // COLORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the color for first place (Gold).
    /// </summary>
    public ConsoleColor FirstPlaceColor { get; set; } = ConsoleColor.Yellow;

    /// <summary>
    /// Gets or sets the color for second place (Silver).
    /// </summary>
    public ConsoleColor SecondPlaceColor { get; set; } = ConsoleColor.Gray;

    /// <summary>
    /// Gets or sets the color for third place (Bronze).
    /// </summary>
    public ConsoleColor ThirdPlaceColor { get; set; } = ConsoleColor.DarkYellow;

    /// <summary>
    /// Gets or sets the color for the current player's entry.
    /// </summary>
    public ConsoleColor PlayerHighlightColor { get; set; } = ConsoleColor.Cyan;

    /// <summary>
    /// Gets or sets the default entry color.
    /// </summary>
    public ConsoleColor DefaultEntryColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Gets or sets the color for new record text.
    /// </summary>
    public ConsoleColor NewRecordColor { get; set; } = ConsoleColor.Green;
}
