using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for rendering ASCII dungeon maps.
/// </summary>
public interface IMapRendererService
{
    /// <summary>
    /// Renders an ASCII map for the specified dungeon level.
    /// </summary>
    /// <param name="dungeon">The dungeon to render.</param>
    /// <param name="z">The Z level to render.</param>
    /// <param name="playerPosition">The player's current position.</param>
    /// <returns>A formatted ASCII map string.</returns>
    string RenderLevel(Dungeon dungeon, int z, Position3D playerPosition);

    /// <summary>
    /// Renders ASCII maps for all explored levels.
    /// </summary>
    /// <param name="dungeon">The dungeon to render.</param>
    /// <param name="playerPosition">The player's current position.</param>
    /// <returns>A formatted ASCII map string with all levels.</returns>
    string RenderAllLevels(Dungeon dungeon, Position3D playerPosition);

    /// <summary>
    /// Gets the display symbol for a room based on type and state.
    /// </summary>
    /// <param name="room">The room to get a symbol for.</param>
    /// <param name="isPlayerHere">Whether the player is in this room.</param>
    /// <returns>The character symbol for the room.</returns>
    char GetRoomSymbol(Room room, bool isPlayerHere);

    /// <summary>
    /// Generates the legend text explaining map symbols.
    /// </summary>
    /// <returns>Formatted legend string.</returns>
    string GetLegend();

    /// <summary>
    /// Gets the connection character for exits between rooms.
    /// </summary>
    /// <param name="direction">The direction of the connection.</param>
    /// <param name="isHidden">Whether the exit is hidden.</param>
    /// <returns>The character for the connection.</returns>
    char GetConnectionSymbol(Direction direction, bool isHidden);
}
