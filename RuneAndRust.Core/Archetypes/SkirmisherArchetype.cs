namespace RuneAndRust.Core.Archetypes;

/// <summary>
/// v0.19.9: Skirmisher Archetype
/// Agility-based combatants who excel at evasion, precision, and speed.
/// Starting Abilities: Quick Strike, Evasive Stance, Fleet Footed
/// </summary>
public class SkirmisherArchetype : Archetype
{
    public override CharacterClass ArchetypeType => CharacterClass.Skirmisher;
    public override ResourceType PrimaryResource => ResourceType.Stamina;

    /// <summary>
    /// Skirmishers automatically receive 3 starting abilities:
    /// 1. Quick Strike - FINESSE-based attack (15 Stamina)
    /// 2. Evasive Stance - Defensive stance (+3 Defense, -50% damage)
    /// 3. Fleet Footed - Passive +2 Vigilance
    /// </summary>
    public override List<Ability> GetStartingAbilities()
    {
        return new List<Ability>
        {
            CreateQuickStrike(),
            CreateEvasiveStance(),
            CreateFleetFooted()
        };
    }

    /// <summary>
    /// Skirmishers are proficient with simple weapons, martial one-handed melee, and all ranged weapons
    /// Heavy two-handed weapons create "system lag" that conflicts with their evasive style
    /// </summary>
    public override List<string> GetWeaponProficiencies()
    {
        return new List<string>
        {
            "All Simple Weapons",
            "Martial One-Handed Melee Weapons",
            "All Ranged Weapons"
        };
    }

    /// <summary>
    /// Skirmishers are proficient with light and medium armor only
    /// Heavy armor creates "system lag" that interferes with mobility
    /// </summary>
    public override List<string> GetArmorProficiencies()
    {
        return new List<string>
        {
            "Light Armor",
            "Medium Armor"
        };
    }

    /// <summary>
    /// Skirmisher base attributes (FINESSE primary, WITS secondary)
    /// </summary>
    public override Attributes GetBaseAttributes()
    {
        return new Attributes
        {
            Might = 2,       // Tertiary: Physical power (low)
            Finesse = 4,     // Primary: Agility, accuracy, evasion
            Wits = 3,        // Secondary: Perception, initiative
            Will = 2,        // Tertiary: Mental fortitude (low)
            Sturdiness = 3   // Secondary: Durability (moderate)
        };
    }

    #region Starting Abilities

    /// <summary>
    /// Quick Strike: A swift, precise attack that uses FINESSE for both accuracy and damage.
    /// The core combat ability of the Skirmisher Archetype.
    /// Special: Overrides weapon's default damage attribute with FINESSE.
    /// </summary>
    private Ability CreateQuickStrike()
    {
        return new Ability
        {
            Name = "Quick Strike",
            Description = "A swift, precise attack using FINESSE for accuracy and damage. Works with any proficient weapon, allowing you to use agility instead of strength.",
            StaminaCost = 15,
            Type = AbilityType.Attack,
            AttributeUsed = "finesse", // Always uses FINESSE
            BonusDice = 0,
            SuccessThreshold = 0, // Always hits, damage based on successes
            DamageDice = 0, // Damage calculated from weapon + successes
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 0
        };
    }

    /// <summary>
    /// Evasive Stance: The Skirmisher dedicates their turn to evasion and defense.
    /// Grants significant defense boost at the cost of offensive power.
    /// </summary>
    private Ability CreateEvasiveStance()
    {
        return new Ability
        {
            Name = "Evasive Stance",
            Description = "Enter an evasive stance. While active: +3 Defense (harder to hit), -50% damage dealt. Focus on survival over offense. Exit as a Free Action.",
            StaminaCost = 0, // Stances don't cost stamina
            Type = AbilityType.Defense,
            AttributeUsed = "", // No roll required
            BonusDice = 0,
            SuccessThreshold = 0, // Auto-success
            DefensePercent = 0, // Custom stance mechanics handled separately
            MaxRank = 1, // Stances don't rank up
            CostToRank2 = 0,
            CostToRank3 = 0
        };
    }

    /// <summary>
    /// Fleet Footed: A Skirmisher's hyper-efficient reflexes grant them superior initiative.
    /// They react to threats before others recognize danger.
    /// </summary>
    private Ability CreateFleetFooted()
    {
        return new Ability
        {
            Name = "Fleet Footed",
            Description = "Passive: +2 Vigilance (initiative bonus). Your lightning reflexes ensure you act first in combat, giving you the tactical advantage.",
            StaminaCost = 0, // Passive abilities are free
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
