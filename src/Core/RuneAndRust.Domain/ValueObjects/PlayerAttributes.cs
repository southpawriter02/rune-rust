namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the core attribute values for a player character.
/// </summary>
public readonly record struct PlayerAttributes
{
    /// <summary>
    /// Physical power and strength.
    /// </summary>
    public int Might { get; init; }

    /// <summary>
    /// Endurance and constitution.
    /// </summary>
    public int Fortitude { get; init; }

    /// <summary>
    /// Mental fortitude and magical power.
    /// </summary>
    public int Will { get; init; }

    /// <summary>
    /// Intelligence and perception.
    /// </summary>
    public int Wits { get; init; }

    /// <summary>
    /// Agility and precision.
    /// </summary>
    public int Finesse { get; init; }

    /// <summary>
    /// Creates attributes with specified values.
    /// </summary>
    public PlayerAttributes(int might, int fortitude, int will, int wits, int finesse)
    {
        Might = ValidateAttribute(might, nameof(might));
        Fortitude = ValidateAttribute(fortitude, nameof(fortitude));
        Will = ValidateAttribute(will, nameof(will));
        Wits = ValidateAttribute(wits, nameof(wits));
        Finesse = ValidateAttribute(finesse, nameof(finesse));
    }

    /// <summary>
    /// Gets the default starting attributes (all 8).
    /// </summary>
    public static PlayerAttributes Default => new(8, 8, 8, 8, 8);

    /// <summary>
    /// Gets an attribute value by its ID.
    /// </summary>
    public int GetByName(string attributeId) => attributeId.ToLowerInvariant() switch
    {
        "might" or "mig" => Might,
        "fortitude" or "for" => Fortitude,
        "will" or "wil" => Will,
        "wits" or "wit" => Wits,
        "finesse" or "fin" => Finesse,
        _ => throw new ArgumentException($"Unknown attribute: {attributeId}")
    };

    /// <summary>
    /// Creates new attributes with a modifier applied.
    /// </summary>
    public PlayerAttributes WithModifiers(IReadOnlyDictionary<string, int> modifiers)
    {
        var might = Might;
        var fortitude = Fortitude;
        var will = Will;
        var wits = Wits;
        var finesse = Finesse;

        foreach (var (attr, mod) in modifiers)
        {
            switch (attr.ToLowerInvariant())
            {
                case "might": might += mod; break;
                case "fortitude": fortitude += mod; break;
                case "will": will += mod; break;
                case "wits": wits += mod; break;
                case "finesse": finesse += mod; break;
            }
        }

        return new PlayerAttributes(
            Math.Clamp(might, 1, 30),
            Math.Clamp(fortitude, 1, 30),
            Math.Clamp(will, 1, 30),
            Math.Clamp(wits, 1, 30),
            Math.Clamp(finesse, 1, 30)
        );
    }

    /// <summary>
    /// Calculates the total point cost of these attributes using point-buy rules.
    /// Base 8 costs 0, each point up to 14 costs 1, each point above 14 costs 2.
    /// </summary>
    public int CalculatePointCost()
    {
        return CalculateSingleCost(Might) +
               CalculateSingleCost(Fortitude) +
               CalculateSingleCost(Will) +
               CalculateSingleCost(Wits) +
               CalculateSingleCost(Finesse);
    }

    /// <summary>
    /// Validates that attributes are within point-buy budget.
    /// </summary>
    /// <param name="maxPoints">Maximum allowed points (default 25).</param>
    /// <returns>True if within budget.</returns>
    public bool IsWithinBudget(int maxPoints = 25) => CalculatePointCost() <= maxPoints;

    private static int CalculateSingleCost(int value)
    {
        // Base 8 costs 0, each point up to 14 costs 1, 15 costs 2 extra
        if (value <= 8) return 0;
        if (value <= 14) return value - 8;
        return (14 - 8) + ((value - 14) * 2); // 6 + 2 per point above 14
    }

    private static int ValidateAttribute(int value, string name)
    {
        if (value < 1 || value > 30)
            throw new ArgumentOutOfRangeException(name, $"Attribute must be between 1 and 30");
        return value;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"MIG:{Might} FOR:{Fortitude} WIL:{Will} WIT:{Wits} FIN:{Finesse}";
}
