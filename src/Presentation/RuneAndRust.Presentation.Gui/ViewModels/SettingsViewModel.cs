using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Serilog;

namespace RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// View model for the settings window.
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    /// <summary>
    /// Available window modes.
    /// </summary>
    public string[] WindowModes { get; } = { "Windowed", "Fullscreen", "Borderless" };

    /// <summary>
    /// Available resolutions.
    /// </summary>
    public string[] Resolutions { get; } = { "1280x720", "1920x1080", "2560x1440" };

    /// <summary>
    /// Gets or sets the selected window mode.
    /// </summary>
    [ObservableProperty]
    private string _selectedWindowMode = "Windowed";

    /// <summary>
    /// Gets or sets the selected resolution.
    /// </summary>
    [ObservableProperty]
    private string _selectedResolution = "1280x720";

    /// <summary>
    /// Gets or sets the master volume (0-100).
    /// </summary>
    [ObservableProperty]
    private int _masterVolume = 80;

    /// <summary>
    /// Gets or sets the music volume (0-100).
    /// </summary>
    [ObservableProperty]
    private int _musicVolume = 60;

    /// <summary>
    /// Gets or sets the SFX volume (0-100).
    /// </summary>
    [ObservableProperty]
    private int _sfxVolume = 80;

    /// <summary>
    /// Applies settings and closes the dialog.
    /// </summary>
    [RelayCommand]
    private void Apply()
    {
        Log.Information("Applying settings: WindowMode={WindowMode}, Resolution={Resolution}, Master={Master}%, Music={Music}%, SFX={SFX}%",
            SelectedWindowMode, SelectedResolution, MasterVolume, MusicVolume, SfxVolume);
        
        // Settings persistence will be enhanced in future versions
        CloseWindow();
    }

    /// <summary>
    /// Cancels changes and closes the dialog.
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        Log.Debug("Settings cancelled");
        CloseWindow();
    }

    private static void CloseWindow()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Find and close the settings window
            foreach (var window in desktop.Windows)
            {
                if (window is Views.SettingsWindow settingsWindow)
                {
                    settingsWindow.Close();
                    break;
                }
            }
        }
    }
}
