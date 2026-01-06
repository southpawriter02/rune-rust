using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.DTOs;

public record ItemDto(
    Guid Id,
    string Name,
    string Description,
    ItemType Type,
    int Value
);
