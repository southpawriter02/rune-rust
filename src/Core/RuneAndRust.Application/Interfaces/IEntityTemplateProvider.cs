using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides entity templates for threat budget population.
/// </summary>
public interface IEntityTemplateProvider
{
    /// <summary>
    /// Gets all available entity templates.
    /// </summary>
    IReadOnlyList<EntityTemplate> GetAllTemplates();

    /// <summary>
    /// Gets templates filtered by biome.
    /// </summary>
    IReadOnlyList<EntityTemplate> GetTemplatesByBiome(Biome biome);

    /// <summary>
    /// Gets templates filtered by faction.
    /// </summary>
    IReadOnlyList<EntityTemplate> GetTemplatesByFaction(string factionId);
}
