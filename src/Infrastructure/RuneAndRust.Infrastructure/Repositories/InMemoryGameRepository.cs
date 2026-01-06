using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of <see cref="IGameRepository"/> for development and testing.
/// </summary>
/// <remarks>
/// This repository stores game sessions in memory using a thread-safe concurrent dictionary.
/// Data is lost when the application stops. This implementation is suitable for development,
/// testing, and scenarios where persistence across sessions is not required.
/// </remarks>
public class InMemoryGameRepository : IGameRepository
{
    /// <summary>
    /// Thread-safe storage for game sessions, keyed by session ID.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, GameSession> _sessions = new();

    /// <summary>
    /// Logger for repository operations and diagnostics.
    /// </summary>
    private readonly ILogger<InMemoryGameRepository> _logger;

    /// <summary>
    /// Creates a new in-memory game repository instance.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics. If null, a no-op logger is used.</param>
    public InMemoryGameRepository(ILogger<InMemoryGameRepository>? logger = null)
    {
        _logger = logger ?? NullLogger<InMemoryGameRepository>.Instance;
        _logger.LogDebug("InMemoryGameRepository initialized");
    }

    /// <inheritdoc/>
    public Task<GameSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogDebug("GetByIdAsync called for session: {SessionId}", id);

        _sessions.TryGetValue(id, out var session);

        if (session != null)
        {
            _logger.LogDebug(
                "Session found - Player: {PlayerName}, Created: {CreatedAt}, LastPlayed: {LastPlayedAt}",
                session.Player.Name,
                session.CreatedAt,
                session.LastPlayedAt);
        }
        else
        {
            _logger.LogDebug("Session not found: {SessionId}", id);
        }

        return Task.FromResult(session);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<GameSessionSummary>> GetSavedGamesAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("GetSavedGamesAsync called. Total sessions in store: {SessionCount}", _sessions.Count);

        var summaries = _sessions.Values
            .OrderByDescending(s => s.LastPlayedAt)
            .Select(s => new GameSessionSummary(
                s.Id,
                s.Player.Name,
                s.CreatedAt,
                s.LastPlayedAt
            ))
            .ToList();

        _logger.LogDebug("Returning {SummaryCount} game session summaries", summaries.Count);

        return Task.FromResult<IReadOnlyList<GameSessionSummary>>(summaries);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown when session is null.</exception>
    public Task<Guid> SaveAsync(GameSession session, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        var isNew = !_sessions.ContainsKey(session.Id);
        _sessions[session.Id] = session;

        if (isNew)
        {
            _logger.LogInformation(
                "New session saved - Id: {SessionId}, Player: {PlayerName}. Total sessions: {TotalSessions}",
                session.Id,
                session.Player.Name,
                _sessions.Count);
        }
        else
        {
            _logger.LogDebug(
                "Session updated - Id: {SessionId}, Player: {PlayerName}, LastPlayed: {LastPlayedAt}",
                session.Id,
                session.Player.Name,
                session.LastPlayedAt);
        }

        return Task.FromResult(session.Id);
    }

    /// <inheritdoc/>
    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogDebug("DeleteAsync called for session: {SessionId}", id);

        if (_sessions.TryRemove(id, out var removed))
        {
            _logger.LogInformation(
                "Session deleted - Id: {SessionId}, Player: {PlayerName}. Remaining sessions: {TotalSessions}",
                id,
                removed.Player.Name,
                _sessions.Count);
        }
        else
        {
            _logger.LogWarning("Delete failed - Session not found: {SessionId}", id);
        }

        return Task.CompletedTask;
    }
}
