using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides room templates for dungeon generation.
/// </summary>
public interface IRoomTemplateProvider
{
    /// <summary>
    /// Gets all available room templates.
    /// </summary>
    IReadOnlyList<RoomTemplate> GetAllTemplates();

    /// <summary>
    /// Gets templates filtered by biome.
    /// </summary>
    IReadOnlyList<RoomTemplate> GetTemplatesByBiome(Biome biome);
}
