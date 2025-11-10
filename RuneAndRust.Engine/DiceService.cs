namespace RuneAndRust.Engine;

public class DiceService
{
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

        return new DiceResult(diceCount, rolls, successes);
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
        for (int i = 0; i < diceCount; i++)
        {
            total += RollD6();
        }
        return total;
    }
}
