namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Repository interface for loading examination-hint links from configuration.
/// </summary>
/// <remarks>
/// Links define which objects reveal which hints when examined at specific
/// layers. This enables the examination system to trigger hint discoveries.
/// </remarks>
public interface IExaminationHintLinkRepository
{
    /// <summary>
    /// Gets all hint links for a specific object.
    /// </summary>
    /// <param name="objectId">The object ID.</param>
    /// <returns>All links for this object.</returns>
    IReadOnlyList<ExaminationHintLink> GetLinksForObject(string objectId);

    /// <summary>
    /// Gets all links for a specific puzzle.
    /// </summary>
    /// <param name="puzzleId">The puzzle ID.</param>
    /// <returns>All links for this puzzle.</returns>
    IReadOnlyList<ExaminationHintLink> GetLinksForPuzzle(string puzzleId);

    /// <summary>
    /// Gets all links for a specific hint.
    /// </summary>
    /// <param name="hintId">The hint ID.</param>
    /// <returns>All links that reveal this hint.</returns>
    IReadOnlyList<ExaminationHintLink> GetLinksForHint(string hintId);

    /// <summary>
    /// Gets all configured links.
    /// </summary>
    /// <returns>All examination-hint links.</returns>
    IReadOnlyList<ExaminationHintLink> GetAll();

    /// <summary>
    /// Gets the count of links.
    /// </summary>
    int Count { get; }
}
