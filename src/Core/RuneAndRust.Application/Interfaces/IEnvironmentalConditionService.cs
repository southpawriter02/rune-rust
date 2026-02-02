using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Applies environmental effects to characters based on their location.
/// </summary>
/// <remarks>
/// <para>
/// IEnvironmentalConditionService handles the application of ambient
/// environmental hazards based on character location. It calculates
/// effective DCs, applies damage, and checks for mitigations.
/// </para>
/// <para>
/// Standard workflow:
/// <list type="number">
/// <item>GetActiveCondition - Determine what hazard applies</item>
/// <item>CalculateEffectiveDc - Apply zone modifiers</item>
/// <item>HasMitigation - Check for character protection</item>
/// <item>ApplyEnvironmentalDamage - Roll check and deal damage</item>
/// </list>
/// </para>
/// </remarks>
public interface IEnvironmentalConditionService
{
    /// <summary>
    /// Gets the active environmental condition for a realm zone.
    /// </summary>
    /// <param name="realm">The realm.</param>
    /// <param name="zoneId">The zone within the realm (null for base realm).</param>
    /// <returns>The active condition, or null if no hazard.</returns>
    EnvironmentalCondition? GetActiveCondition(RealmId realm, string? zoneId = null);

    /// <summary>
    /// Calculates the effective DC for a condition in a zone.
    /// </summary>
    /// <param name="condition">The environmental condition.</param>
    /// <param name="zone">The specific zone (null for base DC).</param>
    /// <returns>The effective DC after zone modifiers.</returns>
    int CalculateEffectiveDc(EnvironmentalCondition condition, RealmBiomeZone? zone);

    /// <summary>
    /// Applies environmental damage to a character.
    /// </summary>
    /// <param name="characterId">Target character.</param>
    /// <param name="condition">The environmental condition.</param>
    /// <param name="dcModifier">Additional DC modifier (default 0).</param>
    /// <returns>Result of the environmental damage application.</returns>
    EnvironmentalDamageResult ApplyEnvironmentalDamage(
        Guid characterId,
        EnvironmentalCondition condition,
        int dcModifier = 0);

    /// <summary>
    /// Checks if a character has mitigation for a condition.
    /// </summary>
    /// <param name="characterId">Character to check.</param>
    /// <param name="condition">The environmental condition.</param>
    /// <returns>True if character has full or partial mitigation.</returns>
    bool HasMitigation(Guid characterId, EnvironmentalCondition condition);
}
