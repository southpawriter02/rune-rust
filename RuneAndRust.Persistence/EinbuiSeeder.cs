using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.27.2: Seeds Einbui specialization for Adept archetype
/// Ultimate survivalist embodying radical self-reliance through mastery of tracking, foraging, trapping, and camp craft
/// Makes extended expeditions possible through resource generation and safe rest creation
/// </summary>
public class EinbuiSeeder
{
    private static readonly ILogger _log = Log.ForContext<EinbuiSeeder>();
    private readonly SpecializationRepository _specializationRepo;
    private readonly AbilityRepository _abilityRepo;

    public EinbuiSeeder(string connectionString)
    {
        _specializationRepo = new SpecializationRepository(connectionString);
        _abilityRepo = new AbilityRepository(connectionString);
    }

    /// <summary>
    /// Seed Einbui specialization and all 9 abilities (3/3/2/1 tier distribution)
    /// Specialization ID: 27002
    /// Ability IDs: 27010-27018
    /// </summary>
    public void SeedEinbuiSpecialization()
    {
        _log.Information("Seeding Einbui specialization");

        var einbui = new SpecializationData
        {
            SpecializationID = 27002,
            Name = "Einbui",
            ArchetypeID = 2, // Adept
            PathType = "Coherent",
            MechanicalRole = "Survival Specialist / Resource Generation / Exploration Support",
            PrimaryAttribute = "WITS",
            SecondaryAttribute = "FINESSE",
            Description = @"The ultimate survivalist embodying radical self-reliance through mastery of survival fundamentals. You are not a specialist but a grim generalist—you have learned a little about everything out of necessity.

Where others focus on single complex disciplines, you master tracking, brewing, trapping, navigating, foraging, and camp craft. You are never unprepared, never lacking the right tool. You are the reason parties survive deep wilderness.

Your entire value lies in exploration, survival, and logistics. You provide zero combat power but make extended expeditions possible through field crafting, resource location, and creating guaranteed-safe rest locations. The ultimate expression is Blight Haven—designating a room as a perfectly safe rest location once per expedition.",
            Tagline = "Loner - Master of Radical Self-Reliance",
            UnlockRequirements = new UnlockRequirements { MinLegend = 5, MaxCorruption = 100 },
            ResourceSystem = "Stamina",
            TraumaRisk = "None",
            IconEmoji = "🏕️",
            PPCostToUnlock = 10,
            IsActive = true
        };

        _specializationRepo.Insert(einbui);

        // Seed all ability tiers
        SeedEinbuiTier1();
        SeedEinbuiTier2();
        SeedEinbuiTier3();
        SeedEinbuiCapstone();

        _log.Information("Einbui seeding complete: 9 abilities (3/3/2/1)");
    }

    #region Tier 1: Foundational Survival (3 abilities, 3 PP each)

    private void SeedEinbuiTier1()
    {
        // Ability 1: Radical Self-Reliance I (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 27010,
            SpecializationID = 27002,
            Name = "Radical Self-Reliance I",
            Description = "Constant adaptation grants baseline competence in core survival tenets.",
            MechanicalSummary = "Bonus dice to Wasteland Survival checks (Tracking and Foraging)",
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
            Notes = "RANK 1: +1 die to Wasteland Survival (Tracking) and Wasteland Survival (Foraging) checks. Stacks with Wasteland Survival +3 skill bonus. RANK 2: +2 dice. RANK 3: +2 dice + automatically succeed on Easy (DC 10) survival checks."
        });

        // Ability 2: Improvised Trap (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 27011,
            SpecializationID = 27002,
            Name = "Improvised Trap",
            Description = "Assemble simple but effective snare using scavenged wire and sinew.",
            MechanicalSummary = "Place trap that roots first enemy stepping on it",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Battlefield Tile (within 2 tiles)",
            ResourceCost = new AbilityResourceCost { Stamina = 15 },
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            StatusEffectsApplied = new List<string> { "Rooted" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Standard Action. Consumes 1 [Scrap Metal] + 1 [Tough Leather]. Place trap on tile within 2 tiles. First enemy stepping on trap: Apply [Rooted] for 1 turn. RANK 2: [Rooted] 2 turns. RANK 3: [Rooted] 2 turns + [Bleeding] (1d4 per turn for 3 turns)."
        });

        // Ability 3: Basic Concoction (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 27012,
            SpecializationID = 27002,
            Name = "Basic Concoction",
            Description = "Crush common herbs and mix with cloth/water to create crude remedy.",
            MechanicalSummary = "Field craft healing poultice or stamina stimulant",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Self (creates held item)",
            ResourceCost = new AbilityResourceCost { Stamina = 10 },
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            HealingDice = 2,
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Standard Action. Consumes 1 [Common Herb] + 1 [Clean Cloth]. Choose: [Crude Poultice] (restore 2d6 HP) OR [Weak Stimulant] (restore 15 Stamina). Can hold max 3. Using held item = Free Action. RANK 2: [Refined Poultice] (4d6 HP) OR [Potent Stimulant] (30 Stamina). RANK 3: [Superior Poultice] (6d6 HP + remove [Bleeding]) OR [Exceptional Stimulant] (45 Stamina + remove [Exhausted])."
        });
    }

    #endregion

    #region Tier 2: Advanced Adaptation (3 abilities, 4 PP each)

    private void SeedEinbuiTier2()
    {
        // Ability 4: Resourceful Eye (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 27013,
            SpecializationID = 27002,
            Name = "Resourceful Eye",
            Description = "Trained eyes spot resources where others see rubble.",
            MechanicalSummary = "Reveal all hidden Resource Nodes in current room",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Current Room (Out of Combat)",
            ResourceCost = new AbilityResourceCost { Stamina = 20 },
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 12,
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Standard Action (out of combat). WITS + Wasteland Survival check (DC 12). Success: Reveal all hidden Resource Nodes in current room. Once per room. RANK 2: DC reduced to 10. RANK 3: DC 10 + also reveals hidden passages and traps."
        });

        // Ability 5: Radical Self-Reliance II (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 27014,
            SpecializationID = 27002,
            Name = "Radical Self-Reliance II",
            Description = "Skills for ruin survival—stealth, climbing, navigation—now second nature.",
            MechanicalSummary = "Bonus dice to Acrobatics checks (Stealth and Climbing)",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "finesse",
            BonusDice = 1,
            SuccessThreshold = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: +1 die to Acrobatics (Stealth) and Acrobatics (Climbing) checks. Stacks with Acrobatics +1 skill bonus. RANK 2: +2 dice. RANK 3: +2 dice + can climb at full movement speed (no penalty)."
        });

        // Ability 6: Wasteland Wanderer (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 27015,
            SpecializationID = 27002,
            Name = "Wasteland Wanderer",
            Description = "Body hardened from harsh exposure. Toxins and hazards that kill others merely inconvenience you.",
            MechanicalSummary = "Resistance to environmental hazards and status effects",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "sturdiness",
            BonusDice = 1,
            SuccessThreshold = 0,
            // StatusEffectsResisted = new List<string> { "Poisoned", "Disease" }, // Property removed from AbilityData
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Resistance to environmental hazard damage (half damage). +1 die to STURDINESS Resolve vs environmental effects. +1 die to resist [Poisoned] and [Disease]. RANK 2: +2 dice to all resistances. RANK 3: +2 dice + can rest safely in mildly hazardous environments (ignore Mild ambient hazards during rest)."
        });
    }

    #endregion

    #region Tier 3: Mastery of the Wastes (2 abilities, 5 PP each)

    private void SeedEinbuiTier3()
    {
        // Ability 7: Master Improviser (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 27016,
            SpecializationID = 27002,
            Name = "Master Improviser",
            Description = "Field crafting reaches peak efficiency. Hands move with practiced mastery.",
            MechanicalSummary = "Upgrade Improvised Trap and Basic Concoction to Rank 3 effects automatically",
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
            Notes = "RANK 1: Improvised Trap upgraded to Rank 3 effects automatically. Basic Concoction upgraded to Rank 3 effects automatically. Stamina costs unchanged. RANK 2: Field crafting costs -5 Stamina (min 5). RANK 3: -5 Stamina + can craft 2 items per Standard Action."
        });

        // Ability 8: Live off the Land (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 27017,
            SpecializationID = 27002,
            Name = "Live off the Land",
            Description = "True child of the wastes. Find sustenance where others starve.",
            MechanicalSummary = "Reduce party ration consumption, automatically find herbs and water",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self and Party",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "wits",
            BonusDice = 0,
            SuccessThreshold = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: No longer need [Rations] to avoid [Exhausted] during Wilderness Rest. Once per day: Wasteland Survival check to find [Clean Water] for party (even in barren environments). Automatically find 1d3 [Common Herbs] when foraging (no check). Reduce party Ration consumption by 25%. RANK 2: Find 2d3 [Common Herbs]. Reduce Rations by 40%. RANK 3: Find 3d3 [Common Herbs]. Reduce Rations by 50% + automatically find edible food (no Ration consumption for party during Wilderness Rest if Einbui present)."
        });
    }

    #endregion

    #region Capstone: The Ultimate Survivor (1 ability, 6 PP)

    private void SeedEinbuiCapstone()
    {
        // Ability 9: The Ultimate Survivor (Passive + Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 27018,
            SpecializationID = 27002,
            Name = "The Ultimate Survivor",
            Description = "Achieved perfect harmony with harsh wastes. Never without the right skill, never without a solution.",
            MechanicalSummary = "Universal skill competence + Blight Haven (guaranteed safe rest once per expedition)",
            TierLevel = 4,
            PPCost = 6,
            Prerequisites = new AbilityPrerequisites
            {
                RequiredPPInTree = 24,
                RequiredAbilityIDs = new List<int> { 27016, 27017 } // Must have Master Improviser OR Live off the Land
            },
            AbilityType = "Passive+Active",
            ActionType = "Free Action (Passive) / Standard Action (Blight Haven)",
            TargetType = "Self (Passive) / Cleared Room (Active)",
            ResourceCost = new AbilityResourceCost { Stamina = 0 },
            AttributeUsed = "",
            BonusDice = 1,
            SuccessThreshold = 0,
            CooldownTurns = 1,
            CooldownType = "Once Per Expedition (Blight Haven only)",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "PASSIVE: Universal Competence - Gain +1 die to ALL non-combat skill checks not already proficient in. ACTIVE: Blight Haven - Once per expedition (out of combat). Designate cleared room as [Hidden Camp]. Effects during rest here: 0% Ambush Chance (guaranteed safety), all Wilderness Rest benefits, party gains +10 to recovery rolls, protected from environmental Psychic Stress gain. RANK 2: +2 dice Universal Competence. Blight Haven also grants +20 recovery. RANK 3: +2 dice + Blight Haven grants +30 recovery + party can perform advanced crafting in Haven without station."
        });
    }

    #endregion
}
