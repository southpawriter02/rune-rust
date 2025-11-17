namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38.2: Environmental feature type enumeration
/// Defines the three core categories of environmental features
/// </summary>
public enum EnvironmentalFeatureType
{
    /// <summary>
    /// Permanent features (cover, chasms, elevation)
    /// Examples: Pillars, Platforms, Bulkheads
    /// </summary>
    StaticTerrain,

    /// <summary>
    /// Active dangers (steam vents, electrical hazards)
    /// Examples: Steam Vents, Power Conduits, Burning Ground
    /// </summary>
    DynamicHazard,

    /// <summary>
    /// Movement restrictions (rubble, difficult terrain)
    /// Examples: Rubble Piles, Debris, Obstacles
    /// </summary>
    NavigationalObstacle
}

/// <summary>
/// Cover quality levels for tactical positioning
/// </summary>
public enum CoverQuality
{
    /// <summary>
    /// No cover provided
    /// </summary>
    None,

    /// <summary>
    /// Light cover: -2 dice to hit, does not block line of sight
    /// Examples: Crates, Low Walls, Rubble
    /// </summary>
    Light,

    /// <summary>
    /// Heavy cover: -4 dice to hit, blocks line of sight
    /// Examples: Pillars, Bulkheads, Large Structures
    /// </summary>
    Heavy
}

/// <summary>
/// Hazard activation patterns for dynamic hazards
/// </summary>
public enum HazardActivationType
{
    /// <summary>
    /// Activates every N turns
    /// Example: Steam Vent (every 3 turns)
    /// </summary>
    Periodic,

    /// <summary>
    /// Activates when character enters range
    /// Example: Power Conduit (range 2 tiles)
    /// </summary>
    Proximity,

    /// <summary>
    /// Activates when specific action occurs
    /// Example: Unstable Ceiling (loud actions, explosions)
    /// </summary>
    Triggered,

    /// <summary>
    /// Activates at start or end of turn
    /// Example: Burning Ground (end of turn)
    /// </summary>
    Persistent,

    /// <summary>
    /// Activates when character moves across tiles
    /// Example: Electrified Floor
    /// </summary>
    Movement
}

/// <summary>
/// Timing for persistent hazard activation
/// </summary>
public enum HazardActivationTiming
{
    /// <summary>
    /// Activates at the start of the character's turn
    /// </summary>
    StartOfTurn,

    /// <summary>
    /// Activates at the end of the character's turn
    /// </summary>
    EndOfTurn,

    /// <summary>
    /// Activates immediately when triggered
    /// </summary>
    Immediate
}

/// <summary>
/// Area effect patterns for hazards
/// </summary>
public enum AreaEffectPattern
{
    /// <summary>
    /// Single tile
    /// </summary>
    Single,

    /// <summary>
    /// 3x3 area
    /// </summary>
    ThreeByThree,

    /// <summary>
    /// 5x5 area
    /// </summary>
    FiveByFive,

    /// <summary>
    /// Linear pattern (river, corridor)
    /// </summary>
    Line,

    /// <summary>
    /// Cone pattern (steam jet, flame burst)
    /// </summary>
    Cone,

    /// <summary>
    /// Entire room affected
    /// </summary>
    RoomWide,

    /// <summary>
    /// All combatants in the room
    /// </summary>
    AllCombatants,

    /// <summary>
    /// Custom pattern defined by tiles
    /// </summary>
    Custom
}
