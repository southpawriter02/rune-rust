using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.25.1: Seeds Strandhogg (Glitch-Raider) specialization for Skirmisher archetype
/// Kinetic predator who builds Momentum through movement and strikes, then spends it on devastating executions
/// </summary>
public class StrandhoggSeeder
{
    private static readonly ILogger _log = Log.ForContext<StrandhoggSeeder>();
    private readonly SpecializationRepository _specializationRepo;
    private readonly AbilityRepository _abilityRepo;

    public StrandhoggSeeder(string connectionString)
    {
        _specializationRepo = new SpecializationRepository(connectionString);
        _abilityRepo = new AbilityRepository(connectionString);
    }

    /// <summary>
    /// Seed Strandhogg specialization and all 9 abilities (3/3/2/1 tier distribution)
    /// Specialization ID: 25001
    /// Ability IDs: 25001-25009
    /// </summary>
    public void SeedStrandhoggSpecialization()
    {
        _log.Information("Seeding Strandhogg (Glitch-Raider) specialization");

        var strandhogg = new SpecializationData
        {
            SpecializationID = 25001,
            Name = "Strandhogg",
            ArchetypeID = 4, // Skirmisher
            PathType = "Coherent",
            MechanicalRole = "Mobile Burst DPS / Momentum Fighter",
            PrimaryAttribute = "FINESSE",
            SecondaryAttribute = "MIGHT",
            Description = @"The Strandhogg is the kinetic blur, the glitch-raider who exploits unstable physics to move impossibly fast. You build Momentum through movement and strikes, then spend it on devastating executions. You are the whirlwind that strikes from unexpected angles and vanishes before retaliation.

            In a reality where space stutters and time skips, you've learned to ride the errors, moving faster than coherent physics should allow. Each movement, each strike adds to your kinetic energy until you're a blur of motion. You don't tank damage—you're never there when it arrives.",
            Tagline = "The Momentum Striker",
            UnlockRequirements = new UnlockRequirements { MinLegend = 5, MaxCorruption = 100 },
            ResourceSystem = "Stamina + Momentum",
            TraumaRisk = "Low",
            IconEmoji = "⚔️",
            PPCostToUnlock = 3,
            IsActive = true
        };

        _specializationRepo.Insert(strandhogg);

        // Seed all ability tiers
        SeedStrandhoggTier1();
        SeedStrandhoggTier2();
        SeedStrandhoggTier3();
        SeedStrandhoggCapstone();

        _log.Information("Strandhogg seeding complete: 9 abilities (3/3/2/1)");
    }

    #region Tier 1: Foundational Momentum (3 abilities, 3 PP each)

    private void SeedStrandhoggTier1()
    {
        // Ability 1: Harrier's Alacrity I (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 25001,
            SpecializationID = 25001,
            Name = "Harrier's Alacrity I",
            Description = "You are always ready. Your muscles are coiled springs, your mind a hair-trigger. When violence erupts, you're already in motion.",
            MechanicalSummary = "Start combat with Momentum; bonus to Vigilance (turn order)",
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
            Notes = "Rank 1: Start every combat with 20 Momentum. Gain +2 bonus to Vigilance (turn order). Rank 2 (20 PP in tree): Start with 20 Momentum. Vigilance bonus increases to +3. Rank 3 (Capstone): Start with 30 Momentum. Vigilance bonus remains +3."
        });

        // Ability 2: Reaver's Strike (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 25002,
            SpecializationID = 25001,
            Name = "Reaver's Strike",
            Description = "A brutal, efficient strike. Not elegant—just fast, hard, and building toward something worse.",
            MechanicalSummary = "FINESSE-based melee attack; generates 15 Momentum on hit",
            TierLevel = 1,
            PPCost = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Melee)",
            ResourceCost = new AbilityResourceCost { Stamina = 35 },
            AttributeUsed = "finesse",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0, // Weapon damage
            DamageType = "Physical",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            CooldownTurns = 0,
            IsActive = true,
            Notes = "Rank 1: FINESSE-based melee attack dealing weapon damage + MIGHT. On hit: Generate 15 Momentum. Rank 2 (20 PP): Resource cost reduced to 30 Stamina. Damage increased by +1d6. Generate 15 Momentum on hit. Rank 3 (Capstone): Damage increased by +2d6 total. When hitting debuffed enemy, generate +10 bonus Momentum (25 total)."
        });

        // Ability 3: Dread Charge (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 25003,
            SpecializationID = 25001,
            Name = "Dread Charge",
            Description = "You explode forward at impossible speed, a kinetic glitch that shouldn't exist. Your target's mind reels, unable to process the violation of physics.",
            MechanicalSummary = "Move + attack; apply [Disoriented] + Psychic Stress; generate Momentum",
            TierLevel = 1,
            PPCost = 0,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
            AttributeUsed = "might",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 2,
            DamageType = "Physical",
            StatusEffectsApplied = new List<string> { "Disoriented" },
            CooldownTurns = 3,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Move from Back Row to Front Row (or within Front Row), then attack for 2d10 + MIGHT Physical damage. Apply [Disoriented] (1 turn) and 10 Psychic Stress to target. Generate 10 Momentum on hit. Rank 2 (20 PP): Damage increases to 3d10 + MIGHT. [Disoriented] duration: 2 turns. Generate 15 Momentum on hit. Rank 3 (Capstone): Can now charge from Front Row into enemy Back Row, disrupting ranged enemies. Damage remains 3d10 + MIGHT."
        });
    }

    #endregion

    #region Tier 2: Advanced Kinetics (3 abilities, 4 PP each)

    private void SeedStrandhoggTier2()
    {
        // Ability 4: Tidal Rush (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 25004,
            SpecializationID = 25001,
            Name = "Tidal Rush",
            Description = "Weakness feeds your momentum. A staggering foe, a bleeding enemy—each flaw in their defense accelerates your kinetic rhythm.",
            MechanicalSummary = "Generate bonus Momentum when hitting debuffed enemies",
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
            Notes = "Rank 1: When you hit an enemy suffering from mental/control debuffs ([Disoriented], [Stunned], [Feared], [Confused], [Slowed], [Rooted]), generate +10 bonus Momentum. Rank 2 (20 PP): Bonus Momentum increases to +15. Rank 3 (Capstone): Also works against enemies with damage-over-time effects ([Bleeding], [Burning], [Poisoned])."
        });

        // Ability 5: Harrier's Whirlwind (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 25005,
            SpecializationID = 25001,
            Name = "Harrier's Whirlwind",
            Description = "Strike and vanish. You're a whirlwind of blades, hitting and repositioning before the enemy can respond. They swing at empty air.",
            MechanicalSummary = "Attack then immediately reposition for free",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Melee)",
            ResourceCost = new AbilityResourceCost { Stamina = 45, SpecialResource = "Momentum:30" },
            AttributeUsed = "might",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 4,
            DamageType = "Physical",
            CooldownTurns = 2,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Deal 4d10 + MIGHT Physical damage. After attacking, immediately move to any valid position (costs 0 Stamina, generates 5 Momentum). Rank 2 (20 PP): Stamina cost reduced to 40. Free move still generates 5 Momentum. Rank 3 (Capstone): Free move generates 10 Momentum (doubled)."
        });

        // Ability 6: Vicious Flank (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 25006,
            SpecializationID = 25001,
            Name = "Vicious Flank",
            Description = "You exploit every opening, every weakness. Against a compromised foe, your strike is surgical and devastating.",
            MechanicalSummary = "Massive damage vs debuffed targets",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Melee)",
            ResourceCost = new AbilityResourceCost { Stamina = 40, SpecialResource = "Momentum:25" },
            AttributeUsed = "might",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 4,
            DamageType = "Physical",
            CooldownTurns = 3,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Deal 4d10 + MIGHT Physical damage. If target is debuffed ([Disoriented], [Stunned], [Feared], [Slowed], [Rooted], [Bleeding]), deal +50% damage. Rank 2 (20 PP): Momentum cost reduced to 20. Rank 3 (Capstone): On kill with this ability, refund 10 Momentum."
        });
    }

    #endregion

    #region Tier 3: Mastery of Motion (2 abilities, 5 PP each)

    private void SeedStrandhoggTier3()
    {
        // Ability 7: No Quarter (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 25007,
            SpecializationID = 25001,
            Name = "No Quarter",
            Description = "You give no quarter. The moment one foe falls, you're already moving to the next, riding the momentum of your kill.",
            MechanicalSummary = "Free move + Momentum on kill",
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
            Notes = "Rank 1: When you reduce an enemy to 0 HP, immediately move to any valid position (costs 0 Stamina, generates 5 Momentum). Rank 2 (20 PP): Generate 10 Momentum from the free move (doubled). Rank 3 (Capstone): Also gain +15 temporary HP when triggering this effect."
        });

        // Ability 8: Savage Harvest (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 25008,
            SpecializationID = 25001,
            Name = "Savage Harvest",
            Description = "This is the reaping. All your built momentum channels into a single, reality-bending strike. If it connects, you harvest their death itself.",
            MechanicalSummary = "Massive execution damage; refund resources on kill",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Melee)",
            ResourceCost = new AbilityResourceCost { Stamina = 50, SpecialResource = "Momentum:40" },
            AttributeUsed = "might",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 8,
            DamageType = "Physical",
            CooldownTurns = 4,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Deal 8d10 + MIGHT Physical damage. If this kills the target, refund 20 Stamina and 20 Momentum. Rank 2 (20 PP): Damage increases to 10d10 + MIGHT. Refund remains same. Rank 3 (Capstone): If this kills the target, also heal for 20% of your max HP."
        });
    }

    #endregion

    #region Capstone: Ultimate Expression (1 ability, 6 PP)

    private void SeedStrandhoggCapstone()
    {
        // Ability 9: Riptide of Carnage (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 25009,
            SpecializationID = 25001,
            Name = "Riptide of Carnage",
            Description = "You become the riptide. In a single, impossible moment, you strike three foes at once—a violation of causality so profound it feels like the world is screaming. When the static clears, bodies fall.",
            MechanicalSummary = "3-4 attacks in one turn; massive multi-target burst",
            TierLevel = 4,
            PPCost = 6,
            Prerequisites = new AbilityPrerequisites
            {
                RequiredPPInTree = 24,
                RequiredAbilityIDs = new List<int> { 25007, 25008 } // Must have No Quarter OR Savage Harvest
            },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Multiple Enemies",
            ResourceCost = new AbilityResourceCost { Stamina = 60, SpecialResource = "Momentum:75" },
            AttributeUsed = "might",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 4,
            DamageType = "Physical",
            CooldownTurns = 0,
            CooldownType = "Once per combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Make 3 attacks against different enemies in a single turn. Each attack deals 4d10 + MIGHT Physical damage. After all attacks, gain 15 Psychic Stress (causality violation is taxing). Rank 2 (20 PP): Make 4 attacks (up from 3). Stress cost remains 15. Rank 3 (Capstone): Make 4 attacks. Stress cost reduced to 10. Each kill refunds 10 Momentum."
        });
    }

    #endregion
}
