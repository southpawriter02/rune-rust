namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Composes room descriptions by combining base descriptions, gated elements,
/// revealed hidden elements, flora/fauna observations, and specialization insights.
/// </summary>
public interface IRoomDescriptionComposer
{
    /// <summary>
    /// Composes a complete room description from all available perception data.
    /// </summary>
    /// <param name="context">The room entry context.</param>
    /// <param name="gatedDescription">The evaluated gated description.</param>
    /// <param name="revealedElements">Elements revealed by passive perception.</param>
    /// <param name="autoDetectedElements">Elements auto-detected by abilities.</param>
    /// <param name="activatedAbilityIds">Specialization abilities that activated.</param>
    /// <returns>The fully composed description text.</returns>
    string ComposeDescription(
        RoomEntryContext context,
        PerceptionGatedDescription gatedDescription,
        IReadOnlyList<RevealedElement> revealedElements,
        IReadOnlyList<AutoDetectedElement> autoDetectedElements,
        IReadOnlyList<string> activatedAbilityIds);

    /// <summary>
    /// Composes a brief description for subsequent room visits.
    /// </summary>
    /// <param name="roomId">The room identifier.</param>
    /// <param name="baseDescription">The room's base description.</param>
    /// <param name="knownElementIds">Element IDs the character has already discovered.</param>
    /// <returns>A brief description noting previously discovered elements.</returns>
    string ComposeRevisitDescription(
        string roomId,
        string baseDescription,
        IReadOnlyList<string> knownElementIds);
}
