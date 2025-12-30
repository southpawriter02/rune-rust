using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Terminal.Controllers;

/// <summary>
/// Handles input for the Specialization UI ("Tree of Runes").
/// Manages navigation between specializations and nodes, and triggers unlock actions.
/// </summary>
/// <remarks>See: v0.4.1c (The Tree of Runes) for implementation.</remarks>
public class SpecializationController
{
    private readonly ISpecializationService _specService;
    private readonly ILogger<SpecializationController> _logger;

    private int _specCount;
    private int _nodeCount;

    /// <summary>
    /// Gets the current view mode (SpecList or TreeDetail).
    /// </summary>
    public SpecializationViewMode ViewMode { get; private set; } = SpecializationViewMode.SpecList;

    /// <summary>
    /// Gets the currently selected specialization index.
    /// </summary>
    public int SelectedSpecIndex { get; private set; }

    /// <summary>
    /// Gets the currently selected node index within the tree.
    /// </summary>
    public int SelectedNodeIndex { get; private set; }

    /// <summary>
    /// Gets the last status message from an unlock attempt.
    /// </summary>
    public string? LastStatusMessage { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecializationController"/> class.
    /// </summary>
    /// <param name="specService">The specialization service for unlock operations.</param>
    /// <param name="logger">The logger for traceability.</param>
    public SpecializationController(
        ISpecializationService specService,
        ILogger<SpecializationController> logger)
    {
        _specService = specService;
        _logger = logger;
    }

    /// <summary>
    /// Updates internal counts for navigation bounds.
    /// Call this before HandleInput when data changes.
    /// </summary>
    /// <param name="specCount">Total number of specializations.</param>
    /// <param name="nodeCount">Total number of nodes in current tree.</param>
    public void UpdateCounts(int specCount, int nodeCount)
    {
        _specCount = specCount;
        _nodeCount = nodeCount;
    }

    /// <summary>
    /// Processes a key input and returns the resulting game phase.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <param name="character">The character for unlock operations.</param>
    /// <param name="specIds">List of specialization IDs in display order.</param>
    /// <param name="nodeIds">List of node IDs in display order.</param>
    /// <returns>The game phase to transition to.</returns>
    public async Task<GamePhase> HandleInputAsync(ConsoleKey key, Character character,
        IReadOnlyList<Guid> specIds, IReadOnlyList<Guid> nodeIds)
    {
        _logger.LogTrace("[SpecController] HandleInputAsync: Key={Key}, ViewMode={Mode}", key, ViewMode);
        LastStatusMessage = null;

        switch (key)
        {
            case ConsoleKey.Escape:
            case ConsoleKey.Q:
                _logger.LogDebug("[SpecController] Exit requested");
                return GamePhase.Exploration;

            case ConsoleKey.UpArrow:
            case ConsoleKey.W:
                NavigateUp();
                return GamePhase.SpecializationMenu;

            case ConsoleKey.DownArrow:
            case ConsoleKey.S:
                NavigateDown();
                return GamePhase.SpecializationMenu;

            case ConsoleKey.LeftArrow:
            case ConsoleKey.A:
                SwitchToSpecList();
                return GamePhase.SpecializationMenu;

            case ConsoleKey.RightArrow:
            case ConsoleKey.D:
                SwitchToTreeDetail();
                return GamePhase.SpecializationMenu;

            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                await HandleUnlockAsync(character, specIds, nodeIds);
                return GamePhase.SpecializationMenu;

            default:
                return GamePhase.SpecializationMenu;
        }
    }

    /// <summary>
    /// Navigates up in the current list (spec list or node list).
    /// </summary>
    private void NavigateUp()
    {
        if (ViewMode == SpecializationViewMode.SpecList)
        {
            if (SelectedSpecIndex > 0)
            {
                SelectedSpecIndex--;
                SelectedNodeIndex = 0; // Reset node selection when changing spec
                _logger.LogDebug("[SpecController] SpecList navigate up: Index={Idx}", SelectedSpecIndex);
            }
        }
        else
        {
            if (SelectedNodeIndex > 0)
            {
                SelectedNodeIndex--;
                _logger.LogDebug("[SpecController] TreeDetail navigate up: Index={Idx}", SelectedNodeIndex);
            }
        }
    }

    /// <summary>
    /// Navigates down in the current list (spec list or node list).
    /// </summary>
    private void NavigateDown()
    {
        if (ViewMode == SpecializationViewMode.SpecList)
        {
            if (SelectedSpecIndex < _specCount - 1)
            {
                SelectedSpecIndex++;
                SelectedNodeIndex = 0;
                _logger.LogDebug("[SpecController] SpecList navigate down: Index={Idx}", SelectedSpecIndex);
            }
        }
        else
        {
            if (SelectedNodeIndex < _nodeCount - 1)
            {
                SelectedNodeIndex++;
                _logger.LogDebug("[SpecController] TreeDetail navigate down: Index={Idx}", SelectedNodeIndex);
            }
        }
    }

    /// <summary>
    /// Switches focus to the specialization list (left panel).
    /// </summary>
    private void SwitchToSpecList()
    {
        ViewMode = SpecializationViewMode.SpecList;
        _logger.LogDebug("[SpecController] Switched to SpecList view");
    }

    /// <summary>
    /// Switches focus to the tree detail (right panel).
    /// </summary>
    private void SwitchToTreeDetail()
    {
        ViewMode = SpecializationViewMode.TreeDetail;
        _logger.LogDebug("[SpecController] Switched to TreeDetail view");
    }

    /// <summary>
    /// Handles unlock action based on current view mode.
    /// </summary>
    private async Task HandleUnlockAsync(Character character,
        IReadOnlyList<Guid> specIds, IReadOnlyList<Guid> nodeIds)
    {
        if (ViewMode == SpecializationViewMode.SpecList)
        {
            // Unlock specialization
            if (SelectedSpecIndex < specIds.Count)
            {
                var specId = specIds[SelectedSpecIndex];
                _logger.LogDebug("[SpecController] Attempting to unlock specialization: {SpecId}", specId);

                var result = await _specService.UnlockSpecializationAsync(character, specId);

                if (result.Success)
                {
                    LastStatusMessage = $"Unlocked {result.SpecializationName}! (-{result.PpSpent} PP)";
                    _logger.LogInformation("[SpecController] Specialization unlocked: {Name}", result.SpecializationName);
                }
                else
                {
                    LastStatusMessage = result.Message;
                    _logger.LogWarning("[SpecController] Unlock failed: {Reason}", result.Message);
                }
            }
        }
        else
        {
            // Unlock node
            if (SelectedNodeIndex < nodeIds.Count)
            {
                var nodeId = nodeIds[SelectedNodeIndex];
                _logger.LogDebug("[SpecController] Attempting to unlock node: {NodeId}", nodeId);

                var result = await _specService.UnlockNodeAsync(character, nodeId);

                if (result.Success)
                {
                    LastStatusMessage = $"Unlocked {result.NodeName}! (-{result.PpSpent} PP)";
                    _logger.LogInformation("[SpecController] Node unlocked: {Name} (Tier {Tier})",
                        result.NodeName, result.Tier);
                }
                else
                {
                    LastStatusMessage = result.Message;
                    _logger.LogWarning("[SpecController] Node unlock failed: {Reason}", result.Message);
                }
            }
        }
    }

    /// <summary>
    /// Resets controller state to initial values.
    /// </summary>
    public void Reset()
    {
        ViewMode = SpecializationViewMode.SpecList;
        SelectedSpecIndex = 0;
        SelectedNodeIndex = 0;
        LastStatusMessage = null;
        _logger.LogDebug("[SpecController] State reset");
    }
}
