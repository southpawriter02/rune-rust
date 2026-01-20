namespace RuneAndRust.Presentation.Gui.ViewModels.Settings;

using CommunityToolkit.Mvvm.ComponentModel;
using RuneAndRust.Presentation.Gui.Models;
using Serilog;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for the Gameplay settings tab.
/// </summary>
public partial class GameplaySettingsTabViewModel : ViewModelBase
{
    private readonly Action? _onChanged;

    [ObservableProperty] private string _difficulty = "Normal";
    [ObservableProperty] private string _autoSaveFrequency = "Every 5 minutes";
    [ObservableProperty] private bool _showCombatTooltips = true;
    [ObservableProperty] private bool _showTutorialHints = true;
    [ObservableProperty] private bool _confirmDangerousActions;

    /// <summary>Available difficulties.</summary>
    public ObservableCollection<string> Difficulties { get; } = ["Easy", "Normal", "Hard"];

    /// <summary>Available auto-save frequencies.</summary>
    public ObservableCollection<string> AutoSaveOptions { get; } =
    [
        "Disabled", "Every 1 minute", "Every 5 minutes", "Every 10 minutes"
    ];

    /// <summary>Description text for selected difficulty.</summary>
    public string DifficultyDescription => Difficulty switch
    {
        "Easy" => "+20% damage dealt, -20% damage taken",
        "Normal" => "Standard experience",
        "Hard" => "-20% damage dealt, +20% damage taken",
        _ => ""
    };

    /// <summary>Creates from settings with change callback.</summary>
    public GameplaySettingsTabViewModel(GameplaySettings settings, Action onChanged)
    {
        _onChanged = onChanged;
        Difficulty = settings.Difficulty;
        AutoSaveFrequency = settings.AutoSaveFrequency;
        ShowCombatTooltips = settings.ShowCombatTooltips;
        ShowTutorialHints = settings.ShowTutorialHints;
        ConfirmDangerousActions = settings.ConfirmDangerousActions;
    }

    /// <summary>Design-time constructor.</summary>
    public GameplaySettingsTabViewModel() { }

    /// <summary>Converts to settings model.</summary>
    public GameplaySettings ToSettings() => new()
    {
        Difficulty = Difficulty,
        AutoSaveFrequency = AutoSaveFrequency,
        ShowCombatTooltips = ShowCombatTooltips,
        ShowTutorialHints = ShowTutorialHints,
        ConfirmDangerousActions = ConfirmDangerousActions
    };

    partial void OnDifficultyChanged(string value)
    {
        Log.Debug("Difficulty: {Diff}", value);
        OnPropertyChanged(nameof(DifficultyDescription));
        _onChanged?.Invoke();
    }

    partial void OnAutoSaveFrequencyChanged(string value) { _onChanged?.Invoke(); }
    partial void OnShowCombatTooltipsChanged(bool value) { _onChanged?.Invoke(); }
    partial void OnShowTutorialHintsChanged(bool value) { _onChanged?.Invoke(); }
    partial void OnConfirmDangerousActionsChanged(bool value) { _onChanged?.Invoke(); }
}
