namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of a damage calculation including resistance effects.
/// </summary>
/// <param name="BaseDamage">The original damage before resistances.</param>
/// <param name="DamageTypeId">The type of damage dealt.</param>
/// <param name="FinalDamage">The damage after applying resistances.</param>
/// <param name="ResistanceApplied">The resistance percentage that was applied.</param>
/// <param name="WasResisted">True if damage was reduced by resistance.</param>
/// <param name="WasVulnerable">True if damage was increased by vulnerability.</param>
/// <param name="WasImmune">True if the target was immune.</param>
public readonly record struct DamageInstance(
    int BaseDamage,
    string DamageTypeId,
    int FinalDamage,
    int ResistanceApplied,
    bool WasResisted,
    bool WasVulnerable,
    bool WasImmune)
{
    /// <summary>
    /// Gets whether any resistance effect was applied.
    /// </summary>
    public bool HadResistanceEffect => WasResisted || WasVulnerable || WasImmune;

    /// <summary>
    /// Gets a human-readable description of the resistance effect.
    /// </summary>
    /// <returns>Description string, or empty if no effect.</returns>
    public string GetResistanceDescription()
    {
        if (WasImmune)
            return "Immune!";

        if (WasVulnerable)
            return $"Vulnerable! ({Math.Abs(ResistanceApplied)}% extra damage)";

        if (WasResisted)
            return $"Resisted ({ResistanceApplied}% reduced)";

        return string.Empty;
    }

    /// <summary>
    /// Creates a DamageInstance with no resistance effects.
    /// </summary>
    /// <param name="damage">The damage amount.</param>
    /// <param name="damageTypeId">The damage type ID.</param>
    /// <returns>A new DamageInstance with normal damage.</returns>
    public static DamageInstance Normal(int damage, string damageTypeId) =>
        new(damage, damageTypeId, damage, 0, false, false, false);

    /// <summary>
    /// Creates a DamageInstance representing immunity.
    /// </summary>
    /// <param name="baseDamage">The base damage that was negated.</param>
    /// <param name="damageTypeId">The damage type ID.</param>
    /// <returns>A new DamageInstance with zero final damage.</returns>
    public static DamageInstance Immune(int baseDamage, string damageTypeId) =>
        new(baseDamage, damageTypeId, 0, 100, false, false, true);
}
