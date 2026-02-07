namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of environmental hazards that can be present in rooms.
/// </summary>
/// <remarks>
/// Environmental hazards deal damage or apply status effects to players
/// who remain in or pass through affected areas. Each type has distinct
/// visual and mechanical characteristics that affect gameplay:
/// <list type="bullet">
///   <item><description><see cref="PoisonGas"/> - Toxic clouds that poison on failed saves</description></item>
///   <item><description><see cref="Fire"/> - Flames that burn with fire damage</description></item>
///   <item><description><see cref="Ice"/> - Extreme cold that slows movement</description></item>
///   <item><description><see cref="Spikes"/> - Physical hazards dealing piercing damage</description></item>
///   <item><description><see cref="AcidPool"/> - Corrosive pools that damage equipment</description></item>
///   <item><description><see cref="Darkness"/> - Magical darkness affecting visibility (no damage)</description></item>
///   <item><description><see cref="Electricity"/> - Electrical fields that shock</description></item>
///   <item><description><see cref="Radiant"/> - Holy energy effective against undead</description></item>
///   <item><description><see cref="Necrotic"/> - Life-draining dark energy</description></item>
/// </list>
/// </remarks>
/// <seealso cref="RuneAndRust.Domain.Entities.HazardZone"/>
public enum HazardType
{
    /// <summary>
    /// Poisonous gas that damages and poisons creatures within.
    /// Typically requires Fortitude saves and applies the poisoned status effect.
    /// </summary>
    PoisonGas = 0,

    /// <summary>
    /// Fire or flames that burn creatures within.
    /// Deals fire damage and may apply the burning status effect.
    /// </summary>
    Fire = 1,

    /// <summary>
    /// Extreme cold that damages and slows creatures within.
    /// Deals cold damage and may apply the slowed or frozen status effects.
    /// </summary>
    Ice = 2,

    /// <summary>
    /// Sharp spikes that damage creatures moving through.
    /// Deals piercing damage, typically on entry rather than per-turn.
    /// </summary>
    Spikes = 3,

    /// <summary>
    /// Corrosive acid that damages and corrodes equipment.
    /// Deals acid damage and may damage armor or weapons over time.
    /// </summary>
    AcidPool = 4,

    /// <summary>
    /// Magical darkness that obscures vision.
    /// Does not deal damage but imposes penalties to perception and attacks.
    /// </summary>
    Darkness = 5,

    /// <summary>
    /// Electrical energy that shocks creatures within.
    /// Deals lightning damage and may cause paralysis on critical failures.
    /// </summary>
    Electricity = 6,

    /// <summary>
    /// Holy or radiant energy.
    /// Deals radiant damage, particularly effective against undead creatures.
    /// </summary>
    Radiant = 7,

    /// <summary>
    /// Necrotic energy that drains life force.
    /// Deals necrotic damage and may reduce maximum health temporarily.
    /// </summary>
    Necrotic = 8,

    /// <summary>
    /// A pit or chasm that creatures can fall into.
    /// Deals bludgeoning damage from the fall and may apply the prone status effect.
    /// Creatures must climb out to escape.
    /// </summary>
    /// <remarks>
    /// Added in v0.10.1c for environmental combat push/knockback mechanics.
    /// </remarks>
    Pit = 9,

    /// <summary>
    /// Molten lava or magma that burns creatures within.
    /// Deals heavy fire damage and applies the burning status effect.
    /// Damage occurs on entry and each turn the creature remains in the lava.
    /// </summary>
    /// <remarks>
    /// Added in v0.10.1c for environmental combat push/knockback mechanics.
    /// </remarks>
    Lava = 10,

    /// <summary>
    /// Hazardous terrain or objects that trigger on contact.
    /// Includes sharp thorns, corrosive moss, or electrified surfaces.
    /// </summary>
    Contact = 11,

    /// <summary>
    /// General environmental effects like storms or extreme weather.
    /// </summary>
    Environmental = 12,

    /// <summary>
    /// Mechanical or magical traps triggered by movement or interaction.
    /// </summary>
    Trap = 13,

    /// <summary>
    /// A dangerous gap or fissure in the terrain.
    /// Similar to pits but may have different effects like steam vents.
    /// </summary>
    Gap = 14,

    /// <summary>
    /// A localized curse or magical affliction.
    /// </summary>
    Curse = 15,

    /// <summary>
    /// Psychic hazards that attack the mind.
    /// </summary>
    Psychic = 16
}
