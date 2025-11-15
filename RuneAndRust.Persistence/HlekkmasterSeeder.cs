using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.25.2: Seeds Hlekkr-master (Chain-Master) specialization for Skirmisher archetype
/// Brutal controller who exploits glitching physics to drag enemies into kill zones and lock them down
/// </summary>
public class HlekkmasterSeeder
{
    private static readonly ILogger _log = Log.ForContext<HlekkmasterSeeder>();
    private readonly SpecializationRepository _specializationRepo;
    private readonly AbilityRepository _abilityRepo;

    public HlekkmasterSeeder(string connectionString)
    {
        _specializationRepo = new SpecializationRepository(connectionString);
        _abilityRepo = new AbilityRepository(connectionString);
    }

    /// <summary>
    /// Seed Hlekkr-master specialization and all 9 abilities (3/3/2/1 tier distribution)
    /// Specialization ID: 25002
    /// Ability IDs: 25010-25018
    /// </summary>
    public void SeedHlekkmasterSpecialization()
    {
        _log.Information("Seeding Hlekkr-master (Chain-Master) specialization");

        var hlekkmaster = new SpecializationData
        {
            SpecializationID = 25002,
            Name = "Hlekkr-master",
            ArchetypeID = 4, // Skirmisher
            PathType = "Coherent",
            MechanicalRole = "Battlefield Controller / Formation Breaker",
            PrimaryAttribute = "FINESSE",
            SecondaryAttribute = "MIGHT",
            Description = @"The Hlekkr-master is the battlefield puppeteer who exploits glitching physics to drag enemies into kill zones and lock them down. You use chains, hooks, and nets to control positioning and punish helplessness. Your chains don't just lock down—they make enemies die faster.

In a reality where corrupted enemies have unstable connections to physical space, you've learned to exploit their 'lag'—making them easier to drag around, easier to control. Each pull breaks formations and creates the conditions where fights cannot be lost. You are the master of puppets.",
            Tagline = "The Chain-Master",
            UnlockRequirements = new UnlockRequirements { MinLegend = 5, MaxCorruption = 100 },
            ResourceSystem = "Stamina",
            TraumaRisk = "Low",
            IconEmoji = "⛓️",
            PPCostToUnlock = 3,
            IsActive = true
        };

        _specializationRepo.Insert(hlekkmaster);

        // Seed all ability tiers
        SeedHlekkmasterTier1();
        SeedHlekkmasterTier2();
        SeedHlekkmasterTier3();
        SeedHlekkmasterCapstone();

        _log.Information("Hlekkr-master seeding complete: 9 abilities (3/3/2/1)");
    }

    #region Tier 1: Foundational Control (3 abilities, 3 PP each)

    private void SeedHlekkmasterTier1()
    {
        // Ability 1: Pragmatic Preparation I (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 25010,
            SpecializationID = 25002,
            Name = "Pragmatic Preparation I",
            Description = "You trust well-maintained tools, not hope. A sharp hook, reliable chain, and practiced technique are your foundation in a glitching world.",
            MechanicalSummary = "Bonus to trap checks; control effects last longer",
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
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Gain +1d10 bonus to FINESSE checks for setting/disarming traps. All your [Rooted] and [Slowed] effects last +1 turn longer. Rank 2 (20 PP in tree): Bonus increases to +2d10. Control duration bonus remains +1 turn. Rank 3 (Capstone): Bonus increases to +3d10. Control effects last +2 turns longer (total)."
        });

        // Ability 2: Netting Shot (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 25011,
            SpecializationID = 25002,
            Name = "Netting Shot",
            Description = "You hurl a weighted net with practiced accuracy, designed to entangle erratic movements and exploit physical instability.",
            MechanicalSummary = "Low damage; apply [Rooted]",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Ranged)",
            ResourceCost = new AbilityResourceCost { Stamina = 20 },
            AttributeUsed = "finesse",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 1,
            DamageType = "Physical",
            StatusEffectsApplied = new List<string> { "Rooted" },
            CooldownTurns = 2,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: FINESSE attack dealing 1d6 Physical damage. Apply [Rooted] for 2 turns (3 with Pragmatic Preparation). Rank 2 (20 PP): [Rooted] duration increases to 3 turns (4 with Pragmatic Preparation). Can target 2 enemies (split net). Rank 3 (Capstone): Stamina cost reduced to 15. Against highly corrupted enemies (60+ Corruption), also apply [Slowed] for 2 turns."
        });

        // Ability 3: Grappling Hook Toss (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 25012,
            SpecializationID = 25002,
            Name = "Grappling Hook Toss",
            Description = "You swing a three-pronged grappling hook, snagging vulnerable targets and dragging them into the frontline. Corrupted enemies slide through space more easily.",
            MechanicalSummary = "Pull enemy from Back Row to Front Row; apply [Disoriented]",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Ranged, Back Row)",
            ResourceCost = new AbilityResourceCost { Stamina = 30 },
            AttributeUsed = "finesse",
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
            Notes = "Rank 1: FINESSE attack dealing 2d8 Physical damage. On hit: Pull target from Back Row to Front Row. Apply [Disoriented] (1 turn). Rank 2 (20 PP): Damage increases to 3d8. Pull distance increases (can pull from further positions). Rank 3 (Capstone): On successful pull vs corrupted enemy, generate 10 Focus."
        });
    }

    #endregion

    #region Tier 2: Advanced Manipulation (3 abilities, 4 PP each)

    private void SeedHlekkmasterTier2()
    {
        // Ability 4: Snag the Glitch (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 25013,
            SpecializationID = 25002,
            Name = "Snag the Glitch",
            Description = "You've learned to read the Blight's rhythm. You anticipate stutters and flickers, making your chains preternaturally accurate against corrupted foes.",
            MechanicalSummary = "Control effects more effective vs corrupted enemies (scales with corruption level)",
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
            Notes = "Rank 1: Your control effects ([Rooted], [Slowed], [Seized]) have increased success chance vs corrupted enemies: Low (1-29): +10%, Medium (30-59): +20%, High (60-89): +40%, Extreme (90+): +60%. Rank 2 (20 PP): Success bonuses doubled (+20%/+40%/+80%/+100%). Also gain +1d10 damage vs corrupted enemies. Rank 3 (Capstone): +3d10 bonus damage vs corrupted enemies. Against Extreme Corruption targets, your control effects cannot miss."
        });

        // Ability 5: Unyielding Grip (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 25014,
            SpecializationID = 25002,
            Name = "Unyielding Grip",
            Description = "Your chain wraps around sparking servos and malfunctioning joints, locking them in place and forcing a critical system error.",
            MechanicalSummary = "Apply [Seized] to machines (prevents all actions)",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Ranged)",
            ResourceCost = new AbilityResourceCost { Stamina = 25 },
            AttributeUsed = "finesse",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 2,
            DamageType = "Physical",
            StatusEffectsApplied = new List<string> { "Seized" },
            CooldownTurns = 4,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: FINESSE attack dealing 2d8 Physical damage. If target is Undying/Mechanical: 60% chance to apply [Seized] (1 turn). [Seized] prevents ALL actions. Rank 2 (20 PP): [Seized] duration increases to 2 turns. Success chance increases to 80%. Rank 3 (Capstone): [Seized] duration increases to 3 turns. Also works on non-mechanical enemies at 40% success rate. Seized enemies take 1d6 damage per turn (crushing)."
        });

        // Ability 6: Punish the Helpless (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 25015,
            SpecializationID = 25002,
            Name = "Punish the Helpless",
            Description = "A trapped enemy is a dead enemy. Your predator's instinct capitalizes on immobility and confusion with brutal, focused strikes.",
            MechanicalSummary = "Bonus damage vs controlled enemies",
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
            Notes = "Rank 1: Your attacks deal +50% damage vs enemies suffering from [Rooted], [Slowed], [Stunned], [Seized], or [Disoriented]. Rank 2 (20 PP): Bonus damage increases to +75%. Also gain Advantage on attack rolls vs controlled enemies. Rank 3 (Capstone): Bonus damage increases to +100% (double damage). Controlled enemies also take 1d6 damage per turn from your chains."
        });
    }

    #endregion

    #region Tier 3: Mastery of the Leash (2 abilities, 5 PP each)

    private void SeedHlekkmasterTier3()
    {
        // Ability 7: Chain Scythe (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 25016,
            SpecializationID = 25002,
            Name = "Chain Scythe",
            Description = "You whirl a massive chain scythe in a devastating horizontal sweep, designed to trip, entangle, and utterly break enemy formations.",
            MechanicalSummary = "AoE row damage; apply [Slowed]; chance for [Knocked Down] vs corrupted",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "All Enemies in Row",
            ResourceCost = new AbilityResourceCost { Stamina = 35 },
            AttributeUsed = "finesse",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 2,
            DamageType = "Physical",
            StatusEffectsApplied = new List<string> { "Slowed", "Knocked Down" },
            CooldownTurns = 4,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Deal 2d8 Physical damage to all enemies in Front Row. Apply [Slowed] (2 turns) to all hit. Against highly corrupted enemies (60+ Corruption): 40% chance to apply [Knocked Down] instead. Rank 2 (20 PP): Damage increases to 3d8. [Slowed] duration increases to 3 turns. Knockdown chance increases to 60%. Rank 3 (Capstone): Can target Back Row instead of Front Row. Knockdown chance increases to 80%. Also apply [Disoriented] (1 turn) to all hit."
        });

        // Ability 8: Corruption Siphon Chain (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 25017,
            SpecializationID = 25002,
            Name = "Corruption Siphon Chain",
            Description = "Using a rune-etched chain, you lash to a corrupted foe and siphon their chaotic energy, causing system shock.",
            MechanicalSummary = "No damage; [Stunned] chance scales with target corruption (20-90%)",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Ranged)",
            ResourceCost = new AbilityResourceCost { Stamina = 30, Stress = 5 },
            AttributeUsed = "finesse",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "None",
            StatusEffectsApplied = new List<string> { "Stunned" },
            CooldownTurns = 3,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: No damage. Chance to apply [Stunned] (1 turn) based on target corruption: Low (1-29): 20% chance, Medium (30-59): 40% chance, High (60-89): 70% chance, Extreme (90+): 90% chance. Cost: Gain 5 Psychic Stress (exposure to raw Blight). Rank 2 (20 PP): [Stunned] duration increases to 2 turns. Stress cost remains 5. Rank 3 (Capstone): If successful vs Extreme Corruption target, also purge 10 Corruption from them. Stress cost reduced to 3."
        });
    }

    #endregion

    #region Capstone: Ultimate Expression (1 ability, 6 PP)

    private void SeedHlekkmasterCapstone()
    {
        // Ability 9: Master of Puppets (Passive + Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 25018,
            SpecializationID = 25002,
            Name = "Master of Puppets",
            Description = "You have achieved perfect understanding of battlefield manipulation in a corrupted world. You see only glitching pieces to be moved at will.",
            MechanicalSummary = "Passive: Pulled enemies become [Vulnerable]; Active: Corruption bomb (once per combat)",
            TierLevel = 4,
            PPCost = 6,
            Prerequisites = new AbilityPrerequisites
            {
                RequiredPPInTree = 24,
                RequiredAbilityIDs = new List<int> { 25016, 25017 } // Must have Chain Scythe OR Corruption Siphon Chain
            },
            AbilityType = "Hybrid",
            ActionType = "Bonus Action",
            TargetType = "Single Enemy (Active)",
            ResourceCost = new AbilityResourceCost { Stamina = 50 },
            AttributeUsed = "finesse",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 8,
            DamageType = "Psychic",
            StatusEffectsApplied = new List<string> { "Vulnerable" },
            CooldownTurns = 0,
            CooldownType = "Once per combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Passive: Whenever you Pull or Push an enemy, they become [Vulnerable] (1 turn) as their connection to reality is severed. Active (once per combat): Target single enemy with maximum Corruption. Make opposed FINESSE check. If successful: Trigger catastrophic feedback loop, causing explosion dealing 8d10 Psychic damage to all other enemies. Rank 2 (20 PP): Passive [Vulnerable] duration increases to 2 turns. Active explosion damage increases to 10d10. Rank 3 (Capstone): Passive also grants you +2d10 bonus to Pull/Push attempts. Active can be used on High Corruption (60+) enemies, not just Extreme."
        });
    }

    #endregion
}
