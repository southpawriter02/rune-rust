namespace RuneAndRust.Presentation.Gui.ViewModels.Settings;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RuneAndRust.Presentation.Gui.Models;
using Serilog;

/// <summary>
/// Main ViewModel for the Settings window.
/// </summary>
public partial class SettingsWindowViewModel : ViewModelBase
{
    private readonly GameSettings _originalSettings;
    private readonly Action? _closeWindow;

    [ObservableProperty] private int _selectedTabIndex;
    [ObservableProperty] private bool _hasChanges;

    /// <summary>Audio tab ViewModel.</summary>
    public AudioSettingsTabViewModel AudioTab { get; }

    /// <summary>Display tab ViewModel.</summary>
    public DisplaySettingsTabViewModel DisplayTab { get; }

    /// <summary>Gameplay tab ViewModel.</summary>
    public GameplaySettingsTabViewModel GameplayTab { get; }

    /// <summary>Controls tab ViewModel.</summary>
    public ControlsSettingsTabViewModel ControlsTab { get; }

    /// <summary>Gets the current tab ViewModel.</summary>
    public object CurrentTab => SelectedTabIndex switch
    {
        0 => AudioTab,
        1 => DisplayTab,
        2 => GameplayTab,
        3 => ControlsTab,
        _ => AudioTab
    };

    /// <summary>Event raised when settings are saved.</summary>
    public event Action<GameSettings>? OnSettingsSaved;

    /// <summary>Creates the settings window ViewModel.</summary>
    public SettingsWindowViewModel(GameSettings settings, Action? closeWindow = null)
    {
        _originalSettings = settings.Clone();
        _closeWindow = closeWindow;

        AudioTab = new AudioSettingsTabViewModel(settings.Audio, OnSettingChanged);
        DisplayTab = new DisplaySettingsTabViewModel(settings.Display, OnSettingChanged);
        GameplayTab = new GameplaySettingsTabViewModel(settings.Gameplay, OnSettingChanged);
        ControlsTab = new ControlsSettingsTabViewModel(settings.Controls, OnSettingChanged);
    }

    /// <summary>Design-time constructor.</summary>
    public SettingsWindowViewModel()
    {
        _originalSettings = new GameSettings();
        AudioTab = new AudioSettingsTabViewModel();
        DisplayTab = new DisplaySettingsTabViewModel();
        GameplayTab = new GameplaySettingsTabViewModel();
        ControlsTab = new ControlsSettingsTabViewModel();
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        OnPropertyChanged(nameof(CurrentTab));
    }

    /// <summary>Applies settings without closing.</summary>
    [RelayCommand]
    private void Apply()
    {
        var settings = BuildSettings();
        OnSettingsSaved?.Invoke(settings);
        HasChanges = false;
        Log.Information("Settings applied");
    }

    /// <summary>Cancels and reverts changes.</summary>
    [RelayCommand]
    private void Cancel()
    {
        Log.Information("Settings cancelled");
        _closeWindow?.Invoke();
    }

    /// <summary>Applies and closes.</summary>
    [RelayCommand]
    private void Ok()
    {
        if (HasChanges) Apply();
        _closeWindow?.Invoke();
    }

    private void OnSettingChanged() => HasChanges = true;

    private GameSettings BuildSettings() => new()
    {
        Audio = AudioTab.ToSettings(),
        Display = DisplayTab.ToSettings(),
        Gameplay = GameplayTab.ToSettings(),
        Controls = ControlsTab.ToSettings()
    };
}
