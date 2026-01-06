namespace RuneAndRust.Domain.ValueObjects;

public readonly record struct Stats
{
    public int MaxHealth { get; init; }
    public int Attack { get; init; }
    public int Defense { get; init; }
    public int Wits { get; init; }

    /// <summary>
    /// Passive perception is WITS / 2, rounded up.
    /// Used for automatic detection of hidden elements on room entry.
    /// </summary>
    public int PassivePerception => (Wits + 1) / 2;

    public Stats(int maxHealth, int attack, int defense, int wits = 10)
    {
        if (maxHealth < 1)
            throw new ArgumentOutOfRangeException(nameof(maxHealth), "Max health must be at least 1");
        if (attack < 0)
            throw new ArgumentOutOfRangeException(nameof(attack), "Attack cannot be negative");
        if (defense < 0)
            throw new ArgumentOutOfRangeException(nameof(defense), "Defense cannot be negative");
        if (wits < 1 || wits > 20)
            throw new ArgumentOutOfRangeException(nameof(wits), "Wits must be between 1 and 20");

        MaxHealth = maxHealth;
        Attack = attack;
        Defense = defense;
        Wits = wits;
    }

    public static Stats Default => new(100, 10, 5, 10);

    public override string ToString() => $"HP: {MaxHealth}, ATK: {Attack}, DEF: {Defense}, WITS: {Wits}";
}
