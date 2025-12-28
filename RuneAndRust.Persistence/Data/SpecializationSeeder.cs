using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Persistence.Data;

/// <summary>
/// Seeds the database with specializations and their ability trees.
/// </summary>
/// <remarks>
/// v0.4.1a introduces two specializations:
/// - Berserkr (Warrior): Rage-focused damage dealer
/// - Skald (Skirmisher): Buff/debuff support via shouts
///
/// Each specialization has 8 nodes across 4 tiers:
/// - Tier 1 (Root): 3 nodes, no prerequisites
/// - Tier 2: 2 nodes, requires 1 Tier 1 parent
/// - Tier 3: 2 nodes, requires Tier 2 parents
/// - Tier 4 (Capstone): 1 node, requires all Tier 3 nodes
///
/// All descriptions must be Domain 4 compliant (no precision measurements).
///
/// See: SPEC-SPECIALIZATION-001 for design documentation.
/// </remarks>
public static class SpecializationSeeder
{
    /// <summary>
    /// Seeds all specializations if none exist.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">Optional logger for seeding operations.</param>
    public static async Task SeedAsync(RuneAndRustDbContext context, ILogger? logger = null)
    {
        if (await context.Specializations.AnyAsync())
        {
            logger?.LogDebug("[Seeder] Specializations already exist, skipping seed");
            return;
        }

        logger?.LogInformation("[Seeder] Seeding Specializations...");

        // 1. Create specialization-specific abilities first
        var berserkrAbilities = GetBerserkrAbilities();
        var skaldAbilities = GetSkaldAbilities();
        var allAbilities = berserkrAbilities.Concat(skaldAbilities).ToList();

        await context.ActiveAbilities.AddRangeAsync(allAbilities);
        await context.SaveChangesAsync();

        logger?.LogDebug("[Seeder] Created {Count} specialization abilities", allAbilities.Count);

        // 2. Create specializations with their node trees
        var berserkr = CreateBerserkrSpecialization(berserkrAbilities);
        var skald = CreateSkaldSpecialization(skaldAbilities);

        await context.Specializations.AddRangeAsync(berserkr, skald);
        await context.SaveChangesAsync();

        logger?.LogInformation(
            "[Seeder] Seeded {SpecCount} specializations with {NodeCount} total nodes",
            2,
            berserkr.Nodes.Count + skald.Nodes.Count);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Berserkr Specialization (Warrior)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates abilities for the Berserkr specialization tree.
    /// </summary>
    private static List<ActiveAbility> GetBerserkrAbilities()
    {
        return new List<ActiveAbility>
        {
            // === TIER 1 (Root Nodes) ===
            new ActiveAbility
            {
                Name = "Battle Cry",
                Description = "A thunderous war cry that stirs the blood and hardens resolve. " +
                              "Allies within earshot feel their muscles surge with borrowed fury.",
                Archetype = ArchetypeType.Warrior,
                Tier = 1,
                StaminaCost = 25,
                AetherCost = 0,
                CooldownTurns = 3,
                Range = 0,
                EffectScript = "STATUS:Empowered:3:1"
            },
            new ActiveAbility
            {
                Name = "Reckless Swing",
                Description = "Abandon all caution for devastating power. The strike leaves the wielder " +
                              "exposed, trading defense for overwhelming offense.",
                Archetype = ArchetypeType.Warrior,
                Tier = 1,
                StaminaCost = 40,
                AetherCost = 0,
                CooldownTurns = 0,
                Range = 1,
                EffectScript = "DAMAGE:Physical:3d6;DAMAGE:Self:1d4"
            },
            new ActiveAbility
            {
                Name = "Blood Frenzy",
                Description = "The sight of fallen foes fuels an insatiable hunger for battle. " +
                              "Each killing blow restores vigor to continue the carnage.",
                Archetype = ArchetypeType.Warrior,
                Tier = 1,
                StaminaCost = 0,
                AetherCost = 0,
                CooldownTurns = 0,
                Range = 0,
                EffectScript = "PASSIVE:OnKill:RestoreStamina:15"
            },

            // === TIER 2 ===
            new ActiveAbility
            {
                Name = "Intimidating Presence",
                Description = "The warrior's mere presence radiates threat. Enemies nearby feel their " +
                              "confidence waver as primal fear takes hold.",
                Archetype = ArchetypeType.Warrior,
                Tier = 2,
                StaminaCost = 30,
                AetherCost = 0,
                CooldownTurns = 4,
                Range = 2,
                EffectScript = "STATUS:Intimidate:2:1;AOE:Enemies"
            },
            new ActiveAbility
            {
                Name = "Berserker Rage",
                Description = "Pain becomes fuel. When wounded, the warrior enters a frenzied state " +
                              "where every blow lands with devastating force.",
                Archetype = ArchetypeType.Warrior,
                Tier = 2,
                StaminaCost = 0,
                AetherCost = 0,
                CooldownTurns = 0,
                Range = 0,
                EffectScript = "PASSIVE:WhenBelowHalf:DamageBonus:50%"
            },

            // === TIER 3 ===
            new ActiveAbility
            {
                Name = "War Cry",
                Description = "A battle hymn that echoes through the ruins. All allies are filled with " +
                              "battle-fury, their attacks becoming swift and certain.",
                Archetype = ArchetypeType.Warrior,
                Tier = 3,
                StaminaCost = 50,
                AetherCost = 0,
                CooldownTurns = 5,
                Range = 0,
                EffectScript = "STATUS:Empowered:3:1;STATUS:Haste:2:1;AOE:Allies"
            },
            new ActiveAbility
            {
                Name = "Unrelenting",
                Description = "The warrior refuses to fall. When struck with a fatal blow, sheer willpower " +
                              "keeps the body fighting for precious moments longer.",
                Archetype = ArchetypeType.Warrior,
                Tier = 3,
                StaminaCost = 0,
                AetherCost = 0,
                CooldownTurns = 0,
                Range = 0,
                EffectScript = "PASSIVE:OnFatalDamage:SurviveAt1HP:Once"
            },

            // === TIER 4 (Capstone) ===
            new ActiveAbility
            {
                Name = "Avatar of Fury",
                Description = "Become wrath incarnate. The warrior's form swells with primal power, " +
                              "each strike a thunderclap, each step an earthquake. Enemies flee or fall.",
                Archetype = ArchetypeType.Warrior,
                Tier = 4,
                StaminaCost = 80,
                AetherCost = 0,
                CooldownTurns = 10,
                Range = 0,
                EffectScript = "TRANSFORM:AvatarOfFury:3;STATUS:Empowered:3:2;STATUS:Fortified:3:2"
            }
        };
    }

    /// <summary>
    /// Creates the Berserkr specialization with its node tree.
    /// </summary>
    private static Specialization CreateBerserkrSpecialization(List<ActiveAbility> abilities)
    {
        var spec = new Specialization
        {
            Type = SpecializationType.Berserkr,
            Name = "Berserkr",
            Description = "The path of unbridled fury. Berserkrs trade caution for overwhelming power, " +
                          "feeding on violence to fuel their rampage. Where others see pain, they find strength.",
            RequiredArchetype = ArchetypeType.Warrior,
            RequiredLevel = 1
        };

        // Get abilities by name for node linking
        var battleCry = abilities.First(a => a.Name == "Battle Cry");
        var recklessSwing = abilities.First(a => a.Name == "Reckless Swing");
        var bloodFrenzy = abilities.First(a => a.Name == "Blood Frenzy");
        var intimidatingPresence = abilities.First(a => a.Name == "Intimidating Presence");
        var berserkerRage = abilities.First(a => a.Name == "Berserker Rage");
        var warCry = abilities.First(a => a.Name == "War Cry");
        var unrelenting = abilities.First(a => a.Name == "Unrelenting");
        var avatarOfFury = abilities.First(a => a.Name == "Avatar of Fury");

        // Create Tier 1 nodes (roots)
        var nodeBattleCry = new SpecializationNode
        {
            AbilityId = battleCry.Id,
            Ability = battleCry,
            Tier = 1,
            CostPP = 1,
            PositionX = 0,
            PositionY = 0,
            ParentNodeIds = new List<Guid>()
        };

        var nodeRecklessSwing = new SpecializationNode
        {
            AbilityId = recklessSwing.Id,
            Ability = recklessSwing,
            Tier = 1,
            CostPP = 1,
            PositionX = 1,
            PositionY = 0,
            ParentNodeIds = new List<Guid>()
        };

        var nodeBloodFrenzy = new SpecializationNode
        {
            AbilityId = bloodFrenzy.Id,
            Ability = bloodFrenzy,
            Tier = 1,
            CostPP = 1,
            PositionX = 2,
            PositionY = 0,
            ParentNodeIds = new List<Guid>()
        };

        // Create Tier 2 nodes
        var nodeIntimidatingPresence = new SpecializationNode
        {
            AbilityId = intimidatingPresence.Id,
            Ability = intimidatingPresence,
            Tier = 2,
            CostPP = 2,
            PositionX = 0,
            PositionY = 1,
            ParentNodeIds = new List<Guid> { nodeBattleCry.Id }
        };

        var nodeBerserkerRage = new SpecializationNode
        {
            AbilityId = berserkerRage.Id,
            Ability = berserkerRage,
            Tier = 2,
            CostPP = 2,
            PositionX = 1,
            PositionY = 1,
            ParentNodeIds = new List<Guid> { nodeRecklessSwing.Id }
        };

        // Create Tier 3 nodes
        var nodeWarCry = new SpecializationNode
        {
            AbilityId = warCry.Id,
            Ability = warCry,
            Tier = 3,
            CostPP = 3,
            PositionX = 0,
            PositionY = 2,
            ParentNodeIds = new List<Guid> { nodeIntimidatingPresence.Id, nodeBerserkerRage.Id }
        };

        var nodeUnrelenting = new SpecializationNode
        {
            AbilityId = unrelenting.Id,
            Ability = unrelenting,
            Tier = 3,
            CostPP = 3,
            PositionX = 1,
            PositionY = 2,
            ParentNodeIds = new List<Guid> { nodeBerserkerRage.Id }
        };

        // Create Tier 4 Capstone
        var nodeAvatarOfFury = new SpecializationNode
        {
            AbilityId = avatarOfFury.Id,
            Ability = avatarOfFury,
            Tier = 4,
            CostPP = 5,
            PositionX = 0,
            PositionY = 3,
            ParentNodeIds = new List<Guid> { nodeWarCry.Id, nodeUnrelenting.Id }
        };

        spec.Nodes = new List<SpecializationNode>
        {
            nodeBattleCry, nodeRecklessSwing, nodeBloodFrenzy,
            nodeIntimidatingPresence, nodeBerserkerRage,
            nodeWarCry, nodeUnrelenting,
            nodeAvatarOfFury
        };

        // Set back-reference
        foreach (var node in spec.Nodes)
        {
            node.Specialization = spec;
            node.SpecializationId = spec.Id;
        }

        return spec;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Skald Specialization (Skirmisher)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates abilities for the Skald specialization tree.
    /// </summary>
    private static List<ActiveAbility> GetSkaldAbilities()
    {
        return new List<ActiveAbility>
        {
            // === TIER 1 (Root Nodes) ===
            new ActiveAbility
            {
                Name = "Inspiring Word",
                Description = "A verse of encouragement that mends both body and spirit. " +
                              "The listener feels wounds close and courage return.",
                Archetype = ArchetypeType.Skirmisher,
                Tier = 1,
                StaminaCost = 30,
                AetherCost = 0,
                CooldownTurns = 2,
                Range = 2,
                EffectScript = "HEAL:10;STATUS:Inspired:2:1"
            },
            new ActiveAbility
            {
                Name = "Discordant Note",
                Description = "A sharp, jarring sound that disrupts concentration. " +
                              "The target's thoughts scatter, their actions becoming clumsy.",
                Archetype = ArchetypeType.Skirmisher,
                Tier = 1,
                StaminaCost = 25,
                AetherCost = 0,
                CooldownTurns = 1,
                Range = 2,
                EffectScript = "STATUS:Dazed:2:1;DAMAGE:Sonic:1d4"
            },
            new ActiveAbility
            {
                Name = "Echoing Chant",
                Description = "The Skald's voice resonates with lingering power. " +
                              "Beneficial effects on allies persist beyond their natural duration.",
                Archetype = ArchetypeType.Skirmisher,
                Tier = 1,
                StaminaCost = 20,
                AetherCost = 0,
                CooldownTurns = 3,
                Range = 0,
                EffectScript = "PASSIVE:ExtendBuffs:1;AOE:Allies"
            },

            // === TIER 2 ===
            new ActiveAbility
            {
                Name = "Verse of Valor",
                Description = "An ancient battle hymn that sharpens reflexes and steadies hands. " +
                              "Those who hear it find their strikes landing true.",
                Archetype = ArchetypeType.Skirmisher,
                Tier = 2,
                StaminaCost = 40,
                AetherCost = 0,
                CooldownTurns = 4,
                Range = 0,
                EffectScript = "STATUS:Focused:3:1;AOE:Allies"
            },
            new ActiveAbility
            {
                Name = "Cacophony",
                Description = "A wall of dissonant sound that assaults the senses. " +
                              "Enemies caught in the cacophony reel in confusion.",
                Archetype = ArchetypeType.Skirmisher,
                Tier = 2,
                StaminaCost = 45,
                AetherCost = 0,
                CooldownTurns = 3,
                Range = 2,
                EffectScript = "DAMAGE:Sonic:2d6;STATUS:Confused:2:1;AOE:Enemies"
            },

            // === TIER 3 ===
            new ActiveAbility
            {
                Name = "Epic Recitation",
                Description = "A fragment of the old epics, sung with power that transcends mere words. " +
                              "Wounds mend, exhaustion fades, hope returns to the battered.",
                Archetype = ArchetypeType.Skirmisher,
                Tier = 3,
                StaminaCost = 60,
                AetherCost = 0,
                CooldownTurns = 5,
                Range = 0,
                EffectScript = "HEAL:25;STATUS:Regenerating:3:1;AOE:Allies"
            },
            new ActiveAbility
            {
                Name = "Silencing Shriek",
                Description = "A piercing note that disrupts all concentration. Spellcasters lose their " +
                              "focus, charged abilities dissipate, and silence falls.",
                Archetype = ArchetypeType.Skirmisher,
                Tier = 3,
                StaminaCost = 50,
                AetherCost = 0,
                CooldownTurns = 4,
                Range = 3,
                EffectScript = "INTERRUPT;STATUS:Silenced:2:1;AOE:Enemies"
            },

            // === TIER 4 (Capstone) ===
            new ActiveAbility
            {
                Name = "The Final Verse",
                Description = "The Skald speaks the words that end stories—and begin them anew. " +
                              "A fallen ally rises, the living surge with renewed purpose, and enemies quail.",
                Archetype = ArchetypeType.Skirmisher,
                Tier = 4,
                StaminaCost = 100,
                AetherCost = 0,
                CooldownTurns = 10,
                Range = 0,
                EffectScript = "REVIVE:50%;STATUS:Empowered:3:2;STATUS:Regenerating:3:2;AOE:Allies"
            }
        };
    }

    /// <summary>
    /// Creates the Skald specialization with its node tree.
    /// </summary>
    private static Specialization CreateSkaldSpecialization(List<ActiveAbility> abilities)
    {
        var spec = new Specialization
        {
            Type = SpecializationType.Skald,
            Name = "Skald",
            Description = "The path of the voice eternal. Skalds wield words as weapons, their songs " +
                          "capable of mending allies and shattering foes. They are the living memory of the old world.",
            RequiredArchetype = ArchetypeType.Skirmisher,
            RequiredLevel = 1
        };

        // Get abilities by name for node linking
        var inspiringWord = abilities.First(a => a.Name == "Inspiring Word");
        var discordantNote = abilities.First(a => a.Name == "Discordant Note");
        var echoingChant = abilities.First(a => a.Name == "Echoing Chant");
        var verseOfValor = abilities.First(a => a.Name == "Verse of Valor");
        var cacophony = abilities.First(a => a.Name == "Cacophony");
        var epicRecitation = abilities.First(a => a.Name == "Epic Recitation");
        var silencingShriek = abilities.First(a => a.Name == "Silencing Shriek");
        var theFinalVerse = abilities.First(a => a.Name == "The Final Verse");

        // Create Tier 1 nodes (roots)
        var nodeInspiringWord = new SpecializationNode
        {
            AbilityId = inspiringWord.Id,
            Ability = inspiringWord,
            Tier = 1,
            CostPP = 1,
            PositionX = 0,
            PositionY = 0,
            ParentNodeIds = new List<Guid>()
        };

        var nodeDiscordantNote = new SpecializationNode
        {
            AbilityId = discordantNote.Id,
            Ability = discordantNote,
            Tier = 1,
            CostPP = 1,
            PositionX = 1,
            PositionY = 0,
            ParentNodeIds = new List<Guid>()
        };

        var nodeEchoingChant = new SpecializationNode
        {
            AbilityId = echoingChant.Id,
            Ability = echoingChant,
            Tier = 1,
            CostPP = 1,
            PositionX = 2,
            PositionY = 0,
            ParentNodeIds = new List<Guid>()
        };

        // Create Tier 2 nodes
        var nodeVerseOfValor = new SpecializationNode
        {
            AbilityId = verseOfValor.Id,
            Ability = verseOfValor,
            Tier = 2,
            CostPP = 2,
            PositionX = 0,
            PositionY = 1,
            ParentNodeIds = new List<Guid> { nodeInspiringWord.Id }
        };

        var nodeCacophony = new SpecializationNode
        {
            AbilityId = cacophony.Id,
            Ability = cacophony,
            Tier = 2,
            CostPP = 2,
            PositionX = 1,
            PositionY = 1,
            ParentNodeIds = new List<Guid> { nodeDiscordantNote.Id }
        };

        // Create Tier 3 nodes
        var nodeEpicRecitation = new SpecializationNode
        {
            AbilityId = epicRecitation.Id,
            Ability = epicRecitation,
            Tier = 3,
            CostPP = 3,
            PositionX = 0,
            PositionY = 2,
            ParentNodeIds = new List<Guid> { nodeVerseOfValor.Id }
        };

        var nodeSilencingShriek = new SpecializationNode
        {
            AbilityId = silencingShriek.Id,
            Ability = silencingShriek,
            Tier = 3,
            CostPP = 3,
            PositionX = 1,
            PositionY = 2,
            ParentNodeIds = new List<Guid> { nodeCacophony.Id }
        };

        // Create Tier 4 Capstone
        var nodeTheFinalVerse = new SpecializationNode
        {
            AbilityId = theFinalVerse.Id,
            Ability = theFinalVerse,
            Tier = 4,
            CostPP = 5,
            PositionX = 0,
            PositionY = 3,
            ParentNodeIds = new List<Guid> { nodeEpicRecitation.Id, nodeSilencingShriek.Id }
        };

        spec.Nodes = new List<SpecializationNode>
        {
            nodeInspiringWord, nodeDiscordantNote, nodeEchoingChant,
            nodeVerseOfValor, nodeCacophony,
            nodeEpicRecitation, nodeSilencingShriek,
            nodeTheFinalVerse
        };

        // Set back-reference
        foreach (var node in spec.Nodes)
        {
            node.Specialization = spec;
            node.SpecializationId = spec.Id;
        }

        return spec;
    }
}
