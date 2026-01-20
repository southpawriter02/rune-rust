namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Combat stance options that modify a combatant's stats and behavior during combat.
/// </summary>
/// <remarks>
/// <para>Combat stances provide strategic options for combatants to adjust their
/// offensive and defensive capabilities based on the situation.</para>
/// <para>Key characteristics:</para>
/// <list type="bullet">
///   <item><description>Stances can be changed once per round as a free action</description></item>
///   <item><description>Stance effects persist between turns until changed</description></item>
///   <item><description>Each stance applies unique stat modifiers</description></item>
/// </list>
/// </remarks>
public enum CombatStance
{
    /// <summary>
    /// Default combat stance with no stat modifiers.
    /// </summary>
    /// <remarks>
    /// <para>A well-rounded approach suitable for most combat situations.</para>
    /// <para>Modifiers: None</para>
    /// </remarks>
    Balanced = 0,

    /// <summary>
    /// Offensive stance that prioritizes damage output at the cost of defense.
    /// </summary>
    /// <remarks>
    /// <para>Best used when attempting to quickly defeat an enemy or when defense is less critical.</para>
    /// <para>Modifiers:</para>
    /// <list type="bullet">
    ///   <item><description>Attack: +2</description></item>
    ///   <item><description>Damage: +1d4</description></item>
    ///   <item><description>Defense: -2</description></item>
    ///   <item><description>All Saves: -1</description></item>
    /// </list>
    /// </remarks>
    Aggressive = 1,

    /// <summary>
    /// Defensive stance that prioritizes survival at the cost of offense.
    /// </summary>
    /// <remarks>
    /// <para>Best used when facing dangerous enemies or when trying to survive until help arrives.</para>
    /// <para>Modifiers:</para>
    /// <list type="bullet">
    ///   <item><description>Attack: -2</description></item>
    ///   <item><description>Damage: -1d4</description></item>
    ///   <item><description>Defense: +2</description></item>
    ///   <item><description>All Saves: +2</description></item>
    /// </list>
    /// </remarks>
    Defensive = 2
}
