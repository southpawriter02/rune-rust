namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents stat modifications that can be applied to a character.
/// </summary>
/// <remarks>
/// Used for class bonuses, equipment bonuses, buff effects, etc.
/// Values can be positive (bonuses) or negative (penalties).
/// </remarks>
public readonly record struct StatModifiers
{
    /// <summary>Maximum health modifier.</summary>
    public int MaxHealth { get; init; }

    /// <summary>Attack power modifier.</summary>
    public int Attack { get; init; }

    /// <summary>Defense modifier.</summary>
    public int Defense { get; init; }

    /// <summary>Might attribute modifier.</summary>
    public int Might { get; init; }

    /// <summary>Fortitude attribute modifier.</summary>
    public int Fortitude { get; init; }

    /// <summary>Will attribute modifier.</summary>
    public int Will { get; init; }

    /// <summary>Wits attribute modifier.</summary>
    public int Wits { get; init; }

    /// <summary>Finesse attribute modifier.</summary>
    public int Finesse { get; init; }

    /// <summary>
    /// Gets a StatModifiers instance with all values set to zero.
    /// </summary>
    public static StatModifiers None => new();

    /// <summary>
    /// Applies these modifiers to a base Stats value.
    /// </summary>
    /// <param name="baseStats">The base stats to modify.</param>
    /// <returns>A new Stats instance with modifiers applied.</returns>
    public Stats ApplyTo(Stats baseStats)
    {
        return new Stats(
            maxHealth: Math.Max(1, baseStats.MaxHealth + MaxHealth),
            attack: Math.Max(0, baseStats.Attack + Attack),
            defense: Math.Max(0, baseStats.Defense + Defense)
        );
    }

    /// <summary>
    /// Combines two StatModifiers by adding their values.
    /// </summary>
    public static StatModifiers operator +(StatModifiers a, StatModifiers b)
    {
        return new StatModifiers
        {
            MaxHealth = a.MaxHealth + b.MaxHealth,
            Attack = a.Attack + b.Attack,
            Defense = a.Defense + b.Defense,
            Might = a.Might + b.Might,
            Fortitude = a.Fortitude + b.Fortitude,
            Will = a.Will + b.Will,
            Wits = a.Wits + b.Wits,
            Finesse = a.Finesse + b.Finesse
        };
    }

    /// <summary>
    /// Returns a formatted string representation of the modifiers.
    /// </summary>
    public override string ToString()
    {
        var parts = new List<string>();
        if (MaxHealth != 0) parts.Add($"HP:{(MaxHealth > 0 ? "+" : "")}{MaxHealth}");
        if (Attack != 0) parts.Add($"ATK:{(Attack > 0 ? "+" : "")}{Attack}");
        if (Defense != 0) parts.Add($"DEF:{(Defense > 0 ? "+" : "")}{Defense}");
        if (Might != 0) parts.Add($"MIG:{(Might > 0 ? "+" : "")}{Might}");
        if (Fortitude != 0) parts.Add($"FOR:{(Fortitude > 0 ? "+" : "")}{Fortitude}");
        if (Will != 0) parts.Add($"WIL:{(Will > 0 ? "+" : "")}{Will}");
        if (Wits != 0) parts.Add($"WIT:{(Wits > 0 ? "+" : "")}{Wits}");
        if (Finesse != 0) parts.Add($"FIN:{(Finesse > 0 ? "+" : "")}{Finesse}");
        return parts.Count > 0 ? string.Join(" ", parts) : "None";
    }
}
