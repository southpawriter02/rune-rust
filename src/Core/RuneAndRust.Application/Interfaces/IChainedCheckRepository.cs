using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Repository interface for persisting chained check state.
/// </summary>
/// <remarks>
/// <para>
/// Provides storage and retrieval of <see cref="ChainedCheckState"/> entities
/// for in-progress chained checks.
/// </para>
/// </remarks>
public interface IChainedCheckRepository
{
    /// <summary>
    /// Adds a new chained check state to the repository.
    /// </summary>
    /// <param name="state">The state to add.</param>
    void Add(ChainedCheckState state);

    /// <summary>
    /// Updates an existing chained check state.
    /// </summary>
    /// <param name="state">The state to update.</param>
    void Update(ChainedCheckState state);

    /// <summary>
    /// Gets a chained check state by ID.
    /// </summary>
    /// <param name="checkId">The chain's unique identifier.</param>
    /// <returns>The state, or null if not found.</returns>
    ChainedCheckState? GetById(string checkId);

    /// <summary>
    /// Gets all active (non-terminal) chains for a character.
    /// </summary>
    /// <param name="characterId">The character's ID.</param>
    /// <returns>Collection of active chain states.</returns>
    IReadOnlyList<ChainedCheckState> GetActiveByCharacterId(string characterId);

    /// <summary>
    /// Removes a completed chain from the repository.
    /// </summary>
    /// <param name="checkId">The chain's unique identifier.</param>
    /// <returns>True if removed; false if not found.</returns>
    bool Remove(string checkId);

    /// <summary>
    /// Removes all chains for a character.
    /// </summary>
    /// <param name="characterId">The character's ID.</param>
    /// <returns>Number of chains removed.</returns>
    int RemoveAllForCharacter(string characterId);
}
