using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.28.2: Seeds Echo-Caller specialization for Mystic archetype
/// The reality manipulator who weaponizes psychic echoes and cascading fear
/// Channels traumatic memories as weapons with medium Trauma Economy risk
/// </summary>
public class EchoCallerSeeder
{
    private static readonly ILogger _log = Log.ForContext<EchoCallerSeeder>();
    private readonly SpecializationRepository _specializationRepo;
    private readonly AbilityRepository _abilityRepo;

    public EchoCallerSeeder(string connectionString)
    {
        _specializationRepo = new SpecializationRepository(connectionString);
        _abilityRepo = new AbilityRepository(connectionString);
    }

    /// <summary>
    /// Seed Echo-Caller specialization and all 9 abilities (3/3/2/1 tier distribution)
    /// Specialization ID: 28002
    /// Ability IDs: 28010-28018
    /// </summary>
    public void SeedEchoCallerSpecialization()
    {
        _log.Information("Seeding Echo-Caller specialization");

        var echoCaller = new SpecializationData
        {
            SpecializationID = 28002,
            Name = "EchoCaller",
            ArchetypeID = 5, // Mystic
            PathType = "Coherent",
            MechanicalRole = "Psychic Artillery / Crowd Control / Medium Trauma Risk",
            PrimaryAttribute = "WILL",
            SecondaryAttribute = "WITS",
            Description = @"You are the reality manipulator who weaponizes the Great Silence's eternal psychic scream. Where others commune with echoes, you command them. You are psychic artillery—projecting traumatic memories as weapons, creating cascading fear, and implanting phantom sensations in enemy minds.

You twist perception itself, making enemies see threats that aren't there or miss dangers that are. Through Echo Chains your attacks spread to adjacent targets. Through Fear Cascade you create mass panic. The ultimate expression is Silence Made Weapon—the eternal scream manifested as battlefield-wide devastation that scales with enemy terror.

You are the nightmare made manifest, proof that in a glitching world, the scariest weapon is what you think you see.",
            Tagline = "Psychic Artillery",
            UnlockRequirements = new UnlockRequirements { MinLegend = 5, MaxCorruption = 100 },
            ResourceSystem = "Aether Pool",
            TraumaRisk = "Medium",
            IconEmoji = "👁️",
            PPCostToUnlock = 10,
            IsActive = true
        };

        _specializationRepo.Insert(echoCaller);

        // Seed all ability tiers
        SeedEchoCallerTier1();
        SeedEchoCallerTier2();
        SeedEchoCallerTier3();
        SeedEchoCallerCapstone();

        _log.Information("Echo-Caller seeding complete: 9 abilities (3/3/2/1)");
    }

    #region Tier 1: Foundational Echoes (3 abilities, 3 PP each)

    private void SeedEchoCallerTier1()
    {
        // Ability 1: Echo Attunement (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 28010,
            SpecializationID = 28002,
            Name = "Echo Attunement",
            Description = "Constantly attuned to psychic reverberations. Sense emotional states and project echoes with greater efficiency.",
            MechanicalSummary = "Passive Aether cost reduction for Echo abilities + psychic resistance",
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
            Notes = "RANK 1: All Echo-tagged abilities cost -5 Aether. +1 die to WILL checks to resist psychic attacks. RANK 2: -10 Aether cost, +2 dice. RANK 3: -15 Aether, +2 dice, Echo Chain range increased by 1 tile."
        });

        // Ability 2: Scream of Silence (Active psychic damage)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 28011,
            SpecializationID = 28002,
            Name = "Scream of Silence",
            Description = "Project concentrated burst of psychic static directly into target's mind.",
            MechanicalSummary = "[Echo] Psychic damage with bonus vs Feared targets, spreads at Rank 3",
            TierLevel = 1,
            PPCost = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 35 }, // Aether mapped to Stamina in code
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 3,
            DamageType = "Psychic",
            StatusEffectsApplied = new List<string> { "Echo" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: [Echo] Deal 3d8 Psychic damage to single target. If target already [Feared], deal +1d8 damage. RANK 2: 4d8 Psychic damage, +2d8 if Feared. RANK 3: 5d8 damage, +2d8 if Feared, [Echo Chain] spreads 50% damage to adjacent enemy."
        });

        // Ability 3: Phantom Menace (Active fear application)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 28012,
            SpecializationID = 28002,
            Name = "Phantom Menace",
            Description = "Implant false sensory data causing target to perceive imminent threat.",
            MechanicalSummary = "[Echo] Apply [Feared] debuff with Echo Chain at Rank 3",
            TierLevel = 1,
            PPCost = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 30 },
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "",
            StatusEffectsApplied = new List<string> { "Feared", "Echo" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: [Echo] Apply [Feared] to single target for 2 turns. [Feared]: Cannot attack, -2 dice to all checks, flee if possible. RANK 2: [Feared] 3 turns. RANK 3: [Feared] 3 turns, [Echo Chain] 50% chance spreads to adjacent enemy for 2 turns."
        });
    }

    #endregion

    #region Tier 2: Advanced Manipulation (3 abilities, 4 PP each)

    private void SeedEchoCallerTier2()
    {
        // Ability 4: Echo Cascade (Passive Echo Chain enhancement)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 28013,
            SpecializationID = 28002,
            Name = "Echo Cascade",
            Description = "Mastered art of psychic resonance. Echoes bounce and amplify through enemy formations.",
            MechanicalSummary = "Passive increases to Echo Chain range and damage",
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
            Notes = "RANK 1: All [Echo Chain] effects have range increased to 2 tiles (can hit enemies 2 tiles away). Chain damage increased from 50% to 60%. RANK 2: Range 2 tiles, 70% damage. RANK 3: Range 3 tiles, 80% damage, chains can hit 2 targets instead of 1."
        });

        // Ability 5: Reality Fracture (Active spatial manipulation)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 28014,
            SpecializationID = 28002,
            Name = "Reality Fracture",
            Description = "Warp local spacetime perception, disorienting target and forcing spatial displacement.",
            MechanicalSummary = "[Echo] Psychic damage + Disoriented + forced Push with Echo Chain",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 2,
            DamageType = "Psychic",
            StatusEffectsApplied = new List<string> { "Disoriented", "Echo" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: [Echo] Single target: Deal 2d8 Psychic damage + apply [Disoriented] for 2 turns + forced Push 2 tiles in chosen direction. [Disoriented]: -2 dice to Accuracy, cannot use complex abilities. RANK 2: 3d8 damage, Push 3 tiles. RANK 3: 4d8 damage, Push 3 tiles, [Echo Chain] adjacent enemy also Pushed 2 tiles."
        });

        // Ability 6: Terror Feedback (Passive resource restoration)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 28015,
            SpecializationID = 28002,
            Name = "Terror Feedback",
            Description = "Channel enemy fear back into yourself as empowering energy.",
            MechanicalSummary = "Passive Aether restoration when applying Fear + Empowered buff at Rank 3",
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
            Notes = "RANK 1: Whenever you apply [Feared] to enemy, restore 10 Aether. RANK 2: Restore 15 Aether. RANK 3: Restore 20 Aether + gain [Empowered] for 1 turn (+2 dice to damage)."
        });
    }

    #endregion

    #region Tier 3: Mastery of Terror (2 abilities, 5 PP each)

    private void SeedEchoCallerTier3()
    {
        // Ability 7: Fear Cascade (Active AoE fear spread)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 28016,
            SpecializationID = 28002,
            Name = "Fear Cascade",
            Description = "Trigger chain reaction of panic spreading through enemy formation like psychic wildfire.",
            MechanicalSummary = "[Echo] AoE WILL check for Fear, auto-damage already-Feared enemies",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "All Enemies in Radius",
            ResourceCost = new AbilityResourceCost { Stamina = 45 },
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 14,
            DamageDice = 2,
            DamageType = "Psychic",
            StatusEffectsApplied = new List<string> { "Feared", "Echo" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: [Echo] All enemies within 3 tiles of target: Make WILL Resolve check (DC 14) or become [Feared] for 2 turns. Already-Feared enemies automatically fail and take 2d6 Psychic damage. RANK 2: DC 15, [Feared] 3 turns, 3d6 damage. RANK 3: DC 16, [Feared] 3 turns, 4d6 damage, [Echo Chain] auto-spreads to one enemy outside initial radius."
        });

        // Ability 8: Echo Displacement (Active forced teleportation)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 28017,
            SpecializationID = 28002,
            Name = "Echo Displacement",
            Description = "Forcibly teleport target by manipulating their psychic spatial perception.",
            MechanicalSummary = "[Echo] Forced teleportation + Psychic damage + Disoriented with Stress cost",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 50, Stress = 5 },
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 2,
            DamageType = "Psychic",
            StatusEffectsApplied = new List<string> { "Disoriented", "Echo" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: [Echo] Single target: Teleport enemy to any unoccupied tile within 5 tiles. Target takes 2d8 Psychic damage and is [Disoriented] for 1 turn. COST: Gain +5 Psychic Stress (forceful reality manipulation). RANK 2: Teleport within 7 tiles, 3d8 damage, +4 Stress. RANK 3: Teleport within 10 tiles, 4d8 damage, +3 Stress, [Echo Chain] adjacent enemy also teleported (random tile within 3 tiles)."
        });
    }

    #endregion

    #region Capstone: Ultimate Expression (6 PP)

    private void SeedEchoCallerCapstone()
    {
        // Ability 9: Silence Made Weapon (Ultimate battlefield-wide psychic assault)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 28018,
            SpecializationID = 28002,
            Name = "Silence Made Weapon",
            Description = "Channel raw Great Silence itself—the eternal scream made manifest as battlefield-wide psychic assault.",
            MechanicalSummary = "Ultimate: AoE Psychic damage scaling with enemy debuffs + mass Fear with Stress cost",
            TierLevel = 4,
            PPCost = 6,
            Prerequisites = new AbilityPrerequisites
            {
                RequiredPPInTree = 24,
                RequiredAbilityIDs = new List<int> { 28016, 28017 } // Must have Fear Cascade OR Echo Displacement
            },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "All Enemies",
            ResourceCost = new AbilityResourceCost { Stamina = 60, Stress = 15 },
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 16,
            DamageDice = 4,
            DamageType = "Psychic",
            StatusEffectsApplied = new List<string> { "Feared", "Echo" },
            CooldownTurns = 0,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Once per combat. [Echo] ALL enemies on battlefield: Deal 4d10 Psychic damage. Damage increases by +1d10 for each [Feared] or [Disoriented] enemy (max +5d10). All enemies make WILL Resolve (DC 16) or become [Feared] for 2 turns. COST: Gain +15 Psychic Stress. RANK 2: 5d10 base, +1d10 per status (max +6d10), DC 17, +12 Stress. RANK 3: 6d10 base, +2d10 per status (max +12d10), DC 18, +10 Stress, can be used twice per combat."
        });
    }

    #endregion
}
