// ═══════════════════════════════════════════════════════════════════════════════
// EquipmentClassAffinity.cs
// Enum identifying which archetype an equipment category is designed for.
// Version: 0.16.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Identifies which archetype an equipment category is designed for.
/// </summary>
/// <remarks>
/// <para>
/// Equipment class affinity enables smart loot generation to bias drops
/// toward gear appropriate for the player's archetype.
/// </para>
/// <para>
/// Each archetype aligns with a primary attribute:
/// <list type="bullet">
///   <item><description><see cref="Warrior"/> → MIGHT (heavy weapons, heavy armor)</description></item>
///   <item><description><see cref="Skirmisher"/> → FINESSE (agile weapons, medium armor)</description></item>
///   <item><description><see cref="Mystic"/> → WILL (arcane implements, light armor)</description></item>
///   <item><description><see cref="Adept"/> → WITS (technical equipment, light armor)</description></item>
///   <item><description><see cref="Universal"/> → All (light armor, basic items)</description></item>
/// </list>
/// </para>
/// <para>
/// Values are explicitly assigned (0-4) for stable serialization and
/// configuration file compatibility.
/// </para>
/// </remarks>
/// <seealso cref="ValueObjects.EquipmentClassMapping"/>
public enum EquipmentClassAffinity
{
    /// <summary>
    /// Equipment designed for Warriors.
    /// </summary>
    /// <remarks>
    /// <para>
    /// MIGHT-scaling weapons (axes, greatswords, hammers) and heavy armor.
    /// Warriors are frontline combatants who prioritize raw damage output
    /// and durability over mobility or finesse.
    /// </para>
    /// <para>
    /// Weapon Categories:
    /// <list type="bullet">
    ///   <item><description>Axes (hand axes, bearded axes, greataxes)</description></item>
    ///   <item><description>Greatswords (claymores, zweihanders, flamberges)</description></item>
    ///   <item><description>Hammers (maces, war hammers, mauls)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Armor Categories:
    /// <list type="bullet">
    ///   <item><description>Heavy Armor (half plate, full plate, fortress armor)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Warrior = 0,

    /// <summary>
    /// Equipment designed for Skirmishers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// FINESSE-scaling weapons (daggers, blades, bows) and medium armor.
    /// Skirmishers excel at precision strikes, ranged combat, and hit-and-run tactics.
    /// </para>
    /// <para>
    /// Weapon Categories:
    /// <list type="bullet">
    ///   <item><description>Daggers (knives, stilettos, dirks)</description></item>
    ///   <item><description>Blades (short swords, sabers, rapiers)</description></item>
    ///   <item><description>Bows (shortbows, longbows, composite bows)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Armor Categories:
    /// <list type="bullet">
    ///   <item><description>Medium Armor (chain mail, scale mail, brigandine)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Skirmisher = 1,

    /// <summary>
    /// Equipment designed for Mystics.
    /// </summary>
    /// <remarks>
    /// <para>
    /// WILL-scaling implements (staves, foci, orbs). Mystics channel Galdr
    /// (runic magic) and require light armor to avoid interference.
    /// </para>
    /// <para>
    /// Weapon Categories:
    /// <list type="bullet">
    ///   <item><description>Staves (quarterstaffs, walking sticks, iron-shod staves)</description></item>
    ///   <item><description>Foci (wands, rods, channeling crystals)</description></item>
    ///   <item><description>Orbs (crystal orbs, spirit spheres, runestones)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Armor Restriction: Only light armor is proficient. Medium armor causes
    /// Galdr interference (-2 to checks), and heavy armor blocks Galdr entirely.
    /// </para>
    /// </remarks>
    Mystic = 2,

    /// <summary>
    /// Equipment designed for Adepts.
    /// </summary>
    /// <remarks>
    /// <para>
    /// WITS-scaling equipment (crossbows, arc-cannons). Adepts are technical
    /// specialists who utilize advanced pre-Glitch technology and gadgetry.
    /// </para>
    /// <para>
    /// Weapon Categories:
    /// <list type="bullet">
    ///   <item><description>Crossbows (light crossbows, heavy crossbows, repeating crossbows)</description></item>
    ///   <item><description>Arc-Cannons (tesla projectors, lightning rods, shock lances)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Armor Restriction: Only light armor is proficient. Heavier armor
    /// impairs WITS-based abilities due to electromagnetic interference.
    /// </para>
    /// </remarks>
    Adept = 3,

    /// <summary>
    /// Equipment usable by all archetypes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Universal equipment has no class restriction and is appropriate for
    /// any archetype. This includes basic protective gear and accessories.
    /// </para>
    /// <para>
    /// Equipment Categories:
    /// <list type="bullet">
    ///   <item><description>Light Armor (cloth armor, leather armor, padded armor)</description></item>
    ///   <item><description>Accessories (rings, amulets, tool belts, cloaks)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Smart loot generation always includes Universal items in the pool
    /// for any archetype.
    /// </para>
    /// </remarks>
    Universal = 4
}
