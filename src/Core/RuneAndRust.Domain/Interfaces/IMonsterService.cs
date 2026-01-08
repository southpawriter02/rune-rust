using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Domain.Interfaces;

/// <summary>
/// Service for spawning and managing monsters.
/// </summary>
public interface IMonsterService
{
    /// <summary>
    /// Spawns a monster from the specified definition ID.
    /// </summary>
    /// <param name="definitionId">The monster definition ID.</param>
    /// <returns>A new Monster instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the definition ID is not found.</exception>
    Monster SpawnMonster(string definitionId);

    /// <summary>
    /// Spawns a random monster using weighted random selection.
    /// </summary>
    /// <returns>A new Monster instance.</returns>
    Monster SpawnRandomMonster();

    /// <summary>
    /// Spawns a random monster that has all of the specified tags.
    /// </summary>
    /// <param name="requiredTags">Tags the monster must have.</param>
    /// <returns>A new Monster instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no monsters match the tags.</exception>
    Monster SpawnRandomMonster(IEnumerable<string> requiredTags);

    /// <summary>
    /// Gets all available monster definitions.
    /// </summary>
    /// <returns>A read-only list of all monster definitions.</returns>
    IReadOnlyList<MonsterDefinition> GetAllDefinitions();

    /// <summary>
    /// Gets a monster definition by ID.
    /// </summary>
    /// <param name="id">The definition ID.</param>
    /// <returns>The monster definition, or null if not found.</returns>
    MonsterDefinition? GetDefinition(string id);
}
