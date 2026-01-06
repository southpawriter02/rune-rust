using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// A lightweight summary of a saved game session for display in lists.
/// </summary>
/// <param name="Id">The unique identifier of the game session.</param>
/// <param name="PlayerName">The name of the player character.</param>
/// <param name="CreatedAt">The UTC timestamp when the session was created.</param>
/// <param name="LastPlayedAt">The UTC timestamp when the session was last played.</param>
public record GameSessionSummary(
    Guid Id,
    string PlayerName,
    DateTime CreatedAt,
    DateTime LastPlayedAt
);

/// <summary>
/// Defines the contract for game session persistence operations.
/// </summary>
/// <remarks>
/// Implementations of this interface handle storing and retrieving game sessions.
/// This abstraction allows for different storage backends (in-memory, database, file system).
/// </remarks>
public interface IGameRepository
{
    /// <summary>
    /// Retrieves a game session by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the session to retrieve.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>The game session if found; otherwise, <c>null</c>.</returns>
    Task<GameSession?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves summaries of all saved game sessions.
    /// </summary>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>A read-only list of game session summaries.</returns>
    Task<IReadOnlyList<GameSessionSummary>> GetSavedGamesAsync(CancellationToken ct = default);

    /// <summary>
    /// Saves or updates a game session.
    /// </summary>
    /// <param name="session">The game session to save.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>The unique identifier of the saved session.</returns>
    Task<Guid> SaveAsync(GameSession session, CancellationToken ct = default);

    /// <summary>
    /// Deletes a game session by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the session to delete.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
