using ReactiveUI;
using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// v0.43.15: View model for the meta-progression and achievements screen.
/// Displays achievements, unlocks, cosmetics, and account-wide statistics.
/// </summary>
public class MetaProgressionViewModel : ViewModelBase
{
    private readonly IMetaProgressionService _metaService;
    private readonly INavigationService _navigationService;
    private MetaProgressionState? _metaState;
    private AchievementViewModel? _selectedAchievement;
    private string _filterCategory = "All";

    /// <summary>
    /// Collection of achievements for display.
    /// </summary>
    public ObservableCollection<AchievementViewModel> Achievements { get; } = new();

    /// <summary>
    /// Collection of account unlocks for display.
    /// </summary>
    public ObservableCollection<UnlockViewModel> Unlocks { get; } = new();

    /// <summary>
    /// Collection of unlocked cosmetics for display.
    /// </summary>
    public ObservableCollection<CosmeticViewModel> Cosmetics { get; } = new();

    /// <summary>
    /// Available filter categories.
    /// </summary>
    public ObservableCollection<string> Categories { get; } = new()
    {
        "All", "Milestone", "Combat", "Exploration", "Challenge", "Narrative"
    };

    /// <summary>
    /// Currently selected filter category.
    /// </summary>
    public string FilterCategory
    {
        get => _filterCategory;
        set
        {
            this.RaiseAndSetIfChanged(ref _filterCategory, value);
            FilterAchievements();
        }
    }

    /// <summary>
    /// Currently selected achievement for details view.
    /// </summary>
    public AchievementViewModel? SelectedAchievement
    {
        get => _selectedAchievement;
        set => this.RaiseAndSetIfChanged(ref _selectedAchievement, value);
    }

    #region Statistics Properties

    /// <summary>
    /// Total number of achievements.
    /// </summary>
    public int TotalAchievements => _metaState?.TotalAchievements ?? 0;

    /// <summary>
    /// Number of unlocked achievements.
    /// </summary>
    public int UnlockedAchievements => _metaState?.UnlockedAchievements ?? 0;

    /// <summary>
    /// Achievement completion percentage (0.0 - 1.0).
    /// </summary>
    public float CompletionPercentage => TotalAchievements > 0 ? (float)UnlockedAchievements / TotalAchievements : 0f;

    /// <summary>
    /// Total achievement points earned.
    /// </summary>
    public int TotalAchievementPoints => _metaState?.TotalAchievementPoints ?? 0;

    /// <summary>
    /// Total number of campaign runs.
    /// </summary>
    public int TotalRuns => _metaState?.TotalRuns ?? 0;

    /// <summary>
    /// Number of successful campaign completions.
    /// </summary>
    public int SuccessfulRuns => _metaState?.SuccessfulRuns ?? 0;

    /// <summary>
    /// Current milestone tier number.
    /// </summary>
    public int CurrentMilestoneTier => _metaState?.CurrentTier?.TierNumber ?? 1;

    /// <summary>
    /// Current milestone tier name.
    /// </summary>
    public string CurrentTierName => _metaState?.CurrentTier?.TierName ?? "Initiate";

    /// <summary>
    /// Highest New Game+ tier completed.
    /// </summary>
    public int HighestNewGamePlus => _metaState?.HighestNewGamePlus ?? 0;

    /// <summary>
    /// Highest wave reached in Endless Mode.
    /// </summary>
    public int HighestEndlessWave => _metaState?.HighestEndlessWave ?? 0;

    /// <summary>
    /// Total play time formatted.
    /// </summary>
    public string TotalPlayTimeDisplay
    {
        get
        {
            var time = _metaState?.TotalPlayTime ?? TimeSpan.Zero;
            if (time.TotalHours >= 1)
                return $"{(int)time.TotalHours}h {time.Minutes}m";
            return $"{time.Minutes}m {time.Seconds}s";
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// Command to return to main menu.
    /// </summary>
    public ICommand BackCommand { get; }

    /// <summary>
    /// Command to refresh meta-progression data.
    /// </summary>
    public ICommand RefreshCommand { get; }

    #endregion

    /// <summary>
    /// Creates a new instance for design-time support.
    /// </summary>
    public MetaProgressionViewModel()
    {
        _metaService = null!;
        _navigationService = null!;
        BackCommand = ReactiveCommand.Create(() => { });
        RefreshCommand = ReactiveCommand.Create(() => { });

        // Add sample data for design-time
        LoadDesignTimeData();
    }

    /// <summary>
    /// Creates a new instance with dependency injection.
    /// </summary>
    public MetaProgressionViewModel(
        IMetaProgressionService metaService,
        INavigationService navigationService)
    {
        _metaService = metaService;
        _navigationService = navigationService;

        BackCommand = ReactiveCommand.Create(OnBack);
        RefreshCommand = ReactiveCommand.Create(LoadMetaProgression);

        // Load data on construction
        LoadMetaProgression();
    }

    /// <summary>
    /// Loads meta-progression data from the service.
    /// </summary>
    public void LoadMetaProgression()
    {
        _metaState = _metaService.LoadMetaProgression();
        LoadAchievements();
        LoadUnlocks();
        LoadCosmetics();
        UpdateStatistics();
    }

    private void LoadAchievements()
    {
        Achievements.Clear();

        if (_metaState == null) return;

        foreach (var achievement in _metaState.Achievements)
        {
            Achievements.Add(new AchievementViewModel(achievement));
        }

        FilterAchievements();
    }

    private void LoadUnlocks()
    {
        Unlocks.Clear();

        if (_metaState == null) return;

        foreach (var unlock in _metaState.Unlocks)
        {
            Unlocks.Add(new UnlockViewModel(unlock));
        }
    }

    private void LoadCosmetics()
    {
        Cosmetics.Clear();

        if (_metaState == null) return;

        foreach (var cosmetic in _metaState.UnlockedCosmetics)
        {
            Cosmetics.Add(new CosmeticViewModel(cosmetic));
        }
    }

    private void FilterAchievements()
    {
        if (_metaState == null) return;

        var filtered = FilterCategory == "All"
            ? _metaState.Achievements
            : _metaState.Achievements.Where(a => a.Category.ToString() == FilterCategory);

        Achievements.Clear();
        foreach (var achievement in filtered)
        {
            Achievements.Add(new AchievementViewModel(achievement));
        }
    }

    private void UpdateStatistics()
    {
        this.RaisePropertyChanged(nameof(TotalAchievements));
        this.RaisePropertyChanged(nameof(UnlockedAchievements));
        this.RaisePropertyChanged(nameof(CompletionPercentage));
        this.RaisePropertyChanged(nameof(TotalAchievementPoints));
        this.RaisePropertyChanged(nameof(TotalRuns));
        this.RaisePropertyChanged(nameof(SuccessfulRuns));
        this.RaisePropertyChanged(nameof(CurrentMilestoneTier));
        this.RaisePropertyChanged(nameof(CurrentTierName));
        this.RaisePropertyChanged(nameof(HighestNewGamePlus));
        this.RaisePropertyChanged(nameof(HighestEndlessWave));
        this.RaisePropertyChanged(nameof(TotalPlayTimeDisplay));
    }

    private void OnBack()
    {
        _navigationService.GoBack();
    }

    private void LoadDesignTimeData()
    {
        // Add sample achievements for designer
        Achievements.Add(new AchievementViewModel(new AchievementWithProgress
        {
            Achievement = new Achievement
            {
                AchievementId = "sample",
                Name = "Sample Achievement",
                Description = "This is a sample achievement",
                Category = AchievementCategory.Milestone,
                AchievementPoints = 10
            },
            Progress = new AchievementProgress { IsUnlocked = true, CurrentProgress = 1 }
        }));
    }
}

/// <summary>
/// v0.43.15: View model wrapper for Achievement display.
/// </summary>
public class AchievementViewModel : ViewModelBase
{
    private readonly AchievementWithProgress _achievement;

    public AchievementWithProgress Achievement => _achievement;

    public string Name => _achievement.Name;
    public string Description => _achievement.Description;
    public string FlavorText => _achievement.FlavorText;
    public string Category => _achievement.Category.ToString();
    public bool IsUnlocked => _achievement.IsUnlocked;
    public bool IsSecret => _achievement.IsSecret;
    public int AchievementPoints => _achievement.AchievementPoints;
    public float Progress => _achievement.ProgressPercentage;
    public int CurrentValue => _achievement.CurrentProgress;
    public int TargetValue => _achievement.RequiredProgress;

    /// <summary>
    /// Display name (hidden for secret achievements).
    /// </summary>
    public string DisplayName => IsSecret && !IsUnlocked ? "???" : Name;

    /// <summary>
    /// Display description (hidden for secret achievements).
    /// </summary>
    public string DisplayDescription => IsSecret && !IsUnlocked ? "Hidden achievement" : Description;

    /// <summary>
    /// Reward description text.
    /// </summary>
    public string RewardDescription => $"+{AchievementPoints} Achievement Points";

    /// <summary>
    /// Icon text for the achievement (based on unlock state).
    /// </summary>
    public string IconText => IsUnlocked ? "★" : "☆";

    /// <summary>
    /// Icon color based on category.
    /// </summary>
    public string IconColor => _achievement.Category switch
    {
        AchievementCategory.Milestone => "#FFD700",  // Gold
        AchievementCategory.Combat => "#FF6B6B",     // Red
        AchievementCategory.Exploration => "#4CAF50", // Green
        AchievementCategory.Challenge => "#9400D3",  // Purple
        AchievementCategory.Narrative => "#4A90E2",  // Blue
        AchievementCategory.Collection => "#FFA500", // Orange
        _ => "#CCCCCC"
    };

    /// <summary>
    /// Background color based on unlock state.
    /// </summary>
    public string BackgroundColor => IsUnlocked ? "#2D3A2D" : "#1C1C1C";

    /// <summary>
    /// Border color based on unlock state.
    /// </summary>
    public string BorderColor => IsUnlocked ? "#4CAF50" : "#3C3C3C";

    public AchievementViewModel(AchievementWithProgress achievement)
    {
        _achievement = achievement;
    }
}

/// <summary>
/// v0.43.15: View model wrapper for AccountUnlock display.
/// </summary>
public class UnlockViewModel : ViewModelBase
{
    private readonly AccountUnlock _unlock;

    public AccountUnlock Unlock => _unlock;

    public string Name => _unlock.Name;
    public string Description => _unlock.Description;
    public string Requirement => _unlock.RequirementDescription;
    public AccountUnlockType Type => _unlock.Type;
    public bool IsUnlocked => _unlock.IsUnlocked;

    /// <summary>
    /// Type display text.
    /// </summary>
    public string TypeDisplay => Type.ToString();

    /// <summary>
    /// Type color for display.
    /// </summary>
    public string TypeColor => Type switch
    {
        AccountUnlockType.Convenience => "#4A90E2",  // Blue
        AccountUnlockType.Variety => "#9400D3",      // Purple
        AccountUnlockType.Progression => "#4CAF50",  // Green
        AccountUnlockType.Cosmetic => "#FFD700",     // Gold
        AccountUnlockType.Knowledge => "#FFA500",    // Orange
        _ => "#CCCCCC"
    };

    /// <summary>
    /// Icon for the unlock type.
    /// </summary>
    public string TypeIcon => Type switch
    {
        AccountUnlockType.Convenience => "⚡",
        AccountUnlockType.Variety => "🎭",
        AccountUnlockType.Progression => "📈",
        AccountUnlockType.Cosmetic => "✨",
        AccountUnlockType.Knowledge => "📚",
        _ => "•"
    };

    /// <summary>
    /// Background color based on unlock state.
    /// </summary>
    public string BackgroundColor => IsUnlocked ? "#2D3A2D" : "#1C1C1C";

    /// <summary>
    /// Border color based on unlock state.
    /// </summary>
    public string BorderColor => IsUnlocked ? "#4CAF50" : "#3C3C3C";

    public UnlockViewModel(AccountUnlock unlock)
    {
        _unlock = unlock;
    }
}

/// <summary>
/// v0.43.15: View model wrapper for Cosmetic display.
/// </summary>
public class CosmeticViewModel : ViewModelBase
{
    private readonly Cosmetic _cosmetic;

    public Cosmetic Cosmetic => _cosmetic;

    public string Name => _cosmetic.Name;
    public string Description => _cosmetic.Description;
    public CosmeticType Type => _cosmetic.Type;
    public string UnlockRequirement => _cosmetic.UnlockRequirement;

    /// <summary>
    /// Type display text.
    /// </summary>
    public string TypeDisplay => Type.ToString();

    /// <summary>
    /// Type color for display.
    /// </summary>
    public string TypeColor => Type switch
    {
        CosmeticType.Title => "#FFD700",        // Gold
        CosmeticType.Portrait => "#4A90E2",     // Blue
        CosmeticType.UITheme => "#9400D3",      // Purple
        CosmeticType.AbilityVFX => "#FF6B6B",   // Red
        CosmeticType.CombatLogStyle => "#4CAF50", // Green
        CosmeticType.CharacterFrame => "#FFA500", // Orange
        CosmeticType.Emblem => "#00CED1",       // Cyan
        _ => "#CCCCCC"
    };

    /// <summary>
    /// Icon for the cosmetic type.
    /// </summary>
    public string TypeIcon => Type switch
    {
        CosmeticType.Title => "📛",
        CosmeticType.Portrait => "🖼️",
        CosmeticType.UITheme => "🎨",
        CosmeticType.AbilityVFX => "✨",
        CosmeticType.CombatLogStyle => "📜",
        CosmeticType.CharacterFrame => "🖼",
        CosmeticType.Emblem => "🏆",
        _ => "•"
    };

    public CosmeticViewModel(Cosmetic cosmetic)
    {
        _cosmetic = cosmetic;
    }
}
