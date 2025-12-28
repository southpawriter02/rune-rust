using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Persistence.Repositories;

/// <summary>
/// Repository implementation for Specialization and SpecializationNode persistence.
/// </summary>
/// <remarks>
/// See: SPEC-SPECIALIZATION-001 for design documentation.
/// See: v0.4.1a for implementation details.
/// </remarks>
public class SpecializationRepository : ISpecializationRepository
{
    private readonly RuneAndRustDbContext _context;
    private readonly ILogger<SpecializationRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecializationRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public SpecializationRepository(
        RuneAndRustDbContext context,
        ILogger<SpecializationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Specialization Queries
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<IEnumerable<Specialization>> GetAllAsync()
    {
        _logger.LogDebug("[SpecRepo] GetAllAsync called");

        return await _context.Specializations
            .Include(s => s.Nodes)
                .ThenInclude(n => n.Ability)
            .OrderBy(s => s.Type)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Specialization?> GetByIdAsync(Guid id)
    {
        _logger.LogDebug("[SpecRepo] GetByIdAsync: {Id}", id);

        return await _context.Specializations
            .Include(s => s.Nodes)
                .ThenInclude(n => n.Ability)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    /// <inheritdoc/>
    public async Task<Specialization?> GetByTypeAsync(SpecializationType type)
    {
        _logger.LogDebug("[SpecRepo] GetByTypeAsync: {Type}", type);

        return await _context.Specializations
            .Include(s => s.Nodes)
                .ThenInclude(n => n.Ability)
            .FirstOrDefaultAsync(s => s.Type == type);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Specialization>> GetByArchetypeAsync(ArchetypeType archetype)
    {
        _logger.LogDebug("[SpecRepo] GetByArchetypeAsync: {Archetype}", archetype);

        return await _context.Specializations
            .Include(s => s.Nodes)
                .ThenInclude(n => n.Ability)
            .Where(s => s.RequiredArchetype == archetype)
            .OrderBy(s => s.Type)
            .ToListAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Node Queries
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<SpecializationNode?> GetNodeByIdAsync(Guid nodeId)
    {
        _logger.LogDebug("[SpecRepo] GetNodeByIdAsync: {NodeId}", nodeId);

        return await _context.SpecializationNodes
            .Include(n => n.Ability)
            .Include(n => n.Specialization)
            .FirstOrDefaultAsync(n => n.Id == nodeId);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SpecializationNode>> GetNodesForSpecializationAsync(Guid specId)
    {
        _logger.LogDebug("[SpecRepo] GetNodesForSpecializationAsync: {SpecId}", specId);

        return await _context.SpecializationNodes
            .Include(n => n.Ability)
            .Where(n => n.SpecializationId == specId)
            .OrderBy(n => n.Tier)
            .ThenBy(n => n.PositionX)
            .ToListAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Character Progress Queries
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<IEnumerable<SpecializationNode>> GetUnlockedNodesAsync(Guid characterId)
    {
        _logger.LogDebug("[SpecRepo] GetUnlockedNodesAsync: {CharacterId}", characterId);

        return await _context.CharacterSpecializationProgress
            .Where(p => p.CharacterId == characterId)
            .Include(p => p.Node)
                .ThenInclude(n => n.Ability)
            .Include(p => p.Node)
                .ThenInclude(n => n.Specialization)
            .Select(p => p.Node)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task RecordNodeUnlockAsync(Guid characterId, Guid nodeId)
    {
        _logger.LogDebug("[SpecRepo] RecordNodeUnlockAsync: Char={CharId}, Node={NodeId}",
            characterId, nodeId);

        var progress = new CharacterSpecializationProgress
        {
            CharacterId = characterId,
            NodeId = nodeId,
            UnlockedAt = DateTime.UtcNow
        };

        await _context.CharacterSpecializationProgress.AddAsync(progress);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Persistence
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
