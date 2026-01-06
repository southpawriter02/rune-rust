using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.DTOs;

public record RoomDto(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyList<Direction> Exits,
    IReadOnlyList<ItemDto> Items,
    IReadOnlyList<MonsterDto> Monsters
);
