namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Parameters controlling how a search operation is performed.
/// </summary>
/// <remarks>
/// SearchParameters define the scope and behavior of an active search,
/// including whether to apply the active perception bonus and whether
/// to include containers in the search.
/// </remarks>
public sealed record SearchParameters
{
    /// <summary>
    /// The character performing the search.
    /// </summary>
    public required string CharacterId { get; init; }

    /// <summary>
    /// The type of target being searched.
    /// </summary>
    public required SearchTarget TargetType { get; init; }

    /// <summary>
    /// The identifier of the target (room, container, or object).
    /// </summary>
    public required string TargetId { get; init; }

    /// <summary>
    /// Whether to include containers in the room search.
    /// </summary>
    /// <remarks>
    /// Defaults to true for room searches. Set to false to only search
    /// the room itself without opening containers.
    /// </remarks>
    public bool IncludeContainers { get; init; } = true;

    /// <summary>
    /// Whether to apply the active search bonus (+2).
    /// </summary>
    /// <remarks>
    /// Always true for standard search commands. May be false for
    /// quick searches or special circumstances.
    /// </remarks>
    public bool ApplyActiveBonus { get; init; } = true;

    /// <summary>
    /// Gets whether this is a room-wide search.
    /// </summary>
    public bool IsRoomSearch => TargetType == SearchTarget.Room;

    /// <summary>
    /// Gets whether this is a container search.
    /// </summary>
    public bool IsContainerSearch => TargetType == SearchTarget.Container;

    /// <summary>
    /// Creates room search parameters.
    /// </summary>
    public static SearchParameters ForRoom(string characterId, string roomId, bool includeContainers = true) =>
        new()
        {
            CharacterId = characterId,
            TargetType = SearchTarget.Room,
            TargetId = roomId,
            IncludeContainers = includeContainers,
            ApplyActiveBonus = true
        };

    /// <summary>
    /// Creates container search parameters.
    /// </summary>
    public static SearchParameters ForContainer(string characterId, string containerId) =>
        new()
        {
            CharacterId = characterId,
            TargetType = SearchTarget.Container,
            TargetId = containerId,
            IncludeContainers = false,
            ApplyActiveBonus = true
        };
}
