namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of environmental hazards that can be present in rooms.
/// </summary>
/// <remarks>
/// Environmental hazards deal damage or apply status effects to players
/// who remain in or pass through affected areas. Each type has distinct
/// visual and mechanical characteristics that affect gameplay:
/// <list type="bullet">
///   <item><description><see cref="PoisonGas"/> - Choking fumes that sicken the body</description></item>
///   <item><description><see cref="Fire"/> - Unsleeping fire that hungers</description></item>
///   <item><description><see cref="Ice"/> - The grave-cold that slows movement</description></item>
///   <item><description><see cref="Spikes"/> - Teeth of iron and stone</description></item>
///   <item><description><see cref="AcidPool"/> - Biting water that eats metal</description></item>
///   <item><description><see cref="Darkness"/> - Shadows thick enough to drown in</description></item>
///   <item><description><see cref="Electricity"/> - The anger of storms made manifest</description></item>
///   <item><description><see cref="Radiant"/> - Sun-fire that scours the shadows</description></item>
///   <item><description><see cref="Necrotic"/> - Shadow-sickness that drains the spirit</description></item>
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
    /// Biting water that eats through metal and flesh.
    /// Deals acid damage and may damage armor or weapons over time.
    /// </summary>
    AcidPool = 4,

    /// <summary>
    /// Unnatural shadow that drowns all light.
    /// Does not deal damage but imposes penalties to perception and attacks.
    /// </summary>
    Darkness = 5,

    /// <summary>
    /// The invisible fire of storms.
    /// Deals lightning damage and may cause paralysis on critical failures.
    /// </summary>
    Electricity = 6,

    /// <summary>
    /// Sun-fire or star-light.
    /// Deals radiant damage, particularly effective against undead creatures.
    /// </summary>
    Radiant = 7,

    /// <summary>
    /// Shadow-sickness that drains the spirit.
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
    Lava = 10
}
