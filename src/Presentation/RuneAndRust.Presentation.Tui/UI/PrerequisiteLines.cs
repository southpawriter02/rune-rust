// ═══════════════════════════════════════════════════════════════════════════════
// PrerequisiteLines.cs
// Renders prerequisite connection lines between ability tree nodes.
// Version: 0.13.2c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Enums;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Renders prerequisite connection lines between ability tree nodes.
/// </summary>
/// <remarks>
/// <para>Lines show the dependency relationships between nodes:</para>
/// <list type="bullet">
///   <item><description>Satisfied prerequisites: solid green lines (──────)</description></item>
///   <item><description>Unsatisfied prerequisites: dashed gray lines (─ ─ ─)</description></item>
/// </list>
/// <para>Integrates with <see cref="TreeNodeRenderer"/> to use node positions.</para>
/// </remarks>
/// <example>
/// <code>
/// var lines = new PrerequisiteLines(renderer, terminal, config, logger);
/// lines.SetNodePositions(positions, bounds);
/// lines.RenderConnections(nodes, unlockedNodeIds);
/// </code>
/// </example>
public class PrerequisiteLines
{
    private readonly LineRenderer _lineRenderer;
    private readonly ITerminalService _terminalService;
    private readonly PrerequisiteLinesConfig _config;
    private readonly ILogger<PrerequisiteLines>? _logger;

    private Dictionary<string, NodeScreenPosition> _nodePositions = new();
    private Dictionary<string, NodeBounds> _nodeBounds = new();
    private readonly List<PrerequisiteLineDto> _renderedLines = [];

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new PrerequisiteLines component.
    /// </summary>
    /// <param name="lineRenderer">The renderer for line formatting.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for line display settings.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when lineRenderer or terminalService is null.
    /// </exception>
    public PrerequisiteLines(
        LineRenderer lineRenderer,
        ITerminalService terminalService,
        IOptions<PrerequisiteLinesConfig>? config = null,
        ILogger<PrerequisiteLines>? logger = null)
    {
        _lineRenderer = lineRenderer ?? throw new ArgumentNullException(nameof(lineRenderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config?.Value ?? PrerequisiteLinesConfig.CreateDefault();
        _logger = logger;

        _logger?.LogDebug("PrerequisiteLines component initialized");
    }

    // ═══════════════════════════════════════════════════════════════
    // NODE POSITION MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the node positions for line routing.
    /// </summary>
    /// <param name="positions">Dictionary mapping node IDs to screen positions.</param>
    /// <param name="bounds">Dictionary mapping node IDs to node bounds.</param>
    /// <remarks>
    /// Call this method after rendering nodes but before rendering connections.
    /// </remarks>
    public void SetNodePositions(
        Dictionary<string, NodeScreenPosition> positions,
        Dictionary<string, NodeBounds> bounds)
    {
        _nodePositions = positions ?? throw new ArgumentNullException(nameof(positions));
        _nodeBounds = bounds ?? throw new ArgumentNullException(nameof(bounds));

        _logger?.LogDebug(
            "Set {PositionCount} node positions and {BoundsCount} node bounds",
            _nodePositions.Count, _nodeBounds.Count);
    }

    /// <summary>
    /// Sets node positions from a TreeNodeRenderer.
    /// </summary>
    /// <param name="nodeRenderer">The tree node renderer to get positions from.</param>
    /// <param name="nodeConfig">Node display config for bounds calculation.</param>
    public void SetNodePositionsFromRenderer(
        TreeNodeRenderer nodeRenderer,
        NodeStateDisplayConfig nodeConfig)
    {
        ArgumentNullException.ThrowIfNull(nodeRenderer);
        ArgumentNullException.ThrowIfNull(nodeConfig);

        _nodePositions.Clear();
        _nodeBounds.Clear();

        // This would iterate through the renderer's tracked positions
        // For now, we rely on SetNodePositions being called with explicit data
        _logger?.LogDebug("Node positions set from TreeNodeRenderer");
    }

    // ═══════════════════════════════════════════════════════════════
    // LINE RENDERING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders all prerequisite connection lines for the ability tree.
    /// </summary>
    /// <param name="nodes">All nodes in the ability tree.</param>
    /// <param name="unlockedNodeIds">Set of node IDs the player has unlocked.</param>
    /// <remarks>
    /// <para>Iterates through all nodes with prerequisites and draws lines:</para>
    /// <list type="bullet">
    ///   <item><description>Clears previously rendered lines</description></item>
    ///   <item><description>Determines state for each prerequisite</description></item>
    ///   <item><description>Draws line from prerequisite to dependent node</description></item>
    /// </list>
    /// </remarks>
    public void RenderConnections(
        IEnumerable<AbilityTreeNode> nodes,
        IReadOnlySet<string> unlockedNodeIds)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(unlockedNodeIds);

        // Clear any previously rendered lines
        ClearLines();

        var linesRendered = 0;

        foreach (var node in nodes)
        {
            if (!node.HasPrerequisites)
            {
                continue;
            }

            foreach (var prerequisiteNodeId in node.PrerequisiteNodeIds)
            {
                var lineState = GetLineState(prerequisiteNodeId, unlockedNodeIds);
                DrawLine(prerequisiteNodeId, node.NodeId, lineState);
                linesRendered++;
            }
        }

        _logger?.LogInformation(
            "Rendered {LineCount} prerequisite lines",
            linesRendered);
    }

    /// <summary>
    /// Determines the line state based on prerequisite satisfaction.
    /// </summary>
    /// <param name="prerequisiteNodeId">The required node ID.</param>
    /// <param name="unlockedNodeIds">Set of node IDs the player has unlocked.</param>
    /// <returns>The line state (Satisfied or Unsatisfied).</returns>
    /// <remarks>
    /// A prerequisite is satisfied if the player has unlocked the required node.
    /// </remarks>
    public LineState GetLineState(string prerequisiteNodeId, IReadOnlySet<string> unlockedNodeIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prerequisiteNodeId);
        ArgumentNullException.ThrowIfNull(unlockedNodeIds);

        // A prerequisite is satisfied if the player has unlocked the required node
        var isUnlocked = unlockedNodeIds.Contains(prerequisiteNodeId);

        if (isUnlocked)
        {
            _logger?.LogDebug(
                "Prerequisite '{NodeId}' is satisfied",
                prerequisiteNodeId);
            return LineState.Satisfied;
        }

        _logger?.LogDebug(
            "Prerequisite '{NodeId}' is unsatisfied",
            prerequisiteNodeId);
        return LineState.Unsatisfied;
    }

    /// <summary>
    /// Draws a line between two nodes with the specified state.
    /// </summary>
    /// <param name="sourceNodeId">The prerequisite (source) node ID.</param>
    /// <param name="targetNodeId">The dependent (target) node ID.</param>
    /// <param name="state">The line state.</param>
    /// <remarks>
    /// <para>Line endpoints are calculated from node bounds:</para>
    /// <list type="bullet">
    ///   <item><description>Start: Right edge of source node (center Y)</description></item>
    ///   <item><description>End: Left edge of target node (center Y)</description></item>
    /// </list>
    /// </remarks>
    public void DrawLine(string sourceNodeId, string targetNodeId, LineState state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceNodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetNodeId);

        // Get source node position
        if (!_nodePositions.TryGetValue(sourceNodeId, out var sourcePos))
        {
            _logger?.LogWarning(
                "No position found for source node '{NodeId}'",
                sourceNodeId);
            return;
        }

        // Get target node position
        if (!_nodePositions.TryGetValue(targetNodeId, out var targetPos))
        {
            _logger?.LogWarning(
                "No position found for target node '{NodeId}'",
                targetNodeId);
            return;
        }

        // Get node bounds for edge calculation
        var sourceBounds = _nodeBounds.GetValueOrDefault(sourceNodeId);
        var targetBounds = _nodeBounds.GetValueOrDefault(targetNodeId);

        // Calculate line endpoints at node edges
        var startX = sourceBounds != null
            ? sourceBounds.X + sourceBounds.Width
            : sourcePos.X + _config.DefaultNodeWidth;
        var startY = sourceBounds != null
            ? sourceBounds.Y + (sourceBounds.Height / 2)
            : sourcePos.Y + 1;

        var endX = targetBounds != null
            ? targetBounds.X
            : targetPos.X;
        var endY = targetBounds != null
            ? targetBounds.Y + (targetBounds.Height / 2)
            : targetPos.Y + 1;

        _logger?.LogDebug(
            "Drawing line from ({StartX}, {StartY}) to ({EndX}, {EndY})",
            startX, startY, endX, endY);

        // Calculate path segments
        var path = _lineRenderer.CalculatePath(
            new LinePoint(startX, startY),
            new LinePoint(endX, endY));

        // Get line color
        var color = _lineRenderer.GetLineColor(state);

        // Draw the line
        var pathList = path.ToList();
        foreach (var segment in pathList)
        {
            var displayChar = state == LineState.Unsatisfied && ShouldSkipForDash(segment.Index)
                ? ' '
                : _lineRenderer.GetLineCharacter(segment.Direction);

            _terminalService.WriteColoredAt(
                segment.X,
                segment.Y,
                displayChar.ToString(),
                color);
        }

        // Track rendered line for clearing
        _renderedLines.Add(new PrerequisiteLineDto(
            SourceNodeId: sourceNodeId,
            TargetNodeId: targetNodeId,
            State: state,
            Path: pathList));

        _logger?.LogDebug(
            "Drew {State} line from '{Source}' to '{Target}' with {SegmentCount} segments",
            state, sourceNodeId, targetNodeId, pathList.Count);
    }

    /// <summary>
    /// Clears all rendered prerequisite lines.
    /// </summary>
    public void ClearLines()
    {
        var count = _renderedLines.Count;

        foreach (var line in _renderedLines)
        {
            foreach (var segment in line.Path)
            {
                _terminalService.WriteAt(segment.X, segment.Y, " ");
            }
        }

        _renderedLines.Clear();

        _logger?.LogDebug("Cleared {Count} prerequisite lines", count);
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the count of rendered lines.
    /// </summary>
    /// <returns>The number of currently rendered lines.</returns>
    public int GetRenderedLineCount() => _renderedLines.Count;

    /// <summary>
    /// Gets all rendered prerequisite line DTOs.
    /// </summary>
    /// <returns>Read-only list of rendered lines.</returns>
    public IReadOnlyList<PrerequisiteLineDto> GetRenderedLines() =>
        _renderedLines.AsReadOnly();

    /// <summary>
    /// Gets the current configuration.
    /// </summary>
    /// <returns>The prerequisite lines configuration.</returns>
    public PrerequisiteLinesConfig GetConfig() => _config;

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines if a segment should be skipped for dashed line effect.
    /// </summary>
    /// <param name="index">The segment index.</param>
    /// <returns>True if the segment should be a space; false otherwise.</returns>
    private static bool ShouldSkipForDash(int index)
    {
        // Create dashed effect by skipping every other character
        return index % 2 == 1;
    }
}

