using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Terminal.Controllers;

/// <summary>
/// Handles input for the Specialization Grid UI.
/// Manages tier-based navigation and node unlock actions.
/// </summary>
/// <remarks>See: v0.4.1d (The Grid) for Specialization UI implementation.</remarks>
public class SpecializationController : ISpecializationController
{
    private readonly ISpecializationService _specService;
    private readonly ISpecializationGridViewModelBuilder _vmBuilder;
    private readonly ILogger<SpecializationController> _logger;

    private Character? _character;
    private SpecializationGridViewModel? _currentVm;

    /// <summary>
    /// Gets the current ViewModel for rendering.
    /// </summary>
    public SpecializationGridViewModel? CurrentViewModel => _currentVm;

    /// <summary>
    /// Gets whether the controller has been initialized.
    /// </summary>
    public bool IsInitialized => _currentVm != null && _character != null;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecializationController"/> class.
    /// </summary>
    /// <param name="specService">The specialization service for unlock operations.</param>
    /// <param name="vmBuilder">The ViewModel builder for grid construction.</param>
    /// <param name="logger">The logger for traceability.</param>
    public SpecializationController(
        ISpecializationService specService,
        ISpecializationGridViewModelBuilder vmBuilder,
        ILogger<SpecializationController> logger)
    {
        _specService = specService;
        _vmBuilder = vmBuilder;
        _logger = logger;
    }

    /// <summary>
    /// Initializes the controller with a character and specialization.
    /// </summary>
    /// <param name="character">The character viewing the specialization.</param>
    /// <param name="specializationId">The specialization to display.</param>
    public async Task InitializeAsync(Character character, Guid specializationId)
    {
        _logger.LogDebug("[SpecController] InitializeAsync: CharName={CharName}, SpecId={SpecId}",
            character.Name, specializationId);

        _character = character;
        _currentVm = await _vmBuilder.BuildAsync(character, specializationId);

        _logger.LogInformation("[SpecController] Initialized with {NodeCount} nodes for {SpecName}",
            _currentVm.TotalNodes, _currentVm.SpecializationName);
    }

    /// <summary>
    /// Processes a key input and returns the resulting game phase.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>The game phase to transition to.</returns>
    public async Task<GamePhase> HandleInputAsync(ConsoleKey key)
    {
        if (!IsInitialized)
        {
            _logger.LogWarning("[SpecController] HandleInputAsync called before initialization");
            return GamePhase.Exploration;
        }

        _logger.LogTrace("[SpecController] HandleInputAsync: Key={Key}", key);

        // Clear previous feedback
        _currentVm!.FeedbackMessage = null;
        _currentVm.FeedbackIsSuccess = false;

        switch (key)
        {
            case ConsoleKey.Escape:
            case ConsoleKey.Q:
                _logger.LogDebug("[SpecController] Exit requested");
                Reset();
                return GamePhase.Exploration;

            case ConsoleKey.UpArrow:
            case ConsoleKey.W:
                NavigateVertical(-1);
                return GamePhase.SpecializationMenu;

            case ConsoleKey.DownArrow:
            case ConsoleKey.S:
                NavigateVertical(1);
                return GamePhase.SpecializationMenu;

            case ConsoleKey.LeftArrow:
            case ConsoleKey.A:
                NavigateHorizontal(-1);
                return GamePhase.SpecializationMenu;

            case ConsoleKey.RightArrow:
            case ConsoleKey.D:
                NavigateHorizontal(1);
                return GamePhase.SpecializationMenu;

            case ConsoleKey.Tab:
                await SwitchSpecializationAsync();
                return GamePhase.SpecializationMenu;

            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                await HandleUnlockAsync();
                return GamePhase.SpecializationMenu;

            default:
                return GamePhase.SpecializationMenu;
        }
    }

    /// <summary>
    /// Navigates vertically within the current tier.
    /// </summary>
    /// <param name="delta">Direction: -1 for up, +1 for down.</param>
    private void NavigateVertical(int delta)
    {
        if (_currentVm == null || _currentVm.AllNodes.Count == 0) return;

        var currentNode = _currentVm.SelectedNode;
        if (currentNode == null)
        {
            _currentVm.SelectedNodeIndex = 0;
            return;
        }

        // Find nodes in the same tier
        var currentTier = currentNode.Tier;
        if (!_currentVm.NodesByTier.TryGetValue(currentTier, out var tierNodes) || tierNodes.Count == 0)
            return;

        // Find current position within tier
        var tierIndex = tierNodes.ToList().FindIndex(n => n.NodeId == currentNode.NodeId);
        if (tierIndex < 0) return;

        // Calculate new position within tier
        var newTierIndex = tierIndex + delta;
        if (newTierIndex < 0 || newTierIndex >= tierNodes.Count) return;

        // Find the global index for the new node
        var targetNode = tierNodes[newTierIndex];
        var globalIndex = _currentVm.AllNodes.ToList().FindIndex(n => n.NodeId == targetNode.NodeId);

        if (globalIndex >= 0)
        {
            _currentVm.SelectedNodeIndex = globalIndex;
            _logger.LogDebug("[SpecController] Navigate vertical: Tier={Tier}, TierIdx={TierIdx}",
                currentTier, newTierIndex);
        }
    }

    /// <summary>
    /// Navigates horizontally between tiers.
    /// </summary>
    /// <param name="delta">Direction: -1 for left (lower tier), +1 for right (higher tier).</param>
    private void NavigateHorizontal(int delta)
    {
        if (_currentVm == null || _currentVm.AllNodes.Count == 0) return;

        var currentNode = _currentVm.SelectedNode;
        if (currentNode == null)
        {
            _currentVm.SelectedNodeIndex = 0;
            return;
        }

        var currentTier = currentNode.Tier;
        var targetTier = currentTier + delta;

        // Clamp to valid tier range (1-4)
        if (targetTier < 1 || targetTier > 4) return;

        // Check if target tier has nodes
        if (!_currentVm.NodesByTier.TryGetValue(targetTier, out var targetTierNodes) || targetTierNodes.Count == 0)
        {
            // Try to find next tier with nodes
            var searchDirection = delta > 0 ? 1 : -1;
            for (var tier = targetTier + searchDirection; tier >= 1 && tier <= 4; tier += searchDirection)
            {
                if (_currentVm.NodesByTier.TryGetValue(tier, out targetTierNodes) && targetTierNodes.Count > 0)
                {
                    targetTier = tier;
                    break;
                }
            }

            if (targetTierNodes == null || targetTierNodes.Count == 0) return;
        }

        // Find the same row position in the target tier, or clamp to available
        var currentTierNodes = _currentVm.NodesByTier.GetValueOrDefault(currentTier);
        var currentRowInTier = currentTierNodes?.ToList().FindIndex(n => n.NodeId == currentNode.NodeId) ?? 0;

        var targetRowIndex = Math.Min(currentRowInTier, targetTierNodes.Count - 1);
        var targetNode = targetTierNodes[targetRowIndex];

        // Find global index
        var globalIndex = _currentVm.AllNodes.ToList().FindIndex(n => n.NodeId == targetNode.NodeId);

        if (globalIndex >= 0)
        {
            _currentVm.SelectedNodeIndex = globalIndex;
            _logger.LogDebug("[SpecController] Navigate horizontal: FromTier={From}, ToTier={To}",
                currentTier, targetTier);
        }
    }

    /// <summary>
    /// Switches to the next unlocked specialization (Tab key).
    /// </summary>
    private async Task SwitchSpecializationAsync()
    {
        if (_character == null || _currentVm == null) return;

        var specIds = _character.UnlockedSpecializationIds;
        if (specIds.Count <= 1)
        {
            _logger.LogTrace("[SpecController] SwitchSpec: Only one spec unlocked, no switch");
            return;
        }

        // Find next spec
        var currentIndex = _currentVm.CurrentSpecIndex;
        var nextIndex = (currentIndex + 1) % specIds.Count;
        var nextSpecId = specIds[nextIndex];

        _logger.LogDebug("[SpecController] Switching spec: {From} -> {To}",
            currentIndex, nextIndex);

        // Rebuild ViewModel for new spec
        _currentVm = await _vmBuilder.BuildAsync(_character, nextSpecId);
    }

    /// <summary>
    /// Handles node unlock (Inscribe) action.
    /// </summary>
    private async Task HandleUnlockAsync()
    {
        if (_character == null || _currentVm == null) return;

        var selectedNode = _currentVm.SelectedNode;
        if (selectedNode == null)
        {
            _currentVm.FeedbackMessage = "No node selected";
            _currentVm.FeedbackIsSuccess = false;
            return;
        }

        // Check if already unlocked
        if (selectedNode.Status == NodeStatus.Unlocked)
        {
            _currentVm.FeedbackMessage = $"{selectedNode.Name} is already inscribed";
            _currentVm.FeedbackIsSuccess = false;
            _logger.LogDebug("[SpecController] Node already unlocked: {Name}", selectedNode.Name);
            return;
        }

        // Check if locked (prerequisites not met)
        if (selectedNode.Status == NodeStatus.Locked)
        {
            _currentVm.FeedbackMessage = $"Prerequisites not met for {selectedNode.Name}";
            _currentVm.FeedbackIsSuccess = false;
            _logger.LogDebug("[SpecController] Node locked: {Name}", selectedNode.Name);
            return;
        }

        // Check if affordable status (prereqs met but insufficient PP)
        if (selectedNode.Status == NodeStatus.Affordable)
        {
            _currentVm.FeedbackMessage = $"Insufficient PP for {selectedNode.Name} (need {selectedNode.CostPP})";
            _currentVm.FeedbackIsSuccess = false;
            _logger.LogDebug("[SpecController] Insufficient PP for: {Name}", selectedNode.Name);
            return;
        }

        // Attempt unlock
        _logger.LogDebug("[SpecController] Attempting to unlock node: {NodeId}", selectedNode.NodeId);

        var result = await _specService.UnlockNodeAsync(_character, selectedNode.NodeId);

        if (result.Success)
        {
            _currentVm.FeedbackMessage = $"Inscribed {result.NodeName}! (-{result.PpSpent} PP)";
            _currentVm.FeedbackIsSuccess = true;
            _logger.LogInformation("[SpecController] Node inscribed: {Name} (Tier {Tier})",
                result.NodeName, result.Tier);

            // Refresh ViewModel to update statuses
            _currentVm = await _vmBuilder.RefreshAsync(_currentVm, _character);
        }
        else
        {
            _currentVm.FeedbackMessage = result.Message;
            _currentVm.FeedbackIsSuccess = false;
            _logger.LogWarning("[SpecController] Node unlock failed: {Reason}", result.Message);
        }
    }

    /// <summary>
    /// Resets controller state to initial values.
    /// </summary>
    public void Reset()
    {
        _character = null;
        _currentVm = null;
        _logger.LogDebug("[SpecController] State reset");
    }
}
