using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Constants;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.Records;

namespace RuneAndRust.Application.Services;

/// <summary>
/// In-memory implementation of <see cref="IDiceRollLogger"/>.
/// </summary>
/// <remarks>
/// <para>
/// Stores roll history in a thread-safe queue with a configurable maximum size.
/// When the limit is reached, oldest rolls are automatically discarded.
/// </para>
/// <para>
/// Suitable for:
/// <list type="bullet">
///   <item><description>Single-session game play</description></item>
///   <item><description>Development and testing</description></item>
///   <item><description>Scenarios where persistence is not required</description></item>
/// </list>
/// </para>
/// <para>
/// For persistent storage, implement a database-backed version of
/// <see cref="IDiceRollLogger"/> in a future version.
/// </para>
/// </remarks>
public class InMemoryDiceRollLogger : IDiceRollLogger
{
    private readonly ConcurrentQueue<DiceRollLog> _rollHistory = new();
    private readonly ILogger<InMemoryDiceRollLogger> _logger;
    private readonly object _trimLock = new();

    /// <summary>
    /// Default maximum history size.
    /// </summary>
    public const int DefaultMaxHistorySize = 1000;

    /// <summary>
    /// Gets the maximum number of rolls stored before oldest are discarded.
    /// </summary>
    public int MaxHistorySize { get; }

    /// <summary>
    /// Gets the current number of rolls in history.
    /// </summary>
    public int HistoryCount => _rollHistory.Count;

    /// <summary>
    /// Creates a new in-memory dice roll logger.
    /// </summary>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="maxHistorySize">Maximum rolls to retain (default 1000).</param>
    public InMemoryDiceRollLogger(
        ILogger<InMemoryDiceRollLogger> logger,
        int maxHistorySize = DefaultMaxHistorySize)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        MaxHistorySize = maxHistorySize > 0 ? maxHistorySize : DefaultMaxHistorySize;

        _logger.LogInformation(
            "InMemoryDiceRollLogger initialized with max history size: {MaxSize}",
            MaxHistorySize);
    }

    /// <summary>
    /// Logs a dice roll with full metadata.
    /// </summary>
    /// <param name="log">The roll log entry to store.</param>
    public void LogRoll(DiceRollLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        _rollHistory.Enqueue(log);

        _logger.LogDebug(
            "Logged roll: {Context} → {Net} net ({Successes}S-{Botches}B){Special}",
            log.Context,
            log.NetSuccesses,
            log.TotalSuccesses,
            log.TotalBotches,
            log.IsFumble ? " [FUMBLE]" : log.IsCriticalSuccess ? " [CRITICAL]" : "");

        // Trim if over capacity
        TrimIfNeeded();
    }

    /// <summary>
    /// Retrieves the most recent rolls from history.
    /// </summary>
    /// <param name="count">Maximum number of rolls to return (default 100).</param>
    /// <returns>A read-only list of roll logs, ordered oldest to newest.</returns>
    public IReadOnlyList<DiceRollLog> GetRollHistory(int count = 100)
    {
        if (count <= 0)
            return Array.Empty<DiceRollLog>();

        var allRolls = _rollHistory.ToArray();
        var startIndex = Math.Max(0, allRolls.Length - count);
        var result = allRolls.Skip(startIndex).ToList();

        _logger.LogDebug("Retrieved {Count} rolls from history", result.Count);

        return result.AsReadOnly();
    }

    /// <summary>
    /// Retrieves rolls matching a specific context prefix.
    /// </summary>
    /// <param name="contextPrefix">The context prefix to filter by.</param>
    /// <returns>A read-only list of matching roll logs, ordered oldest to newest.</returns>
    public IReadOnlyList<DiceRollLog> GetRollsByContext(string contextPrefix)
    {
        var result = _rollHistory
            .Where(log => RollContexts.MatchesPrefix(log.Context, contextPrefix))
            .ToList();

        _logger.LogDebug(
            "Retrieved {Count} rolls matching context prefix '{Prefix}'",
            result.Count, contextPrefix);

        return result.AsReadOnly();
    }

    /// <summary>
    /// Clears all stored roll history.
    /// </summary>
    public void ClearHistory()
    {
        var count = _rollHistory.Count;

        // Drain the queue
        while (_rollHistory.TryDequeue(out _)) { }

        _logger.LogInformation("Cleared {Count} rolls from history", count);
    }

    /// <summary>
    /// Trims the history if it exceeds the maximum size.
    /// </summary>
    private void TrimIfNeeded()
    {
        if (_rollHistory.Count <= MaxHistorySize)
            return;

        lock (_trimLock)
        {
            var trimCount = 0;
            while (_rollHistory.Count > MaxHistorySize && _rollHistory.TryDequeue(out _))
            {
                trimCount++;
            }

            if (trimCount > 0)
            {
                _logger.LogDebug("Trimmed {Count} oldest rolls from history", trimCount);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // BONUS QUERY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all rolls that were fumbles.
    /// </summary>
    /// <returns>All fumble rolls in history.</returns>
    public IReadOnlyList<DiceRollLog> GetFumbles()
    {
        return _rollHistory
            .Where(log => log.IsFumble)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets all rolls that were critical successes.
    /// </summary>
    /// <returns>All critical success rolls in history.</returns>
    public IReadOnlyList<DiceRollLog> GetCriticalSuccesses()
    {
        return _rollHistory
            .Where(log => log.IsCriticalSuccess)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets rolls for a specific actor.
    /// </summary>
    /// <param name="actorId">The actor's ID.</param>
    /// <returns>All rolls by the specified actor.</returns>
    public IReadOnlyList<DiceRollLog> GetRollsByActor(Guid actorId)
    {
        return _rollHistory
            .Where(log => log.ActorId == actorId)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets rolls within a time range.
    /// </summary>
    /// <param name="start">Start time (inclusive).</param>
    /// <param name="end">End time (inclusive).</param>
    /// <returns>Rolls within the time range.</returns>
    public IReadOnlyList<DiceRollLog> GetRollsByTimeRange(DateTime start, DateTime end)
    {
        return _rollHistory
            .Where(log => log.Timestamp >= start && log.Timestamp <= end)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets aggregate statistics for the roll history.
    /// </summary>
    /// <returns>A summary of roll statistics.</returns>
    public RollStatistics GetStatistics()
    {
        var rolls = _rollHistory.ToArray();

        if (rolls.Length == 0)
        {
            return new RollStatistics(
                TotalRolls: 0,
                TotalFumbles: 0,
                TotalCriticals: 0,
                AverageNetSuccesses: 0,
                FumbleRate: 0,
                CriticalRate: 0);
        }

        var totalFumbles = rolls.Count(r => r.IsFumble);
        var totalCriticals = rolls.Count(r => r.IsCriticalSuccess);
        var averageNet = rolls.Average(r => r.NetSuccesses);

        return new RollStatistics(
            TotalRolls: rolls.Length,
            TotalFumbles: totalFumbles,
            TotalCriticals: totalCriticals,
            AverageNetSuccesses: averageNet,
            FumbleRate: (double)totalFumbles / rolls.Length,
            CriticalRate: (double)totalCriticals / rolls.Length);
    }
}

/// <summary>
/// Aggregate statistics for roll history.
/// </summary>
/// <param name="TotalRolls">Total number of rolls.</param>
/// <param name="TotalFumbles">Number of fumble rolls.</param>
/// <param name="TotalCriticals">Number of critical success rolls.</param>
/// <param name="AverageNetSuccesses">Average net successes across all rolls.</param>
/// <param name="FumbleRate">Fumble rate as a decimal (0-1).</param>
/// <param name="CriticalRate">Critical success rate as a decimal (0-1).</param>
public record RollStatistics(
    int TotalRolls,
    int TotalFumbles,
    int TotalCriticals,
    double AverageNetSuccesses,
    double FumbleRate,
    double CriticalRate);
