using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for stat calculation services.
/// Provides modifier operations, attribute clamping, and derived stat calculations.
/// </summary>
public interface IStatCalculationService
{
    /// <summary>
    /// Applies a modifier to a base value.
    /// </summary>
    /// <param name="baseValue">The original value.</param>
    /// <param name="modifier">The modifier to apply (positive or negative).</param>
    /// <returns>The modified value (baseValue + modifier).</returns>
    int ApplyModifier(int baseValue, int modifier);

    /// <summary>
    /// Clamps an attribute value to ensure it stays within valid bounds.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum allowed value (default: 1).</param>
    /// <param name="max">The maximum allowed value (default: 10).</param>
    /// <returns>The clamped value, guaranteed to be within [min, max].</returns>
    int ClampAttribute(int value, int min = 1, int max = 10);

    /// <summary>
    /// Calculates the maximum HP for a character based on Sturdiness.
    /// Formula: 50 + (Sturdiness * 10).
    /// </summary>
    /// <param name="sturdiness">The character's Sturdiness attribute.</param>
    /// <returns>The calculated maximum HP.</returns>
    int CalculateMaxHP(int sturdiness);

    /// <summary>
    /// Calculates the maximum Stamina for a character based on Finesse and Sturdiness.
    /// Formula: 20 + (Finesse * 5) + (Sturdiness * 3).
    /// </summary>
    /// <param name="finesse">The character's Finesse attribute.</param>
    /// <param name="sturdiness">The character's Sturdiness attribute.</param>
    /// <returns>The calculated maximum Stamina.</returns>
    int CalculateMaxStamina(int finesse, int sturdiness);

    /// <summary>
    /// Calculates the action points per turn based on Wits.
    /// Formula: 2 + (Wits / 4) using integer division.
    /// </summary>
    /// <param name="wits">The character's Wits attribute.</param>
    /// <returns>The calculated action points.</returns>
    int CalculateActionPoints(int wits);

    /// <summary>
    /// Calculates the base max Aether Points for a character based on archetype and Will.
    /// Mystics: 10 + (Will * 5), Others: 0.
    /// </summary>
    /// <param name="archetype">The character's archetype.</param>
    /// <param name="will">The character's Will attribute.</param>
    /// <returns>The calculated base max AP (before corruption penalties).</returns>
    int CalculateBaseMaxAp(ArchetypeType archetype, int will);

    /// <summary>
    /// Recalculates and updates all derived stats on a character.
    /// Includes corruption penalties for Max AP and affected attributes.
    /// </summary>
    /// <param name="character">The character whose derived stats should be recalculated.</param>
    void RecalculateDerivedStats(Character character);

    /// <summary>
    /// Gets the attribute bonuses for a given archetype.
    /// </summary>
    /// <param name="archetype">The archetype to get bonuses for.</param>
    /// <returns>A dictionary mapping attributes to their bonus values.</returns>
    Dictionary<CharacterAttribute, int> GetArchetypeBonuses(ArchetypeType archetype);

    /// <summary>
    /// Gets the attribute bonuses for a given lineage.
    /// </summary>
    /// <param name="lineage">The lineage to get bonuses for.</param>
    /// <returns>A dictionary mapping attributes to their bonus values.</returns>
    Dictionary<CharacterAttribute, int> GetLineageBonuses(LineageType lineage);
}
