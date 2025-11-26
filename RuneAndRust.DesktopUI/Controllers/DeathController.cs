using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.DesktopUI.ViewModels;
using RuneAndRust.Engine;
using Serilog;

namespace RuneAndRust.DesktopUI.Controllers;

/// <summary>
/// v0.44.6: Death & Game Over Controller
/// Handles player death gracefully, displays run statistics, updates meta-progression,
/// and returns to the main menu.
///
/// Death Detection:
/// - Combat defeat (HP reaches 0)
/// - Corruption threshold (Runic Blight >= 100)
/// - Psychic Stress threshold (>= 100)
/// </summary>
public class DeathController
{
    private readonly ILogger _logger;
    private readonly GameStateController _gameStateController;
    private readonly INavigationService _navigationService;
    private readonly IMetaProgressionService _metaProgressionService;
    private readonly SagaService _sagaService;

    /// <summary>
    /// Event raised when death handling is complete.
    /// </summary>
    public event EventHandler? DeathHandlingComplete;

    /// <summary>
    /// Event raised when run statistics are calculated.
    /// </summary>
    public event EventHandler<RunStatistics>? StatisticsCalculated;

    public DeathController(
        ILogger logger,
        GameStateController gameStateController,
        INavigationService navigationService,
        IMetaProgressionService metaProgressionService,
        SagaService sagaService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameStateController = gameStateController ?? throw new ArgumentNullException(nameof(gameStateController));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _metaProgressionService = metaProgressionService ?? throw new ArgumentNullException(nameof(metaProgressionService));
        _sagaService = sagaService ?? throw new ArgumentNullException(nameof(sagaService));
    }

    /// <summary>
    /// Handles player death - calculates statistics, updates meta-progression,
    /// and navigates to the death screen.
    /// </summary>
    public async Task HandlePlayerDeathAsync(PlayerCharacter deadSurvivor, string causeOfDeath)
    {
        _logger.Information("Survivor {Name} has died: {Cause}", deadSurvivor.Name, causeOfDeath);

        try
        {
            // Transition to Death state
            await _gameStateController.UpdatePhaseAsync(Core.GamePhase.Death, $"Survivor death: {causeOfDeath}");

            // Calculate run statistics
            var runStats = CalculateRunStatistics(deadSurvivor, causeOfDeath);

            _logger.Information(
                "Run statistics: Level={Level}, Legend={Legend}, HallOfLegends={HoL}, Kills={Kills}",
                runStats.FinalLevel, runStats.TotalLegendEarned, runStats.LegendToHallOfLegends, runStats.TotalKills);

            // Update meta-progression (Legend to Hall of Legends)
            await UpdateMetaProgressionAsync(deadSurvivor, runStats);

            // Raise statistics calculated event
            StatisticsCalculated?.Invoke(this, runStats);

            // Navigate to death screen
            _navigationService.NavigateTo<DeathScreenViewModel>();

            _logger.Information("Death handling complete. Awaiting player acknowledgment.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error handling player death for {Name}", deadSurvivor.Name);
            // Ensure we return to main menu even on error
            ReturnToMainMenu();
        }
    }

    /// <summary>
    /// Called when the player acknowledges the death screen and returns to main menu.
    /// </summary>
    public async Task OnDeathAcknowledgedAsync()
    {
        _logger.Information("Death acknowledged. Returning to main menu.");

        // Reset game state
        _gameStateController.Reset();

        // Navigate to main menu
        _navigationService.NavigateTo<MenuViewModel>();

        DeathHandlingComplete?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Returns directly to main menu (for error recovery).
    /// </summary>
    public void ReturnToMainMenu()
    {
        _gameStateController.Reset();
        _navigationService.NavigateTo<MenuViewModel>();
        _logger.Information("Returned to main menu after death");
    }

    /// <summary>
    /// Checks if the player is dead.
    /// Death conditions: HP <= 0, Corruption >= 100, or PsychicStress >= 100.
    /// </summary>
    public bool IsSurvivorDead(PlayerCharacter? survivor)
    {
        if (survivor == null)
        {
            _logger.Warning("IsSurvivorDead called with null survivor");
            return true;
        }

        bool isDead = survivor.HP <= 0 ||
                      survivor.Corruption >= 100 ||
                      survivor.PsychicStress >= 100;

        if (isDead)
        {
            string cause = DetermineCauseOfDeath(survivor);
            _logger.Information("Survivor {Name} detected as dead: {Cause}", survivor.Name, cause);
        }

        return isDead;
    }

    /// <summary>
    /// Determines the cause of death based on survivor state.
    /// </summary>
    public string DetermineCauseOfDeath(PlayerCharacter survivor)
    {
        if (survivor.HP <= 0)
            return "Slain in combat";

        if (survivor.Corruption >= 100)
            return "Consumed by Runic Blight (Corruption exceeded threshold)";

        if (survivor.PsychicStress >= 100)
            return "Psychological collapse (Psychic Stress exceeded threshold)";

        return "Unknown cause";
    }

    /// <summary>
    /// Determines cause of death with enemy context from combat.
    /// </summary>
    public string DetermineCauseOfDeath(PlayerCharacter survivor, string? lastDamageSource)
    {
        if (survivor.HP <= 0 && !string.IsNullOrEmpty(lastDamageSource))
            return $"Slain by {lastDamageSource}";

        return DetermineCauseOfDeath(survivor);
    }

    /// <summary>
    /// Calculates comprehensive run statistics for the death screen.
    /// </summary>
    private RunStatistics CalculateRunStatistics(PlayerCharacter survivor, string causeOfDeath)
    {
        var gameState = _gameStateController.HasActiveGame ? _gameStateController.CurrentGameState : null;

        var stats = new RunStatistics
        {
            // Basic Info
            SurvivorName = survivor.Name,
            CharacterClass = survivor.Class.ToString(),
            Specialization = survivor.Specialization.ToString(),
            CauseOfDeath = causeOfDeath,

            // Progression
            FinalLevel = survivor.CurrentMilestone,
            TotalLegendEarned = survivor.CurrentLegend, // Legend earned this run
            LegendToHallOfLegends = CalculateLegendForHallOfLegends(survivor),
            ProgressionPointsSpent = CalculateProgressionPointsSpent(survivor),

            // Trauma
            FinalPsychicStress = survivor.PsychicStress,
            FinalCorruption = survivor.Corruption,
            TraumaCount = survivor.Traumas.Count,

            // Combat - estimate from milestone and trauma
            TotalKills = EstimateKills(survivor),

            // Exploration
            RoomsExplored = survivor.RoomsExploredSinceRest + (survivor.CurrentMilestone * 10), // Rough estimate

            // Economy
            FinalCurrency = survivor.Currency,

            // Time
            PlaytimeMinutes = gameState != null ? (int)gameState.PlayTime.TotalMinutes : 0,
            DeathTimestamp = DateTime.UtcNow
        };

        return stats;
    }

    /// <summary>
    /// Calculates how much Legend converts to Hall of Legends (meta-progression currency).
    /// Formula: 10% of total Legend earned (rounded down).
    /// </summary>
    private int CalculateLegendForHallOfLegends(PlayerCharacter survivor)
    {
        int totalLegend = survivor.CurrentLegend;
        int hallOfLegendsAmount = totalLegend / 10; // 10% conversion

        _logger.Debug("Converting {Total} Legend to {HallAmount} Hall of Legends",
            totalLegend, hallOfLegendsAmount);

        return hallOfLegendsAmount;
    }

    /// <summary>
    /// Estimates progression points spent based on milestone and current PP.
    /// </summary>
    private int CalculateProgressionPointsSpent(PlayerCharacter survivor)
    {
        // Each milestone grants PP; starting PP is 2
        // Total PP earned = 2 (starting) + CurrentMilestone (1 per milestone)
        int totalPPEarned = 2 + survivor.CurrentMilestone;
        int ppSpent = totalPPEarned - survivor.ProgressionPoints;
        return Math.Max(0, ppSpent);
    }

    /// <summary>
    /// Estimates total kills based on milestone level.
    /// </summary>
    private int EstimateKills(PlayerCharacter survivor)
    {
        // Rough estimate: ~5 kills per milestone on average
        return survivor.CurrentMilestone * 5 + (survivor.CurrentLegend / 50);
    }

    /// <summary>
    /// Updates meta-progression system with Legend earned and death recorded.
    /// </summary>
    private async Task UpdateMetaProgressionAsync(PlayerCharacter survivor, RunStatistics stats)
    {
        try
        {
            // Add Legend to Hall of Legends via the meta-progression service
            await _metaProgressionService.AddLegendToHallOfLegendsAsync(stats.LegendToHallOfLegends);

            // Record death for statistics
            await _metaProgressionService.RecordDeathAsync(
                survivor.Class.ToString(),
                survivor.Specialization.ToString(),
                survivor.CurrentMilestone,
                stats.FinalCorruption,
                stats.FinalPsychicStress);

            // Check for meta-progression unlocks
            await _metaProgressionService.CheckForUnlocksAsync();

            _logger.Information("Meta-progression updated: {Legend} added to Hall of Legends",
                stats.LegendToHallOfLegends);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error updating meta-progression for {Name}", survivor.Name);
            // Don't throw - allow death handling to continue
        }
    }

    /// <summary>
    /// Gets the current run statistics for display (if available).
    /// </summary>
    public RunStatistics? GetCurrentRunStatistics()
    {
        if (!_gameStateController.HasActiveGame)
            return null;

        var player = _gameStateController.CurrentGameState.Player;
        if (player == null)
            return null;

        return CalculateRunStatistics(player, DetermineCauseOfDeath(player));
    }
}

/// <summary>
/// Data structure for run statistics displayed on death screen.
/// </summary>
public class RunStatistics
{
    // Basic Info
    public string SurvivorName { get; set; } = string.Empty;
    public string CharacterClass { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string CauseOfDeath { get; set; } = string.Empty;

    // Progression
    public int FinalLevel { get; set; }
    public int TotalLegendEarned { get; set; }
    public int LegendToHallOfLegends { get; set; }
    public int ProgressionPointsSpent { get; set; }

    // Trauma
    public int FinalPsychicStress { get; set; }
    public int FinalCorruption { get; set; }
    public int TraumaCount { get; set; }

    // Combat
    public int TotalKills { get; set; }
    public int TotalDamageDealt { get; set; }
    public int TotalDamageTaken { get; set; }
    public int CombatsWon { get; set; }

    // Exploration
    public int RoomsExplored { get; set; }
    public int SecretsFound { get; set; }

    // Economy
    public int FinalCurrency { get; set; }

    // Time
    public int PlaytimeMinutes { get; set; }
    public DateTime DeathTimestamp { get; set; }

    /// <summary>
    /// Gets a formatted playtime string (e.g., "2h 15m").
    /// </summary>
    public string FormattedPlaytime
    {
        get
        {
            int hours = PlaytimeMinutes / 60;
            int minutes = PlaytimeMinutes % 60;
            return hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";
        }
    }
}

/// <summary>
/// Data structure for recording run endings in database.
/// </summary>
public class RunEnding
{
    public int RunEndingId { get; set; }
    public Guid GameSessionId { get; set; }
    public int CharacterId { get; set; }
    public string EndingType { get; set; } = "Death"; // "Death" or "Victory"
    public string? CauseOfDeath { get; set; } // If EndingType = Death
    public int FinalLevel { get; set; }
    public int TotalLegendEarned { get; set; }
    public int LegendToHallOfLegends { get; set; }
    public int FinalPsychicStress { get; set; }
    public int FinalCorruption { get; set; }
    public int TotalKills { get; set; }
    public int RoomsExplored { get; set; }
    public int PlaytimeMinutes { get; set; }
    public DateTime EndingTimestamp { get; set; }
}
