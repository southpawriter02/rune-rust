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
    /// Seed all existing specializations (BoneSetter, JotunReader, Skald, SkarHordeAspirant, IronBane, AtgeirWielder)
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
            UnlockRequirements = new UnlockRequirements { MinLegend = 3, RequiredAttribute = "WILL", RequiredAttributeValue = 3 },
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
            DamageType = "Fire",
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
            DamageType = "Fire",
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
            DamageType = "Fire",
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
            DamageType = "Fire",
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
            DamageType = "Fire",
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
            DamageType = "Fire",
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
            DamageType = "Physical",
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
            DamageType = "Physical",
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
            DamageType = "Physical",
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
            DamageType = "Physical",
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
            DamageType = "Physical",
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
}
