using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Persistence.Data;

/// <summary>
/// Seeds the database with ambient condition definitions (v0.3.3b).
/// Each condition has passive stat penalties and optional active tick effects.
/// Updated in v0.3.3c to include biome tags for environment population.
/// </summary>
/// <remarks>
/// Conditions are room-wide environmental effects that persist until the player leaves.
/// All descriptions must be Domain 4 compliant (no precision measurements).
///
/// See: SPEC-SEED-001 for Database Seeding System design.
/// </remarks>
public static class ConditionSeeder
{
    /// <summary>
    /// Seeds the ambient conditions if none exist.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">Optional logger for seeding operations.</param>
    public static async Task SeedAsync(RuneAndRustDbContext context, ILogger? logger = null)
    {
        if (await context.AmbientConditions.AnyAsync())
        {
            logger?.LogDebug("Ambient conditions already exist, skipping seed");
            return;
        }

        logger?.LogInformation("Seeding ambient conditions...");

        var conditions = GetConditions();
        await context.AmbientConditions.AddRangeAsync(conditions);
        await context.SaveChangesAsync();

        logger?.LogInformation("Seeded {Count} ambient conditions", conditions.Count);
    }

    /// <summary>
    /// Gets all ambient condition definitions for seeding.
    /// </summary>
    /// <returns>A list of AmbientCondition entities to seed.</returns>
    public static List<AmbientCondition> GetConditions()
    {
        return new List<AmbientCondition>
        {
            // Psychic Resonance: -1 WILL, +2 Stress/turn
            // Biomes: Organic (corruption), Void (otherworldly)
            new AmbientCondition
            {
                Type = ConditionType.PsychicResonance,
                Name = "Psychic Resonance",
                Description = "A low hum permeates the air, pressing against your thoughts. The walls themselves seem to whisper.",
                Color = "purple",
                TickScript = "STRESS:2",
                TickChance = 1.0f,
                BiomeTags = new List<BiomeType> { BiomeType.Organic, BiomeType.Void }
            },

            // Toxic Atmosphere: No passive, 1d4 Poison/turn
            // Biomes: Industrial (chemicals), Organic (spores)
            new AmbientCondition
            {
                Type = ConditionType.ToxicAtmosphere,
                Name = "Toxic Atmosphere",
                Description = "The air shimmers with chemical haze. Each breath burns the lungs and corrodes the throat.",
                Color = "green",
                TickScript = "DAMAGE:Poison:1d4",
                TickChance = 1.0f,
                BiomeTags = new List<BiomeType> { BiomeType.Industrial, BiomeType.Organic }
            },

            // Deep Cold: -1 FINESSE, 1 Ice/turn
            // Biomes: Void (entropy, cold emptiness)
            new AmbientCondition
            {
                Type = ConditionType.DeepCold,
                Name = "Deep Cold",
                Description = "Numbing frost clings to every surface. Your fingers grow stiff, and your breath hangs frozen in the air.",
                Color = "cyan",
                TickScript = "DAMAGE:Ice:1d1",
                TickChance = 1.0f,
                BiomeTags = new List<BiomeType> { BiomeType.Void }
            },

            // Scorching Heat: -1 STURDINESS, 1 Fire/turn
            // Biomes: Industrial (machinery, furnaces)
            new AmbientCondition
            {
                Type = ConditionType.ScorchingHeat,
                Name = "Scorching Heat",
                Description = "Oppressive heat radiates from the walls. Sweat beads instantly, and the air itself seems to shimmer.",
                Color = "red",
                TickScript = "DAMAGE:Fire:1d1",
                TickChance = 1.0f,
                BiomeTags = new List<BiomeType> { BiomeType.Industrial }
            },

            // Low Visibility: -2 WITS, No active effect
            // Biomes: Ruin (dust), can appear anywhere
            new AmbientCondition
            {
                Type = ConditionType.LowVisibility,
                Name = "Low Visibility",
                Description = "Thick dust or fog obscures the path ahead. Shapes loom and fade in the murk.",
                Color = "grey",
                TickScript = string.Empty,
                TickChance = 0f,
                BiomeTags = new List<BiomeType> { BiomeType.Ruin }
            },

            // Blighted Ground: -1 WILL, -1 WITS, +1 Corruption/turn
            // Biomes: Organic (corruption)
            new AmbientCondition
            {
                Type = ConditionType.BlightedGround,
                Name = "Blighted Ground",
                Description = "Runic corruption seeps from cracks in the floor. The air tastes of rust and old blood.",
                Color = "darkred",
                TickScript = "CORRUPTION:1",
                TickChance = 1.0f,
                BiomeTags = new List<BiomeType> { BiomeType.Organic }
            },

            // Static Field: -1 FINESSE, 1d6 Lightning (25% chance)
            // Biomes: Industrial (electrical systems)
            new AmbientCondition
            {
                Type = ConditionType.StaticField,
                Name = "Static Field",
                Description = "Electrical discharge crackles through the air. Your hair stands on end, and the taste of ozone fills your mouth.",
                Color = "yellow",
                TickScript = "DAMAGE:Lightning:1d6",
                TickChance = 0.25f,
                BiomeTags = new List<BiomeType> { BiomeType.Industrial }
            },

            // Dread Presence: -2 WILL, +3 Stress/turn
            // Biomes: Ruin (ancient horrors), Void (otherworldly)
            new AmbientCondition
            {
                Type = ConditionType.DreadPresence,
                Name = "Dread Presence",
                Description = "Something ancient and terrible watches from the shadows. Your heart pounds, and primal fear claws at your spine.",
                Color = "darkmagenta",
                TickScript = "STRESS:3",
                TickChance = 1.0f,
                BiomeTags = new List<BiomeType> { BiomeType.Ruin, BiomeType.Void }
            }
        };
    }
}
