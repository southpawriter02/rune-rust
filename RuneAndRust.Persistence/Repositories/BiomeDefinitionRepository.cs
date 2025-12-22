using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Persistence.Repositories;

/// <summary>
/// Repository implementation for BiomeDefinition entities with specialized query methods.
/// Part of the Dynamic Room Engine (v0.4.0).
/// </summary>
public class BiomeDefinitionRepository : GenericRepository<BiomeDefinition>, IBiomeDefinitionRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BiomeDefinitionRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public BiomeDefinitionRepository(RuneAndRustDbContext context, ILogger<GenericRepository<BiomeDefinition>> logger)
        : base(context, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<BiomeDefinition?> GetByBiomeIdAsync(string biomeId)
    {
        _logger.LogDebug("[BiomeDefinitionRepository] Fetching biome: {BiomeId}", biomeId);

        var biome = await _dbSet
            .FirstOrDefaultAsync(b => b.BiomeId == biomeId);

        if (biome == null)
        {
            _logger.LogDebug("[BiomeDefinitionRepository] Biome {BiomeId} not found", biomeId);
        }
        else
        {
            _logger.LogDebug("[BiomeDefinitionRepository] Successfully retrieved biome {BiomeId}", biomeId);
        }

        return biome;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BiomeElement>> GetElementsForBiomeAsync(string biomeId)
    {
        _logger.LogDebug("[BiomeDefinitionRepository] Fetching elements for biome: {BiomeId}", biomeId);

        var elements = await _context.BiomeElements
            .Where(e => e.BiomeId == biomeId)
            .ToListAsync();

        _logger.LogDebug("[BiomeDefinitionRepository] Found {Count} elements for biome {BiomeId}",
            elements.Count, biomeId);

        return elements;
    }

    /// <inheritdoc/>
    public async Task UpsertAsync(BiomeDefinition biomeDefinition)
    {
        _logger.LogDebug("[BiomeDefinitionRepository] Upserting biome: {BiomeId}", biomeDefinition.BiomeId);

        var existing = await GetByBiomeIdAsync(biomeDefinition.BiomeId);

        if (existing != null)
        {
            _logger.LogDebug("[BiomeDefinitionRepository] Biome {BiomeId} exists, updating", biomeDefinition.BiomeId);

            // Update all properties
            existing.Name = biomeDefinition.Name;
            existing.Description = biomeDefinition.Description;
            existing.AvailableTemplates = biomeDefinition.AvailableTemplates;
            existing.DescriptorCategories = biomeDefinition.DescriptorCategories;
            existing.MinRoomCount = biomeDefinition.MinRoomCount;
            existing.MaxRoomCount = biomeDefinition.MaxRoomCount;
            existing.BranchingProbability = biomeDefinition.BranchingProbability;
            existing.SecretRoomProbability = biomeDefinition.SecretRoomProbability;

            _dbSet.Update(existing);
        }
        else
        {
            _logger.LogDebug("[BiomeDefinitionRepository] Biome {BiomeId} is new, inserting", biomeDefinition.BiomeId);
            await _dbSet.AddAsync(biomeDefinition);
        }
    }
}
