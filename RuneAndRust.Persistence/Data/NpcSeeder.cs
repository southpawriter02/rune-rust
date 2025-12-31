using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Persistence.Data;

/// <summary>
/// Seeds the database with sample NPCs for testing dialogue integration.
/// </summary>
/// <remarks>
/// v0.4.2e introduces sample NPCs linked to dialogue trees:
/// - Old Scavenger: Iron-Banes elder with trade/information dialogue
/// - Kjartan: Dvergr smith with crafting services
///
/// All NPC descriptions must be Domain 4 compliant (no precision measurements).
///
/// See: v0.4.2c (The Voice) for DialogueService implementation.
/// See: v0.4.2e (The Archive) for NPC seeding.
/// </remarks>
public static class NpcSeeder
{
    /// <summary>
    /// Seeds sample NPCs if none exist.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">Optional logger for seeding operations.</param>
    public static async Task SeedAsync(RuneAndRustDbContext context, ILogger? logger = null)
    {
        if (await context.Npcs.AnyAsync())
        {
            logger?.LogDebug("[Seeder] NPCs already exist, skipping seed");
            return;
        }

        logger?.LogInformation("[Seeder] Seeding NPCs...");

        var npcs = GetNpcs();

        await context.Npcs.AddRangeAsync(npcs);
        await context.SaveChangesAsync();

        logger?.LogInformation("[Seeder] Seeded {Count} NPCs", npcs.Count);
    }

    /// <summary>
    /// Gets the sample NPC definitions.
    /// </summary>
    private static List<Npc> GetNpcs()
    {
        return new List<Npc>
        {
            new Npc
            {
                Name = "Old Scavenger",
                Title = "Iron-Bane Elder",
                Description = "A weathered figure draped in patched leather and salvaged cloth. " +
                              "His eyes carry the weight of countless expeditions into the depths, " +
                              "and his calloused hands speak of years spent prying treasures from the ruins. " +
                              "Despite his gruff demeanor, there's a knowing warmth in his gaze for those who deal fairly.",
                DialogueTreeId = "npc_old_scavenger",
                Faction = FactionType.IronBanes,
                IsHostile = false
            },
            new Npc
            {
                Name = "Kjartan",
                Title = "Dvergr Smith",
                Description = "A stocky figure with arms like gnarled oak branches, " +
                              "perpetually dusted with soot and iron filings. " +
                              "His beard is braided in the old way, adorned with small metal tokens of craft. " +
                              "The heat of the forge has left his skin ruddy, and his eyes burn with the same intensity as his furnace.",
                DialogueTreeId = "npc_kjartan",
                Faction = FactionType.Dvergr,
                IsHostile = false
            },
            new Npc
            {
                Name = "Bound Acolyte",
                Title = "Whisperer of the Glitch",
                Description = "A gaunt figure swathed in tattered robes that seem to writhe at the edges. " +
                              "Faint luminescence pulses beneath the fabric, tracing patterns that hurt to follow. " +
                              "Where their face should be, a mask of corroded metal reflects nothing. " +
                              "They do not speak so much as resonate, their voice carrying harmonics that twist the air.",
                DialogueTreeId = null, // Hostile, no dialogue
                Faction = FactionType.TheBound,
                IsHostile = true
            },
            new Npc
            {
                Name = "Masked Trader",
                Title = "The Faceless",
                Description = "A figure of indeterminate features, their face obscured by an ancient mask of pale ceramic. " +
                              "They move with unsettling grace, their robes revealing nothing of the form beneath. " +
                              "When they speak, the voice seems to come from everywhere and nowhere. " +
                              "Their wares are displayed on a cloth that shimmers faintly in any light.",
                DialogueTreeId = "npc_faceless_trader", // Placeholder for future dialogue
                Faction = FactionType.TheFaceless,
                IsHostile = false
            },
            new Npc
            {
                Name = "Scrap-Guard",
                Title = "Iron-Bane Watchman",
                Description = "A wary sentinel clad in mismatched armor pieced together from salvage. " +
                              "A heavy cudgel rests against their shoulder, its head wrapped in rusted wire. " +
                              "They scan the perimeter with eyes accustomed to watching for threats from below.",
                DialogueTreeId = null, // Guard, no dialogue
                Faction = FactionType.IronBanes,
                IsHostile = false
            }
        };
    }
}
