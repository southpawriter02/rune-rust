// ═══════════════════════════════════════════════════════════════════════════════
// NodeStateRenderer.cs
// Handles visual formatting of ability tree node states.
// Version: 0.13.2b
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Enums;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Handles the visual formatting of ability tree node states.
/// </summary>
/// <remarks>
/// <para>NodeStateRenderer is a stateless renderer that provides methods for:</para>
/// <list type="bullet">
///   <item><description>Getting state-specific indicators ("[x]", "( )", "[L]")</description></item>
///   <item><description>Getting state-specific colors (Green, Yellow, DarkGray)</description></item>
///   <item><description>Formatting node content including name and rank progress</description></item>
/// </list>
/// <para>Configuration is loaded from <c>config/node-state-display.json</c>.</para>
/// </remarks>
/// <example>
/// <code>
/// var renderer = new NodeStateRenderer(config, logger);
/// var indicator = renderer.GetStateIndicator(NodeState.Available); // Returns "( )"
/// var color = renderer.GetStateColor(NodeState.Unlocked);          // Returns Green
/// </code>
/// </example>
public class NodeStateRenderer
{
    private readonly NodeStateDisplayConfig _config;
    private readonly ILogger<NodeStateRenderer>? _logger;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new NodeStateRenderer with the specified configuration.
    /// </summary>
    /// <param name="config">Configuration for display settings.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public NodeStateRenderer(
        IOptions<NodeStateDisplayConfig>? config = null,
        ILogger<NodeStateRenderer>? logger = null)
    {
        _config = config?.Value ?? NodeStateDisplayConfig.CreateDefault();
        _logger = logger;

        _logger?.LogDebug(
            "NodeStateRenderer initialized with indicators: Unlocked='{Unlocked}', Available='{Available}', Locked='{Locked}'",
            _config.Indicators.UnlockedIndicator,
            _config.Indicators.AvailableIndicator,
            _config.Indicators.LockedIndicator);
    }

    // ═══════════════════════════════════════════════════════════════
    // STATE INDICATOR METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the visual indicator for a node state.
    /// </summary>
    /// <param name="state">The node state.</param>
    /// <returns>The state indicator string (e.g., "[x]", "( )", "[L]").</returns>
    /// <remarks>
    /// <para>Default indicators are:</para>
    /// <list type="bullet">
    ///   <item><description>Unlocked: "[x]" - node has been purchased</description></item>
    ///   <item><description>Available: "( )" - node can be purchased</description></item>
    ///   <item><description>Locked: "[L]" - prerequisites not met</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var indicator = renderer.GetStateIndicator(NodeState.Available);
    /// // Returns: "( )"
    /// </code>
    /// </example>
    public string GetStateIndicator(NodeState state)
    {
        var indicator = state switch
        {
            NodeState.Unlocked => _config.Indicators.UnlockedIndicator,
            NodeState.Available => _config.Indicators.AvailableIndicator,
            NodeState.Locked => _config.Indicators.LockedIndicator,
            _ => _config.Indicators.LockedIndicator
        };

        _logger?.LogDebug(
            "GetStateIndicator for state {State}: '{Indicator}'",
            state, indicator);

        return indicator;
    }

    /// <summary>
    /// Gets the console color for a node state.
    /// </summary>
    /// <param name="state">The node state.</param>
    /// <returns>The console color for the state.</returns>
    /// <remarks>
    /// <para>Default colors are:</para>
    /// <list type="bullet">
    ///   <item><description>Unlocked: Green - indicates active/purchased</description></item>
    ///   <item><description>Available: Yellow - indicates ready to purchase</description></item>
    ///   <item><description>Locked: DarkGray - indicates not accessible</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var color = renderer.GetStateColor(NodeState.Unlocked);
    /// // Returns: ConsoleColor.Green
    /// </code>
    /// </example>
    public ConsoleColor GetStateColor(NodeState state)
    {
        var color = state switch
        {
            NodeState.Unlocked => _config.Colors.UnlockedColor,
            NodeState.Available => _config.Colors.AvailableColor,
            NodeState.Locked => _config.Colors.LockedColor,
            _ => _config.Colors.LockedColor
        };

        _logger?.LogDebug(
            "GetStateColor for state {State}: {Color}",
            state, color);

        return color;
    }

    /// <summary>
    /// Gets the highlight color for selected nodes.
    /// </summary>
    /// <returns>The highlight color (default: White).</returns>
    public ConsoleColor GetHighlightColor()
    {
        return _config.Colors.HighlightedNodeColor;
    }

    /// <summary>
    /// Gets the color for rank progress text.
    /// </summary>
    /// <returns>The rank progress color (default: Cyan).</returns>
    public ConsoleColor GetRankProgressColor()
    {
        return _config.Colors.RankProgressColor;
    }

    // ═══════════════════════════════════════════════════════════════
    // CONTENT FORMATTING METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats the complete node content for display.
    /// </summary>
    /// <param name="dto">The node state display data.</param>
    /// <returns>Formatted node content string.</returns>
    /// <remarks>
    /// <para>Content format: "[indicator] NodeName"</para>
    /// <para>For multi-rank nodes, includes rank progress.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var dto = new NodeStateDisplayDto("power-strike", "Power Strike", 
    ///     NodeState.Available, 0, 3, 1, 1, "berserker");
    /// var content = renderer.FormatNodeContent(dto);
    /// // Returns: "( ) Power Strike"
    /// </code>
    /// </example>
    public string FormatNodeContent(NodeStateDisplayDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var indicator = GetStateIndicator(dto.State);
        var content = $"{indicator} {dto.NodeName}";

        _logger?.LogDebug(
            "FormatNodeContent for node '{NodeId}': '{Content}'",
            dto.NodeId, content);

        return content;
    }

    /// <summary>
    /// Formats the rank progress for multi-rank nodes.
    /// </summary>
    /// <param name="currentRank">The current rank invested.</param>
    /// <param name="maxRank">The maximum available rank.</param>
    /// <returns>Formatted rank progress string, or empty for single-rank nodes.</returns>
    /// <remarks>
    /// <para>Single-rank nodes (maxRank == 1) return an empty string.</para>
    /// <para>Multi-rank nodes return "currentRank/maxRank" format.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var progress = renderer.FormatRankProgress(2, 3);
    /// // Returns: "2/3"
    /// 
    /// var single = renderer.FormatRankProgress(1, 1);
    /// // Returns: ""
    /// </code>
    /// </example>
    public string FormatRankProgress(int currentRank, int maxRank)
    {
        // Single-rank nodes don't show progress
        if (maxRank <= 1)
        {
            _logger?.LogDebug(
                "FormatRankProgress: Single-rank node (maxRank={MaxRank}), returning empty",
                maxRank);
            return string.Empty;
        }

        var progress = $"{currentRank}/{maxRank}";

        _logger?.LogDebug(
            "FormatRankProgress: {Current}/{Max} -> '{Progress}'",
            currentRank, maxRank, progress);

        return progress;
    }

    /// <summary>
    /// Creates a NodeRankDisplayDto from rank values.
    /// </summary>
    /// <param name="currentRank">The current rank invested.</param>
    /// <param name="maxRank">The maximum available rank.</param>
    /// <returns>A NodeRankDisplayDto for the rank values.</returns>
    public NodeRankDisplayDto CreateRankDisplayDto(int currentRank, int maxRank)
    {
        return new NodeRankDisplayDto(currentRank, maxRank);
    }

    /// <summary>
    /// Truncates a node name to fit within the specified width.
    /// </summary>
    /// <param name="name">The node name to truncate.</param>
    /// <param name="maxWidth">The maximum width in characters.</param>
    /// <returns>Truncated name with "..." if needed.</returns>
    /// <remarks>
    /// If the name is longer than maxWidth, it is truncated and "..." is appended.
    /// </remarks>
    /// <example>
    /// <code>
    /// var truncated = renderer.TruncateName("Power Strike", 8);
    /// // Returns: "Power..."
    /// </code>
    /// </example>
    public string TruncateName(string name, int maxWidth)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (name.Length <= maxWidth)
            return name;

        var truncated = name[..(maxWidth - 3)] + "...";

        _logger?.LogDebug(
            "TruncateName: '{Original}' -> '{Truncated}' (maxWidth={MaxWidth})",
            name, truncated, maxWidth);

        return truncated;
    }

    // ═══════════════════════════════════════════════════════════════
    // CONFIGURATION ACCESS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current display configuration.
    /// </summary>
    /// <returns>The node state display configuration.</returns>
    public NodeStateDisplayConfig GetConfig() => _config;
}
