using ReactiveUI;
using RuneAndRust.DesktopUI.Controllers;
using RuneAndRust.DesktopUI.Services;
using Serilog;
using System;
using System.Windows.Input;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// v0.44.6: View model for the death screen.
/// Displays run statistics, cause of death, and meta-progression rewards.
/// </summary>
public class DeathScreenViewModel : ViewModelBase
{
    private readonly DeathController? _deathController;
    private readonly INavigationService? _navigationService;
    private readonly ILogger? _logger;
    private RunStatistics? _statistics;

    /// <summary>
    /// Command to return to main menu.
    /// </summary>
    public ICommand ReturnToMenuCommand { get; }

    /// <summary>
    /// Gets the screen title.
    /// </summary>
    public string Title => "THE SAGA ENDS";

    /// <summary>
    /// Gets the subtitle.
    /// </summary>
    public string Subtitle => "Your journey has come to an end...";

    /// <summary>
    /// Gets or sets the run statistics.
    /// </summary>
    public RunStatistics? Statistics
    {
        get => _statistics;
        set => this.RaiseAndSetIfChanged(ref _statistics, value);
    }

    // Convenience properties for XAML binding
    public string SurvivorName => Statistics?.SurvivorName ?? "Unknown Survivor";
    public string SurvivorDetails => $"{Statistics?.CharacterClass ?? "Unknown"} ({Statistics?.Specialization ?? "None"})";
    public string CauseOfDeath => Statistics?.CauseOfDeath ?? "Unknown";

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

    // Economy stats
    public int FinalCurrency => Statistics?.FinalCurrency ?? 0;

    // Time stats
    public string FormattedPlaytime => Statistics?.FormattedPlaytime ?? "0m";

    // Display messages
    public string HallOfLegendsMessage => LegendToHallOfLegends > 0
        ? $"+{LegendToHallOfLegends} Legend added to Hall of Legends"
        : "No Legend earned for Hall of Legends";

    /// <summary>
    /// Design-time constructor.
    /// </summary>
    public DeathScreenViewModel()
    {
        ReturnToMenuCommand = ReactiveCommand.CreateFromTask(OnReturnToMenuAsync);

        // Design-time sample data
        Statistics = new RunStatistics
        {
            SurvivorName = "Test Survivor",
            CharacterClass = "Warrior",
            Specialization = "Skar-Horde Aspirant",
            CauseOfDeath = "Slain by Draugr Warlord",
            FinalLevel = 5,
            TotalLegendEarned = 2500,
            LegendToHallOfLegends = 250,
            ProgressionPointsSpent = 4,
            FinalPsychicStress = 65,
            FinalCorruption = 35,
            TraumaCount = 2,
            TotalKills = 28,
            RoomsExplored = 45,
            FinalCurrency = 150,
            PlaytimeMinutes = 125,
            DeathTimestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Initializes with DeathController (preferred constructor for v0.44.6+).
    /// </summary>
    public DeathScreenViewModel(
        DeathController deathController,
        INavigationService navigationService,
        ILogger logger) : this()
    {
        _deathController = deathController ?? throw new ArgumentNullException(nameof(deathController));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Subscribe to statistics calculated event
        _deathController.StatisticsCalculated += OnStatisticsCalculated;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        // Load current run statistics if available
        var stats = _deathController?.GetCurrentRunStatistics();
        if (stats != null)
        {
            Statistics = stats;
            RaisePropertyChangedForAllStats();
        }

        _logger?.Debug("Death screen activated: {Name} - {Cause}",
            Statistics?.SurvivorName, Statistics?.CauseOfDeath);
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        // Unsubscribe from events
        if (_deathController != null)
        {
            _deathController.StatisticsCalculated -= OnStatisticsCalculated;
        }
    }

    private void OnStatisticsCalculated(object? sender, RunStatistics stats)
    {
        Statistics = stats;
        RaisePropertyChangedForAllStats();

        _logger?.Information("Death screen received statistics: Level={Level}, Legend={Legend}",
            stats.FinalLevel, stats.TotalLegendEarned);
    }

    private async Task OnReturnToMenuAsync()
    {
        _logger?.Information("Player acknowledged death, returning to main menu");

        if (_deathController != null)
        {
            await _deathController.OnDeathAcknowledgedAsync();
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
    public void SetStatistics(RunStatistics stats)
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
        this.RaisePropertyChanged(nameof(CauseOfDeath));
        this.RaisePropertyChanged(nameof(FinalLevel));
        this.RaisePropertyChanged(nameof(TotalLegendEarned));
        this.RaisePropertyChanged(nameof(LegendToHallOfLegends));
        this.RaisePropertyChanged(nameof(ProgressionPointsSpent));
        this.RaisePropertyChanged(nameof(FinalPsychicStress));
        this.RaisePropertyChanged(nameof(FinalCorruption));
        this.RaisePropertyChanged(nameof(TraumaCount));
        this.RaisePropertyChanged(nameof(TotalKills));
        this.RaisePropertyChanged(nameof(RoomsExplored));
        this.RaisePropertyChanged(nameof(FinalCurrency));
        this.RaisePropertyChanged(nameof(FormattedPlaytime));
        this.RaisePropertyChanged(nameof(HallOfLegendsMessage));
    }
}
