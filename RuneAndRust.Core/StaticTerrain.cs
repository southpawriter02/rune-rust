namespace RuneAndRust.Core;

/// <summary>
/// v0.11 Static Terrain System
/// Permanent environmental features providing tactical options
/// Unlike Dynamic Hazards, these don't activate or move
/// </summary>
public class StaticTerrain
{
    // Identity
    public string TerrainId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StaticTerrainType Type { get; set; }

    // Tactical Properties
    public CoverType CoverProvided { get; set; } = CoverType.None;
    public bool ProvidesTouchCover { get; set; } = false; // +2 DEF when adjacent (compatibility with Population.StaticTerrain)
    public int AccuracyModifier { get; set; } = 0; // -2 dice for partial cover, -4 for full
    public bool BlocksLineOfSight { get; set; } = false;
    public bool BlocksMovement { get; set; } = false;

    // Movement
    public int MovementCostModifier { get; set; } = 0; // +2 for difficult terrain
    public bool IsDifficultTerrain { get; set; } = false;
    public bool RequiresClimbing { get; set; } = false;

    // Elevation
    public bool ProvidesElevation { get; set; } = false;
    public int ElevationBonus { get; set; } = 0; // +1d for ranged attacks from elevation

    // Hazards
    public bool IsHazardous { get; set; } = false; // e.g., Chasm causes fall damage
    public int HazardDamageDice { get; set; } = 0;
    public string HazardCondition { get; set; } = string.Empty; // "if forced into"

    // Destructibility
    public bool IsDestructible { get; set; } = false;
    public int HP { get; set; } = 0;

    // Coherent Glitch Rule Connections
    public string? MandatoryIfHazardPresent { get; set; } = null; // "UnstableCeiling" requires RubblePile
}

/// <summary>
/// Types of static terrain for v0.11
/// </summary>
public enum StaticTerrainType
{
    CollapsedPillar,    // Full cover, blocks LoS and movement
    RubblePile,         // Partial cover, difficult terrain
    RustedBulkhead,     // Full cover, blocks LoS
    Chasm,              // Blocks movement, fall damage if forced in
    ElevatedPlatform,   // Tactical high ground
    CorrodedGrating,    // Fragile floor (also a hazard)
    BrokenGantry,       // Partial bridge over chasm
    CollapseDebris,     // Difficult terrain, no cover
    SteelBarricade,     // Full cover, destructible
    ExposedConduit      // Light cover, electrical hazard adjacent
}

/// <summary>
/// Types of cover provided by terrain
/// </summary>
public enum CoverType
{
    None,       // No cover
    Partial,    // -2 dice to hit
    Full        // -4 dice to hit, blocks LoS
}
