// ═══════════════════════════════════════════════════════════════════════════════
// WeaponCategory.cs
// Enum classifying weapons into categories for proficiency tracking.
// Version: 0.16.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Classifies weapons into categories for proficiency tracking.
/// </summary>
/// <remarks>
/// <para>
/// Weapon categories group similar weapons together, allowing the
/// proficiency system to track category-level competency rather than
/// individual weapon types.
/// </para>
/// <para>
/// Each category has an associated primary attribute (MIGHT, FINESSE,
/// WILL, or WITS) that affects attack and damage calculations.
/// </para>
/// <para>
/// Values are explicitly assigned (0-10) for stable serialization.
/// </para>
/// <para>
/// Category groupings by primary attribute:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Attribute</term>
///     <description>Categories</description>
///   </listheader>
///   <item>
///     <term>MIGHT</term>
///     <description>Axes, Swords, Hammers, Polearms, Shields</description>
///   </item>
///   <item>
///     <term>FINESSE</term>
///     <description>Daggers, Bows</description>
///   </item>
///   <item>
///     <term>WILL</term>
///     <description>Staves, ArcaneImplements</description>
///   </item>
///   <item>
///     <term>WITS</term>
///     <description>Crossbows, Firearms</description>
///   </item>
/// </list>
/// <para>
/// Note: <see cref="Firearms"/> is the only category that requires special
/// training beyond standard archetype proficiencies.
/// </para>
/// </remarks>
/// <seealso cref="AttributeType"/>
/// <seealso cref="ValueObjects.WeaponCategoryDefinition"/>
/// <seealso cref="WeaponProficiencyLevel"/>
public enum WeaponCategory
{
    /// <summary>
    /// Heavy chopping weapons including battleaxes and handaxes.
    /// </summary>
    /// <remarks>
    /// <para>Primary attribute: MIGHT. High damage, moderate speed.</para>
    /// <para>Example weapons: Battleaxe, Handaxe, Greataxe, Throwing Axe.</para>
    /// </remarks>
    Axes = 0,

    /// <summary>
    /// Versatile bladed weapons including longswords and shortswords.
    /// </summary>
    /// <remarks>
    /// <para>Primary attribute: MIGHT. Balanced damage and speed.</para>
    /// <para>Example weapons: Longsword, Shortsword, Greatsword, Rapier.</para>
    /// </remarks>
    Swords = 1,

    /// <summary>
    /// Blunt impact weapons including warhammers and maces.
    /// </summary>
    /// <remarks>
    /// <para>Primary attribute: MIGHT. High damage vs. armored targets.</para>
    /// <para>Example weapons: Warhammer, Mace, Maul, Flail.</para>
    /// </remarks>
    Hammers = 2,

    /// <summary>
    /// Light, precise bladed weapons including daggers and stilettos.
    /// </summary>
    /// <remarks>
    /// <para>Primary attribute: FINESSE. Fast attacks, critical bonuses.</para>
    /// <para>Example weapons: Dagger, Stiletto, Kris, Throwing Knife.</para>
    /// </remarks>
    Daggers = 3,

    /// <summary>
    /// Long-hafted weapons including spears, halberds, and glaives.
    /// </summary>
    /// <remarks>
    /// <para>Primary attribute: MIGHT. Reach advantage in combat.</para>
    /// <para>Example weapons: Spear, Halberd, Glaive, Pike.</para>
    /// </remarks>
    Polearms = 4,

    /// <summary>
    /// Two-handed arcane implements used for both melee and magic.
    /// </summary>
    /// <remarks>
    /// <para>Primary attribute: WILL. Enhances spellcasting.</para>
    /// <para>Example weapons: Quarterstaff, Arcane Staff, Runestaff.</para>
    /// </remarks>
    Staves = 5,

    /// <summary>
    /// Drawn ranged weapons including longbows and shortbows.
    /// </summary>
    /// <remarks>
    /// <para>Primary attribute: FINESSE. Long range, requires arrows.</para>
    /// <para>Example weapons: Longbow, Shortbow, Composite Bow, Recurve Bow.</para>
    /// </remarks>
    Bows = 6,

    /// <summary>
    /// Mechanical ranged weapons including light and heavy crossbows.
    /// </summary>
    /// <remarks>
    /// <para>Primary attribute: WITS. High damage, slower reload.</para>
    /// <para>Example weapons: Light Crossbow, Heavy Crossbow, Hand Crossbow.</para>
    /// </remarks>
    Crossbows = 7,

    /// <summary>
    /// Defensive equipment used for blocking and shield-bashing.
    /// </summary>
    /// <remarks>
    /// <para>Primary attribute: MIGHT. Provides defense bonuses.</para>
    /// <para>Example weapons: Buckler, Round Shield, Kite Shield, Tower Shield.</para>
    /// </remarks>
    Shields = 8,

    /// <summary>
    /// Gunpowder-based ranged weapons including pistols and muskets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Primary attribute: WITS. Requires special training beyond
    /// standard archetype proficiencies.
    /// </para>
    /// <para>Example weapons: Pistol, Musket, Blunderbuss, Arquebus.</para>
    /// <para>
    /// This is the only weapon category that requires special training
    /// (<see cref="ValueObjects.WeaponCategoryDefinition.RequiresSpecialTraining"/> = true).
    /// </para>
    /// </remarks>
    Firearms = 9,

    /// <summary>
    /// Magical foci including wands, orbs, and tomes.
    /// </summary>
    /// <remarks>
    /// <para>Primary attribute: WILL. Enhances spell power.</para>
    /// <para>Example weapons: Wand, Orb, Tome, Crystal Focus.</para>
    /// </remarks>
    ArcaneImplements = 10
}
