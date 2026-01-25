using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Core dice rolling service. Handles all randomization in the game.
/// </summary>
/// <remarks>
/// Supports standard dice pools, exploding dice, and advantage/disadvantage rolls.
/// Accepts an optional Random instance for deterministic testing.
/// </remarks>
public class DiceService : IDiceService
{
    private readonly Random _random;
    private readonly ILogger<DiceService> _logger;
    private readonly IGameEventLogger? _eventLogger;

    /// <summary>
    /// Creates a new DiceService.
    /// </summary>
    public DiceService(
        ILogger<DiceService> logger,
        Random? random = null,
        IGameEventLogger? eventLogger = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = random ?? new Random();
        _eventLogger = eventLogger;
        _logger.LogInformation("DiceService initialized");
    }

    /// <summary>
    /// Rolls a dice pool and returns the result.
    /// </summary>
    /// <param name="pool">The dice pool to roll.</param>
    /// <param name="advantageType">Whether to roll with advantage or disadvantage.</param>
    /// <param name="context">The context for this roll.</param>
    /// <returns>The complete roll result with breakdown.</returns>
    public DiceRollResult Roll(DicePool pool, AdvantageType advantageType = AdvantageType.Normal, string context = "Unspecified")
    {
        _logger.LogTrace("Rolling {Pool} with {AdvantageType} for {Context}", pool, advantageType, context);

        if (advantageType == AdvantageType.Normal)
        {
            return RollOnce(pool, advantageType, context);
        }

        // Roll twice for advantage/disadvantage
        var roll1 = RollOnce(pool, advantageType, context);
        var roll2 = RollOnce(pool, advantageType, context);

        var allTotals = new[] { roll1.Total, roll2.Total };
        var selectedIndex = advantageType == AdvantageType.Advantage
            ? (roll1.Total >= roll2.Total ? 0 : 1)
            : (roll1.Total <= roll2.Total ? 0 : 1);

        var selectedRoll = selectedIndex == 0 ? roll1 : roll2;

        var result = new DiceRollResult(
            pool,
            selectedRoll.Rolls,
            selectedRoll.Total,
            advantageType,
            selectedRoll.ExplosionRolls,
            allTotals,
            selectedIndex);

        _logger.LogDebug(
            "Roll {Pool} ({AdvantageType}) for {Context}: [{Roll1}, {Roll2}] -> {Selected}",
            pool, advantageType, context, roll1.Total, roll2.Total, result.Total);

        _eventLogger?.LogDice("DiceRolled", $"{pool} = {result.Total}",
            data: new Dictionary<string, object>
            {
                ["context"] = context,
                ["pool"] = pool.ToString(),
                ["total"] = result.Total,
                ["advantageType"] = advantageType.ToString()
            });

        return result;
    }

    /// <summary>
    /// Parses dice notation and rolls.
    /// </summary>
    /// <param name="notation">Dice notation (e.g., "3d6+5").</param>
    /// <param name="advantageType">Advantage/disadvantage.</param>
    /// <param name="context">The context for this roll.</param>
    /// <returns>Roll result.</returns>
    /// <exception cref="FormatException">If notation is invalid.</exception>
    public DiceRollResult Roll(string notation, AdvantageType advantageType = AdvantageType.Normal, string context = "Unspecified")
    {
        var pool = DicePool.Parse(notation);
        return Roll(pool, advantageType, context);
    }

    /// <summary>
    /// Convenience method for rolling a single die type.
    /// </summary>
    /// <param name="diceType">Type of die to roll.</param>
    /// <param name="count">Number of dice (default 1).</param>
    /// <param name="modifier">Modifier to add (default 0).</param>
    /// <returns>Roll result.</returns>
    public DiceRollResult Roll(DiceType diceType, int count = 1, int modifier = 0)
    {
        var pool = new DicePool(count, diceType, modifier);
        return Roll(pool);
    }

    /// <summary>
    /// Quick roll returning just the total (for internal use).
    /// </summary>
    /// <param name="pool">The dice pool to roll.</param>
    /// <returns>The total result.</returns>
    public int RollTotal(DicePool pool) => Roll(pool).Total;

    /// <summary>
    /// Quick roll from notation returning just the total.
    /// </summary>
    /// <param name="notation">Dice notation (e.g., "3d6+5").</param>
    /// <returns>The total result.</returns>
    public int RollTotal(string notation) => Roll(notation).Total;

    /// <summary>
    /// Performs a single roll of the dice pool.
    /// </summary>
    private DiceRollResult RollOnce(DicePool pool, AdvantageType advantageType, string context)
    {
        var rolls = new List<int>();
        var explosions = new List<int>();

        // Roll each die
        for (var i = 0; i < pool.Count; i++)
        {
            var roll = RollSingleDie(pool.Faces);
            rolls.Add(roll);

            _logger.LogTrace("Rolled die {Index} for {Context}: {Roll} (d{Faces})", i, context, roll, pool.Faces);

            // Handle exploding dice
            if (pool.Exploding && roll == pool.Faces)
            {
                var explosionCount = 0;
                var explosionRoll = roll;

                while (explosionRoll == pool.Faces && explosionCount < pool.MaxExplosions)
                {
                    explosionRoll = RollSingleDie(pool.Faces);
                    explosions.Add(explosionRoll);
                    explosionCount++;

                    _logger.LogDebug(
                        "Die exploded! Roll {Explosion} on d{Faces} (explosion {Count}) for {Context}",
                        explosionRoll, pool.Faces, explosionCount, context);
                }
            }
        }

        var diceTotal = rolls.Sum() + explosions.Sum();
        var total = diceTotal + pool.Modifier;

        _logger.LogDebug(
            "Rolled {Pool} for {Context}: dice=[{Rolls}] explosions=[{Explosions}] total={Total}",
            pool, context, string.Join(",", rolls), string.Join(",", explosions), total);

        return new DiceRollResult(
            pool,
            rolls.AsReadOnly(),
            total,
            advantageType,
            explosions.AsReadOnly());
    }

    /// <summary>
    /// Rolls a single die with the specified number of faces.
    /// </summary>
    private int RollSingleDie(int faces) => _random.Next(1, faces + 1);
}
