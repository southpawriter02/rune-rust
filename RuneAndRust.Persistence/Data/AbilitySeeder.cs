using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Persistence.Data;

/// <summary>
/// Seeds the database with abilities for characters and enemies.
/// </summary>
/// <remarks>
/// Character abilities (Tier 1): Each archetype receives 2 abilities.
/// Enemy abilities (v0.2.4a): Shared pool for enemy templates to reference.
///
/// All descriptions must be Domain 4 compliant (no precision measurements).
///
/// See: SPEC-SEED-001 for Database Seeding System design.
/// </remarks>
public static class AbilitySeeder
{
    /// <summary>
    /// Seeds all abilities if none exist, including enemy abilities (v0.2.4a).
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

        logger?.LogInformation("Seeding abilities...");

        var playerAbilities = GetTier1Abilities();
        var enemyAbilities = GetEnemyAbilities();
        var allAbilities = playerAbilities.Concat(enemyAbilities).ToList();

        await context.ActiveAbilities.AddRangeAsync(allAbilities);
        await context.SaveChangesAsync();

        logger?.LogInformation("Seeded {PlayerCount} player abilities and {EnemyCount} enemy abilities",
            playerAbilities.Count, enemyAbilities.Count);
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

    /// <summary>
    /// Gets all enemy-specific abilities for seeding (v0.2.4a).
    /// These abilities are referenced by name in EnemyTemplate.AbilityNames.
    /// </summary>
    /// <returns>A list of ActiveAbility entities for enemies.</returns>
    public static List<ActiveAbility> GetEnemyAbilities()
    {
        return new List<ActiveAbility>
        {
            // === UNDEAD ABILITIES ===
            new ActiveAbility
            {
                Name = "Rusty Cleave",
                Description = "A crude, sweeping strike with corroded steel. The jagged edge tears flesh " +
                              "and leaves wounds that fester with rust-borne corruption.",
                Archetype = null, // Enemy ability, no player archetype
                Tier = 0, // Enemy tier marker
                StaminaCost = 2,
                AetherCost = 0,
                CooldownTurns = 0,
                Range = 1,
                EffectScript = "DAMAGE:Physical:1d6+2;STATUS:Bleed:1:1"
            },
            new ActiveAbility
            {
                Name = "Grave Chill",
                Description = "An aura of deathly cold radiates from the corpse-warrior. Nearby foes " +
                              "feel their limbs grow sluggish as warmth drains away.",
                Archetype = null,
                Tier = 0,
                StaminaCost = 4,
                AetherCost = 0,
                CooldownTurns = 3,
                Range = 2,
                EffectScript = "DAMAGE:Cold:1d6;STATUS:Slow:2:1"
            },
            new ActiveAbility
            {
                Name = "Baleful Glare",
                Description = "Eyes burn with malevolent light, freezing prey with dread. " +
                              "Ancient hatred seeps through that terrible gaze.",
                Archetype = null,
                Tier = 0,
                StaminaCost = 3,
                AetherCost = 0,
                CooldownTurns = 2,
                Range = 2,
                EffectScript = "STATUS:Fear:2:1"
            },

            // === MECHANICAL ABILITIES ===
            new ActiveAbility
            {
                Name = "Servo Slam",
                Description = "Hydraulic limbs deliver a crushing blow. The impact rings with " +
                              "the scream of tortured metal, leaving targets staggering.",
                Archetype = null,
                Tier = 0,
                StaminaCost = 3,
                AetherCost = 0,
                CooldownTurns = 1,
                Range = 1,
                EffectScript = "DAMAGE:Physical:1d8+4;STATUS:Stagger:1:1"
            },
            new ActiveAbility
            {
                Name = "Overclock",
                Description = "Internal mechanisms surge with dangerous intensity. Speed increases " +
                              "at the cost of structural integrity as systems redline.",
                Archetype = null,
                Tier = 0,
                StaminaCost = 5,
                AetherCost = 0,
                CooldownTurns = 4,
                Range = 0, // Self
                EffectScript = "STATUS:Haste:2:1;DAMAGE:Self:1d4"
            },

            // === BEAST ABILITIES ===
            new ActiveAbility
            {
                Name = "Savage Lunge",
                Description = "A desperate leap toward vulnerable prey. The beast's momentum " +
                              "carries it forward in a blur of fangs and fury.",
                Archetype = null,
                Tier = 0,
                StaminaCost = 4,
                AetherCost = 0,
                CooldownTurns = 2,
                Range = 2,
                EffectScript = "DAMAGE:Physical:1d8+3;STATUS:Prone:1:1"
            },

            // === HUMANOID ABILITIES ===
            new ActiveAbility
            {
                Name = "Scavenger's Swing",
                Description = "An uncontrolled but devastating attack. Wild and unpredictable, " +
                              "trading precision for sheer brutality.",
                Archetype = null,
                Tier = 0,
                StaminaCost = 2,
                AetherCost = 0,
                CooldownTurns = 0,
                Range = 1,
                EffectScript = "DAMAGE:Physical:1d6+1"
            },
            new ActiveAbility
            {
                Name = "Intimidating Shout",
                Description = "A bellowing war cry that shakes resolve. The primal scream echoes " +
                              "through ruins, sowing doubt in enemy hearts.",
                Archetype = null,
                Tier = 0,
                StaminaCost = 3,
                AetherCost = 0,
                CooldownTurns = 3,
                Range = 2,
                EffectScript = "STATUS:Intimidate:2:1"
            }
        };
    }
}
