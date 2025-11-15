using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.19: Seeds the database with existing v0.7-v0.18 specializations
/// Migrates hard-coded specializations to data-driven model
/// </summary>
public class DataSeeder
{
    private static readonly ILogger _log = Log.ForContext<DataSeeder>();
    private readonly SpecializationRepository _specializationRepo;
    private readonly AbilityRepository _abilityRepo;
    private readonly string _connectionString;

    public DataSeeder(string connectionString)
    {
        _connectionString = connectionString;
        _specializationRepo = new SpecializationRepository(connectionString);
        _abilityRepo = new AbilityRepository(connectionString);
    }

    /// <summary>
    /// Seed all existing specializations (BoneSetter, JotunReader, Skald, SkarHordeAspirant, IronBane, AtgeirWielder, Veiðimaðr)
    /// </summary>
    public void SeedExistingSpecializations()
    {
        _log.Information("Starting specialization data seeding");

        SeedBoneSetterSpecialization();
        SeedJotunReaderSpecialization();
        SeedSkaldSpecialization();
        SeedSkarHordeAspirantSpecialization();
        SeedIronBaneSpecialization();
        SeedAtgeirWielderSpecialization();
        SeedScrapTinkerSpecialization();

        // v0.24.1: Veiðimaðr (Hunter) specialization for Skirmisher
        var veidimadurSeeder = new VeidimadurSeeder(_connectionString);
        veidimadurSeeder.SeedVeidimadurSpecialization();

        // v0.24.2: Myrk-gengr (Shadow-Walker) specialization for Skirmisher
        var myrkgengrSeeder = new MyrkgengrSeeder(_connectionString);
        myrkgengrSeeder.SeedMyrkgengrSpecialization();

        // v0.25.1: Strandhogg (Glitch-Raider) specialization for Skirmisher
        var strandhoggSeeder = new StrandhoggSeeder(_connectionString);
        strandhoggSeeder.SeedStrandhoggSpecialization();

        _log.Information("Specialization data seeding completed successfully");
    }

    #region BoneSetter (ID: 1)

    private void SeedBoneSetterSpecialization()
    {
        _log.Information("Seeding BoneSetter specialization");

        var boneSetter = new SpecializationData
        {
            SpecializationID = 1,
            Name = "Bone-Setter",
            ArchetypeID = 2, // Adept
            PathType = "Coherent",
            MechanicalRole = "Healer / Sanity Anchor",
            PrimaryAttribute = "WITS",
            SecondaryAttribute = "FINESSE",
            Description = @"You are the indispensable combat medic and the anchor of sanity. While others fight monsters, you fight entropy—you stitch wounds, set bones, and talk comrades back from the brink of madness. Your healing is non-magical: it's science, anatomy, herbalism, and psychology.

            You prepare during downtime, craft medical supplies, and deploy them with precision during crisis. You are the quiet hero who keeps the party coherent enough to survive another day.

            In a world where magic is corrupted, you trust science, herbs, and steady hands. You're a pragmatist who understands that surviving the Blight means keeping your body intact AND your mind coherent.",
            Tagline = "Pragmatic Restoration - Restorer of Coherence",
            UnlockRequirements = new UnlockRequirements { MinLegend = 3, MaxCorruption = 100 },
            ResourceSystem = "Stamina + Consumable Items",
            TraumaRisk = "None",
            IconEmoji = "⚕️",
            PPCostToUnlock = 3,
            IsActive = true
        };

        _specializationRepo.Insert(boneSetter);

        // Tier 1 abilities
        SeedBoneSetterTier1();

        // Tier 2 abilities
        SeedBoneSetterTier2();

        // Tier 3 abilities
        SeedBoneSetterTier3();

        // Capstone
        SeedBoneSetterCapstone();

        _log.Information("BoneSetter seeding complete: 9 abilities");
    }

    private void SeedBoneSetterTier1()
    {
        // Field Medic (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 101,
            SpecializationID = 1,
            Name = "Field Medic",
            Description = "You are an expert at preparing medical supplies. Your kit is always ready, your hands always steady.",
            MechanicalSummary = "Bonus to Field Medicine crafting checks, start with free poultices, craft Masterwork items",
            TierLevel = 1,
            PPCost = 3, // v0.19.4: Updated to 3 PP
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: +1d10 bonus to Field Medicine crafting checks. Start expeditions with 3 [Healing Poultices]. Rank 2 (20 PP): +2d10 bonus. Start with 5 Poultices. Crafted items 20% chance to be [Masterwork] (heal 50% more). Rank 3: +3d10 bonus. Start with 7 Poultices + 2 Antidotes. Masterwork chance 35%. Can craft rare [Miracle Tinctures]"
        });

        // Mend Wound (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 102,
            SpecializationID = 1,
            Name = "Mend Wound",
            Description = "You quickly dress the wound, applying poultice with practiced efficiency. The healing begins.",
            MechanicalSummary = "Heal HP using poultice, cleanse debuffs at higher ranks",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Ally",
            ResourceCost = new AbilityResourceCost { Stamina = 35 },
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0, // Auto-success, consumes poultice
            HealingDice = 3,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Heal 3d8 + WITS HP. Consumes one [Healing Poultice]. Rank 2 (20 PP): Heal 4d8 + WITS HP. Costs 30 Stamina. If using [Masterwork Poultice]: +2d8 healing. Rank 3: Heal 5d8 + WITS HP. Also removes [Poisoned] or [Bleeding] (free cleanse)"
        });

        // Apply Tourniquet (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 103,
            SpecializationID = 1,
            Name = "Apply Tourniquet",
            Description = "With speed and precision, you stop the life-threatening blood loss. They'll live.",
            MechanicalSummary = "Remove [Bleeding], grant temporary Soak, immunity to bleeding at higher ranks",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Ally",
            ResourceCost = new AbilityResourceCost { Stamina = 30 },
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0, // Auto-success
            StatusEffectsRemoved = new List<string> { "Bleeding" },
            CooldownTurns = 2,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Instantly remove [Bleeding] status from target. Rank 2 (20 PP): Remove [Bleeding]. Also grant target +2 Soak for 2 turns (field bandages). Rank 3: Remove [Bleeding] + [Hemorrhaging]. Grant +3 Soak for 3 turns. Target immune to [Bleeding] for rest of combat"
        });
    }

    private void SeedBoneSetterTier2()
    {
        // Anatomical Insight (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 104,
            SpecializationID = 1,
            Name = "Anatomical Insight",
            Description = "You observe their anatomy and recognize the weak points. There—that's where to strike.",
            MechanicalSummary = "Apply [Vulnerable] debuff to organic enemies, increasing Physical damage taken",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Organic)",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0, // WITS check vs enemy
            StatusEffectsApplied = new List<string> { "Vulnerable" },
            CooldownTurns = 3,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: WITS check vs enemy. Success: Apply [Vulnerable] for 2 turns (+25% Physical damage taken). Rank 2 (20 PP): Vulnerable lasts 3 turns (+35% damage). Also reveals one enemy weakness/resistance. Rank 3: Vulnerable lasts 4 turns (+50% damage). Automatically succeeds vs [Bloodied] enemies"
        });

        // Administer Antidote (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 105,
            SpecializationID = 1,
            Name = "Administer Antidote",
            Description = "You administer the carefully prepared antidote. The toxins are neutralized.",
            MechanicalSummary = "Remove poison and disease effects, grant immunity at higher ranks",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Ally",
            ResourceCost = new AbilityResourceCost { Stamina = 30 },
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0, // Auto-success, consumes antidote
            StatusEffectsRemoved = new List<string> { "Poisoned", "Disease" },
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Remove one [Poisoned] or [Disease] effect. Consumes one [Common Antidote]. Rank 2 (20 PP): Remove [Poisoned], [Disease], and [Weakened]. Target gains +2 STURDINESS for 2 turns. Rank 3: Remove all poison/disease effects. Target immune to [Poisoned] for rest of combat"
        });

        // Triage (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 106,
            SpecializationID = 1,
            Name = "Triage",
            Description = "You understand battlefield medicine: treat the most grievous wounds first. Maximum efficiency.",
            MechanicalSummary = "Massive healing bonus to bloodied allies, grant temporary buffs",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: All healing abilities restore +25% HP when used on [Bloodied] allies (below 50% HP). Rank 2 (20 PP): +35% healing on Bloodied allies. Healing also grants target +1 Soak for 1 turn. Rank 3: +50% healing on Bloodied allies. When healing brings ally above 50% HP, they gain [Revitalized] (+2 to hit, 2 turns)"
        });
    }

    private void SeedBoneSetterTier3()
    {
        // Cognitive Realignment (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 107,
            SpecializationID = 1,
            Name = "Cognitive Realignment",
            Description = "Calming techniques, pressure points, smelling salts—you reboot their panicked mind.",
            MechanicalSummary = "Remove mental status effects, restore massive Psychic Stress, grant mental buffs",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Ally",
            ResourceCost = new AbilityResourceCost { Stamina = 45 },
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0, // Auto-success, consumes draught
            StatusEffectsRemoved = new List<string> { "Feared", "Panicked" },
            CooldownTurns = 3,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Remove 15 Psychic Stress from target. Consumes [Stabilizing Draught]. Rank 2 (20 PP): Remove 25 Stress. Also remove [Feared] or [Panicked] status effects. Rank 3: Remove 40 Stress. Remove all mental status effects. Grant [Focused] (+1 WILL, 3 turns)"
        });

        // Defensive Focus (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 108,
            SpecializationID = 1,
            Name = "Defensive Focus",
            Description = "When focused on saving another, you enter heightened awareness. You will not fall.",
            MechanicalSummary = "Gain Defense bonus after healing, extra Soak when allies wounded, resistance to mental effects",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: After using any healing ability, gain +2 Defense until end of turn. Rank 2 (20 PP): +3 Defense. Also gain +1 Soak while adjacent ally is below 50% HP (protective instinct). Rank 3: +4 Defense. You have advantage on saves vs [Fear] and [Stun] while healing allies"
        });
    }

    private void SeedBoneSetterCapstone()
    {
        // Miracle Worker
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 109,
            SpecializationID = 1,
            Name = "Miracle Worker",
            Description = "⭐ CAPSTONE: A complex procedure—stimulants, field surgery, sheer will. You bring them back from the brink.",
            MechanicalSummary = "Massive emergency heal, remove all debuffs, grant death protection and buffs",
            TierLevel = 4,
            PPCost = 6,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 24, RequiredAbilityIDs = new List<int> { 107, 108 } },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Ally ([Bloodied] - below 50% HP)",
            ResourceCost = new AbilityResourceCost { Stamina = 50 },
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0, // Auto-success, consumes Miracle Tincture
            HealingDice = 8,
            StatusEffectsRemoved = new List<string> { "Bleeding", "Poisoned", "Disease", "Vulnerable", "Weakened", "Stunned" },
            CooldownType = "Per Expedition",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Heal 8d10 + (WITS × 2) HP. Remove all status effects. Target cannot drop below 1 HP for 1 turn. Consumes 1 [Miracle Tincture]. Once per expedition. Rank 2 (20 PP): Heal 10d10 + (WITS × 2) HP. Grant [Invigorated] (+3 to all actions, 2 turns). Protected from death for 2 turns. Rank 3: Heal 12d10 + (WITS × 3) HP. Remove 30 Psychic Stress. Grant [Second Wind] (next ability costs 0, 1 use)"
        });
    }

    #endregion

    #region JotunReader (ID: 2)

    private void SeedJotunReaderSpecialization()
    {
        _log.Information("Seeding JotunReader specialization");

        var jotunReader = new SpecializationData
        {
            SpecializationID = 2,
            Name = "Jötun-Reader",
            ArchetypeID = 2, // Adept
            PathType = "Coherent", // v0.19.7: Updated from Heretical to Coherent
            MechanicalRole = "Controller / Utility Specialist",
            PrimaryAttribute = "WITS",
            SecondaryAttribute = "FINESSE",
            Description = @"You are the scholar-pathologist who reads the crash logs of a dead civilization. Where others see chaos, you see patterns. You translate error messages carved in ancient stone, identify structural flaws in corrupted war-machines, and speak fragments of command-line code that freeze enemies in logic conflicts.

            You don't repair the broken world—you document its death throes and turn that knowledge into tactical advantage. Every analysis of corrupted systems costs sanity. You apply clinical observation to reveal enemy weaknesses, communicate tactical advantages to allies, and weaponize forbidden knowledge as psychological attacks.

            The price? Every truth you uncover costs sanity. You are the ultimate force multiplier who contributes zero direct damage but dramatically increases party effectiveness.",
            Tagline = "Forensic Observation and Documentation - Weaponize Forbidden Knowledge",
            UnlockRequirements = new UnlockRequirements { MinLegend = 3, MaxCorruption = 100 },
            ResourceSystem = "Stamina + Psychic Stress",
            TraumaRisk = "High",
            IconEmoji = "🔍", // v0.19.7: Updated icon
            PPCostToUnlock = 3,
            IsActive = true
        };

        _specializationRepo.Insert(jotunReader);

        SeedJotunReaderTier1();
        SeedJotunReaderTier2();
        SeedJotunReaderTier3();
        SeedJotunReaderCapstone();

        _log.Information("JotunReader seeding complete: 9 abilities");
    }

    private void SeedJotunReaderTier1()
    {
        // Scholarly Acumen I (Passive) - 3 ranks
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 201,
            SpecializationID = 2,
            Name = "Scholarly Acumen I",
            Description = "Your mind is a finely honed instrument, constantly processing layers of forgotten history.",
            MechanicalSummary = "+2d10 bonus to WITS-based Investigate and System Bypass checks, increases to +4d10 at higher ranks",
            TierLevel = 1,
            PPCost = 3, // v0.19.7: Updated from 0 to 3
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: +2d10 bonus to WITS-based Investigate and System Bypass checks (lore, terminals, artifacts). Rank 2 (20 PP): +4d10 bonus (doubled). Translation/hacking time reduced 25%. Rank 3: +4d10 bonus. Auto-upgrade Success → Critical Success on Investigate checks. Mastery = intuitive leaps."
        });

        // Analyze Weakness (Active) - 3 ranks
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 202,
            SpecializationID = 2,
            Name = "Analyze Weakness",
            Description = "Clinical observation reveals structural flaws. You document weakness like a pathologist identifies cause of death.",
            MechanicalSummary = "WITS check to reveal enemy Resistances/Vulnerabilities, costs Psychic Stress",
            TierLevel = 1,
            PPCost = 3, // v0.19.7: Updated from 0 to 3
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 30, Stress = 5 },
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: WITS check. Success = reveal 1 Resistance + 1 Vulnerability. Critical = ALL Resistances/Vulnerabilities + AI hint. Costs 30 Stamina + 5 Psychic Stress. Rank 2 (20 PP): Stamina 25, Stress 3. Success reveals 2 Resistances + 2 Vulnerabilities (doubled). Critical also reveals special ability. Rank 3: Stamina 25, Stress 0 (mind adapted). Success = auto-Critical Success info. True Critical also applies [Analyzed] for 1 round. Can use as Free Action once per combat."
        });

        // Runic Linguistics (Passive) - 3 ranks
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 203,
            SpecializationID = 2,
            Name = "Runic Linguistics",
            Description = "You read the grammar of reality's operating system. You understand error messages in a dead language.",
            MechanicalSummary = "Translate Elder Futhark inscriptions, bypass puzzle gates, extrapolate missing text at higher ranks",
            TierLevel = 1,
            PPCost = 3, // v0.19.7: Updated from 0 to 3
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Can read and translate all non-magical Elder Futhark inscriptions (error logs, commands, warnings). Translation only—cannot execute. Rank 2 (20 PP): Translation instantaneous (no time). Can translate corrupted/fragmentary text (30-40% missing). Identify age and origin facility. Rank 3: Can translate ANY corruption level. Extrapolate missing sections (70-80% accuracy). Identify author/system by syntax. Once per day: Deep analysis reveals hidden subtext/encoded messages."
        });
    }

    private void SeedJotunReaderTier2()
    {
        // Exploit Design Flaw (Active) - 3 ranks
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 204,
            SpecializationID = 2,
            Name = "Exploit Design Flaw",
            Description = "Strike the left knee joint—actuator is damaged. Your tactical guidance turns allies into precision instruments.",
            MechanicalSummary = "Apply [Analyzed] debuff - all party attacks gain +2 to +4 Accuracy against target",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 35 },
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            StatusEffectsApplied = new List<string> { "Analyzed" },
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Apply [Analyzed] debuff for 2 rounds. All party attacks gain +2 Accuracy against target. Must have previously used Analyze Weakness on target. Rank 2 (20 PP): Stamina 35. [Analyzed] grants +3 Accuracy (increased) for 3 rounds (longer). Rank 3: Stamina 25. [Analyzed] grants +4 Accuracy for 4 rounds. Also adds +1d10 bonus damage to all attacks. No longer requires prior Analysis (can identify weaknesses mid-combat)."
        });

        // Navigational Bypass (Active) - 3 ranks
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 205,
            SpecializationID = 2,
            Name = "Navigational Bypass",
            Description = "The trigger mechanism is corroded on the western edge. Distribute weight evenly—sensor won't register threshold pressure.",
            MechanicalSummary = "Analyze trap construction, grant party bonus dice to bypass checks",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Environmental Hazard",
            ResourceCost = new AbilityResourceCost { Stamina = 30 },
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Analyze trap construction. Grant party +1d10 to next bypass check (avoid/disarm hazard). Rank 2 (20 PP): Stamina 20. Grant +2d10. Critical Success on bypass grants retry on different hazard within 1 hour. Rank 3: Stamina 20. Grant +3d10 to next 2 bypass checks. Critical Success permanently disables hazard. Can use in combat for environmental hazards."
        });

        // Structural Insight (Passive) - 3 ranks
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 206,
            SpecializationID = 2,
            Name = "Structural Insight",
            Description = "Support beams compromised. Eastern wall provides solid cover—load-bearing, reinforced. Center of room will collapse.",
            MechanicalSummary = "Automatically detect hazards, cover quality, and structural weak points",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Automatically detect [Structurally Unstable] features, [Cover] quality, hazards upon entering area. Current room only. Rank 2 (20 PP): Detection extends to adjacent visible areas. Assess exact integrity percentages. Once per combat: Warning shout as Free Action grants allies +2d10 to defensive checks vs hazard. Rank 3: Detection extends to entire dungeon floor/zone. Once per combat: Call out structural weak point for controlled collapse (Standard Action + ally attack). Auto-warn of ambush positions. Party gains +1 Defense in analyzed areas."
        });
    }

    private void SeedJotunReaderTier3()
    {
        // Calculated Triage (Passive) - 3 ranks
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 207,
            SpecializationID = 2,
            Name = "Calculated Triage",
            Description = "Apply pressure to brachial artery first. Follow wound track with the applicator. Your clinical guidance optimizes treatment.",
            MechanicalSummary = "Nearby allies gain increased effectiveness from healing consumables, up to +50% at Rank 3",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Healing consumables used on allies adjacent to Jötun-Reader heal for +25% more. Rank 2 (20 PP): (Same as Rank 1—Tier 3 abilities start at this rank). Rank 3: Healing bonus +50%. Range increased to 2 squares. Healing also removes one minor debuff ([Bleeding], [Poisoned], [Disoriented]). Once per combat: Activate 'Field Hospital' zone (3x3, 3 rounds): +75% healing, +2 Resolve Checks, healing costs half action."
        });

        // The Unspoken Truth (Active) - 3 ranks
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 208,
            SpecializationID = 2,
            Name = "The Unspoken Truth",
            Description = "Your 'god' is ERROR CODE 0x4A7F. You worship a crash log. The truth shatters their worldview.",
            MechanicalSummary = "Weaponize forbidden knowledge as psychological attack - opposed WITS vs WILL check",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Intelligent Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            IgnoresArmor = true,
            StatusEffectsApplied = new List<string> { "Disoriented" },
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Opposed WITS vs WILL check. Success = [Disoriented] for 2 rounds. Weaponize forbidden lore as psychological attack. Rank 2 (20 PP): (Same as Rank 1). Rank 3: Stamina 30. Success = [Disoriented] 3 rounds + 5-7 Psychic Stress to target. Critical = also [Shaken] 2 rounds + 10-12 Stress. Target must pass WILL check or become [Fixated] on Jötun-Reader 1 round. Boss/Elite: May trigger narrative consequences (flee, parley, identity crisis)."
        });
    }

    private void SeedJotunReaderCapstone()
    {
        // Architect of the Silence (Capstone) - 3 ranks
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 209,
            SpecializationID = 2,
            Name = "Architect of the Silence",
            Description = "⭐ CAPSTONE: PRIORITY OVERRIDE: TIWAZ PROTOCOL ALPHA. CEASE HOSTILE OPERATIONS. The machine's logic wars with corrupted directives.",
            MechanicalSummary = "Speak command-line code to apply [Seized] status to Jötun-Forged/Undying enemies - complete paralysis",
            TierLevel = 4,
            PPCost = 6,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 24, RequiredAbilityIDs = new List<int> { 207, 208 } },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Jötun-Forged or Undying Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 60, Stress = 15 },
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            StatusEffectsApplied = new List<string> { "Seized" },
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            CooldownTurns = 1,
            CooldownType = "Per Combat",
            IsActive = true,
            Notes = "Rank 1: Speak original command-line code fragments. Target makes high-DC WILL check. Failure = [Seized] for 1 round (total paralysis). Costs 60 Stamina + Large Psychic Stress (15-20). Once per combat. Only works on Jötun-Forged or Undying enemies. Rank 2 (20 PP): (Same as Rank 1). Rank 3: Stress reduced to Moderate (10-15). On Success (not just Failure), target suffers [Disoriented] 1 round. On Failure = [Seized] 2 rounds. If target <50% HP: Can make effect automatic (no save), but locks ability for entire day. Passive: Auto-Critical analyze ALL Jötun-Forged/Undying at combat start (no action)."
        });
    }

    #endregion

    #region Skald (ID: 3)

    private void SeedSkaldSpecialization()
    {
        _log.Information("Seeding Skald specialization");

        var skald = new SpecializationData
        {
            SpecializationID = 3,
            Name = "Skald",
            ArchetypeID = 2, // Adept
            PathType = "Coherent",
            MechanicalRole = "Buffer/Debuffer",
            PrimaryAttribute = "WILL",
            SecondaryAttribute = "WITS",
            Description = "Warrior-poet who wields structured narrative as weapon. Maintains performances for battlefield control.",
            Tagline = "Provide party-wide buffs/debuffs through sustained performances",
            UnlockRequirements = new UnlockRequirements { MinLegend = 0, MaxCorruption = 100 },
            ResourceSystem = "Stamina",
            TraumaRisk = "Low",
            IconEmoji = "🎵",
            PPCostToUnlock = 3,
            IsActive = true
        };

        _specializationRepo.Insert(skald);

        SeedSkaldTier1();
        SeedSkaldTier2();
        SeedSkaldTier3();
        SeedSkaldCapstone();

        _log.Information("Skald seeding complete: 9 abilities");
    }

    private void SeedSkaldTier1()
    {
        // Oral Tradition I
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 301,
            SpecializationID = 3,
            Name = "Oral Tradition I",
            Description = "[PASSIVE] +1d to Rhetoric and lore checks. Your words carry weight.",
            TierLevel = 1,
            PPCost = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            MechanicalSummary = "+1d to Rhetoric and lore",
            MaxRank = 1,
            IsActive = true
        });

        // Saga of Courage
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 302,
            SpecializationID = 3,
            Name = "Saga of Courage",
            Description = "[PERFORMANCE] All allies immune to [Feared] and gain +1d to WILL Resolve checks. Duration: WILL score rounds. Cannot take other actions while performing.",
            TierLevel = 1,
            PPCost = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Performance",
            TargetType = "All Allies",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
            AttributeUsed = "will",
            BonusDice = 2,
            SuccessThreshold = 2,
            MechanicalSummary = "Allies immune to [Feared], +1d WILL checks",
            MaxRank = 3,
            CostToRank2 = 5,
            IsActive = true
        });

        // Dirge of Defeat
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 303,
            SpecializationID = 3,
            Name = "Dirge of Defeat",
            Description = "[PERFORMANCE] All enemies suffer -2 Accuracy penalty. Duration: WILL score rounds. Cannot take other actions while performing.",
            TierLevel = 1,
            PPCost = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Performance",
            TargetType = "All Enemies",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
            AttributeUsed = "will",
            BonusDice = 2,
            SuccessThreshold = 2,
            MechanicalSummary = "Enemies get -2 Accuracy",
            MaxRank = 3,
            CostToRank2 = 5,
            IsActive = true
        });
    }

    private void SeedSkaldTier2()
    {
        // Rousing Verse
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 304,
            SpecializationID = 3,
            Name = "Rousing Verse",
            Description = "Restore 2d6 Stamina to target ally with a brief, energizing verse. Not a performance - immediate effect.",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Ally",
            ResourceCost = new AbilityResourceCost { Stamina = 20 },
            AttributeUsed = "will",
            BonusDice = 2,
            SuccessThreshold = 2,
            MechanicalSummary = "Restore 2d6 Stamina to ally",
            HealingDice = 2,
            MaxRank = 3,
            CostToRank2 = 5,
            IsActive = true
        });

        // Song of Silence
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 305,
            SpecializationID = 3,
            Name = "Song of Silence",
            Description = "Apply [Silenced] status to enemy caster/channeler. Interrupted performances end immediately. WILL vs enemy WILL.",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 35 },
            AttributeUsed = "will",
            BonusDice = 2,
            SuccessThreshold = 3,
            MechanicalSummary = "Apply [Silenced] - prevents casting/performances",
            StatusEffectsApplied = new List<string> { "Silenced" },
            MaxRank = 3,
            CostToRank2 = 5,
            IsActive = true
        });

        // Enduring Performance
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 306,
            SpecializationID = 3,
            Name = "Enduring Performance",
            Description = "[PASSIVE] All your [PERFORMANCE] abilities last +2 additional rounds. Longer sagas, deeper impact.",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            MechanicalSummary = "Performances last +2 rounds",
            MaxRank = 1,
            IsActive = true
        });
    }

    private void SeedSkaldTier3()
    {
        // Lay of the Iron Wall
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 307,
            SpecializationID = 3,
            Name = "Lay of the Iron Wall",
            Description = "[PERFORMANCE] Front row allies gain +2 Soak (damage reduction). Duration: WILL score rounds. Shield-song of ancient defenders.",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Performance",
            TargetType = "Front Row Allies",
            ResourceCost = new AbilityResourceCost { Stamina = 50 },
            AttributeUsed = "will",
            BonusDice = 2,
            SuccessThreshold = 2,
            MechanicalSummary = "Front row gets +2 Soak",
            MaxRank = 3,
            CostToRank2 = 5,
            IsActive = true
        });

        // Heart of the Clan
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 308,
            SpecializationID = 3,
            Name = "Heart of the Clan",
            Description = "[PASSIVE] Allies in the same row as you gain +1d to defensive Resolve checks (vs fear, stun, etc.). Your presence bolsters their will.",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            MechanicalSummary = "Allies in your row get +1d to defensive Resolve",
            MaxRank = 1,
            IsActive = true
        });
    }

    private void SeedSkaldCapstone()
    {
        // Saga of the Einherjar
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 309,
            SpecializationID = 3,
            Name = "Saga of the Einherjar",
            Description = "⭐ CAPSTONE: [PERFORMANCE] All allies gain [Inspired] (+3 damage dice) and 2d6 temporary HP. Duration: WILL score rounds. When performance ends, you suffer 10 Psychic Stress from the effort.",
            TierLevel = 4,
            PPCost = 6,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 24, RequiredAbilityIDs = new List<int> { 307, 308 } },
            AbilityType = "Active",
            ActionType = "Performance",
            TargetType = "All Allies",
            ResourceCost = new AbilityResourceCost { Stamina = 70 },
            AttributeUsed = "will",
            BonusDice = 3,
            SuccessThreshold = 2,
            MechanicalSummary = "Allies get [Inspired] +3d damage + 2d6 temp HP, 10 Stress on end",
            StatusEffectsApplied = new List<string> { "Inspired" },
            HealingDice = 2,
            MaxRank = 3,
            CostToRank2 = 5,
            CooldownType = "Per Combat",
            IsActive = true,
            Notes = "Costs 10 Stress when performance ends"
        });
    }

    #endregion

    #region SkarHordeAspirant (ID: 10)

    /// <summary>
    /// v0.19.1: Seeds Skar-Horde Aspirant specialization for Warrior
    /// Self-destructive melee DPS that trades humanity for devastating armor-bypassing power
    /// </summary>
    private void SeedSkarHordeAspirantSpecialization()
    {
        _log.Information("Seeding Skar-Horde Aspirant specialization");

        var skarHordeAspirant = new SpecializationData
        {
            SpecializationID = 10,
            Name = "Skar-Horde Aspirant",
            ArchetypeID = 1, // Warrior
            PathType = "Heretical",
            MechanicalRole = "Melee DPS / Armor-Breaker",
            PrimaryAttribute = "MIGHT",
            SecondaryAttribute = "",
            Description = @"The Skar-Horde Aspirant embodies the heretical philosophy of achieving power through savage, willful self-mutilation. You have ritualistically replaced your hand with a modular weapon-stump augment, trading humanity for devastating combat prowess. Build Savagery by fighting in melee, then unleash armor-bypassing attacks that ignore all defenses. Every strike pushes you closer to madness, but power is worth any price. You are no longer human. You are a weapon.",
            Tagline = "The Warrior Who Bleeds for Power",
            UnlockRequirements = new UnlockRequirements { MinLegend = 5, MaxCorruption = 100 },
            ResourceSystem = "Stamina + Savagery",
            TraumaRisk = "Extreme",
            IconEmoji = "⚔️",
            PPCostToUnlock = 3,
            IsActive = true
        };

        _specializationRepo.Insert(skarHordeAspirant);

        // Seed all ability tiers
        SeedSkarHordeTier1();
        SeedSkarHordeTier2();
        SeedSkarHordeTier3();
        SeedSkarHordeCapstone();

        _log.Information("Skar-Horde Aspirant seeding complete: 9 abilities");
    }

    private void SeedSkarHordeTier1()
    {
        // Tier 1 - Ability 1: Heretical Augmentation (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1001,
            SpecializationID = 10,
            Name = "Heretical Augmentation",
            Description = "You have performed the ritual of replacement, carving away weakness and grafting brutal functionality. Your hand is gone. Your weapon-stump remains.",
            MechanicalSummary = "Unlocks [Augmentation] slot (replaces weapon slot). Enables crafting and installing augments at workbenches.",
            TierLevel = 1,
            PPCost = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0, // Capstone rank
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 2 (20 PP): Swap augments in 1 action. Rank 3 (Capstone): Augments gain +1 to all damage dice"
        });

        // Tier 1 - Ability 2: Savage Strike (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1002,
            SpecializationID = 10,
            Name = "Savage Strike",
            Description = "A brutal, straightforward blow with your augmented stump. Savage. Effective. Yours.",
            MechanicalSummary = "MIGHT-based melee attack; damage type depends on augment; generates Savagery on hit",
            TierLevel = 1,
            PPCost = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Melee)",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
            AttributeUsed = "might",
            BonusDice = 2,
            SuccessThreshold = 2,
            DamageDice = 2,
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: Generates 15 Savagery. Rank 2 (20 PP): Generates 20 Savagery, costs 35 Stamina. Rank 3: Generates 25 Savagery, apply [Bleeding] on crit"
        });

        // Tier 1 - Ability 3: Horrific Form (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1003,
            SpecializationID = 10,
            Name = "Horrific Form",
            Description = "Your self-mutilation is deeply unsettling. Good. Let them see what you have become.",
            MechanicalSummary = "Enemies that hit you in melee have chance to become [Feared]",
            TierLevel = 1,
            PPCost = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            StatusEffectsApplied = new List<string> { "Feared" },
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: 25% chance to Fear melee attackers for 1 turn. Rank 2 (20 PP): 35% chance, Feared enemies deal -2 damage to you. Rank 3: 50% chance, gain 5 Savagery when enemy becomes Feared"
        });
    }

    private void SeedSkarHordeTier2()
    {
        // Tier 2 - Ability 4: Grievous Wound (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1004,
            SpecializationID = 10,
            Name = "Grievous Wound",
            Description = "You carve a wound that armor cannot protect against. A wound that does not close. A wound that reminds them what mortality means.",
            MechanicalSummary = "Deals Physical damage and applies [Grievous Wound] DoT that bypasses all Soak",
            TierLevel = 2,
            PPCost = 4,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Melee)",
            ResourceCost = new AbilityResourceCost { Stamina = 45 },
            AttributeUsed = "might",
            BonusDice = 2,
            SuccessThreshold = 2,
            DamageDice = 3,
            IgnoresArmor = false,
            StatusEffectsApplied = new List<string> { "Grievous Wound" },
            CooldownTurns = 2,
            CooldownType = "Per Combat",
            IsActive = true,
            Notes = "Rank 1: 3d8 damage + [Grievous Wound] (1d10/turn, bypasses Soak, 3 turns). Rank 2 (20 PP): 4d8 damage, lasts 4 turns. Rank 3: 1d12/turn, refund 20 Savagery if target dies. Requires 30 Savagery"
        });

        // Tier 2 - Ability 5: Impaling Spike (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1005,
            SpecializationID = 10,
            Name = "Impaling Spike",
            Description = "You slam your spike through foot, pinning them to the broken earth. They are not going anywhere.",
            MechanicalSummary = "Deals damage and Roots target; requires Piercing-type augment",
            TierLevel = 2,
            PPCost = 4,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Melee)",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
            AttributeUsed = "might",
            BonusDice = 2,
            SuccessThreshold = 2,
            DamageDice = 2,
            StatusEffectsApplied = new List<string> { "Rooted" },
            CooldownTurns = 3,
            CooldownType = "Per Combat",
            IsActive = true,
            Notes = "Rank 1: 2d10 damage, 75% chance [Rooted] 2 turns. Rank 2 (20 PP): 90% Root chance, 3 turn duration. Rank 3: 100% Root chance, +2 to hit Rooted target. Requires 25 Savagery"
        });

        // Tier 2 - Ability 6: Pain Fuels Savagery (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1006,
            SpecializationID = 10,
            Name = "Pain Fuels Savagery",
            Description = "Every wound is fuel. Every blow against you is a gift. Pain is just another resource.",
            MechanicalSummary = "Generate Savagery when taking damage",
            TierLevel = 2,
            PPCost = 4,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: Generate Savagery = 10% of damage taken (max 20/hit). Rank 2 (20 PP): 15% of damage (max 25/hit). Rank 3: 20% of damage (max 30/hit), gain +1 Soak per 25 Savagery"
        });
    }

    private void SeedSkarHordeTier3()
    {
        // Tier 3 - Ability 7: Overcharged Piston Slam (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1007,
            SpecializationID = 10,
            Name = "Overcharged Piston Slam",
            Description = "Superheated steam vents. Pistons compress. And then—impact. A concussive blast that reduces bone to powder.",
            MechanicalSummary = "Massive damage + Stun; requires Blunt-type augment",
            TierLevel = 3,
            PPCost = 5,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Melee)",
            ResourceCost = new AbilityResourceCost { Stamina = 55 },
            AttributeUsed = "might",
            BonusDice = 3,
            SuccessThreshold = 2,
            DamageDice = 6,
            StatusEffectsApplied = new List<string> { "Stunned" },
            CooldownTurns = 4,
            CooldownType = "Per Combat",
            IsActive = true,
            Notes = "Rank 1: 6d10 damage, 60% chance [Stunned] 1 turn. Rank 2 (20 PP): 7d10 damage, 75% Stun chance. Rank 3: 100% Stun, next attack deals double damage. Requires 40 Savagery"
        });

        // Tier 3 - Ability 8: The Price of Power (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1008,
            SpecializationID = 10,
            Name = "The Price of Power",
            Description = "The rush of transhuman power is intoxicating. The whispers in your mind grow louder. You do not care. Power is worth any price.",
            MechanicalSummary = "Massively increased Savagery generation, but gain Psychic Stress when generating Savagery",
            TierLevel = 3,
            PPCost = 5,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: +50% Savagery generation, gain 1 Stress per 10 Savagery generated. Rank 2 (20 PP): +75% Savagery generation. Rank 3: +100% Savagery (double), 1 Stress per 15 Savagery"
        });
    }

    private void SeedSkarHordeCapstone()
    {
        // Capstone - Ability 9: Monstrous Apotheosis (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1009,
            SpecializationID = 10,
            Name = "Monstrous Apotheosis",
            Description = "You give in completely. The whispers become a roar. Your augment screams with power. You are no longer human. You are a weapon. You are inevitable.",
            MechanicalSummary = "Enter [Apotheosis] state: free Savage Strikes, enhanced Grievous Wound, immune to Fear/Stun, massive Stress penalty after",
            TierLevel = 4,
            PPCost = 6,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites
            {
                RequiredPPInTree = 24,
                RequiredAbilityIDs = new List<int> { 1007, 1008 }
            },
            AbilityType = "Active",
            ActionType = "Bonus Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost { Stamina = 20 },
            StatusEffectsApplied = new List<string> { "Apotheosis" },
            CooldownTurns = 999,
            CooldownType = "Once Per Combat",
            IsActive = true,
            Notes = "Rank 1: 3 turns: Savage Strike costs 0, Grievous Wound applies [Bleeding], immune Fear/Stun, 30 Stress after. Rank 2 (20 PP): 4 turn duration, 25 Stress penalty. Rank 3: +25% all damage, 20 Stress, can end early to avoid penalty. Requires 75 Savagery"
        });
    }

    #endregion

    #region Iron-Bane (ID: 11)

    /// <summary>
    /// v0.19.2: Seeds Iron-Bane specialization for Warrior
    /// Zealous purifier who destroys corrupted machines and Undying with knowledge and holy fire
    /// </summary>
    private void SeedIronBaneSpecialization()
    {
        _log.Information("Seeding Iron-Bane specialization");

        var ironBane = new SpecializationData
        {
            SpecializationID = 11,
            Name = "Iron-Bane",
            ArchetypeID = 1, // Warrior
            PathType = "Coherent",
            MechanicalRole = "Anti-Mechanical Specialist / Controller",
            PrimaryAttribute = "WILL",
            SecondaryAttribute = "MIGHT",
            Description = @"You are a warrior-scholar who has studied the Undying and their mechanical corruption. Where others see invincible foes, you see exploitable weaknesses in their code. You wield flame and faith to purge the abominations, using your knowledge of pre-Glitch technology to identify and destroy critical systems.

            You are the debugger, the antivirus, the purifier who turns the Blight's own corrupted machinery against itself. Through study and conviction, you have become the deadliest weapon against the mechanical horrors that plague the world.

            Your power comes not from brute force, but from understanding. Every Undying weakness memorized. Every mechanical schematic analyzed. Every corrupted system waiting to be shut down.",
            Tagline = "Knowledge is the Deadliest Weapon",
            UnlockRequirements = new UnlockRequirements { MinLegend = 3 },
            ResourceSystem = "Stamina + Righteous Fervor",
            TraumaRisk = "Low",
            IconEmoji = "🔥",
            PPCostToUnlock = 3,
            IsActive = true
        };

        _specializationRepo.Insert(ironBane);

        // Seed all ability tiers
        SeedIronBaneTier1();
        SeedIronBaneTier2();
        SeedIronBaneTier3();
        SeedIronBaneCapstone();

        _log.Information("Iron-Bane seeding complete: 9 abilities");
    }

    private void SeedIronBaneTier1()
    {
        // Tier 1 - Ability 1: Scholar of Corruption (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1101,
            SpecializationID = 11,
            Name = "Scholar of Corruption",
            Description = "You have studied the schematics of the Undying, memorized their corrupted code. Where others see invincible foes, you see exploitable bugs.",
            MechanicalSummary = "Observe enemies to reveal type, resistances, and vulnerabilities; auto-observe at combat start",
            TierLevel = 1,
            PPCost = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Single Enemy",
            ResourceCost = new AbilityResourceCost(),
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: Observe enemy (Free Action) reveals type, resistances, vulnerabilities. Mechanical/Undying reveal 1 extra weakness. Rank 2 (20 PP): Observing grants 10 Fervor. Auto-observe all enemies at combat start. Rank 3: See enemy HP values. Mechanical/Undying below 30% HP marked [Critical Failure] (guaranteed crit)"
        });

        // Tier 1 - Ability 2: Purifying Flame (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1102,
            SpecializationID = 11,
            Name = "Purifying Flame",
            Description = "Holy fire cleanses corrupted iron. Your flame burns hotter against the abominations.",
            MechanicalSummary = "WILL-based Fire attack with massive bonus damage vs Mechanical/Undying; generates Righteous Fervor",
            TierLevel = 1,
            PPCost = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 35 },
            AttributeUsed = "will",
            BonusDice = 2,
            SuccessThreshold = 2,
            DamageDice = 2,
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: 2d8 Fire damage. Vs Mechanical/Undying: +2d6 damage, generate 15 Fervor. Rank 2 (20 PP): +3d6 vs targets, generate 20 Fervor, costs 30 Stamina. Rank 3: +4d6 vs targets, generate 25 Fervor, apply [Burning] 1d6/turn for 3 turns"
        });

        // Tier 1 - Ability 3: Weakness Exploiter (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1103,
            SpecializationID = 11,
            Name = "Weakness Exploiter",
            Description = "Every system has a flaw. Every machine has a breaking point. You know exactly where to strike.",
            MechanicalSummary = "Massive damage bonus against enemies identified by Scholar of Corruption",
            TierLevel = 1,
            PPCost = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: +25% damage vs enemies identified by Scholar of Corruption. Rank 2 (20 PP): +35% damage, +1 to hit. Rank 3: +50% damage. Critical hits deal triple damage instead of double"
        });
    }

    private void SeedIronBaneTier2()
    {
        // Tier 2 - Ability 4: System Shutdown (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1104,
            SpecializationID = 11,
            Name = "System Shutdown",
            Description = "You strike at the central processor, the corrupted core. Their systems crash. They stand frozen, helpless.",
            MechanicalSummary = "Fire damage with Stun effect; only affects Mechanical/Undying enemies",
            TierLevel = 2,
            PPCost = 4,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Mechanical/Undying)",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
            AttributeUsed = "will",
            BonusDice = 3,
            SuccessThreshold = 2,
            DamageDice = 3,
            StatusEffectsApplied = new List<string> { "Stunned" },
            CooldownTurns = 3,
            CooldownType = "Per Combat",
            IsActive = true,
            Notes = "Rank 1: 3d10 Fire damage. WILL save DC 15 or [Stunned] 2 turns. Only affects Mechanical/Undying. Requires 30 Fervor. Rank 2 (20 PP): 4d10 damage, DC 17. Failed save also gives -3 to all actions for rest of combat. Rank 3: 5d10 damage, 3 turn Stun. Failed save adds [System Malfunction] (30% skip turn each round)"
        });

        // Tier 2 - Ability 5: Critical Strike (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1105,
            SpecializationID = 11,
            Name = "Critical Strike",
            Description = "You've identified the critical failure point. One precise strike and their entire system collapses.",
            MechanicalSummary = "Guaranteed critical hit vs Mechanical/Undying with execute at low HP",
            TierLevel = 2,
            PPCost = 4,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Mechanical/Undying)",
            ResourceCost = new AbilityResourceCost { Stamina = 45 },
            AttributeUsed = "will",
            BonusDice = 4,
            SuccessThreshold = 2,
            DamageDice = 4,
            CooldownTurns = 4,
            CooldownType = "Per Combat",
            IsActive = true,
            Notes = "Rank 1: Guaranteed crit vs Mechanical/Undying. 4d8 Fire + Weakness Exploiter bonuses. Requires 25 Fervor. Rank 2 (20 PP): 5d8 damage. Vs Mechanical/Undying: target loses 1 action next turn. Rank 3: 6d8 damage. Vs Mechanical/Undying below 40% HP: instant death (execute)"
        });

        // Tier 2 - Ability 6: Flame Ward (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1106,
            SpecializationID = 11,
            Name = "Flame Ward",
            Description = "You are wreathed in holy flame. The corrupted dare not touch you. Those who try burn.",
            MechanicalSummary = "Fire resistance and retaliation damage against melee attackers",
            TierLevel = 2,
            PPCost = 4,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: 50% Fire resistance. Mechanical/Undying melee attackers take 1d6 Fire damage. Rank 2 (20 PP): 75% Fire resistance. 1d8 retaliation. +2 Soak vs Mechanical/Undying attacks. Rank 3: Fire immunity. 1d10 retaliation. Generate 10 Fervor when hit by Mechanical/Undying"
        });
    }

    private void SeedIronBaneTier3()
    {
        // Tier 3 - Ability 7: Purging Flame (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1107,
            SpecializationID = 11,
            Name = "Purging Flame",
            Description = "A wave of cleansing fire washes over the battlefield. Corrupted metal screams as it melts.",
            MechanicalSummary = "AoE Fire attack that devastates Mechanical/Undying enemies with persistent Burning",
            TierLevel = 3,
            PPCost = 5,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "All Enemies (Front Row)",
            ResourceCost = new AbilityResourceCost { Stamina = 55 },
            AttributeUsed = "will",
            BonusDice = 4,
            SuccessThreshold = 2,
            DamageDice = 4,
            StatusEffectsApplied = new List<string> { "Burning" },
            CooldownTurns = 5,
            CooldownType = "Per Combat",
            IsActive = true,
            Notes = "Rank 1: 4d8 Fire to all. Mechanical/Undying take double, [Burning] 2d6/turn for 3 turns. Requires 40 Fervor. Rank 2 (20 PP): 5d8 damage. Burning lasts 4 turns, cannot be cleansed from Mechanical/Undying. Rank 3: 6d8 damage. Mechanical/Undying also [Vulnerable] +50% damage taken for 2 turns"
        });

        // Tier 3 - Ability 8: Righteous Conviction (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1108,
            SpecializationID = 11,
            Name = "Righteous Conviction",
            Description = "Your faith is unshakeable. Your purpose is clear. The corrupted will fall.",
            MechanicalSummary = "Massively increased Fervor generation and near-perfect accuracy vs Mechanical/Undying",
            TierLevel = 3,
            PPCost = 5,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: +50% Righteous Fervor generation. +2 WILL vs Psychic/Corruption saves. Rank 2 (20 PP): +75% Fervor. +3 WILL. Attacks vs Mechanical/Undying cannot miss (95% min hit chance). Rank 3: +100% Fervor (double). +5 WILL. Defeating Mechanical/Undying refunds 50% ability costs"
        });
    }

    private void SeedIronBaneCapstone()
    {
        // Capstone - Ability 9: Divine Purge (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1109,
            SpecializationID = 11,
            Name = "Divine Purge",
            Description = "You channel every lesson, every moment of study, into one perfect strike. This is not combat. This is deletion.",
            MechanicalSummary = "Ultimate purge that can instantly destroy Mechanical/Undying enemies; spreads fear and destruction",
            TierLevel = 4,
            PPCost = 6,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites
            {
                RequiredPPInTree = 24,
                RequiredAbilityIDs = new List<int> { 1107, 1108 }
            },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Mechanical/Undying)",
            ResourceCost = new AbilityResourceCost { Stamina = 60 },
            AttributeUsed = "will",
            BonusDice = 5,
            SuccessThreshold = 3,
            DamageDice = 10,
            StatusEffectsApplied = new List<string> { "Feared" },
            CooldownTurns = 999,
            CooldownType = "Once Per Combat",
            IsActive = true,
            Notes = "Rank 1: 10d10 Fire. WILL DC 18 or instant death. On save: double damage + Stun 2 turns. Requires 75 Fervor. Rank 2 (20 PP): 12d10, DC 20. Destroyed enemies explode for 6d6 Fire AoE. Rank 3: 15d10, DC 22. Success still death (but gets death save). Destroy causes Fear on all Mechanical/Undying 3 turns"
        });
    }

    #endregion

    #region Atgeir-wielder (ID: 12)

    /// <summary>
    /// v0.19.3: Seeds Atgeir-wielder specialization for Warrior
    /// Formation master who controls the battlefield through reach, forced movement, and defensive anchoring
    /// </summary>
    private void SeedAtgeirWielderSpecialization()
    {
        _log.Information("Seeding Atgeir-wielder specialization");

        var atgeirWielder = new SpecializationData
        {
            SpecializationID = 12,
            Name = "Atgeir-wielder",
            ArchetypeID = 1, // Warrior
            PathType = "Coherent",
            MechanicalRole = "Battlefield Controller / Formation Anchor",
            PrimaryAttribute = "MIGHT",
            SecondaryAttribute = "WITS",
            Description = @"You are the disciplined hoplite, the master of formation warfare. Wielding a long polearm, you command the space around you with tactical precision. Your [Reach] allows you to strike from safety while your Push and Pull effects shatter enemy formations.

            You are the immovable anchor that holds the line, the thinking warrior who controls where battles happen. You don't fight with rage—you fight with discipline, leverage, and perfect positioning.

            In a chaotic, glitching world, you impose order through superior reach, forced movement, and formation mastery. The polearm is logic made physical.",
            Tagline = "Tactical discipline — control the battlefield",
            UnlockRequirements = new UnlockRequirements { MinLegend = 3 },
            ResourceSystem = "Stamina",
            TraumaRisk = "None",
            IconEmoji = "⚔️",
            PPCostToUnlock = 3,
            IsActive = true
        };

        _specializationRepo.Insert(atgeirWielder);

        // Seed all ability tiers
        SeedAtgeirWielderTier1();
        SeedAtgeirWielderTier2();
        SeedAtgeirWielderTier3();
        SeedAtgeirWielderCapstone();

        _log.Information("Atgeir-wielder seeding complete: 9 abilities");
    }

    private void SeedAtgeirWielderTier1()
    {
        // Tier 1 - Ability 1: Formal Training (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1201,
            SpecializationID = 12,
            Name = "Formal Training",
            Description = "Your formal training instills deep physical and mental discipline, allowing you to remain focused amid chaos.",
            MechanicalSummary = "Increased Stamina regeneration and resistance to disorientation effects",
            TierLevel = 1,
            PPCost = 3,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: +5 Stamina regeneration per turn. +1d10 to Resolve checks vs [Stagger]. Rank 2 (20 PP): +7 Stamina regen. +1d10 vs [Stagger] and [Disoriented]. Rank 3: +10 Stamina regen. +2d10 vs [Stagger] and [Disoriented]. +1 WITS."
        });

        // Tier 1 - Ability 2: Skewer (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1202,
            SpecializationID = 12,
            Name = "Skewer",
            Description = "A precise, powerful thrust designed to exploit your weapon's length. Strike from tactical safety.",
            MechanicalSummary = "MIGHT-based Physical attack with [Reach] - can attack front row from back row",
            TierLevel = 1,
            PPCost = 3,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (front row)",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
            AttributeUsed = "might",
            BonusDice = 2,
            SuccessThreshold = 2,
            DamageDice = 2,
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: 2d8 Physical. [Reach] - Can attack front row from back row. Rank 2 (20 PP): Costs 35 Stamina. +1d6 damage (2d8+1d6). Rank 3: +2d6 damage (2d8+2d6). On crit: apply [Bleeding] for 2 turns."
        });

        // Tier 1 - Ability 3: Disciplined Stance (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1203,
            SpecializationID = 12,
            Name = "Disciplined Stance",
            Description = "You plant your feet, becoming an anchor of stability. This line will not be broken.",
            MechanicalSummary = "Defensive stance providing massive Soak and immunity to forced movement; cannot move while active",
            TierLevel = 1,
            PPCost = 3,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Bonus Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost { Stamina = 30 },
            CooldownTurns = 3,
            CooldownType = "Standard",
            IsActive = true,
            Notes = "Rank 1: +4 Soak. +3 bonus dice vs [Push]/[Pull]. Cannot move while active. Duration: 2 turns. Rank 2 (20 PP): +6 Soak. +4 bonus dice vs [Push]/[Pull]. Duration 3 turns. Rank 3: +8 Soak. Immune to [Push]/[Pull]. Gain Stamina regen +5 while in stance."
        });
    }

    private void SeedAtgeirWielderTier2()
    {
        // Tier 2 - Ability 4: Hook and Drag (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1204,
            SpecializationID = 12,
            Name = "Hook and Drag",
            Description = "Using your weapon's hooked blade, you violently yank a priority target out of position.",
            MechanicalSummary = "Physical attack with [Pull] effect - drag enemy from back row to front row",
            TierLevel = 2,
            PPCost = 4,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (back row)",
            ResourceCost = new AbilityResourceCost { Stamina = 45 },
            AttributeUsed = "might",
            BonusDice = 2,
            SuccessThreshold = 2,
            DamageDice = 2,
            CooldownTurns = 4,
            CooldownType = "Standard",
            IsActive = true,
            Notes = "Rank 1: 2d8 Physical damage. STURDINESS opposed check to [Pull] target from back row to front row. Rank 2 (20 PP): 3d8 damage. +2 bonus to Pull check. Pulled target is [Slowed] for 1 turn. Rank 3: 4d8 damage. +3 bonus to Pull check. If Pull succeeds, target is also [Stunned] for 1 turn."
        });

        // Tier 2 - Ability 5: Line Breaker (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1205,
            SpecializationID = 12,
            Name = "Line Breaker",
            Description = "A wide, sweeping strike that shatters enemy formations and drives them backward.",
            MechanicalSummary = "AoE Physical attack with [Push] effect on all front-row enemies",
            TierLevel = 2,
            PPCost = 4,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "All Enemies (front row)",
            ResourceCost = new AbilityResourceCost { Stamina = 50 },
            AttributeUsed = "might",
            BonusDice = 2,
            SuccessThreshold = 2,
            DamageDice = 3,
            CooldownTurns = 5,
            CooldownType = "Standard",
            IsActive = true,
            Notes = "Rank 1: 3d6 Physical to all front row. STURDINESS opposed check to [Push] all targets to back row. Rank 2 (20 PP): 4d6 damage. +1 bonus to Push check. Successfully pushed enemies take +1d6 bonus damage. Rank 3: 5d6 damage. +2 bonus to Push check. Enemies pushed into back row are [Off-Balance] (-2 to hit, 1 turn)."
        });

        // Tier 2 - Ability 6: Guarding Presence (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1206,
            SpecializationID = 12,
            Name = "Guarding Presence",
            Description = "Your disciplined presence inspires fortitude in those around you. The formation holds.",
            MechanicalSummary = "Aura providing Soak bonus and stamina regeneration to adjacent front-row allies",
            TierLevel = 2,
            PPCost = 4,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Adjacent Front-Row Allies",
            ResourceCost = new AbilityResourceCost(),
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: While in front row: You and adjacent front-row allies gain +1 Soak. Rank 2 (20 PP): +2 Soak aura. Aura also grants +1 bonus die vs [Fear]. Rank 3: +3 Soak aura. Allies in aura regenerate +3 Stamina per turn."
        });
    }

    private void SeedAtgeirWielderTier3()
    {
        // Tier 3 - Ability 7: Brace for Charge (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1207,
            SpecializationID = 12,
            Name = "Brace for Charge",
            Description = "You set your weapon with expert precision. They will run onto your spear and break.",
            MechanicalSummary = "Defensive stance with massive counter-damage and stun effect when hit by melee",
            TierLevel = 3,
            PPCost = 5,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
            CooldownTurns = 999,
            CooldownType = "Once Per Combat",
            IsActive = true,
            Notes = "Rank 1: Enter defensive stance (1 turn). If hit by melee: +10 Soak, Immune [Knocked Down], attacker takes 4d8 Physical damage. Rank 2 (20 PP): Counter-damage increases to 5d8. Attacker must make WILL save DC 15 or be [Stunned] for 1 turn. Rank 3: Counter-damage 6d8. Stun save DC 18. If attacker is Mechanical/Undying, automatically Stunned."
        });

        // Tier 3 - Ability 8: Unstoppable Phalanx (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1208,
            SpecializationID = 12,
            Name = "Unstoppable Phalanx",
            Description = "Your polearm punches through armor and flesh, impaling one target and striking another.",
            MechanicalSummary = "Line-piercing attack hitting primary target and enemy behind them",
            TierLevel = 3,
            PPCost = 5,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy + Enemy Behind",
            ResourceCost = new AbilityResourceCost { Stamina = 60 },
            AttributeUsed = "might",
            BonusDice = 3,
            SuccessThreshold = 3,
            DamageDice = 6,
            CooldownTurns = 4,
            CooldownType = "Standard",
            IsActive = true,
            Notes = "Rank 1: 6d10 Physical to primary target. If hit: 4d10 Physical to enemy directly behind. Rank 2 (20 PP): 7d10 to primary, 5d10 to secondary. Both targets [Off-Balance] for 1 turn. Rank 3: 8d10 to primary, 6d10 to secondary. If primary dies: bonus damage to secondary doubled."
        });
    }

    private void SeedAtgeirWielderCapstone()
    {
        // Capstone - Ability 9: Living Fortress (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1209,
            SpecializationID = 12,
            Name = "Living Fortress",
            Description = "You have become the absolute master of your domain. A living fortress around which battles are won.",
            MechanicalSummary = "Permanent immunity to forced movement, reactive Brace for Charge, zone of control",
            TierLevel = 4,
            PPCost = 6,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites
            {
                RequiredPPInTree = 24,
                RequiredAbilityIDs = new List<int> { 1207, 1208 }  // Both Tier 3 required
            },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: While in front row: Immune to [Push] and [Pull]. Brace for Charge can be used as Reaction (once per combat). Rank 2 (20 PP): Aura: Adjacent allies also resistant to [Push]/[Pull] (+3 dice to resist). Your Skewer range increased by 1 row. Rank 3: Zone of Control: Enemies in front row opposite you have -1 to hit and cannot move freely. Brace reactive triggers twice per combat."
        });
    }

    #endregion

    #region Scrap-Tinker (ID: 14)

    private void SeedScrapTinkerSpecialization()
    {
        _log.Information("Seeding Scrap-Tinker specialization");

        var scrapTinker = new SpecializationData
        {
            SpecializationID = 14,
            Name = "Scrap-Tinker",
            ArchetypeID = 2, // Adept
            PathType = "Coherent",
            MechanicalRole = "Crafter / Pet Controller",
            PrimaryAttribute = "WITS",
            SecondaryAttribute = "FINESSE",
            Description = @"You are the scavenger-engineer who sees treasure in ruins. Where others see broken machines, you see repurposable parts. You salvage corrupted technology, reverse-engineer pre-Glitch devices, and cobble together functional gadgets from scrap.

            You craft drones for reconnaissance, bombs for crowd control, and weapon mods for allies. You're the tinkerer who proves that in a crashed system, the best debugger is the one who can rebuild from the ground up.",
            Tagline = "Salvage and innovation — craft gadgets, deploy drones, modify weapons",
            UnlockRequirements = new UnlockRequirements { MinLegend = 3 },
            ResourceSystem = "Stamina + Scrap Materials",
            TraumaRisk = "None",
            IconEmoji = "🔧",
            PPCostToUnlock = 3,
            IsActive = true
        };

        _specializationRepo.Insert(scrapTinker);

        // Seed all ability tiers
        SeedScrapTinkerTier1();
        SeedScrapTinkerTier2();
        SeedScrapTinkerTier3();
        SeedScrapTinkerCapstone();

        _log.Information("Scrap-Tinker seeding complete: 9 abilities");
    }

    private void SeedScrapTinkerTier1()
    {
        // Tier 1 - Ability 1: Master Scavenger (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1401,
            SpecializationID = 14,
            Name = "Master Scavenger",
            Description = "You see value where others see junk. Every bolt, every wire, every corroded gear—repurposable.",
            MechanicalSummary = "Bonus to scavenging Scrap Materials; find more from enemies and containers",
            TierLevel = 1,
            PPCost = 3,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: +1d10 bonus to scavenging Scrap Materials. Find 50% more Scrap from defeated mechanical enemies and loot containers. Rank 2 (20 PP): +2d10 bonus. Find 75% more Scrap. Can salvage Scrap from broken weapons/armor (dismantle for materials). Rank 3: +3d10 bonus. Find 100% more Scrap (double). Salvaged materials include rare components. Start expeditions with 20 Scrap."
        });

        // Tier 1 - Ability 2: Deploy Flash Bomb (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1402,
            SpecializationID = 14,
            Name = "Deploy Flash Bomb",
            Description = "You lob the improvised device. Flash! Their optics overload, their eyes burn.",
            MechanicalSummary = "AoE attack applying [Blinded] status to all enemies in 3x3 area",
            TierLevel = 1,
            PPCost = 3,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Ground location (3x3 area)",
            ResourceCost = new AbilityResourceCost { Stamina = 30 },
            AttributeUsed = "wits",
            BonusDice = 2,
            SuccessThreshold = 2,
            CooldownTurns = 2,
            CooldownType = "Standard",
            IsActive = true,
            Notes = "Rank 1: Throw Flash Bomb. All enemies in area make WILL save DC 13 or become [Blinded] for 2 turns. Consumes 1 Flash Bomb. Rank 2 (20 PP): DC increases to 15. Blinded enemies also take -2 Defense. Costs 25 Stamina. Rank 3: DC 17. Blinded duration 3 turns. [Masterwork Flash Bomb]: also deals 2d6 damage."
        });

        // Tier 1 - Ability 3: Salvage Expertise (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1403,
            SpecializationID = 14,
            Name = "Salvage Expertise",
            Description = "Your understanding of pre-Glitch engineering is encyclopedic. Your work is precise, efficient, masterful.",
            MechanicalSummary = "Crafting bonuses and chance to create superior-quality gadgets",
            TierLevel = 1,
            PPCost = 3,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: All Engineering crafting checks have +1d10 bonus. Crafted gadgets have 15% chance to be [Masterwork] (enhanced effects). Rank 2 (20 PP): +2d10 crafting bonus. Masterwork chance 25%. Crafting time reduced by 25%. Rank 3: +3d10 bonus. Masterwork chance 40%. Can craft [Prototype] quality (superior to Masterwork, 10% chance)."
        });
    }

    private void SeedScrapTinkerTier2()
    {
        // Tier 2 - Ability 4: Deploy Scout Drone (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1404,
            SpecializationID = 14,
            Name = "Deploy Scout Drone",
            Description = "The jerry-rigged drone buzzes to life. Its optics scan the battlefield, feeding you tactical data.",
            MechanicalSummary = "Deploy reconnaissance drone providing vision, revealing hidden enemies and traps",
            TierLevel = 2,
            PPCost = 4,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Self (deploys drone)",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
            CooldownTurns = 4,
            CooldownType = "Standard",
            IsActive = true,
            Notes = "Rank 1: Deploy Scout Drone (10 HP, 0 Armor). Grants vision in 5x5 area. Reveals hidden enemies and traps. Moves 3 spaces per turn (your command). Duration: Until destroyed or dismissed. Costs 15 Scrap Materials. Rank 2 (20 PP): Drone has 15 HP, 2 Armor. Vision radius 7x7. Can mark priority targets (+1 ally to hit vs marked enemy). Rank 3: Drone has 20 HP, 4 Armor. Vision radius 10x10. Can self-destruct for 4d6 damage (3x3 AoE, destroys drone)."
        });

        // Tier 2 - Ability 5: Deploy Shock Mine (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1405,
            SpecializationID = 14,
            Name = "Deploy Shock Mine",
            Description = "You carefully arm the mine. Step on it—instant overload. Nervous system fried.",
            MechanicalSummary = "Place trap mine that damages and stuns enemies who trigger it",
            TierLevel = 2,
            PPCost = 4,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Ground location",
            ResourceCost = new AbilityResourceCost { Stamina = 35 },
            AttributeUsed = "wits",
            BonusDice = 2,
            SuccessThreshold = 2,
            DamageDice = 3,
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: Place Shock Mine. When enemy moves onto it: trigger, deal 3d8 Lightning damage, STURDINESS save DC 14 or [Stunned] 1 turn. Consumes 1 Shock Mine. Rank 2 (20 PP): Damage 4d8. DC 16. Stun duration 2 turns. Can place 2 mines per combat. Rank 3: Damage 5d8. DC 18. [Masterwork Mine]: also applies [Slowed] for 2 turns after Stun ends."
        });

        // Tier 2 - Ability 6: Weapon Modification (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1406,
            SpecializationID = 14,
            Name = "Weapon Modification",
            Description = "You disassemble the weapon, integrate salvaged components, reassemble. Better than factory spec.",
            MechanicalSummary = "Apply permanent enhancement to ally weapon (elemental, precision, or durability)",
            TierLevel = 2,
            PPCost = 4,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single ally weapon",
            ResourceCost = new AbilityResourceCost { Stamina = 0 },
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: Apply permanent modification (out-of-combat, 10 minutes at workbench): [Elemental] (+1d6 Fire/Frost/Lightning), [Precision] (+1 to hit), or [Reinforced] (+50% durability). Costs 25 Scrap Materials. Rank 2 (20 PP): Costs 20 Scrap. Mods more powerful: [Elemental] (+2d6), [Precision] (+2 to hit), [Reinforced] (+100% durability + 10% crit chance). Rank 3: Can apply 2 modifications to same weapon (stacking). Prototype quality mods: bonus doubled."
        });
    }

    private void SeedScrapTinkerTier3()
    {
        // Tier 3 - Ability 7: Automated Scavenging (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1407,
            SpecializationID = 14,
            Name = "Automated Scavenging",
            Description = "You've built automated collection systems. Magnets, sensors, retrieval claws—never leave materials behind.",
            MechanicalSummary = "Automatically scavenge Scrap Materials after combat without action cost",
            TierLevel = 3,
            PPCost = 5,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: After combat, automatically scavenge 5 Scrap Materials from battlefield (no action required). Rank 2 (20 PP): Auto-scavenge 10 Scrap. Scout Drone can scavenge while deployed (adds 5 Scrap per combat). Rank 3: Auto-scavenge 15 Scrap. 25% chance to find rare components. Scrap Golem (if active) scavenges additional 10 Scrap."
        });

        // Tier 3 - Ability 8: Efficient Assembly (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1408,
            SpecializationID = 14,
            Name = "Efficient Assembly",
            Description = "Muscle memory. Optimized workflows. You assemble gadgets faster than most people load a gun.",
            MechanicalSummary = "Reduced crafting costs and time; can craft multiple gadgets simultaneously",
            TierLevel = 3,
            PPCost = 5,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            CooldownTurns = 0,
            CooldownType = "None",
            IsActive = true,
            Notes = "Rank 1: All gadget crafting costs 25% less Scrap Materials. Crafting time reduced by 50%. Rank 2 (20 PP): Costs 40% less. Crafting time reduced by 75%. Can craft 2 gadgets simultaneously. Rank 3: Costs 50% less. Some gadgets craftable instantly (Flash Bombs, Repair Kits). Can craft 3 gadgets simultaneously."
        });
    }

    private void SeedScrapTinkerCapstone()
    {
        // Capstone - Ability 9: Deploy Scrap Golem (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 1409,
            SpecializationID = 14,
            Name = "Deploy Scrap Golem",
            Description = "Your masterpiece. A walking junk pile animated by salvaged power cores. Loyal. Brutal. Yours.",
            MechanicalSummary = "Deploy powerful combat pet with high HP, armor, and damage; can self-destruct for AoE",
            TierLevel = 4,
            PPCost = 6,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            Prerequisites = new AbilityPrerequisites
            {
                RequiredPPInTree = 24,
                RequiredAbilityIDs = new List<int> { 1407, 1408 }  // Both Tier 3 required
            },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Self (deploys golem)",
            ResourceCost = new AbilityResourceCost { Stamina = 50 },
            DamageDice = 3,
            CooldownTurns = 999,
            CooldownType = "Once Per Expedition",
            IsActive = true,
            Notes = "Rank 1: Deploy Scrap Golem (40 HP, 6 Armor, immune to psychic effects). Acts on your turn. Slam: 3d10 Physical damage. Defend: Grant adjacent ally +3 Soak. Costs 50 Scrap Materials (once per expedition, out-of-combat: 1 hour assembly). Duration: Until destroyed or expedition ends. Rank 2 (20 PP): Golem has 60 HP, 8 Armor. Slam: 4d10 damage. Repair Protocol: Once per combat, self-heal 20 HP. Can carry 50 extra Scrap capacity. Rank 3: Golem has 80 HP, 10 Armor. Slam: 5d10 damage. Detonate: Command golem to self-destruct (8d10 damage, 5x5 AoE, destroys golem). Can rebuild with 25 Scrap (half cost)."
        });
    }

    #endregion
}
