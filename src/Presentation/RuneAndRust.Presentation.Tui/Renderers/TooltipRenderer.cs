// ═══════════════════════════════════════════════════════════════════════════════
// TooltipRenderer.cs
// Renders tooltip content for ability tree nodes.
// Version: 0.13.2d
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Enums;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Renders tooltip content for ability tree nodes.
/// </summary>
/// <remarks>
/// <para>Provides consistent formatting for ability details, requirements,
/// and status information displayed in node tooltips:</para>
/// <list type="bullet">
///   <item><description>Title: Node name in uppercase</description></item>
///   <item><description>Subtitle: Tier number and point cost</description></item>
///   <item><description>Description: Ability description (word-wrapped)</description></item>
///   <item><description>Details Section: Cooldown and resource cost</description></item>
///   <item><description>Requirements Section: Prerequisites with satisfaction indicators</description></item>
///   <item><description>Footer: Status message with action prompt</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var renderer = new TooltipRenderer(config, logger);
/// var tooltip = renderer.FormatAbilityDetails(node, NodeState.Available);
/// var lines = renderer.BuildTooltipLines(tooltip, 45);
/// </code>
/// </example>
public class TooltipRenderer
{
    private readonly NodeTooltipConfig _config;
    private readonly ILogger<TooltipRenderer>? _logger;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new instance of the TooltipRenderer.
    /// </summary>
    /// <param name="config">Configuration for tooltip display settings.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    public TooltipRenderer(
        IOptions<NodeTooltipConfig>? config = null,
        ILogger<TooltipRenderer>? logger = null)
    {
        _config = config?.Value ?? NodeTooltipConfig.CreateDefault();
        _logger = logger;

        _logger?.LogDebug("TooltipRenderer initialized with width {Width}", _config.TooltipWidth);
    }

    // ═══════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Formats ability details into a tooltip DTO.
    /// </summary>
    /// <param name="node">The ability node.</param>
    /// <param name="state">The current state of the node.</param>
    /// <param name="currentRank">The current rank for multi-rank abilities.</param>
    /// <returns>A formatted tooltip display DTO.</returns>
    public TooltipDisplayDto FormatAbilityDetails(
        AbilityTreeNode node,
        NodeState state,
        int currentRank = 0)
    {
        ArgumentNullException.ThrowIfNull(node);

        var sections = new List<TooltipSectionDto>();

        // Build rank section for multi-rank abilities
        if (node.MaxRank > 1)
        {
            var rankLines = new List<TooltipLineDto>
            {
                new("Max Rank", $"{node.MaxRank}")
            };
            sections.Add(new TooltipSectionDto(null, rankLines));
        }

        // Build status line based on state
        var statusText = GetStatusText(state, currentRank, node.MaxRank);
        var footer = $"Status: {statusText}";

        _logger?.LogDebug(
            "Formatted tooltip for node {NodeId}: state={State}",
            node.NodeId, state);

        return new TooltipDisplayDto(
            Title: node.Name.ToUpperInvariant(),
            Subtitle: $"Tier {node.Tier} | Cost: {FormatCost(node.PointCost)}",
            Description: node.Description,
            Sections: sections,
            Footer: footer);
    }

    /// <summary>
    /// Formats requirements into a tooltip section.
    /// </summary>
    /// <param name="prerequisites">The prerequisite node IDs.</param>
    /// <param name="unlockedNodeIds">Set of unlocked node IDs for satisfaction checking.</param>
    /// <returns>A formatted requirements section DTO.</returns>
    public TooltipSectionDto FormatRequirements(
        IReadOnlyList<string> prerequisites,
        IReadOnlySet<string> unlockedNodeIds)
    {
        ArgumentNullException.ThrowIfNull(prerequisites);
        ArgumentNullException.ThrowIfNull(unlockedNodeIds);

        var lines = new List<TooltipLineDto>();

        foreach (var prereqId in prerequisites)
        {
            var isSatisfied = unlockedNodeIds.Contains(prereqId);
            var indicator = isSatisfied ? "[x]" : "[ ]";

            // Format prerequisite ID as human-readable name
            var displayName = FormatNodeIdAsName(prereqId);
            var label = $"{indicator} {displayName}";

            lines.Add(new TooltipLineDto(label, "", isSatisfied));
        }

        _logger?.LogDebug(
            "Formatted {Count} prerequisites, {Satisfied} satisfied",
            prerequisites.Count,
            lines.Count(l => l.IsSatisfied));

        return new TooltipSectionDto("Prerequisites:", lines);
    }

    /// <summary>
    /// Formats the point cost display.
    /// </summary>
    /// <param name="pointCost">The talent point cost.</param>
    /// <returns>The formatted cost string (e.g., "1 point" or "2 points").</returns>
    public string FormatCost(int pointCost)
    {
        return pointCost == 1 ? "1 point" : $"{pointCost} points";
    }

    /// <summary>
    /// Gets the configured tooltip width.
    /// </summary>
    /// <returns>The tooltip width in characters.</returns>
    public int GetTooltipWidth() => _config.TooltipWidth;

    /// <summary>
    /// Gets the current configuration.
    /// </summary>
    /// <returns>The tooltip configuration.</returns>
    public NodeTooltipConfig GetConfig() => _config;

    /// <summary>
    /// Calculates the tooltip position relative to a node.
    /// </summary>
    /// <param name="nodeBounds">The bounds of the selected node.</param>
    /// <param name="tooltipWidth">The tooltip width.</param>
    /// <param name="maxHeight">The maximum tooltip height.</param>
    /// <returns>The calculated tooltip position.</returns>
    public TooltipPosition GetTooltipPosition(
        NodeBounds nodeBounds,
        int tooltipWidth,
        int maxHeight)
    {
        ArgumentNullException.ThrowIfNull(nodeBounds);

        // Default: position tooltip to the right of the node
        var x = nodeBounds.X + nodeBounds.Width + _config.TooltipOffset;
        var y = nodeBounds.Y;

        // Adjust if tooltip would go off-screen
        var screenWidth = Console.WindowWidth;
        if (x + tooltipWidth > screenWidth - 2)
        {
            // Position to the left of the node instead
            x = nodeBounds.X - tooltipWidth - _config.TooltipOffset;
        }

        // Ensure minimum X position
        if (x < 1)
        {
            x = 1;
        }

        _logger?.LogDebug(
            "Calculated tooltip position ({X}, {Y}) for node at ({NodeX}, {NodeY})",
            x, y, nodeBounds.X, nodeBounds.Y);

        return new TooltipPosition(x, y);
    }

    /// <summary>
    /// Builds the tooltip content lines for rendering.
    /// </summary>
    /// <param name="tooltip">The tooltip DTO.</param>
    /// <param name="width">The available width.</param>
    /// <returns>A list of formatted lines ready for display.</returns>
    public List<string> BuildTooltipLines(TooltipDisplayDto tooltip, int width)
    {
        ArgumentNullException.ThrowIfNull(tooltip);

        var lines = new List<string>();
        var innerWidth = width - 2; // Account for border characters

        // Title (centered and uppercase)
        lines.Add(CenterText(tooltip.Title, innerWidth));
        lines.Add(new string('─', innerWidth));

        // Subtitle
        if (!string.IsNullOrEmpty(tooltip.Subtitle))
        {
            lines.Add(CenterText(tooltip.Subtitle, innerWidth));
            lines.Add(new string('─', innerWidth));
        }

        // Description (word-wrapped)
        if (!string.IsNullOrEmpty(tooltip.Description))
        {
            var wrappedLines = WrapText(tooltip.Description, innerWidth);
            lines.AddRange(wrappedLines);
            lines.Add(new string('─', innerWidth));
        }

        // Sections (details, prerequisites)
        foreach (var section in tooltip.Sections)
        {
            // Section header
            if (!string.IsNullOrEmpty(section.Header))
            {
                lines.Add(section.Header.PadRight(innerWidth));
            }

            // Section lines
            foreach (var line in section.Lines)
            {
                var formattedLine = string.IsNullOrEmpty(line.Value)
                    ? line.Label
                    : $"{line.Label}: {line.Value}";
                lines.Add(formattedLine.PadRight(innerWidth));
            }
        }

        // Footer (status)
        if (!string.IsNullOrEmpty(tooltip.Footer))
        {
            if (tooltip.Sections.Count > 0)
            {
                lines.Add(new string('─', innerWidth));
            }
            lines.Add(tooltip.Footer.PadRight(innerWidth));
        }

        _logger?.LogDebug("Built {LineCount} tooltip lines", lines.Count);

        return lines;
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the status text based on node state.
    /// </summary>
    private static string GetStatusText(NodeState state, int currentRank, int maxRank)
    {
        return state switch
        {
            NodeState.Unlocked when maxRank > 1 => $"UNLOCKED (Rank {currentRank}/{maxRank})",
            NodeState.Unlocked => "UNLOCKED",
            NodeState.Available => "AVAILABLE - Press [U] to unlock",
            NodeState.Locked => "LOCKED",
            _ => "UNKNOWN"
        };
    }

    /// <summary>
    /// Centers text within the given width.
    /// </summary>
    private static string CenterText(string text, int width)
    {
        if (text.Length >= width)
        {
            return text[..width];
        }

        var padding = (width - text.Length) / 2;
        return text.PadLeft(padding + text.Length).PadRight(width);
    }

    /// <summary>
    /// Wraps text to fit within the given width.
    /// </summary>
    private static IEnumerable<string> WrapText(string text, int maxWidth)
    {
        var words = text.Split(' ');
        var currentLine = new List<string>();
        var currentLength = 0;

        foreach (var word in words)
        {
            var spaceNeeded = currentLine.Count > 0 ? 1 : 0;

            if (currentLength + word.Length + spaceNeeded > maxWidth)
            {
                if (currentLine.Count > 0)
                {
                    yield return string.Join(" ", currentLine).PadRight(maxWidth);
                    currentLine.Clear();
                    currentLength = 0;
                }
            }

            currentLine.Add(word);
            currentLength += word.Length + (currentLine.Count > 1 ? 1 : 0);
        }

        if (currentLine.Count > 0)
        {
            yield return string.Join(" ", currentLine).PadRight(maxWidth);
        }
    }

    /// <summary>
    /// Formats a node ID as a human-readable name.
    /// </summary>
    private static string FormatNodeIdAsName(string nodeId)
    {
        // Convert kebab-case to Title Case
        // "power-strike" -> "Power Strike"
        var words = nodeId.Split('-');
        var formatted = words.Select(w =>
            string.IsNullOrEmpty(w) ? w :
            char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant());
        return string.Join(" ", formatted);
    }
}
