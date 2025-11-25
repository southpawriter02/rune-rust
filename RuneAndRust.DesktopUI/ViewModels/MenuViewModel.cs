using RuneAndRust.DesktopUI.Services;
using System;
using System.Windows.Input;
using ReactiveUI;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// v0.43.21: View model for the main menu screen.
/// Allows starting a new game, loading saved games, and accessing settings.
/// </summary>
public class MenuViewModel : ViewModelBase
{
    private readonly INavigationService? _navigationService;
    private readonly ISpriteService? _spriteService;
    private readonly IMetaProgressionService? _metaProgressionService;
    private readonly IEndgameService? _endgameService;
    private readonly IConfigurationService? _configurationService;
    private readonly IAudioService? _audioService;
    private readonly ISaveGameService? _saveGameService;
    private readonly IDialogService? _dialogService;
    /// <summary>
    /// Command to start a new game.
    /// </summary>
    public ICommand NewGameCommand { get; }

    /// <summary>
    /// Command to continue the most recent saved game.
    /// </summary>
    public ICommand ContinueGameCommand { get; }

    /// <summary>
    /// Command to load a saved game from the browser.
    /// </summary>
    public ICommand LoadGameCommand { get; }

    /// <summary>
    /// Command to open settings.
    /// </summary>
    public ICommand SettingsCommand { get; }

    /// <summary>
    /// Command to view achievements.
    /// </summary>
    public ICommand AchievementsCommand { get; }

    /// <summary>
    /// Command to view sprite demo.
    /// </summary>
    public ICommand SpriteDemoCommand { get; }

    /// <summary>
    /// Command to access endgame modes.
    /// </summary>
    public ICommand EndgameCommand { get; }

    /// <summary>
    /// Command to exit the application.
    /// </summary>
    public ICommand ExitCommand { get; }

    /// <summary>
    /// Gets the application title.
    /// </summary>
    public string Title => "Rune & Rust";

    /// <summary>
    /// Gets the application version.
    /// </summary>
    public string Version => "v0.43.21 - UI Testing & Optimization";

    /// <summary>
    /// Whether there is a saved game to continue.
    /// </summary>
    public bool HasSavedGame => _saveGameService?.GetMostRecentSave() != null;

    public MenuViewModel()
    {
        // Placeholder commands for v0.43.1
        // Full implementation will be added in later specs
        NewGameCommand = ReactiveCommand.Create(OnNewGame);
        ContinueGameCommand = ReactiveCommand.Create(OnContinueGame);
        LoadGameCommand = ReactiveCommand.Create(OnLoadGame);
        SettingsCommand = ReactiveCommand.Create(OnSettings);
        AchievementsCommand = ReactiveCommand.Create(OnAchievements);
        SpriteDemoCommand = ReactiveCommand.Create(OnSpriteDemo);
        EndgameCommand = ReactiveCommand.Create(OnEndgame);
        ExitCommand = ReactiveCommand.Create(OnExit);
    }

    /// <summary>
    /// Initializes a new instance with navigation support.
    /// </summary>
    public MenuViewModel(
        INavigationService navigationService,
        ISpriteService spriteService,
        IMetaProgressionService metaProgressionService,
        IEndgameService endgameService,
        IConfigurationService configurationService,
        IAudioService audioService,
        ISaveGameService saveGameService,
        IDialogService dialogService) : this()
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _spriteService = spriteService ?? throw new ArgumentNullException(nameof(spriteService));
        _metaProgressionService = metaProgressionService ?? throw new ArgumentNullException(nameof(metaProgressionService));
        _endgameService = endgameService ?? throw new ArgumentNullException(nameof(endgameService));
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _saveGameService = saveGameService ?? throw new ArgumentNullException(nameof(saveGameService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    private void OnNewGame()
    {
        // Placeholder - will navigate to character creation in later specs
        Console.WriteLine("[MENU] New Game selected");
    }

    private async void OnContinueGame()
    {
        if (_saveGameService != null && _navigationService != null)
        {
            var mostRecent = _saveGameService.GetMostRecentSave();
            if (mostRecent != null)
            {
                Console.WriteLine($"[MENU] Loading most recent save: {mostRecent.SaveName}");
                var success = await _saveGameService.LoadGameAsync(mostRecent.FileName);
                if (success)
                {
                    // In full implementation, navigate to game view
                    Console.WriteLine("[MENU] Continue game loaded successfully");
                }
                else
                {
                    Console.WriteLine("[MENU] Failed to load continue game");
                }
            }
            else
            {
                Console.WriteLine("[MENU] No saved games to continue");
            }
        }
        else
        {
            Console.WriteLine("[MENU] Continue Game selected (services not available)");
        }
    }

    private void OnLoadGame()
    {
        if (_navigationService != null && _saveGameService != null && _dialogService != null)
        {
            // Navigate to save/load view (v0.43.19)
            var saveLoadViewModel = new SaveLoadViewModel(_saveGameService, _dialogService, _navigationService);
            _navigationService.NavigateTo(saveLoadViewModel);
            Console.WriteLine("[MENU] Navigated to Save/Load");
        }
        else
        {
            Console.WriteLine("[MENU] Load Game selected (navigation not available)");
        }
    }

    private void OnSettings()
    {
        if (_navigationService != null && _configurationService != null && _audioService != null)
        {
            // Navigate to settings view (v0.43.18)
            var settingsViewModel = new SettingsViewModel(_configurationService, _audioService, _navigationService);
            _navigationService.NavigateTo(settingsViewModel);
            Console.WriteLine("[MENU] Navigated to Settings");
        }
        else
        {
            Console.WriteLine("[MENU] Settings selected (navigation not available)");
        }
    }

    private void OnAchievements()
    {
        if (_navigationService != null && _metaProgressionService != null)
        {
            // Navigate to meta-progression view (v0.43.15)
            var metaProgressionViewModel = new MetaProgressionViewModel(_metaProgressionService, _navigationService);
            _navigationService.NavigateTo(metaProgressionViewModel);
            Console.WriteLine("[MENU] Navigated to Meta-Progression & Achievements");
        }
        else
        {
            Console.WriteLine("[MENU] Achievements selected (navigation not available)");
        }
    }

    private void OnSpriteDemo()
    {
        if (_navigationService != null && _spriteService != null)
        {
            // Navigate to sprite demo
            var spriteDemoViewModel = new SpriteDemoViewModel(_spriteService);
            _navigationService.NavigateTo(spriteDemoViewModel);
            Console.WriteLine("[MENU] Navigated to Sprite Demo");
        }
        else
        {
            Console.WriteLine("[MENU] Sprite Demo selected (navigation not available)");
        }
    }

    private void OnEndgame()
    {
        if (_navigationService != null && _endgameService != null)
        {
            // Navigate to endgame mode selection (v0.43.16)
            var endgameModeViewModel = new EndgameModeViewModel(_endgameService, _navigationService);
            _navigationService.NavigateTo(endgameModeViewModel);
            Console.WriteLine("[MENU] Navigated to Endgame Modes");
        }
        else
        {
            Console.WriteLine("[MENU] Endgame selected (navigation not available)");
        }
    }

    private void OnExit()
    {
        // Placeholder - will exit application
        Console.WriteLine("[MENU] Exit selected");
        Environment.Exit(0);
    }
}
