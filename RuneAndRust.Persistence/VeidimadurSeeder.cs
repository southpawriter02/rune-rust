using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.24.1: Seeds Veiðimaðr (Hunter) specialization for Skirmisher archetype
/// Patient predator who tracks Blighted targets, exploits corruption, and delivers precision ranged damage
/// </summary>
public class VeidimadurSeeder
{
    private static readonly ILogger _log = Log.ForContext<VeidimadurSeeder>();
    private readonly SpecializationRepository _specializationRepo;
    private readonly AbilityRepository _abilityRepo;

    public VeidimadurSeeder(string connectionString)
    {
        _specializationRepo = new SpecializationRepository(connectionString);
        _abilityRepo = new AbilityRepository(connectionString);
    }

    /// <summary>
    /// Seed Veiðimaðr specialization and all 9 abilities (3/3/2/1 tier distribution)
    /// Specialization ID: 24001
    /// Ability IDs: 24001-24009
    /// </summary>
    public void SeedVeidimadurSpecialization()
    {
        _log.Information("Seeding Veiðimaðr (Hunter) specialization");

        var veidimadur = new SpecializationData
        {
            SpecializationID = 24001,
            Name = "Veiðimaðr",
            ArchetypeID = 4, // Skirmisher
            PathType = "Coherent",
            MechanicalRole = "Ranged DPS / Corruption Tracker",
            PrimaryAttribute = "FINESSE",
            SecondaryAttribute = "WITS",
            Description = @"You are the patient predator of a corrupted world. You've learned to read the invisible signs of the Runic Blight—the subtle tells that reveal a creature's corruption level. You mark high-priority targets, exploit their weaknesses, and deliver devastating shots from the safety of the back row.

            Your arrows can even purge corruption from heavily Blighted foes. You are the hunter who culls the sick before the infection spreads. Your precision is your weapon against chaos.",
            Tagline = "The Hunter Who Tracks the Blight",
            UnlockRequirements = new UnlockRequirements { MinLegend = 5, MaxCorruption = 100 },
            ResourceSystem = "Stamina + Focus",
            TraumaRisk = "Medium",
            IconEmoji = "🏹",
            PPCostToUnlock = 3,
            IsActive = true
        };

        _specializationRepo.Insert(veidimadur);

        // Seed all ability tiers
        SeedVeidimadurTier1();
        SeedVeidimadurTier2();
        SeedVeidimadurTier3();
        SeedVeidimadurCapstone();

        _log.Information("Veiðimaðr seeding complete: 9 abilities (3/3/2/1)");
    }

    #region Tier 1: Foundational Marksmanship (3 abilities, 3 PP each)

    private void SeedVeidimadurTier1()
    {
        // Ability 1: Wilderness Acclimation I (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 24001,
            SpecializationID = 24001,
            Name = "Wilderness Acclimation I",
            Description = "Your senses are honed to a razor's edge. You distinguish unnatural spoor from healthy, reading the subtle signs of a corrupted landscape.",
            MechanicalSummary = "+1d10 to WITS checks for tracking/foraging/perception; can identify Blighted creatures",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "wits",
            BonusDice = 1,
            SuccessThreshold = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: +1d10 bonus to WITS-based checks for tracking, foraging, and perceiving hidden/ambushing creatures. Can identify Blighted creatures by spoor. Rank 2 (20 PP in tree): Bonus increases to +2d10. Can estimate corruption level of tracked creatures (Low/Medium/High/Extreme). Rank 3 (Capstone): Bonus increases to +3d10. Automatically detect [Blighted] items without touching them (avoid accidental Corruption)."
        });

        // Ability 2: Aimed Shot (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 24002,
            SpecializationID = 24001,
            Name = "Aimed Shot",
            Description = "You steady your breath, your focus absolute, and release a single, perfectly aimed shot. Pure, logical precision against a chaotic world.",
            MechanicalSummary = "FINESSE-based ranged attack; bread-and-butter damage ability",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Ranged)",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
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
            Notes = "Rank 1: FINESSE-based ranged attack dealing weapon damage. Always uses FINESSE regardless of weapon type. Rank 2 (20 PP): Resource cost reduced to 35 Stamina. Damage increased by +1d6. Rank 3 (Capstone): Damage increased by +2d6 (total). On critical hit, apply [Bleeding] for 2 turns (1d6/turn)."
        });

        // Ability 3: Set Snare (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 24003,
            SpecializationID = 24001,
            Name = "Set Snare",
            Description = "You quickly assemble a simple but effective snare, concealing it beneath corrupted earth and rubble.",
            MechanicalSummary = "Place trap that Roots first enemy to step on it",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Target Tile",
            ResourceCost = new AbilityResourceCost { Stamina = 35 },
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            StatusEffectsApplied = new List<string> { "Rooted" },
            CooldownTurns = 2,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Place concealed trap. First enemy to step on it becomes [Rooted] for 1 turn. Requires 1 Trap Component. Rank 2 (20 PP): [Rooted] duration increases to 2 turns. Can place up to 2 active traps (up from 1). Rank 3 (Capstone): [Rooted] duration increases to 3 turns. Trapped enemy also takes 2d6 Physical damage."
        });
    }

    #endregion

    #region Tier 2: Advanced Tracking & Tactics (3 abilities, 4 PP each)

    private void SeedVeidimadurTier2()
    {
        // Ability 4: Mark for Death (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 24004,
            SpecializationID = 24001,
            Name = "Mark for Death",
            Description = "You focus your intent on a single target, observing the subtle tells of its Blighted nature—a twitch, an unnatural luminescence, a faint hum of static. You mark it as the primary corruption to be cleansed.",
            MechanicalSummary = "Apply [Marked]; your attacks deal bonus damage vs marked target; inflicts Psychic Stress on you",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Bonus Action",
            TargetType = "Single Enemy (Visible)",
            ResourceCost = new AbilityResourceCost { Stamina = 30 },
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            StatusEffectsApplied = new List<string> { "Marked" },
            CooldownTurns = 3,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Apply [Marked] debuff for 3 turns. Your attacks vs [Marked] target deal +8 bonus damage. Inflicts 5 Psychic Stress on you (focusing on Blight). Rank 2 (20 PP): Bonus damage increases to +12. Duration increases to 4 turns. Stress cost reduced to 3. Rank 3 (Capstone): Bonus damage increases to +15. Allies also gain +5 damage vs [Marked] target. Stress cost reduced to 2."
        });

        // Ability 5: Blight-Tipped Arrow (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 24005,
            SpecializationID = 24001,
            Name = "Blight-Tipped Arrow",
            Description = "You draw an arrow tipped with toxin harvested from Blighted flora. The shot introduces a sliver of the world's own sickness into your foe.",
            MechanicalSummary = "Physical damage + [Blighted Toxin] DoT; vs corrupted targets has chance to inflict [Glitch]",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Ranged)",
            ResourceCost = new AbilityResourceCost { Stamina = 45 },
            AttributeUsed = "finesse",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 3,
            DamageType = "Physical",
            StatusEffectsApplied = new List<string> { "Blighted Toxin", "Glitch" },
            CooldownTurns = 3,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Deal 3d6 Physical damage + apply [Blighted Toxin] (2d6 damage/turn, 3 turns). If target has 30+ Corruption, 40% chance to inflict [Glitch] (skip next action). Requires 1 Alchemical Component. Rank 2 (20 PP): Base damage increases to 4d6. Toxin lasts 4 turns. Glitch chance increases to 60%. Rank 3 (Capstone): Toxin damage increases to 3d6/turn. Glitch chance increases to 80%. Glitch duration: 1 turn."
        });

        // Ability 6: Predator's Focus (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 24006,
            SpecializationID = 24001,
            Name = "Predator's Focus",
            Description = "Your mind is a fortress of calm focus. The familiar rhythm of the hunt—draw, aim, release—filters out the maddening psychic noise.",
            MechanicalSummary = "While in back row: bonus to Resolve checks vs Psychic Stress",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
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
            Notes = "Rank 1: While in back row and not adjacent to enemies, gain +1d10 to Resolve checks vs Psychic Stress. Rank 2 (20 PP): Bonus increases to +2d10. Also gain +1d10 to Perception checks while in back row. Rank 3 (Capstone): Bonus increases to +3d10. While in back row, regenerate 5 Stamina per turn (out of combat only)."
        });
    }

    #endregion

    #region Tier 3: Mastery of the Hunt (2 abilities, 5 PP each)

    private void SeedVeidimadurTier3()
    {
        // Ability 7: Exploit Corruption (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 24007,
            SpecializationID = 24001,
            Name = "Exploit Corruption",
            Description = "You have studied the Blight so long you can predict its chaotic influence. A corrupted creature is unstable—its form less coherent, more susceptible to system-shocking blows.",
            MechanicalSummary = "Increased critical hit chance vs corrupted targets (scales with corruption level)",
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
            Notes = "Rank 1: Your attacks gain increased critical hit chance vs corrupted targets: Low Corruption (1-29): +5% crit chance, Medium Corruption (30-59): +10% crit chance, High Corruption (60-89): +15% crit chance, Extreme Corruption (90+): +20% crit chance. Rank 2 (20 PP): Crit bonuses doubled (+10%/+20%/+30%/+40%). Critical hits vs High/Extreme Corruption targets apply [Staggered] (1 turn). Rank 3 (Capstone): Critical hits vs corrupted targets deal +50% damage. If critical kills target, refund 20 Stamina."
        });

        // Ability 8: Heartseeker Shot (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 24008,
            SpecializationID = 24001,
            Name = "Heartseeker Shot",
            Description = "You take a full turn to aim not for a vital organ, but for the metaphysical core of the target's corruption—the focal point of its glitching existence. Your stable arrow corrects unstable code.",
            MechanicalSummary = "Charge 1 turn, then massive damage; if [Marked] purges corruption for bonus damage",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Full Turn (Charge)",
            TargetType = "Single Enemy (Ranged)",
            ResourceCost = new AbilityResourceCost { Stamina = 60 },
            AttributeUsed = "finesse",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 6,
            DamageType = "Physical",
            CooldownTurns = 4,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Requires full turn to charge (cannot move or use other abilities). Next turn: Release shot dealing 6d10 Physical damage. If target is [Marked]: Purge 10 Corruption from target, dealing +2 bonus damage per Corruption purged (max +20). Requires 30 Focus. Rank 2 (20 PP): Base damage increases to 8d10. Purge amount increases to 15 Corruption (max +30 bonus damage). Rank 3 (Capstone): Base damage increases to 10d10. Purge amount increases to 20 Corruption (max +40 bonus damage). If this kills a [Marked] target, refund 30 Stamina and 15 Focus."
        });
    }

    #endregion

    #region Capstone: Ultimate Expression (1 ability, 6 PP)

    private void SeedVeidimadurCapstone()
    {
        // Ability 9: Stalker of the Unseen (Passive + Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 24009,
            SpecializationID = 24001,
            Name = "Stalker of the Unseen",
            Description = "You have become the perfect predator. Your senses transcend the physical, perceiving the invisible threads of Blight itself. To be marked by you is to have your corruption laid bare.",
            MechanicalSummary = "Passive: Auto-learn vulnerabilities on Mark. Active: Toggle stance for vision immunity + Stagger procs",
            TierLevel = 4,
            PPCost = 6,
            Prerequisites = new AbilityPrerequisites
            {
                RequiredPPInTree = 24,
                RequiredAbilityIDs = new List<int> { 24007, 24008 } // Must have Exploit Corruption OR Heartseeker Shot
            },
            AbilityType = "Hybrid",
            ActionType = "Bonus Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost { Stamina = 20 }, // Per turn upkeep
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            CooldownTurns = 0,
            CooldownType = "None",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Passive: When you use Mark for Death, automatically learn target's Vulnerabilities and precise Corruption level. Active: Enter 'Blight-Stalker's Stance' (toggle). While active: Immune to visual impairment effects, Your Aimed Shots vs High/Extreme Corruption targets have 50% chance to inflict [Staggered] (1 turn), Pay 20 Stamina per turn to maintain, When stance ends, gain 10 Psychic Stress. Rank 2 (20 PP): Passive reveals 1 additional weakness. Active: Stagger chance increases to 70%. Stance upkeep reduced to 15 Stamina/turn. Rank 3 (Capstone): Passive reveals all weaknesses. Active: Stagger chance increases to 90%. While in stance, gain +2d10 to all attack rolls vs corrupted targets. Stress penalty reduced to 5."
        });
    }

    #endregion
}
