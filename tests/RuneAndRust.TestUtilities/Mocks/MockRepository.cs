using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.TestUtilities.Mocks;

/// <summary>
/// In-memory repository for tests.
/// </summary>
/// <remarks>
/// Implements IGameRepository with dictionary-based storage.
/// Use for service tests that require persistence without database access.
/// </remarks>
public class MockRepository : IGameRepository
{
    private readonly Dictionary<Guid, GameSession> _sessions = new();

    /// <summary>
    /// Pre-populates the repository with a session.
    /// </summary>
    public MockRepository WithSession(GameSession session)
    {
        _sessions[session.Id] = session;
        return this;
    }

    /// <summary>
    /// Gets all stored sessions (for test verification).
    /// </summary>
    public IReadOnlyDictionary<Guid, GameSession> Sessions => _sessions;

    /// <summary>
    /// Gets the number of stored sessions.
    /// </summary>
    public int Count => _sessions.Count;

    /// <summary>
    /// Clears all stored sessions.
    /// </summary>
    public void Clear() => _sessions.Clear();

    // ===== IGameRepository Implementation =====

    public Task<GameSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _sessions.TryGetValue(id, out var session);
        return Task.FromResult(session);
    }

    public Task<IReadOnlyList<GameSessionSummary>> GetSavedGamesAsync(CancellationToken ct = default)
    {
        var summaries = _sessions.Values
            .Select(s => new GameSessionSummary(
                s.Id,
                s.Player.Name,
                s.CreatedAt,
                s.LastPlayedAt))
            .OrderByDescending(s => s.LastPlayedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<GameSessionSummary>>(summaries);
    }

    public Task<Guid> SaveAsync(GameSession session, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        _sessions[session.Id] = session;
        return Task.FromResult(session.Id);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _sessions.Remove(id);
        return Task.CompletedTask;
    }
}
