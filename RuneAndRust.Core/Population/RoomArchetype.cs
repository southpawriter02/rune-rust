namespace RuneAndRust.Core.Population;

/// <summary>
/// Room archetype classifications for procedural generation (v0.10+)
/// Determines structural layout, size, and population constraints
/// </summary>
public enum RoomArchetype
{
    // Core Room Types (v0.10)
    EntryHall,          // Starting room - safe haven
    Corridor,           // Linear connector
    Chamber,            // Medium multi-purpose room
    LargeChamber,       // Spacious room for major encounters
    Hub,                // Multi-exit junction
    BossArena,          // Final boss encounter space
    SecretRoom,         // Hidden optional room

    // v0.11+ Specialized Rooms
    GeothermalStation,  // Biome-specific: thermal pumping infrastructure
    MaintenanceHub,     // Biome-specific: automaton service area
    StorageCavern,      // Loot-heavy room
    CollapseZone        // Hazard-heavy room
}
