namespace RuneAndRust.Core.Models.Magic;

public class AetherPool
{
    public int Current { get; private set; }
    public int Max { get; private set; }
    public int Flux { get; private set; }

    public AetherPool(int max)
    {
        Max = max;
        Current = max;
        Flux = 0;
    }

    public void Modify(int amount)
    {
        Current = Math.Clamp(Current + amount, 0, Max);
    }

    public void AddFlux(int amount)
    {
        Flux += amount;
    }

    public void ReduceFlux(int amount)
    {
        Flux = Math.Max(0, Flux - amount);
    }
}
