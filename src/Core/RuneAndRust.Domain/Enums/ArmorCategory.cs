// ═══════════════════════════════════════════════════════════════════════════════
// ArmorCategory.cs
// Enum classifying armor into categories for proficiency and penalty tracking.
// Version: 0.16.2b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Classifies armor into categories for proficiency and penalty tracking.
/// </summary>
/// <remarks>
/// <para>
/// Armor categories group protective equipment by weight class, determining
/// base penalties to agility, stamina costs, movement speed, and stealth.
/// </para>
/// <para>
/// Categories Light, Medium, and Heavy form a weight tier hierarchy (0-2)
/// used by the proficiency tier reduction system. Shields and Specialized
/// armor are handled separately.
/// </para>
/// <para>
/// The tier hierarchy enables Expert/Master proficiency to reduce penalties:
/// <list type="bullet">
///   <item><description>Expert in Heavy armor treats it as Medium for penalties</description></item>
///   <item><description>Expert in Medium armor treats it as Light for penalties</description></item>
///   <item><description>Shields are not affected by tier reduction</description></item>
/// </list>
/// </para>
/// <para>
/// Values are explicitly assigned (0-4) for stable serialization and
/// configuration file compatibility.
/// </para>
/// </remarks>
/// <seealso cref="ValueObjects.ArmorPenalties"/>
/// <seealso cref="ValueObjects.ArmorCategoryDefinition"/>
public enum ArmorCategory
{
    /// <summary>
    /// Light protective gear including cloth, padded, leather, and hide armor.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Light armor has no penalties and does not impose stealth disadvantage.
    /// This category is appropriate for characters who prioritize mobility
    /// and stealth over protection.
    /// </para>
    /// <para>
    /// Weight Tier: 0 (lowest tier, cannot be reduced further).
    /// </para>
    /// <para>
    /// Base Penalties: None.
    /// </para>
    /// <para>
    /// Example Types:
    /// <list type="bullet">
    ///   <item><description>Cloth Armor</description></item>
    ///   <item><description>Padded Armor</description></item>
    ///   <item><description>Leather Armor</description></item>
    ///   <item><description>Hide Armor</description></item>
    ///   <item><description>Studded Leather</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Light = 0,

    /// <summary>
    /// Medium protective gear including chain mail, scale mail, and brigandine.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Medium armor provides balanced protection with moderate penalties.
    /// This category trades some mobility for improved defense.
    /// </para>
    /// <para>
    /// Weight Tier: 1 (can be reduced to Light by Expert/Master proficiency).
    /// </para>
    /// <para>
    /// Base Penalties:
    /// <list type="bullet">
    ///   <item><description>-1d10 Agility dice on Agility-based rolls</description></item>
    ///   <item><description>+2 Stamina cost per combat action</description></item>
    ///   <item><description>-5 feet movement speed per round</description></item>
    ///   <item><description>Stealth disadvantage (imposed on stealth checks)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Example Types:
    /// <list type="bullet">
    ///   <item><description>Chain Mail</description></item>
    ///   <item><description>Scale Mail</description></item>
    ///   <item><description>Ring Mail</description></item>
    ///   <item><description>Brigandine</description></item>
    ///   <item><description>Splint Armor</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Medium = 1,

    /// <summary>
    /// Heavy protective gear including half plate and full plate armor.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Heavy armor provides maximum protection at the cost of severe penalties.
    /// This category is appropriate for frontline warriors who can afford
    /// reduced mobility in exchange for superior defense.
    /// </para>
    /// <para>
    /// Weight Tier: 2 (can be reduced to Medium by Expert/Master proficiency).
    /// </para>
    /// <para>
    /// Base Penalties:
    /// <list type="bullet">
    ///   <item><description>-2d10 Agility dice on Agility-based rolls</description></item>
    ///   <item><description>+5 Stamina cost per combat action</description></item>
    ///   <item><description>-10 feet movement speed per round</description></item>
    ///   <item><description>Stealth disadvantage (imposed on stealth checks)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Example Types:
    /// <list type="bullet">
    ///   <item><description>Half Plate</description></item>
    ///   <item><description>Full Plate</description></item>
    ///   <item><description>Fortress Armor</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Special Note: Heavy armor blocks Galdr casting for Mystics (Faraday-cage effect).
    /// </para>
    /// </remarks>
    Heavy = 2,

    /// <summary>
    /// Defensive equipment held in hand including bucklers and tower shields.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Shields are held defensive equipment that provide additional blocking
    /// capability. Unlike body armor, shields impose minimal penalties.
    /// </para>
    /// <para>
    /// Weight Tier: N/A (shields do not use the weight tier system and are
    /// not affected by tier reduction from proficiency).
    /// </para>
    /// <para>
    /// Base Penalties:
    /// <list type="bullet">
    ///   <item><description>+1 Stamina cost per combat action</description></item>
    ///   <item><description>No agility penalty</description></item>
    ///   <item><description>No movement penalty</description></item>
    ///   <item><description>No stealth disadvantage</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Example Types:
    /// <list type="bullet">
    ///   <item><description>Buckler</description></item>
    ///   <item><description>Round Shield</description></item>
    ///   <item><description>Kite Shield</description></item>
    ///   <item><description>Tower Shield</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Shields = 3,

    /// <summary>
    /// Exotic protective equipment requiring special training to use effectively.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Specialized armor includes unusual protective equipment that cannot
    /// be effectively used without specific training. These items often
    /// represent faction-specific technology or unique construction methods.
    /// </para>
    /// <para>
    /// Weight Tier: Varies by specific item (configured per-item).
    /// </para>
    /// <para>
    /// RequiresSpecialTraining: true (always requires explicit training).
    /// </para>
    /// <para>
    /// Base Penalties: Varies by specific item (example defaults provided).
    /// </para>
    /// <para>
    /// Example Types:
    /// <list type="bullet">
    ///   <item><description>Servitor Shell</description></item>
    ///   <item><description>Symbiotic Carapace</description></item>
    ///   <item><description>Rune-Bonded Plate</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Specialized = 4
}
