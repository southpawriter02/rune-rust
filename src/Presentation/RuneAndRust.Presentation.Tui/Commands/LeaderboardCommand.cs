// ═══════════════════════════════════════════════════════════════════════════════
// LeaderboardCommand.cs
// Command handler for displaying the leaderboard view.
// Version: 0.13.4c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Tui.Configuration;
using RuneAndRust.Presentation.Tui.DTOs;
using RuneAndRust.Presentation.Tui.Renderers;
using RuneAndRust.Presentation.Tui.UI;

namespace RuneAndRust.Presentation.Tui.Commands;

/// <summary>
/// Command handler for displaying the leaderboard view.
/// </summary>
/// <remarks>
/// <para>Command aliases: "leaderboard", "lb", "scores", "highscores"</para>
/// <para>Optional arguments:</para>
/// <list type="bullet">
///   <item><description>speed, speedrun - Show Speedrun category</description></item>
///   <item><description>nodeath, no-death - Show No Death category</description></item>
///   <item><description>ach, achievements, points - Show Achievement Points category</description></item>
///   <item><description>boss, bosses - Show Boss Slayer category</description></item>
/// </list>
/// </remarks>
public class LeaderboardCommand
{
    private readonly ILeaderboardService _leaderboardService;
    private readonly ITerminalService _terminalService;
    private readonly ILogger<LeaderboardCommand>? _logger;

    // UI components - created on demand
    private LeaderboardView? _view;
    private ScoreBreakdownPanel? _breakdownPanel;
    private LeaderboardViewConfig _config;

    /// <summary>
    /// Gets the command name.
    /// </summary>
    public string Name => "leaderboard";

    /// <summary>
    /// Gets the command aliases.
    /// </summary>
    public IReadOnlyList<string> Aliases => new[] { "lb", "scores", "highscores" };

    /// <summary>
    /// Gets the command description.
    /// </summary>
    public string Description => "View leaderboards";

    /// <summary>
    /// Creates a new instance of the LeaderboardCommand.
    /// </summary>
    /// <param name="leaderboardService">The leaderboard service.</param>
    /// <param name="terminalService">The terminal service.</param>
    /// <param name="config">Optional configuration for the view.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public LeaderboardCommand(
        ILeaderboardService leaderboardService,
        ITerminalService terminalService,
        LeaderboardViewConfig? config = null,
        ILogger<LeaderboardCommand>? logger = null)
    {
        _leaderboardService = leaderboardService ?? throw new ArgumentNullException(nameof(leaderboardService));
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
        _config = config ?? new LeaderboardViewConfig();
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMMAND EXECUTION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Executes the leaderboard command.
    /// </summary>
    /// <param name="player">The current player.</param>
    /// <param name="args">The command arguments.</param>
    public void Execute(Player player, string[] args)
    {
        _logger?.LogInformation("Executing leaderboard command");

        EnsureViewsInitialized();

        // Parse category from args if provided
        var category = ParseCategory(args);
        _view!.SelectCategory(category);

        // Get leaderboard entries
        var entries = _leaderboardService.GetLeaderboard(category, _config.MaxDisplayEntries);

        // Get player's personal best
        var playerBest = _leaderboardService.GetPlayerBest(player, category);

        // Get player's rank
        var playerRank = _leaderboardService.GetPlayerRank(player, category);

        // Map to display DTOs (using player name for comparison)
        var displayEntries = MapToDisplayDtos(entries, player.Name);

        // Set player info
        _view.SetCurrentPlayer(player.Name);
        if (playerBest is not null)
        {
            var playerBestDto = MapToDisplayDto(playerBest, true, playerRank);
            _view.SetPersonalBest(playerBestDto);
        }

        // Set view position
        _view.SetPosition(0, 0);

        // Render leaderboard
        _view.RenderLeaderboard(displayEntries);

        // Show player rank
        if (playerRank > 0)
        {
            var totalEntries = _leaderboardService.GetLeaderboard(category, 100).Count;
            _view.ShowPlayerRank(playerRank, totalEntries);
        }

        // For High Score category, show score breakdown
        if (playerBest is not null && category == LeaderboardCategory.HighScore)
        {
            var breakdownDtos = BuildScoreBreakdown(player);
            _breakdownPanel!.RenderBreakdown(breakdownDtos, playerBest.Score, 3, 26);
        }

        _logger?.LogInformation("Leaderboard displayed successfully for {Category}", category);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    private void EnsureViewsInitialized()
    {
        if (_view is null)
        {
            var entryRenderer = new LeaderboardEntryRenderer();
            var personalBestHighlight = new PersonalBestHighlight(_terminalService, _config);
            _view = new LeaderboardView(
                entryRenderer,
                personalBestHighlight,
                _terminalService,
                _config);
            _breakdownPanel = new ScoreBreakdownPanel(_terminalService, _config);
        }
    }

    private LeaderboardCategory ParseCategory(string[] args)
    {
        if (args.Length == 0)
        {
            return LeaderboardCategory.HighScore;
        }

        return args[0].ToLowerInvariant() switch
        {
            "speed" or "speedrun" or "s" => LeaderboardCategory.Speedrun,
            "nodeath" or "no-death" or "nd" => LeaderboardCategory.NoDeath,
            "ach" or "achievements" or "points" or "a" => LeaderboardCategory.AchievementPoints,
            "boss" or "bosses" or "b" => LeaderboardCategory.BossSlayer,
            _ => LeaderboardCategory.HighScore
        };
    }

    private IReadOnlyList<LeaderboardDisplayDto> MapToDisplayDtos(
        IReadOnlyList<LeaderboardEntry> entries,
        string currentPlayerName)
    {
        var result = new List<LeaderboardDisplayDto>();
        var rank = 1;

        foreach (var entry in entries)
        {
            // Compare by player name since LeaderboardEntry doesn't have PlayerId
            var isCurrentPlayer = entry.PlayerName == currentPlayerName;
            result.Add(MapToDisplayDto(entry, isCurrentPlayer, rank));
            rank++;
        }

        return result;
    }

    private LeaderboardDisplayDto MapToDisplayDto(
        LeaderboardEntry entry,
        bool isCurrentPlayer,
        int rank)
    {
        // Derive category-specific values from Score based on category type
        // (LeaderboardEntry stores all values in Score field)
        return new LeaderboardDisplayDto
        {
            Rank = rank,
            PlayerName = entry.PlayerName,
            CharacterClass = entry.ClassName,  // LeaderboardEntry uses ClassName
            Level = entry.Level,
            Score = entry.Score,
            Date = entry.AchievedAt,
            IsCurrentPlayer = isCurrentPlayer,
            Category = entry.Category,
            TimeElapsed = entry.CompletionTime,
            // FloorsCleared and BossesDefeated are derived from Score for their categories
            FloorsCleared = entry.Category == LeaderboardCategory.NoDeath ? (int)entry.Score : null,
            BossesDefeated = entry.Category == LeaderboardCategory.BossSlayer ? (int)entry.Score : null
        };
    }

    /// <summary>
    /// Builds the score breakdown from player statistics.
    /// </summary>
    /// <remarks>
    /// Since the service doesn't expose a GetScoreBreakdown method,
    /// we construct it from the player's current statistics.
    /// Uses PlayerStatistics.GetStatistic() for stat lookup.
    /// </remarks>
    private IReadOnlyList<ScoreComponentDto> BuildScoreBreakdown(Player player)
    {
        var components = new List<ScoreComponentDto>();

        // Guard: ensure player has statistics
        if (player.Statistics is null)
        {
            return components;
        }

        // Base components from player stats (using GetStatistic method)
        var monstersKilled = player.Statistics.GetStatistic("monstersKilled");
        if (monstersKilled > 0)
        {
            components.Add(new ScoreComponentDto
            {
                Name = "Monsters Killed",
                Count = (int)monstersKilled,
                PointsEach = 10,
                TotalPoints = (int)monstersKilled * 10
            });
        }

        var bossesKilled = player.Statistics.GetStatistic("bossesKilled");
        if (bossesKilled > 0)
        {
            components.Add(new ScoreComponentDto
            {
                Name = "Bosses Killed",
                Count = (int)bossesKilled,
                PointsEach = 500,
                TotalPoints = (int)bossesKilled * 500
            });
        }

        var roomsDiscovered = player.Statistics.GetStatistic("roomsDiscovered");
        if (roomsDiscovered > 0)
        {
            components.Add(new ScoreComponentDto
            {
                Name = "Rooms Discovered",
                Count = (int)roomsDiscovered,
                PointsEach = 25,
                TotalPoints = (int)roomsDiscovered * 25
            });
        }

        var goldEarned = player.Statistics.GetStatistic("goldEarned");
        if (goldEarned > 0)
        {
            components.Add(new ScoreComponentDto
            {
                Name = "Gold Earned",
                TotalPoints = (int)goldEarned
            });
        }

        // Level multiplier: 1.0 + (level * 0.1)
        var levelMultiplier = 1.0 + (player.Level * 0.1);
        components.Add(new ScoreComponentDto
        {
            Name = "Level Multiplier",
            TotalPoints = (int)(levelMultiplier * 100),
            IsMultiplier = true
        });

        // Achievement bonus
        var achievementPoints = player.Statistics.GetStatistic("achievementPoints");
        if (achievementPoints > 0)
        {
            components.Add(new ScoreComponentDto
            {
                Name = "Achievement Bonus",
                TotalPoints = (int)achievementPoints * 10,
                IsMultiplier = true
            });
        }

        // Death penalty
        var deaths = player.Statistics.GetStatistic("deaths");
        if (deaths > 0)
        {
            components.Add(new ScoreComponentDto
            {
                Name = "Death Penalty",
                TotalPoints = (int)deaths * -100,
                IsPenalty = true
            });
        }

        return components;
    }
}

