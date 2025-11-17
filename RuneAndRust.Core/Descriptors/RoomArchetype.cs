namespace RuneAndRust.Core.Descriptors;

/// <summary>
/// v0.38.1: Room archetype enumeration
/// Defines the six core room types for procedural generation (from v0.10)
/// </summary>
public enum RoomArchetype
{
    /// <summary>
    /// Starting rooms - safer, provides orientation
    /// Size: Medium, Exits: 1-2, Danger: Low
    /// </summary>
    EntryHall,

    /// <summary>
    /// Linear transit passages - connectors between rooms
    /// Size: Small, Exits: 2, Danger: Medium
    /// </summary>
    Corridor,

    /// <summary>
    /// Large combat and exploration spaces
    /// Size: Large, Exits: 1-4, Danger: High
    /// </summary>
    Chamber,

    /// <summary>
    /// Branching points - offers navigation choices
    /// Size: Medium, Exits: 3-4, Danger: Medium
    /// </summary>
    Junction,

    /// <summary>
    /// Climactic boss encounter spaces
    /// Size: XLarge, Exits: 1, Danger: Extreme
    /// </summary>
    BossArena,

    /// <summary>
    /// Hidden optional rooms with bonus loot
    /// Size: Small-Medium, Exits: 1, Danger: Low-Medium
    /// </summary>
    SecretRoom,

    /// <summary>
    /// Vertical transit between biome levels
    /// Size: Medium, Exits: 2, Danger: High
    /// </summary>
    VerticalShaft,

    /// <summary>
    /// Maintenance and utility junction
    /// Size: Medium, Exits: 2-4, Danger: Medium
    /// </summary>
    MaintenanceHub,

    /// <summary>
    /// Storage and salvage area
    /// Size: Large, Exits: 1-2, Danger: Low
    /// </summary>
    StorageBay,

    /// <summary>
    /// Elevated vantage point
    /// Size: Medium, Exits: 1-2, Danger: Low
    /// </summary>
    ObservationPlatform,

    /// <summary>
    /// Power generation facility
    /// Size: Large, Exits: 1-3, Danger: High
    /// </summary>
    PowerStation,

    /// <summary>
    /// Research and experimentation facility
    /// Size: Medium, Exits: 1-2, Danger: Medium
    /// </summary>
    Laboratory,

    /// <summary>
    /// Military living quarters
    /// Size: Medium, Exits: 1-2, Danger: Medium
    /// </summary>
    Barracks,

    /// <summary>
    /// Forge and metalworking facility
    /// Size: Large, Exits: 1-2, Danger: High
    /// </summary>
    ForgeCharnber,

    /// <summary>
    /// Cryogenic storage facility
    /// Size: Large, Exits: 1-2, Danger: Medium
    /// </summary>
    CryoVault
}

/// <summary>
/// Extension methods for RoomArchetype
/// </summary>
public static class RoomArchetypeExtensions
{
    /// <summary>
    /// Gets the base template name for this archetype
    /// </summary>
    public static string GetBaseTemplateName(this RoomArchetype archetype)
    {
        return archetype switch
        {
            RoomArchetype.EntryHall => "Entry_Hall_Base",
            RoomArchetype.Corridor => "Corridor_Base",
            RoomArchetype.Chamber => "Chamber_Base",
            RoomArchetype.Junction => "Junction_Base",
            RoomArchetype.BossArena => "Boss_Arena_Base",
            RoomArchetype.SecretRoom => "Secret_Room_Base",
            RoomArchetype.VerticalShaft => "Vertical_Shaft_Base",
            RoomArchetype.MaintenanceHub => "Maintenance_Hub_Base",
            RoomArchetype.StorageBay => "Storage_Bay_Base",
            RoomArchetype.ObservationPlatform => "Observation_Platform_Base",
            RoomArchetype.PowerStation => "Power_Station_Base",
            RoomArchetype.Laboratory => "Laboratory_Base",
            RoomArchetype.Barracks => "Barracks_Base",
            RoomArchetype.ForgeCharnber => "Forge_Chamber_Base",
            RoomArchetype.CryoVault => "Cryo_Vault_Base",
            _ => "Chamber_Base"  // Fallback
        };
    }

    /// <summary>
    /// Gets the expected room size for this archetype
    /// </summary>
    public static string GetExpectedSize(this RoomArchetype archetype)
    {
        return archetype switch
        {
            RoomArchetype.EntryHall => "Medium",
            RoomArchetype.Corridor => "Small",
            RoomArchetype.Chamber => "Large",
            RoomArchetype.Junction => "Medium",
            RoomArchetype.BossArena => "XLarge",
            RoomArchetype.SecretRoom => "Small",
            RoomArchetype.VerticalShaft => "Medium",
            RoomArchetype.MaintenanceHub => "Medium",
            RoomArchetype.StorageBay => "Large",
            RoomArchetype.ObservationPlatform => "Medium",
            RoomArchetype.PowerStation => "Large",
            RoomArchetype.Laboratory => "Medium",
            RoomArchetype.Barracks => "Medium",
            RoomArchetype.ForgeCharnber => "Large",
            RoomArchetype.CryoVault => "Large",
            _ => "Medium"
        };
    }

    /// <summary>
    /// Checks if this archetype is suitable for a specific biome
    /// </summary>
    public static bool IsSuitableForBiome(this RoomArchetype archetype, string biome)
    {
        return (archetype, biome) switch
        {
            (RoomArchetype.ForgeCharnber, "Muspelheim") => true,
            (RoomArchetype.ForgeCharnber, _) => false,  // Forge only in Muspelheim

            (RoomArchetype.CryoVault, "Niflheim") => true,
            (RoomArchetype.CryoVault, _) => false,  // Cryo only in Niflheim

            (RoomArchetype.Laboratory, "Alfheim") => true,  // Prefer Alfheim
            (RoomArchetype.Laboratory, _) => true,  // But allowed elsewhere

            _ => true  // All other archetypes suitable for all biomes
        };
    }
}
