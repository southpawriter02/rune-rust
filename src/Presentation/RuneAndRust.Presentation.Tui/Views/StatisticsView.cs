// ═══════════════════════════════════════════════════════════════════════════════
// StatisticsView.cs
// TUI view for displaying comprehensive player statistics.
// Version: 0.12.0c
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Models;
using RuneAndRust.Application.Utilities;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Records;
using Spectre.Console;

namespace RuneAndRust.Presentation.Tui.Views;

/// <summary>
/// View for displaying comprehensive player statistics in the TUI.
/// </summary>
/// <remarks>
/// <para>StatisticsView provides a tabbed interface for viewing different categories of statistics:</para>
/// <list type="bullet">
///   <item><description>Combat: Kills, damage, abilities, combat rating</description></item>
///   <item><description>Exploration: Rooms, secrets, traps discovered</description></item>
///   <item><description>Progression: XP, levels, items, gold</description></item>
///   <item><description>Dice: Roll history, luck rating, streaks</description></item>
///   <item><description>Time: Playtime, sessions, dates</description></item>
/// </list>
/// <para>Uses Spectre.Console for rich terminal rendering with tables, colors, and formatting.</para>
/// </remarks>
public class StatisticsView
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Fields
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Service for retrieving player statistics.
    /// </summary>
    private readonly IStatisticsService _statisticsService;

    /// <summary>
    /// Service for retrieving dice roll history.
    /// </summary>
    private readonly IDiceHistoryService _diceHistoryService;

    /// <summary>
    /// Logger for view operations and diagnostics.
    /// </summary>
    private readonly ILogger<StatisticsView> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new statistics view instance.
    /// </summary>
    /// <param name="statisticsService">Service for retrieving player statistics.</param>
    /// <param name="diceHistoryService">Service for retrieving dice roll history.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public StatisticsView(
        IStatisticsService statisticsService,
        IDiceHistoryService diceHistoryService,
        ILogger<StatisticsView> logger)
    {
        _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
        _diceHistoryService = diceHistoryService ?? throw new ArgumentNullException(nameof(diceHistoryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("StatisticsView initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Public Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the statistics view for a player.
    /// </summary>
    /// <param name="player">The player whose statistics to display.</param>
    /// <param name="category">The category to display.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when player is null.</exception>
    public Task RenderAsync(Player player, StatisticCategory category, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        _logger.LogDebug("Rendering statistics for player {PlayerId}, category: {Category}",
            player.Id, category);

        // Get statistics data
        var stats = _statisticsService.GetPlayerStatistics(player);
        var metrics = _statisticsService.GetMetrics(player);
        var diceStats = _diceHistoryService.GetStatistics(player);

        _logger.LogDebug("Retrieved statistics - TotalAttacks: {Attacks}, D20Rolls: {D20Rolls}",
            stats.TotalAttacks, diceStats.TotalRolls);

        // Render header and tabs
        RenderHeader();
        RenderTabs(category);

        // Render category-specific content
        switch (category)
        {
            case StatisticCategory.Combat:
                RenderCombatStats(stats, metrics);
                break;

            case StatisticCategory.Exploration:
                RenderExplorationStats(stats);
                break;

            case StatisticCategory.Progression:
                RenderProgressionStats(stats);
                break;

            case StatisticCategory.Dice:
                var recentRolls = _diceHistoryService.GetRecentRolls(player, 10);
                _logger.LogDebug("Retrieved {RollCount} recent dice rolls for display", recentRolls.Count);
                RenderDiceStats(diceStats, recentRolls);
                break;

            case StatisticCategory.Time:
                RenderTimeStats(stats, metrics);
                break;

            default:
                _logger.LogWarning("Unknown statistics category: {Category}", category);
                AnsiConsole.MarkupLine("[yellow]Unknown category. Displaying combat statistics.[/]");
                RenderCombatStats(stats, metrics);
                break;
        }

        _logger.LogDebug("Statistics view render completed for category: {Category}", category);
        return Task.CompletedTask;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Header and Tabs Rendering
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders the statistics header.
    /// </summary>
    private void RenderHeader()
    {
        AnsiConsole.Write(new Rule("[yellow]STATISTICS[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("grey")
        });
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Renders the category tabs with the selected tab highlighted.
    /// </summary>
    /// <param name="selected">The currently selected category.</param>
    private void RenderTabs(StatisticCategory selected)
    {
        var categories = new[]
        {
            (StatisticCategory.Combat, "Combat"),
            (StatisticCategory.Exploration, "Exploration"),
            (StatisticCategory.Progression, "Progression"),
            (StatisticCategory.Dice, "Dice"),
            (StatisticCategory.Time, "Time")
        };

        var tabMarkup = string.Join(" ", categories.Select(c =>
            c.Item1 == selected
                ? $"[bold cyan][[{c.Item2}]][/]"
                : $"[grey]{c.Item2}[/]"));

        AnsiConsole.MarkupLine(tabMarkup);
        AnsiConsole.Write(new Rule { Style = Style.Parse("grey") });
        AnsiConsole.WriteLine();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Combat Statistics Rendering
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders combat statistics including kills, damage, and combat rating.
    /// </summary>
    /// <param name="stats">The player statistics.</param>
    /// <param name="metrics">The calculated metrics.</param>
    private void RenderCombatStats(PlayerStatistics stats, StatisticsMetrics metrics)
    {
        AnsiConsole.MarkupLine("[bold white]COMBAT STATISTICS[/]");
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey);

        table.AddColumn(new TableColumn("[cyan]Statistic[/]").Width(25));
        table.AddColumn(new TableColumn("[white]Value[/]").Width(15));
        table.AddColumn(new TableColumn("[grey]Details[/]").Width(30));

        // Monster kills
        table.AddRow(
            "[cyan]Monsters Killed[/]",
            StatisticsFormatter.FormatNumber(stats.MonstersKilled),
            StatisticsFormatter.FormatTopMonsters(stats.MonstersByType, 3));

        table.AddRow(
            "[cyan]Bosses Killed[/]",
            StatisticsFormatter.FormatNumber(stats.BossesKilled),
            "");

        // Damage dealt/received
        table.AddRow(
            "[cyan]Total Damage Dealt[/]",
            StatisticsFormatter.FormatNumber(stats.TotalDamageDealt),
            $"Avg: {metrics.AverageDamagePerHit:F1} per hit");

        table.AddRow(
            "[cyan]Total Damage Taken[/]",
            StatisticsFormatter.FormatNumber(stats.TotalDamageReceived),
            $"Avg: {metrics.AverageDamageReceived:F1} per hit");

        // Critical hits and misses
        table.AddRow(
            "[cyan]Critical Hits[/]",
            StatisticsFormatter.FormatNumber(stats.CriticalHits),
            $"Rate: {metrics.CriticalHitRateDisplay}");

        table.AddRow(
            "[cyan]Attacks Missed[/]",
            StatisticsFormatter.FormatNumber(stats.AttacksMissed),
            $"Miss Rate: {metrics.MissRateDisplay}");

        // Abilities and deaths
        table.AddRow(
            "[cyan]Abilities Used[/]",
            StatisticsFormatter.FormatNumber(stats.AbilitiesUsed),
            "");

        table.AddRow(
            "[cyan]Times Defeated[/]",
            StatisticsFormatter.FormatNumber(stats.DeathCount),
            "");

        AnsiConsole.Write(table);

        // Combat rating
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule { Style = Style.Parse("yellow") });
        AnsiConsole.MarkupLine($"[bold yellow]COMBAT RATING:[/] {StatisticsFormatter.FormatCombatRating(metrics.CombatRating)}");
        AnsiConsole.WriteLine();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Exploration Statistics Rendering
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders exploration statistics including rooms, secrets, and traps.
    /// </summary>
    /// <param name="stats">The player statistics.</param>
    private void RenderExplorationStats(PlayerStatistics stats)
    {
        AnsiConsole.MarkupLine("[bold white]EXPLORATION STATISTICS[/]");
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey);

        table.AddColumn(new TableColumn("[cyan]Statistic[/]").Width(25));
        table.AddColumn(new TableColumn("[white]Value[/]").Width(15));

        table.AddRow("[cyan]Rooms Discovered[/]", StatisticsFormatter.FormatNumber(stats.RoomsDiscovered));
        table.AddRow("[cyan]Secrets Found[/]", StatisticsFormatter.FormatNumber(stats.SecretsFound));
        table.AddRow("[cyan]Traps Triggered[/]", StatisticsFormatter.FormatNumber(stats.TrapsTriggered));
        table.AddRow("[cyan]Traps Avoided[/]", StatisticsFormatter.FormatNumber(stats.TrapsAvoided));
        table.AddRow("[cyan]Doors Opened[/]", StatisticsFormatter.FormatNumber(stats.DoorsOpened));
        table.AddRow("[cyan]Chests Opened[/]", StatisticsFormatter.FormatNumber(stats.ChestsOpened));

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Progression Statistics Rendering
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders progression statistics including XP, levels, items, and gold.
    /// </summary>
    /// <param name="stats">The player statistics.</param>
    private void RenderProgressionStats(PlayerStatistics stats)
    {
        AnsiConsole.MarkupLine("[bold white]PROGRESSION STATISTICS[/]");
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey);

        table.AddColumn(new TableColumn("[cyan]Statistic[/]").Width(25));
        table.AddColumn(new TableColumn("[white]Value[/]").Width(15));

        table.AddRow("[cyan]XP Earned[/]", StatisticsFormatter.FormatNumber(stats.TotalXPEarned));
        table.AddRow("[cyan]Levels Gained[/]", StatisticsFormatter.FormatNumber(stats.LevelsGained));
        table.AddRow("[cyan]Items Found[/]", StatisticsFormatter.FormatNumber(stats.ItemsFound));
        table.AddRow("[cyan]Items Crafted[/]", StatisticsFormatter.FormatNumber(stats.ItemsCrafted));
        table.AddRow("[cyan]Gold Earned[/]", StatisticsFormatter.FormatNumber(stats.GoldEarned));
        table.AddRow("[cyan]Gold Spent[/]", StatisticsFormatter.FormatNumber(stats.GoldSpent));
        table.AddRow("[cyan]Quests Completed[/]", StatisticsFormatter.FormatNumber(stats.QuestsCompleted));
        table.AddRow("[cyan]Puzzles Solved[/]", StatisticsFormatter.FormatNumber(stats.PuzzlesSolved));
        table.AddRow("[cyan]Resources Gathered[/]", StatisticsFormatter.FormatNumber(stats.ResourcesGathered));

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Dice Statistics Rendering
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders dice statistics including roll counts, luck rating, and streaks.
    /// </summary>
    /// <param name="diceStats">The dice statistics.</param>
    /// <param name="recentRolls">The recent dice rolls.</param>
    private void RenderDiceStats(DiceStatistics diceStats, IReadOnlyList<DiceRollRecord> recentRolls)
    {
        AnsiConsole.MarkupLine("[bold white]DICE STATISTICS[/]");
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey);

        table.AddColumn(new TableColumn("[cyan]Statistic[/]").Width(25));
        table.AddColumn(new TableColumn("[white]Value[/]").Width(15));
        table.AddColumn(new TableColumn("[grey]Details[/]").Width(30));

        // Roll counts
        table.AddRow(
            "[cyan]Total d20 Rolls[/]",
            StatisticsFormatter.FormatNumber(diceStats.TotalRolls),
            "");

        // Natural 20s
        table.AddRow(
            "[cyan]Natural 20s[/]",
            StatisticsFormatter.FormatNumber(diceStats.TotalNat20s),
            $"Rate: {diceStats.Nat20RateDisplay} (exp: {diceStats.ExpectedNat20Rate:P1})");

        // Natural 1s
        table.AddRow(
            "[cyan]Natural 1s[/]",
            StatisticsFormatter.FormatNumber(diceStats.TotalNat1s),
            $"Rate: {diceStats.Nat1RateDisplay} (exp: {diceStats.ExpectedNat1Rate:P1})");

        // Average roll
        table.AddRow(
            "[cyan]Average d20 Roll[/]",
            diceStats.AverageD20Display,
            $"(expected: {diceStats.ExpectedD20:F1})");

        AnsiConsole.Write(table);

        // Luck rating
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule { Style = Style.Parse("green") });
        AnsiConsole.MarkupLine($"[bold green]LUCK RATING:[/] {StatisticsFormatter.FormatLuckRating(diceStats.Rating, diceStats.LuckPercentage)}");

        // Streaks
        AnsiConsole.WriteLine();
        var streakTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Title("[yellow]Streaks[/]");

        streakTable.AddColumn(new TableColumn("[cyan]Type[/]"));
        streakTable.AddColumn(new TableColumn("[white]Value[/]"));

        streakTable.AddRow("[cyan]Current Streak[/]", StatisticsFormatter.FormatStreak(diceStats.CurrentStreak));
        streakTable.AddRow("[cyan]Longest Lucky Streak[/]", $"🔥 +{diceStats.LongestLuckyStreak}");
        streakTable.AddRow("[cyan]Longest Unlucky Streak[/]", $"❄️ -{diceStats.LongestUnluckyStreak}");

        AnsiConsole.Write(streakTable);

        // Recent rolls
        if (recentRolls.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[grey]Recent Rolls (d20):[/] {StatisticsFormatter.FormatRecentRolls(recentRolls)}");
        }

        AnsiConsole.WriteLine();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Time Statistics Rendering
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Renders time statistics including playtime, sessions, and dates.
    /// </summary>
    /// <param name="stats">The player statistics.</param>
    /// <param name="metrics">The calculated metrics.</param>
    private void RenderTimeStats(PlayerStatistics stats, StatisticsMetrics metrics)
    {
        AnsiConsole.MarkupLine("[bold white]TIME STATISTICS[/]");
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey);

        table.AddColumn(new TableColumn("[cyan]Statistic[/]").Width(25));
        table.AddColumn(new TableColumn("[white]Value[/]").Width(30));

        table.AddRow("[cyan]Total Playtime[/]", StatisticsFormatter.FormatDuration(stats.TotalPlaytime));
        table.AddRow("[cyan]Sessions[/]", StatisticsFormatter.FormatNumber(stats.SessionCount));
        table.AddRow("[cyan]Average Session[/]", metrics.AverageSessionLengthDisplay);
        table.AddRow("[cyan]First Played[/]", stats.FirstPlayed.HasValue
            ? StatisticsFormatter.FormatDate(stats.FirstPlayed.Value)
            : "—");
        table.AddRow("[cyan]Last Played[/]", stats.LastPlayed.HasValue
            ? StatisticsFormatter.FormatDate(stats.LastPlayed.Value)
            : "—");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }
}
