using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Persistence.Repositories;

/// <summary>
/// Repository implementation for Faction and CharacterFactionStanding persistence.
/// </summary>
/// <remarks>See: v0.4.2a (The Repute) for Faction System implementation.</remarks>
public class FactionRepository : IFactionRepository
{
    private readonly RuneAndRustDbContext _context;
    private readonly ILogger<FactionRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FactionRepository"/> class.
    /// </summary>
    public FactionRepository(
        RuneAndRustDbContext context,
        ILogger<FactionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Faction Queries
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<Faction?> GetFactionAsync(FactionType type)
    {
        _logger.LogDebug("[FactionRepo] GetFactionAsync: {Type}", type);
        return await _context.Factions.FirstOrDefaultAsync(f => f.Type == type);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Faction>> GetAllFactionsAsync()
    {
        _logger.LogDebug("[FactionRepo] GetAllFactionsAsync");
        return await _context.Factions.OrderBy(f => f.Type).ToListAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Standing Queries
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<CharacterFactionStanding?> GetStandingAsync(
        Guid characterId,
        FactionType faction)
    {
        _logger.LogDebug(
            "[FactionRepo] GetStandingAsync: CharId={CharId}, Faction={Faction}",
            characterId, faction);

        return await _context.CharacterFactionStandings
            .FirstOrDefaultAsync(s =>
                s.CharacterId == characterId &&
                s.FactionType == faction);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CharacterFactionStanding>> GetStandingsForCharacterAsync(
        Guid characterId)
    {
        _logger.LogDebug("[FactionRepo] GetStandingsForCharacterAsync: {CharId}", characterId);

        return await _context.CharacterFactionStandings
            .Where(s => s.CharacterId == characterId)
            .Include(s => s.Faction)
            .OrderBy(s => s.FactionType)
            .ToListAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Standing Mutations
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task AddStandingAsync(CharacterFactionStanding standing)
    {
        _logger.LogDebug(
            "[FactionRepo] AddStandingAsync: CharId={CharId}, Faction={Faction}, Rep={Rep}",
            standing.CharacterId, standing.FactionType, standing.Reputation);

        await _context.CharacterFactionStandings.AddAsync(standing);
    }

    /// <inheritdoc/>
    public Task UpdateStandingAsync(CharacterFactionStanding standing)
    {
        _logger.LogDebug(
            "[FactionRepo] UpdateStandingAsync: CharId={CharId}, Faction={Faction}, Rep={Rep}",
            standing.CharacterId, standing.FactionType, standing.Reputation);

        _context.CharacterFactionStandings.Update(standing);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync()
    {
        var changeCount = await _context.SaveChangesAsync();
        _logger.LogDebug("[FactionRepo] SaveChangesAsync: {Count} changes", changeCount);
    }
}
