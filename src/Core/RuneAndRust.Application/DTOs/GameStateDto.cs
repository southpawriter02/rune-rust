using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Data transfer object representing the complete state of an active game session.
/// </summary>
/// <param name="SessionId">The unique identifier of the game session.</param>
/// <param name="Player">The player character's current state.</param>
/// <param name="CurrentRoom">The room the player is currently in.</param>
/// <param name="State">The current game state (Playing, GameOver, Victory, etc.).</param>
/// <param name="LastPlayedAt">The UTC timestamp when the session was last played.</param>
public record GameStateDto(
    Guid SessionId,
    PlayerDto Player,
    RoomDto CurrentRoom,
    GameState State,
    DateTime LastPlayedAt
);
