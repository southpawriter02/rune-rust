using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.DTOs;

public record GameStateDto(
    Guid SessionId,
    PlayerDto Player,
    RoomDto CurrentRoom,
    GameState State,
    DateTime LastPlayedAt
);
