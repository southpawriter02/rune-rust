using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.26.1: Seeds Berserkr specialization for Warrior archetype
/// The roaring fire who channels trauma into untamed physical power through the Fury resource system
/// </summary>
public class BerserkrSeeder
{
    private static readonly ILogger _log = Log.ForContext<BerserkrSeeder>();
    private readonly SpecializationRepository _specializationRepo;
    private readonly AbilityRepository _abilityRepo;

    public BerserkrSeeder(string connectionString)
    {
        _specializationRepo = new SpecializationRepository(connectionString);
        _abilityRepo = new AbilityRepository(connectionString);
    }

    /// <summary>
    /// Seed Berserkr specialization and all 9 abilities (3/3/2/1 tier distribution)
    /// Specialization ID: 26001
    /// Ability IDs: 26001-26009
    /// </summary>
    public void SeedBerserkrSpecialization()
    {
        _log.Information("Seeding Berserkr specialization");

        var berserkr = new SpecializationData
        {
            SpecializationID = 26001,
            Name = "Berserkr",
            ArchetypeID = 1, // Warrior
            PathType = "Heretical",
            MechanicalRole = "Melee Damage Dealer / Fury Fighter",
            PrimaryAttribute = "MIGHT",
            SecondaryAttribute = "STURDINESS",
            Description = @"The Berserkr embodies the heretical warrior who channels the world's trauma into pure, untamed physical power. They are roaring fires of destruction who open their minds to the violent psychic static of the Great Silence, transforming that chaos into battle-lust that pushes their bodies beyond normal limits.

Playing a Berserkr means embracing high-risk, high-reward gameplay—becoming more dangerous as combat intensifies, but also more mentally vulnerable. Your Fury resource (0-100) is gained by dealing damage and taking damage, creating a dangerous feedback loop where pain fuels power.

However, the Trauma Economy exacts its toll: while holding any Fury, you suffer -2 dice penalty to WILL Resolve Checks, making you vulnerable to Fear, psychic attacks, and mental status effects.",
            Tagline = "The Roaring Fire",
            UnlockRequirements = new UnlockRequirements { MinLegend = 5, MaxCorruption = 100 },
            ResourceSystem = "Stamina + Fury (0-100)",
            TraumaRisk = "High",
            IconEmoji = "🔥",
            PPCostToUnlock = 10,
            IsActive = true
        };

        _specializationRepo.Insert(berserkr);

        // Seed all ability tiers
        SeedBerserkrTier1();
        SeedBerserkrTier2();
        SeedBerserkrTier3();
        SeedBerserkrCapstone();

        _log.Information("Berserkr seeding complete: 9 abilities (3/3/2/1)");
    }

    #region Tier 1: Foundational Fury (3 abilities, 3 PP each)

    private void SeedBerserkrTier1()
    {
        // Ability 1: Primal Vigor (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26001,
            SpecializationID = 26001,
            Name = "Primal Vigor",
            Description = "The Berserkr's physiology is tied to their rage. As fury builds, their body surges with adrenaline, accelerating stamina recovery.",
            MechanicalSummary = "Gain +2 Stamina regen per 25 Fury (scales to +8 at 100 Fury)",
            TierLevel = 1,
            PPCost = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
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
            Notes = "Rank 1: For every 25 Fury, gain +2 Stamina regeneration per turn. Breakpoints: 25 Fury = +2, 50 Fury = +4, 75 Fury = +6, 100 Fury = +8. Rank 2 (20 PP): Bonus increases to +3 per 25 Fury (max +12 at 100 Fury). Rank 3 (Capstone): Bonus increases to +4 per 25 Fury (max +16 at 100 Fury)."
        });

        // Ability 2: Wild Swing (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26002,
            SpecializationID = 26001,
            Name = "Wild Swing",
            Description = "The Berserkr unleashes a wide, reckless swing, caring little for precision and focusing only on widespread destruction.",
            MechanicalSummary = "AoE attack hitting all Front Row enemies; generates +5 Fury per hit",
            TierLevel = 1,
            PPCost = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "AoE Front Row",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
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
            Notes = "Rank 1: Deals 2d8+MIGHT Physical damage to all enemies in Front Row. Generates +5 Fury per enemy hit. Rank 2 (20 PP): Damage increases to 2d10+MIGHT. Fury generation +7 per hit. Rank 3 (Capstone): Damage increases to 3d8+MIGHT. Fury generation +10 per hit. Cost reduced to 35 Stamina. Can hit Back Row if Front Row is clear."
        });

        // Ability 3: Reckless Assault (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26003,
            SpecializationID = 26001,
            Name = "Reckless Assault",
            Description = "Lowering their guard completely, the Berserkr lunges forward to deliver a powerful single-target attack.",
            MechanicalSummary = "High damage single-target attack; generates +15 Fury but applies [Vulnerable] to self",
            TierLevel = 1,
            PPCost = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Melee)",
            ResourceCost = new AbilityResourceCost { Stamina = 35 },
            AttributeUsed = "might",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 3,
            DamageType = "Physical",
            StatusEffectsApplied = new List<string> { "Vulnerable" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Deals 3d10+MIGHT Physical damage. Generates +15 Fury. Applies [Vulnerable] to self for 1 round (+25% damage taken). Rank 2 (20 PP): Damage 4d10+MIGHT. Fury +18. [Vulnerable] reduced to +20% damage. Rank 3 (Capstone): Damage 5d10+MIGHT. Fury +20. [Vulnerable] +15%. Cost 30 Stamina. If kills target, [Vulnerable] not applied."
        });
    }

    #endregion

    #region Tier 2: Advanced Carnage (3 abilities, 4 PP each)

    private void SeedBerserkrTier2()
    {
        // Ability 4: Unleashed Roar (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26004,
            SpecializationID = 26001,
            Name = "Unleashed Roar",
            Description = "The Berserkr lets out a terrifying, guttural war cry, challenging a single foe to face their wrath.",
            MechanicalSummary = "Taunt single enemy; gain +10 Fury when taunted enemy attacks you",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 30, SpecialResource = "Fury:20" },
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            StatusEffectsApplied = new List<string> { "Taunted" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Taunts single enemy for 2 rounds. Gain +10 Fury each time taunted enemy attacks. Rank 2 (20 PP): Cost 25 Stamina+20 Fury. Fury refund +12. First attack from taunted enemy deals -10% damage. Rank 3 (Capstone): Taunt duration 3 rounds. Cost 25 Stamina+15 Fury. Fury refund +15. First attack -20% damage. If taunted enemy dies, gain [Empowered] for 1 round."
        });

        // Ability 5: Whirlwind of Destruction (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26005,
            SpecializationID = 26001,
            Name = "Whirlwind of Destruction",
            Description = "A spinning vortex of pure destruction that reaches across the entire battlefield.",
            MechanicalSummary = "Massive AoE damage to ALL enemies (Front + Back rows)",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "AoE All Enemies",
            ResourceCost = new AbilityResourceCost { Stamina = 50, SpecialResource = "Fury:30" },
            AttributeUsed = "might",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 3,
            DamageType = "Physical",
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Deals 3d8+MIGHT Physical damage to ALL enemies (Front + Back rows). Rank 2 (20 PP): Damage 4d8+MIGHT. Cost 45 Stamina+30 Fury. Each enemy killed refunds +5 Fury. Rank 3 (Capstone): Damage 5d8+MIGHT. Cost 45 Stamina+25 Fury. Each kill refunds +8 Fury. If kills 3+ enemies, gain [Fortified] for 2 rounds."
        });

        // Ability 6: Blood-Fueled (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26006,
            SpecializationID = 26001,
            Name = "Blood-Fueled",
            Description = "Pain is a catalyst. Every wound is an invitation to greater violence. The Berserkr has learned to transform suffering into power.",
            MechanicalSummary = "Doubles Fury gained from taking HP damage",
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
            Notes = "Rank 1: Doubles Fury gained from taking damage (1 HP damage = 2 Fury). Rank 2 (20 PP): Fury from damage increased to 2.5x (10 HP = 25 Fury). Rank 3 (Capstone): Fury from damage increased to 3x (10 HP = 30 Fury). Also gain +1 Stamina per 5 damage taken."
        });
    }

    #endregion

    #region Tier 3: Mastery of Rage (2 abilities, 5 PP each)

    private void SeedBerserkrTier3()
    {
        // Ability 7: Hemorrhaging Strike (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26007,
            SpecializationID = 26001,
            Name = "Hemorrhaging Strike",
            Description = "Focusing their rage into a single savage blow, the Berserkr opens a grievous injury that will bleed the enemy dry.",
            MechanicalSummary = "Massive single-target burst + [Bleeding] DoT",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Melee)",
            ResourceCost = new AbilityResourceCost { Stamina = 45, SpecialResource = "Fury:40" },
            AttributeUsed = "might",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 4,
            DamageType = "Physical",
            StatusEffectsApplied = new List<string> { "Bleeding" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Deals 4d10+MIGHT Physical damage. Applies [Bleeding] (3d6 per turn for 3 rounds). Rank 2 (20 PP): Damage 5d10+MIGHT. [Bleeding] 4d6 per turn for 3 rounds. If target dies from bleed, recover 20 Stamina. Rank 3 (Capstone): Damage 6d10+MIGHT. [Bleeding] 5d6 per turn for 4 rounds. Cost 40 Stamina+35 Fury. If target dies from bleed, recover 30 Stamina and 20 Fury. [Bleeding] cannot be cleansed."
        });

        // Ability 8: Death or Glory (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26008,
            SpecializationID = 26001,
            Name = "Death or Glory",
            Description = "The Berserkr fights with the greatest ferocity when on the brink of death. Desperation fuels transcendent rage.",
            MechanicalSummary = "While [Bloodied] (below 50% HP), all Fury generation increased by +50%",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
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
            Notes = "Rank 1: While [Bloodied] (below 50% HP), all Fury generation increased by +50%. Example: Reckless Assault generates 15 Fury normally, 22-23 Fury while Bloodied. Rank 2 (20 PP): Fury generation bonus increases to +75%. Falling below 25% HP grants +5 Fury immediately (once per combat). Rank 3 (Capstone): Fury generation bonus +100% (doubled). Falling below 25% HP grants +10 Fury. While below 25% HP, gain +2 MIGHT."
        });
    }

    #endregion

    #region Capstone: Ultimate Expression (1 ability, 6 PP)

    private void SeedBerserkrCapstone()
    {
        // Ability 9: Unstoppable Fury (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26009,
            SpecializationID = 26001,
            Name = "Unstoppable Fury",
            Description = "The Berserkr's rage transcends mere emotion and becomes a force of nature, allowing them to defy death itself through sheer will and fury.",
            MechanicalSummary = "Immunity to Fear/Stun; once per combat survive lethal damage at 1 HP with 100 Fury",
            TierLevel = 4,
            PPCost = 6,
            Prerequisites = new AbilityPrerequisites
            {
                RequiredPPInTree = 24,
                RequiredAbilityIDs = new List<int> { 26007, 26008 } // Must have Hemorrhaging Strike OR Death or Glory
            },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            CooldownTurns = 0,
            CooldownType = "Once per combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Immunity to [Feared] and [Stunned]. Once per combat: When reduced to 0 HP, HP is instead set to 1 and gain 100 Fury. Rank 2 (20 PP): Immunity to [Feared], [Stunned], [Disoriented]. Twice per combat survival trigger. When triggered, also gain [Fortified] for 2 rounds. Rank 3 (Capstone): Immunity to [Feared], [Stunned], [Disoriented], [Charmed]. Twice per combat survival. When triggered, gain [Fortified] and [Empowered] for 2 rounds. Next ability costs 0 Fury."
        });
    }

    #endregion
}
