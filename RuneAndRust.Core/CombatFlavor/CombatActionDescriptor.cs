namespace RuneAndRust.Core.CombatFlavor;

/// <summary>
/// v0.38.6: Represents a single combat action flavor text descriptor
/// Used for player attacks, defenses, and enemy actions
/// </summary>
public class CombatActionDescriptor
{
    public int DescriptorId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? WeaponType { get; set; }
    public string? EnemyArchetype { get; set; }
    public string? OutcomeType { get; set; }
    public string DescriptorText { get; set; } = string.Empty;
    public string? Tags { get; set; }
}

/// <summary>
/// Combat action categories
/// </summary>
public enum CombatActionCategory
{
    PlayerMeleeAttack,
    PlayerRangedAttack,
    PlayerDefense,
    PlayerMovement,
    EnemyAttack,
    EnemyDefense,
    EnemyMovement,
    EnvironmentalReaction
}

/// <summary>
/// Combat outcome types for flavor text variation
/// </summary>
public enum CombatOutcome
{
    Miss,
    Deflected,
    GlancingHit,
    SolidHit,
    DevastatingHit,
    CriticalHit,
    Fumble
}

/// <summary>
/// Weapon types for action descriptor filtering
/// </summary>
public enum WeaponType
{
    SwordOneHanded,
    SwordTwoHanded,
    AxeOneHanded,
    AxeTwoHanded,
    HammerOneHanded,
    HammerTwoHanded,
    Bow,
    Crossbow,
    Unarmed,
    Dodge,
    Parry,
    Block,
    Shield
}
