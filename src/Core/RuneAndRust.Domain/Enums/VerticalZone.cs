namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Classifies vertical layers for realm placement in the YGGDRASIL structure.
/// </summary>
/// <remarks>
/// <para>
/// The YGGDRASIL Network spans multiple vertical strata, from deep underground
/// root systems to orbital platforms. Each realm has a valid vertical range
/// where it can appear during sector generation.
/// </para>
/// <para>
/// Vertical zones influence navigation times, available resources, and
/// environmental conditions. Deeper zones tend toward industrial/toxic,
/// while higher zones trend toward command/research functions.
/// </para>
/// </remarks>
public enum VerticalZone
{
    /// <summary>
    /// Deepest underground level (Z = -3).
    /// </summary>
    /// <remarks>
    /// Ancient foundations, sealed vaults, forgotten infrastructure.
    /// Typical realms: Helheim, Niflheim.
    /// </remarks>
    DeepRoots = -3,

    /// <summary>
    /// Lower underground level (Z = -2).
    /// </summary>
    /// <remarks>
    /// Mining tunnels, geothermal taps, resource extraction.
    /// Typical realms: Svartalfheim, Muspelheim.
    /// </remarks>
    LowerRoots = -2,

    /// <summary>
    /// Upper underground level (Z = -1).
    /// </summary>
    /// <remarks>
    /// Subterranean transit, storage facilities, support infrastructure.
    /// Typical realms: Svartalfheim, Helheim.
    /// </remarks>
    UpperRoots = -1,

    /// <summary>
    /// Ground level (Z = 0).
    /// </summary>
    /// <remarks>
    /// Primary habitation, agricultural zones, main thoroughfares.
    /// Typical realms: Midgard, Vanaheim.
    /// </remarks>
    GroundLevel = 0,

    /// <summary>
    /// Lower trunk level (Z = +1).
    /// </summary>
    /// <remarks>
    /// Elevated platforms, biodomes, research facilities.
    /// Typical realms: Midgard, Vanaheim, Jotunheim.
    /// </remarks>
    LowerTrunk = 1,

    /// <summary>
    /// Upper trunk level (Z = +2).
    /// </summary>
    /// <remarks>
    /// High-altitude structures, specialized research, restricted areas.
    /// Typical realms: Alfheim, Jotunheim.
    /// </remarks>
    UpperTrunk = 2,

    /// <summary>
    /// Canopy level (Z = +3).
    /// </summary>
    /// <remarks>
    /// Observation decks, command overlooks, luxury habitations.
    /// Typical realms: Alfheim, Asgard.
    /// </remarks>
    Canopy = 3,

    /// <summary>
    /// Orbital level (Z = +4).
    /// </summary>
    /// <remarks>
    /// Orbital ring segments, command stations, satellite facilities.
    /// Typical realms: Asgard.
    /// </remarks>
    Orbital = 4
}
