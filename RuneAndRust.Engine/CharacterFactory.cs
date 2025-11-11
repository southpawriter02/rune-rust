using RuneAndRust.Core;

namespace RuneAndRust.Engine;

public class CharacterFactory
{
    public static PlayerCharacter CreateCharacter(CharacterClass characterClass, string name = "Survivor")
    {
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
        }

        // [v0.5] Add heretical abilities (available to all classes)
        AddHereticalAbilities(character);

        return character;
    }

    private static void InitializeWarrior(PlayerCharacter character)
    {
        // Stats
        character.Attributes = new Attributes(
            might: 4,
            finesse: 2,
            wits: 2,
            will: 2,
            sturdiness: 4
        );

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

        // Abilities (4 total: 2 starting, unlock 3rd at Level 3, 4th at Level 5)
        character.Abilities = new List<Ability>
        {
            // Level 1 - Starting ability
            new Ability
            {
                Name = "Power Strike",
                Description = "A devastating melee attack that deals double damage on success",
                StaminaCost = 5,
                Type = AbilityType.Attack,
                AttributeUsed = "might",
                BonusDice = 2,
                SuccessThreshold = 3,
                DamageDice = 0 // Uses weapon damage, doubled
            },
            // Level 1 - Starting ability
            new Ability
            {
                Name = "Shield Wall",
                Description = "Raise your defenses, reducing incoming damage by 50% for 2 turns",
                StaminaCost = 10,
                Type = AbilityType.Defense,
                AttributeUsed = "sturdiness",
                BonusDice = 1,
                SuccessThreshold = 2,
                DefensePercent = 50,
                DefenseDuration = 2
            },
            // Level 3 - Unlocked ability
            new Ability
            {
                Name = "Cleaving Strike",
                Description = "Powerful strike that hits target and deals 50% damage to another enemy if you get 3+ successes",
                StaminaCost = 8,
                Type = AbilityType.Attack,
                AttributeUsed = "might",
                BonusDice = 1,
                SuccessThreshold = 2,
                DamageDice = 1
            },
            // Level 5 - Unlocked ability
            new Ability
            {
                Name = "Battle Rage",
                Description = "Enter a berserker state gaining +2 dice on all attacks for 3 turns, but take 25% more damage",
                StaminaCost = 15,
                Type = AbilityType.Utility,
                AttributeUsed = "will",
                BonusDice = 0,
                SuccessThreshold = 2
            }
        };
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
        // Stats
        character.Attributes = new Attributes(
            might: 2,
            finesse: 2,
            wits: 3,
            will: 4,
            sturdiness: 2
        );

        // Resources (base values before equipment)
        character.MaxHP = 30;
        character.HP = 30;
        character.MaxStamina = 50;
        character.Stamina = 50;
        character.AP = 10;

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
                Name = "Aetheric Bolt",
                Description = "Unleash a bolt of corrupted energy that ignores armor",
                StaminaCost = 8,
                Type = AbilityType.Attack,
                AttributeUsed = "will",
                BonusDice = 1,
                SuccessThreshold = 2,
                DamageDice = 2,
                IgnoresArmor = true
            },
            // Level 1 - Starting ability
            new Ability
            {
                Name = "Disrupt",
                Description = "Overwhelm your enemy's systems, causing them to skip their next turn",
                StaminaCost = 12,
                Type = AbilityType.Control,
                AttributeUsed = "will",
                BonusDice = 0,
                SuccessThreshold = 2,
                SkipEnemyTurn = true
            },
            // Level 3 - Unlocked ability
            new Ability
            {
                Name = "Aetheric Shield",
                Description = "Create a protective shield that absorbs the next 15 damage",
                StaminaCost = 10,
                Type = AbilityType.Defense,
                AttributeUsed = "will",
                BonusDice = 0,
                SuccessThreshold = 2
            },
            // Level 5 - Unlocked ability
            new Ability
            {
                Name = "Chain Lightning",
                Description = "Unleash lightning that damages all enemies (2d6 with 4+ successes, 1d6 with 3+)",
                StaminaCost = 15,
                Type = AbilityType.Attack,
                AttributeUsed = "will",
                BonusDice = 2,
                SuccessThreshold = 3,
                DamageDice = 1
            }
        };
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
    }
}
