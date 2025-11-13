namespace RuneAndRust.Core.Archetypes;

/// <summary>
/// v0.19.8: Mystic Archetype
/// Aether-wielding spellcasters who directly interface with reality's corrupted source code.
/// Starting Abilities: Aether Dart, Focus Aether, Aetheric Attunement
/// </summary>
public class MysticArchetype : Archetype
{
    public override CharacterClass ArchetypeType => CharacterClass.Mystic;
    public override ResourceType PrimaryResource => ResourceType.Aether;

    /// <summary>
    /// Mystics automatically receive 3 starting abilities:
    /// 1. Aether Dart - Basic magical attack (15 AP, Arcane damage)
    /// 2. Focus Aether - AP regeneration ability (ends turn, restore 25 AP)
    /// 3. Aetheric Attunement - Passive +10% Max AP
    /// </summary>
    public override List<Ability> GetStartingAbilities()
    {
        return new List<Ability>
        {
            CreateAetherDart(),
            CreateFocusAether(),
            CreateAethericAttunement()
        };
    }

    /// <summary>
    /// Mystics are proficient with simple one-handed weapons only
    /// Heavy gear acts as "grounding rod", interferes with Aether channeling
    /// </summary>
    public override List<string> GetWeaponProficiencies()
    {
        return new List<string>
        {
            "Daggers",
            "Staves",
            "Wands",
            "Hand Axes",
            "Simple One-Handed Weapons"
        };
    }

    /// <summary>
    /// Mystics are proficient with light armor only
    /// Heavy armor interferes with Aether channeling
    /// </summary>
    public override List<string> GetArmorProficiencies()
    {
        return new List<string>
        {
            "Light Armor"
        };
    }

    /// <summary>
    /// Mystic base attributes (WILL primary, WITS secondary)
    /// </summary>
    public override Attributes GetBaseAttributes()
    {
        return new Attributes
        {
            Might = 2,       // Tertiary: Physical power (low)
            Finesse = 2,     // Tertiary: Combat agility (low)
            Wits = 4,        // Secondary: Perception, analysis
            Will = 4,        // Primary: Mental fortitude, spell potency
            Sturdiness = 2   // Tertiary: Durability (glass cannon)
        };
    }

    #region Starting Abilities

    /// <summary>
    /// Aether Dart: Compressed packet of raw Aether, single-target attack.
    /// The foundational spellcasting ability of the Mystic Archetype.
    /// </summary>
    private Ability CreateAetherDart()
    {
        return new Ability
        {
            Name = "Aether Dart",
            Description = "Unleash a compressed packet of raw Aether at a single target. Deals 2d6 + WILL bonus Arcane damage. Your core magical attack.",
            StaminaCost = 0, // Mystics use AP, not Stamina
            APCost = 15,
            Type = AbilityType.Attack,
            AttributeUsed = "will",
            BonusDice = 0,
            SuccessThreshold = 0, // Always hits, damage based on successes
            DamageDice = 2, // 2d6 base damage
            IgnoresArmor = false, // Arcane damage respects armor
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 0
        };
    }

    /// <summary>
    /// Focus Aether: Draw ambient corrupted Aether to restore AP.
    /// Teaches resource regeneration and AP management.
    /// </summary>
    private Ability CreateFocusAether()
    {
        return new Ability
        {
            Name = "Focus Aether",
            Description = "Spend your entire turn channeling ambient Aether to restore 25 AP. Cannot take other actions this turn. Essential for sustained spellcasting.",
            StaminaCost = 0, // Special: Ends turn
            APCost = 0, // Free, but ends turn
            Type = AbilityType.Utility,
            AttributeUsed = "", // No roll required
            BonusDice = 0,
            SuccessThreshold = 0, // Auto-success
            MaxRank = 3, // Can rank up to restore more AP
            CostToRank2 = 5,
            CostToRank3 = 0
        };
    }

    /// <summary>
    /// Aetheric Attunement: A Mystic's deep connection to corrupted Aether
    /// grants them an expanded mental capacity for channeling.
    /// </summary>
    private Ability CreateAethericAttunement()
    {
        return new Ability
        {
            Name = "Aetheric Attunement",
            Description = "Passive: +10% Maximum Aether Pool. Your deep connection to corrupted Aether expands your mental capacity for channeling.",
            StaminaCost = 0, // Passive abilities are free
            APCost = 0,
            Type = AbilityType.Utility,
            AttributeUsed = "", // Passive, no roll
            BonusDice = 0,
            SuccessThreshold = 0,
            MaxRank = 1, // Passives don't rank up
            CostToRank2 = 0,
            CostToRank3 = 0
        };
    }

    #endregion
}
