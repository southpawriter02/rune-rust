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

        // Resources
        character.MaxHP = 50;
        character.HP = 50;
        character.MaxStamina = 30;
        character.Stamina = 30;
        character.AP = 10;

        // Weapon
        character.WeaponName = "Scavenged Axe";
        character.WeaponAttribute = "might";
        character.BaseDamage = 1;

        // Abilities
        character.Abilities = new List<Ability>
        {
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

        // Resources
        character.MaxHP = 40;
        character.HP = 40;
        character.MaxStamina = 40;
        character.Stamina = 40;
        character.AP = 10;

        // Weapon
        character.WeaponName = "Makeshift Spear";
        character.WeaponAttribute = "finesse";
        character.BaseDamage = 1;

        // Abilities
        character.Abilities = new List<Ability>
        {
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

        // Resources
        character.MaxHP = 30;
        character.HP = 30;
        character.MaxStamina = 50;
        character.Stamina = 50;
        character.AP = 10;

        // Weapon
        character.WeaponName = "Improvised Staff";
        character.WeaponAttribute = "will";
        character.BaseDamage = 1;

        // Abilities
        character.Abilities = new List<Ability>
        {
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
}
