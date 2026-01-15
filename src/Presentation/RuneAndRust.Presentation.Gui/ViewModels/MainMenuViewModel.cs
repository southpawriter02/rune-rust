using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RuneAndRust.Presentation.Gui.Services;
using RuneAndRust.Presentation.Gui.Views;
using Serilog;

namespace RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// View model for the main menu window.
/// </summary>
/// <remarks>
/// Provides commands for starting a new game, loading existing games,
/// opening settings, and quitting the application.
/// </remarks>
public partial class MainMenuViewModel : ViewModelBase
{
    private readonly INavigationService? _navigation;

    /// <summary>
    /// Gets the application version string.
    /// </summary>
    [ObservableProperty]
    private string _version = "v0.7.0 - GUI Foundation";

    /// <summary>
    /// Initializes a new instance of the <see cref="MainMenuViewModel"/> class.
    /// </summary>
    /// <param name="navigation">The navigation service for window management.</param>
    public MainMenuViewModel(INavigationService navigation)
    {
        _navigation = navigation;
        Log.Debug("MainMenuViewModel initialized with NavigationService");
    }

    /// <summary>
    /// Design-time constructor.
    /// </summary>
    public MainMenuViewModel()
    {
        _navigation = null;
    }

    /// <summary>
    /// Starts a new game session and navigates to the game window.
    /// </summary>
    [RelayCommand]
    private void NewGame()
    {
        Log.Information("Starting new game");
        _navigation?.NavigateTo<GameWindow>();
    }

    /// <summary>
    /// Opens a file dialog to load an existing save.
    /// </summary>
    [RelayCommand]
    private async Task LoadGameAsync()
    {
        Log.Information("Opening load game dialog");
        
        var path = await _navigation!.ShowLoadDialogAsync();
        if (path is not null)
        {
            Log.Information("Selected save file: {Path}", path);
            // Navigate to game window after loading
            _navigation.NavigateTo<GameWindow>();
        }
    }

    /// <summary>
    /// Opens the settings dialog.
    /// </summary>
    [RelayCommand]
    private void OpenSettings()
    {
        Log.Information("Opening settings dialog");
        _navigation?.ShowDialog<SettingsWindow>();
    }

    /// <summary>
    /// Exits the application.
    /// </summary>
    [RelayCommand]
    private void Quit()
    {
        Log.Information("Quitting application");
        _navigation?.Quit();
    }
}
