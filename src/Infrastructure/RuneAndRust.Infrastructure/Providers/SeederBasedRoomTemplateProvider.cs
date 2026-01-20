using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Infrastructure.Persistence.Seeders;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// Provides room templates from seeder data.
/// </summary>
public class SeederBasedRoomTemplateProvider : IRoomTemplateProvider
{
    private readonly Lazy<IReadOnlyList<RoomTemplate>> _templates;

    public SeederBasedRoomTemplateProvider()
    {
        _templates = new Lazy<IReadOnlyList<RoomTemplate>>(
            () => RoomTemplateSeeder.GetAllTemplates().ToList());
    }

    public IReadOnlyList<RoomTemplate> GetAllTemplates() => _templates.Value;

    public IReadOnlyList<RoomTemplate> GetTemplatesByBiome(Biome biome) =>
        _templates.Value.Where(t => t.Biome == biome).ToList();
}
