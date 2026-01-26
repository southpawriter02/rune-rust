namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a room description with perception-gated content.
/// Contains a base description always shown plus fragments revealed
/// based on the viewer's passive perception value.
/// </summary>
public sealed record PerceptionGatedDescription
{
    /// <summary>
    /// Unique identifier of the room this description belongs to.
    /// </summary>
    public required string RoomId { get; init; }

    /// <summary>
    /// The base description that is always shown regardless of perception.
    /// This provides the fundamental room atmosphere and layout.
    /// </summary>
    public required string BaseDescription { get; init; }

    /// <summary>
    /// Perception-gated description elements that may or may not be revealed
    /// based on the viewer's passive perception value.
    /// </summary>
    public required IReadOnlyList<GatedElement> GatedElements { get; init; }

    /// <summary>
    /// Gets the count of all gated elements.
    /// </summary>
    public int TotalGatedElements => GatedElements.Count;

    /// <summary>
    /// Gets the count of revealed elements.
    /// </summary>
    public int RevealedCount => GatedElements.Count(e => e.IsRevealed);

    /// <summary>
    /// Gets whether any elements are revealed.
    /// </summary>
    public bool HasRevealedElements => GatedElements.Any(e => e.IsRevealed);

    /// <summary>
    /// Creates a new PerceptionGatedDescription with the specified elements.
    /// </summary>
    public static PerceptionGatedDescription Create(
        string roomId,
        string baseDescription,
        IReadOnlyList<GatedElement> gatedElements) =>
        new()
        {
            RoomId = roomId,
            BaseDescription = baseDescription,
            GatedElements = gatedElements
        };

    /// <summary>
    /// Evaluates which gated elements are revealed for the given perception value.
    /// </summary>
    /// <param name="passivePerception">The viewer's effective passive perception.</param>
    /// <returns>A new instance with IsRevealed flags updated.</returns>
    public PerceptionGatedDescription EvaluateForPerception(int passivePerception)
    {
        var evaluatedElements = GatedElements
            .Select(e => e with { IsRevealed = passivePerception >= e.RequiredDc })
            .ToList();

        return this with { GatedElements = evaluatedElements };
    }

    /// <summary>
    /// Gets revealed elements ordered by priority.
    /// </summary>
    public IReadOnlyList<GatedElement> GetRevealedByPriority() =>
        GatedElements
            .Where(e => e.IsRevealed)
            .OrderBy(e => e.Priority)
            .ToList();
}
