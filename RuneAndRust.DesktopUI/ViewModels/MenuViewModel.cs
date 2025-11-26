using RuneAndRust.DesktopUI.Controllers;
using RuneAndRust.DesktopUI.Services;
using System;
using System.Windows.Input;
using ReactiveUI;
using Serilog;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// v0.44.1: View model for the main menu screen.
/// Delegates game flow operations to MainMenuController.
/// </summary>
public class MenuViewModel : ViewModelBase
{
    private readonly MainMenuController? _controller;
    private readonly INavigationService? _navigationService;
    private readonly ISpriteService? _spriteService;
    private readonly IMetaProgressionService? _metaProgressionService;
    private readonly IEndgameService? _endgameService;
    private readonly IConfigurationService? _configurationService;
    private readonly IAudioService? _audioService;
    private readonly ISaveGameService? _saveGameService;
    private readonly IDialogService? _dialogService;
    private readonly ILogger? _logger;

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
    public string Version => "v0.44.1 - Game Flow Integration";

    /// <summary>
    /// Whether there is a saved game to continue.
    /// </summary>
    public bool HasSavedGame => _controller?.HasSavedGame() ?? _saveGameService?.GetMostRecentSave() != null;

    /// <summary>
    /// Design-time constructor.
    /// </summary>
    public MenuViewModel()
    {
        NewGameCommand = ReactiveCommand.CreateFromTask(OnNewGameAsync);
        ContinueGameCommand = ReactiveCommand.CreateFromTask(OnContinueGameAsync);
        LoadGameCommand = ReactiveCommand.Create(OnLoadGame);
        SettingsCommand = ReactiveCommand.Create(OnSettings);
        AchievementsCommand = ReactiveCommand.Create(OnAchievements);
        SpriteDemoCommand = ReactiveCommand.Create(OnSpriteDemo);
        EndgameCommand = ReactiveCommand.Create(OnEndgame);
        ExitCommand = ReactiveCommand.CreateFromTask(OnExitAsync);
    }

    /// <summary>
    /// Initializes with MainMenuController (preferred constructor for v0.44.1+).
    /// </summary>
    public MenuViewModel(
        MainMenuController controller,
        INavigationService navigationService,
        ISpriteService spriteService,
        IMetaProgressionService metaProgressionService,
        IEndgameService endgameService,
        IConfigurationService configurationService,
        IAudioService audioService,
        ISaveGameService saveGameService,
        IDialogService dialogService,
        ILogger logger) : this()
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _spriteService = spriteService ?? throw new ArgumentNullException(nameof(spriteService));
        _metaProgressionService = metaProgressionService ?? throw new ArgumentNullException(nameof(metaProgressionService));
        _endgameService = endgameService ?? throw new ArgumentNullException(nameof(endgameService));
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _saveGameService = saveGameService ?? throw new ArgumentNullException(nameof(saveGameService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Legacy constructor without controller (for backwards compatibility).
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

    private async Task OnNewGameAsync()
    {
        if (_controller != null)
        {
            // Use controller (v0.44.1+)
            await _controller.OnNewGameAsync();
        }
        else if (_navigationService != null)
        {
            // Fallback to legacy behavior - use factory-based navigation
            _navigationService.NavigateTo<CharacterCreationViewModel>();
            _logger?.Information("[MENU] Navigated to Character Creation (legacy path)");
        }
    }

    private async Task OnContinueGameAsync()
    {
        if (_controller != null)
        {
            // Use controller (v0.44.1+)
            await _controller.OnContinueGameAsync();
        }
        else if (_saveGameService != null && _navigationService != null)
        {
            // Fallback to legacy behavior
            var mostRecent = _saveGameService.GetMostRecentSave();
            if (mostRecent != null)
            {
                _logger?.Information("Loading most recent save: {SaveName}", mostRecent.SaveName);
                var success = await _saveGameService.LoadGameAsync(mostRecent.FileName);
                if (success)
                {
                    _logger?.Information("Continue game loaded successfully");
                }
            }
        }
    }

    private void OnLoadGame()
    {
        if (_controller != null)
        {
            _controller.OnLoadGame();
        }
        else if (_navigationService != null && _saveGameService != null && _dialogService != null)
        {
            var saveLoadViewModel = new SaveLoadViewModel(_saveGameService, _dialogService, _navigationService);
            _navigationService.NavigateTo(saveLoadViewModel);
        }
    }

    private void OnSettings()
    {
        if (_controller != null)
        {
            _controller.OnSettings();
        }
        else if (_navigationService != null && _configurationService != null && _audioService != null)
        {
            var settingsViewModel = new SettingsViewModel(_configurationService, _audioService, _navigationService);
            _navigationService.NavigateTo(settingsViewModel);
        }
    }

    private void OnAchievements()
    {
        if (_controller != null)
        {
            _controller.OnAchievements();
        }
        else if (_navigationService != null && _metaProgressionService != null)
        {
            var metaProgressionViewModel = new MetaProgressionViewModel(_metaProgressionService, _navigationService);
            _navigationService.NavigateTo(metaProgressionViewModel);
        }
    }

    private void OnSpriteDemo()
    {
        if (_controller != null)
        {
            _controller.OnSpriteDemo();
        }
        else if (_navigationService != null && _spriteService != null)
        {
            var spriteDemoViewModel = new SpriteDemoViewModel(_spriteService, _navigationService);
            _navigationService.NavigateTo(spriteDemoViewModel);
        }
    }

    private void OnEndgame()
    {
        if (_controller != null)
        {
            _controller.OnEndgame();
        }
        else if (_navigationService != null && _endgameService != null)
        {
            var endgameModeViewModel = new EndgameModeViewModel(_endgameService, _navigationService);
            _navigationService.NavigateTo(endgameModeViewModel);
        }
    }

    private async Task OnExitAsync()
    {
        if (_controller != null)
        {
            await _controller.OnQuitAsync();
        }
        else
        {
            Environment.Exit(0);
        }
    }
}
