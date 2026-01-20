using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Data transfer object representing a room in the dungeon for display.
/// </summary>
/// <param name="Id">The unique identifier of the room.</param>
/// <param name="Name">The room's display name.</param>
/// <param name="Description">A narrative description of the room.</param>
/// <param name="Exits">The available exit directions from this room.</param>
/// <param name="Items">The items present in this room.</param>
/// <param name="Monsters">The monsters present in this room.</param>
/// <param name="IsFirstVisit">Indicates whether this is the player's first time visiting this room.</param>
public record RoomDto(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyList<Direction> Exits,
    IReadOnlyList<ItemDto> Items,
    IReadOnlyList<MonsterDto> Monsters,
    bool IsFirstVisit = true
);
