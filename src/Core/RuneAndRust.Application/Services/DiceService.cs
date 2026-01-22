using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Constants;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Records;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Core dice rolling service using success-counting mechanics.
/// </summary>
/// <remarks>
/// <para>
/// Supports standard dice pools with success-counting, exploding dice,
/// and advantage/disadvantage rolls.
/// </para>
/// <para>
/// Success-counting mechanics:
/// <list type="bullet">
///   <item><description>Dice showing 8-10 count as successes</description></item>
///   <item><description>Dice showing 1 count as botches</description></item>
///   <item><description>Net successes = successes - botches (min 0)</description></item>
///   <item><description>Fumble = 0 successes AND ≥1 botch</description></item>
///   <item><description>Critical = net ≥ 5</description></item>
/// </list>
/// </para>
/// <para>
/// <b>v0.12.0b Update:</b> Added optional dice history tracking integration.
/// When an IDiceHistoryService is provided and a Player is passed to Roll methods,
/// rolls are automatically recorded to the player's dice history for statistics tracking.
/// </para>
/// <para>
/// <b>v0.15.0a Update:</b> Refactored to success-counting mechanics.
/// Advantage/disadvantage now compare NetSuccesses instead of raw totals.
/// </para>
/// </remarks>
public class DiceService : IDiceService
{
    private readonly Random _random;
    private readonly ILogger<DiceService> _logger;
    private readonly IGameEventLogger? _eventLogger;
    private readonly IDiceHistoryService? _historyService;

    /// <summary>
    /// Creates a new DiceService.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <param name="random">Optional Random instance for deterministic testing.</param>
    /// <param name="eventLogger">Optional game event logger for roll event tracking.</param>
    /// <param name="historyService">
    /// Optional dice history service for recording rolls to player statistics (v0.12.0b).
    /// </param>
    public DiceService(
        ILogger<DiceService> logger,
        Random? random = null,
        IGameEventLogger? eventLogger = null,
        IDiceHistoryService? historyService = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = random ?? new Random();
        _eventLogger = eventLogger;
        _historyService = historyService;
        _logger.LogInformation(
            "DiceService initialized with success-counting mechanics (history tracking: {HistoryEnabled})",
            historyService is not null);
    }

    /// <summary>
    /// Rolls a dice pool and returns the result with success counts.
    /// </summary>
    /// <param name="pool">The dice pool to roll.</param>
    /// <param name="advantageType">Whether to roll with advantage or disadvantage.</param>
    /// <returns>The complete roll result with success-counting breakdown.</returns>
    public DiceRollResult Roll(DicePool pool, AdvantageType advantageType = AdvantageType.Normal)
    {
        _logger.LogDebug("Rolling {Pool} with {AdvantageType}", pool, advantageType);

        if (advantageType == AdvantageType.Normal)
        {
            var result = RollOnce(pool, advantageType);
            LogRollEvent(result);
            return result;
        }

        // Roll twice for advantage/disadvantage
        var roll1 = RollOnce(pool, advantageType);
        var roll2 = RollOnce(pool, advantageType);

        // For success-counting, compare NetSuccesses (not Total)
        var allNetSuccesses = new[] { roll1.NetSuccesses, roll2.NetSuccesses };
        var selectedIndex = advantageType == AdvantageType.Advantage
            ? (roll1.NetSuccesses >= roll2.NetSuccesses ? 0 : 1)
            : (roll1.NetSuccesses <= roll2.NetSuccesses ? 0 : 1);

        var selectedRoll = selectedIndex == 0 ? roll1 : roll2;

        // Reconstruct with advantage info
        var result2 = new DiceRollResult(
            pool,
            selectedRoll.Rolls,
            advantageType,
            selectedRoll.ExplosionRolls,
            allNetSuccesses,
            selectedIndex);

        _logger.LogInformation(
            "Roll {Pool} ({AdvantageType}): [{Roll1}, {Roll2}] net successes -> {Selected} selected",
            pool, advantageType, roll1.NetSuccesses, roll2.NetSuccesses, result2.NetSuccesses);

        LogRollEvent(result2);

        return result2;
    }

    /// <summary>
    /// Parses dice notation and rolls.
    /// </summary>
    /// <param name="notation">Dice notation (e.g., "3d10").</param>
    /// <param name="advantageType">Advantage/disadvantage.</param>
    /// <returns>Roll result with success counts.</returns>
    /// <exception cref="FormatException">If notation is invalid.</exception>
    public DiceRollResult Roll(string notation, AdvantageType advantageType = AdvantageType.Normal)
    {
        var pool = DicePool.Parse(notation);
        return Roll(pool, advantageType);
    }

    /// <summary>
    /// Convenience method for rolling a single die type.
    /// </summary>
    /// <param name="diceType">Type of die to roll.</param>
    /// <param name="count">Number of dice (default 1).</param>
    /// <param name="modifier">Modifier to add (default 0).</param>
    /// <returns>Roll result with success counts.</returns>
    public DiceRollResult Roll(DiceType diceType, int count = 1, int modifier = 0)
    {
        var pool = new DicePool(count, diceType, modifier);
        return Roll(pool);
    }

    /// <summary>
    /// Rolls a dice pool specifically for damage (sum-based, not success-counting).
    /// </summary>
    /// <param name="pool">The dice pool to roll.</param>
    /// <returns>Roll result; use RawTotal or Total for damage value.</returns>
    /// <remarks>
    /// Damage rolls use sum-based mechanics. Use the RawTotal property
    /// for the damage value before modifiers, or Total for final damage.
    /// </remarks>
    public DiceRollResult RollDamage(DicePool pool)
    {
        var result = Roll(pool);
        _logger.LogDebug("Damage roll {Pool}: {Total} total (raw: {RawTotal})", pool, result.Total, result.RawTotal);
        return result;
    }

    /// <summary>
    /// Quick roll returning net successes (for skill checks).
    /// </summary>
    /// <param name="pool">The dice pool to roll.</param>
    /// <returns>The net success count.</returns>
    public int RollNetSuccesses(DicePool pool) => Roll(pool).NetSuccesses;

    /// <summary>
    /// Quick roll returning just the raw total (for damage).
    /// </summary>
    /// <param name="pool">The dice pool to roll.</param>
    /// <returns>The total result (sum-based).</returns>
    public int RollTotal(DicePool pool) => Roll(pool).Total;

    /// <summary>
    /// Quick roll from notation returning just the total.
    /// </summary>
    /// <param name="notation">Dice notation (e.g., "3d6+5").</param>
    /// <returns>The total result (sum-based).</returns>
    public int RollTotal(string notation) => Roll(notation).Total;

    // ═══════════════════════════════════════════════════════════════════════════
    // History-Recording Roll Methods (v0.12.0b)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Rolls a dice pool and records the result to the player's dice history.
    /// </summary>
    /// <param name="pool">The dice pool to roll.</param>
    /// <param name="player">The player whose history to record the roll to.</param>
    /// <param name="context">The context of the roll (e.g., "attack", "skill check", "damage").</param>
    /// <param name="advantageType">Whether to roll with advantage or disadvantage.</param>
    /// <returns>The complete roll result with breakdown.</returns>
    /// <remarks>
    /// <para>
    /// This overload records the roll to the player's dice history for statistics tracking.
    /// If the dice history service is not available, the roll is still performed but not recorded.
    /// </para>
    /// </remarks>
    public DiceRollResult Roll(
        DicePool pool,
        Player player,
        string context = "general",
        AdvantageType advantageType = AdvantageType.Normal)
    {
        var result = Roll(pool, advantageType);
        RecordRollToHistory(result, player, context);
        return result;
    }

    /// <summary>
    /// Parses dice notation, rolls, and records the result to the player's dice history.
    /// </summary>
    /// <param name="notation">Dice notation (e.g., "3d6+5").</param>
    /// <param name="player">The player whose history to record the roll to.</param>
    /// <param name="context">The context of the roll (e.g., "attack", "skill check", "damage").</param>
    /// <param name="advantageType">Advantage/disadvantage.</param>
    /// <returns>Roll result.</returns>
    /// <exception cref="FormatException">If notation is invalid.</exception>
    /// <remarks>
    /// <para>
    /// This overload records the roll to the player's dice history for statistics tracking.
    /// If the dice history service is not available, the roll is still performed but not recorded.
    /// </para>
    /// </remarks>
    public DiceRollResult Roll(
        string notation,
        Player player,
        string context = "general",
        AdvantageType advantageType = AdvantageType.Normal)
    {
        var pool = DicePool.Parse(notation);
        return Roll(pool, player, context, advantageType);
    }

    /// <summary>
    /// Rolls a single die type and records the result to the player's dice history.
    /// </summary>
    /// <param name="diceType">Type of die to roll.</param>
    /// <param name="player">The player whose history to record the roll to.</param>
    /// <param name="context">The context of the roll (e.g., "attack", "skill check", "damage").</param>
    /// <param name="count">Number of dice (default 1).</param>
    /// <param name="modifier">Modifier to add (default 0).</param>
    /// <returns>Roll result.</returns>
    /// <remarks>
    /// <para>
    /// This overload records the roll to the player's dice history for statistics tracking.
    /// If the dice history service is not available, the roll is still performed but not recorded.
    /// </para>
    /// </remarks>
    public DiceRollResult Roll(
        DiceType diceType,
        Player player,
        string context = "general",
        int count = 1,
        int modifier = 0)
    {
        var pool = new DicePool(count, diceType, modifier);
        return Roll(pool, player, context);
    }

    /// <summary>
    /// Quick roll returning just the total, with history recording.
    /// </summary>
    /// <param name="pool">The dice pool to roll.</param>
    /// <param name="player">The player whose history to record the roll to.</param>
    /// <param name="context">The context of the roll.</param>
    /// <returns>The total result.</returns>
    public int RollTotal(DicePool pool, Player player, string context = "general")
        => Roll(pool, player, context).Total;

    /// <summary>
    /// Quick roll from notation returning just the total, with history recording.
    /// </summary>
    /// <param name="notation">Dice notation (e.g., "3d6+5").</param>
    /// <param name="player">The player whose history to record the roll to.</param>
    /// <param name="context">The context of the roll.</param>
    /// <returns>The total result.</returns>
    public int RollTotal(string notation, Player player, string context = "general")
        => Roll(notation, player, context).Total;

    /// <summary>
    /// Records a roll result to the player's dice history.
    /// </summary>
    /// <param name="result">The roll result to record.</param>
    /// <param name="player">The player whose history to record to.</param>
    /// <param name="context">The context of the roll.</param>
    private void RecordRollToHistory(DiceRollResult result, Player player, string context)
    {
        if (_historyService is null || player is null)
        {
            return;
        }

        // Create a DiceRollRecord from the DiceRollResult using RawTotal for damage-like contexts
        var record = DiceRollRecord.Create(
            expression: result.Pool.ToString(),
            result: result.RawTotal,
            rolls: result.Rolls.ToArray(),
            context: context);

        _historyService.RecordRoll(player, record);

        _logger.LogDebug(
            "Recorded roll to history: {Expression} = {Result} ({NetSuccesses} net successes) for player {PlayerId}",
            result.Pool,
            result.RawTotal,
            result.NetSuccesses,
            player.Id);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Private Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Performs a single roll of the dice pool.
    /// </summary>
    private DiceRollResult RollOnce(DicePool pool, AdvantageType advantageType)
    {
        // Enforce minimum pool size
        var actualCount = Math.Max(DiceConstants.MinimumPool, pool.Count);
        var actualPool = actualCount != pool.Count
            ? new DicePool(actualCount, pool.DiceType, pool.Modifier, pool.Exploding, pool.MaxExplosions)
            : pool;

        var rolls = new List<int>();
        var explosions = new List<int>();

        // Roll each die
        for (var i = 0; i < actualPool.Count; i++)
        {
            var roll = RollSingleDie(actualPool.Faces);
            rolls.Add(roll);

            // Handle exploding dice
            if (actualPool.Exploding && roll == actualPool.Faces)
            {
                var explosionCount = 0;
                var explosionRoll = roll;

                while (explosionRoll == actualPool.Faces && explosionCount < actualPool.MaxExplosions)
                {
                    explosionRoll = RollSingleDie(actualPool.Faces);
                    explosions.Add(explosionRoll);
                    explosionCount++;

                    _logger.LogDebug(
                        "Die exploded! Roll {Explosion} on d{Faces} (explosion {Count})",
                        explosionRoll, actualPool.Faces, explosionCount);
                }
            }
        }

        // Create result (constructor handles success counting)
        var result = new DiceRollResult(
            actualPool,
            rolls.AsReadOnly(),
            advantageType,
            explosions.AsReadOnly());

        _logger.LogDebug(
            "Rolled {Pool}: dice=[{Rolls}] explosions=[{Explosions}] → {Successes}S - {Botches}B = {Net} net{Special}",
            actualPool,
            string.Join(",", rolls),
            string.Join(",", explosions),
            result.TotalSuccesses,
            result.TotalBotches,
            result.NetSuccesses,
            result.IsFumble ? " [FUMBLE]" : result.IsCriticalSuccess ? " [CRITICAL]" : "");

        return result;
    }

    /// <summary>
    /// Rolls a single die with the specified number of faces.
    /// </summary>
    private int RollSingleDie(int faces) => _random.Next(1, faces + 1);

    /// <summary>
    /// Logs a roll event to the game event logger if available.
    /// </summary>
    private void LogRollEvent(DiceRollResult result)
    {
        _eventLogger?.LogDice("DiceRolled", $"{result.Pool} → {result.NetSuccesses} net successes",
            data: new Dictionary<string, object>
            {
                ["pool"] = result.Pool.ToString(),
                ["rolls"] = result.Rolls.ToArray(),
                ["totalSuccesses"] = result.TotalSuccesses,
                ["totalBotches"] = result.TotalBotches,
                ["netSuccesses"] = result.NetSuccesses,
                ["isFumble"] = result.IsFumble,
                ["isCriticalSuccess"] = result.IsCriticalSuccess,
                ["rawTotal"] = result.RawTotal,
                ["advantageType"] = result.AdvantageType.ToString()
            });
    }
}
