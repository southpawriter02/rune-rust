// ═══════════════════════════════════════════════════════════════════════════════
// NodeStateDisplayConfig.cs
// Configuration for ability tree node state display settings.
// Version: 0.13.2b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration for ability tree node state display settings.
/// </summary>
/// <remarks>
/// <para>Loaded from config/node-state-display.json.</para>
/// <para>Provides customization for node dimensions, state indicators,
/// state colors, and display formatting.</para>
/// </remarks>
public class NodeStateDisplayConfig
{
    /// <summary>
    /// The width of a node box in characters.
    /// </summary>
    /// <remarks>
    /// Default width of 11 characters accommodates the node name
    /// and state indicator with padding.
    /// </remarks>
    public int NodeWidth { get; set; } = 11;

    /// <summary>
    /// The height of a node box in rows.
    /// </summary>
    /// <remarks>
    /// Default height of 3 rows provides space for top border,
    /// content, and bottom border.
    /// </remarks>
    public int NodeHeight { get; set; } = 3;

    /// <summary>
    /// Horizontal spacing between nodes in the same row.
    /// </summary>
    public int NodeHorizontalSpacing { get; set; } = 4;

    /// <summary>
    /// Vertical spacing between nodes in the same column.
    /// </summary>
    public int NodeVerticalSpacing { get; set; } = 2;

    /// <summary>
    /// State indicator configuration.
    /// </summary>
    public NodeStateIndicators Indicators { get; set; } = new();

    /// <summary>
    /// State color configuration.
    /// </summary>
    public NodeStateColors Colors { get; set; } = new();

    /// <summary>
    /// Creates a default configuration with standard values.
    /// </summary>
    /// <returns>Default configuration instance.</returns>
    public static NodeStateDisplayConfig CreateDefault() => new();
}

/// <summary>
/// State indicator strings for node rendering.
/// </summary>
/// <remarks>
/// <para>Defines the visual indicators displayed for each node state.</para>
/// <para>Indicators are typically 3 characters to fit within the node box.</para>
/// </remarks>
public class NodeStateIndicators
{
    /// <summary>
    /// Indicator for unlocked/purchased nodes.
    /// </summary>
    /// <remarks>
    /// Default "[x]" indicates the node has been unlocked.
    /// </remarks>
    public string UnlockedIndicator { get; set; } = "[x]";

    /// <summary>
    /// Indicator for available nodes.
    /// </summary>
    /// <remarks>
    /// Default "( )" indicates the node can be unlocked.
    /// </remarks>
    public string AvailableIndicator { get; set; } = "( )";

    /// <summary>
    /// Indicator for locked nodes.
    /// </summary>
    /// <remarks>
    /// Default "[L]" indicates the node cannot yet be unlocked.
    /// </remarks>
    public string LockedIndicator { get; set; } = "[L]";
}

/// <summary>
/// Console colors for node state rendering.
/// </summary>
/// <remarks>
/// <para>Defines the colors used for each node state.</para>
/// <para>Colors provide visual distinction between states at a glance.</para>
/// </remarks>
public class NodeStateColors
{
    /// <summary>
    /// Color for unlocked nodes.
    /// </summary>
    /// <remarks>
    /// Green indicates the node is active and providing benefits.
    /// </remarks>
    public ConsoleColor UnlockedColor { get; set; } = ConsoleColor.Green;

    /// <summary>
    /// Color for available nodes.
    /// </summary>
    /// <remarks>
    /// Yellow indicates the node is ready to be unlocked.
    /// </remarks>
    public ConsoleColor AvailableColor { get; set; } = ConsoleColor.Yellow;

    /// <summary>
    /// Color for locked nodes.
    /// </summary>
    /// <remarks>
    /// DarkGray indicates the node is not yet accessible.
    /// </remarks>
    public ConsoleColor LockedColor { get; set; } = ConsoleColor.DarkGray;

    /// <summary>
    /// Color for highlighted/selected nodes.
    /// </summary>
    /// <remarks>
    /// White provides high contrast for the currently selected node.
    /// </remarks>
    public ConsoleColor HighlightedNodeColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Color for rank progress text on multi-rank nodes.
    /// </summary>
    /// <remarks>
    /// Cyan distinguishes the rank progress from the node name.
    /// </remarks>
    public ConsoleColor RankProgressColor { get; set; } = ConsoleColor.Cyan;
}
