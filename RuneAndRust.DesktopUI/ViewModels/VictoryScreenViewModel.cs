using ReactiveUI;
using RuneAndRust.DesktopUI.Controllers;
using RuneAndRust.DesktopUI.Services;
using Serilog;
using System.Windows.Input;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// v0.44.7: View model for the victory screen.
/// Displays run statistics, rewards, achievements, and transitions to endgame mode selection.
/// </summary>
public class VictoryScreenViewModel : ViewModelBase
{
    private readonly VictoryController? _victoryController;
    private readonly INavigationService? _navigationService;
    private readonly ILogger? _logger;
    private VictoryStatistics? _statistics;

    /// <summary>
    /// Command to proceed to endgame mode selection.
    /// </summary>
    public ICommand ProceedToEndgameCommand { get; }

    /// <summary>
    /// Command to return to main menu.
    /// </summary>
    public ICommand ReturnToMenuCommand { get; }

    /// <summary>
    /// Gets the screen title.
    /// </summary>
    public string Title => "VICTORY";

    /// <summary>
    /// Gets the subtitle.
    /// </summary>
    public string Subtitle => "The Sector has been conquered!";

    /// <summary>
    /// Gets or sets the victory statistics.
    /// </summary>
    public VictoryStatistics? Statistics
    {
        get => _statistics;
        set => this.RaiseAndSetIfChanged(ref _statistics, value);
    }

    // Convenience properties for XAML binding
    public string SurvivorName => Statistics?.SurvivorName ?? "Unknown Survivor";
    public string SurvivorDetails => $"{Statistics?.CharacterClass ?? "Unknown"} ({Statistics?.Specialization ?? "None"})";

    // Progression stats
    public int FinalLevel => Statistics?.FinalLevel ?? 0;
    public int TotalLegendEarned => Statistics?.TotalLegendEarned ?? 0;
    public int LegendToHallOfLegends => Statistics?.LegendToHallOfLegends ?? 0;
    public int ProgressionPointsSpent => Statistics?.ProgressionPointsSpent ?? 0;

    // Trauma stats
    public int FinalPsychicStress => Statistics?.FinalPsychicStress ?? 0;
    public int FinalCorruption => Statistics?.FinalCorruption ?? 0;
    public int TraumaCount => Statistics?.TraumaCount ?? 0;

    // Combat stats
    public int TotalKills => Statistics?.TotalKills ?? 0;

    // Exploration stats
    public int RoomsExplored => Statistics?.RoomsExplored ?? 0;
    public int TotalRooms => Statistics?.TotalRooms ?? 0;
    public int SecretsFound => Statistics?.SecretsFound ?? 0;
    public int ExplorationPercentage => Statistics?.ExplorationPercentage ?? 0;

    // Economy stats
    public int FinalCurrency => Statistics?.FinalCurrency ?? 0;

    // Time stats
    public string FormattedPlaytime => Statistics?.FormattedPlaytime ?? "0m";

    // Performance
    public string PerformanceRating => Statistics?.PerformanceRating ?? "?";

    // Mode info
    public int NGPlusTier => Statistics?.NGPlusTier ?? 0;
    public bool IsNGPlusRun => NGPlusTier > 0;
    public string? ChallengeSectorName => Statistics?.ChallengeSectorName;
    public bool IsChallengeSectorRun => !string.IsNullOrEmpty(ChallengeSectorName);

    // Display messages
    public string HallOfLegendsMessage => LegendToHallOfLegends > 0
        ? $"+{LegendToHallOfLegends} Legend added to Hall of Legends (Victory Bonus!)"
        : "Legend earned for Hall of Legends";

    public string RatingMessage => PerformanceRating switch
    {
        "S" => "Legendary Performance!",
        "A" => "Excellent Run!",
        "B" => "Well Done!",
        "C" => "Good Effort!",
        _ => "Sector Conquered!"
    };

    /// <summary>
    /// Design-time constructor.
    /// </summary>
    public VictoryScreenViewModel()
    {
        ProceedToEndgameCommand = ReactiveCommand.CreateFromTask(OnProceedToEndgameAsync);
        ReturnToMenuCommand = ReactiveCommand.CreateFromTask(OnReturnToMenuAsync);

        // Design-time sample data
        Statistics = new VictoryStatistics
        {
            SurvivorName = "Test Survivor",
            CharacterClass = "Warrior",
            Specialization = "Skar-Horde Aspirant",
            FinalLevel = 8,
            TotalLegendEarned = 4500,
            LegendToHallOfLegends = 900, // 20% for victory
            ProgressionPointsSpent = 6,
            FinalPsychicStress = 35,
            FinalCorruption = 20,
            TraumaCount = 1,
            TotalKills = 52,
            RoomsExplored = 28,
            TotalRooms = 32,
            SecretsFound = 3,
            FinalCurrency = 450,
            PlaytimeMinutes = 95,
            VictoryTimestamp = DateTime.UtcNow,
            NGPlusTier = 0,
            WasVictory = true
        };
    }

    /// <summary>
    /// Initializes with VictoryController (preferred constructor for v0.44.7+).
    /// </summary>
    public VictoryScreenViewModel(
        VictoryController victoryController,
        INavigationService navigationService,
        ILogger logger) : this()
    {
        _victoryController = victoryController ?? throw new ArgumentNullException(nameof(victoryController));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Subscribe to statistics calculated event
        _victoryController.StatisticsCalculated += OnStatisticsCalculated;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        // Load current victory statistics if available
        var stats = _victoryController?.GetCurrentVictoryStatistics();
        if (stats != null)
        {
            Statistics = stats;
            RaisePropertyChangedForAllStats();
        }

        _logger?.Debug("Victory screen activated: {Name} - Rating {Rating}",
            Statistics?.SurvivorName, Statistics?.PerformanceRating);
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        // Unsubscribe from events
        if (_victoryController != null)
        {
            _victoryController.StatisticsCalculated -= OnStatisticsCalculated;
        }
    }

    private void OnStatisticsCalculated(object? sender, VictoryStatistics stats)
    {
        Statistics = stats;
        RaisePropertyChangedForAllStats();

        _logger?.Information("Victory screen received statistics: Level={Level}, Legend={Legend}, Rating={Rating}",
            stats.FinalLevel, stats.TotalLegendEarned, stats.PerformanceRating);
    }

    private async Task OnProceedToEndgameAsync()
    {
        _logger?.Information("Player proceeding to endgame mode selection");

        if (_victoryController != null)
        {
            await _victoryController.OnVictoryAcknowledgedAsync();
        }
        else if (_navigationService != null)
        {
            // Fallback to direct navigation
            _navigationService.NavigateTo<EndgameModeViewModel>();
        }
    }

    private async Task OnReturnToMenuAsync()
    {
        _logger?.Information("Player returning to main menu from victory");

        if (_victoryController != null)
        {
            await _victoryController.OnReturnToMainMenuAsync();
        }
        else if (_navigationService != null)
        {
            // Fallback to direct navigation
            _navigationService.NavigateTo<MenuViewModel>();
        }
    }

    /// <summary>
    /// Sets the statistics and updates all bound properties.
    /// </summary>
    public void SetStatistics(VictoryStatistics stats)
    {
        Statistics = stats;
        RaisePropertyChangedForAllStats();
    }

    /// <summary>
    /// Raises property changed for all statistics-derived properties.
    /// </summary>
    private void RaisePropertyChangedForAllStats()
    {
        this.RaisePropertyChanged(nameof(SurvivorName));
        this.RaisePropertyChanged(nameof(SurvivorDetails));
        this.RaisePropertyChanged(nameof(FinalLevel));
        this.RaisePropertyChanged(nameof(TotalLegendEarned));
        this.RaisePropertyChanged(nameof(LegendToHallOfLegends));
        this.RaisePropertyChanged(nameof(ProgressionPointsSpent));
        this.RaisePropertyChanged(nameof(FinalPsychicStress));
        this.RaisePropertyChanged(nameof(FinalCorruption));
        this.RaisePropertyChanged(nameof(TraumaCount));
        this.RaisePropertyChanged(nameof(TotalKills));
        this.RaisePropertyChanged(nameof(RoomsExplored));
        this.RaisePropertyChanged(nameof(TotalRooms));
        this.RaisePropertyChanged(nameof(SecretsFound));
        this.RaisePropertyChanged(nameof(ExplorationPercentage));
        this.RaisePropertyChanged(nameof(FinalCurrency));
        this.RaisePropertyChanged(nameof(FormattedPlaytime));
        this.RaisePropertyChanged(nameof(PerformanceRating));
        this.RaisePropertyChanged(nameof(NGPlusTier));
        this.RaisePropertyChanged(nameof(IsNGPlusRun));
        this.RaisePropertyChanged(nameof(ChallengeSectorName));
        this.RaisePropertyChanged(nameof(IsChallengeSectorRun));
        this.RaisePropertyChanged(nameof(HallOfLegendsMessage));
        this.RaisePropertyChanged(nameof(RatingMessage));
    }
}
