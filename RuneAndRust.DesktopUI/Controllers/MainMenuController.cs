using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.DesktopUI.ViewModels;
using Serilog;

namespace RuneAndRust.DesktopUI.Controllers;

/// <summary>
/// v0.44.1: Controller for main menu operations.
/// Handles New Game, Continue, Load Game, and other menu actions.
/// </summary>
public class MainMenuController
{
    private readonly ILogger _logger;
    private readonly INavigationService _navigationService;
    private readonly ISaveGameService _saveGameService;
    private readonly GameStateController _gameStateController;

    public MainMenuController(
        ILogger logger,
        INavigationService navigationService,
        ISaveGameService saveGameService,
        GameStateController gameStateController)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _saveGameService = saveGameService ?? throw new ArgumentNullException(nameof(saveGameService));
        _gameStateController = gameStateController ?? throw new ArgumentNullException(nameof(gameStateController));
    }

    /// <summary>
    /// Starts a new game, initializing game state and navigating to character creation.
    /// </summary>
    public async Task OnNewGameAsync()
    {
        _logger.Information("New Game started");

        try
        {
            // Get next run number from save service
            var runNumber = await GetNextRunNumberAsync();

            // Create new game state
            var gameState = GameState.CreateNew(runNumber);

            _logger.Information("Created new game state: SessionId={SessionId}, RunNumber={RunNumber}",
                gameState.SessionId, gameState.RunNumber);

            // Initialize with GameStateController
            _gameStateController.InitializeNewGame(gameState);

            // Navigate to character creation
            _navigationService.NavigateTo<CharacterCreationViewModel>();

            _logger.Information("Navigated to character creation");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error starting new game");
            throw;
        }
    }

    /// <summary>
    /// Continues the most recent saved game.
    /// </summary>
    public async Task OnContinueGameAsync()
    {
        _logger.Information("Continue Game selected");

        try
        {
            // Get most recent save
            var mostRecentSave = _saveGameService.GetMostRecentSave();

            if (mostRecentSave == null)
            {
                _logger.Warning("No save games found, starting new game instead");
                await OnNewGameAsync();
                return;
            }

            _logger.Information("Loading most recent save: {SaveName} ({CharacterName})",
                mostRecentSave.SaveName, mostRecentSave.CharacterName);

            // Load the game
            var success = await _saveGameService.LoadGameAsync(mostRecentSave.FileName);

            if (!success)
            {
                _logger.Error("Failed to load save: {SaveName}", mostRecentSave.SaveName);
                return;
            }

            // For now, navigate to exploration view
            // In full implementation, would navigate based on saved phase
            _navigationService.NavigateTo<DungeonExplorationViewModel>();

            _logger.Information("Continue game loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error continuing game");
            throw;
        }
    }

    /// <summary>
    /// Opens the load game browser.
    /// </summary>
    public void OnLoadGame()
    {
        _logger.Information("Load Game selected");
        _navigationService.NavigateTo<SaveLoadViewModel>();
    }

    /// <summary>
    /// Opens the settings menu.
    /// </summary>
    public void OnSettings()
    {
        _logger.Information("Settings selected");
        _navigationService.NavigateTo<SettingsViewModel>();
    }

    /// <summary>
    /// Opens the achievements/meta-progression view.
    /// </summary>
    public void OnAchievements()
    {
        _logger.Information("Achievements selected");
        _navigationService.NavigateTo<MetaProgressionViewModel>();
    }

    /// <summary>
    /// Opens the endgame mode selection.
    /// </summary>
    public void OnEndgame()
    {
        _logger.Information("Endgame modes selected");
        _navigationService.NavigateTo<EndgameModeViewModel>();
    }

    /// <summary>
    /// Opens the sprite demo view.
    /// </summary>
    public void OnSpriteDemo()
    {
        _logger.Information("Sprite Demo selected");
        _navigationService.NavigateTo<SpriteDemoViewModel>();
    }

    /// <summary>
    /// Quits the application, ensuring any active game is saved.
    /// </summary>
    public async Task OnQuitAsync()
    {
        _logger.Information("Quit selected");

        try
        {
            // Auto-save if there's an active game
            if (_gameStateController.HasActiveGame)
            {
                _logger.Information("Saving active game before quit...");
                await _gameStateController.AutoSaveAsync();
            }

            // Shutdown the application
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during quit");
            // Still try to shutdown even if save failed
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
    }

    /// <summary>
    /// Checks if there is a saved game to continue.
    /// </summary>
    public bool HasSavedGame()
    {
        return _saveGameService.GetMostRecentSave() != null;
    }

    /// <summary>
    /// Gets the next run number for a new game.
    /// </summary>
    private async Task<int> GetNextRunNumberAsync()
    {
        try
        {
            // Count existing saves to determine run number
            var saves = _saveGameService.GetAllSaveFiles();
            return saves.Count + 1;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Could not determine run number, defaulting to 1");
            return 1;
        }
    }
}
