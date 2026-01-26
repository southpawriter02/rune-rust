using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of a passive perception check on room entry.
/// </summary>
/// <remarks>
/// <para>
/// Contains the revealed elements with full details, but only a count
/// of missed elements to avoid revealing what the character didn't notice.
/// </para>
/// <para>
/// This separation ensures players don't know exactly what they're missing,
/// maintaining the mystery of undiscovered content.
/// </para>
/// </remarks>
/// <param name="RoomId">The ID of the room that was checked.</param>
/// <param name="PassiveValue">The passive perception value used for the check.</param>
/// <param name="ElementsChecked">Total number of hidden elements in the room.</param>
/// <param name="ElementsRevealed">List of elements that were discovered.</param>
/// <param name="ElementsMissed">Count of elements not discovered (no details).</param>
public readonly record struct PassivePerceptionResult(
    string RoomId,
    int PassiveValue,
    int ElementsChecked,
    IReadOnlyList<HiddenElement> ElementsRevealed,
    int ElementsMissed)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DERIVED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether any elements were revealed.
    /// </summary>
    public bool HasDiscoveries => ElementsRevealed.Count > 0;

    /// <summary>
    /// Gets whether any elements remain hidden.
    /// </summary>
    public bool HasMissedElements => ElementsMissed > 0;

    /// <summary>
    /// Gets whether all hidden elements were revealed.
    /// </summary>
    public bool AllRevealed => ElementsMissed == 0 && ElementsRevealed.Count > 0;

    /// <summary>
    /// Gets whether the room had no hidden elements.
    /// </summary>
    public bool NoHiddenElements => ElementsChecked == 0;

    /// <summary>
    /// Gets the count of revealed elements.
    /// </summary>
    public int RevealedCount => ElementsRevealed.Count;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates an empty result for a room with no hidden elements.
    /// </summary>
    /// <param name="roomId">The room ID.</param>
    /// <param name="passiveValue">The passive perception value used.</param>
    /// <returns>A result indicating no hidden elements existed.</returns>
    public static PassivePerceptionResult Empty(string roomId, int passiveValue) =>
        new(roomId, passiveValue, 0, Array.Empty<HiddenElement>(), 0);

    /// <summary>
    /// Creates a result from the check operation.
    /// </summary>
    /// <param name="roomId">The room ID.</param>
    /// <param name="passiveValue">The passive perception value used.</param>
    /// <param name="revealed">Elements that were revealed.</param>
    /// <param name="totalChecked">Total elements checked.</param>
    /// <returns>A new PassivePerceptionResult.</returns>
    public static PassivePerceptionResult Create(
        string roomId,
        int passiveValue,
        IReadOnlyList<HiddenElement> revealed,
        int totalChecked)
    {
        var missed = totalChecked - revealed.Count;
        return new PassivePerceptionResult(
            roomId,
            passiveValue,
            totalChecked,
            revealed,
            missed);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string summarizing the perception result.
    /// </summary>
    /// <returns>A formatted string showing discovery summary.</returns>
    public string ToDisplayString()
    {
        if (NoHiddenElements)
        {
            return $"Room '{RoomId}': No hidden elements present.";
        }

        if (AllRevealed)
        {
            return $"Room '{RoomId}': Discovered all {RevealedCount} hidden element(s) (Passive {PassiveValue}).";
        }

        if (HasDiscoveries)
        {
            return $"Room '{RoomId}': Discovered {RevealedCount} of {ElementsChecked} hidden element(s) (Passive {PassiveValue}).";
        }

        return $"Room '{RoomId}': Nothing discovered (Passive {PassiveValue}).";
    }
}
