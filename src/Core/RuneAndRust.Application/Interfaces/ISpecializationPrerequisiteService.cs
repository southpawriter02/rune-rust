// ═══════════════════════════════════════════════════════════════════════════════
// ISpecializationPrerequisiteService.cs
// Interface for validating specialization prerequisites during character
// migration and normal specialization selection.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Validates specialization prerequisites including archetype compatibility,
/// PP investment thresholds, and unlock costs.
/// </summary>
/// <remarks>
/// <para>
/// This service enforces the rules governing specialization selection:
/// </para>
/// <list type="bullet">
///   <item><description>Characters must have the correct parent archetype</description></item>
///   <item><description>Characters must meet minimum PP investment thresholds</description></item>
///   <item><description>Characters must pay the unlock cost (0 for first, 3 for additional)</description></item>
///   <item><description>Some specializations require other specializations as prerequisites</description></item>
/// </list>
/// <para>
/// During migration, the first specialization is always free (0 PP cost)
/// regardless of the normal unlock cost.
/// </para>
/// </remarks>
/// <seealso cref="SpecializationPrerequisite"/>
/// <seealso cref="SpecializationId"/>
public interface ISpecializationPrerequisiteService
{
    /// <summary>
    /// Retrieves the full prerequisites for a specialization.
    /// </summary>
    /// <param name="specialization">The specialization to query.</param>
    /// <returns>
    /// The <see cref="SpecializationPrerequisite"/> defining all requirements.
    /// </returns>
    SpecializationPrerequisite GetPrerequisites(SpecializationId specialization);

    /// <summary>
    /// Checks whether a character can select the specified specialization.
    /// </summary>
    /// <param name="characterId">The character to validate.</param>
    /// <param name="specialization">The specialization to check.</param>
    /// <returns>
    /// <c>true</c> if all prerequisites are met; <c>false</c> otherwise.
    /// </returns>
    bool CanSelectSpecialization(Guid characterId, SpecializationId specialization);

    /// <summary>
    /// Returns all specializations currently available for a character
    /// based on their archetype and PP investment.
    /// </summary>
    /// <param name="characterId">The character to query.</param>
    /// <returns>
    /// A read-only list of <see cref="SpecializationId"/> values the
    /// character is eligible to select.
    /// </returns>
    IReadOnlyList<SpecializationId> GetAvailableSpecializations(Guid characterId);

    /// <summary>
    /// Validates that a specialization belongs to the specified archetype.
    /// </summary>
    /// <param name="archetype">The archetype to validate against.</param>
    /// <param name="specialization">The specialization to check.</param>
    /// <returns>
    /// <c>true</c> if the specialization belongs to the archetype;
    /// <c>false</c> otherwise.
    /// </returns>
    bool ValidateArchetypeCompatibility(Archetype archetype, SpecializationId specialization);

    /// <summary>
    /// Calculates the PP cost to unlock a specialization for a character.
    /// Returns 0 if this is the character's first specialization.
    /// </summary>
    /// <param name="characterId">The character to calculate for.</param>
    /// <param name="specialization">The specialization to unlock.</param>
    /// <returns>
    /// The PP cost to unlock. 0 for first specialization, 3 for additional.
    /// </returns>
    int GetUnlockCost(Guid characterId, SpecializationId specialization);
}
