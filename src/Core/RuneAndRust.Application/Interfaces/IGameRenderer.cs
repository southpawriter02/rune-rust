using RuneAndRust.Application.DTOs;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Categorizes messages for appropriate visual styling in the renderer.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// General informational messages (default styling).
    /// </summary>
    Info,

    /// <summary>
    /// Warning messages that require attention but are not errors.
    /// </summary>
    Warning,

    /// <summary>
    /// Error messages indicating something went wrong.
    /// </summary>
    Error,

    /// <summary>
    /// Success messages confirming a positive outcome.
    /// </summary>
    Success,

    /// <summary>
    /// Combat-related messages describing attacks and damage.
    /// </summary>
    Combat,

    /// <summary>
    /// Story/narrative messages for immersion and atmosphere.
    /// </summary>
    Narrative
}

/// <summary>
/// Defines the contract for rendering game output to the user interface.
/// </summary>
/// <remarks>
/// Implementations of this interface handle displaying game state, messages,
/// and other visual feedback. This abstraction allows for different UI implementations
/// (console, TUI, GUI) while maintaining consistent game logic.
/// </remarks>
public interface IGameRenderer
{
    /// <summary>
    /// Renders the complete game state including player info, room, and status.
    /// </summary>
    /// <param name="gameState">The current game state to render.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    Task RenderGameStateAsync(GameStateDto gameState, CancellationToken ct = default);

    /// <summary>
    /// Renders a message with optional styling based on message type.
    /// </summary>
    /// <param name="message">The message text to display.</param>
    /// <param name="type">The type of message for styling purposes.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    Task RenderMessageAsync(string message, MessageType type = MessageType.Info, CancellationToken ct = default);

    /// <summary>
    /// Renders detailed room information including description, exits, items, and monsters.
    /// </summary>
    /// <param name="room">The room data to render.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    Task RenderRoomAsync(RoomDto room, CancellationToken ct = default);

    /// <summary>
    /// Renders the player's inventory contents and capacity.
    /// </summary>
    /// <param name="inventory">The inventory data to render.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    Task RenderInventoryAsync(InventoryDto inventory, CancellationToken ct = default);

    /// <summary>
    /// Renders combat result text with appropriate combat styling.
    /// </summary>
    /// <param name="combatDescription">The combat description to display.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    Task RenderCombatResultAsync(string combatDescription, CancellationToken ct = default);

    /// <summary>
    /// Renders detailed examination information about a target.
    /// </summary>
    /// <param name="result">The examination result to display.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    Task RenderExamineResultAsync(ExamineResultDto result, CancellationToken ct = default);

    /// <summary>
    /// Renders comprehensive player statistics.
    /// </summary>
    /// <param name="stats">The player statistics to display.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    Task RenderPlayerStatsAsync(PlayerStatsDto stats, CancellationToken ct = default);

    /// <summary>
    /// Clears the screen or display area.
    /// </summary>
    /// <param name="ct">Cancellation token for async operation.</param>
    Task ClearScreenAsync(CancellationToken ct = default);
}
