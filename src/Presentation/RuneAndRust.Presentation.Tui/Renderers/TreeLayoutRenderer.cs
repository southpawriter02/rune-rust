// ═══════════════════════════════════════════════════════════════════════════════
// TreeLayoutRenderer.cs
// Handles layout calculations and text formatting for ability tree display.
// Version: 0.13.2a
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;

namespace RuneAndRust.Presentation.Tui.Renderers;

/// <summary>
/// Handles layout calculations and text formatting for ability tree display.
/// </summary>
/// <remarks>
/// <para>
/// Follows the Combat UI Component Pattern established in v0.13.0a.
/// Responsible for:
/// </para>
/// <list type="bullet">
///   <item><description>Calculate node screen positions from tree definitions</description></item>
///   <item><description>Format tier labels and column layouts</description></item>
///   <item><description>Create ASCII node boxes</description></item>
///   <item><description>Format headers and branch labels</description></item>
/// </list>
/// <para>
/// Configuration is loaded from <c>config/ability-tree-display.json</c>.
/// </para>
/// </remarks>
public class TreeLayoutRenderer
{
    private readonly AbilityTreeDisplayConfig _config;
    private readonly ILogger<TreeLayoutRenderer> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="TreeLayoutRenderer"/>.
    /// </summary>
    /// <param name="config">Configuration for display settings. If null, uses defaults.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public TreeLayoutRenderer(
        AbilityTreeDisplayConfig? config = null,
        ILogger<TreeLayoutRenderer>? logger = null)
    {
        _config = config ?? AbilityTreeDisplayConfig.CreateDefault();
        _logger = logger ?? NullLogger<TreeLayoutRenderer>.Instance;

        _logger.LogDebug(
            "TreeLayoutRenderer initialized with NodeWidth={NodeWidth}, NodeHeight={NodeHeight}, ColumnSpacing={ColumnSpacing}",
            _config.NodeWidth,
            _config.NodeHeight,
            _config.ColumnSpacing);
    }

    /// <summary>
    /// Calculates screen positions for all nodes in the tree.
    /// </summary>
    /// <param name="treeDto">The tree display data.</param>
    /// <returns>A dictionary mapping node IDs to screen positions.</returns>
    /// <remarks>
    /// <para>
    /// Positions are calculated based on tier number (X) and definition Y (vertical order).
    /// Nodes within each tier are sorted by definition Y before position assignment.
    /// </para>
    /// </remarks>
    public virtual Dictionary<string, NodeScreenPosition> CalculateNodePositions(TreeDisplayDto treeDto)
    {
        ArgumentNullException.ThrowIfNull(treeDto);

        var positions = new Dictionary<string, NodeScreenPosition>();

        _logger.LogDebug(
            "Calculating positions for tree '{TreeId}' with {TierCount} tiers",
            treeDto.TreeId,
            treeDto.Tiers.Count);

        foreach (var tier in treeDto.Tiers)
        {
            var tierX = CalculateTierColumnX(tier.TierNumber);

            // Sort nodes by their Y position from the definition
            var sortedNodes = tier.Nodes.OrderBy(n => n.DefinitionY).ToList();

            for (var i = 0; i < sortedNodes.Count; i++)
            {
                var node = sortedNodes[i];
                var nodeY = CalculateNodeY(i);

                positions[node.NodeId] = new NodeScreenPosition(tierX, nodeY);

                _logger.LogDebug(
                    "Node '{NodeId}' positioned at ({X}, {Y})",
                    node.NodeId,
                    tierX,
                    nodeY);
            }
        }

        _logger.LogDebug("Calculated {Count} node positions", positions.Count);
        return positions;
    }

    /// <summary>
    /// Renders a tier column and returns the formatted string.
    /// </summary>
    /// <param name="tier">The tier display data.</param>
    /// <returns>The formatted tier column string.</returns>
    /// <remarks>
    /// The column includes tier label, underline, and vertically stacked node boxes.
    /// </remarks>
    public virtual string RenderTierColumn(TierDisplayDto tier)
    {
        ArgumentNullException.ThrowIfNull(tier);

        var lines = new List<string>
        {
            // Tier label
            GetTierLabel(tier.TierNumber),
            new string('─', GetTierLabel(tier.TierNumber).Length),
            string.Empty
        };

        // Nodes sorted by definition Y
        foreach (var node in tier.Nodes.OrderBy(n => n.DefinitionY))
        {
            var nodeBox = FormatNodeBox(node, _config.NodeWidth, _config.NodeHeight);
            lines.AddRange(nodeBox.Split('\n'));
            lines.Add(string.Empty); // Spacing between nodes
        }

        _logger.LogDebug("Rendered tier column for TIER {TierNumber} with {NodeCount} nodes",
            tier.TierNumber,
            tier.Nodes.Count);

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Gets the formatted tier label.
    /// </summary>
    /// <param name="tierNumber">The tier number (1-based).</param>
    /// <returns>The formatted tier label string (e.g., "TIER 1").</returns>
    public virtual string GetTierLabel(int tierNumber)
    {
        return $"TIER {tierNumber}";
    }

    /// <summary>
    /// Calculates the total width of the tree display.
    /// </summary>
    /// <param name="maxTier">The maximum tier number in the tree.</param>
    /// <returns>The total width in characters.</returns>
    /// <remarks>
    /// Width = Padding + (Tiers × NodeWidth) + ((Tiers - 1) × ColumnSpacing) + Padding
    /// </remarks>
    public virtual int GetTreeWidth(int maxTier)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTier);

        var width = (_config.Padding * 2) +
                    (maxTier * _config.NodeWidth) +
                    ((maxTier - 1) * _config.ColumnSpacing);

        _logger.LogDebug("Calculated tree width: {Width} for {MaxTier} tiers", width, maxTier);
        return width;
    }

    /// <summary>
    /// Formats a node box with the node name.
    /// </summary>
    /// <param name="node">The node layout data.</param>
    /// <param name="width">The box width.</param>
    /// <param name="height">The box height (typically 3 lines).</param>
    /// <returns>The formatted node box string.</returns>
    /// <remarks>
    /// <para>
    /// Creates an ASCII box with borders and centered node name.
    /// Long names are split across multiple content lines.
    /// </para>
    /// <example>
    /// <code>
    /// ┌─────────┐
    /// │  Power  │
    /// │  Strike │
    /// └─────────┘
    /// </code>
    /// </example>
    /// </remarks>
    public virtual string FormatNodeBox(NodeLayoutDto node, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 3);

        var innerWidth = width - 2; // Account for borders

        var topBorder = $"┌{new string('─', innerWidth)}┐";
        var bottomBorder = $"└{new string('─', innerWidth)}┘";

        // Split name across multiple lines if needed
        var nameLines = SplitNodeName(node.NodeName, innerWidth - 2);
        var contentLines = new List<string>();

        // Pad to fill height (height - 2 for borders)
        var contentHeight = height - 2;
        var paddingLines = contentHeight - nameLines.Count;
        var topPadding = paddingLines / 2;
        var bottomPadding = paddingLines - topPadding;

        for (var i = 0; i < topPadding; i++)
        {
            contentLines.Add(FormatContentLine(string.Empty, innerWidth));
        }

        foreach (var nameLine in nameLines)
        {
            contentLines.Add(FormatContentLine(nameLine, innerWidth));
        }

        for (var i = 0; i < bottomPadding; i++)
        {
            contentLines.Add(FormatContentLine(string.Empty, innerWidth));
        }

        var lines = new List<string> { topBorder };
        lines.AddRange(contentLines);
        lines.Add(bottomBorder);

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Formats the tree header with the tree name.
    /// </summary>
    /// <param name="treeName">The tree display name.</param>
    /// <param name="totalWidth">The total display width.</param>
    /// <returns>The formatted header string (centered, uppercase).</returns>
    /// <remarks>
    /// The header is converted to uppercase and centered within the available width.
    /// </remarks>
    public virtual string FormatTreeHeader(string treeName, int totalWidth)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(treeName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(totalWidth);

        var upperName = treeName.ToUpperInvariant();
        var availableWidth = totalWidth - 4; // Account for border characters

        if (upperName.Length > availableWidth)
        {
            upperName = upperName[..availableWidth];
        }

        var padding = (availableWidth - upperName.Length) / 2;
        var result = upperName.PadLeft(padding + upperName.Length).PadRight(availableWidth);

        _logger.LogDebug("Formatted tree header: '{Header}' (width={Width})", result.Trim(), result.Length);
        return result;
    }

    /// <summary>
    /// Formats a branch header.
    /// </summary>
    /// <param name="branchName">The branch display name.</param>
    /// <returns>The formatted branch header string (e.g., "[ BERSERKER ]").</returns>
    /// <remarks>
    /// Branch headers use bracket notation and uppercase for emphasis.
    /// </remarks>
    public virtual string FormatBranchHeader(string branchName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(branchName);
        return $"[ {branchName.ToUpperInvariant()} ]";
    }

    #region Private Helper Methods

    /// <summary>
    /// Calculates the X coordinate for a tier column.
    /// </summary>
    /// <param name="tierNumber">The tier number (1-based).</param>
    /// <returns>The X coordinate for the tier column.</returns>
    private int CalculateTierColumnX(int tierNumber)
    {
        // Tier 1 starts at StartX + Padding
        // Each subsequent tier adds NodeWidth + ColumnSpacing
        return _config.StartX +
               _config.Padding +
               ((tierNumber - 1) * (_config.NodeWidth + _config.ColumnSpacing));
    }

    /// <summary>
    /// Calculates the Y coordinate for a node based on its index within the tier.
    /// </summary>
    /// <param name="nodeIndex">The node's index after sorting (0-based).</param>
    /// <returns>The Y coordinate for the node.</returns>
    private int CalculateNodeY(int nodeIndex)
    {
        // Start after header, tier label, and spacing
        var baseY = _config.StartY + _config.NodeAreaStartRow;
        return baseY + (nodeIndex * (_config.NodeHeight + _config.NodeVerticalSpacing));
    }

    /// <summary>
    /// Splits a node name across multiple lines if it exceeds max width.
    /// </summary>
    /// <param name="name">The node name to split.</param>
    /// <param name="maxWidth">Maximum width per line.</param>
    /// <returns>List of name lines.</returns>
    private static List<string> SplitNodeName(string name, int maxWidth)
    {
        var lines = new List<string>();

        if (name.Length <= maxWidth)
        {
            lines.Add(name);
            return lines;
        }

        // Try to split at spaces
        var words = name.Split(' ');
        var currentLine = string.Empty;

        foreach (var word in words)
        {
            if (string.IsNullOrEmpty(currentLine))
            {
                currentLine = word;
            }
            else if (currentLine.Length + 1 + word.Length <= maxWidth)
            {
                currentLine += " " + word;
            }
            else
            {
                lines.Add(currentLine);
                currentLine = word;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine);
        }

        return lines;
    }

    /// <summary>
    /// Formats a content line for a node box, centered within the width.
    /// </summary>
    /// <param name="content">The content to center.</param>
    /// <param name="width">The inner width of the box.</param>
    /// <returns>The formatted line with borders.</returns>
    private static string FormatContentLine(string content, int width)
    {
        var centeredContent = content.Length >= width
            ? content[..width]
            : content.PadLeft((width + content.Length) / 2).PadRight(width);

        return $"│{centeredContent}│";
    }

    #endregion
}
