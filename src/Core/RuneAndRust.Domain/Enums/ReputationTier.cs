namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents a player's standing with a faction.
/// </summary>
/// <remarks>
/// <para>Tiers map to reputation value ranges defined in the faction-reputation design doc (v1.2).</para>
/// <para>Each tier has gameplay consequences: price modifiers, quest gating, settlement access,
/// and NPC behavior changes.</para>
/// <para>
/// Tier boundaries:
/// <list type="table">
///   <listheader><term>Tier</term><description>Range</description></listheader>
///   <item><term>Hated</term><description>-100 to -76</description></item>
///   <item><term>Hostile</term><description>-75 to -26</description></item>
///   <item><term>Neutral</term><description>-25 to +24</description></item>
///   <item><term>Friendly</term><description>+25 to +49</description></item>
///   <item><term>Allied</term><description>+50 to +74</description></item>
///   <item><term>Exalted</term><description>+75 to +100</description></item>
/// </list>
/// </para>
/// </remarks>
public enum ReputationTier
{
    /// <summary>Kill on sight, +50% prices. Range: -100 to -76.</summary>
    Hated = 0,

    /// <summary>Attack if provoked, +25% prices. Range: -75 to -26.</summary>
    Hostile = 1,

    /// <summary>Standard interactions. Range: -25 to +24.</summary>
    Neutral = 2,

    /// <summary>-10% prices, some quests available. Range: +25 to +49.</summary>
    Friendly = 3,

    /// <summary>-20% prices, most content available. Range: +50 to +74.</summary>
    Allied = 4,

    /// <summary>-30% prices, all rewards available. Range: +75 to +100.</summary>
    Exalted = 5
}
