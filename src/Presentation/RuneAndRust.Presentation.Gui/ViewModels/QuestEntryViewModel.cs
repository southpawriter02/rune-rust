namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for a quest entry in the quest log.
/// </summary>
public partial class QuestEntryViewModel : ViewModelBase
{
    /// <summary>Gets the quest ID.</summary>
    public Guid QuestId { get; }

    /// <summary>Gets the quest title.</summary>
    [ObservableProperty] private string _title;

    /// <summary>Gets the quest description.</summary>
    [ObservableProperty] private string _description;

    /// <summary>Gets whether the quest is completed.</summary>
    [ObservableProperty] private bool _isCompleted;

    /// <summary>Gets whether the quest is tracked.</summary>
    [ObservableProperty] private bool _isTracked;

    /// <summary>Gets whether the quest is selected.</summary>
    [ObservableProperty] private bool _isSelected;

    /// <summary>Gets the quest objectives.</summary>
    public ObservableCollection<ObjectiveViewModel> Objectives { get; } = [];

    /// <summary>Gets the quest rewards.</summary>
    public ObservableCollection<RewardViewModel> Rewards { get; } = [];

    /// <summary>Gets the progress percentage (0-100).</summary>
    public double ProgressPercentage
    {
        get
        {
            if (Objectives.Count == 0) return 0;
            var completed = Objectives.Count(o => o.IsCompleted);
            return (completed / (double)Objectives.Count) * 100;
        }
    }

    /// <summary>Gets the progress text.</summary>
    public string ProgressText => $"Progress: {Objectives.Count(o => o.IsCompleted)}/{Objectives.Count} objectives";

    /// <summary>Creates a quest entry ViewModel.</summary>
    public QuestEntryViewModel(Guid questId, string title, string description, bool isCompleted = false)
    {
        QuestId = questId;
        _title = title;
        _description = description;
        _isCompleted = isCompleted;
    }

    /// <summary>Adds an objective to the quest.</summary>
    public void AddObjective(string description, int current, int required)
    {
        Objectives.Add(new ObjectiveViewModel(description, current, required));
        OnPropertyChanged(nameof(ProgressPercentage));
        OnPropertyChanged(nameof(ProgressText));
    }

    /// <summary>Adds a reward to the quest.</summary>
    public void AddReward(string type, string description)
    {
        Rewards.Add(new RewardViewModel(type, description));
    }
}
