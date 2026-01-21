// ═══════════════════════════════════════════════════════════════════════════════
// NodeTooltip.cs
// Displays ability node information in a tooltip overlay.
// Version: 0.13.2d
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
/// Displays ability node information in a tooltip overlay.
/// </summary>
/// <remarks>
/// <para>Tooltips are triggered by selection (Enter key) since TUI does not support
/// hover events. The tooltip shows ability details, requirements, and status
/// with contextual information based on the node's current state:</para>
/// <list type="bullet">
///   <item><description>Unlocked: Shows rank status</description></item>
///   <item><description>Available: Shows "[U] to unlock" prompt</description></item>
///   <item><description>Locked: Shows unsatisfied prerequisites</description></item>
/// </list>
/// <para>Also handles unlock confirmation prompts with Y/N response.</para>
/// </remarks>
/// <example>
/// <code>
/// var tooltip = new NodeTooltip(renderer, terminal, config, logger);
/// tooltip.ShowTooltip(node, NodeState.Available, nodeBounds);
/// tooltip.ShowUnlockPrompt(node); // After user presses [U]
/// </code>
/// </example>
public class NodeTooltip
{
    private readonly TooltipRenderer _tooltipRenderer;
    private readonly ITerminalService _terminalService;
    private readonly NodeTooltipConfig _config;
    private readonly ILogger<NodeTooltip>? _logger;

    private TooltipDisplayDto? _currentTooltip;
    private TooltipPosition _currentPosition = new(0, 0);
    private List<string> _renderedLines = [];

    // ═══════════════════════════════════════════════════════════════
    // PUBLIC PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the tooltip is currently visible.
    /// </summary>
    public bool IsVisible { get; private set; }

    /// <summary>
    /// Gets whether the tooltip is awaiting unlock confirmation.
    /// </summary>
    public bool AwaitingConfirmation { get; private set; }

    /// <summary>
    /// Gets the current node ID being displayed.
    /// </summary>
    public string? CurrentNodeId { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new instance of the NodeTooltip component.
    /// </summary>
    /// <param name="tooltipRenderer">The renderer for tooltip formatting.</param>
    /// <param name="terminalService">The terminal output service.</param>
    /// <param name="config">Configuration for tooltip display settings.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when tooltipRenderer or terminalService is null.
    /// </exception>
    public NodeTooltip(
        TooltipRenderer tooltipRenderer,
        ITerminalService terminalService,
        IOptions<NodeTooltipConfig>? config = null,
        ILogger<NodeTooltip>? logger = null)
    {
        _tooltipRenderer = tooltipRenderer ?? throw new ArgumentNullException(nameof(tooltipRenderer));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config?.Value ?? NodeTooltipConfig.CreateDefault();
        _logger = logger;

        _logger?.LogDebug("NodeTooltip component initialized");
    }

    // ═══════════════════════════════════════════════════════════════
    // TOOLTIP DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Shows the tooltip for a node.
    /// </summary>
    /// <param name="node">The ability node to display.</param>
    /// <param name="state">The current state of the node.</param>
    /// <param name="nodeBounds">The screen bounds of the selected node.</param>
    /// <param name="currentRank">The current rank for multi-rank abilities.</param>
    public void ShowTooltip(
        AbilityTreeNode node,
        NodeState state,
        NodeBounds nodeBounds,
        int currentRank = 0)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(nodeBounds);

        // Hide any existing tooltip first
        if (IsVisible)
        {
            HideTooltip();
        }

        // Store current node
        CurrentNodeId = node.NodeId;

        // Create tooltip content
        _currentTooltip = _tooltipRenderer.FormatAbilityDetails(node, state, currentRank);

        // Calculate position adjacent to node
        _currentPosition = _tooltipRenderer.GetTooltipPosition(
            nodeBounds,
            _config.TooltipWidth,
            _config.MaxTooltipHeight);

        // Render the tooltip
        RenderTooltip();

        IsVisible = true;
        AwaitingConfirmation = false;

        _logger?.LogDebug(
            "Showed tooltip for node {NodeId} at ({X}, {Y})",
            node.NodeId,
            _currentPosition.X,
            _currentPosition.Y);
    }

    /// <summary>
    /// Hides the currently displayed tooltip.
    /// </summary>
    public void HideTooltip()
    {
        if (!IsVisible)
        {
            return;
        }

        // Clear rendered lines from screen
        ClearTooltip();

        _currentTooltip = null;
        CurrentNodeId = null;
        IsVisible = false;
        AwaitingConfirmation = false;

        _logger?.LogDebug("Hid tooltip");
    }

    /// <summary>
    /// Shows the requirements section in the tooltip.
    /// </summary>
    /// <param name="prerequisites">The prerequisite node IDs.</param>
    /// <param name="unlockedNodeIds">Set of unlocked node IDs for satisfaction checking.</param>
    public void ShowRequirements(
        IReadOnlyList<string> prerequisites,
        IReadOnlySet<string> unlockedNodeIds)
    {
        if (_currentTooltip == null || !IsVisible)
        {
            _logger?.LogDebug("Cannot show requirements - tooltip not visible");
            return;
        }

        ArgumentNullException.ThrowIfNull(prerequisites);
        ArgumentNullException.ThrowIfNull(unlockedNodeIds);

        // Add requirements section to tooltip
        var requirementsSection = _tooltipRenderer.FormatRequirements(prerequisites, unlockedNodeIds);

        // Re-create tooltip with requirements section added
        var sectionsWithReqs = new List<TooltipSectionDto>(_currentTooltip.Sections)
        {
            requirementsSection
        };

        _currentTooltip = _currentTooltip with { Sections = sectionsWithReqs };

        // Re-render with updated content
        ClearTooltip();
        RenderTooltip();

        _logger?.LogDebug("Updated tooltip with {Count} requirements", prerequisites.Count);
    }

    // ═══════════════════════════════════════════════════════════════
    // UNLOCK PROMPT METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Shows the unlock confirmation prompt.
    /// </summary>
    /// <param name="node">The node to unlock.</param>
    public void ShowUnlockPrompt(AbilityTreeNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (!IsVisible)
        {
            _logger?.LogDebug("Cannot show unlock prompt - tooltip not visible");
            return;
        }

        AwaitingConfirmation = true;

        // Render confirmation prompt below tooltip
        var promptY = _currentPosition.Y + _renderedLines.Count + 2; // +2 for borders
        var promptLine = $"Unlock {node.Name} for {node.PointCost} talent points?";
        var confirmLine = "[Y] Yes   [N] No";

        _terminalService.WriteAt(
            _currentPosition.X,
            promptY,
            promptLine);

        _terminalService.WriteColoredAt(
            _currentPosition.X,
            promptY + 1,
            confirmLine,
            _config.PromptColor);

        _logger?.LogDebug(
            "Showed unlock prompt for node {NodeId} (cost: {Cost})",
            node.NodeId,
            node.PointCost);
    }

    /// <summary>
    /// Hides the unlock confirmation prompt.
    /// </summary>
    public void HideUnlockPrompt()
    {
        if (!AwaitingConfirmation)
        {
            return;
        }

        // Clear prompt lines
        var promptY = _currentPosition.Y + _renderedLines.Count + 2;
        var blankLine = new string(' ', _config.TooltipWidth + 10);

        _terminalService.WriteAt(_currentPosition.X, promptY, blankLine);
        _terminalService.WriteAt(_currentPosition.X, promptY + 1, blankLine);

        AwaitingConfirmation = false;

        _logger?.LogDebug("Hid unlock prompt");
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current configuration.
    /// </summary>
    /// <returns>The tooltip configuration.</returns>
    public NodeTooltipConfig GetConfig() => _config;

    /// <summary>
    /// Gets the current tooltip position.
    /// </summary>
    /// <returns>The current tooltip position.</returns>
    public TooltipPosition GetPosition() => _currentPosition;

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the tooltip to the terminal.
    /// </summary>
    private void RenderTooltip()
    {
        if (_currentTooltip == null)
        {
            return;
        }

        _renderedLines = _tooltipRenderer.BuildTooltipLines(
            _currentTooltip,
            _config.TooltipWidth);

        var tooltipWidth = _config.TooltipWidth;
        var topBorder = $"┌{new string('─', tooltipWidth - 2)}┐";
        var bottomBorder = $"└{new string('─', tooltipWidth - 2)}┘";

        // Draw top border
        _terminalService.WriteColoredAt(
            _currentPosition.X,
            _currentPosition.Y,
            topBorder,
            _config.BorderColor);

        // Draw content lines
        for (var i = 0; i < _renderedLines.Count; i++)
        {
            var line = _renderedLines[i];
            var paddedLine = line.PadRight(tooltipWidth - 2);

            _terminalService.WriteColoredAt(
                _currentPosition.X,
                _currentPosition.Y + 1 + i,
                $"│{paddedLine}│",
                _config.BorderColor);
        }

        // Draw bottom border
        _terminalService.WriteColoredAt(
            _currentPosition.X,
            _currentPosition.Y + 1 + _renderedLines.Count,
            bottomBorder,
            _config.BorderColor);

        _logger?.LogDebug(
            "Rendered tooltip with {LineCount} content lines",
            _renderedLines.Count);
    }

    /// <summary>
    /// Clears the tooltip from the terminal.
    /// </summary>
    private void ClearTooltip()
    {
        if (_renderedLines.Count == 0)
        {
            return;
        }

        var blankLine = new string(' ', _config.TooltipWidth);
        var totalHeight = _renderedLines.Count + 2; // +2 for borders

        // Clear tooltip area
        for (var i = 0; i < totalHeight; i++)
        {
            _terminalService.WriteAt(
                _currentPosition.X,
                _currentPosition.Y + i,
                blankLine);
        }

        // Clear confirmation prompt if shown
        if (AwaitingConfirmation)
        {
            var promptY = _currentPosition.Y + totalHeight;
            _terminalService.WriteAt(_currentPosition.X, promptY, blankLine);
            _terminalService.WriteAt(_currentPosition.X, promptY + 1, blankLine);
        }

        _renderedLines.Clear();

        _logger?.LogDebug("Cleared tooltip display");
    }
}
