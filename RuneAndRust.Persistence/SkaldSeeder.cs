using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.27.1: Seeds Skald specialization for Adept archetype
/// The keeper of coherent narratives who wields structured verse as weapon and shield
/// Creates narrative firewalls that fortify allies' minds and break enemy morale through performance
/// </summary>
public class SkaldSeeder
{
    private static readonly ILogger _log = Log.ForContext<SkaldSeeder>();
    private readonly SpecializationRepository _specializationRepo;
    private readonly AbilityRepository _abilityRepo;

    public SkaldSeeder(string connectionString)
    {
        _specializationRepo = new SpecializationRepository(connectionString);
        _abilityRepo = new AbilityRepository(connectionString);
    }

    /// <summary>
    /// Seed Skald specialization and all 9 abilities (3/3/2/1 tier distribution)
    /// Specialization ID: 27001
    /// Ability IDs: 27001-27009
    /// </summary>
    public void SeedSkaldSpecialization()
    {
        _log.Information("Seeding Skald specialization");

        var skald = new SpecializationData
        {
            SpecializationID = 27001,
            Name = "Skald",
            ArchetypeID = 2, // Adept
            PathType = "Coherent",
            MechanicalRole = "Performance Buffer / Trauma Economy Support",
            PrimaryAttribute = "WILL",
            SecondaryAttribute = "WITS",
            Description = @"The keeper of coherent narratives in a world whose story has shattered. You are a warrior-poet who wields structured verse as both weapon and shield, creating 'narrative firewalls'—pockets of logic and meaning that fortify allies' minds against psychic static or break enemies' morale with the weight of foreseen doom.

You are not a mystic but a coherence-keeper, proving that in a glitching reality, a well-told story is tangible power. Your performances channel battlefield-wide effects, steadying allies' resolve and unnerving intelligent foes through the sheer narrative weight of sagas and dirges.

The ultimate expression is Saga of the Einherjar—a masterpiece performance that elevates allies to legendary status, granting massive temporary power at the cost of psychic exhaustion when the saga ends.",
            Tagline = "Chronicler of Coherence",
            UnlockRequirements = new UnlockRequirements { MinLegend = 5, MaxCorruption = 100 },
            ResourceSystem = "Stamina",
            TraumaRisk = "Low",
            IconEmoji = "📜",
            PPCostToUnlock = 10,
            IsActive = true
        };

        _specializationRepo.Insert(skald);

        // Seed all ability tiers
        SeedSkaldTier1();
        SeedSkaldTier2();
        SeedSkaldTier3();
        SeedSkaldCapstone();

        _log.Information("Skald seeding complete: 9 abilities (3/3/2/1)");
    }

    #region Tier 1: Foundational Performance (3 abilities, 3 PP each)

    private void SeedSkaldTier1()
    {
        // Ability 1: Oral Tradition (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 27001,
            SpecializationID = 27001,
            Name = "Oral Tradition",
            Description = "The great sagas are part of the Skald's very being, carried in verse and cadence.",
            MechanicalSummary = "Bonus to Rhetoric and historical lore checks; perfect recall at Rank 3",
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
            Notes = "RANK 1: +1 die to Rhetoric checks and investigate checks for historical lore. RANK 2: +2 dice. RANK 3: +2 dice + can recall any historical fact with DC 15 WITS check."
        });

        // Ability 2: Saga of Courage (Performance)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 27002,
            SpecializationID = 27001,
            Name = "Saga of Courage",
            Description = "Rousing chant of a hero who stood against overwhelming odds. Creates pocket of coherence steadying allies' resolve.",
            MechanicalSummary = "Performance: Grant allies Fear immunity and Psychic Stress resistance",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "All Allies (Aura)",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "",
            StatusEffectsApplied = new List<string> { "FearImmunity", "PsychicStressResistance" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Performance. While performing, all allies IMMUNE to [Feared] + +1 die to WILL Resolve vs Psychic Stress. Duration: WILL rounds. RANK 2: +2 dice to Stress resistance. RANK 3: +2 dice + allies also gain +1 die to resist [Disoriented]."
        });

        // Ability 3: Dirge of Defeat (Performance)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 27003,
            SpecializationID = 27001,
            Name = "Dirge of Defeat",
            Description = "Sorrowful dirge recounting doom of a great army. Narrative weight unnerves intelligent foes.",
            MechanicalSummary = "Performance: Debuff intelligent enemies with accuracy and damage penalty",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "All Intelligent Enemies (Aura)",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "",
            StatusEffectsApplied = new List<string> { "Unnerved" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Performance. While performing, all intelligent enemies suffer -1 die penalty to Accuracy and damage. Does not affect mindless/Undying. Duration: WILL rounds. RANK 2: -2 dice penalty. RANK 3: -2 dice + intelligent enemies take 1d4 Psychic damage per turn from narrative weight."
        });
    }

    #endregion

    #region Tier 2: Advanced Composition (3 abilities, 4 PP each)

    private void SeedSkaldTier2()
    {
        // Ability 4: Rousing Verse (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 27004,
            SpecializationID = 27001,
            Name = "Rousing Verse",
            Description = "Quick verse from saga about tireless warrior, banishing fatigue through structured recollection.",
            MechanicalSummary = "Restore Stamina to single ally, remove Exhausted at Rank 3",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Ally",
            ResourceCost = new AbilityResourceCost { Stamina = 35 },
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            HealingDice = 0,
            StatusEffectsRemoved = new List<string>(),
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: NOT a Performance. Standard Action. Restore 15 + (WILL × 2) Stamina to single ally. RANK 2: Restore 20 + (WILL × 3) Stamina. RANK 3: Restore 25 + (WILL × 3) Stamina + remove [Exhausted] status."
        });

        // Ability 5: Song of Silence (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 27005,
            SpecializationID = 27001,
            Name = "Song of Silence",
            Description = "Counter-resonant chant designed to disrupt hostile vocalizations and choke caster's words.",
            MechanicalSummary = "Silence single intelligent enemy with opposed check",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Intelligent Enemy",
            ResourceCost = new AbilityResourceCost { Stamina = 45 },
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "Psychic",
            StatusEffectsApplied = new List<string> { "Silenced" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Standard Action. Opposed WILL + Rhetoric vs single intelligent enemy. Success: Apply [Silenced] for 2 rounds. RANK 2: [Silenced] for 3 rounds. RANK 3: [Silenced] 3 rounds + target takes 2d6 Psychic damage from vocal disruption."
        });

        // Ability 6: Enduring Performance (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 27006,
            SpecializationID = 27001,
            Name = "Enduring Performance",
            Description = "Honed vocal endurance allows maintaining powerful performances longer.",
            MechanicalSummary = "Increase all Performance durations; maintain 2 performances at Rank 3",
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
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: All Performance durations increased by +2 rounds. RANK 2: +3 rounds. RANK 3: +4 rounds + can maintain 2 Performances simultaneously (costs both Stamina costs, requires 2 Standard Actions to initiate)."
        });
    }

    #endregion

    #region Tier 3: Mastery of the Saga (2 abilities, 5 PP each)

    private void SeedSkaldTier3()
    {
        // Ability 7: Lay of the Iron Wall (Performance)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 27007,
            SpecializationID = 27001,
            Name = "Lay of the Iron Wall",
            Description = "Story of unbreakable shield wall at Battle of Black Pass. Narrative imposes structural coherence on formation.",
            MechanicalSummary = "Performance: Grant Front Row allies +Soak and Push/Pull resistance",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "All Front Row Allies (Aura)",
            ResourceCost = new AbilityResourceCost { Stamina = 55 },
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "",
            StatusEffectsApplied = new List<string> { "Fortified" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Performance. While performing, all Front Row allies gain +2 Soak. Duration: WILL rounds. RANK 2: +3 Soak. RANK 3: +4 Soak + Front Row allies also gain Resistance to Push/Pull effects."
        });

        // Ability 8: Heart of the Clan (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 27008,
            SpecializationID = 27001,
            Name = "Heart of the Clan",
            Description = "The Skald is the living heart of their clan. Their presence is a source of unshakeable resolve.",
            MechanicalSummary = "Passive aura: Allies in row gain bonus to defensive Resolve checks",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Allies in Same Row",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "",
            BonusDice = 1,
            SuccessThreshold = 0,
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: All allies in same row as Skald gain +1 die to defensive Resolve Checks (Fear, Disoriented). Inactive if Skald [Stunned], [Feared], or [Silenced]. RANK 2: +2 dice. RANK 3: +2 dice + aura extends to adjacent row."
        });
    }

    #endregion

    #region Capstone: Ultimate Expression (1 ability, 6 PP)

    private void SeedSkaldCapstone()
    {
        // Ability 9: Saga of the Einherjar (Capstone Performance)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 27009,
            SpecializationID = 27001,
            Name = "Saga of the Einherjar",
            Description = "Masterpiece saga of greatest heroes. Allies believe themselves elevated to legendary state.",
            MechanicalSummary = "Ultimate Performance: Massive ally buffs with Psychic Stress cost at end",
            TierLevel = 4,
            PPCost = 6,
            Prerequisites = new AbilityPrerequisites
            {
                RequiredPPInTree = 24,
                RequiredAbilityIDs = new List<int> { 27007, 27008 } // Must have Lay of the Iron Wall OR Heart of the Clan
            },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "All Allies (Aura)",
            ResourceCost = new AbilityResourceCost { Stamina = 75 },
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "",
            StatusEffectsApplied = new List<string> { "Inspired" },
            CooldownTurns = 1,
            CooldownType = "Once Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Performance. Once per combat. While performing, all allies gain [Inspired] (+3 dice to damage) + 20 temp HP. When performance ends, all affected allies take 10 Psychic Stress. Duration: WILL rounds. RANK 2: +4 dice + 30 temp HP, 8 Stress cost. RANK 3: +5 dice + 40 temp HP, 6 Stress cost + allies immune to [Feared] and [Stunned] during performance."
        });
    }

    #endregion
}
