using RuneAndRust.Application.DTOs;

namespace RuneAndRust.Application.Interfaces;

public enum MessageType
{
    Info,
    Warning,
    Error,
    Success,
    Combat,
    Narrative
}

public interface IGameRenderer
{
    Task RenderGameStateAsync(GameStateDto gameState, CancellationToken ct = default);
    Task RenderMessageAsync(string message, MessageType type = MessageType.Info, CancellationToken ct = default);
    Task RenderRoomAsync(RoomDto room, CancellationToken ct = default);
    Task RenderInventoryAsync(InventoryDto inventory, CancellationToken ct = default);
    Task RenderCombatResultAsync(string combatDescription, CancellationToken ct = default);
    Task ClearScreenAsync(CancellationToken ct = default);
}
