using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Infrastructure.Persistence.Seeders;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// Provides entity templates from seeder data.
/// </summary>
public class SeederBasedEntityTemplateProvider : IEntityTemplateProvider
{
    private readonly Lazy<IReadOnlyList<EntityTemplate>> _templates;

    public SeederBasedEntityTemplateProvider()
    {
        _templates = new Lazy<IReadOnlyList<EntityTemplate>>(
            () => EntityTableSeeder.GetAllTemplates().ToList());
    }

    public IReadOnlyList<EntityTemplate> GetAllTemplates() => _templates.Value;

    public IReadOnlyList<EntityTemplate> GetTemplatesByBiome(Biome biome) =>
        _templates.Value.Where(t => t.Biome == biome).ToList();

    public IReadOnlyList<EntityTemplate> GetTemplatesByFaction(string factionId) =>
        _templates.Value.Where(t => t.BelongsToFaction(factionId)).ToList();
}
