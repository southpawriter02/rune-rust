// ═══════════════════════════════════════════════════════════════════════════════
// NodeTooltipConfig.cs
// Configuration for ability tree node tooltip display settings.
// Version: 0.13.2d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.Configuration;

/// <summary>
/// Configuration for node tooltip display settings.
/// </summary>
/// <remarks>
/// <para>Controls tooltip appearance and behavior:</para>
/// <list type="bullet">
///   <item><description>Dimensions: Tooltip width, height, and offset from nodes</description></item>
///   <item><description>Colors: Border, prompt, and point display colors</description></item>
///   <item><description>Requirement colors: Satisfied and unsatisfied states</description></item>
/// </list>
/// <para>Configuration is loaded from <c>config/node-tooltip.json</c>.</para>
/// </remarks>
/// <example>
/// <code>
/// var config = new NodeTooltipConfig
/// {
///     TooltipWidth = 45,
///     BorderColor = ConsoleColor.DarkCyan,
///     AvailablePointsColor = ConsoleColor.Yellow
/// };
/// </code>
/// </example>
public class NodeTooltipConfig
{
    // ═══════════════════════════════════════════════════════════════
    // TOOLTIP DIMENSIONS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the width of the tooltip box in characters.
    /// </summary>
    /// <value>Default: 45 characters.</value>
    public int TooltipWidth { get; set; } = 45;

    /// <summary>
    /// Gets or sets the maximum height of the tooltip in lines.
    /// </summary>
    /// <value>Default: 20 lines.</value>
    /// <remarks>
    /// Tooltips taller than this will be truncated.
    /// </remarks>
    public int MaxTooltipHeight { get; set; } = 20;

    /// <summary>
    /// Gets or sets the horizontal offset from the node when positioning tooltip.
    /// </summary>
    /// <value>Default: 2 characters.</value>
    /// <remarks>
    /// Provides visual separation between the node and tooltip.
    /// </remarks>
    public int TooltipOffset { get; set; } = 2;

    // ═══════════════════════════════════════════════════════════════
    // POINT DISPLAY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the width of the talent point display area.
    /// </summary>
    /// <value>Default: 40 characters.</value>
    public int PointDisplayWidth { get; set; } = 40;

    // ═══════════════════════════════════════════════════════════════
    // TOOLTIP COLORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the color for the tooltip border.
    /// </summary>
    /// <value>Default: <see cref="ConsoleColor.DarkCyan"/>.</value>
    public ConsoleColor BorderColor { get; set; } = ConsoleColor.DarkCyan;

    /// <summary>
    /// Gets or sets the color for the confirmation prompt.
    /// </summary>
    /// <value>Default: <see cref="ConsoleColor.Yellow"/>.</value>
    /// <remarks>
    /// Used for the "[Y] Yes [N] No" unlock confirmation text.
    /// </remarks>
    public ConsoleColor PromptColor { get; set; } = ConsoleColor.Yellow;

    // ═══════════════════════════════════════════════════════════════
    // POINT DISPLAY COLORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the color for available points when greater than zero.
    /// </summary>
    /// <value>Default: <see cref="ConsoleColor.Yellow"/>.</value>
    public ConsoleColor AvailablePointsColor { get; set; } = ConsoleColor.Yellow;

    /// <summary>
    /// Gets or sets the color for spent points.
    /// </summary>
    /// <value>Default: <see cref="ConsoleColor.Gray"/>.</value>
    public ConsoleColor SpentPointsColor { get; set; } = ConsoleColor.Gray;

    // ═══════════════════════════════════════════════════════════════
    // REQUIREMENT COLORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the color for satisfied requirements.
    /// </summary>
    /// <value>Default: <see cref="ConsoleColor.Green"/>.</value>
    public ConsoleColor SatisfiedColor { get; set; } = ConsoleColor.Green;

    /// <summary>
    /// Gets or sets the color for unsatisfied requirements.
    /// </summary>
    /// <value>Default: <see cref="ConsoleColor.DarkGray"/>.</value>
    public ConsoleColor UnsatisfiedColor { get; set; } = ConsoleColor.DarkGray;

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a default configuration instance.
    /// </summary>
    /// <returns>A new <see cref="NodeTooltipConfig"/> with default values.</returns>
    public static NodeTooltipConfig CreateDefault() => new();
}
