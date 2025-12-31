using RuneAndRust.Core.Enums;
using RuneAndRust.Core.ValueObjects;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Represents a discrete location in the game world.
/// Rooms form a connected graph via the Exits dictionary.
/// </summary>
public class Room
{
    /// <summary>
    /// Unique identifier for the room.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display name for the room (e.g., "Entry Hall", "Rusted Corridor").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descriptive text shown when the player looks or enters.
    /// Should follow AAM-VOICE guidelines for Layer 2 content.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The room's 3D position in the world grid.
    /// Backed by individual coordinate fields for EF Core compatibility (v0.3.18a).
    /// </summary>
    public Coordinate Position
    {
        get => new(PositionX, PositionY, PositionZ);
        set
        {
            PositionX = value.X;
            PositionY = value.Y;
            PositionZ = value.Z;
        }
    }

    /// <summary>
    /// X coordinate (East-West). Backing field for EF Core.
    /// </summary>
    public int PositionX { get; set; }

    /// <summary>
    /// Y coordinate (North-South). Backing field for EF Core.
    /// </summary>
    public int PositionY { get; set; }

    /// <summary>
    /// Z coordinate (Vertical). Backing field for EF Core.
    /// </summary>
    public int PositionZ { get; set; }

    /// <summary>
    /// Maps directions to connected room IDs.
    /// Not all directions need exits; blocked directions are simply absent.
    /// </summary>
    public Dictionary<Direction, Guid> Exits { get; set; } = new();

    /// <summary>
    /// Indicates if this room is the starting location for new games.
    /// </summary>
    public bool IsStartingRoom { get; set; } = false;

    /// <summary>
    /// Gets or sets the biome type affecting atmosphere and loot tables.
    /// </summary>
    public BiomeType BiomeType { get; set; } = BiomeType.Ruin;

    /// <summary>
    /// Gets or sets the danger level affecting encounter and loot quality.
    /// </summary>
    public DangerLevel DangerLevel { get; set; } = DangerLevel.Safe;

    /// <summary>
    /// Features present in this room that enable specific gameplay mechanics.
    /// Examples: RunicAnchor (Sanctuary rest), Workbench (crafting).
    /// </summary>
    public List<RoomFeature> Features { get; set; } = new();

    /// <summary>
    /// Dynamic hazards present in this room (v0.3.3a).
    /// Loaded lazily via repository or navigation property.
    /// </summary>
    public List<DynamicHazard> Hazards { get; set; } = new();

    /// <summary>
    /// NPCs present in this room (v0.4.2e).
    /// </summary>
    public ICollection<Npc> Npcs { get; set; } = new List<Npc>();

    /// <summary>
    /// The ambient condition affecting this room, if any (v0.3.3b).
    /// Null means no active condition.
    /// </summary>
    public Guid? ConditionId { get; set; }

    /// <summary>
    /// Checks if this room has a specific feature.
    /// </summary>
    /// <param name="feature">The feature to check for.</param>
    /// <returns>True if the room has the feature; false otherwise.</returns>
    public bool HasFeature(RoomFeature feature) => Features.Contains(feature);
}
