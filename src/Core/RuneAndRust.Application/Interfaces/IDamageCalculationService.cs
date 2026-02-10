using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for calculating damage with resistance modifiers.
/// </summary>
public interface IDamageCalculationService
{
    /// <summary>
    /// The default damage type used when none is specified.
    /// </summary>
    public const string DefaultDamageType = "physical";

    /// <summary>
    /// Calculates final damage after applying resistance modifiers.
    /// </summary>
    /// <param name="baseDamage">The base damage before resistances.</param>
    /// <param name="damageTypeId">The type of damage (e.g., "physical", "fire"). Null uses physical.</param>
    /// <param name="targetResistances">The target's damage resistances.</param>
    /// <param name="context">The context in which this damage is being calculated.</param>
    /// <returns>A DamageInstance containing the calculation results.</returns>
    DamageInstance CalculateDamage(int baseDamage, string? damageTypeId, DamageResistances targetResistances, string context = "Unspecified");

    /// <summary>
    /// Gets a human-readable description of a resistance value.
    /// </summary>
    /// <param name="damageTypeId">The damage type ID.</param>
    /// <param name="resistances">The resistance set to check.</param>
    /// <returns>Description like "Resistant (50%)" or "Vulnerable (-50%)".</returns>
    string GetResistanceDescription(string damageTypeId, DamageResistances resistances);

    /// <summary>
    /// Gets a label for a resistance value.
    /// </summary>
    /// <param name="resistance">The resistance percentage (-100 to +100).</param>
    /// <returns>A label like "Immune", "Resistant", "Normal", or "Vulnerable".</returns>
    string GetResistanceLabel(int resistance);
}
