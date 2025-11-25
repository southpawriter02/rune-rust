using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.DesktopUI.ViewModels;
using RuneAndRust.Engine;
using Serilog;

namespace RuneAndRust.DesktopUI.Controllers;

/// <summary>
/// v0.44.3: Stub controller for combat integration.
/// Full implementation in v0.44.4.
/// Handles combat initiation and return to exploration.
/// </summary>
public class CombatController
{
    private readonly ILogger _logger;
    private readonly GameStateController _gameStateController;
    private readonly INavigationService _navigationService;
    private readonly CombatEngine _combatEngine;
    private CombatViewModel? _viewModel;

    /// <summary>
    /// Event raised when combat ends (victory or defeat).
    /// </summary>
    public event EventHandler<CombatEndedEventArgs>? CombatEnded;

    /// <summary>
    /// Event raised when the player flees combat.
    /// </summary>
    public event EventHandler? PlayerFled;

    public CombatController(
        ILogger logger,
        GameStateController gameStateController,
        INavigationService navigationService,
        CombatEngine combatEngine)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameStateController = gameStateController ?? throw new ArgumentNullException(nameof(gameStateController));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _combatEngine = combatEngine ?? throw new ArgumentNullException(nameof(combatEngine));
    }

    /// <summary>
    /// Initializes the controller with the combat view model.
    /// </summary>
    public void Initialize(CombatViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _logger.Debug("CombatController initialized");
    }

    /// <summary>
    /// Starts combat with the given combat state.
    /// </summary>
    public async Task StartCombatAsync(CombatState combatState)
    {
        if (combatState == null)
            throw new ArgumentNullException(nameof(combatState));

        _logger.Information("Combat starting: {EnemyCount} enemies, CanFlee={CanFlee}",
            combatState.Enemies.Count, combatState.CanFlee);

        // Initialize combat engine
        _combatEngine.InitializeCombat(combatState);

        // Navigate to combat view
        _navigationService.NavigateTo<CombatViewModel>();
    }

    /// <summary>
    /// Handles victory in combat.
    /// </summary>
    public async Task OnCombatVictoryAsync()
    {
        if (!_gameStateController.HasActiveGame) return;

        _logger.Information("Combat victory!");

        // End combat state
        _gameStateController.EndCombat();

        // Transition to loot collection or back to exploration
        await _gameStateController.UpdatePhaseAsync(GamePhase.LootCollection, "Combat victory - collecting loot");

        CombatEnded?.Invoke(this, new CombatEndedEventArgs(true, false));
    }

    /// <summary>
    /// Handles player defeat in combat.
    /// </summary>
    public async Task OnCombatDefeatAsync()
    {
        if (!_gameStateController.HasActiveGame) return;

        _logger.Warning("Combat defeat - Survivor has fallen!");

        // End combat state
        _gameStateController.EndCombat();

        // Transition to death phase
        await _gameStateController.UpdatePhaseAsync(GamePhase.Death, "Survivor has fallen in combat");

        CombatEnded?.Invoke(this, new CombatEndedEventArgs(false, false));
    }

    /// <summary>
    /// Handles the player fleeing combat.
    /// </summary>
    public async Task OnFleeAttemptAsync()
    {
        if (!_gameStateController.HasActiveGame) return;

        var combatState = _gameStateController.CurrentGameState.CurrentCombat;
        if (combatState == null || !combatState.CanFlee)
        {
            _logger.Warning("Flee attempt blocked - combat does not allow fleeing");
            return;
        }

        // Simple flee success for now (v0.44.4 will implement proper flee mechanics)
        var random = new Random();
        var fleeSuccess = random.NextDouble() < 0.6; // 60% base flee chance

        if (fleeSuccess)
        {
            _logger.Information("Survivor fled combat successfully");

            _gameStateController.EndCombat();
            await _gameStateController.UpdatePhaseAsync(GamePhase.DungeonExploration, "Fled combat");

            PlayerFled?.Invoke(this, EventArgs.Empty);
            CombatEnded?.Invoke(this, new CombatEndedEventArgs(false, true));
        }
        else
        {
            _logger.Information("Flee attempt failed!");
            // Flee failure - enemies get free attacks (v0.44.4 implementation)
        }
    }

    /// <summary>
    /// Returns to exploration after combat ends.
    /// </summary>
    public void ReturnToExploration()
    {
        _navigationService.NavigateTo<DungeonExplorationViewModel>();
    }
}

/// <summary>
/// Event args for combat ending.
/// </summary>
public class CombatEndedEventArgs : EventArgs
{
    /// <summary>Whether the player won.</summary>
    public bool WasVictory { get; }

    /// <summary>Whether the player fled.</summary>
    public bool PlayerFled { get; }

    public CombatEndedEventArgs(bool wasVictory, bool playerFled)
    {
        WasVictory = wasVictory;
        PlayerFled = playerFled;
    }
}
