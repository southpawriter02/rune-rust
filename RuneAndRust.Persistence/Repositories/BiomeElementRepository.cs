using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Persistence.Repositories;

/// <summary>
/// Repository implementation for BiomeElement entities with specialized query methods.
/// Part of the Dynamic Room Engine (v0.4.0).
/// </summary>
public class BiomeElementRepository : GenericRepository<BiomeElement>, IBiomeElementRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BiomeElementRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public BiomeElementRepository(RuneAndRustDbContext context, ILogger<GenericRepository<BiomeElement>> logger)
        : base(context, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BiomeElement>> GetByBiomeIdAsync(string biomeId)
    {
        _logger.LogDebug("[BiomeElementRepository] Fetching elements for biome: {BiomeId}", biomeId);

        var elements = await _dbSet
            .Where(e => e.BiomeId == biomeId)
            .ToListAsync();

        _logger.LogDebug("[BiomeElementRepository] Found {Count} elements for biome {BiomeId}",
            elements.Count, biomeId);

        return elements;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BiomeElement>> GetByElementTypeAsync(string elementType)
    {
        _logger.LogDebug("[BiomeElementRepository] Fetching elements of type: {ElementType}", elementType);

        var elements = await _dbSet
            .Where(e => e.ElementType == elementType)
            .ToListAsync();

        _logger.LogDebug("[BiomeElementRepository] Found {Count} elements of type {ElementType}",
            elements.Count, elementType);

        return elements;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BiomeElement>> GetByBiomeAndTypeAsync(string biomeId, string elementType)
    {
        _logger.LogDebug("[BiomeElementRepository] Fetching elements for biome {BiomeId} of type {ElementType}",
            biomeId, elementType);

        var elements = await _dbSet
            .Where(e => e.BiomeId == biomeId && e.ElementType == elementType)
            .ToListAsync();

        _logger.LogDebug("[BiomeElementRepository] Found {Count} elements for biome {BiomeId} of type {ElementType}",
            elements.Count, biomeId, elementType);

        return elements;
    }
}
