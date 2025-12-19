using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Persistence.Repositories;

/// <summary>
/// Repository implementation for Data Capture operations.
/// Provides CRUD and specialized queries for Scavenger's Journal fragments.
/// </summary>
public class DataCaptureRepository : GenericRepository<DataCapture>, IDataCaptureRepository
{
    private readonly ILogger<DataCaptureRepository> _captureLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataCaptureRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="genericLogger">The generic repository logger.</param>
    /// <param name="captureLogger">The capture-specific logger.</param>
    public DataCaptureRepository(
        RuneAndRustDbContext context,
        ILogger<GenericRepository<DataCapture>> genericLogger,
        ILogger<DataCaptureRepository> captureLogger)
        : base(context, genericLogger)
    {
        _captureLogger = captureLogger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DataCapture>> GetByCharacterIdAsync(Guid characterId)
    {
        _captureLogger.LogDebug("Fetching DataCaptures for Character {CharacterId}", characterId);

        var captures = await _dbSet
            .Include(d => d.CodexEntry)
            .Where(d => d.CharacterId == characterId)
            .OrderByDescending(d => d.DiscoveredAt)
            .ToListAsync();

        _captureLogger.LogDebug("Retrieved {Count} DataCaptures for Character {CharacterId}",
            captures.Count, characterId);

        return captures;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DataCapture>> GetByEntryIdAsync(Guid entryId, Guid characterId)
    {
        _captureLogger.LogDebug("Fetching DataCaptures for Entry {EntryId} and Character {CharacterId}",
            entryId, characterId);

        var captures = await _dbSet
            .Where(d => d.CodexEntryId == entryId && d.CharacterId == characterId)
            .OrderBy(d => d.DiscoveredAt)
            .ToListAsync();

        _captureLogger.LogDebug("Retrieved {Count} DataCaptures for Entry {EntryId} and Character {CharacterId}",
            captures.Count, entryId, characterId);

        return captures;
    }

    /// <inheritdoc/>
    public async Task<int> GetFragmentCountAsync(Guid entryId, Guid characterId)
    {
        _captureLogger.LogDebug("Counting fragments for Entry {EntryId} and Character {CharacterId}",
            entryId, characterId);

        var count = await _dbSet
            .CountAsync(d => d.CodexEntryId == entryId && d.CharacterId == characterId);

        _captureLogger.LogDebug("Character {CharacterId} has {Count} fragments for entry {EntryId}",
            characterId, count, entryId);

        return count;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DataCapture>> GetUnassignedAsync(Guid characterId)
    {
        _captureLogger.LogDebug("Fetching unassigned DataCaptures for Character {CharacterId}", characterId);

        var captures = await _dbSet
            .Where(d => d.CharacterId == characterId && d.CodexEntryId == null)
            .OrderByDescending(d => d.DiscoveredAt)
            .ToListAsync();

        _captureLogger.LogDebug("Retrieved {Count} unassigned DataCaptures for Character {CharacterId}",
            captures.Count, characterId);

        return captures;
    }

    /// <inheritdoc/>
    public async Task AddRangeAsync(IEnumerable<DataCapture> captures)
    {
        var captureList = captures.ToList();
        _captureLogger.LogDebug("Adding {Count} DataCaptures", captureList.Count);

        await _dbSet.AddRangeAsync(captureList);

        _captureLogger.LogDebug("Successfully added {Count} DataCaptures to context", captureList.Count);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Guid>> GetDiscoveredEntryIdsAsync(Guid characterId)
    {
        _captureLogger.LogDebug("Fetching discovered entry IDs for Character {CharacterId}", characterId);

        var entryIds = await _dbSet
            .Where(d => d.CharacterId == characterId && d.CodexEntryId != null)
            .Select(d => d.CodexEntryId!.Value)
            .Distinct()
            .ToListAsync();

        _captureLogger.LogDebug("Found {Count} discovered entries for Character {CharacterId}",
            entryIds.Count, characterId);

        return entryIds;
    }
}
