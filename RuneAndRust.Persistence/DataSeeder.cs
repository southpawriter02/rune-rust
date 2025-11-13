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

    public DataSeeder(string connectionString)
    {
        _specializationRepo = new SpecializationRepository(connectionString);
        _abilityRepo = new AbilityRepository(connectionString);
    }

    /// <summary>
    /// Seed all existing specializations (BoneSetter, JotunReader, Skald)
    /// </summary>
    public void SeedExistingSpecializations()
    {
        _log.Information("Starting specialization data seeding");

        SeedBoneSetterSpecialization();
        SeedJotunReaderSpecialization();
        SeedSkaldSpecialization();

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
            MechanicalRole = "Support/Healer",
            PrimaryAttribute = "WITS",
            SecondaryAttribute = "FINESSE",
            Description = "Non-magical medic and sanity anchor. Uses Field Medicine crafting to create healing items.",
            Tagline = "Keep party alive with healing items, remove debuffs, stabilize sanity",
            UnlockRequirements = new UnlockRequirements { MinLegend = 0, MaxCorruption = 100 },
            ResourceSystem = "Stamina",
            TraumaRisk = "None",
            IconEmoji = "🩺",
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
        // Field Medic I (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 101,
            SpecializationID = 1,
            Name = "Field Medic I",
            Description = "[PASSIVE] +2 to Field Medicine crafting checks. Start each expedition with 3 free Standard Healing Poultices.",
            TierLevel = 1,
            PPCost = 0, // Tier 1 granted free on unlock
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            MechanicalSummary = "+2 Field Medicine, 3 free healing poultices per expedition",
            MaxRank = 1,
            IsActive = true
        });

        // Mend Wound
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 102,
            SpecializationID = 1,
            Name = "Mend Wound",
            Description = "Consume a healing poultice to restore HP to an ally. Effectiveness based on poultice quality.",
            TierLevel = 1,
            PPCost = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Ally",
            ResourceCost = new AbilityResourceCost { Stamina = 5 },
            AttributeUsed = "wits",
            BonusDice = 1,
            SuccessThreshold = 2,
            MechanicalSummary = "Use healing poultice on ally",
            MaxRank = 3,
            CostToRank2 = 5,
            IsActive = true
        });

        // Apply Tourniquet
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 103,
            SpecializationID = 1,
            Name = "Apply Tourniquet",
            Description = "Remove [Bleeding] status from an ally. No stamina cost - emergency field medicine.",
            TierLevel = 1,
            PPCost = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Ally",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "wits",
            BonusDice = 2,
            SuccessThreshold = 2,
            MechanicalSummary = "Remove [Bleeding]",
            StatusEffectsRemoved = new List<string> { "Bleeding" },
            MaxRank = 3,
            CostToRank2 = 5,
            IsActive = true
        });
    }

    private void SeedBoneSetterTier2()
    {
        // Anatomical Insight
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 104,
            SpecializationID = 1,
            Name = "Anatomical Insight",
            Description = "Apply [Vulnerable] status to organic enemy. Target takes +25% damage for 3 turns. WITS check vs target's defense.",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 20 }, // v0.18: Reduced from 25
            AttributeUsed = "wits",
            BonusDice = 2,
            SuccessThreshold = 3,
            MechanicalSummary = "Apply [Vulnerable] - target takes +25% damage for 3 turns",
            StatusEffectsApplied = new List<string> { "Vulnerable" },
            MaxRank = 3,
            CostToRank2 = 5,
            IsActive = true
        });

        // Administer Antidote
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 105,
            SpecializationID = 1,
            Name = "Administer Antidote",
            Description = "Remove [Poisoned] and [Disease] status effects from ally. Crafted antidotes are more effective.",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Ally",
            ResourceCost = new AbilityResourceCost { Stamina = 15 },
            AttributeUsed = "wits",
            BonusDice = 2,
            SuccessThreshold = 2,
            MechanicalSummary = "Remove [Poisoned] and [Disease]",
            StatusEffectsRemoved = new List<string> { "Poisoned", "Disease" },
            MaxRank = 3,
            CostToRank2 = 5,
            IsActive = true
        });

        // Triage (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 106,
            SpecializationID = 1,
            Name = "Triage",
            Description = "[PASSIVE] All healing you provide to bloodied allies (HP < 50%) gains +25% effectiveness.",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            MechanicalSummary = "+25% healing to bloodied allies",
            MaxRank = 1,
            IsActive = true
        });
    }

    private void SeedBoneSetterTier3()
    {
        // Cognitive Realignment
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 107,
            SpecializationID = 1,
            Name = "Cognitive Realignment",
            Description = "Remove [Feared] and [Disoriented] status effects from ally. Restore 2d6 Psychic Stress. Your presence anchors their sanity.",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Ally",
            ResourceCost = new AbilityResourceCost { Stamina = 25 }, // v0.18: Reduced from 30
            AttributeUsed = "will",
            BonusDice = 2,
            SuccessThreshold = 2,
            MechanicalSummary = "Remove [Feared] and [Disoriented], restore 2d6 Stress",
            StatusEffectsRemoved = new List<string> { "Feared", "Disoriented" },
            HealingDice = 2, // Used for Stress restoration
            MaxRank = 3,
            CostToRank2 = 5,
            IsActive = true
        });

        // First, Do No Harm (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 108,
            SpecializationID = 1,
            Name = "First, Do No Harm",
            Description = "[PASSIVE] After healing an ally, gain +2 Defense until your next turn. Helping others keeps you vigilant.",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            MechanicalSummary = "+2 Defense after healing (lasts 1 turn)",
            MaxRank = 1,
            IsActive = true
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
            Description = "⭐ CAPSTONE: Massive heal (4d6 + WITS) and remove ALL physical debuffs ([Bleeding], [Poisoned], [Disease], [Vulnerable]). Limited uses per expedition.",
            TierLevel = 4,
            PPCost = 6,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 24, RequiredAbilityIDs = new List<int> { 107, 108 } },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Ally",
            ResourceCost = new AbilityResourceCost { Stamina = 40 }, // v0.18: Reduced from 60
            AttributeUsed = "wits",
            BonusDice = 3,
            SuccessThreshold = 2,
            MechanicalSummary = "4d6+WITS heal, remove all physical debuffs",
            HealingDice = 4,
            StatusEffectsRemoved = new List<string> { "Bleeding", "Poisoned", "Disease", "Vulnerable" },
            MaxRank = 3,
            CostToRank2 = 5,
            CooldownType = "Per Expedition",
            IsActive = true
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
            PathType = "Heretical",
            MechanicalRole = "Utility/Analyst",
            PrimaryAttribute = "WITS",
            SecondaryAttribute = "FINESSE",
            Description = "Forensic analyst of the apocalypse. Exposes enemy weaknesses for tactical advantage.",
            Tagline = "Analyze enemies, apply debuffs, translate runic puzzles, unlock secrets",
            UnlockRequirements = new UnlockRequirements { MinLegend = 0, MaxCorruption = 100 },
            ResourceSystem = "Stamina",
            TraumaRisk = "High",
            IconEmoji = "📜",
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
        // Scholarly Acumen I
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 201,
            SpecializationID = 2,
            Name = "Scholarly Acumen I",
            Description = "[PASSIVE] +1d to System Bypass and investigation checks. Knowledge is your weapon.",
            TierLevel = 1,
            PPCost = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            MechanicalSummary = "+1d to System Bypass and investigation",
            MaxRank = 1,
            IsActive = true
        });

        // Analyze Weakness
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 202,
            SpecializationID = 2,
            Name = "Analyze Weakness",
            Description = "Reveal enemy HP, Resistances, and Vulnerabilities to entire party. Costs 5 Psychic Stress (staring into the crash hurts).",
            TierLevel = 1,
            PPCost = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 25, Stress = 5 }, // v0.18: Reduced stamina from 30
            AttributeUsed = "wits",
            BonusDice = 3,
            SuccessThreshold = 2,
            MechanicalSummary = "Reveal enemy stats to party, 5 Stress cost",
            MaxRank = 3,
            CostToRank2 = 5,
            IsActive = true
        });

        // Runic Linguistics
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 203,
            SpecializationID = 2,
            Name = "Runic Linguistics",
            Description = "[PASSIVE] Automatically translate non-magical runic inscriptions. Bypass puzzle gates that would require difficult WITS checks.",
            TierLevel = 1,
            PPCost = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            MechanicalSummary = "Auto-translate runes, bypass puzzles",
            MaxRank = 1,
            IsActive = true
        });
    }

    private void SeedJotunReaderTier2()
    {
        // Exploit Design Flaw
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 204,
            SpecializationID = 2,
            Name = "Exploit Design Flaw",
            Description = "Apply [Analyzed] debuff to target. All allies gain +2 Accuracy against [Analyzed] enemies for 3 turns.",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 28 }, // v0.18: Reduced from 35
            AttributeUsed = "wits",
            BonusDice = 2,
            SuccessThreshold = 2,
            MechanicalSummary = "Apply [Analyzed] - party gets +2 Accuracy vs target",
            StatusEffectsApplied = new List<string> { "Analyzed" },
            MaxRank = 3,
            CostToRank2 = 5,
            IsActive = true
        });

        // Navigational Bypass
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 205,
            SpecializationID = 2,
            Name = "Navigational Bypass",
            Description = "Grant entire party +2d to resist/avoid trap damage for next 3 rooms. Your knowledge of Jötun-Forged systems protects your allies.",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "All Allies",
            ResourceCost = new AbilityResourceCost { Stamina = 30 },
            AttributeUsed = "wits",
            BonusDice = 3,
            SuccessThreshold = 2,
            MechanicalSummary = "Party gets +2d vs traps for 3 rooms",
            MaxRank = 3,
            CostToRank2 = 5,
            IsActive = true
        });

        // Structural Insight
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 206,
            SpecializationID = 2,
            Name = "Structural Insight",
            Description = "[PASSIVE] Automatically detect unstable structures (collapsing floors, weak walls, environmental hazards) before entering a room.",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            MechanicalSummary = "Auto-detect hazards before room entry",
            MaxRank = 1,
            IsActive = true
        });
    }

    private void SeedJotunReaderTier3()
    {
        // Calculated Triage
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 207,
            SpecializationID = 2,
            Name = "Calculated Triage",
            Description = "[PASSIVE] Allies within 10 feet of you gain +25% effectiveness from consumable healing items. Your analysis optimizes treatment.",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            MechanicalSummary = "Allies near you get +25% from healing consumables",
            MaxRank = 1,
            IsActive = true
        });

        // The Unspoken Truth
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 208,
            SpecializationID = 2,
            Name = "The Unspoken Truth",
            Description = "Knowledge attack that inflicts [Disoriented] status. Speak the enemy's true designation - most cannot bear the weight of their own identity.",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
            AttributeUsed = "will",
            BonusDice = 3,
            SuccessThreshold = 3,
            MechanicalSummary = "2d6 psychic damage + [Disoriented], ignores armor",
            DamageDice = 2,
            IgnoresArmor = true,
            StatusEffectsApplied = new List<string> { "Disoriented" },
            MaxRank = 3,
            CostToRank2 = 5,
            IsActive = true
        });
    }

    private void SeedJotunReaderCapstone()
    {
        // Architect of the Silence
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 209,
            SpecializationID = 2,
            Name = "Architect of the Silence",
            Description = "⭐ CAPSTONE: Speak original Jötun command syntax. Apply [Seized] status to Jötun-Forged or Undying enemy for 2 turns (cannot take actions). Costs 15 Psychic Stress.",
            TierLevel = 4,
            PPCost = 6,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 24, RequiredAbilityIDs = new List<int> { 207, 208 } },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 60, Stress = 15 },
            AttributeUsed = "will",
            BonusDice = 4,
            SuccessThreshold = 4,
            MechanicalSummary = "[Seized] for 2 turns - target cannot act, 15 Stress cost",
            StatusEffectsApplied = new List<string> { "Seized" },
            MaxRank = 3,
            CostToRank2 = 5,
            CooldownType = "Per Combat",
            IsActive = true,
            Notes = "Only works on Jötun-Forged or Undying enemies"
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
}
