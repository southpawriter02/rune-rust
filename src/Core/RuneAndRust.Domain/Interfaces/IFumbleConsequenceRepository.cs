namespace RuneAndRust.Domain.Interfaces;

using RuneAndRust.Domain.Entities;

/// <summary>
/// Repository interface for managing fumble consequence persistence.
/// </summary>
public interface IFumbleConsequenceRepository
{
    /// <summary>
    /// Adds a new fumble consequence to the repository.
    /// </summary>
    /// <param name="consequence">The consequence to add.</param>
    void Add(FumbleConsequence consequence);

    /// <summary>
    /// Gets a fumble consequence by its ID.
    /// </summary>
    /// <param name="consequenceId">The consequence ID.</param>
    /// <returns>The fumble consequence, or null if not found.</returns>
    FumbleConsequence? GetById(string consequenceId);

    /// <summary>
    /// Gets all active fumble consequences for a character.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>List of active fumble consequences.</returns>
    IReadOnlyList<FumbleConsequence> GetActiveByCharacter(string characterId);

    /// <summary>
    /// Gets all fumble consequences for a character (including inactive).
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>List of all fumble consequences.</returns>
    IReadOnlyList<FumbleConsequence> GetAllByCharacter(string characterId);

    /// <summary>
    /// Updates an existing fumble consequence.
    /// </summary>
    /// <param name="consequence">The consequence to update.</param>
    void Update(FumbleConsequence consequence);

    /// <summary>
    /// Removes a fumble consequence from the repository.
    /// </summary>
    /// <param name="consequenceId">The consequence ID to remove.</param>
    void Remove(string consequenceId);

    /// <summary>
    /// Gets all consequences that have expired as of the specified time.
    /// </summary>
    /// <param name="asOfTime">The time to check against.</param>
    /// <returns>List of expired fumble consequences.</returns>
    IReadOnlyList<FumbleConsequence> GetExpired(DateTime asOfTime);

    /// <summary>
    /// Gets all active consequences in the system.
    /// </summary>
    /// <returns>List of all active fumble consequences.</returns>
    IReadOnlyList<FumbleConsequence> GetAllActive();

    /// <summary>
    /// Gets consequences affecting a specific target.
    /// </summary>
    /// <param name="targetId">The target ID.</param>
    /// <returns>List of fumble consequences affecting the target.</returns>
    IReadOnlyList<FumbleConsequence> GetByTarget(string targetId);
}
