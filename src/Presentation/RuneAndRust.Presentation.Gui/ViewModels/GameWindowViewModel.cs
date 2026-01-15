namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RuneAndRust.Presentation.Gui.Services;
using Serilog;

/// <summary>
/// View model for the main game window.
/// </summary>
/// <remarks>
/// Manages game window state including:
/// - Window title
/// - Combat mode state
/// - Menu commands (save, load, settings, etc.)
/// </remarks>
public partial class GameWindowViewModel : ViewModelBase
{
    private readonly INavigationService? _navigation;
    private readonly IWindowLayoutService? _layout;

    /// <summary>
    /// Gets or sets the window title.
    /// </summary>
    [ObservableProperty]
    private string _windowTitle = "Rune and Rust";

    /// <summary>
    /// Gets or sets whether the game is currently in combat mode.
    /// </summary>
    /// <remarks>
    /// When true, the combat overlay region becomes visible.
    /// </remarks>
    [ObservableProperty]
    private bool _isInCombat;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameWindowViewModel"/> class.
    /// </summary>
    /// <param name="navigation">The navigation service for window management.</param>
    /// <param name="layout">The layout service for panel management.</param>
    public GameWindowViewModel(
        INavigationService navigation,
        IWindowLayoutService layout)
    {
        _navigation = navigation;
        _layout = layout;
        Log.Debug("GameWindowViewModel initialized with services");
    }

    /// <summary>
    /// Design-time constructor.
    /// </summary>
    public GameWindowViewModel()
    {
        _navigation = null;
        _layout = null;
    }

    /// <summary>
    /// Starts a new game session.
    /// </summary>
    [RelayCommand]
    private void NewGame()
    {
        Log.Information("Starting new game from game window");
        // Will reset game state in future versions
    }

    /// <summary>
    /// Opens a file dialog to load a saved game.
    /// </summary>
    [RelayCommand]
    private async Task LoadGameAsync()
    {
        Log.Information("Loading game from game window");
        var path = await _navigation!.ShowLoadDialogAsync();
        if (path is not null)
        {
            Log.Information("Loading save: {Path}", path);
            // Game loading implementation in future versions
        }
    }

    /// <summary>
    /// Opens a file dialog to save the current game.
    /// </summary>
    [RelayCommand]
    private async Task SaveGameAsync()
    {
        Log.Information("Saving game");
        var path = await _navigation!.ShowSaveDialogAsync();
        if (path is not null)
        {
            Log.Information("Saving to: {Path}", path);
            // Game saving implementation in future versions
        }
    }

    /// <summary>
    /// Opens the settings dialog.
    /// </summary>
    [RelayCommand]
    private void OpenSettings()
    {
        Log.Information("Opening settings from game window");
        _navigation?.ShowDialog<Views.SettingsWindow>();
    }

    /// <summary>
    /// Returns to the main menu.
    /// </summary>
    [RelayCommand]
    private void ReturnToMainMenu()
    {
        Log.Information("Returning to main menu");
        _navigation?.ReturnToMainMenu();
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    [RelayCommand]
    private void Quit()
    {
        Log.Information("Quitting from game window");
        _navigation?.Quit();
    }

    /// <summary>
    /// Shows the about dialog.
    /// </summary>
    [RelayCommand]
    private void ShowAbout()
    {
        Log.Information("Showing about dialog");
        // Simple about info - full dialog in future version
    }

    /// <summary>
    /// Toggles combat mode for testing.
    /// </summary>
    /// <param name="inCombat">True to enable combat mode, false to disable.</param>
    public void SetCombatMode(bool inCombat)
    {
        IsInCombat = inCombat;
        _layout?.SetPanelVisible(PanelRegion.CombatOverlay, inCombat);
        Log.Debug("Combat mode: {InCombat}", inCombat);
    }
}
