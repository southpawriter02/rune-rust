namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents stat bonuses provided by a weapon when equipped.
/// </summary>
/// <param name="Might">Bonus to Might attribute.</param>
/// <param name="Fortitude">Bonus to Fortitude attribute.</param>
/// <param name="Will">Bonus to Will attribute.</param>
/// <param name="Wits">Bonus to Wits attribute.</param>
/// <param name="Finesse">Bonus to Finesse attribute.</param>
/// <param name="AttackModifier">Flat bonus/penalty to attack rolls.</param>
public readonly record struct WeaponBonuses(
    int Might = 0,
    int Fortitude = 0,
    int Will = 0,
    int Wits = 0,
    int Finesse = 0,
    int AttackModifier = 0)
{
    /// <summary>
    /// Returns true if this weapon provides any bonuses.
    /// </summary>
    public bool HasBonuses => Might != 0 || Fortitude != 0 || Will != 0 ||
                              Wits != 0 || Finesse != 0 || AttackModifier != 0;

    /// <summary>
    /// Creates an empty bonuses instance (no bonuses).
    /// </summary>
    public static WeaponBonuses None => new();

    /// <summary>
    /// Creates bonuses for a Finesse-based weapon.
    /// </summary>
    /// <param name="bonus">The Finesse bonus value.</param>
    /// <returns>A WeaponBonuses with the specified Finesse bonus.</returns>
    public static WeaponBonuses ForFinesse(int bonus) => new(Finesse: bonus);

    /// <summary>
    /// Creates bonuses for a Will-based weapon.
    /// </summary>
    /// <param name="bonus">The Will bonus value.</param>
    /// <returns>A WeaponBonuses with the specified Will bonus.</returns>
    public static WeaponBonuses ForWill(int bonus) => new(Will: bonus);

    /// <summary>
    /// Creates bonuses with an attack modifier.
    /// </summary>
    /// <param name="modifier">The attack roll modifier (positive or negative).</param>
    /// <returns>A WeaponBonuses with the specified attack modifier.</returns>
    public static WeaponBonuses ForAttack(int modifier) => new(AttackModifier: modifier);

    /// <summary>
    /// Returns a display string of non-zero bonuses.
    /// </summary>
    /// <returns>A formatted string of bonuses, or "None" if no bonuses.</returns>
    public override string ToString()
    {
        var parts = new List<string>();
        if (Might != 0) parts.Add($"{(Might > 0 ? "+" : "")}{Might} Might");
        if (Fortitude != 0) parts.Add($"{(Fortitude > 0 ? "+" : "")}{Fortitude} Fortitude");
        if (Will != 0) parts.Add($"{(Will > 0 ? "+" : "")}{Will} Will");
        if (Wits != 0) parts.Add($"{(Wits > 0 ? "+" : "")}{Wits} Wits");
        if (Finesse != 0) parts.Add($"{(Finesse > 0 ? "+" : "")}{Finesse} Finesse");
        if (AttackModifier != 0) parts.Add($"{(AttackModifier > 0 ? "+" : "")}{AttackModifier} Attack");
        return parts.Count > 0 ? string.Join(", ", parts) : "None";
    }
}
