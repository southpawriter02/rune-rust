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
    Junction,           // Branching point (alias for Hub, for compatibility)
    BossArena,          // Final boss encounter space
    SecretRoom,         // Hidden optional room

    // v0.11+ Specialized Rooms
    GeothermalStation,  // Biome-specific: thermal pumping infrastructure
    MaintenanceHub,     // Biome-specific: automaton service area
    StorageCavern,      // Loot-heavy room
    CollapseZone,       // Hazard-heavy room

    // v0.38+ Extended Types (for descriptor system compatibility)
    VerticalShaft,      // Vertical transit between levels
    StorageBay,         // Storage and salvage area
    ObservationPlatform, // Elevated vantage point
    PowerStation,       // Power generation facility
    Laboratory,         // Research facility
    Barracks,           // Military living quarters
    ForgeCharnber,      // Forge and metalworking facility
    CryoVault           // Cryogenic storage facility
}
