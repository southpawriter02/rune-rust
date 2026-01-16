namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for the Quest Log Window.
/// </summary>
public partial class QuestLogWindowViewModel : ViewModelBase
{
    private readonly Action? _closeAction;

    /// <summary>Active quests collection.</summary>
    [ObservableProperty] private ObservableCollection<QuestEntryViewModel> _activeQuests = [];

    /// <summary>Completed quests collection.</summary>
    [ObservableProperty] private ObservableCollection<QuestEntryViewModel> _completedQuests = [];

    /// <summary>Currently selected quest.</summary>
    [ObservableProperty] private QuestEntryViewModel? _selectedQuest;

    /// <summary>Whether to show completed quests section.</summary>
    [ObservableProperty] private bool _showCompletedQuests = true;

    /// <summary>Tracked quest ID.</summary>
    [ObservableProperty] private Guid? _trackedQuestId;

    /// <summary>Active quest section header.</summary>
    public string ActiveQuestHeader => $"ACTIVE QUESTS ({ActiveQuests.Count})";

    /// <summary>Completed quest section header.</summary>
    public string CompletedQuestHeader => $"COMPLETED ({CompletedQuests.Count})";

    /// <summary>Completed section expander icon.</summary>
    public string CompletedExpander => ShowCompletedQuests ? "▼" : "►";

    /// <summary>Whether a quest is selected.</summary>
    public bool HasSelectedQuest => SelectedQuest is not null;

    /// <summary>Whether the selected quest can be tracked.</summary>
    public bool CanTrackQuest => SelectedQuest is not null && !SelectedQuest.IsCompleted;

    /// <summary>Tracking status message.</summary>
    public string TrackingStatus => SelectedQuest?.IsTracked == true
        ? "Currently tracking this quest"
        : "";

    /// <summary>Creates a quest log ViewModel.</summary>
    public QuestLogWindowViewModel(Action? closeAction = null)
    {
        _closeAction = closeAction;
        LoadSampleQuests();
    }

    /// <summary>Selects a quest to show details.</summary>
    [RelayCommand]
    private void SelectQuest(QuestEntryViewModel quest)
    {
        if (SelectedQuest is not null)
            SelectedQuest.IsSelected = false;

        SelectedQuest = quest;
        quest.IsSelected = true;

        OnPropertyChanged(nameof(HasSelectedQuest));
        OnPropertyChanged(nameof(CanTrackQuest));
        OnPropertyChanged(nameof(TrackingStatus));

        Log.Debug("Selected quest: {Title}", quest.Title);
    }

    /// <summary>Tracks the selected quest.</summary>
    [RelayCommand]
    private void TrackQuest()
    {
        if (SelectedQuest is null || SelectedQuest.IsCompleted) return;

        // Untrack previous
        foreach (var q in ActiveQuests.Where(q => q.IsTracked))
            q.IsTracked = false;

        SelectedQuest.IsTracked = true;
        TrackedQuestId = SelectedQuest.QuestId;
        OnPropertyChanged(nameof(TrackingStatus));

        Log.Information("Tracking quest: {Title}", SelectedQuest.Title);
    }

    /// <summary>Toggles completed quests visibility.</summary>
    [RelayCommand]
    private void ToggleCompletedQuests()
    {
        ShowCompletedQuests = !ShowCompletedQuests;
        OnPropertyChanged(nameof(CompletedExpander));
    }

    /// <summary>Closes the window.</summary>
    [RelayCommand]
    private void Close() => _closeAction?.Invoke();

    private void LoadSampleQuests()
    {
        // Sample active quests
        var mainQuest = new QuestEntryViewModel(Guid.NewGuid(), "The Lost Artifact",
            "Find the ancient artifact hidden deep within the dungeon. Legends say it holds immense power.");
        mainQuest.AddObjective("Enter the dungeon", 1, 1);
        mainQuest.AddObjective("Defeat the guardian", 0, 1);
        mainQuest.AddObjective("Retrieve the artifact", 0, 1);
        mainQuest.AddReward("Experience", "500 XP");
        mainQuest.AddReward("Gold", "100 Gold");
        mainQuest.IsTracked = true;
        TrackedQuestId = mainQuest.QuestId;
        ActiveQuests.Add(mainQuest);

        var huntQuest = new QuestEntryViewModel(Guid.NewGuid(), "Goblin Menace",
            "Clear the surrounding area of the goblin threat. They've been raiding nearby villages.");
        huntQuest.AddObjective("Kill goblins", 3, 10);
        huntQuest.AddObjective("Find goblin camp", 0, 1);
        huntQuest.AddReward("Experience", "250 XP");
        huntQuest.AddReward("Item", "Goblin Slayer Badge");
        ActiveQuests.Add(huntQuest);

        var collectQuest = new QuestEntryViewModel(Guid.NewGuid(), "Herb Gathering",
            "The local healer needs rare herbs that only grow in the dungeon depths.");
        collectQuest.AddObjective("Collect Moonpetal", 2, 5);
        collectQuest.AddObjective("Collect Shadowroot", 1, 3);
        collectQuest.AddReward("Gold", "50 Gold");
        collectQuest.AddReward("Unlock", "Healer's Discount");
        ActiveQuests.Add(collectQuest);

        // Sample completed quest
        var completedQuest = new QuestEntryViewModel(Guid.NewGuid(), "First Steps",
            "Learn the basics of dungeon exploration.", isCompleted: true);
        completedQuest.AddObjective("Talk to the guide", 1, 1);
        completedQuest.AddObjective("Explore a room", 1, 1);
        completedQuest.AddReward("Experience", "50 XP");
        CompletedQuests.Add(completedQuest);

        Log.Information("Quest log loaded: {Active} active, {Completed} completed",
            ActiveQuests.Count, CompletedQuests.Count);
    }
}
