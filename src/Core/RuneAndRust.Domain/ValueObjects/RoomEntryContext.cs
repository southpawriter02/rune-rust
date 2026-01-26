namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Contains all context information for processing a room entry event.
/// Passed through the room entry perception flow to provide necessary data.
/// </summary>
public sealed record RoomEntryContext
{
    /// <summary>
    /// The character entering the room.
    /// </summary>
    public required string CharacterId { get; init; }

    /// <summary>
    /// The room being entered.
    /// </summary>
    public required string RoomId { get; init; }

    /// <summary>
    /// The character's calculated passive perception (including all modifiers).
    /// </summary>
    public required int PassivePerceptionValue { get; init; }

    /// <summary>
    /// The room's biome for flora/fauna generation.
    /// </summary>
    public required string Biome { get; init; }

    /// <summary>
    /// Whether this is the first time the character has entered this room.
    /// </summary>
    public bool IsFirstVisit { get; init; }

    /// <summary>
    /// The character's X coordinate entry position within the room.
    /// </summary>
    public int? EntryPositionX { get; init; }

    /// <summary>
    /// The character's Y coordinate entry position within the room.
    /// </summary>
    public int? EntryPositionY { get; init; }

    /// <summary>
    /// Gets whether a position is specified.
    /// </summary>
    public bool HasEntryPosition => EntryPositionX.HasValue && EntryPositionY.HasValue;

    /// <summary>
    /// Creates a room entry context for testing.
    /// </summary>
    public static RoomEntryContext Create(
        string characterId,
        string roomId,
        int passivePerception,
        string biome,
        bool isFirstVisit = true) =>
        new()
        {
            CharacterId = characterId,
            RoomId = roomId,
            PassivePerceptionValue = passivePerception,
            Biome = biome,
            IsFirstVisit = isFirstVisit
        };
}
