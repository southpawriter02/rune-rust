using RuneAndRust.Core.Entities;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Repository interface for Character entity operations.
/// Extends the generic repository with character-specific queries.
/// </summary>
/// <remarks>See: SPEC-REPO-001 for Repository Pattern design.</remarks>
public interface ICharacterRepository : IRepository<Character>
{
    /// <summary>
    /// Gets a character by their name.
    /// </summary>
    /// <param name="name">The character name to search for.</param>
    /// <returns>The character with that name, or null if not found.</returns>
    Task<Character?> GetByNameAsync(string name);

    /// <summary>
    /// Checks if a character with the specified name already exists.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>True if a character with that name exists.</returns>
    Task<bool> NameExistsAsync(string name);

    /// <summary>
    /// Gets all characters ordered by creation date (newest first).
    /// </summary>
    /// <returns>All characters ordered by creation date descending.</returns>
    Task<IEnumerable<Character>> GetAllOrderedByCreationAsync();

    /// <summary>
    /// Gets the most recently modified character.
    /// </summary>
    /// <returns>The most recently modified character, or null if none exist.</returns>
    Task<Character?> GetMostRecentAsync();
}
