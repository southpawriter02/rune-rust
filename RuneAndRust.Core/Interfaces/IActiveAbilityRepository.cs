using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Defines the contract for Active Ability repository operations.
/// Extends IRepository with archetype-specific queries.
/// </summary>
public interface IActiveAbilityRepository : IRepository<ActiveAbility>
{
    /// <summary>
    /// Gets all abilities available to a specific character archetype.
    /// </summary>
    /// <param name="archetype">The archetype to filter by.</param>
    /// <param name="maxTier">The maximum tier to include (defaults to 1 for Tier 1 only).</param>
    /// <returns>A collection of abilities for that archetype up to the specified tier.</returns>
    Task<IEnumerable<ActiveAbility>> GetByArchetypeAsync(ArchetypeType archetype, int maxTier = 1);

    /// <summary>
    /// Gets an ability by its name using case-insensitive matching.
    /// </summary>
    /// <param name="name">The ability name to search for.</param>
    /// <returns>The ability if found, null otherwise.</returns>
    Task<ActiveAbility?> GetByNameAsync(string name);

    /// <summary>
    /// Adds multiple abilities in a single operation.
    /// Used for batch seeding of ability data.
    /// </summary>
    /// <param name="abilities">The abilities to add.</param>
    Task AddRangeAsync(IEnumerable<ActiveAbility> abilities);

    /// <summary>
    /// Checks if an ability with the given name already exists.
    /// Used for idempotent seeding operations.
    /// </summary>
    /// <param name="name">The ability name to check.</param>
    /// <returns>True if an ability with that name exists, false otherwise.</returns>
    Task<bool> ExistsByNameAsync(string name);
}
