namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the comprehensive results of an active search operation,
/// including all items found, hidden elements revealed, and containers searched.
/// </summary>
/// <remarks>
/// <para>
/// ActiveSearchResult encapsulates the complete outcome of a search command,
/// providing detailed information about what was discovered, where it was
/// found, and how long the search took.
/// </para>
/// <para>
/// The WitsCheckResult represents the total successes from the perception
/// check, including the +2 active search bonus.
/// </para>
/// </remarks>
public sealed record ActiveSearchResult
{
    /// <summary>
    /// The unique identifier of the room that was searched.
    /// </summary>
    public required string RoomId { get; init; }

    /// <summary>
    /// The unique identifier of the character performing the search.
    /// </summary>
    public required string CharacterId { get; init; }

    /// <summary>
    /// The total number of successes from the Wits check (including +2 bonus).
    /// </summary>
    /// <remarks>
    /// This value includes:
    /// - Base Wits dice pool successes (8-10 = success, 1 = botch)
    /// - Active search bonus (+2 to effective perception)
    /// - Any applicable modifiers (equipment, conditions, specializations)
    /// </remarks>
    public required int WitsCheckResult { get; init; }

    /// <summary>
    /// Collection of items discovered during the search.
    /// </summary>
    public required IReadOnlyList<FoundItem> ItemsFound { get; init; }

    /// <summary>
    /// Collection of hidden element IDs revealed by the search.
    /// </summary>
    /// <remarks>
    /// Hidden elements include secret doors, concealed compartments,
    /// hidden mechanisms, and other elements with discovery DCs.
    /// </remarks>
    public required IReadOnlyList<string> HiddenElementsRevealed { get; init; }

    /// <summary>
    /// Collection of container identifiers that were searched.
    /// </summary>
    public required IReadOnlyList<string> ContainersSearched { get; init; }

    /// <summary>
    /// The time spent searching, in minutes.
    /// </summary>
    /// <remarks>
    /// Time cost varies by room size:
    /// - Small room: 5 minutes
    /// - Medium room: 10 minutes
    /// - Large room: 15 minutes
    /// - XLarge room: 20 minutes
    /// </remarks>
    public required int TimeSpent { get; init; }

    /// <summary>
    /// Collection of puzzle hint IDs discovered during the search.
    /// </summary>
    public required IReadOnlyList<string> HintsDiscovered { get; init; }

    /// <summary>
    /// Indicates whether the search was completed or interrupted.
    /// </summary>
    public bool WasCompleted { get; init; } = true;

    /// <summary>
    /// Gets the total count of all discoveries.
    /// </summary>
    public int TotalDiscoveries =>
        ItemsFound.Count + HiddenElementsRevealed.Count + HintsDiscovered.Count;

    /// <summary>
    /// Gets whether the search yielded any discoveries.
    /// </summary>
    public bool HasDiscoveries => TotalDiscoveries > 0;

    /// <summary>
    /// Gets whether any items were found.
    /// </summary>
    public bool HasItems => ItemsFound.Count > 0;

    /// <summary>
    /// Gets whether any hidden elements were revealed.
    /// </summary>
    public bool HasHiddenElements => HiddenElementsRevealed.Count > 0;

    /// <summary>
    /// Gets whether any hints were discovered.
    /// </summary>
    public bool HasHints => HintsDiscovered.Count > 0;

    /// <summary>
    /// Creates an empty search result.
    /// </summary>
    public static ActiveSearchResult Empty(string roomId, string characterId, int witsResult, int timeSpent) =>
        new()
        {
            RoomId = roomId,
            CharacterId = characterId,
            WitsCheckResult = witsResult,
            ItemsFound = Array.Empty<FoundItem>(),
            HiddenElementsRevealed = Array.Empty<string>(),
            ContainersSearched = Array.Empty<string>(),
            TimeSpent = timeSpent,
            HintsDiscovered = Array.Empty<string>()
        };
}
