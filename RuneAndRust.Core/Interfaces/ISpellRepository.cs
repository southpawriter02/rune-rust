using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for spell repository operations.
/// Extends IRepository with spell-specific query methods.
/// </summary>
/// <remarks>
/// See: v0.4.3b (The Grimoire) for implementation details.
/// </remarks>
public interface ISpellRepository : IRepository<Spell>
{
    /// <summary>
    /// Gets all spells belonging to a specific school.
    /// </summary>
    /// <param name="school">The spell school to filter by.</param>
    /// <returns>A collection of spells in that school.</returns>
    Task<IEnumerable<Spell>> GetBySchoolAsync(SpellSchool school);

    /// <summary>
    /// Gets all spells with a specific target type.
    /// </summary>
    /// <param name="targetType">The target type to filter by.</param>
    /// <returns>A collection of spells with that target type.</returns>
    Task<IEnumerable<Spell>> GetByTargetTypeAsync(SpellTargetType targetType);

    /// <summary>
    /// Gets all spells at or below a specific tier.
    /// </summary>
    /// <param name="maxTier">The maximum tier (inclusive).</param>
    /// <returns>A collection of spells up to that tier.</returns>
    Task<IEnumerable<Spell>> GetByMaxTierAsync(int maxTier);

    /// <summary>
    /// Gets all spells available to a specific archetype.
    /// Includes spells with no archetype restriction and spells
    /// specifically restricted to the given archetype.
    /// </summary>
    /// <param name="archetype">The archetype to filter by.</param>
    /// <returns>A collection of spells available to that archetype.</returns>
    Task<IEnumerable<Spell>> GetByArchetypeAsync(ArchetypeType archetype);

    /// <summary>
    /// Gets a spell by name using case-insensitive matching.
    /// </summary>
    /// <param name="name">The spell name to search for.</param>
    /// <returns>The spell if found; otherwise, null.</returns>
    Task<Spell?> GetByNameAsync(string name);

    /// <summary>
    /// Gets all spells that require charging (ChargeTurns > 0).
    /// </summary>
    /// <returns>A collection of charged spells.</returns>
    Task<IEnumerable<Spell>> GetChargedSpellsAsync();

    /// <summary>
    /// Gets all spells that can be cast instantly (ChargeTurns == 0).
    /// </summary>
    /// <returns>A collection of instant cast spells.</returns>
    Task<IEnumerable<Spell>> GetInstantSpellsAsync();

    /// <summary>
    /// Gets all spells that require concentration.
    /// </summary>
    /// <returns>A collection of concentration spells.</returns>
    Task<IEnumerable<Spell>> GetConcentrationSpellsAsync();

    /// <summary>
    /// Gets all spells within a specified range category.
    /// </summary>
    /// <param name="range">The spell range to filter by.</param>
    /// <returns>A collection of spells with that range.</returns>
    Task<IEnumerable<Spell>> GetByRangeAsync(SpellRange range);

    /// <summary>
    /// Adds multiple spells in a single operation.
    /// </summary>
    /// <param name="spells">The spells to add.</param>
    Task AddRangeAsync(IEnumerable<Spell> spells);
}
