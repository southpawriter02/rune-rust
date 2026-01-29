// ═══════════════════════════════════════════════════════════════════════════════
// CoreAttribute.cs
// Enum defining the five core character attributes for the character creation system.
// Version: 0.17.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the five core character attributes used in character creation and progression.
/// </summary>
/// <remarks>
/// <para>
/// CoreAttribute represents the fundamental attributes that define a character's
/// capabilities. These attributes are set during character creation through the
/// point-buy system and can be modified by lineage, background, and equipment.
/// </para>
/// <para>
/// The five core attributes are:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="Might"/>: Physical power for melee damage and carrying capacity
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Finesse"/>: Agility and precision for ranged accuracy and evasion
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Wits"/>: Perception and knowledge for crafting and initiative
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Will"/>: Mental fortitude for Aether channeling and resolve checks
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Sturdiness"/>: Endurance and resilience for HP and resistance
///     </description>
///   </item>
/// </list>
/// <para>
/// Values are explicitly assigned (0-4) to ensure stable serialization
/// and database storage.
/// </para>
/// <para>
/// <strong>Point-Buy System:</strong>
/// <list type="bullet">
///   <item><description>Starting Pool: 15 points (14 for Adept archetype)</description></item>
///   <item><description>All attributes start at: 1</description></item>
///   <item><description>Values 2-8: 1 point each</description></item>
///   <item><description>Values 9-10: 2 points each</description></item>
///   <item><description>Maximum: 10 per attribute</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="ValueObjects.LineageAttributeModifiers"/>
public enum CoreAttribute
{
    /// <summary>
    /// Physical power and strength attribute.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Might determines raw physical capability. Characters with high Might
    /// excel at melee combat, breaking objects, and intimidation.
    /// </para>
    /// <para>
    /// Primary effects:
    /// <list type="bullet">
    ///   <item><description>Melee weapon damage bonus</description></item>
    ///   <item><description>Carrying capacity</description></item>
    ///   <item><description>Breaking objects and obstacles</description></item>
    ///   <item><description>Intimidation and physical presence</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Might = 0,

    /// <summary>
    /// Agility and precision attribute.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Finesse determines physical coordination and reaction speed.
    /// Characters with high Finesse excel at ranged combat, evasion,
    /// and precise manipulation.
    /// </para>
    /// <para>
    /// Primary effects:
    /// <list type="bullet">
    ///   <item><description>Ranged weapon accuracy</description></item>
    ///   <item><description>Dodge and evasion</description></item>
    ///   <item><description>Stealth movement</description></item>
    ///   <item><description>Lockpicking and fine manipulation</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Derived stat contribution: Max Stamina = (FINESSE × 5) + (MIGHT × 5) + 20
    /// </para>
    /// </remarks>
    Finesse = 1,

    /// <summary>
    /// Intelligence and perception attribute.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Wits determines mental acuity, perception, and technical knowledge.
    /// Characters with high Wits excel at investigation, crafting, and
    /// spotting hidden details.
    /// </para>
    /// <para>
    /// Primary effects:
    /// <list type="bullet">
    ///   <item><description>Critical hit chance</description></item>
    ///   <item><description>Trap detection</description></item>
    ///   <item><description>Lore and technical knowledge</description></item>
    ///   <item><description>Initiative bonus</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Derived stat contribution: Max AP = (WILL × 10) + (WITS × 5)
    /// </para>
    /// </remarks>
    Wits = 2,

    /// <summary>
    /// Mental fortitude and magical aptitude attribute.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Will determines mental resilience and connection to the Aether.
    /// Characters with high Will excel at spellcasting, resisting mind
    /// effects, and maintaining focus under pressure.
    /// </para>
    /// <para>
    /// Primary effects:
    /// <list type="bullet">
    ///   <item><description>Magic power and Aether channeling</description></item>
    ///   <item><description>Mind effect resistance</description></item>
    ///   <item><description>Focus and concentration</description></item>
    ///   <item><description>Aether Pool size</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Derived stat contribution: Max AP = (WILL × 10) + (WITS × 5)
    /// </para>
    /// </remarks>
    Will = 3,

    /// <summary>
    /// Endurance and physical resilience attribute.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sturdiness determines physical toughness and stamina. Characters
    /// with high Sturdiness have more health, better poison resistance,
    /// and can endure environmental hazards.
    /// </para>
    /// <para>
    /// Primary effects:
    /// <list type="bullet">
    ///   <item><description>Maximum health points</description></item>
    ///   <item><description>Poison and disease resistance</description></item>
    ///   <item><description>Environmental hazard resistance</description></item>
    ///   <item><description>Stamina regeneration</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Derived stat contribution: Max HP = (STURDINESS × 10) + 50 + Archetype Bonus
    /// </para>
    /// </remarks>
    Sturdiness = 4
}
