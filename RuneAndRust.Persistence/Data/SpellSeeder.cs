using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Persistence.Data;

/// <summary>
/// Seeds starter spells for the magic system.
/// All descriptions are Domain 4 compliant (archaeologist-perspective language).
/// </summary>
/// <remarks>
/// See: v0.4.3e (The Resonance) for implementation details.
///
/// Spells are organized by school:
/// - Destruction (4): Offensive damage spells
/// - Restoration (3): Healing and protective spells
/// - Alteration (3): Buff and debuff spells
/// - Divination (2): Information and perception spells
/// </remarks>
public static class SpellSeeder
{
    /// <summary>
    /// Seeds starter spells if none exist.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">Optional logger for seeding operations.</param>
    public static async Task SeedAsync(RuneAndRustDbContext context, ILogger? logger = null)
    {
        // Idempotency check
        if (await context.Spells.AnyAsync())
        {
            logger?.LogDebug("[SpellSeeder] Spells already exist, skipping seed");
            return;
        }

        logger?.LogInformation("[SpellSeeder] Seeding starter spells...");

        var spells = GetStarterSpells();

        await context.Spells.AddRangeAsync(spells);
        await context.SaveChangesAsync();

        // Log summary by school
        var bySchool = spells.GroupBy(s => s.School);
        foreach (var group in bySchool)
        {
            logger?.LogInformation("[SpellSeeder] Seeded {Count} {School} spells",
                group.Count(), group.Key);
        }

        logger?.LogInformation("[SpellSeeder] Seeded {Total} spells across {Schools} schools",
            spells.Count, bySchool.Count());
    }

    /// <summary>
    /// Gets all starter spells.
    /// </summary>
    public static List<Spell> GetStarterSpells()
    {
        var spells = new List<Spell>();

        spells.AddRange(GetDestructionSpells());
        spells.AddRange(GetRestorationSpells());
        spells.AddRange(GetAlterationSpells());
        spells.AddRange(GetDivinationSpells());

        return spells;
    }

    /// <summary>
    /// Gets Destruction school spells.
    /// </summary>
    public static List<Spell> GetDestructionSpells()
    {
        return new List<Spell>
        {
            new Spell
            {
                Id = Guid.NewGuid(),
                Name = "Spark",
                Description = "A whisper to the old fire-spirits, coaxing forth a tongue of flame.",
                School = SpellSchool.Destruction,
                TargetType = SpellTargetType.SingleEnemy,
                Range = SpellRange.Close,
                ApCost = 2,
                FluxCost = 8,
                BasePower = 6,
                EffectScript = "DAMAGE:Fire:1d6",
                Tier = 1,
                Archetype = ArchetypeType.Mystic
            },
            new Spell
            {
                Id = Guid.NewGuid(),
                Name = "Frost Touch",
                Description = "The caster's hand draws heat from flesh, leaving only the cold of the grave.",
                School = SpellSchool.Destruction,
                TargetType = SpellTargetType.SingleEnemy,
                Range = SpellRange.Touch,
                ApCost = 3,
                FluxCost = 10,
                BasePower = 8,
                EffectScript = "DAMAGE:Cold:1d8",
                Tier = 1,
                Archetype = ArchetypeType.Mystic
            },
            new Spell
            {
                Id = Guid.NewGuid(),
                Name = "Firebolt",
                Description = "A bolt of fire hurled from the palm, ancient and terrible.",
                School = SpellSchool.Destruction,
                TargetType = SpellTargetType.SingleEnemy,
                Range = SpellRange.Medium,
                ApCost = 3,
                FluxCost = 10,
                BasePower = 8,
                EffectScript = "DAMAGE:Fire:1d8",
                Tier = 1,
                Archetype = ArchetypeType.Mystic
            },
            new Spell
            {
                Id = Guid.NewGuid(),
                Name = "Thunder Strike",
                Description = "The sky's anger called down upon foes—a gift the Old Ones left behind.",
                School = SpellSchool.Destruction,
                TargetType = SpellTargetType.SingleEnemy,
                Range = SpellRange.Medium,
                ApCost = 5,
                FluxCost = 15,
                BasePower = 16,
                EffectScript = "DAMAGE:Lightning:2d8",
                Tier = 2,
                Archetype = ArchetypeType.Mystic
            }
        };
    }

    /// <summary>
    /// Gets Restoration school spells.
    /// </summary>
    public static List<Spell> GetRestorationSpells()
    {
        return new List<Spell>
        {
            new Spell
            {
                Id = Guid.NewGuid(),
                Name = "Mend Flesh",
                Description = "Ancient words spoken over wounds, knitting flesh as the elders once did.",
                School = SpellSchool.Restoration,
                TargetType = SpellTargetType.SingleAlly,
                Range = SpellRange.Touch,
                ApCost = 3,
                FluxCost = 6,
                BasePower = 12,
                EffectScript = "HEAL:2d6",
                Tier = 1,
                Archetype = ArchetypeType.Mystic
            },
            new Spell
            {
                Id = Guid.NewGuid(),
                Name = "Cleansing Light",
                Description = "Pure radiance that burns away corruption and soothes the wounded.",
                School = SpellSchool.Restoration,
                TargetType = SpellTargetType.SingleAlly,
                Range = SpellRange.Close,
                ApCost = 4,
                FluxCost = 10,
                BasePower = 8,
                EffectScript = "HEAL:1d8;CLEANSE:1",
                Tier = 1,
                Archetype = ArchetypeType.Mystic
            },
            new Spell
            {
                Id = Guid.NewGuid(),
                Name = "Sanctuary",
                Description = "A shield of light wraps the caster, turning aside harm.",
                School = SpellSchool.Restoration,
                TargetType = SpellTargetType.Self,
                Range = SpellRange.Self,
                ApCost = 5,
                FluxCost = 12,
                BasePower = 10,
                EffectScript = "STATUS:Fortified:2:3;HEAL:1d6",
                Tier = 2,
                Archetype = ArchetypeType.Mystic
            }
        };
    }

    /// <summary>
    /// Gets Alteration school spells.
    /// </summary>
    public static List<Spell> GetAlterationSpells()
    {
        return new List<Spell>
        {
            new Spell
            {
                Id = Guid.NewGuid(),
                Name = "Stone Skin",
                Description = "The skin hardens like the Old Ones' shells—briefly proof against harm.",
                School = SpellSchool.Alteration,
                TargetType = SpellTargetType.Self,
                Range = SpellRange.Self,
                ApCost = 4,
                FluxCost = 12,
                BasePower = 0,
                EffectScript = "STATUS:Fortified:3:2",
                Tier = 1,
                Archetype = ArchetypeType.Mystic
            },
            new Spell
            {
                Id = Guid.NewGuid(),
                Name = "Weaken",
                Description = "A curse that frays the target's defenses, leaving them open to harm.",
                School = SpellSchool.Alteration,
                TargetType = SpellTargetType.SingleEnemy,
                Range = SpellRange.Close,
                ApCost = 3,
                FluxCost = 8,
                BasePower = 0,
                EffectScript = "STATUS:Vulnerable:2",
                Tier = 1,
                Archetype = ArchetypeType.Mystic
            },
            new Spell
            {
                Id = Guid.NewGuid(),
                Name = "Haste",
                Description = "Time bends, granting swiftness to the touched—a dangerous gift.",
                School = SpellSchool.Alteration,
                TargetType = SpellTargetType.SingleAlly,
                Range = SpellRange.Close,
                ApCost = 5,
                FluxCost = 14,
                BasePower = 0,
                EffectScript = "STATUS:Hasted:2",
                Tier = 2,
                Archetype = ArchetypeType.Mystic
            }
        };
    }

    /// <summary>
    /// Gets Divination school spells.
    /// </summary>
    public static List<Spell> GetDivinationSpells()
    {
        return new List<Spell>
        {
            new Spell
            {
                Id = Guid.NewGuid(),
                Name = "Spirit Sight",
                Description = "The caster's eyes cloud, then clear—seeing what others cannot.",
                School = SpellSchool.Divination,
                TargetType = SpellTargetType.Self,
                Range = SpellRange.Self,
                ApCost = 2,
                FluxCost = 4,
                BasePower = 0,
                EffectScript = "STATUS:TrueSeeing:3",
                Tier = 1,
                Archetype = ArchetypeType.Mystic
            },
            new Spell
            {
                Id = Guid.NewGuid(),
                Name = "Analyze Foe",
                Description = "The target's weaknesses revealed to the discerning eye.",
                School = SpellSchool.Divination,
                TargetType = SpellTargetType.SingleEnemy,
                Range = SpellRange.Medium,
                ApCost = 2,
                FluxCost = 5,
                BasePower = 0,
                EffectScript = "STATUS:Analyzed:3",
                Tier = 1,
                Archetype = ArchetypeType.Mystic
            }
        };
    }
}
