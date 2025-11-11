namespace RuneAndRust.Core.Archetypes;

/// <summary>
/// v0.7.1: Warrior Archetype
/// Trained combatants who excel at direct combat and survival.
/// Starting Abilities: Strike, Defensive Stance, Warrior's Vigor
/// </summary>
public class WarriorArchetype : Archetype
{
    public override CharacterClass ArchetypeType => CharacterClass.Warrior;
    public override ResourceType PrimaryResource => ResourceType.Stamina;

    /// <summary>
    /// Warriors automatically receive 3 starting abilities:
    /// 1. Strike - Standard weapon attack (10 Stamina)
    /// 2. Defensive Stance - Defensive stance (+3 Soak, -25% damage)
    /// 3. Warrior's Vigor - Passive +10% Max HP
    /// </summary>
    public override List<Ability> GetStartingAbilities()
    {
        return new List<Ability>
        {
            CreateStrike(),
            CreateDefensiveStance(),
            CreateWarriorsVigor()
        };
    }

    /// <summary>
    /// Warriors are proficient with all weapons
    /// </summary>
    public override List<string> GetWeaponProficiencies()
    {
        return new List<string>
        {
            "All Simple Weapons",
            "All Martial Weapons",
            "All Ranged Weapons"
        };
    }

    /// <summary>
    /// Warriors are proficient with all armor and shields
    /// </summary>
    public override List<string> GetArmorProficiencies()
    {
        return new List<string>
        {
            "Light Armor",
            "Medium Armor",
            "Heavy Armor",
            "Shields"
        };
    }

    /// <summary>
    /// Warrior base attributes (balanced physical combatant)
    /// </summary>
    public override Attributes GetBaseAttributes()
    {
        return new Attributes
        {
            Might = 4,       // Primary: Physical power
            Finesse = 2,     // Secondary: Combat agility
            Wits = 2,        // Tertiary: Tactical awareness
            Will = 2,        // Tertiary: Mental fortitude
            Sturdiness = 4   // Primary: Durability
        };
    }

    #region Starting Abilities

    /// <summary>
    /// Strike: A standard, reliable attack made with a proficient weapon.
    /// The foundational combat art of the Warrior Archetype.
    /// </summary>
    private Ability CreateStrike()
    {
        return new Ability
        {
            Name = "Strike",
            Description = "A standard, reliable attack made with a proficient weapon. Uses MIGHT for melee or FINESSE for ranged weapons.",
            StaminaCost = 10,
            Type = AbilityType.Attack,
            AttributeUsed = "might", // Can be overridden to "finesse" for ranged weapons
            BonusDice = 0,
            SuccessThreshold = 0, // Always hits, damage based on successes
            DamageDice = 0, // Damage calculated from weapon + successes
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 0
        };
    }

    /// <summary>
    /// Defensive Stance: The Warrior focuses on survival, raising their guard
    /// and preparing to weather incoming assault. A core defensive stance.
    /// </summary>
    private Ability CreateDefensiveStance()
    {
        return new Ability
        {
            Name = "Defensive Stance",
            Description = "Enter a defensive stance. While active: +3 Soak (flat damage reduction), -25% damage dealt, +1d to defensive Resolve Checks. Exit as a Free Action.",
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
    /// Warrior's Vigor: A Warrior's training and physical conditioning grants
    /// them natural resilience beyond ordinary scavengers.
    /// </summary>
    private Ability CreateWarriorsVigor()
    {
        return new Ability
        {
            Name = "Warrior's Vigor",
            Description = "Passive: +10% Maximum HP. Your training grants natural resilience beyond ordinary scavengers.",
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
