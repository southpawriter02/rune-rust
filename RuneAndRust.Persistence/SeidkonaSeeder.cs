using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.28.1: Seeds Seidkona specialization for Mystic archetype
/// The psychic archaeologist who communes with fragmented echoes of crashed reality
/// Channels corrupted memories for healing and knowledge with high Trauma Economy risk
/// </summary>
public class SeidkonaSeeder
{
    private static readonly ILogger _log = Log.ForContext<SeidkonaSeeder>();
    private readonly SpecializationRepository _specializationRepo;
    private readonly AbilityRepository _abilityRepo;

    public SeidkonaSeeder(string connectionString)
    {
        _specializationRepo = new SpecializationRepository(connectionString);
        _abilityRepo = new AbilityRepository(connectionString);
    }

    /// <summary>
    /// Seed Seidkona specialization and all 9 abilities (3/3/2/1 tier distribution)
    /// Specialization ID: 28001
    /// Ability IDs: 28001-28009
    /// </summary>
    public void SeedSeidkonaSpecialization()
    {
        _log.Information("Seeding Seidkona specialization");

        var seidkona = new SpecializationData
        {
            SpecializationID = 28001,
            Name = "Seidkona",
            ArchetypeID = 4, // Mystic
            PathType = "Heretical",
            MechanicalRole = "Psychic Archaeologist / Trauma Economy High Risk",
            PrimaryAttribute = "WILL",
            SecondaryAttribute = "WITS",
            Description = @"You are the psychic archaeologist who communes with fragmented echoes of a crashed reality. Not a nature shaman but an interpreter of corrupted data-logs, traumatic memories, and fragmented consciousness left by the Great Silence.

Your magic, Seiðr, is not command but interpretation—sifting through the eternal psychic scream for coherent whispers of truth. You are a medium between living and dead code, a translator of corruption, trading sanity for knowledge that could save or doom your party.

Through Spirit Bargains you channel unpredictable bonus effects. Through Forlorn Communion you trade Psychic Stress for forbidden knowledge. The ultimate expression is Moment of Clarity—two turns of perfect spirit control where all bargains succeed.",
            Tagline = "Psychic Archaeologist",
            UnlockRequirements = new UnlockRequirements { MinLegend = 5, MaxCorruption = 100 },
            ResourceSystem = "Aether Pool",
            TraumaRisk = "High",
            IconEmoji = "🔮",
            PPCostToUnlock = 10,
            IsActive = true
        };

        _specializationRepo.Insert(seidkona);

        // Seed all ability tiers
        SeedSeidkonaTier1();
        SeedSeidkonaTier2();
        SeedSeidkonaTier3();
        SeedSeidkonaCapstone();

        _log.Information("Seidkona seeding complete: 9 abilities (3/3/2/1)");
    }

    #region Tier 1: Foundational Bargains (3 abilities, 3 PP each)

    private void SeedSeidkonaTier1()
    {
        // Ability 1: Spiritual Attunement I (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 28001,
            SpecializationID = 28001,
            Name = "Spiritual Attunement I",
            Description = "Senses permanently open to unseen world. Perceive psychic echoes and traumatic residue.",
            MechanicalSummary = "Passive perception bonuses for magical/metaphysical phenomena, Psychic Resonance zones, Forlorn entities",
            TierLevel = 1,
            PPCost = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "will",
            BonusDice = 1,
            SuccessThreshold = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: +1 die to WILL checks to perceive magical/metaphysical phenomena. Can see faint aura around [Psychic Resonance] zones and Forlorn entities. Gain instinctual awareness of Blight intensity. RANK 2: +2 dice. RANK 3: +2 dice + can sense Forlorn presence through walls (30 ft radius)."
        });

        // Ability 2: Echo of Vigor (Active healing with Spirit Bargain)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 28002,
            SpecializationID = 28001,
            Name = "Echo of Vigor",
            Description = "Channel coherent memory of life and health from before the crash to mend wounds.",
            MechanicalSummary = "Active healing with [Spirit Bargain] chance to cleanse physical debuffs",
            TierLevel = 1,
            PPCost = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Ally",
            ResourceCost = new AbilityResourceCost { Stress = 5 }, // Replaced Aether with Stress as placeholder
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 3,
            DamageType = "Healing",
            StatusEffectsApplied = new List<string> { "SpiritBargain_DebuffCleanse" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Restore 3d8 HP to single ally. [Spirit Bargain] 25% chance: Also cleanse one minor physical debuff ([Bleeding], [Poisoned], [Disease]). Chance increases to 40% in [Psychic Resonance] zones. RANK 2: Restore 4d8 HP, bargain 30%. RANK 3: Restore 5d8 HP, bargain 35%, can target self."
        });

        // Ability 3: Echo of Misfortune (Active curse with Spirit Bargain)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 28003,
            SpecializationID = 28001,
            Name = "Echo of Misfortune",
            Description = "Channel chaotic echo of pain and confusion, clouding enemy mind with psychic static.",
            MechanicalSummary = "Apply [Cursed] debuff with [Spirit Bargain] spread on critical success",
            TierLevel = 1,
            PPCost = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy",
            ResourceCost = new AbilityResourceCost { Stress = 6 }, // Replaced Aether with Stress
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "",
            StatusEffectsApplied = new List<string> { "Cursed", "SpiritBargain_CurseSpread" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Apply [Cursed] to single enemy for 2 turns. [Cursed]: 25% miss chance, -1 die to all checks. [Spirit Bargain] On Critical Success: Curse spreads to adjacent enemy for 1 turn. RANK 2: [Cursed] for 3 turns, 30% miss chance. RANK 3: [Cursed] 3 turns, 30% miss + -2 dice penalty."
        });
    }

    #endregion

    #region Tier 2: Advanced Communion (3 abilities, 4 PP each)

    private void SeedSeidkonaTier2()
    {
        // Ability 4: Forlorn Communion (Knowledge acquisition with Psychic Stress cost)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 28004,
            SpecializationID = 28001,
            Name = "Forlorn Communion",
            Description = "Deliberately open mind to Forlorn or [Psychic Resonance] zone, sifting through screaming madness for truth.",
            MechanicalSummary = "Out-of-combat WITS check to reveal secrets; unavoidable +15 Psychic Stress cost",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Forlorn Entity or Psychic Resonance Zone",
            ResourceCost = new AbilityResourceCost { Stress = 4 }, // Replaced Aether with Stress
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 12,
            DamageDice = 0,
            DamageType = "",
            StatusEffectsApplied = new List<string> { "PsychicStress" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Standard Action (out of combat). WITS check (DC 12+). Success reveals: enemy weaknesses, puzzle solutions, lore secrets, hidden paths, item locations. Quality scales with success. COST: Gain +15 Psychic Stress (unavoidable). RANK 2: DC -2, Stress cost reduced to +12. RANK 3: DC -4, Stress +10, can ask 2 questions per communion."
        });

        // Ability 5: Spiritual Anchor (Self-healing for Psychic Stress)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 28005,
            SpecializationID = 28001,
            Name = "Spiritual Anchor",
            Description = "Retreat into quietest corner of mind, finding stillness between the screams.",
            MechanicalSummary = "Meditative state to remove Psychic Stress from self",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost { Stress = 6 }, // Replaced Aether with Stress
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "PsychicStressHealing",
            StatusEffectsApplied = new List<string> { "Meditating" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Standard Action. Spend turn in meditative state (no other actions). Remove 20 Psychic Stress from self. Cannot be interrupted. RANK 2: Remove 25 Stress. RANK 3: Remove 30 Stress + cleanse one mental debuff ([Fear], [Disoriented], [Charmed])."
        });

        // Ability 6: Fickle Fortune (Passive Spirit Bargain enhancement)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 28006,
            SpecializationID = 28001,
            Name = "Fickle Fortune",
            Description = "Become nexus for chaotic Aether. Luck clings to you as you constantly meddle with corrupted data streams.",
            MechanicalSummary = "Passive increase to all [Spirit Bargain] trigger chances; can force bargain success at Rank 3",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: All [Spirit Bargain] trigger chances increased by +15%. Echo of Vigor: 25%→40%, Echo of Misfortune: Critical spread more likely, Spirit Ward: Chance to last +1 turn. RANK 2: +20% increase. RANK 3: +25% increase + once per combat, can force a failed bargain to succeed (declare before roll)."
        });
    }

    #endregion

    #region Tier 3: Mastery of the Veil (2 abilities, 5 PP each)

    private void SeedSeidkonaTier3()
    {
        // Ability 7: Spirit Ward (Protective psychic barrier)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 28007,
            SpecializationID = 28001,
            Name = "Spirit Ward",
            Description = "Create protective circle inscribed with ancient glyphs acting as psychic filter.",
            MechanicalSummary = "Place protective ward negating environmental Psychic Stress for allies",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Row (All Allies)",
            ResourceCost = new AbilityResourceCost { Stress = 7 }, // Replaced Aether with Stress
            AttributeUsed = "will",
            BonusDice = 1,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "",
            StatusEffectsApplied = new List<string> { "SpiritWard", "SpiritBargain_ExtendedDuration" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Place [Spirit Ward] on target row for 3 turns. Allies in ward: Negate all environmental Psychic Stress gain, +1 die to Resolve vs psychic attacks, protected from [Psychic Resonance] effects. [Spirit Bargain] 25% chance: Lasts 4 turns instead. RANK 2: Ward grants +2 dice, bargain 30%. RANK 3: +2 dice, bargain 35%, can place on both rows simultaneously (costs 60 Aether)."
        });

        // Ability 8: Ride the Echoes (Teleportation with Corruption cost)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 28008,
            SpecializationID = 28001,
            Name = "Ride the Echoes",
            Description = "Attune deeply to psychic static, stepping into space between code, becoming flicker in corrupted data.",
            MechanicalSummary = "Instant battlefield teleportation with Runic Blight Corruption cost",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Self (+ Optional Ally at Rank 3)",
            ResourceCost = new AbilityResourceCost { Stress = 8 }, // Replaced Aether with Stress
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "",
            StatusEffectsApplied = new List<string> { "RunicBlightCorruption" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Instantly teleport to any unoccupied tile on battlefield. No line-of-sight required. COST: Gain +2 Runic Blight Corruption (interfacing with Blight nature). RANK 2: +1 Corruption cost (reduced). RANK 3: +1 Corruption + can bring one adjacent ally with you (they gain +1 Corruption)."
        });
    }

    #endregion

    #region Capstone: Ultimate Expression (6 PP)

    private void SeedSeidkonaCapstone()
    {
        // Ability 9: Moment of Clarity (Ultimate Spirit Bargain mastery)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 28009,
            SpecializationID = 28001,
            Name = "Moment of Clarity",
            Description = "Achieve perfect harmony with Great Silence. Screaming chaos resolves into coherent symphony of memory and intent.",
            MechanicalSummary = "Ultimate: 2-3 turns of guaranteed [Spirit Bargain] success with reduced costs, followed by Psychic Stress aftermath",
            TierLevel = 4,
            PPCost = 6,
            Prerequisites = new AbilityPrerequisites
            {
                RequiredPPInTree = 24,
                RequiredAbilityIDs = new List<int> { 28007, 28008 } // Must have Spirit Ward OR Ride the Echoes
            },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost { Stress = 10 }, // Replaced Aether with Stress
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "",
            StatusEffectsApplied = new List<string> { "Clarity", "PsychicStress_Aftermath" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Once per combat. Enter [Clarity] for 2 turns. Effects: ALL [Spirit Bargain] effects guaranteed (100%). Echo of Vigor always cleanses debuff. Echo of Misfortune always spreads. Spirit Ward always lasts 4 turns. Forlorn Communion costs 0 Aether, only +7 Stress, auto-success. Can cast Spiritual Anchor on ally (removes 20 Stress). When Clarity ends: Gain +20 Psychic Stress. RANK 2: [Clarity] for 3 turns, aftermath +15 Stress. RANK 3: 3 turns, aftermath +10 Stress, can use Moment of Clarity twice per combat."
        });
    }

    #endregion
}
