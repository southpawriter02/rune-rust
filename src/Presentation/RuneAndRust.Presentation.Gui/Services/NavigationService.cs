namespace RuneAndRust.Presentation.Gui.Services;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using RuneAndRust.Presentation.Gui.Views;
using Serilog;

/// <summary>
/// Implementation of window navigation service.
/// </summary>
/// <remarks>
/// Handles window replacement, modal dialogs, file picker dialogs,
/// and application lifecycle control for the desktop application.
/// </remarks>
public class NavigationService : INavigationService
{
    /// <inheritdoc />
    public Window? CurrentWindow
    {
        get
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }
            return null;
        }
    }

    /// <inheritdoc />
    public void NavigateTo<TWindow>() where TWindow : Window, new()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var oldWindow = desktop.MainWindow;
            var newWindow = new TWindow();
            
            // Set the view model if the window is a GameWindow
            if (newWindow is GameWindow gameWindow)
            {
                gameWindow.DataContext = App.Services?.GetService(typeof(ViewModels.GameWindowViewModel));
            }
            
            desktop.MainWindow = newWindow;
            newWindow.Show();
            oldWindow?.Close();
            
            Log.Information("Navigated to {WindowType}", typeof(TWindow).Name);
        }
    }

    /// <inheritdoc />
    public async void ShowDialog<TWindow>() where TWindow : Window, new()
    {
        if (CurrentWindow is not null)
        {
            var dialog = new TWindow();
            await dialog.ShowDialog(CurrentWindow);
            Log.Debug("Showed dialog {WindowType}", typeof(TWindow).Name);
        }
    }

    /// <inheritdoc />
    public async Task<string?> ShowLoadDialogAsync()
    {
        if (CurrentWindow is null)
        {
            Log.Warning("Cannot show load dialog: no current window");
            return null;
        }

        var files = await CurrentWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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
            Log.Debug("Load dialog selected: {Path}", path);
            return path;
        }

        Log.Debug("Load dialog cancelled");
        return null;
    }

    /// <inheritdoc />
    public async Task<string?> ShowSaveDialogAsync()
    {
        if (CurrentWindow is null)
        {
            Log.Warning("Cannot show save dialog: no current window");
            return null;
        }

        var file = await CurrentWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Game",
            DefaultExtension = "json",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Save Files") { Patterns = new[] { "*.json" } }
            }
        });

        if (file is not null)
        {
            var path = file.Path.LocalPath;
            Log.Debug("Save dialog selected: {Path}", path);
            return path;
        }

        Log.Debug("Save dialog cancelled");
        return null;
    }

    /// <inheritdoc />
    public void ReturnToMainMenu()
    {
        Log.Information("Returning to main menu");
        NavigateTo<MainMenuWindow>();
    }

    /// <inheritdoc />
    public void Quit()
    {
        Log.Information("Quitting application");
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}
