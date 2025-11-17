namespace RuneAndRust.Core;

public enum AbilityType
{
    Attack,      // Direct damage ability
    Defense,     // Defensive ability
    Utility,     // Support/utility ability
    Control      // Disabling ability
}

public class Ability
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StaminaCost { get; set; }
    // v0.19.8: Aether Pool cost for Mystic abilities
    public int APCost { get; set; } = 0; // Aether Pool cost (Mystic abilities use this instead of Stamina)
    public AbilityType Type { get; set; }

    // Rank System (Aethelgard)
    public int CurrentRank { get; set; } = 1; // Always starts at Rank 1
    public int MaxRank { get; set; } = 3;
    public int CostToRank2 { get; set; } = 5; // PP cost to advance to Rank 2
    public int CostToRank3 { get; set; } = 0; // Locked until v0.5+ (Capstone)

    // For dice rolling
    public string AttributeUsed { get; set; } = string.Empty;
    public int BonusDice { get; set; } = 0;
    public int SuccessThreshold { get; set; } = 2; // Successes needed for ability to work

    // Effects
    public int DamageDice { get; set; } = 0; // For direct damage abilities
    public bool IgnoresArmor { get; set; } = false;
    public int DefensePercent { get; set; } = 0; // For defensive abilities
    public int DefenseDuration { get; set; } = 0; // Turns
    public bool SkipEnemyTurn { get; set; } = false; // For control abilities
    public int NextAttackBonusDice { get; set; } = 0; // For utility abilities
    public bool NegateNextAttack { get; set; } = false; // For dodge abilities

    // v0.38.7: Galdr & Ability Flavor Text Integration
    /// <summary>
    /// Rune school for Galdr (magical) abilities.
    /// Values: Fehu, Thurisaz, Ansuz, Raido, Hagalaz, Naudiz, Isa, Jera,
    ///         Tiwaz, Berkanan, Mannaz, Laguz, null (non-Galdr)
    /// </summary>
    public string? RuneSchool { get; set; } = null;

    /// <summary>
    /// Ability category for non-Galdr abilities (Warrior/Adept abilities).
    /// Values: WeaponArt, TacticalAbility, DefensiveAbility, PassiveAbility,
    ///         ResourceAbility, null (Galdr ability)
    /// </summary>
    public string? AbilityCategory { get; set; } = null;

    /// <summary>
    /// Element associated with this ability (for manifestation descriptors).
    /// Values: Fire, Ice, Lightning, Wind, Earth, Water, Healing, Shadow, Aether, null
    /// </summary>
    public string? Element { get; set; } = null;

    /// <summary>
    /// Whether this ability is a Galdr (magical) ability.
    /// True if RuneSchool is set, false otherwise.
    /// </summary>
    public bool IsGaldr => !string.IsNullOrEmpty(RuneSchool);
}
