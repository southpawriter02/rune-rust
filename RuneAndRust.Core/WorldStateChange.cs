namespace RuneAndRust.Core;

/// <summary>
/// v0.13: Represents a persistent change to the world state within a Saga.
/// Uses delta-based storage to efficiently track player modifications.
/// </summary>
public class WorldStateChange
{
    public int Id { get; set; }
    public int SaveId { get; set; } // Foreign key to saves table

    // Room identification
    public string SectorSeed { get; set; } = string.Empty; // Which generated Sector
    public string RoomId { get; set; } = string.Empty;     // Which room in Sector

    // Change details
    public WorldStateChangeType ChangeType { get; set; }
    public string TargetId { get; set; } = string.Empty;   // What was changed
    public string ChangeData { get; set; } = string.Empty; // JSON with change details

    // Metadata
    public DateTime Timestamp { get; set; }
    public int TurnNumber { get; set; }

    // Versioning for future compatibility
    public int SchemaVersion { get; set; } = 1;
}

/// <summary>
/// v0.13: Types of world state changes that can be recorded
/// </summary>
public enum WorldStateChangeType
{
    TerrainDestroyed,      // Pillar, wall, obstacle removed
    TerrainCreated,        // Rubble pile, barricade added
    TerrainModified,       // Partial damage, state change
    HazardDestroyed,       // Hazard permanently removed
    HazardTriggered,       // One-time hazard already activated
    EnemyDefeated,         // Dormant Process permanently killed
    LootCollected,         // Loot node depleted
    DoorUnlocked,          // Persistent unlocked state
    SecretRevealed         // Hidden passage discovered
}

/// <summary>
/// v0.13: Data for terrain destruction changes
/// </summary>
public class TerrainDestroyedData
{
    public string ElementType { get; set; } = string.Empty; // "CollapsedPillar", "RustedBulkhead"
    public bool SpawnRubble { get; set; }
    public string DestroyedBy { get; set; } = string.Empty; // Player or combat event
}

/// <summary>
/// v0.13: Data for hazard destruction changes
/// </summary>
public class HazardDestroyedData
{
    public string HazardType { get; set; } = string.Empty; // "SteamVent", "PowerConduit"
    public bool CausedSecondaryEffect { get; set; }       // Explosion, flooding, etc.
}

/// <summary>
/// v0.13: Data for enemy defeat changes
/// </summary>
public class EnemyDefeatedData
{
    public string EnemyType { get; set; } = string.Empty;
    public string EnemyName { get; set; } = string.Empty;
    public bool DroppedLoot { get; set; }
    public string? LootDropId { get; set; }
}

/// <summary>
/// v0.13: Data for loot collection changes
/// </summary>
public class LootCollectedData
{
    public string LootNodeType { get; set; } = string.Empty;
    public string ItemsCollected { get; set; } = string.Empty; // Comma-separated
}
