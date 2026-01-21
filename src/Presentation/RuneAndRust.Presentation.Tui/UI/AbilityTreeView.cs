// ═══════════════════════════════════════════════════════════════════════════════
// AbilityTreeView.cs
// Renders the ability tree visualization with hierarchical nodes by tier.
// Version: 0.13.2a
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Renders the ability tree visualization showing hierarchical nodes
/// organized by tiers in a column-based layout.
/// </summary>
/// <remarks>
/// <para>
/// The ability tree view displays the tree structure with clear tier
/// separation. Node states, prerequisite lines, and tooltips are handled
/// by separate components (v0.13.2b-d).
/// </para>
/// <para>
/// Key responsibilities:
/// </para>
/// <list type="bullet">
///   <item><description>Render complete tree with header and tier columns</description></item>
///   <item><description>Display nodes in column-based tier layout</description></item>
///   <item><description>Handle keyboard navigation selection</description></item>
///   <item><description>Draw tree border and branch headers</description></item>
/// </list>
/// </remarks>
public class AbilityTreeView
{
    private readonly TreeLayoutRenderer _renderer;
    private readonly ITerminalService _terminalService;
    private readonly AbilityTreeDisplayConfig _config;
    private readonly ILogger<AbilityTreeView> _logger;

    private TreeDisplayDto? _currentTree;
    private Dictionary<string, NodeScreenPosition> _nodePositions = new();
    private (int TierIndex, int NodeIndex) _selectedPosition;

    /// <summary>
    /// Creates a new instance of the AbilityTreeView.
    /// </summary>
    /// <param name="renderer">The renderer for layout calculations.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for display settings. If null, uses defaults.</param>
    /// <param name="logger">Logger for diagnostic output. If null, uses NullLogger.</param>
    /// <exception cref="ArgumentNullException">Thrown when renderer or terminalService is null.</exception>
    public AbilityTreeView(
        TreeLayoutRenderer renderer,
        ITerminalService terminalService,
        AbilityTreeDisplayConfig? config = null,
        ILogger<AbilityTreeView>? logger = null)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? AbilityTreeDisplayConfig.CreateDefault();
        _logger = logger ?? NullLogger<AbilityTreeView>.Instance;

        _logger.LogDebug("AbilityTreeView initialized");
    }

    /// <summary>
    /// Renders the complete ability tree display.
    /// </summary>
    /// <param name="treeDto">The tree display data.</param>
    /// <exception cref="ArgumentNullException">Thrown when treeDto is null.</exception>
    /// <remarks>
    /// Renders in order: border, header, node positions, tier columns, branch headers.
    /// </remarks>
    public void RenderTree(TreeDisplayDto treeDto)
    {
        _currentTree = treeDto ?? throw new ArgumentNullException(nameof(treeDto));

        _logger.LogInformation(
            "Rendering ability tree '{TreeName}' with {TierCount} tiers",
            treeDto.TreeName,
            treeDto.Tiers.Count);

        // Clear previous display
        ClearTreeArea();

        // Render tree border and header
        RenderTreeBorder();
        SetTreeHeader(treeDto.TreeName);

        // Calculate node positions
        _nodePositions = _renderer.CalculateNodePositions(treeDto);

        // Render tier columns
        RenderTiers(treeDto.Tiers);

        // Render branch headers if multiple branches
        if (treeDto.Branches.Count > 1)
        {
            RenderBranchHeaders(treeDto.Branches);
            _logger.LogDebug("Rendered {BranchCount} branch headers", treeDto.Branches.Count);
        }

        _logger.LogDebug(
            "Completed rendering tree '{TreeId}' with {NodeCount} nodes",
            treeDto.TreeId,
            _nodePositions.Count);
    }

    /// <summary>
    /// Renders the tier columns with their labels and nodes.
    /// </summary>
    /// <param name="tiers">The tier display data collection.</param>
    /// <exception cref="ArgumentNullException">Thrown when tiers is null.</exception>
    public void RenderTiers(IReadOnlyList<TierDisplayDto> tiers)
    {
        ArgumentNullException.ThrowIfNull(tiers);

        foreach (var tier in tiers)
        {
            RenderTierColumn(tier);
        }

        _logger.LogDebug("Rendered {TierCount} tier columns", tiers.Count);
    }

    /// <summary>
    /// Sets the tree header with the tree name.
    /// </summary>
    /// <param name="treeName">The display name of the ability tree.</param>
    /// <exception cref="ArgumentException">Thrown when treeName is null or whitespace.</exception>
    public void SetTreeHeader(string treeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(treeName);

        var header = _renderer.FormatTreeHeader(treeName, _config.TotalWidth);

        _terminalService.WriteColoredAt(
            _config.StartX + 1,
            _config.StartY + 1,
            header,
            _config.Colors.HeaderColor);

        _logger.LogDebug("Set tree header: {TreeName}", treeName);
    }

    /// <summary>
    /// Handles keyboard navigation selection within the tree.
    /// </summary>
    /// <param name="tierIndex">The tier column index (0-based).</param>
    /// <param name="nodeIndex">The node index within the tier (0-based).</param>
    /// <remarks>
    /// Updates the visual highlight state when selection changes.
    /// Invalid selections are ignored with debug logging.
    /// </remarks>
    public void HandleSelection(int tierIndex, int nodeIndex)
    {
        if (_currentTree == null)
        {
            _logger.LogWarning("Cannot handle selection - no tree rendered");
            return;
        }

        // Validate selection bounds
        if (tierIndex < 0 || tierIndex >= _currentTree.Tiers.Count)
        {
            _logger.LogDebug("Selection tier index {TierIndex} out of bounds", tierIndex);
            return;
        }

        var tier = _currentTree.Tiers[tierIndex];
        if (nodeIndex < 0 || nodeIndex >= tier.Nodes.Count)
        {
            _logger.LogDebug("Selection node index {NodeIndex} out of bounds for tier {TierIndex}", nodeIndex, tierIndex);
            return;
        }

        // Update selection
        var previousSelection = _selectedPosition;
        _selectedPosition = (tierIndex, nodeIndex);

        // Redraw previous selection (unhighlighted)
        if (previousSelection != _selectedPosition)
        {
            RedrawNodeAtPosition(previousSelection.TierIndex, previousSelection.NodeIndex, highlighted: false);
        }

        // Draw new selection (highlighted)
        RedrawNodeAtPosition(tierIndex, nodeIndex, highlighted: true);

        _logger.LogDebug("Selection moved to tier {TierIndex}, node {NodeIndex}", tierIndex, nodeIndex);
    }

    /// <summary>
    /// Gets the currently selected node ID.
    /// </summary>
    /// <returns>The node ID of the selected node, or null if none selected.</returns>
    public string? GetSelectedNodeId()
    {
        if (_currentTree == null) return null;
        if (_selectedPosition.TierIndex >= _currentTree.Tiers.Count) return null;

        var tier = _currentTree.Tiers[_selectedPosition.TierIndex];
        if (_selectedPosition.NodeIndex >= tier.Nodes.Count) return null;

        return tier.Nodes[_selectedPosition.NodeIndex].NodeId;
    }

    /// <summary>
    /// Gets the currently selected position.
    /// </summary>
    /// <returns>A tuple of (TierIndex, NodeIndex).</returns>
    public (int TierIndex, int NodeIndex) GetSelectedPosition() => _selectedPosition;

    /// <summary>
    /// Clears the ability tree display from the terminal.
    /// </summary>
    public void Clear()
    {
        _currentTree = null;
        _nodePositions.Clear();
        _selectedPosition = (0, 0);

        ClearTreeArea();

        _logger.LogDebug("Cleared ability tree display");
    }

    #region Private Rendering Methods

    /// <summary>
    /// Clears the entire tree display area.
    /// </summary>
    private void ClearTreeArea()
    {
        var clearLine = new string(' ', _config.TotalWidth);
        for (var row = 0; row < _config.TotalHeight; row++)
        {
            _terminalService.WriteAt(_config.StartX, _config.StartY + row, clearLine);
        }
    }

    /// <summary>
    /// Renders the tree border frame.
    /// </summary>
    private void RenderTreeBorder()
    {
        var topBorder = "┌" + new string('─', _config.TotalWidth - 2) + "┐";
        var bottomBorder = "└" + new string('─', _config.TotalWidth - 2) + "┘";
        var divider = "├" + new string('─', _config.TotalWidth - 2) + "┤";

        // Top border
        _terminalService.WriteColoredAt(
            _config.StartX,
            _config.StartY,
            topBorder,
            _config.Colors.BorderColor);

        // Header divider
        _terminalService.WriteColoredAt(
            _config.StartX,
            _config.StartY + 2,
            divider,
            _config.Colors.BorderColor);

        // Bottom border
        _terminalService.WriteColoredAt(
            _config.StartX,
            _config.StartY + _config.TotalHeight - 1,
            bottomBorder,
            _config.Colors.BorderColor);

        // Side borders
        for (var row = 1; row < _config.TotalHeight - 1; row++)
        {
            _terminalService.WriteColoredAt(
                _config.StartX,
                _config.StartY + row,
                "│",
                _config.Colors.BorderColor);
            _terminalService.WriteColoredAt(
                _config.StartX + _config.TotalWidth - 1,
                _config.StartY + row,
                "│",
                _config.Colors.BorderColor);
        }

        _logger.LogDebug("Rendered tree border");
    }

    /// <summary>
    /// Renders a single tier column.
    /// </summary>
    /// <param name="tier">The tier to render.</param>
    private void RenderTierColumn(TierDisplayDto tier)
    {
        var tierX = CalculateTierColumnX(tier.TierNumber);
        var tierLabelY = _config.StartY + _config.TierLabelRow;

        // Render tier label
        var tierLabel = _renderer.GetTierLabel(tier.TierNumber);
        _terminalService.WriteColoredAt(tierX, tierLabelY, tierLabel, _config.Colors.TierLabelColor);

        // Render underline
        var underline = new string('─', tierLabel.Length);
        _terminalService.WriteAt(tierX, tierLabelY + 1, underline);

        // Render nodes in this tier
        foreach (var node in tier.Nodes)
        {
            RenderNodePlaceholder(node);
        }
    }

    /// <summary>
    /// Renders a node box at its calculated position.
    /// </summary>
    /// <param name="node">The node to render.</param>
    private void RenderNodePlaceholder(NodeLayoutDto node)
    {
        if (!_nodePositions.TryGetValue(node.NodeId, out var screenPos))
        {
            _logger.LogWarning("No screen position calculated for node {NodeId}", node.NodeId);
            return;
        }

        var nodeBox = _renderer.FormatNodeBox(node, _config.NodeWidth, _config.NodeHeight);
        var lines = nodeBox.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            _terminalService.WriteColoredAt(
                screenPos.X,
                screenPos.Y + i,
                lines[i],
                _config.Colors.NodeBorderColor);
        }
    }

    /// <summary>
    /// Renders branch headers for multi-branch trees.
    /// </summary>
    /// <param name="branches">The branches to render.</param>
    private void RenderBranchHeaders(IReadOnlyList<BranchDisplayDto> branches)
    {
        var branchY = _config.StartY + _config.BranchHeaderRow;

        foreach (var branch in branches)
        {
            var branchX = CalculateBranchHeaderX(branch);
            var branchHeader = _renderer.FormatBranchHeader(branch.BranchName);

            _terminalService.WriteColoredAt(
                branchX,
                branchY,
                branchHeader,
                _config.Colors.BranchHeaderColor);
        }
    }

    /// <summary>
    /// Redraws a node with optional highlight.
    /// </summary>
    /// <param name="tierIndex">The tier index.</param>
    /// <param name="nodeIndex">The node index within the tier.</param>
    /// <param name="highlighted">Whether to highlight the node.</param>
    private void RedrawNodeAtPosition(int tierIndex, int nodeIndex, bool highlighted)
    {
        if (_currentTree == null) return;
        if (tierIndex >= _currentTree.Tiers.Count) return;

        var tier = _currentTree.Tiers[tierIndex];
        if (nodeIndex >= tier.Nodes.Count) return;

        var node = tier.Nodes[nodeIndex];
        if (!_nodePositions.TryGetValue(node.NodeId, out var screenPos)) return;

        var color = highlighted
            ? _config.Colors.SelectedNodeColor
            : _config.Colors.NodeBorderColor;

        var nodeBox = _renderer.FormatNodeBox(node, _config.NodeWidth, _config.NodeHeight);
        var lines = nodeBox.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            _terminalService.WriteColoredAt(screenPos.X, screenPos.Y + i, lines[i], color);
        }
    }

    /// <summary>
    /// Calculates the X coordinate for a tier column.
    /// </summary>
    /// <param name="tierNumber">The tier number (1-based).</param>
    /// <returns>The X coordinate.</returns>
    private int CalculateTierColumnX(int tierNumber)
    {
        return _config.StartX +
               _config.Padding +
               ((tierNumber - 1) * (_config.NodeWidth + _config.ColumnSpacing));
    }

    /// <summary>
    /// Calculates the X coordinate for a branch header.
    /// </summary>
    /// <param name="branch">The branch.</param>
    /// <returns>The X coordinate.</returns>
    private int CalculateBranchHeaderX(BranchDisplayDto branch)
    {
        // Branch header X is based on the start position + padding
        // Future: could be aligned with first node of the branch
        return _config.StartX + _config.Padding;
    }

    #endregion
}
