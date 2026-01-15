using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Serilog;

namespace RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// View model for the main menu window.
/// </summary>
public partial class MainMenuViewModel : ViewModelBase
{
    /// <summary>
    /// Gets the application version string.
    /// </summary>
    [ObservableProperty]
    private string _version = "v0.7.0 - GUI Foundation";

    /// <summary>
    /// Starts a new game session.
    /// </summary>
    [RelayCommand]
    private void NewGame()
    {
        Log.Information("Starting new game");
        // GameWindow navigation will be implemented in v0.7.0c
    }

    /// <summary>
    /// Opens a file dialog to load an existing save.
    /// </summary>
    [RelayCommand]
    private async Task LoadGameAsync()
    {
        Log.Information("Opening load game dialog");
        
        var window = GetCurrentWindow();
        if (window is null) return;

        var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Load Game",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Save Files") { Patterns = new[] { "*.json" } },
                new FilePickerFileType("All Files") { Patterns = new[] { "*" } }
            }
        });

        if (files.Count > 0)
        {
            var path = files[0].Path.LocalPath;
            Log.Information("Selected save file: {Path}", path);
            // Game loading will be implemented in v0.7.0c
        }
    }

    /// <summary>
    /// Opens the settings dialog.
    /// </summary>
    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        Log.Information("Opening settings dialog");
        
        var window = GetCurrentWindow();
        if (window is null) return;

        var settingsWindow = new Views.SettingsWindow
        {
            DataContext = new SettingsViewModel()
        };
        
        await settingsWindow.ShowDialog(window);
    }

    /// <summary>
    /// Exits the application.
    /// </summary>
    [RelayCommand]
    private void Quit()
    {
        Log.Information("Quitting application");
        
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    private static Window? GetCurrentWindow()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }
}
