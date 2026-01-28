// ═══════════════════════════════════════════════════════════════════════════════
// AttributeType.cs
// Enum defining the primary character attributes used for weapon scaling.
// Version: 0.16.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the primary character attributes used for weapon and ability scaling.
/// </summary>
/// <remarks>
/// <para>
/// AttributeType categorizes the four primary combat attributes that weapons
/// use for attack and damage calculations. Each weapon category has an associated
/// primary attribute that determines its effectiveness scaling.
/// </para>
/// <para>
/// The four primary attributes are:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="Might"/>: Raw physical power for heavy melee weapons and shields</description></item>
///   <item><description><see cref="Finesse"/>: Agility and precision for light melee and drawn ranged weapons</description></item>
///   <item><description><see cref="Will"/>: Mental fortitude and magical aptitude for arcane implements and staves</description></item>
///   <item><description><see cref="Wits"/>: Intelligence and perception for mechanical ranged and firearms</description></item>
/// </list>
/// <para>
/// Values are explicitly assigned (0-3) to ensure stable serialization
/// and database storage.
/// </para>
/// <para>
/// Weapon category to attribute mapping:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Attribute</term>
///     <description>Weapon Categories</description>
///   </listheader>
///   <item>
///     <term>Might</term>
///     <description>Axes, Swords, Hammers, Polearms, Shields</description>
///   </item>
///   <item>
///     <term>Finesse</term>
///     <description>Daggers, Bows</description>
///   </item>
///   <item>
///     <term>Will</term>
///     <description>Staves, ArcaneImplements</description>
///   </item>
///   <item>
///     <term>Wits</term>
///     <description>Crossbows, Firearms</description>
///   </item>
/// </list>
/// </remarks>
/// <seealso cref="WeaponCategory"/>
/// <seealso cref="ValueObjects.WeaponCategoryDefinition"/>
public enum AttributeType
{
    /// <summary>
    /// Physical power and strength attribute.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Might is the primary attribute for heavy melee weapons that rely on
    /// raw physical power. Characters with high Might excel at:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Axes: Heavy chopping attacks with devastating damage</description></item>
    ///   <item><description>Swords: Versatile bladed combat with balanced offense/defense</description></item>
    ///   <item><description>Hammers: Crushing blows effective against armored targets</description></item>
    ///   <item><description>Polearms: Reach weapons for controlling battlefield spacing</description></item>
    ///   <item><description>Shields: Defensive blocking and offensive shield bashing</description></item>
    /// </list>
    /// <para>
    /// Might-based weapons typically deal higher base damage but may have
    /// lower attack speed compared to Finesse weapons.
    /// </para>
    /// </remarks>
    Might = 0,

    /// <summary>
    /// Agility and precision attribute.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Finesse is the primary attribute for weapons requiring dexterity and
    /// precise aim. Characters with high Finesse excel at:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Daggers: Swift, precise strikes with critical hit bonuses</description></item>
    ///   <item><description>Bows: Drawn ranged weapons requiring steady aim and coordination</description></item>
    /// </list>
    /// <para>
    /// Finesse-based weapons typically have faster attack speeds and higher
    /// critical hit chances but may deal lower base damage per hit.
    /// </para>
    /// </remarks>
    Finesse = 1,

    /// <summary>
    /// Mental fortitude and magical aptitude attribute.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Will is the primary attribute for arcane implements and magical staves.
    /// Characters with high Will excel at:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Staves: Two-handed implements that channel arcane energy</description></item>
    ///   <item><description>ArcaneImplements: Wands, orbs, and tomes that enhance spellcasting</description></item>
    /// </list>
    /// <para>
    /// Will-based weapons enhance magical abilities and may provide bonuses
    /// to spell power, mana regeneration, or Galdr effectiveness.
    /// </para>
    /// </remarks>
    Will = 2,

    /// <summary>
    /// Intelligence and mechanical aptitude attribute.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Wits is the primary attribute for mechanical and gunpowder-based weapons.
    /// Characters with high Wits excel at:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Crossbows: Mechanical ranged weapons with powerful bolts</description></item>
    ///   <item><description>Firearms: Gunpowder weapons requiring specialized training</description></item>
    /// </list>
    /// <para>
    /// Wits-based weapons often deal high damage but may have longer reload
    /// times or require special ammunition. Firearms additionally require
    /// special training beyond standard archetype proficiencies.
    /// </para>
    /// </remarks>
    Wits = 3
}
