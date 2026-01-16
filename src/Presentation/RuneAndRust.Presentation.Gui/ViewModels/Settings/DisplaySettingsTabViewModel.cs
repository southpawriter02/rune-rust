namespace RuneAndRust.Presentation.Gui.ViewModels.Settings;

using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Presentation.Gui.Models;
using Serilog;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for the Display settings tab.
/// </summary>
public partial class DisplaySettingsTabViewModel : ViewModelBase
{
    private readonly Action? _onChanged;

    [ObservableProperty] private string _selectedTheme = "Dark Fantasy";
    [ObservableProperty] private int _fontSizePercent = 100;
    [ObservableProperty] private bool _enableCombatAnimations = true;
    [ObservableProperty] private bool _enableRoomTransitions = true;
    [ObservableProperty] private bool _highContrastMode;

    /// <summary>Available themes.</summary>
    public ObservableCollection<string> AvailableThemes { get; } =
    [
        "Dark Fantasy", "High Fantasy", "Classic", "Minimal"
    ];

    /// <summary>Creates from settings with change callback.</summary>
    public DisplaySettingsTabViewModel(DisplaySettings settings, Action onChanged)
    {
        _onChanged = onChanged;
        SelectedTheme = settings.Theme;
        FontSizePercent = settings.FontSizePercent;
        EnableCombatAnimations = settings.EnableCombatAnimations;
        EnableRoomTransitions = settings.EnableRoomTransitions;
        HighContrastMode = settings.HighContrastMode;
    }

    /// <summary>Design-time constructor.</summary>
    public DisplaySettingsTabViewModel() { }

    /// <summary>Converts to settings model.</summary>
    public DisplaySettings ToSettings() => new()
    {
        Theme = SelectedTheme,
        FontSizePercent = FontSizePercent,
        EnableCombatAnimations = EnableCombatAnimations,
        EnableRoomTransitions = EnableRoomTransitions,
        HighContrastMode = HighContrastMode
    };

    partial void OnSelectedThemeChanged(string value) { Log.Debug("Theme: {Theme}", value); _onChanged?.Invoke(); }
    partial void OnFontSizePercentChanged(int value) { Log.Debug("Font size: {Size}%", value); _onChanged?.Invoke(); }
    partial void OnEnableCombatAnimationsChanged(bool value) { _onChanged?.Invoke(); }
    partial void OnEnableRoomTransitionsChanged(bool value) { _onChanged?.Invoke(); }
    partial void OnHighContrastModeChanged(bool value) { _onChanged?.Invoke(); }
}
