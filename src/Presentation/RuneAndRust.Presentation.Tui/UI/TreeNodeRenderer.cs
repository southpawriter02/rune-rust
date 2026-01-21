// ═══════════════════════════════════════════════════════════════════════════════
// TreeNodeRenderer.cs
// UI component for rendering ability tree nodes with state-specific styling.
// Version: 0.13.2b
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Enums;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// UI component for rendering ability tree nodes with state-specific styling.
/// </summary>
/// <remarks>
/// <para>TreeNodeRenderer handles the visual representation of ability tree nodes:</para>
/// <list type="bullet">
///   <item><description>Determines node state (Locked, Available, Unlocked) based on player progress</description></item>
///   <item><description>Renders nodes with state-specific indicators and colors</description></item>
///   <item><description>Manages node highlighting for selection</description></item>
///   <item><description>Tracks node screen positions for prerequisite line drawing</description></item>
/// </list>
/// <para>Depends on <see cref="NodeStateRenderer"/> for formatting operations.</para>
/// </remarks>
/// <example>
/// <code>
/// var renderer = new TreeNodeRenderer(stateRenderer, terminal, talentService, 
///     prereqValidator, config, logger);
/// 
/// var dto = new NodeStateDisplayDto("power-strike", "Power Strike", 
///     NodeState.Available, 0, 3, 1, 1, "berserker");
/// renderer.RenderNode(dto, 10, 5);
/// </code>
/// </example>
public class TreeNodeRenderer
{
    private readonly NodeStateRenderer _stateRenderer;
    private readonly ITerminalService _terminalService;
    private readonly ITalentPointService? _talentPointService;
    private readonly IPrerequisiteValidator? _prerequisiteValidator;
    private readonly NodeStateDisplayConfig _config;
    private readonly ILogger<TreeNodeRenderer>? _logger;

    // Node position tracking for prerequisite line drawing
    private readonly Dictionary<string, NodeScreenPosition> _nodePositions = new();

    // Highlighted nodes set for visual distinction
    private readonly HashSet<string> _highlightedNodes = new();

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new TreeNodeRenderer with the specified dependencies.
    /// </summary>
    /// <param name="stateRenderer">Renderer for node state formatting.</param>
    /// <param name="terminalService">Terminal service for output operations.</param>
    /// <param name="talentPointService">Optional service for talent point operations.</param>
    /// <param name="prerequisiteValidator">Optional validator for prerequisites.</param>
    /// <param name="config">Configuration for display settings.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public TreeNodeRenderer(
        NodeStateRenderer stateRenderer,
        ITerminalService terminalService,
        ITalentPointService? talentPointService = null,
        IPrerequisiteValidator? prerequisiteValidator = null,
        IOptions<NodeStateDisplayConfig>? config = null,
        ILogger<TreeNodeRenderer>? logger = null)
    {
        _stateRenderer = stateRenderer ?? throw new ArgumentNullException(nameof(stateRenderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _talentPointService = talentPointService;
        _prerequisiteValidator = prerequisiteValidator;
        _config = config?.Value ?? NodeStateDisplayConfig.CreateDefault();
        _logger = logger;

        _logger?.LogDebug(
            "TreeNodeRenderer initialized with node dimensions: {Width}x{Height}",
            _config.NodeWidth, _config.NodeHeight);
    }

    // ═══════════════════════════════════════════════════════════════
    // STATE DETERMINATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines the current state of a node based on player progress.
    /// </summary>
    /// <param name="node">The ability tree node definition.</param>
    /// <param name="player">The player to check progress for.</param>
    /// <param name="unlockedNodeIds">Set of node IDs the player has unlocked.</param>
    /// <param name="nodeRanks">Dictionary of node ranks (nodeId -> currentRank).</param>
    /// <returns>The determined node state.</returns>
    /// <remarks>
    /// <para>State determination logic:</para>
    /// <list type="number">
    ///   <item><description>If player has ranks in node → Unlocked</description></item>
    ///   <item><description>If prerequisites met → Available</description></item>
    ///   <item><description>Otherwise → Locked</description></item>
    /// </list>
    /// </remarks>
    public NodeState DetermineNodeState(
        AbilityTreeNode node,
        Player player,
        IReadOnlySet<string> unlockedNodeIds,
        IReadOnlyDictionary<string, int> nodeRanks)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(unlockedNodeIds);
        ArgumentNullException.ThrowIfNull(nodeRanks);

        // Check if already unlocked (has at least one rank)
        if (nodeRanks.TryGetValue(node.NodeId, out var currentRank) && currentRank > 0)
        {
            _logger?.LogDebug(
                "Node '{NodeId}' is Unlocked with rank {Rank}/{MaxRank}",
                node.NodeId, currentRank, node.MaxRank);
            return NodeState.Unlocked;
        }

        // Check if prerequisites are met
        var prerequisitesMet = CheckPrerequisitesMet(node, player, unlockedNodeIds);

        if (prerequisitesMet)
        {
            _logger?.LogDebug(
                "Node '{NodeId}' is Available (prerequisites met)",
                node.NodeId);
            return NodeState.Available;
        }

        _logger?.LogDebug(
            "Node '{NodeId}' is Locked (prerequisites not met)",
            node.NodeId);
        return NodeState.Locked;
    }

    /// <summary>
    /// Checks if prerequisites are met for a node.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <param name="player">The player to check for.</param>
    /// <param name="unlockedNodeIds">Set of unlocked node IDs.</param>
    /// <returns>True if all prerequisites are met; otherwise, false.</returns>
    private bool CheckPrerequisitesMet(
        AbilityTreeNode node,
        Player player,
        IReadOnlySet<string> unlockedNodeIds)
    {
        // Use validator if available
        if (_prerequisiteValidator is not null)
        {
            var result = _prerequisiteValidator.ValidatePrerequisites(player, node);
            return result.IsValid;
        }

        // Fallback: Check node prerequisites manually
        if (!node.HasPrerequisites)
            return true;

        // All referenced nodes must be unlocked
        foreach (var prereqId in node.PrerequisiteNodeIds)
        {
            if (!unlockedNodeIds.Contains(prereqId))
            {
                _logger?.LogDebug(
                    "Node '{NodeId}' missing prerequisite '{PrereqId}'",
                    node.NodeId, prereqId);
                return false;
            }
        }

        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    // NODE RENDERING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders a node at the specified screen position.
    /// </summary>
    /// <param name="dto">The node state display data.</param>
    /// <param name="x">The X coordinate (column).</param>
    /// <param name="y">The Y coordinate (row).</param>
    /// <remarks>
    /// <para>The node is rendered as a box with:</para>
    /// <list type="bullet">
    ///   <item><description>State indicator (e.g., "[x]")</description></item>
    ///   <item><description>Node name (truncated if needed)</description></item>
    ///   <item><description>Rank progress for multi-rank nodes</description></item>
    /// </list>
    /// <para>Position is tracked for prerequisite line drawing.</para>
    /// </remarks>
    public void RenderNode(NodeStateDisplayDto dto, int x, int y)
    {
        ArgumentNullException.ThrowIfNull(dto);

        _logger?.LogDebug(
            "Rendering node '{NodeId}' at ({X}, {Y}) with state {State}",
            dto.NodeId, x, y, dto.State);

        // Track the node position for line drawing
        _nodePositions[dto.NodeId] = new NodeScreenPosition(x, y);

        // Determine colors
        var isHighlighted = _highlightedNodes.Contains(dto.NodeId);
        var nodeColor = isHighlighted
            ? _stateRenderer.GetHighlightColor()
            : _stateRenderer.GetStateColor(dto.State);

        // Get formatted content
        var indicator = _stateRenderer.GetStateIndicator(dto.State);
        var truncatedName = _stateRenderer.TruncateName(dto.NodeName, _config.NodeWidth - 4);

        // Render the node box
        RenderNodeBox(x, y, indicator, truncatedName, dto.CurrentRank, dto.MaxRank, nodeColor);

        _logger?.LogDebug(
            "Node '{NodeId}' rendered successfully at ({X}, {Y})",
            dto.NodeId, x, y);
    }

    /// <summary>
    /// Renders the node box with all visual elements.
    /// </summary>
    /// <param name="x">X position.</param>
    /// <param name="y">Y position.</param>
    /// <param name="indicator">State indicator string.</param>
    /// <param name="name">Node name (possibly truncated).</param>
    /// <param name="currentRank">Current rank.</param>
    /// <param name="maxRank">Maximum rank.</param>
    /// <param name="color">Color for the node.</param>
    private void RenderNodeBox(
        int x,
        int y,
        string indicator,
        string name,
        int currentRank,
        int maxRank,
        ConsoleColor color)
    {
        // Build the content line
        var contentLine = $"{indicator} {name}";

        // Add rank progress for multi-rank nodes
        var rankProgress = _stateRenderer.FormatRankProgress(currentRank, maxRank);
        if (!string.IsNullOrEmpty(rankProgress))
        {
            contentLine = $"{indicator} {rankProgress}";
        }

        // Pad to node width
        contentLine = contentLine.PadRight(_config.NodeWidth);
        if (contentLine.Length > _config.NodeWidth)
        {
            contentLine = contentLine[.._config.NodeWidth];
        }

        // Write the content using the terminal service's WriteColoredAt method
        _terminalService.WriteColoredAt(x, y, contentLine, color);
    }


    // ═══════════════════════════════════════════════════════════════
    // HIGHLIGHTING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets or clears highlight state for a node.
    /// </summary>
    /// <param name="nodeId">The node ID to highlight.</param>
    /// <param name="highlighted">True to highlight, false to clear.</param>
    /// <remarks>
    /// Highlighted nodes are rendered with the highlight color instead of
    /// their state color. Call <see cref="RenderNode"/> after setting
    /// highlight to update the visual.
    /// </remarks>
    public void SetHighlight(string nodeId, bool highlighted)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        if (highlighted)
        {
            _highlightedNodes.Add(nodeId);
            _logger?.LogDebug("Node '{NodeId}' highlighted", nodeId);
        }
        else
        {
            _highlightedNodes.Remove(nodeId);
            _logger?.LogDebug("Node '{NodeId}' highlight cleared", nodeId);
        }
    }

    /// <summary>
    /// Clears all node highlights.
    /// </summary>
    public void ClearAllHighlights()
    {
        var count = _highlightedNodes.Count;
        _highlightedNodes.Clear();
        _logger?.LogDebug("Cleared {Count} node highlights", count);
    }

    /// <summary>
    /// Checks if a node is currently highlighted.
    /// </summary>
    /// <param name="nodeId">The node ID to check.</param>
    /// <returns>True if highlighted; otherwise, false.</returns>
    public bool IsHighlighted(string nodeId)
    {
        return _highlightedNodes.Contains(nodeId);
    }

    // ═══════════════════════════════════════════════════════════════
    // POSITION TRACKING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the screen position of a rendered node.
    /// </summary>
    /// <param name="nodeId">The node ID to look up.</param>
    /// <returns>The node's screen position, or null if not rendered.</returns>
    /// <remarks>
    /// Used by prerequisite line renderers to draw connection lines
    /// between nodes.
    /// </remarks>
    public NodeScreenPosition? GetNodeScreenPosition(string nodeId)
    {
        if (_nodePositions.TryGetValue(nodeId, out var position))
        {
            return position;
        }

        _logger?.LogDebug(
            "Node position not found for '{NodeId}'",
            nodeId);
        return null;
    }

    /// <summary>
    /// Gets the screen bounds for a node (top-left and bottom-right corners).
    /// </summary>
    /// <param name="nodeId">The node ID to get bounds for.</param>
    /// <returns>Tuple of (topLeft, bottomRight) positions, or null if not found.</returns>
    public (NodeScreenPosition TopLeft, NodeScreenPosition BottomRight)? GetNodeScreenBounds(string nodeId)
    {
        var position = GetNodeScreenPosition(nodeId);
        if (position is null)
            return null;

        var topLeft = position;
        var bottomRight = new NodeScreenPosition(
            position.X + _config.NodeWidth,
            position.Y + _config.NodeHeight);

        return (topLeft, bottomRight);
    }

    /// <summary>
    /// Clears all tracked node positions.
    /// </summary>
    public void ClearNodePositions()
    {
        var count = _nodePositions.Count;
        _nodePositions.Clear();
        _logger?.LogDebug("Cleared {Count} node positions", count);
    }

    // ═══════════════════════════════════════════════════════════════
    // DTO CREATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a NodeStateDisplayDto from a node definition and player progress.
    /// </summary>
    /// <param name="node">The ability tree node definition.</param>
    /// <param name="player">The player for state determination.</param>
    /// <param name="unlockedNodeIds">Set of unlocked node IDs.</param>
    /// <param name="nodeRanks">Dictionary of node ranks.</param>
    /// <param name="branchId">The branch ID this node belongs to.</param>
    /// <returns>A NodeStateDisplayDto ready for rendering.</returns>
    public NodeStateDisplayDto CreateNodeDisplayDto(
        AbilityTreeNode node,
        Player player,
        IReadOnlySet<string> unlockedNodeIds,
        IReadOnlyDictionary<string, int> nodeRanks,
        string branchId)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(player);

        var state = DetermineNodeState(node, player, unlockedNodeIds, nodeRanks);
        var currentRank = nodeRanks.TryGetValue(node.NodeId, out var rank) ? rank : 0;

        return new NodeStateDisplayDto(
            NodeId: node.NodeId,
            NodeName: node.Name,
            State: state,
            CurrentRank: currentRank,
            MaxRank: node.MaxRank,
            PointCost: node.PointCost,
            Tier: node.Tier,
            BranchId: branchId);
    }

    // ═══════════════════════════════════════════════════════════════
    // CLEAR OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Clears all tracked state (positions, highlights).
    /// </summary>
    public void ClearAll()
    {
        ClearNodePositions();
        ClearAllHighlights();
        _logger?.LogDebug("TreeNodeRenderer state cleared");
    }

    /// <summary>
    /// Gets the current configuration.
    /// </summary>
    /// <returns>The node state display configuration.</returns>
    public NodeStateDisplayConfig GetConfig() => _config;
}
