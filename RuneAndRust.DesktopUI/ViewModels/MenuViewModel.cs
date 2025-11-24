using RuneAndRust.DesktopUI.Services;
using System;
using System.Windows.Input;
using ReactiveUI;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// View model for the main menu screen.
/// Allows starting a new game, loading saved games, and accessing settings.
/// </summary>
public class MenuViewModel : ViewModelBase
{
    private readonly INavigationService? _navigationService;
    private readonly ISpriteService? _spriteService;
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
    public string Version => "v0.43.2 - Sprite System & Asset Pipeline";

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
        ExitCommand = ReactiveCommand.Create(OnExit);
    }

    /// <summary>
    /// Initializes a new instance with navigation support.
    /// </summary>
    public MenuViewModel(INavigationService navigationService, ISpriteService spriteService) : this()
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _spriteService = spriteService ?? throw new ArgumentNullException(nameof(spriteService));
    }

    private void OnNewGame()
    {
        // Placeholder - will navigate to character creation in later specs
        Console.WriteLine("[MENU] New Game selected");
    }

    private void OnContinueGame()
    {
        // Placeholder - will load most recent save in later specs
        Console.WriteLine("[MENU] Continue Game selected");
    }

    private void OnLoadGame()
    {
        // Placeholder - will show save browser in v0.43.19
        Console.WriteLine("[MENU] Load Game selected");
    }

    private void OnSettings()
    {
        // Placeholder - will show settings in v0.43.18
        Console.WriteLine("[MENU] Settings selected");
    }

    private void OnAchievements()
    {
        // Placeholder - will show achievements in v0.43.15
        Console.WriteLine("[MENU] Achievements selected");
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

    private void OnExit()
    {
        // Placeholder - will exit application
        Console.WriteLine("[MENU] Exit selected");
        Environment.Exit(0);
    }
}
