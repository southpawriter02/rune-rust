namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tracks a hint that has been discovered by the player, including
/// when and where it was found for puzzle progress tracking.
/// </summary>
/// <remarks>
/// Discovery tracking enables the game to:
/// 1. Prevent re-displaying already-discovered hints
/// 2. Track puzzle progress based on hints found
/// 3. Satisfy reveal conditions for dependent hints
/// 4. Maintain unlocked interactions across sessions
/// </remarks>
/// <param name="HintId">The discovered hint ID.</param>
/// <param name="PuzzleId">The puzzle this hint relates to.</param>
/// <param name="DiscoveredAt">When the hint was discovered.</param>
/// <param name="DiscoveredInRoom">The room where discovery occurred.</param>
/// <param name="DiscoveredFromObject">The object examined to reveal this hint.</param>
public readonly record struct DiscoveredHint(
    string HintId,
    string PuzzleId,
    DateTime DiscoveredAt,
    string DiscoveredInRoom,
    string DiscoveredFromObject)
{
    /// <summary>
    /// Gets how long ago this hint was discovered.
    /// </summary>
    public TimeSpan TimeSinceDiscovery => DateTime.UtcNow - DiscoveredAt;

    /// <summary>
    /// Gets whether this discovery was recent (within last hour).
    /// </summary>
    public bool IsRecentDiscovery => TimeSinceDiscovery.TotalHours < 1;

    /// <summary>
    /// Creates a journal-style entry for this discovery.
    /// </summary>
    /// <returns>A formatted string for display in hint logs.</returns>
    public string ToJournalEntry() =>
        $"[{DiscoveredAt:HH:mm}] Discovered in {DiscoveredInRoom} (from {DiscoveredFromObject})";

    /// <summary>
    /// Gets a summary for logging.
    /// </summary>
    public override string ToString() =>
        $"DiscoveredHint({HintId}, {PuzzleId}, at {DiscoveredAt:yyyy-MM-dd HH:mm})";
}
