using ReactiveUI;
using RuneAndRust.Core.BossGauntlet;
using RuneAndRust.Core.ChallengeSectors;
using RuneAndRust.DesktopUI.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// v0.43.16: View model for endgame mode selection and configuration.
/// Allows players to select NG+ tiers, Challenge Sectors, Boss Gauntlet, and Endless Mode.
/// </summary>
public class EndgameModeViewModel : ViewModelBase
{
    private readonly IEndgameService _endgameService;
    private readonly INavigationService _navigationService;
    private EndgameMode _selectedMode = EndgameMode.NGPlus;
    private int _selectedNGPlusTier = 1;
    private ChallengeSectorViewModel? _selectedSector;
    private GauntletSequenceViewModel? _selectedGauntlet;

    /// <summary>
    /// Collection of available endgame modes.
    /// </summary>
    public ObservableCollection<EndgameMode> AvailableModes { get; } = new();

    /// <summary>
    /// Collection of active difficulty modifiers.
    /// </summary>
    public ObservableCollection<DifficultyModifierViewModel> ActiveModifiers { get; } = new();

    /// <summary>
    /// Collection of reward multipliers.
    /// </summary>
    public ObservableCollection<RewardMultiplierViewModel> RewardMultipliers { get; } = new();

    /// <summary>
    /// Collection of available Challenge Sectors.
    /// </summary>
    public ObservableCollection<ChallengeSectorViewModel> ChallengeSectors { get; } = new();

    /// <summary>
    /// Collection of available Boss Gauntlet sequences.
    /// </summary>
    public ObservableCollection<GauntletSequenceViewModel> GauntletSequences { get; } = new();

    /// <summary>
    /// Currently selected endgame mode.
    /// </summary>
    public EndgameMode SelectedMode
    {
        get => _selectedMode;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedMode, value);
            UpdateModeInfo();
            this.RaisePropertyChanged(nameof(IsNGPlusMode));
            this.RaisePropertyChanged(nameof(IsChallengeSectorMode));
            this.RaisePropertyChanged(nameof(IsBossGauntletMode));
            this.RaisePropertyChanged(nameof(IsEndlessMode));
            this.RaisePropertyChanged(nameof(ModeDescription));
            this.RaisePropertyChanged(nameof(CanStartMode));
        }
    }

    /// <summary>
    /// Selected NG+ tier (1-5).
    /// </summary>
    public int SelectedNGPlusTier
    {
        get => _selectedNGPlusTier;
        set
        {
            var clamped = Math.Clamp(value, 1, MaxUnlockedNGPlusTier);
            this.RaiseAndSetIfChanged(ref _selectedNGPlusTier, clamped);
            UpdateModifiers();
            UpdateRewards();
            this.RaisePropertyChanged(nameof(CanDecrementTier));
            this.RaisePropertyChanged(nameof(CanIncrementTier));
        }
    }

    /// <summary>
    /// Selected Challenge Sector.
    /// </summary>
    public ChallengeSectorViewModel? SelectedSector
    {
        get => _selectedSector;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSector, value);
            if (SelectedMode == EndgameMode.ChallengeSector)
            {
                UpdateModifiers();
                UpdateRewards();
            }
            this.RaisePropertyChanged(nameof(CanStartMode));
        }
    }

    /// <summary>
    /// Selected Boss Gauntlet sequence.
    /// </summary>
    public GauntletSequenceViewModel? SelectedGauntlet
    {
        get => _selectedGauntlet;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedGauntlet, value);
            if (SelectedMode == EndgameMode.BossGauntlet)
            {
                UpdateModifiers();
                UpdateRewards();
            }
            this.RaisePropertyChanged(nameof(CanStartMode));
        }
    }

    /// <summary>
    /// Maximum unlocked NG+ tier.
    /// </summary>
    public int MaxUnlockedNGPlusTier { get; private set; } = 1;

    /// <summary>
    /// Endless mode high score (waves survived).
    /// </summary>
    public int EndlessModeHighScore { get; private set; }

    #region Mode Visibility Properties

    /// <summary>
    /// Whether NG+ mode is selected.
    /// </summary>
    public bool IsNGPlusMode => SelectedMode == EndgameMode.NGPlus;

    /// <summary>
    /// Whether Challenge Sector mode is selected.
    /// </summary>
    public bool IsChallengeSectorMode => SelectedMode == EndgameMode.ChallengeSector;

    /// <summary>
    /// Whether Boss Gauntlet mode is selected.
    /// </summary>
    public bool IsBossGauntletMode => SelectedMode == EndgameMode.BossGauntlet;

    /// <summary>
    /// Whether Endless Mode is selected.
    /// </summary>
    public bool IsEndlessMode => SelectedMode == EndgameMode.EndlessMode;

    #endregion

    #region Computed Properties

    /// <summary>
    /// Description of the currently selected mode.
    /// </summary>
    public string ModeDescription => SelectedMode switch
    {
        EndgameMode.NGPlus => "Restart the dungeon with increased difficulty and better rewards. " +
                             "Your character's progress carries over as you face tougher challenges.",
        EndgameMode.ChallengeSector => "Enter handcrafted extreme difficulty challenges with unique modifiers. " +
                                       "Complete sectors to earn exclusive legendary rewards.",
        EndgameMode.BossGauntlet => "Face a sequence of bosses with limited healing and revives. " +
                                    "Test your skills against the mightiest foes in succession.",
        EndgameMode.EndlessMode => "Survive waves of increasing difficulty for as long as possible. " +
                                   "Compete for seasonal leaderboard rankings.",
        _ => string.Empty
    };

    /// <summary>
    /// Whether the current mode can be started.
    /// </summary>
    public bool CanStartMode => SelectedMode switch
    {
        EndgameMode.NGPlus => SelectedNGPlusTier <= MaxUnlockedNGPlusTier,
        EndgameMode.ChallengeSector => SelectedSector != null,
        EndgameMode.BossGauntlet => SelectedGauntlet != null,
        EndgameMode.EndlessMode => true,
        _ => false
    };

    /// <summary>
    /// Whether the tier can be decremented.
    /// </summary>
    public bool CanDecrementTier => SelectedNGPlusTier > 1;

    /// <summary>
    /// Whether the tier can be incremented.
    /// </summary>
    public bool CanIncrementTier => SelectedNGPlusTier < MaxUnlockedNGPlusTier;

    #endregion

    #region Commands

    /// <summary>
    /// Command to return to previous view.
    /// </summary>
    public ICommand BackCommand { get; }

    /// <summary>
    /// Command to start the selected endgame mode.
    /// </summary>
    public ICommand StartModeCommand { get; }

    /// <summary>
    /// Command to increment the NG+ tier.
    /// </summary>
    public ICommand IncrementTierCommand { get; }

    /// <summary>
    /// Command to decrement the NG+ tier.
    /// </summary>
    public ICommand DecrementTierCommand { get; }

    /// <summary>
    /// Command to select an endgame mode.
    /// </summary>
    public ICommand SelectModeCommand { get; }

    #endregion

    /// <summary>
    /// Creates a new instance for design-time support.
    /// </summary>
    public EndgameModeViewModel()
    {
        _endgameService = null!;
        _navigationService = null!;

        BackCommand = ReactiveCommand.Create(() => { });
        StartModeCommand = ReactiveCommand.Create(() => { });
        IncrementTierCommand = ReactiveCommand.Create(() => { });
        DecrementTierCommand = ReactiveCommand.Create(() => { });
        SelectModeCommand = ReactiveCommand.Create<EndgameMode>(_ => { });

        // Load sample data for design-time
        LoadDesignTimeData();
    }

    /// <summary>
    /// Creates a new instance with dependency injection.
    /// </summary>
    public EndgameModeViewModel(
        IEndgameService endgameService,
        INavigationService navigationService)
    {
        _endgameService = endgameService;
        _navigationService = navigationService;

        BackCommand = ReactiveCommand.Create(OnBack);
        StartModeCommand = ReactiveCommand.CreateFromTask(StartModeAsync);
        IncrementTierCommand = ReactiveCommand.Create(OnIncrementTier);
        DecrementTierCommand = ReactiveCommand.Create(OnDecrementTier);
        SelectModeCommand = ReactiveCommand.Create<EndgameMode>(OnSelectMode);

        LoadAvailableContent();
    }

    /// <summary>
    /// Loads all available endgame content from the service.
    /// </summary>
    public void LoadAvailableContent()
    {
        var content = _endgameService.GetAvailableContent();

        MaxUnlockedNGPlusTier = content.MaxUnlockedNGPlusTier;
        EndlessModeHighScore = content.EndlessModeHighScore;

        LoadAvailableModes(content);
        LoadChallengeSectors(content.AvailableSectors);
        LoadGauntletSequences(content.AvailableGauntlets);
        UpdateModeInfo();

        this.RaisePropertyChanged(nameof(MaxUnlockedNGPlusTier));
        this.RaisePropertyChanged(nameof(EndlessModeHighScore));
    }

    private void LoadAvailableModes(EndgameContentAvailability content)
    {
        AvailableModes.Clear();

        // NG+ is always available
        AvailableModes.Add(EndgameMode.NGPlus);

        if (content.ChallengeSectorsUnlocked)
            AvailableModes.Add(EndgameMode.ChallengeSector);

        if (content.BossGauntletUnlocked)
            AvailableModes.Add(EndgameMode.BossGauntlet);

        if (content.EndlessModeUnlocked)
            AvailableModes.Add(EndgameMode.EndlessMode);
    }

    private void LoadChallengeSectors(List<ChallengeSector> sectors)
    {
        ChallengeSectors.Clear();
        foreach (var sector in sectors)
        {
            ChallengeSectors.Add(new ChallengeSectorViewModel(sector));
        }
    }

    private void LoadGauntletSequences(List<GauntletSequence> gauntlets)
    {
        GauntletSequences.Clear();
        foreach (var gauntlet in gauntlets)
        {
            GauntletSequences.Add(new GauntletSequenceViewModel(gauntlet));
        }
    }

    private void UpdateModeInfo()
    {
        UpdateModifiers();
        UpdateRewards();
    }

    private void UpdateModifiers()
    {
        ActiveModifiers.Clear();

        var modifiers = SelectedMode switch
        {
            EndgameMode.NGPlus => _endgameService.GetNGPlusModifiers(SelectedNGPlusTier),
            EndgameMode.ChallengeSector => _endgameService.GetChallengeSectorModifiers(
                SelectedSector != null ? new ChallengeSectorConfig { Sector = SelectedSector.Sector } : null),
            EndgameMode.BossGauntlet => _endgameService.GetBossGauntletModifiers(),
            EndgameMode.EndlessMode => _endgameService.GetEndlessModeModifiers(),
            _ => new List<DifficultyModifier>()
        };

        foreach (var modifier in modifiers)
        {
            ActiveModifiers.Add(new DifficultyModifierViewModel(modifier));
        }
    }

    private void UpdateRewards()
    {
        RewardMultipliers.Clear();

        var rewards = _endgameService.GetRewardMultipliers(SelectedMode, SelectedNGPlusTier);

        foreach (var reward in rewards)
        {
            RewardMultipliers.Add(new RewardMultiplierViewModel(reward));
        }
    }

    private async Task StartModeAsync()
    {
        var config = new EndgameModeConfig
        {
            Mode = SelectedMode,
            NGPlusTier = SelectedNGPlusTier,
            ChallengeSectorConfig = SelectedSector != null
                ? new ChallengeSectorConfig { Sector = SelectedSector.Sector }
                : null,
            GauntletSequence = SelectedGauntlet?.Gauntlet
        };

        var success = await _endgameService.StartEndgameModeAsync(config);

        if (success)
        {
            // Navigate to dungeon exploration (placeholder for now)
            // _navigationService.NavigateTo<DungeonExplorationViewModel>();
            Console.WriteLine($"[EndgameModeViewModel] Started {SelectedMode} mode");
        }
    }

    private void OnBack()
    {
        _navigationService.NavigateBack();
    }

    private void OnIncrementTier()
    {
        if (SelectedNGPlusTier < MaxUnlockedNGPlusTier)
        {
            SelectedNGPlusTier++;
        }
    }

    private void OnDecrementTier()
    {
        if (SelectedNGPlusTier > 1)
        {
            SelectedNGPlusTier--;
        }
    }

    private void OnSelectMode(EndgameMode mode)
    {
        SelectedMode = mode;
    }

    private void LoadDesignTimeData()
    {
        MaxUnlockedNGPlusTier = 3;
        EndlessModeHighScore = 47;

        AvailableModes.Add(EndgameMode.NGPlus);
        AvailableModes.Add(EndgameMode.ChallengeSector);
        AvailableModes.Add(EndgameMode.BossGauntlet);
        AvailableModes.Add(EndgameMode.EndlessMode);

        // Sample modifiers
        ActiveModifiers.Add(new DifficultyModifierViewModel(new DifficultyModifier
        {
            Name = "Enemy Power",
            Description = "Enemy HP and damage increased",
            Type = ModifierType.Percentage,
            Value = 0.5f
        }));
        ActiveModifiers.Add(new DifficultyModifierViewModel(new DifficultyModifier
        {
            Name = "Enemy Levels",
            Description = "Enemies gain additional levels",
            Type = ModifierType.Flat,
            Value = 2
        }));

        // Sample rewards
        RewardMultipliers.Add(new RewardMultiplierViewModel(new RewardMultiplier
        {
            Type = "Legend Points",
            Multiplier = 1.15f
        }));
        RewardMultipliers.Add(new RewardMultiplierViewModel(new RewardMultiplier
        {
            Type = "Loot Quality",
            Multiplier = 1.1f
        }));

        // Sample sectors
        ChallengeSectors.Add(new ChallengeSectorViewModel(new ChallengeSector
        {
            Name = "Iron Gauntlet",
            Description = "Face endless waves of armored foes.",
            TotalDifficultyMultiplier = 2.2f // Results in "Hard" difficulty tier
        }));
    }
}

/// <summary>
/// v0.43.16: View model wrapper for DifficultyModifier display.
/// </summary>
public class DifficultyModifierViewModel : ViewModelBase
{
    private readonly DifficultyModifier _modifier;

    public DifficultyModifier Modifier => _modifier;

    public string Name => _modifier.Name;
    public string Description => _modifier.Description;
    public float Value => _modifier.Value;
    public ModifierType Type => _modifier.Type;
    public string Category => _modifier.Category;
    public bool IsDetrimental => _modifier.IsDetrimental;

    /// <summary>
    /// Whether the value is positive (makes game harder).
    /// </summary>
    public bool IsPositive => Value > 0;

    /// <summary>
    /// Formatted display value based on modifier type.
    /// </summary>
    public string DisplayValue
    {
        get
        {
            var sign = Value >= 0 ? "+" : "";
            return Type switch
            {
                ModifierType.Percentage => $"{sign}{Value * 100:F0}%",
                ModifierType.Flat => $"{sign}{Value:F0}",
                ModifierType.Multiplier => $"{Value:F2}x",
                _ => Value.ToString("F1")
            };
        }
    }

    /// <summary>
    /// Color for the modifier value (red for detrimental, green for beneficial).
    /// </summary>
    public string ValueColor => IsDetrimental ? "#DC143C" : "#4CAF50";

    /// <summary>
    /// Icon for the modifier category.
    /// </summary>
    public string CategoryIcon => Category switch
    {
        "Combat" => "\u2694", // Crossed swords
        "Boss" => "\u2620",    // Skull
        "Hazard" => "\u2622",  // Radioactive
        "Spawn" => "\u2728",   // Sparkles
        "Death" => "\u2620",   // Skull
        "Resources" => "\u2764", // Heart
        "Exploration" => "\u26A0", // Warning
        "Format" => "\u2699",  // Gear
        _ => "\u2022"          // Bullet
    };

    public DifficultyModifierViewModel(DifficultyModifier modifier)
    {
        _modifier = modifier;
    }
}

/// <summary>
/// v0.43.16: View model wrapper for RewardMultiplier display.
/// </summary>
public class RewardMultiplierViewModel : ViewModelBase
{
    private readonly RewardMultiplier _reward;

    public RewardMultiplier Reward => _reward;

    public string Type => _reward.Type;
    public float Multiplier => _reward.Multiplier;
    public string Description => _reward.Description;

    /// <summary>
    /// Formatted display text.
    /// </summary>
    public string DisplayText => Multiplier > 1.0f
        ? $"{Type}: x{Multiplier:F1}"
        : Type;

    /// <summary>
    /// Whether this is a bonus multiplier.
    /// </summary>
    public bool IsBonus => Multiplier > 1.0f;

    /// <summary>
    /// Color for the reward (gold for bonus).
    /// </summary>
    public string RewardColor => IsBonus ? "#FFD700" : "#4A90E2";

    public RewardMultiplierViewModel(RewardMultiplier reward)
    {
        _reward = reward;
    }
}

/// <summary>
/// v0.43.16: View model wrapper for ChallengeSector display.
/// </summary>
public class ChallengeSectorViewModel : ViewModelBase
{
    private readonly ChallengeSector _sector;

    public ChallengeSector Sector => _sector;

    public string SectorId => _sector.SectorId;
    public string Name => _sector.Name;
    public string Description => _sector.Description;
    public string? LoreText => _sector.LoreText;
    public string DifficultyTier => _sector.DifficultyTier;
    public float TotalDifficultyMultiplier => _sector.TotalDifficultyMultiplier;
    public int RoomCount => _sector.RoomCount;
    public int RequiredNGPlusTier => _sector.RequiredNGPlusTier;
    public bool IsCompleted => _sector.IsCompleted;
    public int AttemptCount => _sector.AttemptCount;
    public string? UniqueRewardName => _sector.UniqueRewardName;
    public string? UniqueRewardDescription => _sector.UniqueRewardDescription;

    /// <summary>
    /// Formatted difficulty display.
    /// </summary>
    public string DifficultyDisplay => $"{DifficultyTier} ({TotalDifficultyMultiplier:F1}x)";

    /// <summary>
    /// Completion status text.
    /// </summary>
    public string CompletionStatus => IsCompleted
        ? "Completed"
        : AttemptCount > 0 ? $"{AttemptCount} attempts" : "Not attempted";

    /// <summary>
    /// Color based on difficulty tier.
    /// </summary>
    public string DifficultyColor => DifficultyTier switch
    {
        "Moderate" => "#4CAF50",      // Green
        "Hard" => "#FFA500",          // Orange
        "Extreme" => "#DC143C",       // Red
        "Near-Impossible" => "#9400D3", // Purple
        "Impossible" => "#FF0000",    // Bright red
        _ => "#CCCCCC"
    };

    /// <summary>
    /// Background color based on completion state.
    /// </summary>
    public string BackgroundColor => IsCompleted ? "#2D3A2D" : "#1C1C1C";

    /// <summary>
    /// Border color based on completion state.
    /// </summary>
    public string BorderColor => IsCompleted ? "#4CAF50" : "#3C3C3C";

    public ChallengeSectorViewModel(ChallengeSector sector)
    {
        _sector = sector;
    }
}

/// <summary>
/// v0.43.16: View model wrapper for GauntletSequence display.
/// </summary>
public class GauntletSequenceViewModel : ViewModelBase
{
    private readonly GauntletSequence _gauntlet;

    public GauntletSequence Gauntlet => _gauntlet;

    public string SequenceId => _gauntlet.SequenceId;
    public string SequenceName => _gauntlet.SequenceName;
    public string Description => _gauntlet.Description;
    public string DifficultyTier => _gauntlet.DifficultyTier;
    public int BossCount => _gauntlet.BossCount;
    public int MaxFullHeals => _gauntlet.MaxFullHeals;
    public int MaxRevives => _gauntlet.MaxRevives;
    public int RequiredNGPlusTier => _gauntlet.RequiredNGPlusTier;
    public string? TitleReward => _gauntlet.TitleReward;
    public bool IsExtreme => _gauntlet.IsExtreme;

    /// <summary>
    /// Formatted display text.
    /// </summary>
    public string DisplayText => $"{SequenceName} ({BossCount} Bosses)";

    /// <summary>
    /// Resource limits display.
    /// </summary>
    public string ResourceLimits => $"{MaxFullHeals} Heals / {MaxRevives} Revive{(MaxRevives != 1 ? "s" : "")}";

    /// <summary>
    /// Color based on difficulty tier.
    /// </summary>
    public string DifficultyColor => DifficultyTier switch
    {
        "Moderate" => "#4CAF50",   // Green
        "Hard" => "#FFA500",       // Orange
        "Extreme" => "#DC143C",    // Red
        "Nightmare" => "#9400D3",  // Purple
        _ => "#CCCCCC"
    };

    /// <summary>
    /// Title reward display.
    /// </summary>
    public string TitleRewardDisplay => TitleReward != null ? $"Title: {TitleReward}" : "";

    public GauntletSequenceViewModel(GauntletSequence gauntlet)
    {
        _gauntlet = gauntlet;
    }
}
