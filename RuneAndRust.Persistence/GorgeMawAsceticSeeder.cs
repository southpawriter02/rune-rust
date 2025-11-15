using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.26.2: Seeds GorgeMawAscetic specialization for Warrior archetype
/// Warrior-philosophers who perceive through earth vibrations, masters of seismic control and unarmed combat
/// </summary>
public class GorgeMawAsceticSeeder
{
    private static readonly ILogger _log = Log.ForContext<GorgeMawAsceticSeeder>();
    private readonly SpecializationRepository _specializationRepo;
    private readonly AbilityRepository _abilityRepo;

    public GorgeMawAsceticSeeder(string connectionString)
    {
        _specializationRepo = new SpecializationRepository(connectionString);
        _abilityRepo = new AbilityRepository(connectionString);
    }

    /// <summary>
    /// Seed GorgeMawAscetic specialization and all 9 abilities (3/3/2/1 tier distribution)
    /// Specialization ID: 26002
    /// Ability IDs: 26010-26018
    /// </summary>
    public void SeedGorgeMawAsceticSpecialization()
    {
        _log.Information("Seeding GorgeMawAscetic specialization");

        var gorgeMawAscetic = new SpecializationData
        {
            SpecializationID = 26002,
            Name = "GorgeMawAscetic",
            ArchetypeID = 1, // Warrior
            PathType = "Coherent",
            MechanicalRole = "Control Fighter / Seismic Monk",
            PrimaryAttribute = "MIGHT",
            SecondaryAttribute = "WILL",
            Description = @"The Gorge-Maw Ascetic embodies the warrior-philosopher who perceives the world through vibrations in the earth rather than sight. Through disciplined meditation near colossal Gorge-Maws, they have mastered Tremorsense—a seismic perception that makes them immune to darkness and blindness but completely vulnerable to flying enemies.

They weaponize the earth itself with unarmed strikes and shockwaves, creating a unique tactical dynamic of extreme situational power. Their mental discipline grants exceptional resistance to Fear and mental effects, providing aura protection to allies.

This path emphasizes control over raw damage, manipulating the battlefield through Push, Stun, Root, and Difficult Terrain effects. The ultimate expression is Earthshaker—a battlefield-wide earthquake that permanently alters terrain and knocks down all ground-based enemies.",
            Tagline = "The Seismic Monk",
            UnlockRequirements = new UnlockRequirements { MinLegend = 5, MaxCorruption = 100 },
            ResourceSystem = "Stamina",
            TraumaRisk = "None",
            IconEmoji = "⛰️",
            PPCostToUnlock = 10,
            IsActive = true
        };

        _specializationRepo.Insert(gorgeMawAscetic);

        // Seed all ability tiers
        SeedGorgeMawAsceticTier1();
        SeedGorgeMawAsceticTier2();
        SeedGorgeMawAsceticTier3();
        SeedGorgeMawAsceticCapstone();

        _log.Information("GorgeMawAscetic seeding complete: 9 abilities (3/3/2/1)");
    }

    #region Tier 1: Foundational Resonance (3 abilities, 3 PP each)

    private void SeedGorgeMawAsceticTier1()
    {
        // Ability 1: Tremorsense (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26010,
            SpecializationID = 26002,
            Name = "Tremorsense",
            Description = "Perceive the world through earth vibrations. Immune to blindness and darkness, auto-detect all ground-based enemies, but completely blind to flying enemies.",
            MechanicalSummary = "IMMUNE: [Blinded], [Thick Fog], [Absolute Darkness]. AUTO-DETECT: All ground enemies. BLIND TO FLYING: 50% miss vs flying, 0 Defense vs flying attacks",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            MaxRank = 1,
            CostToRank2 = 0,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "IMMUNE: [Blinded], [Thick Fog], [Absolute Darkness]. AUTO-DETECT: All ground enemies (including Hidden/Stealth). BLIND TO FLYING: 50% miss chance vs flying, 0 Defense vs flying attacks, flying enemies invisible on minimap."
        });

        // Ability 2: Stone Fist (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26011,
            SpecializationID = 26002,
            Name = "Stone Fist",
            Description = "Unarmed strike using weighted gauntlets, channeling seismic force into the blow.",
            MechanicalSummary = "MIGHT-based unarmed attack; bread-and-butter damage ability",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Melee)",
            ResourceCost = new AbilityResourceCost { Stamina = 30 },
            AttributeUsed = "might",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 2,
            DamageType = "Physical",
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: 2d8+MIGHT Physical damage, single target, melee. RANK 2: 3d8+MIGHT. RANK 3: 4d8+MIGHT + 10% chance to Stagger."
        });

        // Ability 3: Concussive Pulse (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26012,
            SpecializationID = 26002,
            Name = "Concussive Pulse",
            Description = "Strike the ground creating shockwave that pushes enemies back.",
            MechanicalSummary = "Push Front Row enemies to Back Row + damage",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "AoE Front Row",
            ResourceCost = new AbilityResourceCost { Stamina = 35 },
            AttributeUsed = "might",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 1,
            DamageType = "Physical",
            StatusEffectsApplied = new List<string> { "Staggered" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Push all Front Row enemies to Back Row, 1d6+MIGHT damage. RANK 2: Push + 2d6+MIGHT damage. RANK 3: Push + 2d8+MIGHT damage + [Staggered] for 1 round if they collide with Back Row."
        });
    }

    #endregion

    #region Tier 2: Advanced Vibration (3 abilities, 4 PP each)

    private void SeedGorgeMawAsceticTier2()
    {
        // Ability 4: Sensory Discipline (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26013,
            SpecializationID = 26002,
            Name = "Sensory Discipline",
            Description = "Profound mental stillness grants resistance to mental effects.",
            MechanicalSummary = "Bonus dice vs [Fear] and [Disoriented]",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "will",
            BonusDice = 2,
            SuccessThreshold = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: +2 dice vs [Fear] and [Disoriented]. RANK 2: +3 dice. RANK 3: +4 dice + immune to [Fear] from ambient sources."
        });

        // Ability 5: Shattering Wave (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26014,
            SpecializationID = 26002,
            Name = "Shattering Wave",
            Description = "Targeted tremor that stuns priority target at range.",
            MechanicalSummary = "Single target auto-hit stun attempt at any range",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Any Range)",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "Physical",
            StatusEffectsApplied = new List<string> { "Stunned", "Staggered" },
            CooldownTurns = 2,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Single target (any range), auto-hit, 60% [Stunned] for 1 round OR guaranteed [Staggered]. RANK 2: 75% Stun OR guaranteed Staggered + 2d6 damage. RANK 3: 85% Stun for 2 rounds OR guaranteed Staggered + 3d6 damage."
        });

        // Ability 6: Resonant Tremor (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26015,
            SpecializationID = 26002,
            Name = "Resonant Tremor",
            Description = "Create zone of difficult terrain under enemy formation.",
            MechanicalSummary = "Create [Difficult Terrain] area with damage over time",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Target Area (3x3)",
            ResourceCost = new AbilityResourceCost { Stamina = 35 },
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 1,
            DamageType = "Physical",
            CooldownTurns = 3,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Target area (3x3 tiles), creates [Difficult Terrain] for 3 rounds (double movement costs). RANK 2: 4x4 tiles, 3 rounds, 1d6 damage per turn in zone. RANK 3: 5x5 tiles, 4 rounds, 2d6 damage per turn + -1 Accuracy for enemies in zone."
        });
    }

    #endregion

    #region Tier 3: Mastery of the Earth (2 abilities, 5 PP each)

    private void SeedGorgeMawAsceticTier3()
    {
        // Ability 7: Earthen Grasp (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26016,
            SpecializationID = 26002,
            Name = "Earthen Grasp",
            Description = "Earth erupts to root all enemies in area.",
            MechanicalSummary = "AoE Root + damage",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "AoE Front Row",
            ResourceCost = new AbilityResourceCost { Stamina = 45 },
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 2,
            DamageType = "Physical",
            StatusEffectsApplied = new List<string> { "Rooted", "Vulnerable" },
            CooldownTurns = 3,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: AoE Front Row, [Rooted] for 2 rounds, 2d6 damage. RANK 2: Both rows, [Rooted] 2 rounds, 3d6 damage. RANK 3: Both rows, [Rooted] 3 rounds, 4d6 damage + [Vulnerable] while Rooted."
        });

        // Ability 8: Inner Stillness (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26017,
            SpecializationID = 26002,
            Name = "Inner Stillness",
            Description = "Complete mental immunity and aura protection for adjacent allies.",
            MechanicalSummary = "Immune to mental effects + provide aura protection",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self + Adjacent Allies",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "will",
            BonusDice = 1,
            SuccessThreshold = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: IMMUNE to [Fear]. Aura: Adjacent allies +1 die vs [Fear]. RANK 2: IMMUNE to [Fear] and [Disoriented]. Aura: +1 die vs both. RANK 3: IMMUNE to [Fear], [Disoriented], [Charmed]. Aura: +2 dice vs all three."
        });
    }

    #endregion

    #region Capstone: Ultimate Expression (1 ability, 6 PP)

    private void SeedGorgeMawAsceticCapstone()
    {
        // Ability 9: Earthshaker (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26018,
            SpecializationID = 26002,
            Name = "Earthshaker",
            Description = "Massive earthquake knocking down all ground enemies and permanently altering terrain.",
            MechanicalSummary = "AoE massive damage + [Knocked Down] + permanent terrain alteration",
            TierLevel = 4,
            PPCost = 6,
            Prerequisites = new AbilityPrerequisites
            {
                RequiredPPInTree = 24,
                RequiredAbilityIDs = new List<int> { 26016, 26017 } // Must have Earthen Grasp OR Inner Stillness
            },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "All Ground Enemies",
            ResourceCost = new AbilityResourceCost { Stamina = 60 },
            AttributeUsed = "might",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 4,
            DamageType = "Physical",
            StatusEffectsApplied = new List<string> { "KnockedDown", "Vulnerable" },
            CooldownTurns = 1,
            CooldownType = "Once Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: 4d8+MIGHT damage to ALL ground enemies, guaranteed [Knocked Down], creates permanent Difficult Terrain (3x3). Once per combat. RANK 2: 5d8+MIGHT, [Knocked Down] for 2 rounds, 4x4 terrain + Cover. RANK 3: 6d10+MIGHT, [Knocked Down] 2 rounds, 5x5 terrain + Cover + [Vulnerable] for 1 round."
        });
    }

    #endregion
}
