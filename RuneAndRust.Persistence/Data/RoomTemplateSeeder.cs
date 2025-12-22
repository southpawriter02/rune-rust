using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Persistence.Data;

/// <summary>
/// Seeds the database with room templates and biome definitions from JSON files (v0.4.0).
/// Executes on first application launch if the RoomTemplates table is empty.
/// </summary>
public static class RoomTemplateSeeder
{
    /// <summary>
    /// Seeds room templates and biome definitions if none exist.
    /// Uses TemplateLoaderService to load from data/templates and data/biomes directories.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="templateLoader">The template loader service.</param>
    /// <param name="logger">Optional logger for seeding operations.</param>
    public static async Task SeedAsync(
        RuneAndRustDbContext context,
        ITemplateLoaderService templateLoader,
        ILogger? logger = null)
    {
        if (await context.RoomTemplates.AnyAsync())
        {
            logger?.LogDebug("[RoomTemplateSeeder] Room templates already exist, skipping seed");
            return;
        }

        logger?.LogInformation("[RoomTemplateSeeder] Seeding room templates and biome definitions...");

        try
        {
            // TemplateLoaderService handles all JSON loading, upsert logic, and validation
            await templateLoader.LoadAllTemplatesAsync();

            logger?.LogInformation("[RoomTemplateSeeder] Room template seeding complete");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[RoomTemplateSeeder] Failed to seed room templates");
            throw;
        }
    }
}
