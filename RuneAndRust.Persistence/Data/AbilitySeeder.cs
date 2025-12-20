using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Persistence.Data;

/// <summary>
/// Seeds the database with Tier 1 abilities for each character archetype.
/// </summary>
/// <remarks>
/// Each archetype receives 2 abilities at Tier 1:
/// - One offensive/primary ability
/// - One utility/defensive ability
///
/// All descriptions must be Domain 4 compliant (no precision measurements).
/// </remarks>
public static class AbilitySeeder
{
    /// <summary>
    /// Seeds the abilities if none exist.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">Optional logger for seeding operations.</param>
    public static async Task SeedAsync(RuneAndRustDbContext context, ILogger? logger = null)
    {
        if (await context.ActiveAbilities.AnyAsync())
        {
            logger?.LogDebug("Active abilities already exist, skipping seed");
            return;
        }

        logger?.LogInformation("Seeding Tier 1 abilities...");

        var abilities = GetTier1Abilities();
        await context.ActiveAbilities.AddRangeAsync(abilities);
        await context.SaveChangesAsync();

        logger?.LogInformation("Seeded {Count} Tier 1 abilities", abilities.Count);
    }

    /// <summary>
    /// Gets all Tier 1 abilities for seeding.
    /// </summary>
    /// <returns>A list of ActiveAbility entities to seed.</returns>
    public static List<ActiveAbility> GetTier1Abilities()
    {
        return new List<ActiveAbility>
        {
            // === WARRIOR ABILITIES ===
            new ActiveAbility
            {
                Name = "Wild Swing",
                Description = "A powerful but reckless attack that sacrifices precision for raw power. " +
                              "The force behind this strike can crack armor and shatter bone.",
                Archetype = ArchetypeType.Warrior,
                Tier = 1,
                StaminaCost = 35,
                AetherCost = 0,
                CooldownTurns = 0,
                Range = 1,
                EffectScript = "DAMAGE:Physical:2d8"
            },
            new ActiveAbility
            {
                Name = "Defensive Stance",
                Description = "Brace yourself against incoming attacks, raising your guard to absorb blows. " +
                              "A moment of focus in the chaos of battle.",
                Archetype = ArchetypeType.Warrior,
                Tier = 1,
                StaminaCost = 0,
                AetherCost = 0,
                CooldownTurns = 2,
                Range = 0, // Self-target
                EffectScript = "STATUS:Fortified:1:1"
            },

            // === SKIRMISHER ABILITIES ===
            new ActiveAbility
            {
                Name = "Precise Shot",
                Description = "Take careful aim at an exposed weakness. Patience and precision yield results " +
                              "that brute force cannot match.",
                Archetype = ArchetypeType.Skirmisher,
                Tier = 1,
                StaminaCost = 30,
                AetherCost = 0,
                CooldownTurns = 0,
                Range = 2, // Ranged
                EffectScript = "DAMAGE:Physical:1d8"
            },
            new ActiveAbility
            {
                Name = "Evasive Roll",
                Description = "A quick tumble to avoid incoming attacks. Survival in the Rust-lands often depends " +
                              "on not being where the enemy expects.",
                Archetype = ArchetypeType.Skirmisher,
                Tier = 1,
                StaminaCost = 20,
                AetherCost = 0,
                CooldownTurns = 2,
                Range = 0, // Self-target
                EffectScript = "STATUS:Evasion:1:1"
            },

            // === MYSTIC ABILITIES ===
            new ActiveAbility
            {
                Name = "Aether Dart",
                Description = "Channel raw Aetheric energy into a concentrated bolt. The air crackles with " +
                              "residual power as the projectile streaks toward its target.",
                Archetype = ArchetypeType.Mystic,
                Tier = 1,
                StaminaCost = 0,
                AetherCost = 15,
                CooldownTurns = 0,
                Range = 3, // Ranged
                EffectScript = "DAMAGE:Arcane:1d10"
            },
            new ActiveAbility
            {
                Name = "Mind Spike",
                Description = "A psychic assault that pierces mental defenses. The target's thoughts scatter, " +
                              "leaving them momentarily vulnerable and disoriented.",
                Archetype = ArchetypeType.Mystic,
                Tier = 1,
                StaminaCost = 0,
                AetherCost = 25,
                CooldownTurns = 1,
                Range = 2, // Ranged
                EffectScript = "DAMAGE:Psychic:1d6;STATUS:Dazed:1:1"
            },

            // === ADEPT ABILITIES ===
            new ActiveAbility
            {
                Name = "Mend Wound",
                Description = "Channel restorative energy through your hands. Minor wounds close, bleeding stops, " +
                              "and vitality returns. Deep injuries require rest and proper care.",
                Archetype = ArchetypeType.Adept,
                Tier = 1,
                StaminaCost = 20,
                AetherCost = 0,
                CooldownTurns = 2,
                Range = 0, // Self-target
                EffectScript = "HEAL:12"
            },
            new ActiveAbility
            {
                Name = "Blessed Strike",
                Description = "Infuse your weapon with purifying light. The blow both harms enemies and " +
                              "revitalizes the wielder through the energy released.",
                Archetype = ArchetypeType.Adept,
                Tier = 1,
                StaminaCost = 0,
                AetherCost = 15,
                CooldownTurns = 1,
                Range = 1, // Melee
                EffectScript = "DAMAGE:Radiant:1d6;HEAL:6"
            }
        };
    }
}
