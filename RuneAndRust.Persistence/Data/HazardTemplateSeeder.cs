using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Persistence.Data;

/// <summary>
/// Seeds the database with hazard template definitions (v0.3.3c).
/// Each template has biome tags to control where hazards can spawn.
/// </summary>
/// <remarks>
/// Templates define the properties of hazards; DynamicHazard instances are
/// created from templates by EnvironmentPopulator during dungeon generation.
/// All descriptions must be Domain 4 compliant (no precision measurements).
/// </remarks>
public static class HazardTemplateSeeder
{
    /// <summary>
    /// Seeds hazard templates if none exist.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">Optional logger for seeding operations.</param>
    public static async Task SeedAsync(RuneAndRustDbContext context, ILogger? logger = null)
    {
        if (await context.HazardTemplates.AnyAsync())
        {
            logger?.LogDebug("Hazard templates already exist, skipping seed");
            return;
        }

        logger?.LogInformation("Seeding hazard templates...");

        var templates = GetTemplates();
        await context.HazardTemplates.AddRangeAsync(templates);
        await context.SaveChangesAsync();

        logger?.LogInformation("Seeded {Count} hazard templates", templates.Count);
    }

    /// <summary>
    /// Gets all hazard template definitions for seeding.
    /// </summary>
    /// <returns>A list of HazardTemplate entities to seed.</returns>
    public static List<HazardTemplate> GetTemplates()
    {
        return new List<HazardTemplate>
        {
            // ==========================================
            // RUIN BIOME HAZARDS
            // ==========================================
            new HazardTemplate
            {
                Name = "Pressure Plate",
                Description = "A concealed plate clicks underfoot. Ancient mechanisms grind to life.",
                HazardType = HazardType.Mechanical,
                Trigger = TriggerType.Movement,
                EffectScript = "DAMAGE:Physical:1d6",
                MaxCooldown = 3,
                BiomeTags = new List<BiomeType> { BiomeType.Ruin }
            },
            new HazardTemplate
            {
                Name = "Collapsing Floor",
                Description = "The floor groans ominously before giving way to the void below.",
                HazardType = HazardType.Environmental,
                Trigger = TriggerType.Movement,
                EffectScript = "DAMAGE:Physical:2d6",
                OneTimeUse = true,
                BiomeTags = new List<BiomeType> { BiomeType.Ruin }
            },
            new HazardTemplate
            {
                Name = "Dart Trap",
                Description = "Corroded vents in the wall whisper of forgotten defenses.",
                HazardType = HazardType.Mechanical,
                Trigger = TriggerType.Movement,
                EffectScript = "DAMAGE:Physical:1d4;STATUS:Poisoned:2",
                MaxCooldown = 2,
                BiomeTags = new List<BiomeType> { BiomeType.Ruin }
            },

            // ==========================================
            // INDUSTRIAL BIOME HAZARDS
            // ==========================================
            new HazardTemplate
            {
                Name = "Steam Vent",
                Description = "Superheated vapor bursts from a cracked pipe, scalding the air.",
                HazardType = HazardType.Environmental,
                Trigger = TriggerType.TurnStart,
                EffectScript = "DAMAGE:Fire:1d4",
                MaxCooldown = 2,
                BiomeTags = new List<BiomeType> { BiomeType.Industrial }
            },
            new HazardTemplate
            {
                Name = "Electrified Floor",
                Description = "Arcs of electricity dance across the corroded grating.",
                HazardType = HazardType.Mechanical,
                Trigger = TriggerType.Movement,
                EffectScript = "DAMAGE:Lightning:1d6;STATUS:Stunned:1",
                MaxCooldown = 3,
                BiomeTags = new List<BiomeType> { BiomeType.Industrial }
            },
            new HazardTemplate
            {
                Name = "Unstable Machinery",
                Description = "Forgotten machinery shudders with accumulated energy.",
                HazardType = HazardType.Mechanical,
                Trigger = TriggerType.DamageTaken,
                EffectScript = "DAMAGE:Physical:2d4",
                OneTimeUse = true,
                BiomeTags = new List<BiomeType> { BiomeType.Industrial }
            },

            // ==========================================
            // ORGANIC BIOME HAZARDS
            // ==========================================
            new HazardTemplate
            {
                Name = "Spore Pod",
                Description = "A swollen fungal growth pulses with malevolent life.",
                HazardType = HazardType.Biological,
                Trigger = TriggerType.Movement,
                EffectScript = "DAMAGE:Poison:1d4;STATUS:Poisoned:2",
                MaxCooldown = 3,
                BiomeTags = new List<BiomeType> { BiomeType.Organic }
            },
            new HazardTemplate
            {
                Name = "Corruption Pool",
                Description = "Dark ichor pools on the floor, whispering promises of power.",
                HazardType = HazardType.Biological,
                Trigger = TriggerType.Movement,
                EffectScript = "CORRUPTION:2",
                MaxCooldown = 1,
                BiomeTags = new List<BiomeType> { BiomeType.Organic }
            },
            new HazardTemplate
            {
                Name = "Grasping Tendrils",
                Description = "Pale tendrils reach from cracks in the walls, hungry for warmth.",
                HazardType = HazardType.Biological,
                Trigger = TriggerType.TurnStart,
                EffectScript = "DAMAGE:Physical:1d4;STATUS:Slowed:1",
                MaxCooldown = 2,
                BiomeTags = new List<BiomeType> { BiomeType.Organic }
            },

            // ==========================================
            // VOID BIOME HAZARDS
            // ==========================================
            new HazardTemplate
            {
                Name = "Reality Fissure",
                Description = "The air shimmers where reality thins. Something watches from beyond.",
                HazardType = HazardType.Environmental,
                Trigger = TriggerType.TurnStart,
                EffectScript = "STRESS:2;DAMAGE:Psychic:1d4",
                MaxCooldown = 2,
                BiomeTags = new List<BiomeType> { BiomeType.Void }
            },
            new HazardTemplate
            {
                Name = "Entropy Field",
                Description = "A zone of pure emptiness drains life and light.",
                HazardType = HazardType.Environmental,
                Trigger = TriggerType.Movement,
                EffectScript = "DAMAGE:Cold:1d6",
                MaxCooldown = 3,
                BiomeTags = new List<BiomeType> { BiomeType.Void }
            },
            new HazardTemplate
            {
                Name = "Echoing Whispers",
                Description = "Voices speak from the darkness, eroding sanity with each word.",
                HazardType = HazardType.Environmental,
                Trigger = TriggerType.TurnStart,
                EffectScript = "STRESS:3",
                MaxCooldown = 2,
                BiomeTags = new List<BiomeType> { BiomeType.Void }
            }
        };
    }
}
