using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

public record GameSessionSummary(
    Guid Id,
    string PlayerName,
    DateTime CreatedAt,
    DateTime LastPlayedAt
);

public interface IGameRepository
{
    Task<GameSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<GameSessionSummary>> GetSavedGamesAsync(CancellationToken ct = default);
    Task<Guid> SaveAsync(GameSession session, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
