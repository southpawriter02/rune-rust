// ═══════════════════════════════════════════════════════════════════════════════
// AbilityTreeDisplayConfig.cs
// Configuration for ability tree display settings.
// Version: 0.13.2a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration for ability tree display settings.
/// </summary>
/// <remarks>
/// <para>
/// Loaded from <c>config/ability-tree-display.json</c> via dependency injection.
/// Provides configurable layout dimensions, spacing, and colors for tree rendering.
/// </para>
/// <para>
/// Key configuration areas:
/// </para>
/// <list type="bullet">
///   <item><description>Display area dimensions (width, height, position)</description></item>
///   <item><description>Node box sizing and spacing</description></item>
///   <item><description>Tier column layout offsets</description></item>
///   <item><description>Color scheme for visual elements</description></item>
/// </list>
/// </remarks>
public record AbilityTreeDisplayConfig
{
    /// <summary>The total width of the display area in characters.</summary>
    /// <remarks>Should be wide enough to accommodate MaxVisibleTiers columns.</remarks>
    public int TotalWidth { get; init; } = 72;

    /// <summary>The total height of the display area in rows.</summary>
    /// <remarks>Should be tall enough to accommodate header, labels, and MaxNodesPerTier nodes.</remarks>
    public int TotalHeight { get; init; } = 24;

    /// <summary>Starting X coordinate for the display.</summary>
    /// <remarks>Offset from left edge of terminal.</remarks>
    public int StartX { get; init; } = 0;

    /// <summary>Starting Y coordinate for the display.</summary>
    /// <remarks>Offset from top of terminal.</remarks>
    public int StartY { get; init; } = 0;

    /// <summary>Padding from the border to content.</summary>
    /// <remarks>Applied on left and right sides.</remarks>
    public int Padding { get; init; } = 2;

    /// <summary>Row offset for the tier labels (from StartY).</summary>
    /// <remarks>Tier labels appear above the node area.</remarks>
    public int TierLabelRow { get; init; } = 4;

    /// <summary>Row offset for branch headers (from StartY).</summary>
    /// <remarks>Branch headers appear between header and tier labels when multiple branches exist.</remarks>
    public int BranchHeaderRow { get; init; } = 3;

    /// <summary>Row offset where node area begins (from StartY).</summary>
    /// <remarks>Accounts for header, tier labels, and spacing.</remarks>
    public int NodeAreaStartRow { get; init; } = 7;

    /// <summary>Width of each node box in characters.</summary>
    /// <remarks>Should be wide enough for typical ability names with padding.</remarks>
    public int NodeWidth { get; init; } = 11;

    /// <summary>Height of each node box in rows.</summary>
    /// <remarks>Typically 3 rows: top border, content, bottom border.</remarks>
    public int NodeHeight { get; init; } = 3;

    /// <summary>Horizontal spacing between tier columns.</summary>
    /// <remarks>Space for prerequisite lines in future versions.</remarks>
    public int ColumnSpacing { get; init; } = 8;

    /// <summary>Vertical spacing between nodes in the same tier.</summary>
    /// <remarks>Space between node boxes within a column.</remarks>
    public int NodeVerticalSpacing { get; init; } = 1;

    /// <summary>Maximum number of tiers to display before scrolling.</summary>
    /// <remarks>Horizontal scrolling when tree has more tiers than visible.</remarks>
    public int MaxVisibleTiers { get; init; } = 4;

    /// <summary>Maximum number of nodes per tier before scrolling.</summary>
    /// <remarks>Vertical scrolling within a tier when many nodes exist.</remarks>
    public int MaxNodesPerTier { get; init; } = 4;

    /// <summary>Color configuration for the tree display.</summary>
    public AbilityTreeColors Colors { get; init; } = new();

    /// <summary>Creates default configuration.</summary>
    /// <returns>A new <see cref="AbilityTreeDisplayConfig"/> with default values.</returns>
    public static AbilityTreeDisplayConfig CreateDefault() => new();
}

/// <summary>
/// Color configuration for ability tree elements.
/// </summary>
/// <remarks>
/// <para>
/// Defines the color scheme for all visual elements of the ability tree.
/// Colors can be customized via <c>config/ability-tree-display.json</c>.
/// </para>
/// </remarks>
public record AbilityTreeColors
{
    /// <summary>Color for the tree border.</summary>
    /// <remarks>Used for the outer frame of the tree display.</remarks>
    public ConsoleColor BorderColor { get; init; } = ConsoleColor.DarkGray;

    /// <summary>Color for the tree header text.</summary>
    /// <remarks>Used for the tree name in the header area.</remarks>
    public ConsoleColor HeaderColor { get; init; } = ConsoleColor.Cyan;

    /// <summary>Color for tier labels.</summary>
    /// <remarks>Used for "TIER 1", "TIER 2", etc.</remarks>
    public ConsoleColor TierLabelColor { get; init; } = ConsoleColor.Yellow;

    /// <summary>Color for branch headers.</summary>
    /// <remarks>Used for branch names when multiple branches exist.</remarks>
    public ConsoleColor BranchHeaderColor { get; init; } = ConsoleColor.Magenta;

    /// <summary>Color for node borders.</summary>
    /// <remarks>Used for the box around unselected nodes.</remarks>
    public ConsoleColor NodeBorderColor { get; init; } = ConsoleColor.Gray;

    /// <summary>Color for selected node borders.</summary>
    /// <remarks>Used to highlight the currently selected node.</remarks>
    public ConsoleColor SelectedNodeColor { get; init; } = ConsoleColor.White;

    /// <summary>Default text color.</summary>
    /// <remarks>Fallback color for general text.</remarks>
    public ConsoleColor DefaultColor { get; init; } = ConsoleColor.Gray;
}
