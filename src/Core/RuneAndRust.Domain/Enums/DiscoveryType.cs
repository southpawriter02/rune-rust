// ═══════════════════════════════════════════════════════════════════════════════
// LoreDiscoveryType.cs
// Categorizes types of lore discoveries that generate Lore Insight for the
// Jötun-Reader specialization. Named LoreDiscoveryType to distinguish from
// the perception-layer DiscoveryType in ExaminationEnums.cs.
// Version: 0.20.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of lore discoveries that generate Lore Insight for Jötun-Reader characters.
/// </summary>
/// <remarks>
/// <para>
/// Named <c>LoreDiscoveryType</c> to distinguish from the perception-layer
/// <see cref="DiscoveryType"/> (Secret, Lore, Danger) in ExaminationEnums.cs.
/// </para>
/// <para>
/// Each discovery type yields a base amount of Lore Insight when examined.
/// Significant or critical discoveries add bonus insight on top of the base.
/// </para>
/// <list type="bullet">
///   <item><description><b>+1 Insight:</b> JotunMachinery, DvergrArtifact, AncientText, Terminal</description></item>
///   <item><description><b>+2 Insight:</b> Ruin, LoreObject</description></item>
/// </list>
/// </remarks>
/// <seealso cref="ValueObjects.LoreInsightResource"/>
public enum LoreDiscoveryType
{
    /// <summary>
    /// Examining Jötun machinery or technology. Base: +1 Lore Insight.
    /// </summary>
    JotunMachinery = 0,

    /// <summary>
    /// Examining Dvergr artifacts or items. Base: +1 Lore Insight.
    /// </summary>
    DvergrArtifact = 1,

    /// <summary>
    /// Reading ancient texts or inscriptions. Base: +1 Lore Insight.
    /// </summary>
    AncientText = 2,

    /// <summary>
    /// Accessing data terminals. Base: +1 Lore Insight.
    /// </summary>
    Terminal = 3,

    /// <summary>
    /// Exploring a new ruin location. Base: +2 Lore Insight.
    /// </summary>
    Ruin = 4,

    /// <summary>
    /// Identifying objects of historical significance. Base: +2 Lore Insight.
    /// </summary>
    LoreObject = 5
}
