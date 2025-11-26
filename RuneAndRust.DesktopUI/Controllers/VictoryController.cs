using RuneAndRust.Core;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.DesktopUI.ViewModels;
using RuneAndRust.Engine;
using Serilog;

namespace RuneAndRust.DesktopUI.Controllers;

/// <summary>
/// v0.44.7: Victory & Endgame Transition Controller
/// Handles dungeon completion, displays victory screens with run summaries,
/// awards achievements, and transitions to endgame mode selection (NG+, Challenge Sectors, etc.).
///
/// Victory Conditions:
/// - Boss room cleared (final boss defeated)
/// - All sector objectives completed
/// </summary>
public class VictoryController
{
    private readonly ILogger _logger;
    private readonly GameStateController _gameStateController;
    private readonly INavigationService _navigationService;
    private readonly IMetaProgressionService _metaProgressionService;
    private readonly IEndgameService _endgameService;
    private readonly SagaService _sagaService;

    /// <summary>
    /// Event raised when victory handling is complete.
    /// </summary>
    public event EventHandler? VictoryHandlingComplete;

    /// <summary>
    /// Event raised when victory statistics are calculated.
    /// </summary>
    public event EventHandler<VictoryStatistics>? StatisticsCalculated;

    /// <summary>
    /// Event raised when transitioning to endgame mode selection.
    /// </summary>
    public event EventHandler? EndgameMenuRequested;

    public VictoryController(
        ILogger logger,
        GameStateController gameStateController,
        INavigationService navigationService,
        IMetaProgressionService metaProgressionService,
        IEndgameService endgameService,
        SagaService sagaService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameStateController = gameStateController ?? throw new ArgumentNullException(nameof(gameStateController));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _metaProgressionService = metaProgressionService ?? throw new ArgumentNullException(nameof(metaProgressionService));
        _endgameService = endgameService ?? throw new ArgumentNullException(nameof(endgameService));
        _sagaService = sagaService ?? throw new ArgumentNullException(nameof(sagaService));
    }

    /// <summary>
    /// Checks if victory conditions are met.
    /// Called after combat victory to determine if the dungeon is complete.
    /// </summary>
    public bool CheckVictoryCondition()
    {
        if (!_gameStateController.HasActiveGame)
            return false;

        var gameState = _gameStateController.CurrentGameState;
        return CheckDungeonCompletion(gameState);
    }

    /// <summary>
    /// Checks if the dungeon has been completed.
    /// Victory condition: Boss room cleared.
    /// </summary>
    private bool CheckDungeonCompletion(GameState gameState)
    {
        var dungeon = gameState.CurrentDungeon as Dungeon;
        if (dungeon == null) return false;

        // Check if boss room exists and has been cleared
        var bossRoom = dungeon.GetBossRoom();
        if (bossRoom == null)
        {
            _logger.Warning("No boss room found in dungeon {DungeonId}", dungeon.DungeonId);
            return false;
        }

        bool bossDefeated = bossRoom.HasBeenCleared && bossRoom.Enemies.Count == 0;

        if (bossDefeated)
        {
            _logger.Information("Victory condition met: Boss room {RoomId} cleared",
                bossRoom.RoomId);
        }

        return bossDefeated;
    }

    /// <summary>
    /// Handles victory - calculates statistics, updates meta-progression,
    /// and navigates to the victory screen.
    /// </summary>
    public async Task HandleVictoryAsync()
    {
        if (!_gameStateController.HasActiveGame) return;

        var gameState = _gameStateController.CurrentGameState;
        var player = gameState.Player;

        if (player == null)
        {
            _logger.Error("HandleVictoryAsync called without player");
            return;
        }

        _logger.Information("Victory! {Name} completed the Sector at Milestone {Level}",
            player.Name, player.CurrentMilestone);

        try
        {
            // Transition to Victory state
            await _gameStateController.UpdatePhaseAsync(GamePhase.Victory, "Sector completed - Victory!");

            // Calculate victory statistics
            var victoryStats = CalculateVictoryStatistics(gameState);

            _logger.Information(
                "Victory statistics: Level={Level}, Legend={Legend}, Kills={Kills}, Playtime={Time}",
                victoryStats.FinalLevel, victoryStats.TotalLegendEarned, victoryStats.TotalKills,
                victoryStats.FormattedPlaytime);

            // Update meta-progression
            await UpdateMetaProgressionAsync(player, victoryStats);

            // Check for achievements and unlocks
            await CheckForUnlocksAsync(victoryStats);

            // Raise statistics calculated event
            StatisticsCalculated?.Invoke(this, victoryStats);

            // Navigate to victory screen
            _navigationService.NavigateTo<VictoryScreenViewModel>();

            _logger.Information("Victory handling complete. Awaiting player acknowledgment.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error handling victory for {Name}", player.Name);
            // Still transition to endgame menu on error
            await TransitionToEndgameMenuAsync();
        }
    }

    /// <summary>
    /// Calculates comprehensive victory statistics.
    /// </summary>
    private VictoryStatistics CalculateVictoryStatistics(GameState gameState)
    {
        var player = gameState.Player!;
        var dungeon = gameState.CurrentDungeon as Dungeon;

        var stats = new VictoryStatistics
        {
            // Basic Info
            SurvivorName = player.Name,
            CharacterClass = player.Class.ToString(),
            Specialization = player.Specialization.ToString(),

            // Progression
            FinalLevel = player.CurrentMilestone,
            TotalLegendEarned = player.CurrentLegend,
            LegendToHallOfLegends = CalculateLegendForHallOfLegends(player, wasVictory: true),
            ProgressionPointsSpent = CalculateProgressionPointsSpent(player),

            // Trauma (lower is better for victory)
            FinalPsychicStress = player.PsychicStress,
            FinalCorruption = player.Corruption,
            TraumaCount = player.Traumas.Count,

            // Combat
            TotalKills = EstimateKills(player),

            // Exploration
            RoomsExplored = dungeon?.Rooms.Count(r => r.Value.HasBeenCleared) ?? 0,
            TotalRooms = dungeon?.TotalRoomCount ?? 0,
            SecretsFound = dungeon?.GetSecretRooms().Count(r => r.HasBeenCleared) ?? 0,

            // Economy
            FinalCurrency = player.Currency,

            // Time
            PlaytimeMinutes = (int)gameState.PlayTime.TotalMinutes,
            VictoryTimestamp = DateTime.UtcNow,

            // Mode info
            NGPlusTier = gameState.CurrentNGPlusTier ?? 0,
            ChallengeSectorName = gameState.CurrentChallengeSector?.Name,
            WasVictory = true
        };

        return stats;
    }

    /// <summary>
    /// Calculates how much Legend converts to Hall of Legends.
    /// Victory bonus: 20% of total Legend earned (vs 10% for death).
    /// </summary>
    private int CalculateLegendForHallOfLegends(PlayerCharacter player, bool wasVictory)
    {
        int totalLegend = player.CurrentLegend;
        // Victory grants 20% conversion (double the death rate)
        int conversionRate = wasVictory ? 5 : 10; // 100/5 = 20%, 100/10 = 10%
        int hallOfLegendsAmount = totalLegend / conversionRate;

        _logger.Debug("Converting {Total} Legend to {HallAmount} Hall of Legends ({Rate}% rate)",
            totalLegend, hallOfLegendsAmount, 100 / conversionRate);

        return hallOfLegendsAmount;
    }

    /// <summary>
    /// Estimates progression points spent.
    /// </summary>
    private int CalculateProgressionPointsSpent(PlayerCharacter player)
    {
        int totalPPEarned = 2 + player.CurrentMilestone;
        int ppSpent = totalPPEarned - player.ProgressionPoints;
        return Math.Max(0, ppSpent);
    }

    /// <summary>
    /// Estimates total kills based on milestone level.
    /// </summary>
    private int EstimateKills(PlayerCharacter player)
    {
        // Victory typically means more kills than death
        return player.CurrentMilestone * 6 + (player.CurrentLegend / 40);
    }

    /// <summary>
    /// Updates meta-progression system with victory recorded.
    /// </summary>
    private async Task UpdateMetaProgressionAsync(PlayerCharacter player, VictoryStatistics stats)
    {
        try
        {
            // Add Legend to Hall of Legends (victory bonus: 20% vs 10% for death)
            await _metaProgressionService.AddLegendToHallOfLegendsAsync(stats.LegendToHallOfLegends);

            // Record victory for statistics
            await _metaProgressionService.RecordVictoryAsync(
                player.Class.ToString(),
                player.Specialization.ToString(),
                player.CurrentMilestone,
                stats.NGPlusTier,
                stats.ChallengeSectorName);

            _logger.Information("Meta-progression updated: {Legend} added to Hall of Legends (Victory bonus)",
                stats.LegendToHallOfLegends);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error updating meta-progression for victory");
            // Don't throw - allow victory handling to continue
        }
    }

    /// <summary>
    /// Checks for and processes meta-progression unlocks.
    /// </summary>
    private async Task CheckForUnlocksAsync(VictoryStatistics stats)
    {
        try
        {
            // Check for unlocks based on victory
            await _metaProgressionService.CheckForUnlocksAsync();

            // First victory might unlock NG+ or other content
            if (stats.NGPlusTier == 0)
            {
                _logger.Information("First completion - NG+ should now be available");
            }

            _logger.Debug("Checked for meta-progression unlocks after victory");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error checking for unlocks after victory");
        }
    }

    /// <summary>
    /// Called when the player acknowledges the victory screen.
    /// Transitions to endgame mode selection.
    /// </summary>
    public async Task OnVictoryAcknowledgedAsync()
    {
        _logger.Information("Victory acknowledged. Transitioning to endgame mode selection.");

        await TransitionToEndgameMenuAsync();
    }

    /// <summary>
    /// Transitions to the endgame mode selection menu.
    /// </summary>
    public async Task TransitionToEndgameMenuAsync()
    {
        _logger.Information("Transitioning to endgame menu");

        await _gameStateController.UpdatePhaseAsync(GamePhase.EndgameMenu, "Victory acknowledged");

        // Navigate to endgame mode selection
        _navigationService.NavigateTo<EndgameModeViewModel>();

        EndgameMenuRequested?.Invoke(this, EventArgs.Empty);
        VictoryHandlingComplete?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when player selects an endgame mode.
    /// </summary>
    public async Task OnSelectEndgameModeAsync(EndgameModeConfig config)
    {
        _logger.Information("Player selected endgame mode: {Mode} (NG+ Tier: {Tier})",
            config.Mode, config.NGPlusTier);

        bool success = await _endgameService.StartEndgameModeAsync(config);

        if (success)
        {
            _logger.Information("Endgame mode {Mode} started successfully", config.Mode);
        }
        else
        {
            _logger.Warning("Failed to start endgame mode {Mode}", config.Mode);
        }
    }

    /// <summary>
    /// Called when player chooses to return to main menu from victory/endgame.
    /// </summary>
    public async Task OnReturnToMainMenuAsync()
    {
        _logger.Information("Returning to main menu from victory");

        // Reset game state
        _gameStateController.Reset();

        // Navigate to main menu
        _navigationService.NavigateTo<MenuViewModel>();
    }

    /// <summary>
    /// Gets the current victory statistics for display (if in victory phase).
    /// </summary>
    public VictoryStatistics? GetCurrentVictoryStatistics()
    {
        if (!_gameStateController.HasActiveGame)
            return null;

        var gameState = _gameStateController.CurrentGameState;
        if (gameState.CurrentPhase != GamePhase.Victory)
            return null;

        return CalculateVictoryStatistics(gameState);
    }

    /// <summary>
    /// Gets available endgame content based on current unlocks.
    /// </summary>
    public EndgameContentAvailability GetAvailableEndgameContent()
    {
        return _endgameService.GetAvailableContent();
    }
}

/// <summary>
/// Data structure for victory statistics displayed on victory screen.
/// </summary>
public class VictoryStatistics
{
    // Basic Info
    public string SurvivorName { get; set; } = string.Empty;
    public string CharacterClass { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;

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

    // Exploration
    public int RoomsExplored { get; set; }
    public int TotalRooms { get; set; }
    public int SecretsFound { get; set; }

    // Economy
    public int FinalCurrency { get; set; }

    // Time
    public int PlaytimeMinutes { get; set; }
    public DateTime VictoryTimestamp { get; set; }

    // Mode Info
    public int NGPlusTier { get; set; }
    public string? ChallengeSectorName { get; set; }
    public bool WasVictory { get; set; }

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

    /// <summary>
    /// Gets the exploration percentage.
    /// </summary>
    public int ExplorationPercentage => TotalRooms > 0 ? (RoomsExplored * 100) / TotalRooms : 0;

    /// <summary>
    /// Gets a rating based on performance (S/A/B/C/D).
    /// </summary>
    public string PerformanceRating
    {
        get
        {
            int score = 0;

            // Level factor (max 25 points)
            score += Math.Min(25, FinalLevel * 5);

            // Exploration factor (max 25 points)
            score += ExplorationPercentage / 4;

            // Trauma factor (max 25 points - lower is better)
            int traumaScore = 25 - (FinalPsychicStress / 4) - (FinalCorruption / 4);
            score += Math.Max(0, traumaScore);

            // Time factor (max 25 points - faster is better for short runs)
            int timeScore = PlaytimeMinutes < 30 ? 25 :
                           PlaytimeMinutes < 60 ? 20 :
                           PlaytimeMinutes < 120 ? 15 :
                           PlaytimeMinutes < 180 ? 10 : 5;
            score += timeScore;

            // NG+ bonus
            score += NGPlusTier * 10;

            return score >= 90 ? "S" :
                   score >= 75 ? "A" :
                   score >= 60 ? "B" :
                   score >= 40 ? "C" : "D";
        }
    }
}
