using RuneAndRust.Core;
using RuneAndRust.Core.Archetypes;
using Serilog;

namespace RuneAndRust.Engine;

public class CharacterFactory
{
    private static readonly ILogger _log = Log.ForContext<CharacterFactory>();

    /// <summary>
    /// v0.41: Create character with optional account progression integration
    /// </summary>
    public static PlayerCharacter CreateCharacter(
        CharacterClass characterClass,
        string name = "Survivor",
        int? accountId = null,
        AccountProgressionService? accountService = null,
        string? alternativeStartId = null,
        AlternativeStartService? alternativeStartService = null)
    {
        _log.Information("Creating character: Name={Name}, Class={Class}, AccountID={AccountId}, AlternativeStart={AlternativeStart}",
            name, characterClass, accountId, alternativeStartId);

        var character = new PlayerCharacter
        {
            Name = name,
            Class = characterClass
        };

        switch (characterClass)
        {
            case CharacterClass.Warrior:
                InitializeWarrior(character);
                break;
            case CharacterClass.Scavenger:
                InitializeScavenger(character);
                break;
            case CharacterClass.Mystic:
                InitializeMystic(character);
                break;
            case CharacterClass.Adept:
                InitializeAdept(character);
                break;
            case CharacterClass.Skirmisher:
                InitializeSkirmisher(character);
                break;
        }

        // [v0.5] Add heretical abilities (available to all classes)
        AddHereticalAbilities(character);

        // [v0.9] Set starting currency
        character.Currency = 50; // Starting Dvergr Cogs

        // [v0.41] Apply account unlocks and alternative starts
        if (accountId.HasValue && accountService != null)
        {
            _log.Information("Applying account unlocks for AccountID={AccountId}", accountId.Value);
            accountService.ApplyAccountUnlocksToCharacter(accountId.Value, character);

            // Track character creation
            accountService.OnCharacterCreated(accountId.Value);
        }

        // [v0.41] Apply alternative start scenario
        if (accountId.HasValue && !string.IsNullOrEmpty(alternativeStartId) && alternativeStartService != null)
        {
            _log.Information("Applying alternative start: {StartId}", alternativeStartId);
            alternativeStartService.InitializeCharacterWithScenario(accountId.Value, character, alternativeStartId);
        }

        _log.Information("Character created successfully: Name={Name}, Class={Class}, HP={HP}, Stamina={Stamina}, Currency={Currency}, Abilities={AbilityCount}, AccountUnlocksApplied={UnlocksApplied}",
            character.Name, character.Class, character.HP, character.Stamina, character.Currency, character.Abilities.Count, accountId.HasValue);

        return character;
    }

    private static void InitializeWarrior(PlayerCharacter character)
    {
        // v0.7.1: Create formal archetype instance
        var warriorArchetype = new WarriorArchetype();
        character.Archetype = warriorArchetype;

        // Stats from archetype
        character.Attributes = warriorArchetype.GetBaseAttributes();

        // Resources (base values before equipment)
        character.MaxHP = 50;
        character.HP = 50;
        character.MaxStamina = 30;
        character.Stamina = 30;
        character.AP = 10;

        // Legacy weapon (v0.1/v0.2 compatibility)
        character.WeaponName = "Rusty Hatchet";
        character.WeaponAttribute = "might";
        character.BaseDamage = 1;

        // Starting equipment (v0.3)
        var startingWeapon = EquipmentDatabase.GetByName("Rusty Hatchet");
        var startingArmor = EquipmentDatabase.GetByName("Scrap Plating");

        if (startingWeapon != null)
        {
            character.EquippedWeapon = startingWeapon;
        }

        if (startingArmor != null)
        {
            var equipmentService = new EquipmentService();
            character.EquippedArmor = startingArmor;
            equipmentService.RecalculatePlayerStats(character);
        }

        // v0.7.1: Grant 3 starting abilities automatically from archetype
        // 1. Strike - Standard weapon attack (10 Stamina)
        // 2. Defensive Stance - Defensive stance (+3 Soak, -25% damage)
        // 3. Warrior's Vigor - Passive +10% Max HP
        character.Abilities = warriorArchetype.GetStartingAbilities();

        // v0.7.1: Initialize stance system
        character.ActiveStance = Stance.CreateBalancedStance();

        // Recalculate stats to apply Warrior's Vigor bonus
        var equipService = new EquipmentService();
        equipService.RecalculatePlayerStats(character);
    }

    private static void InitializeScavenger(PlayerCharacter character)
    {
        // Stats
        character.Attributes = new Attributes(
            might: 3,
            finesse: 3,
            wits: 3,
            will: 2,
            sturdiness: 3
        );

        // Resources (base values before equipment)
        character.MaxHP = 40;
        character.HP = 40;
        character.MaxStamina = 40;
        character.Stamina = 40;
        character.AP = 10;

        // Legacy weapon (v0.1/v0.2 compatibility)
        character.WeaponName = "Makeshift Spear";
        character.WeaponAttribute = "finesse";
        character.BaseDamage = 1;

        // Starting equipment (v0.3)
        var startingWeapon = EquipmentDatabase.GetByName("Makeshift Spear");
        var startingArmor = EquipmentDatabase.GetByName("Tattered Leathers");

        if (startingWeapon != null)
        {
            character.EquippedWeapon = startingWeapon;
        }

        if (startingArmor != null)
        {
            var equipmentService = new EquipmentService();
            character.EquippedArmor = startingArmor;
            equipmentService.RecalculatePlayerStats(character);
        }

        // Abilities (4 total: 2 starting, unlock 3rd at Level 3, 4th at Level 5)
        character.Abilities = new List<Ability>
        {
            // Level 1 - Starting ability
            new Ability
            {
                Name = "Exploit Weakness",
                Description = "Analyze your enemy's defenses, granting +2 bonus dice to your next attack",
                StaminaCost = 5,
                Type = AbilityType.Utility,
                AttributeUsed = "wits",
                BonusDice = 0,
                SuccessThreshold = 2,
                NextAttackBonusDice = 2
            },
            // Level 1 - Starting ability
            new Ability
            {
                Name = "Quick Dodge",
                Description = "Use your agility to completely avoid the next incoming attack",
                StaminaCost = 10,
                Type = AbilityType.Defense,
                AttributeUsed = "finesse",
                BonusDice = 1,
                SuccessThreshold = 2,
                NegateNextAttack = true
            },
            // Level 3 - Unlocked ability
            new Ability
            {
                Name = "Precision Strike",
                Description = "A precise attack using FINESSE and WITS that causes bleeding (1d6 damage for 2 turns)",
                StaminaCost = 8,
                Type = AbilityType.Attack,
                AttributeUsed = "finesse",
                BonusDice = 0,
                SuccessThreshold = 3,
                DamageDice = 1
            },
            // Level 5 - Unlocked ability
            new Ability
            {
                Name = "Survivalist",
                Description = "Use survival skills to restore 2d6 HP during combat (costs your turn)",
                StaminaCost = 20,
                Type = AbilityType.Utility,
                AttributeUsed = "sturdiness",
                BonusDice = 0,
                SuccessThreshold = 2
            }
        };
    }

    private static void InitializeMystic(PlayerCharacter character)
    {
        // v0.19.8: Create formal archetype instance
        var mysticArchetype = new MysticArchetype();
        character.Archetype = mysticArchetype;

        // Stats from archetype (WILL primary, WITS secondary)
        character.Attributes = mysticArchetype.GetBaseAttributes();

        // Resources (base values before equipment)
        character.MaxHP = 30; // Glass cannon - lowest HP
        character.HP = 30;
        character.MaxStamina = 30; // Lower stamina than Warriors/Adepts (AP is primary)
        character.Stamina = 30;

        // v0.19.8: Aether Pool - primary resource for Mystics
        // Base MaxAP = (WILL × 10) + 50
        // With WILL 4: (4 × 10) + 50 = 90 AP
        int baseMaxAP = (character.Attributes.Will * 10) + 50;
        character.MaxAP = baseMaxAP;
        character.AP = character.MaxAP; // Start at full AP

        // Legacy weapon (v0.1/v0.2 compatibility)
        character.WeaponName = "Crude Staff";
        character.WeaponAttribute = "will";
        character.BaseDamage = 1;

        // Starting equipment (v0.3)
        var startingWeapon = EquipmentDatabase.GetByName("Crude Staff");
        var startingArmor = EquipmentDatabase.GetByName("Tattered Leathers");

        if (startingWeapon != null)
        {
            character.EquippedWeapon = startingWeapon;
        }

        if (startingArmor != null)
        {
            character.EquippedArmor = startingArmor;
        }

        // v0.19.8: Grant 3 starting abilities automatically from archetype
        // 1. Aether Dart - Basic magical attack (15 AP, Arcane damage)
        // 2. Focus Aether - AP regeneration (ends turn, restore 25 AP)
        // 3. Aetheric Attunement - Passive +10% Max AP
        character.Abilities = mysticArchetype.GetStartingAbilities();

        // Recalculate stats to apply Aetheric Attunement bonus (+10% Max AP)
        var equipService = new EquipmentService();
        equipService.RecalculatePlayerStats(character);
    }

    private static void InitializeAdept(PlayerCharacter character)
    {
        // Stats - WITS primary, balanced secondary
        character.Attributes = new Attributes(
            might: 2,
            finesse: 3,
            wits: 4,      // Primary: Analysis, knowledge, skills
            will: 3,      // Secondary: Mental resilience
            sturdiness: 2
        );

        // Resources (skill-based specialist)
        character.MaxHP = 35;     // Lower than Warrior, higher than Mystic
        character.HP = 35;
        character.MaxStamina = 40; // Moderate stamina pool
        character.Stamina = 40;
        character.AP = 10;

        // Legacy weapon (v0.1/v0.2 compatibility)
        character.WeaponName = "Simple Staff";
        character.WeaponAttribute = "wits";
        character.BaseDamage = 1;

        // Starting equipment (v0.3)
        // Note: Will need to create "Scholar's Robes" and "Simple Staff" in EquipmentDatabase
        var startingWeapon = EquipmentDatabase.GetByName("Crude Staff"); // Temporary - using Mystic weapon
        var startingArmor = EquipmentDatabase.GetByName("Tattered Leathers"); // Light armor

        if (startingWeapon != null)
        {
            character.EquippedWeapon = startingWeapon;
        }

        if (startingArmor != null)
        {
            var equipmentService = new EquipmentService();
            character.EquippedArmor = startingArmor;
            equipmentService.RecalculatePlayerStats(character);
        }

        // v0.7: Adept Starting Abilities (3 free abilities)
        character.Abilities = new List<Ability>
        {
            // Starting Ability 1: Improvised Strike
            new Ability
            {
                Name = "Improvised Strike",
                Description = "Basic self-defense attack. Weak melee strike - combat is not your strength.",
                StaminaCost = 10,
                Type = AbilityType.Attack,
                AttributeUsed = "wits",  // Using analysis, not raw power
                BonusDice = 0,           // No bonus dice - intentionally weak
                SuccessThreshold = 2,
                DamageDice = 1           // Only 1d6 damage (vs Warrior's higher damage)
            },

            // Starting Ability 2: Analyze
            new Ability
            {
                Name = "Analyze",
                Description = "Study enemy to reveal one random piece of information (Resistance, Vulnerability, or HP).",
                StaminaCost = 20,
                Type = AbilityType.Utility,
                AttributeUsed = "wits",
                BonusDice = 2,           // Good chance to succeed
                SuccessThreshold = 2,
                // Special: Reveals enemy info (handled in CombatEngine)
                CurrentRank = 1,
                MaxRank = 1              // Cannot rank up - basic ability
            },

            // Starting Ability 3: Keen Eye (Passive)
            new Ability
            {
                Name = "Keen Eye",
                Description = "[PASSIVE] Gain +1 bonus rank in one skill: System Bypass, Wasteland Survival, or Rhetoric. (Choose during character creation)",
                StaminaCost = 0,         // Passive ability
                Type = AbilityType.Utility,
                AttributeUsed = "wits",
                BonusDice = 0,
                SuccessThreshold = 0,
                // Special: Skill bonus (will need skill system implementation)
                CurrentRank = 1,
                MaxRank = 1
            }
        };
    }

    private static void InitializeSkirmisher(PlayerCharacter character)
    {
        // v0.19.9: Create formal archetype instance
        var skirmisherArchetype = new SkirmisherArchetype();
        character.Archetype = skirmisherArchetype;

        // Stats from archetype (FINESSE primary, WITS secondary)
        character.Attributes = skirmisherArchetype.GetBaseAttributes();

        // Resources (base values before equipment)
        character.MaxHP = 40; // Moderate HP (higher than Mystic, lower than Warrior)
        character.HP = 40;
        character.MaxStamina = 35; // Moderate stamina pool
        character.Stamina = 35;
        character.AP = 10;

        // Legacy weapon (v0.1/v0.2 compatibility)
        character.WeaponName = "Makeshift Spear";
        character.WeaponAttribute = "finesse";
        character.BaseDamage = 1;

        // Starting equipment (v0.3)
        var startingWeapon = EquipmentDatabase.GetByName("Makeshift Spear");
        var startingArmor = EquipmentDatabase.GetByName("Tattered Leathers");

        if (startingWeapon != null)
        {
            character.EquippedWeapon = startingWeapon;
        }

        if (startingArmor != null)
        {
            character.EquippedArmor = startingArmor;
        }

        // v0.19.9: Grant 3 starting abilities automatically from archetype
        // 1. Quick Strike - FINESSE-based attack (15 Stamina)
        // 2. Evasive Stance - Defensive stance (+3 Defense, -50% damage)
        // 3. Fleet Footed - Passive +2 Vigilance
        character.Abilities = skirmisherArchetype.GetStartingAbilities();

        // v0.19.9: Initialize stance system for Skirmisher
        character.ActiveStance = Stance.CreateBalancedStance();

        // Recalculate stats to apply Fleet Footed bonus (if needed)
        var equipService = new EquipmentService();
        equipService.RecalculatePlayerStats(character);
    }

    public static string GetClassDescription(CharacterClass characterClass)
    {
        return characterClass switch
        {
            CharacterClass.Warrior =>
                "A hardy fighter who excels in close combat. High HP and defensive capabilities make the Warrior ideal for surviving prolonged battles.\n" +
                "Starting Stats: MIGHT 4, FINESSE 2, WITS 2, WILL 2, STURDINESS 4\n" +
                "HP: 50 | Stamina: 30\n" +
                "Weapon: Scavenged Axe (MIGHT-based)\n" +
                "Abilities: Power Strike, Shield Wall",

            CharacterClass.Scavenger =>
                "A balanced survivor with tactical options. The Scavenger uses cunning and agility to exploit enemy weaknesses.\n" +
                "Starting Stats: MIGHT 3, FINESSE 3, WITS 3, WILL 2, STURDINESS 3\n" +
                "HP: 40 | Stamina: 40\n" +
                "Weapon: Makeshift Spear (FINESSE-based)\n" +
                "Abilities: Exploit Weakness, Quick Dodge",

            CharacterClass.Mystic =>
                "A wielder of corrupted aetheric energy. Low HP but powerful abilities make the Mystic a high-risk, high-reward choice.\n" +
                "Starting Stats: MIGHT 2, FINESSE 2, WITS 3, WILL 4, STURDINESS 2\n" +
                "HP: 30 | Stamina: 50\n" +
                "Weapon: Improvised Staff (WILL-based)\n" +
                "Abilities: Aetheric Bolt, Disrupt",

            CharacterClass.Adept =>
                "[v0.7] A skill-based specialist who relies on knowledge and preparation. Weak in direct combat but excels in support, analysis, and utility.\n" +
                "Starting Stats: MIGHT 2, FINESSE 3, WITS 4, WILL 3, STURDINESS 2\n" +
                "HP: 35 | Stamina: 40\n" +
                "Weapon: Simple Staff (WITS-based)\n" +
                "Abilities: Improvised Strike, Analyze, Keen Eye\n" +
                "Specializations (unlock with 3 PP): Bone-Setter, Jötun-Reader, Skald",

            CharacterClass.Skirmisher =>
                "[v0.19.9] An agility-based combatant who excels at evasion, precision, and speed. High Defense but lower endurance. Acts first in combat.\n" +
                "Starting Stats: MIGHT 2, FINESSE 4, WITS 3, WILL 2, STURDINESS 3\n" +
                "HP: 40 | Stamina: 35\n" +
                "Weapon: Makeshift Spear (FINESSE-based)\n" +
                "Abilities: Quick Strike, Evasive Stance, Fleet Footed\n" +
                "Specializations (coming soon): Myrk-gengr, Veiðimaðr, Hlekkr-master",

            _ => "Unknown class"
        };
    }

    /// <summary>
    /// [v0.5] Add heretical abilities - forbidden powers that cost Corruption/Stress
    /// Available to all classes from the start
    /// </summary>
    private static void AddHereticalAbilities(PlayerCharacter character)
    {
        // Void Strike - Corruption-based devastation
        character.Abilities.Add(new Ability
        {
            Name = "Void Strike",
            Description = "Channel corrupted energy for devastating damage. Ignores armor but costs permanent Corruption.",
            StaminaCost = 20,
            Type = AbilityType.Attack,
            AttributeUsed = "might",
            BonusDice = 4,
            SuccessThreshold = 3,
            DamageDice = 3, // 3d8 damage
            IgnoresArmor = true,
            // Trauma costs handled in CombatEngine: 3 Corruption
            CurrentRank = 1,
            MaxRank = 1 // Cannot be ranked up - heretical abilities are fixed
        });

        // Psychic Lash - Stress-based mental assault
        character.Abilities.Add(new Ability
        {
            Name = "Psychic Lash",
            Description = "Mental assault that bypasses armor. Uses WILL. Costs recoverable Psychic Stress.",
            StaminaCost = 25,
            Type = AbilityType.Attack,
            AttributeUsed = "will",
            BonusDice = 3,
            SuccessThreshold = 2,
            DamageDice = 2, // 2d6 psychic damage
            IgnoresArmor = true,
            // Trauma costs handled in CombatEngine: 10 Psychic Stress
            CurrentRank = 1,
            MaxRank = 1
        });

        // Desperate Gambit - Ultimate power at ultimate cost
        character.Abilities.Add(new Ability
        {
            Name = "Desperate Gambit",
            Description = "⚠️ EMERGENCY ONLY: Devastating AOE attack. Costs both Stress AND Corruption.",
            StaminaCost = 40,
            Type = AbilityType.Attack,
            AttributeUsed = "might", // Can use MIGHT or WILL (handled in combat)
            BonusDice = 5,
            SuccessThreshold = 3,
            DamageDice = 4, // 4d10 damage AOE
            IgnoresArmor = true,
            // Trauma costs handled in CombatEngine: 15 Stress + 5 Corruption
            // AOE: Hits all enemies (handled in CombatEngine)
            CurrentRank = 1,
            MaxRank = 1
        });

        // [v0.6] NEW HERETICAL ABILITIES (THE LOWER DEPTHS)

        // Blight Surge - Corruption debuff attack
        character.Abilities.Add(new Ability
        {
            Name = "Blight Surge",
            Description = "Channel raw Blight energy for burst damage. Target gains [Corrupted] status (+20% Stress).",
            StaminaCost = 30,
            Type = AbilityType.Attack,
            AttributeUsed = "will",
            BonusDice = 4,
            SuccessThreshold = 3,
            DamageDice = 3, // 3d6 damage
            IgnoresArmor = true,
            // Trauma costs handled in CombatEngine: 8 Stress + 2 Corruption
            CurrentRank = 1,
            MaxRank = 1
        });

        // Blood Sacrifice - HP cost for overwhelming power
        character.Abilities.Add(new Ability
        {
            Name = "Blood Sacrifice",
            Description = "⚠️ Trade vitality for power. Costs HP instead of Stamina. Cannot use if HP < 30.",
            StaminaCost = 0, // No stamina cost - uses HP instead
            Type = AbilityType.Attack,
            AttributeUsed = "might",
            BonusDice = 6,
            SuccessThreshold = 3,
            DamageDice = 4, // 4d8 damage
            IgnoresArmor = true,
            // Trauma costs handled in CombatEngine: 20 HP + 3 Corruption
            CurrentRank = 1,
            MaxRank = 1
        });

        // Mass Psychic Lash - Stress AOE
        character.Abilities.Add(new Ability
        {
            Name = "Mass Psychic Lash",
            Description = "Project trauma outward to all enemies. High Stress cost limits uses.",
            StaminaCost = 40,
            Type = AbilityType.Attack,
            AttributeUsed = "will",
            BonusDice = 3,
            SuccessThreshold = 2,
            DamageDice = 2, // 2d6 psychic damage
            IgnoresArmor = true,
            // Trauma costs handled in CombatEngine: 20 Stress
            // AOE: Hits all enemies
            CurrentRank = 1,
            MaxRank = 1
        });

        // Corruption Nova - Scales with your Corruption
        character.Abilities.Add(new Ability
        {
            Name = "Corruption Nova",
            Description = "Release accumulated Corruption as destructive wave. Damage scales with your Corruption (+1d6 per 20 Corruption).",
            StaminaCost = 50,
            Type = AbilityType.Attack,
            AttributeUsed = "will",
            BonusDice = 0, // No dice roll - automatic hit
            SuccessThreshold = 0, // Automatic hit
            DamageDice = 5, // 5d6 base damage
            IgnoresArmor = true,
            // Trauma costs handled in CombatEngine: 10 Corruption
            // AOE: Hits all enemies
            // Scaling: +1d6 per 20 Corruption
            CurrentRank = 1,
            MaxRank = 1
        });

        // Siphon Sanity - Stress drain
        character.Abilities.Add(new Ability
        {
            Name = "Siphon Sanity",
            Description = "Drain enemy's mental coherence to restore your own. Recover Stress equal to damage dealt.",
            StaminaCost = 25,
            Type = AbilityType.Attack,
            AttributeUsed = "will",
            BonusDice = 3,
            SuccessThreshold = 2,
            DamageDice = 2, // 2d6 psychic damage
            IgnoresArmor = true,
            // Special: Recover Stress equal to damage dealt (handled in CombatEngine)
            // Only works on living enemies (not constructs)
            CurrentRank = 1,
            MaxRank = 1
        });

        // Glitch Reality - Random chaos effect
        character.Abilities.Add(new Ability
        {
            Name = "Glitch Reality",
            Description = "⚠️ Warp local reality with unpredictable effects. Roll 1d6 for random outcome.",
            StaminaCost = 35,
            Type = AbilityType.Utility, // Mixed effects
            AttributeUsed = "will",
            BonusDice = 0, // No attack roll - random effect
            SuccessThreshold = 0,
            DamageDice = 0, // Variable damage based on effect
            IgnoresArmor = true,
            // Trauma costs handled in CombatEngine: 5 Stress + 4 Corruption
            // Effects: 1=AOE damage, 2=Heal, 3=Debuff, 4=Teleport, 5=Reality Tear, 6=Extra Turn
            CurrentRank = 1,
            MaxRank = 1
        });
    }
}
