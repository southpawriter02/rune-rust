// ═══════════════════════════════════════════════════════════════════════════════
// AbilityTreeCommand.cs
// Handles ability tree user interactions and commands.
// Version: 0.13.2d
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Enums;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.Commands;

/// <summary>
/// Handles ability tree user interactions and commands.
/// </summary>
/// <remarks>
/// <para>Coordinates between the tree view, tooltip, and talent point service
/// to handle node selection, tooltip display, and unlock actions:</para>
/// <list type="bullet">
///   <item><description>Enter: Select node and show tooltip</description></item>
///   <item><description>[U]: Initiate unlock for available nodes</description></item>
///   <item><description>[Y]/[N]: Confirm or cancel unlock</description></item>
///   <item><description>Escape: Close tooltip or cancel confirmation</description></item>
/// </list>
/// <para>This command handler manages the interaction state machine for the ability tree.</para>
/// </remarks>
/// <example>
/// <code>
/// var command = new AbilityTreeCommand(tooltip, pointDisplay, nodeRenderer, logger);
/// command.SetSelectedNode("power-strike");
/// if (command.ProcessKeyInput(ConsoleKey.Enter, tree, unlockedNodes, pointsAvailable))
/// {
///     // Input was handled
/// }
/// </code>
/// </example>
public class AbilityTreeCommand
{
    private readonly NodeTooltip _tooltip;
    private readonly TalentPointDisplay _pointDisplay;
    private readonly NodeStateRenderer _nodeStateRenderer;
    private readonly NodeStateDisplayConfig _nodeConfig;
    private readonly ILogger<AbilityTreeCommand>? _logger;

    private string? _selectedNodeId;
    private AbilityTreeNode? _pendingUnlockNode;
    private AbilityTreeDefinition? _currentTree;
    private IReadOnlySet<string>? _unlockedNodeIds;
    private int _availablePoints;

    // ═══════════════════════════════════════════════════════════════
    // PUBLIC PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the currently selected node ID.
    /// </summary>
    public string? SelectedNodeId => _selectedNodeId;

    /// <summary>
    /// Gets whether the command is awaiting unlock confirmation.
    /// </summary>
    public bool AwaitingConfirmation => _tooltip.AwaitingConfirmation;

    /// <summary>
    /// Gets the pending unlock node (if any).
    /// </summary>
    public AbilityTreeNode? PendingUnlockNode => _pendingUnlockNode;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new instance of the AbilityTreeCommand.
    /// </summary>
    /// <param name="tooltip">The tooltip UI component.</param>
    /// <param name="pointDisplay">The talent point display component.</param>
    /// <param name="nodeStateRenderer">The node state renderer for state determination.</param>
    /// <param name="nodeConfig">The node display configuration.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when required dependencies are null.
    /// </exception>
    public AbilityTreeCommand(
        NodeTooltip tooltip,
        TalentPointDisplay pointDisplay,
        NodeStateRenderer nodeStateRenderer,
        NodeStateDisplayConfig? nodeConfig = null,
        ILogger<AbilityTreeCommand>? logger = null)
    {
        _tooltip = tooltip ?? throw new ArgumentNullException(nameof(tooltip));
        _pointDisplay = pointDisplay ?? throw new ArgumentNullException(nameof(pointDisplay));
        _nodeStateRenderer = nodeStateRenderer ?? throw new ArgumentNullException(nameof(nodeStateRenderer));
        _nodeConfig = nodeConfig ?? NodeStateDisplayConfig.CreateDefault();
        _logger = logger;

        _logger?.LogDebug("AbilityTreeCommand initialized");
    }

    // ═══════════════════════════════════════════════════════════════
    // SETUP METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the context for command processing.
    /// </summary>
    /// <param name="tree">The ability tree definition.</param>
    /// <param name="unlockedNodeIds">Set of unlocked node IDs.</param>
    /// <param name="availablePoints">Available talent points.</param>
    public void SetContext(
        AbilityTreeDefinition tree,
        IReadOnlySet<string> unlockedNodeIds,
        int availablePoints)
    {
        _currentTree = tree ?? throw new ArgumentNullException(nameof(tree));
        _unlockedNodeIds = unlockedNodeIds ?? throw new ArgumentNullException(nameof(unlockedNodeIds));
        _availablePoints = availablePoints;

        _logger?.LogDebug(
            "Set command context: tree={TreeId}, unlocked={UnlockedCount}, points={Points}",
            tree.TreeId, unlockedNodeIds.Count, availablePoints);
    }

    /// <summary>
    /// Sets the selected node ID.
    /// </summary>
    /// <param name="nodeId">The node ID to select.</param>
    public void SetSelectedNode(string? nodeId)
    {
        _selectedNodeId = nodeId;

        _logger?.LogDebug("Selected node set to {NodeId}", nodeId ?? "(none)");
    }

    // ═══════════════════════════════════════════════════════════════
    // INPUT PROCESSING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Processes a key input for the ability tree.
    /// </summary>
    /// <param name="key">The pressed key.</param>
    /// <param name="tree">The ability tree definition.</param>
    /// <param name="unlockedNodeIds">Set of unlocked node IDs.</param>
    /// <param name="availablePoints">Available talent points.</param>
    /// <returns>True if the input was handled, false otherwise.</returns>
    public bool ProcessKeyInput(
        ConsoleKey key,
        AbilityTreeDefinition tree,
        IReadOnlySet<string> unlockedNodeIds,
        int availablePoints)
    {
        // Update context
        SetContext(tree, unlockedNodeIds, availablePoints);

        // If awaiting confirmation, only handle Y/N/Escape
        if (AwaitingConfirmation)
        {
            return HandleConfirmationInput(key);
        }

        var handled = key switch
        {
            ConsoleKey.Enter => HandleNodeSelection(),
            ConsoleKey.U => HandleUnlockAction(),
            ConsoleKey.Escape => HandleEscape(),
            _ => false
        };

        _logger?.LogDebug(
            "Processed key {Key}: handled={Handled}",
            key, handled);

        return handled;
    }

    // ═══════════════════════════════════════════════════════════════
    // NODE SELECTION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Handles node selection (Enter key).
    /// </summary>
    /// <returns>True if handled successfully.</returns>
    public bool HandleNodeSelection()
    {
        if (string.IsNullOrEmpty(_selectedNodeId) || _currentTree == null || _unlockedNodeIds == null)
        {
            _logger?.LogDebug("Cannot select node - missing context or selection");
            return false;
        }

        // Find the node in the tree
        var node = FindNode(_selectedNodeId);
        if (node == null)
        {
            _logger?.LogWarning("Selected node {NodeId} not found in tree", _selectedNodeId);
            return false;
        }

        // Determine node state
        var state = DetermineNodeState(node);
        var currentRank = _unlockedNodeIds.Contains(_selectedNodeId) ? 1 : 0;

        // Create bounds from node position and config
        var bounds = new NodeBounds(
            node.Position.X,
            node.Position.Y,
            _nodeConfig.NodeWidth,
            _nodeConfig.NodeHeight);

        // Show tooltip
        _tooltip.ShowTooltip(node, state, bounds, currentRank);

        // Show requirements if not unlocked
        if (state != NodeState.Unlocked && node.PrerequisiteNodeIds.Count > 0)
        {
            _tooltip.ShowRequirements(node.PrerequisiteNodeIds, _unlockedNodeIds);
        }

        _logger?.LogDebug("Showed tooltip for node {NodeId} with state {State}", _selectedNodeId, state);
        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    // UNLOCK ACTION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Handles the unlock action ([U] key).
    /// </summary>
    /// <returns>True if unlock prompt was shown.</returns>
    public bool HandleUnlockAction()
    {
        if (string.IsNullOrEmpty(_selectedNodeId) || _currentTree == null || _unlockedNodeIds == null)
        {
            _logger?.LogDebug("Cannot unlock - no node selected or missing context");
            return false;
        }

        var node = FindNode(_selectedNodeId);
        if (node == null)
        {
            return false;
        }

        // Determine node state
        var state = DetermineNodeState(node);

        // Can only unlock available nodes
        if (state != NodeState.Available)
        {
            _logger?.LogDebug(
                "Cannot unlock node {NodeId} - state is {State}",
                _selectedNodeId, state);
            return false;
        }

        // Check if enough points
        if (_availablePoints < node.PointCost)
        {
            _logger?.LogDebug(
                "Cannot unlock node {NodeId} - not enough points ({Available} < {Cost})",
                _selectedNodeId, _availablePoints, node.PointCost);
            return false;
        }

        // Show unlock prompt
        _pendingUnlockNode = node;
        _tooltip.ShowUnlockPrompt(node);

        _logger?.LogInformation(
            "Showing unlock prompt for node {NodeId} (cost: {Cost} points)",
            _selectedNodeId, node.PointCost);

        return true;
    }

    /// <summary>
    /// Handles confirmation input ([Y]/[N]).
    /// </summary>
    /// <param name="confirmed">Whether the action was confirmed.</param>
    /// <returns>A result indicating the unlock outcome.</returns>
    public UnlockResult HandleConfirmation(bool confirmed)
    {
        if (_pendingUnlockNode == null)
        {
            return new UnlockResult(false, null, "No pending unlock");
        }

        if (!confirmed)
        {
            // Cancel unlock
            var cancelledNode = _pendingUnlockNode;
            _pendingUnlockNode = null;
            _tooltip.HideUnlockPrompt();

            _logger?.LogDebug("Unlock cancelled by user for node {NodeId}", cancelledNode.NodeId);
            return new UnlockResult(false, cancelledNode.NodeId, "Cancelled by user");
        }

        // Return unlock request for the calling code to execute
        var nodeToUnlock = _pendingUnlockNode;
        _pendingUnlockNode = null;
        _tooltip.HideTooltip();

        _logger?.LogInformation(
            "User confirmed unlock for node {NodeId} (cost: {Cost})",
            nodeToUnlock.NodeId, nodeToUnlock.PointCost);

        return new UnlockResult(true, nodeToUnlock.NodeId, "Confirmed");
    }

    // ═══════════════════════════════════════════════════════════════
    // POINT DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Updates the point display after a successful unlock.
    /// </summary>
    /// <param name="newAvailable">The new available point count.</param>
    /// <param name="newSpent">The new spent point count.</param>
    public void UpdatePointDisplay(int newAvailable, int newSpent)
    {
        _pointDisplay.AnimatePointSpend();
        _pointDisplay.RenderPoints(newAvailable, newSpent);
        _availablePoints = newAvailable;

        _logger?.LogDebug(
            "Updated point display: {Available} available, {Spent} spent",
            newAvailable, newSpent);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Handles confirmation key input.
    /// </summary>
    private bool HandleConfirmationInput(ConsoleKey key)
    {
        var result = key switch
        {
            ConsoleKey.Y => HandleConfirmation(true),
            ConsoleKey.N => HandleConfirmation(false),
            ConsoleKey.Escape => HandleConfirmation(false),
            _ => new UnlockResult(false, null, "Unknown key")
        };

        return result.WasProcessed || key == ConsoleKey.Y || key == ConsoleKey.N || key == ConsoleKey.Escape;
    }

    /// <summary>
    /// Handles the Escape key.
    /// </summary>
    private bool HandleEscape()
    {
        if (_tooltip.IsVisible)
        {
            _tooltip.HideTooltip();
            _logger?.LogDebug("Closed tooltip via Escape");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Finds a node in the current tree by ID.
    /// </summary>
    private AbilityTreeNode? FindNode(string nodeId)
    {
        if (_currentTree == null)
        {
            return null;
        }

        return _currentTree.GetAllNodes().FirstOrDefault(n =>
            string.Equals(n.NodeId, nodeId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determines the node state based on unlock status and prerequisites.
    /// </summary>
    private NodeState DetermineNodeState(AbilityTreeNode node)
    {
        if (_unlockedNodeIds == null)
        {
            return NodeState.Locked;
        }

        // Check if already unlocked
        if (_unlockedNodeIds.Contains(node.NodeId))
        {
            return NodeState.Unlocked;
        }

        // Check if prerequisites are met
        var prereqsMet = node.PrerequisiteNodeIds.All(prereq => _unlockedNodeIds.Contains(prereq));
        if (!prereqsMet)
        {
            return NodeState.Locked;
        }

        // Check if enough points
        if (_availablePoints < node.PointCost)
        {
            return NodeState.Locked;
        }

        return NodeState.Available;
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// UNLOCK RESULT
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Result of an unlock confirmation action.
/// </summary>
/// <param name="WasProcessed">Whether the confirmation was processed (not waiting).</param>
/// <param name="NodeId">The node ID to unlock (if confirmed).</param>
/// <param name="Message">Status message.</param>
public record UnlockResult(bool WasProcessed, string? NodeId, string Message);
