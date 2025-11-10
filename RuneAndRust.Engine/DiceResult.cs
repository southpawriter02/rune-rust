namespace RuneAndRust.Engine;

public class DiceResult
{
    public int DiceRolled { get; set; }
    public List<int> Rolls { get; set; } = new();
    public int Successes { get; set; }
    public int TotalValue { get; set; }

    public DiceResult() { }

    public DiceResult(int diceRolled, List<int> rolls, int successes)
    {
        DiceRolled = diceRolled;
        Rolls = rolls;
        Successes = successes;
        TotalValue = rolls.Sum();
    }

    public override string ToString()
    {
        var rollsString = string.Join(", ", Rolls);
        return $"Rolled {DiceRolled}d6: [{rollsString}] = {Successes} successes";
    }
}
