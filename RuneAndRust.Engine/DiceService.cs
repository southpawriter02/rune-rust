using Serilog;

namespace RuneAndRust.Engine;

public class DiceService
{
    private static readonly ILogger _log = Log.ForContext<DiceService>();
    private readonly Random _random;

    public DiceService()
    {
        _random = new Random();
    }

    public DiceService(int seed)
    {
        _random = new Random(seed);
    }

    /// <summary>
    /// Rolls the specified number of d6 dice and counts successes (5-6 are successes)
    /// </summary>
    public DiceResult Roll(int diceCount)
    {
        if (diceCount <= 0)
        {
            return new DiceResult(0, new List<int>(), 0);
        }

        var rolls = new List<int>();
        int successes = 0;

        for (int i = 0; i < diceCount; i++)
        {
            int roll = _random.Next(1, 7); // 1-6
            rolls.Add(roll);

            if (roll >= 5) // 5 or 6 is a success
            {
                successes++;
            }
        }

        var result = new DiceResult(diceCount, rolls, successes);

        // Log at Debug level - dice rolls are frequent
        _log.Debug("Dice rolled: DiceCount={DiceCount}, Rolls=[{Rolls}], Successes={Successes}",
            diceCount, string.Join(", ", rolls), successes);

        return result;
    }

    /// <summary>
    /// Rolls a standard d6 for simple damage calculations
    /// </summary>
    public int RollD6()
    {
        return _random.Next(1, 7);
    }

    /// <summary>
    /// Rolls multiple d6 and returns the total
    /// </summary>
    public int RollDamage(int diceCount)
    {
        int total = 0;
        var rolls = new List<int>();

        for (int i = 0; i < diceCount; i++)
        {
            int roll = RollD6();
            rolls.Add(roll);
            total += roll;
        }

        _log.Debug("Damage dice rolled: DiceCount={DiceCount}, Rolls=[{Rolls}], Total={Total}",
            diceCount, string.Join(", ", rolls), total);

        return total;
    }

    /// <summary>
    /// v0.21.3: Rolls dice with variable die size (e.g., 3d4, 2d6, 1d10)
    /// </summary>
    public int Roll(int numDice, int dieSize)
    {
        if (numDice <= 0 || dieSize <= 0)
        {
            return 0;
        }

        int total = 0;
        var rolls = new List<int>();

        for (int i = 0; i < numDice; i++)
        {
            int roll = _random.Next(1, dieSize + 1);
            rolls.Add(roll);
            total += roll;
        }

        _log.Debug("Dice rolled: {NumDice}d{DieSize}, Rolls=[{Rolls}], Total={Total}",
            numDice, dieSize, string.Join(", ", rolls), total);

        return total;
    }
}
