namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

/// <summary>
/// ViewModel for a quest objective.
/// </summary>
public partial class ObjectiveViewModel : ViewModelBase
{
    /// <summary>Gets the objective description.</summary>
    [ObservableProperty] private string _description;

    /// <summary>Gets whether the objective is completed.</summary>
    [ObservableProperty] private bool _isCompleted;

    /// <summary>Gets current progress count.</summary>
    [ObservableProperty] private int _currentProgress;

    /// <summary>Gets required progress count.</summary>
    [ObservableProperty] private int _requiredProgress;

    /// <summary>Gets whether this objective has progress tracking.</summary>
    public bool HasProgress => RequiredProgress > 1;

    /// <summary>Gets the status icon (☑/☐).</summary>
    public string StatusIcon => IsCompleted ? "☑" : "☐";

    /// <summary>Gets progress text (e.g., "2/5").</summary>
    public string ProgressText => HasProgress ? $"{CurrentProgress}/{RequiredProgress}" : "";

    /// <summary>Creates an objective ViewModel.</summary>
    public ObjectiveViewModel(string description, int current, int required)
    {
        _description = description;
        _currentProgress = current;
        _requiredProgress = required;
        _isCompleted = current >= required;
    }
}
