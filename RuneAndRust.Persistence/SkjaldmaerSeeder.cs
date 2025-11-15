using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.26.3: Seeds Skjaldmaer specialization for Warrior archetype
/// The bastion of coherence - shields both body and mind, channeling WILL to protect allies from physical trauma and psychic breakdown
/// </summary>
public class SkjaldmaerSeeder
{
    private static readonly ILogger _log = Log.ForContext<SkjaldmaerSeeder>();
    private readonly SpecializationRepository _specializationRepo;
    private readonly AbilityRepository _abilityRepo;

    public SkjaldmaerSeeder(string connectionString)
    {
        _specializationRepo = new SpecializationRepository(connectionString);
        _abilityRepo = new AbilityRepository(connectionString);
    }

    /// <summary>
    /// Seed Skjaldmaer specialization and all 9 abilities (3/3/2/1 tier distribution)
    /// Specialization ID: 26003
    /// Ability IDs: 26019-26027
    /// </summary>
    public void SeedSkjaldmaerSpecialization()
    {
        _log.Information("Seeding Skjaldmaer specialization");

        var skjaldmaer = new SpecializationData
        {
            SpecializationID = 26003,
            Name = "Skjaldmaer",
            ArchetypeID = 1, // Warrior
            PathType = "Coherent",
            MechanicalRole = "Tank / Psychic Stress Mitigation",
            PrimaryAttribute = "STURDINESS",
            SecondaryAttribute = "WILL",
            Description = @"The bastion of coherence—a living firewall against both physical trauma and mental breakdown. In a world where reality glitches, the Skjaldmaer shields not just bodies but sanity itself. Her shield is a grounding rod against the psychic scream of the Great Silence. Her power comes from indomitable WILL channeled into protection, transforming the tank role from 'meat shield' to 'reality anchor.'

This specialization provides dual protection: shields both HP and Psychic Stress simultaneously. As the Trauma Economy anchor, she actively mitigates party Psychic Stress through abilities and auras. Her taunt system draws aggro with WILL-based projection of coherence. Unparalleled damage reduction and HP pool make her the ultimate soak master.

The ultimate expression is Bastion of Sanity—absorb Trauma to save an ally from permanent mental scarring.",
            Tagline = "The Bastion of Coherence",
            UnlockRequirements = new UnlockRequirements { MinLegend = 5, MaxCorruption = 100 },
            ResourceSystem = "Stamina",
            TraumaRisk = "Low",
            IconEmoji = "🛡️",
            PPCostToUnlock = 10,
            IsActive = true
        };

        _specializationRepo.Insert(skjaldmaer);

        // Seed all ability tiers
        SeedSkjaldmaerTier1();
        SeedSkjaldmaerTier2();
        SeedSkjaldmaerTier3();
        SeedSkjaldmaerCapstone();

        _log.Information("Skjaldmaer seeding complete: 9 abilities (3/3/2/1)");
    }

    #region Tier 1: Foundational Resolve (3 abilities, 3 PP each)

    private void SeedSkjaldmaerTier1()
    {
        // Ability 1: Sanctified Resolve (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26019,
            SpecializationID = 26003,
            Name = "Sanctified Resolve",
            Description = "Mental fortitude training grants resistance to Fear and Psychic Stress.",
            MechanicalSummary = "Bonus dice vs [Fear] and Psychic Stress resistance checks",
            TierLevel = 1,
            PPCost = 3,
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
            Notes = "RANK 1: +1 die to WILL Resolve Checks vs. [Fear] and Psychic Stress. RANK 2: +2 dice. RANK 3: +3 dice + reduce ambient Psychic Stress gain by 10%."
        });

        // Ability 2: Shield Bash (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26020,
            SpecializationID = 26003,
            Name = "Shield Bash",
            Description = "Slam shield into foe—a brutal statement of physical truth.",
            MechanicalSummary = "Physical melee attack with [Staggered] chance",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Enemy (Melee)",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
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
            Notes = "RANK 1: 1d8+MIGHT damage, single target, 50% chance [Staggered]. RANK 2: 2d8+MIGHT, 65% Staggered. RANK 3: 3d8+MIGHT, 75% Staggered + Push to Back Row on Stagger."
        });

        // Ability 3: Oath of the Protector (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26021,
            SpecializationID = 26003,
            Name = "Oath of the Protector",
            Description = "Extend protective aura to single ally, shielding flesh and mind.",
            MechanicalSummary = "Single target ally buff: +Soak and +Psychic Stress resistance",
            TierLevel = 1,
            PPCost = 3,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 0 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Single Ally",
            ResourceCost = new AbilityResourceCost { Stamina = 35 },
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "",
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Target ally gains +2 Soak and +1 die vs. Psychic Stress for 2 turns. RANK 2: +3 Soak, +2 dice, 2 turns. RANK 3: +4 Soak, +2 dice, 3 turns + cleanse 1 mental debuff."
        });
    }

    #endregion

    #region Tier 2: Advanced Guardianship (3 abilities, 4 PP each)

    private void SeedSkjaldmaerTier2()
    {
        // Ability 4: Guardians Taunt (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26022,
            SpecializationID = 26003,
            Name = "Guardians Taunt",
            Description = "Projection of coherent will draws even maddened creatures to attack.",
            MechanicalSummary = "Taunt enemies to attack caster; costs Psychic Stress",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "AoE Front Row / All Enemies",
            ResourceCost = new AbilityResourceCost { Stamina = 30 },
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "",
            StatusEffectsApplied = new List<string> { "Taunted" },
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Taunt all Front Row enemies for 2 rounds. Self gains 5 Psychic Stress (cost of drawing trauma). RANK 2: Taunt Front Row, 2 rounds, 3 Stress cost. RANK 3: Taunt ALL enemies (both rows), 2 rounds, 5 Stress cost."
        });

        // Ability 5: Shield Wall (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26023,
            SpecializationID = 26003,
            Name = "Shield Wall",
            Description = "Plant feet creating bastion of physical and metaphysical stability.",
            MechanicalSummary = "AoE defensive buff: +Soak, immune to Push/Pull, +Stress resistance",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Self + Adjacent Front Row Allies",
            ResourceCost = new AbilityResourceCost { Stamina = 45 },
            AttributeUsed = "",
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
            Notes = "RANK 1: Self + adjacent Front Row allies gain +3 Soak, Immune to Push/Pull, +1 die vs. Psychic Stress for 2 turns. RANK 2: +4 Soak, +2 dice, 2 turns. RANK 3: +5 Soak, +2 dice, 3 turns + [Fortified]."
        });

        // Ability 6: Interposing Shield (Reaction)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26024,
            SpecializationID = 26003,
            Name = "Interposing Shield",
            Description = "React to incoming Critical Hit on adjacent ally, redirecting to self.",
            MechanicalSummary = "Reaction: Redirect Critical Hit to self with damage reduction",
            TierLevel = 2,
            PPCost = 4,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 8 },
            AbilityType = "Reaction",
            ActionType = "Reaction",
            TargetType = "Adjacent Ally",
            ResourceCost = new AbilityResourceCost { Stamina = 25 },
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "",
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: Once per round reaction. Redirect Critical Hit to self, take 50% damage. RANK 2: Take 40% damage. RANK 3: Take 30% damage + reflect 10% back to attacker."
        });
    }

    #endregion

    #region Tier 3: Mastery of the Bulwark (2 abilities, 5 PP each)

    private void SeedSkjaldmaerTier3()
    {
        // Ability 7: Implacable Defense (Active)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26025,
            SpecializationID = 26003,
            Name = "Implacable Defense",
            Description = "Achieve state of perfect focus—immovable against physical and mental assault.",
            MechanicalSummary = "Self-buff: Immune to major debuffs and +Soak",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Active",
            ActionType = "Standard Action",
            TargetType = "Self",
            ResourceCost = new AbilityResourceCost { Stamina = 40 },
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "",
            StatusEffectsApplied = new List<string>(),
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: IMMUNE to [Stun], [Staggered], [Knocked Down], [Fear], [Disoriented] for 3 turns. RANK 2: Immune + gain +2 Soak for 3 turns. RANK 3: Immune + +3 Soak + Aura (adjacent allies immune to [Fear]) for 3 turns."
        });

        // Ability 8: Aegis of the Clan (Passive)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26026,
            SpecializationID = 26003,
            Name = "Aegis of the Clan",
            Description = "Automatic protection triggers when ally enters mental crisis.",
            MechanicalSummary = "Passive: Auto-apply Oath of Protector when ally Stress hits 66%+",
            TierLevel = 3,
            PPCost = 5,
            Prerequisites = new AbilityPrerequisites { RequiredPPInTree = 16 },
            AbilityType = "Passive",
            ActionType = "Free Action",
            TargetType = "Allies in Crisis",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "",
            BonusDice = 0,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "",
            CooldownTurns = 0,
            MaxRank = 3,
            CostToRank2 = 20,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "RANK 1: When any ally Psychic Stress enters High threshold (66%+), automatically apply Oath of the Protector to them for 1 turn (free, once per ally per combat). RANK 2: Applies for 2 turns. RANK 3: Applies for 2 turns + reduce their Stress by 10 immediately."
        });
    }

    #endregion

    #region Capstone: Ultimate Expression (1 ability, 6 PP)

    private void SeedSkjaldmaerCapstone()
    {
        // Ability 9: Bastion of Sanity (Passive+Reaction)
        _abilityRepo.Insert(new AbilityData
        {
            AbilityID = 26027,
            SpecializationID = 26003,
            Name = "Bastion of Sanity",
            Description = "Become living Runic Anchor—a kernel of stable reality.",
            MechanicalSummary = "Passive aura + Reaction: Absorb ally's permanent Trauma",
            TierLevel = 4,
            PPCost = 6,
            Prerequisites = new AbilityPrerequisites
            {
                RequiredPPInTree = 24,
                RequiredAbilityIDs = new List<int> { 26025, 26026 } // Must have Implacable Defense OR Aegis of the Clan
            },
            AbilityType = "Passive+Reaction",
            ActionType = "Passive / Reaction",
            TargetType = "All Allies (Aura) / Ally in Crisis (Reaction)",
            ResourceCost = new AbilityResourceCost(),
            AttributeUsed = "will",
            BonusDice = 1,
            SuccessThreshold = 0,
            DamageDice = 0,
            DamageType = "",
            CooldownTurns = 1,
            CooldownType = "Once Per Combat",
            MaxRank = 1,
            CostToRank2 = 0,
            CostToRank3 = 0,
            IsActive = true,
            Notes = "PASSIVE AURA: While in Front Row, all allies in row gain +1 WILL and -10% ambient Psychic Stress gain. REACTION (once per combat): When ally would gain permanent Trauma from Breaking Point, Skjaldmaer absorbs it—ally avoids Trauma, Skjaldmaer takes 40 Psychic Stress + 1 Corruption."
        });
    }

    #endregion
}
