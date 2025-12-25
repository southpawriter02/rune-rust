using RuneAndRust.Core.Entities;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Populates rooms with biome-appropriate hazards and conditions (v0.3.3c).
/// Used during dungeon generation to assign environmental threats based on
/// room BiomeType and DangerLevel properties.
/// </summary>
/// <remarks>See: SPEC-ENVPOP-001 for Environment Population System design.</remarks>
public interface IEnvironmentPopulator
{
    /// <summary>
    /// Populates a room with hazards and conditions based on its BiomeType and DangerLevel.
    /// The room must have these properties already set.
    /// </summary>
    /// <param name="room">The room to populate.</param>
    /// <returns>The populated room (same instance, modified in place).</returns>
    Task<Room> PopulateRoomAsync(Room room);

    /// <summary>
    /// Populates all rooms in a collection with thematic hazards and conditions.
    /// Each room is processed based on its individual BiomeType and DangerLevel.
    /// </summary>
    /// <param name="rooms">The rooms to populate.</param>
    Task PopulateDungeonAsync(IEnumerable<Room> rooms);
}
