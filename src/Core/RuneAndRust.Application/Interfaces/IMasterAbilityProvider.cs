namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;

/// <summary>
/// Provides access to master ability definitions loaded from configuration.
/// </summary>
/// <remarks>
/// <para>
/// The provider loads master ability definitions from <c>master-abilities.json</c>
/// and caches them for efficient lookup. Abilities are keyed by their ID and
/// indexed by skill ID for fast retrieval.
/// </para>
/// <para>
/// This interface is implemented by MasterAbilityProvider in the Infrastructure layer.
/// </para>
/// </remarks>
public interface IMasterAbilityProvider
{
    /// <summary>
    /// Gets all defined master abilities.
    /// </summary>
    /// <returns>A read-only collection of all master abilities.</returns>
    IReadOnlyList<MasterAbility> GetAllAbilities();

    /// <summary>
    /// Gets a master ability by its unique identifier.
    /// </summary>
    /// <param name="abilityId">The ability ID (e.g., "spider-climb").</param>
    /// <returns>The ability if found; otherwise <c>null</c>.</returns>
    MasterAbility? GetAbilityById(string abilityId);

    /// <summary>
    /// Gets all master abilities for a specific skill.
    /// </summary>
    /// <param name="skillId">The skill ID (e.g., "acrobatics").</param>
    /// <returns>
    /// A collection of all master abilities associated with the skill.
    /// Returns an empty collection if no abilities exist for the skill.
    /// </returns>
    IReadOnlyList<MasterAbility> GetAbilitiesForSkill(string skillId);

    /// <summary>
    /// Gets master abilities for a skill filtered by subtype.
    /// </summary>
    /// <param name="skillId">The skill ID (e.g., "acrobatics").</param>
    /// <param name="subType">The check subtype (e.g., "climbing", "stealth").</param>
    /// <returns>
    /// Abilities that apply to the given skill and subtype.
    /// Includes abilities with no subtype restrictions.
    /// </returns>
    IReadOnlyList<MasterAbility> GetAbilitiesForSkillAndSubType(string skillId, string? subType);

    /// <summary>
    /// Checks if a skill has any master abilities defined.
    /// </summary>
    /// <param name="skillId">The skill ID to check.</param>
    /// <returns><c>true</c> if the skill has master abilities; otherwise <c>false</c>.</returns>
    bool HasAbilitiesForSkill(string skillId);
}
