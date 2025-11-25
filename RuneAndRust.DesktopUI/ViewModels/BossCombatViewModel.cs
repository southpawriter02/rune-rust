using Avalonia.Threading;
using ReactiveUI;
using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using RuneAndRust.DesktopUI.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// v0.43.17: View model for boss-specific combat UI elements.
/// Manages boss phase display, mechanic warnings, enrage timer, and phase transitions.
/// </summary>
public class BossCombatViewModel : ViewModelBase
{
    private readonly IBossDisplayService _bossDisplayService;
    private readonly IAnimationService _animationService;

    private Enemy? _currentBoss;
    private BossDisplayData? _displayData;
    private int _currentPhase = 1;
    private float _enrageProgress = 0f;
    private bool _isTransitioning;
    private int _turnCount;
    private int _enrageTurn = 30; // Default enrage turn

    /// <summary>
    /// Whether this is a boss fight.
    /// </summary>
    public bool IsBossFight => _currentBoss != null;

    /// <summary>
    /// The current boss enemy.
    /// </summary>
    public Enemy? CurrentBoss
    {
        get => _currentBoss;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentBoss, value);
            this.RaisePropertyChanged(nameof(IsBossFight));
            this.RaisePropertyChanged(nameof(BossName));
            this.RaisePropertyChanged(nameof(BossTitle));
            this.RaisePropertyChanged(nameof(CurrentHP));
            this.RaisePropertyChanged(nameof(MaxHP));
            this.RaisePropertyChanged(nameof(HPPercentage));
            UpdateBossDisplay();
        }
    }

    /// <summary>
    /// Boss display data.
    /// </summary>
    public BossDisplayData? DisplayData
    {
        get => _displayData;
        private set => this.RaiseAndSetIfChanged(ref _displayData, value);
    }

    /// <summary>
    /// Current phase number.
    /// </summary>
    public int CurrentPhase
    {
        get => _currentPhase;
        set
        {
            var oldPhase = _currentPhase;
            this.RaiseAndSetIfChanged(ref _currentPhase, value);
            if (oldPhase != value)
            {
                UpdatePhaseSegments();
            }
        }
    }

    /// <summary>
    /// Total number of phases.
    /// </summary>
    public int TotalPhases => _displayData?.TotalPhases ?? 3;

    /// <summary>
    /// Enrage progress (0.0 - 1.0).
    /// </summary>
    public float EnrageProgress
    {
        get => _enrageProgress;
        set => this.RaiseAndSetIfChanged(ref _enrageProgress, Math.Clamp(value, 0f, 1f));
    }

    /// <summary>
    /// Whether the boss is enraged.
    /// </summary>
    public bool IsEnraged => EnrageProgress >= 1.0f;

    /// <summary>
    /// Whether a phase transition is in progress.
    /// </summary>
    public bool IsTransitioning
    {
        get => _isTransitioning;
        set => this.RaiseAndSetIfChanged(ref _isTransitioning, value);
    }

    /// <summary>
    /// Whether the boss is in a vulnerable state.
    /// </summary>
    public bool IsVulnerable => _displayData?.IsVulnerable ?? false;

    /// <summary>
    /// Turns remaining in vulnerable state.
    /// </summary>
    public int VulnerableTurnsRemaining => _displayData?.VulnerableTurnsRemaining ?? 0;

    #region Boss Properties

    /// <summary>
    /// Boss name.
    /// </summary>
    public string BossName => _currentBoss?.Name ?? string.Empty;

    /// <summary>
    /// Boss title.
    /// </summary>
    public string BossTitle => _displayData?.BossTitle ?? string.Empty;

    /// <summary>
    /// Current HP.
    /// </summary>
    public int CurrentHP => _currentBoss?.HP ?? 0;

    /// <summary>
    /// Maximum HP.
    /// </summary>
    public int MaxHP => _currentBoss?.MaxHP ?? 1;

    /// <summary>
    /// HP percentage (0.0 - 1.0).
    /// </summary>
    public float HPPercentage => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;

    #endregion

    #region Collections

    /// <summary>
    /// Phase health segments for the boss health bar.
    /// </summary>
    public ObservableCollection<PhaseHealthSegmentViewModel> PhaseSegments { get; } = new();

    /// <summary>
    /// Active mechanic warnings.
    /// </summary>
    public ObservableCollection<BossMechanicWarningViewModel> ActiveWarnings { get; } = new();

    #endregion

    #region Commands

    /// <summary>
    /// Command to dismiss a warning.
    /// </summary>
    public ICommand DismissWarningCommand { get; }

    #endregion

    /// <summary>
    /// Creates a new instance for design-time support.
    /// </summary>
    public BossCombatViewModel()
    {
        _bossDisplayService = null!;
        _animationService = null!;

        DismissWarningCommand = ReactiveCommand.Create<BossMechanicWarningViewModel>(_ => { });

        // Load design-time data
        LoadDesignTimeData();
    }

    /// <summary>
    /// Creates a new instance with dependency injection.
    /// </summary>
    public BossCombatViewModel(
        IBossDisplayService bossDisplayService,
        IAnimationService animationService)
    {
        _bossDisplayService = bossDisplayService;
        _animationService = animationService;

        DismissWarningCommand = ReactiveCommand.Create<BossMechanicWarningViewModel>(OnDismissWarning);
    }

    /// <summary>
    /// Initializes the boss combat view model with a boss enemy.
    /// </summary>
    public void InitializeBossFight(Enemy boss)
    {
        CurrentBoss = boss;
        _turnCount = 0;
        EnrageProgress = 0f;
        ActiveWarnings.Clear();
        UpdateBossDisplay();
    }

    /// <summary>
    /// Clears the boss fight state.
    /// </summary>
    public void ClearBossFight()
    {
        CurrentBoss = null;
        DisplayData = null;
        PhaseSegments.Clear();
        ActiveWarnings.Clear();
        EnrageProgress = 0f;
        _turnCount = 0;
    }

    /// <summary>
    /// Updates the boss display when HP changes.
    /// </summary>
    public void UpdateBossHP()
    {
        this.RaisePropertyChanged(nameof(CurrentHP));
        this.RaisePropertyChanged(nameof(HPPercentage));
        CheckPhaseTransition();
    }

    /// <summary>
    /// Called when a turn ends to update enrage progress.
    /// </summary>
    public void OnTurnEnd()
    {
        _turnCount++;
        EnrageProgress = _bossDisplayService?.CalculateEnrageProgress(_turnCount, _enrageTurn) ?? 0f;
    }

    /// <summary>
    /// Handles a boss phase transition.
    /// </summary>
    public async Task OnBossPhaseTransitionAsync(int newPhase)
    {
        IsTransitioning = true;

        CurrentPhase = newPhase;
        UpdatePhaseSegments();

        // Play phase transition animation
        if (_animationService != null)
        {
            await _animationService.PlayPhaseTransitionAsync(newPhase);
        }

        IsTransitioning = false;
    }

    /// <summary>
    /// Adds a mechanic warning for a telegraphed ability.
    /// </summary>
    public void AddMechanicWarning(string abilityName, string description, int turnsRemaining, DangerLevel dangerLevel, bool canInterrupt)
    {
        var warning = _bossDisplayService?.CreateMechanicWarning(abilityName, description, turnsRemaining, dangerLevel, canInterrupt)
            ?? new BossMechanicWarning
            {
                MechanicName = abilityName,
                Description = description,
                TurnsRemaining = turnsRemaining,
                DangerLevel = dangerLevel,
                CanBeInterrupted = canInterrupt
            };

        var warningVm = new BossMechanicWarningViewModel(warning);
        ActiveWarnings.Add(warningVm);
    }

    /// <summary>
    /// Decrements warning turns and removes expired warnings.
    /// </summary>
    public void UpdateWarningTurns()
    {
        var expiredWarnings = new List<BossMechanicWarningViewModel>();

        foreach (var warning in ActiveWarnings)
        {
            warning.TurnsRemaining--;
            if (warning.TurnsRemaining <= 0)
            {
                expiredWarnings.Add(warning);
            }
        }

        foreach (var expired in expiredWarnings)
        {
            ActiveWarnings.Remove(expired);
        }
    }

    /// <summary>
    /// Removes a specific warning (e.g., when interrupted).
    /// </summary>
    public void RemoveWarning(string warningId)
    {
        var warning = ActiveWarnings.FirstOrDefault(w => w.WarningId == warningId);
        if (warning != null)
        {
            ActiveWarnings.Remove(warning);
        }
    }

    private void UpdateBossDisplay()
    {
        if (_currentBoss == null || _bossDisplayService == null)
        {
            DisplayData = null;
            PhaseSegments.Clear();
            return;
        }

        DisplayData = _bossDisplayService.GetBossDisplayData(_currentBoss);
        CurrentPhase = _currentBoss.Phase;
        UpdatePhaseSegments();
    }

    private void UpdatePhaseSegments()
    {
        PhaseSegments.Clear();

        if (_currentBoss == null || _bossDisplayService == null) return;

        var segments = _bossDisplayService.GetPhaseSegments(_currentBoss, CurrentPhase);
        foreach (var segment in segments)
        {
            PhaseSegments.Add(new PhaseHealthSegmentViewModel(segment));
        }
    }

    private void CheckPhaseTransition()
    {
        if (_currentBoss == null) return;

        var hpPercent = HPPercentage;
        var expectedPhase = hpPercent switch
        {
            > 0.66f => 1,
            > 0.33f => 2,
            _ => 3
        };

        if (expectedPhase != CurrentPhase)
        {
            _ = OnBossPhaseTransitionAsync(expectedPhase);
        }
    }

    private void OnDismissWarning(BossMechanicWarningViewModel warning)
    {
        ActiveWarnings.Remove(warning);
    }

    private void LoadDesignTimeData()
    {
        // Sample boss data for design-time
        _currentPhase = 2;
        _enrageProgress = 0.45f;

        // Sample phase segments
        PhaseSegments.Add(new PhaseHealthSegmentViewModel(new PhaseHealthSegment
        {
            PhaseNumber = 1,
            IsCurrentPhase = false,
            IsCompleted = true,
            HealthThreshold = 1.0f,
            PhaseName = "Phase I"
        }));
        PhaseSegments.Add(new PhaseHealthSegmentViewModel(new PhaseHealthSegment
        {
            PhaseNumber = 2,
            IsCurrentPhase = true,
            IsCompleted = false,
            HealthThreshold = 0.66f,
            PhaseName = "Phase II"
        }));
        PhaseSegments.Add(new PhaseHealthSegmentViewModel(new PhaseHealthSegment
        {
            PhaseNumber = 3,
            IsCurrentPhase = false,
            IsCompleted = false,
            HealthThreshold = 0.33f,
            PhaseName = "Phase III"
        }));

        // Sample warnings
        ActiveWarnings.Add(new BossMechanicWarningViewModel(new BossMechanicWarning
        {
            MechanicName = "SYSTEM OVERLOAD",
            Description = "Heavy AoE damage incoming! Spread out!",
            TurnsRemaining = 2,
            DangerLevel = DangerLevel.High,
            CanBeInterrupted = true
        }));
    }
}

/// <summary>
/// v0.43.17: View model wrapper for PhaseHealthSegment display.
/// </summary>
public class PhaseHealthSegmentViewModel : ViewModelBase
{
    private readonly PhaseHealthSegment _segment;

    public PhaseHealthSegment Segment => _segment;

    public int PhaseNumber => _segment.PhaseNumber;
    public bool IsCurrentPhase => _segment.IsCurrentPhase;
    public bool IsCompleted => _segment.IsCompleted;
    public float HealthThreshold => _segment.HealthThreshold;
    public string PhaseName => _segment.PhaseName;

    /// <summary>
    /// Display text for the phase.
    /// </summary>
    public string DisplayText => PhaseNumber.ToString();

    /// <summary>
    /// Background color based on phase state.
    /// </summary>
    public string BackgroundColor => IsCompleted ? "#4CAF50" : IsCurrentPhase ? "#DC143C" : "#3C3C3C";

    /// <summary>
    /// Border color based on current phase.
    /// </summary>
    public string BorderColor => IsCurrentPhase ? "#FFD700" : "Transparent";

    /// <summary>
    /// Border thickness based on current phase.
    /// </summary>
    public int BorderThickness => IsCurrentPhase ? 3 : 0;

    public PhaseHealthSegmentViewModel(PhaseHealthSegment segment)
    {
        _segment = segment;
    }
}

/// <summary>
/// v0.43.17: View model wrapper for BossMechanicWarning display.
/// </summary>
public class BossMechanicWarningViewModel : ViewModelBase
{
    private readonly BossMechanicWarning _warning;
    private int _turnsRemaining;

    public BossMechanicWarning Warning => _warning;

    public string WarningId => _warning.WarningId;
    public string MechanicName => _warning.MechanicName;
    public string Description => _warning.Description;
    public DangerLevel DangerLevel => _warning.DangerLevel;
    public bool CanBeInterrupted => _warning.CanBeInterrupted;
    public string Icon => _warning.Icon;

    /// <summary>
    /// Turns remaining until execution.
    /// </summary>
    public int TurnsRemaining
    {
        get => _turnsRemaining;
        set => this.RaiseAndSetIfChanged(ref _turnsRemaining, value);
    }

    /// <summary>
    /// Color based on danger level.
    /// </summary>
    public string WarningColor => DangerLevel switch
    {
        DangerLevel.Low => "#FFC107",      // Yellow/Amber
        DangerLevel.Medium => "#FF9800",   // Orange
        DangerLevel.High => "#F44336",     // Red
        DangerLevel.Lethal => "#B71C1C",   // Dark Red
        _ => "#FFFFFF"
    };

    /// <summary>
    /// Text color for contrast.
    /// </summary>
    public string TextColor => "White";

    /// <summary>
    /// Display text showing turns remaining.
    /// </summary>
    public string TurnsDisplay => TurnsRemaining == 1 ? "1 TURN" : $"{TurnsRemaining} TURNS";

    /// <summary>
    /// Interrupt hint text.
    /// </summary>
    public string InterruptHint => CanBeInterrupted ? "Can be interrupted!" : "Cannot be interrupted";

    public BossMechanicWarningViewModel(BossMechanicWarning warning)
    {
        _warning = warning;
        _turnsRemaining = warning.TurnsRemaining;
    }
}
