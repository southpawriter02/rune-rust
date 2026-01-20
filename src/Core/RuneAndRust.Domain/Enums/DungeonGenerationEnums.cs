namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the structural archetype of a room node in the dungeon graph.
/// Used by the topology generator to classify nodes based on connection count.
/// </summary>
public enum RoomArchetype
{
    /// <summary>A room with exactly 2 connections (passage between areas).</summary>
    Corridor,

    /// <summary>A larger room, typically with 2-4 connections and more space.</summary>
    Chamber,

    /// <summary>A room with 3+ connections (crossroads).</summary>
    Junction,

    /// <summary>A room with exactly 1 connection (end of a path).</summary>
    DeadEnd,

    /// <summary>A vertical transition room connecting different Z-levels.</summary>
    Stairwell,

    /// <summary>The final room of a sector, containing the boss encounter.</summary>
    BossArena
}

/// <summary>
/// Defines the combat role of an entity for threat budget allocation.
/// </summary>
public enum EntityRole
{
    /// <summary>Cheap fodder units (cost 1-5). Bought with leftover budget.</summary>
    Swarm,

    /// <summary>Standard melee combatants (cost 5-15).</summary>
    Melee,

    /// <summary>Ranged attackers (cost 10-20).</summary>
    Ranged,

    /// <summary>High-health defensive units (cost 20-40).</summary>
    Tank,

    /// <summary>Powerful special units (cost 40-80). 20% spawn chance.</summary>
    Elite,

    /// <summary>Sector boss (cost 100+). Only in BossArena rooms.</summary>
    Boss
}

/// <summary>
/// Types of features that can spawn in a room template.
/// </summary>
public enum RoomFeatureType
{
    /// <summary>Environmental hazards (traps, unstable floors, toxic gas).</summary>
    Hazard,

    /// <summary>Non-interactive visual elements for atmosphere.</summary>
    Decoration,

    /// <summary>Interactive objects (levers, terminals, containers).</summary>
    Interactable,

    /// <summary>Light sources affecting visibility and mood.</summary>
    LightSource
}

/// <summary>
/// Base difficulty tiers for dungeon generation.
/// Values represent the base threat budget for the sector.
/// </summary>
public enum DifficultyTier
{
    /// <summary>Easy - 100 base threat budget.</summary>
    Tier1 = 100,

    /// <summary>Normal - 150 base threat budget.</summary>
    Tier2 = 150,

    /// <summary>Hard - 200 base threat budget.</summary>
    Tier3 = 200,

    /// <summary>Nightmare - 300 base threat budget.</summary>
    Tier4 = 300
}

// Note: WitsCheckMargin, WitsFailureMargin, and LootQuality are defined in ExaminationEnums.cs
