using System.Collections.Concurrent;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Infrastructure.Repositories;

public class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<Guid, GameSession> _sessions = new();

    public Task<GameSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _sessions.TryGetValue(id, out var session);
        return Task.FromResult(session);
    }

    public Task<IReadOnlyList<GameSessionSummary>> GetSavedGamesAsync(CancellationToken ct = default)
    {
        var summaries = _sessions.Values
            .OrderByDescending(s => s.LastPlayedAt)
            .Select(s => new GameSessionSummary(
                s.Id,
                s.Player.Name,
                s.CreatedAt,
                s.LastPlayedAt
            ))
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
        _sessions.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
