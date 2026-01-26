namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Processes perception checks and description composition on room entry.
/// Integrates passive perception, hidden element detection, flora/fauna
/// observation, and gated descriptions into a unified room entry experience.
/// </summary>
public interface IRoomEntryPerceptionService
{
    /// <summary>
    /// Processes room entry for a character, performing all perception checks
    /// and composing the final room description.
    /// </summary>
    /// <param name="characterId">The character entering the room.</param>
    /// <param name="roomId">The room being entered.</param>
    /// <param name="entryPositionX">Optional entry X position for proximity checks.</param>
    /// <param name="entryPositionY">Optional entry Y position for proximity checks.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The complete perception result including composed description.</returns>
    Task<RoomEntryPerceptionResult> ProcessRoomEntryAsync(
        string characterId,
        string roomId,
        int? entryPositionX = null,
        int? entryPositionY = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the perception-gated description for a room without processing
    /// a full room entry (useful for look command).
    /// </summary>
    /// <param name="characterId">The viewing character.</param>
    /// <param name="roomId">The room to describe.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The evaluated gated description.</returns>
    Task<PerceptionGatedDescription> GetRoomDescriptionAsync(
        string characterId,
        string roomId,
        CancellationToken ct = default);

    /// <summary>
    /// Recalculates revealed elements for a room when character perception changes.
    /// Used when status effects or abilities modify passive perception.
    /// </summary>
    /// <param name="characterId">The character to recalculate for.</param>
    /// <param name="roomId">The room to recalculate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Any newly revealed elements.</returns>
    Task<IReadOnlyList<RevealedElement>> RecalculateRoomPerceptionAsync(
        string characterId,
        string roomId,
        CancellationToken ct = default);
}
