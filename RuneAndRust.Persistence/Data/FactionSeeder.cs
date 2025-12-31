using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Persistence.Data;

/// <summary>
/// Seeds the database with canonical factions of Aethelgard.
/// </summary>
/// <remarks>
/// v0.4.2e introduces 4 canonical factions:
/// - Iron-Banes: Scavenger clans of the upper ruins
/// - Dvergr: Deep-dwelling master smiths
/// - The Bound: Glitch cultists (default hostile)
/// - The Faceless: Mysterious masked traders
///
/// All descriptions must be Domain 4 compliant (no precision measurements).
///
/// See: v0.4.2a (The Repute) for Faction System design.
/// </remarks>
public static class FactionSeeder
{
    /// <summary>
    /// Seeds all factions if none exist.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">Optional logger for seeding operations.</param>
    public static async Task SeedAsync(RuneAndRustDbContext context, ILogger? logger = null)
    {
        if (await context.Factions.AnyAsync())
        {
            logger?.LogDebug("[Seeder] Factions already exist, skipping seed");
            return;
        }

        logger?.LogInformation("[Seeder] Seeding Factions...");

        var factions = GetFactions();

        await context.Factions.AddRangeAsync(factions);
        await context.SaveChangesAsync();

        logger?.LogInformation("[Seeder] Seeded {Count} factions", factions.Count);
    }

    /// <summary>
    /// Gets the canonical faction definitions.
    /// </summary>
    private static List<Faction> GetFactions()
    {
        return new List<Faction>
        {
            new Faction
            {
                Type = FactionType.IronBanes,
                Name = "The Iron-Banes",
                Description = "Scavenger clans who make their homes in the upper ruins. " +
                              "Pragmatic survivors who trade in salvage, information, and the occasional favor. " +
                              "They know the safe paths through the rubble and the dangers that lurk in the deep.",
                DefaultReputation = 0,
                IconName = "faction_ironbanes",
                ColorHex = "#8B4513"
            },
            new Faction
            {
                Type = FactionType.Dvergr,
                Name = "The Dvergr",
                Description = "Deep-dwelling master smiths and artificers who guard the secrets of Pre-Glitch forging. " +
                              "Secretive and suspicious of outsiders, they trade only with those who prove their worth. " +
                              "Their craftsmanship is unmatched, their prices steep.",
                DefaultReputation = 0,
                IconName = "faction_dvergr",
                ColorHex = "#4A4A4A"
            },
            new Faction
            {
                Type = FactionType.TheBound,
                Name = "The Bound",
                Description = "Cultists who worship the Glitch as divine transformation rather than corruption. " +
                              "They seek to spread its 'blessing' to all flesh, viewing resistance as heresy. " +
                              "Dangerous zealots who see your uncorrupted form as an insult to their god.",
                DefaultReputation = -25, // Start hostile
                IconName = "faction_bound",
                ColorHex = "#8B0000"
            },
            new Faction
            {
                Type = FactionType.TheFaceless,
                Name = "The Faceless",
                Description = "Mysterious masked traders whose true allegiances remain unknown. " +
                              "They deal in rare artifacts and forbidden knowledge, appearing when least expected. " +
                              "Their prices are strange—sometimes coin, sometimes favors, sometimes memories.",
                DefaultReputation = 0,
                IconName = "faction_faceless",
                ColorHex = "#2F4F4F"
            }
        };
    }
}
