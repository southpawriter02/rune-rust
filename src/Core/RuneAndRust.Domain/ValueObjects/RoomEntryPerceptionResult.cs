namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Contains the complete result of processing room entry perception,
/// including the composed description and all discoveries made.
/// </summary>
public sealed record RoomEntryPerceptionResult
{
    /// <summary>
    /// The room that was entered.
    /// </summary>
    public required string RoomId { get; init; }

    /// <summary>
    /// The character who entered.
    /// </summary>
    public required string CharacterId { get; init; }

    /// <summary>
    /// The fully composed room description including all revealed elements.
    /// </summary>
    public required string ComposedDescription { get; init; }

    /// <summary>
    /// Hidden elements that were revealed by passive perception.
    /// </summary>
    public required IReadOnlyList<RevealedElement> RevealedElements { get; init; }

    /// <summary>
    /// Hidden elements that were auto-detected by specialization abilities.
    /// </summary>
    public required IReadOnlyList<AutoDetectedElement> AutoDetectedElements { get; init; }

    /// <summary>
    /// Gated description elements that were revealed.
    /// </summary>
    public required IReadOnlyList<GatedElement> RevealedGatedElements { get; init; }

    /// <summary>
    /// Specialization ability IDs that activated during room entry.
    /// </summary>
    public required IReadOnlyList<string> ActivatedAbilityIds { get; init; }

    /// <summary>
    /// The effective passive perception used for this check.
    /// </summary>
    public required int EffectivePassivePerception { get; init; }

    /// <summary>
    /// Whether any new discoveries were made on this entry.
    /// </summary>
    public bool HasNewDiscoveries =>
        RevealedElements.Count > 0 ||
        AutoDetectedElements.Count > 0 ||
        RevealedGatedElements.Count > 0;

    /// <summary>
    /// Gets the total number of discoveries made.
    /// </summary>
    public int TotalDiscoveries =>
        RevealedElements.Count +
        AutoDetectedElements.Count +
        RevealedGatedElements.Count;

    /// <summary>
    /// Creates a room entry result with no discoveries.
    /// </summary>
    public static RoomEntryPerceptionResult Empty(
        string roomId,
        string characterId,
        string baseDescription,
        int passivePerception) =>
        new()
        {
            RoomId = roomId,
            CharacterId = characterId,
            ComposedDescription = baseDescription,
            RevealedElements = Array.Empty<RevealedElement>(),
            AutoDetectedElements = Array.Empty<AutoDetectedElement>(),
            RevealedGatedElements = Array.Empty<GatedElement>(),
            ActivatedAbilityIds = Array.Empty<string>(),
            EffectivePassivePerception = passivePerception
        };
}
