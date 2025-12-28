using RuneAndRust.Core.Entities;
using RuneAndRust.Core.ValueObjects;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service contract for exporting explored maps as ASCII text files.
/// Generates normalized grid representations of visited rooms across depth levels.
/// </summary>
/// <remarks>See: SPEC-MAP-001 for Cartographer II (Map Export) design (v0.3.20b).</remarks>
public interface IMapExportService
{
    /// <summary>
    /// Exports the explored map as an ASCII text file.
    /// </summary>
    /// <param name="characterName">The character name for the header.</param>
    /// <param name="playerPosition">The player's current coordinate position.</param>
    /// <param name="visitedRoomIds">Set of room IDs the player has explored.</param>
    /// <param name="userNotes">User-defined notes keyed by room ID.</param>
    /// <returns>The full file path where the map was saved.</returns>
    Task<string> ExportMapAsync(
        string characterName,
        Coordinate playerPosition,
        HashSet<Guid> visitedRoomIds,
        Dictionary<Guid, string> userNotes);

    /// <summary>
    /// Generates the ASCII map content without saving to disk.
    /// Used for preview or testing purposes.
    /// </summary>
    /// <param name="characterName">The character name for the header.</param>
    /// <param name="playerPosition">The player's current coordinate position.</param>
    /// <param name="rooms">The rooms to include in the map.</param>
    /// <param name="userNotes">User-defined notes keyed by room ID.</param>
    /// <returns>The complete ASCII map as a string.</returns>
    string GenerateMapContent(
        string characterName,
        Coordinate playerPosition,
        IEnumerable<Room> rooms,
        Dictionary<Guid, string> userNotes);

    /// <summary>
    /// Resolves the ASCII symbol for a room based on priority.
    /// </summary>
    /// <param name="room">The room to resolve (null for empty cell).</param>
    /// <param name="isPlayerPosition">Whether this is the player's current position.</param>
    /// <param name="hasNote">Whether this room has a user note.</param>
    /// <returns>The single ASCII character representing the room.</returns>
    char ResolveSymbol(Room? room, bool isPlayerPosition, bool hasNote);
}
