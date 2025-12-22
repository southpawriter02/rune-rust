using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Persistence.Data;

namespace RuneAndRust.Persistence.Repositories;

/// <summary>
/// Repository implementation for RoomTemplate entities with specialized query methods.
/// Part of the Dynamic Room Engine (v0.4.0).
/// </summary>
public class RoomTemplateRepository : GenericRepository<RoomTemplate>, IRoomTemplateRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoomTemplateRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public RoomTemplateRepository(RuneAndRustDbContext context, ILogger<GenericRepository<RoomTemplate>> logger)
        : base(context, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RoomTemplate>> GetByBiomeIdAsync(string biomeId)
    {
        _logger.LogDebug("[RoomTemplateRepository] Fetching templates for biome: {BiomeId}", biomeId);

        var templates = await _dbSet
            .Where(t => t.BiomeId == biomeId)
            .ToListAsync();

        _logger.LogDebug("[RoomTemplateRepository] Found {Count} templates for biome {BiomeId}",
            templates.Count, biomeId);

        return templates;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RoomTemplate>> GetByArchetypeAsync(string archetype)
    {
        _logger.LogDebug("[RoomTemplateRepository] Fetching templates for archetype: {Archetype}", archetype);

        var templates = await _dbSet
            .Where(t => t.Archetype == archetype)
            .ToListAsync();

        _logger.LogDebug("[RoomTemplateRepository] Found {Count} templates for archetype {Archetype}",
            templates.Count, archetype);

        return templates;
    }

    /// <inheritdoc/>
    public async Task<RoomTemplate?> GetByTemplateIdAsync(string templateId)
    {
        _logger.LogDebug("[RoomTemplateRepository] Fetching template by TemplateId: {TemplateId}", templateId);

        var template = await _dbSet
            .FirstOrDefaultAsync(t => t.TemplateId == templateId);

        if (template == null)
        {
            _logger.LogDebug("[RoomTemplateRepository] Template {TemplateId} not found", templateId);
        }
        else
        {
            _logger.LogDebug("[RoomTemplateRepository] Successfully retrieved template {TemplateId}", templateId);
        }

        return template;
    }

    /// <inheritdoc/>
    public async Task UpsertAsync(RoomTemplate template)
    {
        _logger.LogDebug("[RoomTemplateRepository] Upserting template: {TemplateId}", template.TemplateId);

        var existing = await GetByTemplateIdAsync(template.TemplateId);

        if (existing != null)
        {
            _logger.LogDebug("[RoomTemplateRepository] Template {TemplateId} exists, updating", template.TemplateId);

            // Update all properties
            existing.BiomeId = template.BiomeId;
            existing.Size = template.Size;
            existing.Archetype = template.Archetype;
            existing.NameTemplates = template.NameTemplates;
            existing.Adjectives = template.Adjectives;
            existing.DescriptionTemplates = template.DescriptionTemplates;
            existing.Details = template.Details;
            existing.ValidConnections = template.ValidConnections;
            existing.Tags = template.Tags;
            existing.MinConnectionPoints = template.MinConnectionPoints;
            existing.MaxConnectionPoints = template.MaxConnectionPoints;
            existing.Difficulty = template.Difficulty;

            _dbSet.Update(existing);
        }
        else
        {
            _logger.LogDebug("[RoomTemplateRepository] Template {TemplateId} is new, inserting", template.TemplateId);
            await _dbSet.AddAsync(template);
        }
    }
}
