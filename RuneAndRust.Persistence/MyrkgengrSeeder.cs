using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.24.2: Seeds Myrk-gengr (Shadow-Walker) specialization for Skirmisher archetype
/// Perceptual predator who weaponizes psychic static, delivers terror strikes, vanishes into sensory voids
/// </summary>
public class MyrkgengrSeeder
{
    private static readonly ILogger _log = Log.ForContext<MyrkgengrSeeder>();
    private readonly SpecializationRepository _specializationRepo;
    private readonly AbilityRepository _abilityRepo;

    public MyrkgengrSeeder(string connectionString)
    {
        _specializationRepo = new SpecializationRepository(connectionString);
        _abilityRepo = new AbilityRepository(connectionString);
    }

    /// <summary>
    /// Seed Myrk-gengr specialization and all 9 abilities (3/3/2/1 tier distribution)
    /// Specialization ID: 24002
    /// Ability IDs: 24010-24018
    /// </summary>
    public void SeedMyrkgengrSpecialization()
    {
        _log.Information("Seeding Myrk-gengr (Shadow-Walker) specialization");

        var myrkgengr = new SpecializationData
        {
            SpecializationID = 24002,
            Name = "Myrk-gengr",
            ArchetypeID = 4, // Skirmisher
            PathType = "Heretical",
            MechanicalRole = "Stealth Assassin / Alpha Strike",
            PrimaryAttribute = "FINESSE",
            SecondaryAttribute = "WILL",
            Description = @"You are the ghost in the machine. You've learned to wrap yourself in the world's psychic static, becoming a blind spot in enemy perception. Your attacks from stealth don't just deal physical damage—they inflict psychological terror, shattering minds alongside bodies.

Your capstone ability lets you become a living glitch, a reality-warping violation of causality. You are the predator who strikes from places the mind insists are empty. The ultimate alpha strike specialist who deletes high-priority targets before they can act.",
            Tagline = "The Ghost in the Machine",
            UnlockRequirements = new UnlockRequirements { MinLegend = 5, MaxCorruption = 100 },
            ResourceSystem = "Stamina + Focus",
            TraumaRisk = "High",
            IconEmoji = "🌑",
            PPCostToUnlock = 3,
            IsActive = true
        };

        _specializationRepo.Insert(myrkgengr);

        // Seed all ability tiers
        SeedMyrkgengrTier1();
        SeedMyrkgengrTier2();
        SeedMyrkgengrTier3();
        SeedMyrkgengrCapstone();

        _log.Information("Myrk-gengr seeding complete: 9 abilities (3/3/2/1)");
    }

    #region Tier 1: Foundational Shadows (3 abilities, 3 PP each)

    private void SeedMyrkgengrTier1()
    {
        // Ability 1: One with the Static I (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 24010,
            SpecializationID = 24002,
            Name = "One with the Static I",
            Description = "You find comfort in the world's background noise. The hum of the Blight is not a threat—it is camouflage.",
            MechanicalSummary = "+1d10 to Stealth checks; +2d10 additional in [Psychic Resonance] zones",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
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
            Notes = "Rank 1: +1d10 to FINESSE-based Stealth checks. +2d10 additional in [Psychic Resonance] zones. Rank 2 (20 PP in tree): Base bonus increases to +2d10. Partial immunity to [Psychic Resonance] penalties (ignore -1d10 to other checks). Rank 3 (Capstone): Base bonus increases to +3d10. In [Psychic Resonance] zones, enemies have -2d10 to detect you."
        });

        // Ability 2: Enter the Void (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 24011,
            SpecializationID = 24002,
            Name = "Enter the Void",
            Description = "You focus your will, synchronizing your presence with the world's psychic static. You vanish from sight—not hidden in shadow, but erased from perception.",
            MechanicalSummary = "Make Stealth check (DC 16) to enter [Hidden] state",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
            AttributeUsed = "finesse",
            BonusDice = 0,
            SuccessThreshold = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            CooldownTurns = 0,
            IsActive = true,
            Notes = "Rank 1: Make FINESSE + Acrobatics (Stealth) check vs DC 16 to enter [Hidden] state. While Hidden: enemies cannot target you directly, +2d10 Defense (from Ghostly Form). Rank 2 (20 PP): DC reduced to 14. Stamina cost reduced to 35. Rank 3 (Capstone): DC reduced to 12. Can use as Bonus Action instead of Standard Action."
        });

        // Ability 3: Shadow Strike (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 24012,
            SpecializationID = 24002,
            Name = "Shadow Strike",
            Description = "A precise, brutal attack from a blind spot. The blade finds its mark before their corrupted processors can register the threat.",
            MechanicalSummary = "From [Hidden]: Guaranteed crit (double damage); immediately breaks stealth",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Melee)",
            ResourceCost = new AbilityResourceCost { Stamina = 35 },
            AttributeUsed = "finesse",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0, // Weapon damage, doubled as crit
            DamageType = "Physical",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            CooldownTurns = 0,
            IsActive = true,
            Notes = "Rank 1: FINESSE-based melee attack. Requires [Hidden] state. Guaranteed critical hit (double damage). Immediately breaks stealth (unless Ghostly Form procs). Rank 2 (20 PP): Damage increased by +2d6. If this kills target, refund 20 Stamina. Rank 3 (Capstone): Damage increased by +4d6 (total). Apply [Bleeding] for 2 turns (2d6/turn) on hit."
        });
    }

    #endregion

    #region Tier 2: Advanced Perceptual Warfare (3 abilities, 4 PP each)

    private void SeedMyrkgengrTier2()
    {
        // Ability 4: Throat-Cutter (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 24013,
            SpecializationID = 24002,
            Name = "Throat-Cutter",
            Description = "You strike from behind with lethal precision, severing vocal cords. Their screams are silenced before they can escape.",
            MechanicalSummary = "Melee attack + bonus damage; from flank/Hidden: apply [Silenced]",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Melee)",
            ResourceCost = new AbilityResourceCost { Stamina = 45 },
            AttributeUsed = "finesse",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 2, // 2d8 base
            DamageType = "Physical",
            StatusEffectsApplied = new List<string> { "Silenced" },
            CooldownTurns = 3,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Melee attack dealing weapon damage + 2d8. If attacking from flanking or [Hidden]: apply [Silenced] for 1 turn (prevents vocal abilities). Rank 2 (20 PP): Bonus damage increases to 3d8. [Silenced] duration increases to 2 turns. Rank 3 (Capstone): Bonus damage increases to 4d8. If target was [Feared] when hit, also apply [Bleeding] (2d6/turn, 3 turns)."
        });

        // Ability 5: Sensory Scramble (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 24014,
            SpecializationID = 24002,
            Name = "Sensory Scramble",
            Description = "You shatter a dart of Blighted reagents, releasing powder that overloads senses with corrupted data. A temporary zone of pure psychic noise.",
            MechanicalSummary = "Create [Psychic Resonance] zone: enemies -1d10 Perception, you +2d10 Stealth",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Target Row",
            ResourceCost = new AbilityResourceCost { Stamina = 30 },
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            StatusEffectsApplied = new List<string> { "Psychic Resonance" },
            CooldownTurns = 4,
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Create [Psychic Resonance] zone in target row for 2 turns. Enemies in zone: -1d10 Perception. You: +2d10 to Enter the Void checks while in zone. Requires 1 Alchemical Component. Rank 2 (20 PP): Duration increases to 3 turns. Zone also inflicts 1d6 Psychic Stress/turn to enemies within. Rank 3 (Capstone): Duration increases to 4 turns. You can move through zone without breaking stealth (normally movement requires new stealth check)."
        });

        // Ability 6: Mind of Stillness (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 24015,
            SpecializationID = 24002,
            Name = "Mind of Stillness",
            Description = "To manipulate the static, your mind must be a fortress of perfect calm. You meditate within chaos, becoming the quiet center of the psychic storm.",
            MechanicalSummary = "While [Hidden]: remove Psychic Stress per turn, regenerate Stamina per turn",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: While in [Hidden] state, remove 3 Psychic Stress per turn and regenerate 5 Stamina per turn. Rank 2 (20 PP): Stress removal increases to 5/turn. Stamina regen increases to 8/turn. Rank 3 (Capstone): Stress removal increases to 7/turn. Stamina regen increases to 10/turn. Also gain +1d10 to Resolve checks while Hidden."
        });
    }

    #endregion

    #region Tier 3: Mastery of the Unseen (2 abilities, 5 PP each)

    private void SeedMyrkgengrTier3()
    {
        // Ability 7: Terror from the Void (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 24016,
            SpecializationID = 24002,
            Name = "Terror from the Void",
            Description = "You have mastered the art of the psychological alpha strike. The sheer shock and terror of your initial assault shatters minds.",
            MechanicalSummary = "First Shadow Strike per combat: inflict Psychic Stress + high chance [Feared]",
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
            StatusEffectsApplied = new List<string> { "Feared" },
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: The first Shadow Strike you perform in each combat inflicts 12 Psychic Stress on target. 70% chance to apply [Feared] for 2 turns. Rank 2 (20 PP): Psychic Stress increases to 15. Fear chance increases to 85%. Fear duration: 3 turns. Rank 3 (Capstone): Psychic Stress increases to 18. Fear chance increases to 100% (guaranteed). All enemies who witness the attack (same row or adjacent) also make Resolve check vs [Feared] (DC 18)."
        });

        // Ability 8: Ghostly Form (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 24017,
            SpecializationID = 24002,
            Name = "Ghostly Form",
            Description = "Your connection to the world's static becomes so profound you are no longer just hiding—you are a part of it. Your form flickers and desynchronizes.",
            MechanicalSummary = "While [Hidden]: +Defense; chance to stay Hidden after Shadow Strike",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "",
            BonusDice = 2, // +2d10 Defense at Rank 1
            SuccessThreshold = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: While [Hidden], gain +2d10 Defense. After attacking with Shadow Strike, 50% chance to remain [Hidden] (stealth doesn't break). Rank 2 (20 PP): Defense bonus increases to +3d10. Stealth persistence chance increases to 65%. Rank 3 (Capstone): Defense bonus increases to +4d10. Stealth persistence chance increases to 80%. If stealth persists, gain Free Move action (reposition without breaking stealth)."
        });
    }

    #endregion

    #region Capstone: Ultimate Expression (1 ability, 6 PP)

    private void SeedMyrkgengrCapstone()
    {
        // Ability 9: Living Glitch (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 24018,
            SpecializationID = 24002,
            Name = "Living Glitch",
            Description = "For a single, horrifying moment, you do not hide in a glitch—you become one. You de-compile your own physical presence, stepping outside the world's logical grid to deliver a blow that is a fundamental violation of causality.",
            MechanicalSummary = "From [Hidden]: Guaranteed hit, massive damage + catastrophic Psychic Stress, 18 self-Corruption, breaks stealth",
            TierLevel = 4,
            PPCost = 6,
            Prerequisites = new AbilityPrerequisites
            {
                RequiredPPInTree = 24,
                RequiredAbilityIDs = new List<int> { 24016, 24017 } // Must have Terror from Void OR Ghostly Form
            },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Melee)",
            ResourceCost = new AbilityResourceCost { Stamina = 60, Corruption = 18 },
            AttributeUsed = "finesse",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 8, // 8d10 base
            DamageType = "Physical",
            CooldownTurns = 999, // Once per combat
            CooldownType = "Per Combat",
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "Rank 1: Requires [Hidden] state. Guaranteed hit that cannot be parried, dodged, or blocked. Deals massive Physical damage (8d10 + weapon damage + FINESSE × 2). Inflicts 25 Psychic Stress (catastrophic). You gain 18 Corruption (forcing your own reality-glitch). Immediately breaks stealth. Requires 75 Focus. Once per combat. Rank 2 (20 PP): Base damage increases to 10d10. Psychic Stress increases to 30. Self-Corruption reduced to 15. Rank 3 (Capstone): Base damage increases to 12d10. Psychic Stress increases to 35. Self-Corruption reduced to 12. If this kills target, you do not break stealth."
        });
    }

    #endregion
}
