namespace RuneAndRust.Domain.ValueObjects;

public readonly record struct Stats
{
    public int MaxHealth { get; init; }
    public int Attack { get; init; }
    public int Defense { get; init; }

    public Stats(int maxHealth, int attack, int defense)
    {
        if (maxHealth < 1)
            throw new ArgumentOutOfRangeException(nameof(maxHealth), "Max health must be at least 1");
        if (attack < 0)
            throw new ArgumentOutOfRangeException(nameof(attack), "Attack cannot be negative");
        if (defense < 0)
            throw new ArgumentOutOfRangeException(nameof(defense), "Defense cannot be negative");

        MaxHealth = maxHealth;
        Attack = attack;
        Defense = defense;
    }

    public static Stats Default => new(100, 10, 5);

    public override string ToString() => $"HP: {MaxHealth}, ATK: {Attack}, DEF: {Defense}";
}
