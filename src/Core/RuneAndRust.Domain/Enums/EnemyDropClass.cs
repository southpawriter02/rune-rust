namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Classifies enemies by their loot drop potential.
/// </summary>
/// <remarks>
/// <para>
/// Enemy drop classes determine the probability distribution of quality
/// tiers when an enemy is defeated. Higher classes have better chances
/// of dropping higher-tier equipment.
/// </para>
/// <para>
/// The classification loosely maps to enemy threat levels:
/// - Trash: Weak fodder enemies (Servitors)
/// - Standard: Regular combat encounters (Drones)
/// - Elite: Challenging enemies (Wardens)
/// - MiniBoss: Mid-dungeon bosses
/// - Boss: Major bosses with guaranteed high-tier drops
/// </para>
/// </remarks>
public enum EnemyDropClass
{
    /// <summary>
    /// Weak fodder enemies with poor drop rates.
    /// </summary>
    /// <remarks>
    /// Trash enemies (e.g., Servitors) primarily drop Tier 0-1 items
    /// and have a 10% chance of dropping nothing.
    /// </remarks>
    Trash = 0,

    /// <summary>
    /// Standard combat enemies with decent drop rates.
    /// </summary>
    /// <remarks>
    /// Standard enemies (e.g., Drones) are guaranteed to drop something
    /// and can drop up to Tier 3 items.
    /// </remarks>
    Standard = 1,

    /// <summary>
    /// Challenging elite enemies with good drop rates.
    /// </summary>
    /// <remarks>
    /// Elite enemies (e.g., Wardens) are guaranteed drops with a small
    /// chance of Tier 4 items.
    /// </remarks>
    Elite = 2,

    /// <summary>
    /// Mid-dungeon bosses with excellent drop rates.
    /// </summary>
    /// <remarks>
    /// MiniBosses are guaranteed to drop Tier 2+ items with a 30%
    /// chance of Tier 4 (Myth-Forged) equipment.
    /// </remarks>
    MiniBoss = 3,

    /// <summary>
    /// Major bosses with the best drop rates.
    /// </summary>
    /// <remarks>
    /// Bosses always drop high-tier items: 70% Tier 4 (Myth-Forged),
    /// 30% Tier 3 (Optimized). Never drops nothing.
    /// </remarks>
    Boss = 4
}
