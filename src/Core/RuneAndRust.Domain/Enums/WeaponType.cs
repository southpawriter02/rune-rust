namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the type of weapon, affecting combat style and bonuses.
/// </summary>
/// <remarks>
/// Each weapon type has distinct characteristics:
/// <list type="bullet">
///   <item><description>Swords: Balanced, reliable damage</description></item>
///   <item><description>Axes: High damage, accuracy penalty</description></item>
///   <item><description>Daggers: Low damage, Finesse bonus</description></item>
///   <item><description>Staffs: Moderate damage, Will bonus</description></item>
/// </list>
/// </remarks>
public enum WeaponType
{
    /// <summary>
    /// Balanced melee weapons. Standard damage, no special modifiers.
    /// </summary>
    Sword,

    /// <summary>
    /// Heavy chopping weapons. Higher damage dice, attack roll penalty.
    /// </summary>
    Axe,

    /// <summary>
    /// Quick, precise weapons. Lower damage dice, Finesse bonus.
    /// </summary>
    Dagger,

    /// <summary>
    /// Magic-focused weapons. Moderate damage, Will bonus.
    /// </summary>
    Staff
}
