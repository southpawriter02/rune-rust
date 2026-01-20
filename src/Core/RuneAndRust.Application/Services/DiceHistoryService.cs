// ═══════════════════════════════════════════════════════════════════════════════
// DiceHistoryService.cs
// Service implementation for tracking and querying dice roll history.
// Version: 0.12.0b
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Models;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Records;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for tracking and querying dice roll history.
/// </summary>
/// <remarks>
/// <para>This service:</para>
/// <list type="bullet">
///   <item><description>Records rolls to the player's DiceRollHistory entity</description></item>
///   <item><description>Calculates luck ratings from roll averages</description></item>
///   <item><description>Provides streak and statistics information</description></item>
///   <item><description>Maintains recent rolls buffer for display</description></item>
/// </list>
/// <para>
/// The service ensures each player has a DiceRollHistory entity initialized
/// before recording or querying data. This is done lazily on first access.
/// </para>
/// </remarks>
public class DiceHistoryService : IDiceHistoryService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Fields
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger instance for diagnostic output.
    /// </summary>
    private readonly ILogger<DiceHistoryService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="DiceHistoryService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is null.
    /// </exception>
    public DiceHistoryService(ILogger<DiceHistoryService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogDebug("DiceHistoryService initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Recording Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public void RecordRoll(Player player, DiceRollRecord roll)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentNullException.ThrowIfNull(roll, nameof(roll));

        var history = GetOrCreateHistory(player);

        _logger.LogDebug(
            "Recording roll: {Expression} = {Result} for player {PlayerId} (context: {Context})",
            roll.DiceExpression,
            roll.Result,
            player.Id,
            roll.Context);

        history.RecordRoll(roll);

        // Log significant events at Debug level for visibility
        if (roll.HasNatural20)
        {
            _logger.LogDebug(
                "Natural 20 rolled by player {PlayerId}! Total nat20s: {Count}",
                player.Id,
                history.TotalNaturalTwenties);
        }
        else if (roll.HasNatural1)
        {
            _logger.LogDebug(
                "Natural 1 rolled by player {PlayerId}. Total nat1s: {Count}",
                player.Id,
                history.TotalNaturalOnes);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Query Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public DiceRollHistory GetHistory(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        return GetOrCreateHistory(player);
    }

    /// <inheritdoc />
    public LuckRating GetLuckRating(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        var history = GetOrCreateHistory(player);
        var luckPercentage = history.GetLuckPercentage();

        var rating = CalculateLuckRating(luckPercentage);

        _logger.LogDebug(
            "Calculated luck rating for player {PlayerId}: {Rating} (luck: {LuckPercentage:F2}%)",
            player.Id,
            rating,
            luckPercentage);

        return rating;
    }

    /// <inheritdoc />
    public int GetCurrentStreak(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        var history = GetOrCreateHistory(player);
        return history.CurrentStreak;
    }

    /// <inheritdoc />
    public IReadOnlyList<DiceRollRecord> GetRecentRolls(Player player, int count = 10)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count, nameof(count));

        var history = GetOrCreateHistory(player);

        // Return the most recent 'count' rolls (taking from the end of the buffer)
        return history.RecentRolls
            .TakeLast(count)
            .ToList();
    }

    /// <inheritdoc />
    public DiceStatistics GetStatistics(Player player)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        var history = GetOrCreateHistory(player);
        var luckPercentage = history.GetLuckPercentage();

        _logger.LogDebug(
            "Getting statistics for player {PlayerId}: {D20Count} d20 rolls, avg {Average:F2}",
            player.Id,
            history.D20RollCount,
            history.GetAverageD20Roll());

        return new DiceStatistics(
            TotalRolls: history.D20RollCount,
            TotalNat20s: history.TotalNaturalTwenties,
            TotalNat1s: history.TotalNaturalOnes,
            AverageD20: history.GetAverageD20Roll(),
            ExpectedD20: DiceRollHistory.D20ExpectedAverage,
            Nat20Rate: history.GetNat20Rate(),
            ExpectedNat20Rate: 0.05,
            Nat1Rate: history.GetNat1Rate(),
            ExpectedNat1Rate: 0.05,
            LuckPercentage: luckPercentage,
            Rating: CalculateLuckRating(luckPercentage),
            CurrentStreak: history.CurrentStreak,
            LongestLuckyStreak: history.LongestLuckyStreak,
            LongestUnluckyStreak: history.LongestUnluckyStreak);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Helper Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or creates the dice roll history for a player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The player's dice roll history entity.</returns>
    /// <remarks>
    /// <para>
    /// This method ensures the player has a DiceRollHistory entity initialized.
    /// If one doesn't exist, it creates a new one and associates it with the player
    /// via the InitializeDiceHistory method.
    /// </para>
    /// </remarks>
    private static DiceRollHistory GetOrCreateHistory(Player player)
    {
        if (player.DiceHistory is null)
        {
            var history = DiceRollHistory.Create(player.Id);
            player.InitializeDiceHistory(history);
        }

        return player.DiceHistory!;
    }

    /// <summary>
    /// Calculates the luck rating from a luck percentage.
    /// </summary>
    /// <param name="luckPercentage">The luck percentage (deviation from expected average).</param>
    /// <returns>The corresponding luck rating tier.</returns>
    /// <remarks>
    /// <para>
    /// Thresholds:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Cursed: &lt; -10%</description></item>
    ///   <item><description>Unlucky: -10% to -5%</description></item>
    ///   <item><description>Average: -5% to +5%</description></item>
    ///   <item><description>Lucky: +5% to +10%</description></item>
    ///   <item><description>Blessed: &gt; +10%</description></item>
    /// </list>
    /// </remarks>
    internal static LuckRating CalculateLuckRating(double luckPercentage)
    {
        return luckPercentage switch
        {
            < -10.0 => LuckRating.Cursed,
            < -5.0 => LuckRating.Unlucky,
            <= 5.0 => LuckRating.Average,
            <= 10.0 => LuckRating.Lucky,
            _ => LuckRating.Blessed
        };
    }
}
