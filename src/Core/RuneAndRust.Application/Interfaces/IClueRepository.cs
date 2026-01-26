namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Repository interface for accessing clue definitions.
/// </summary>
public interface IClueRepository
{
    /// <summary>
    /// Gets a clue by its identifier.
    /// </summary>
    /// <param name="clueId">The clue identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The clue if found, null otherwise.</returns>
    Task<Clue?> GetByIdAsync(string clueId, CancellationToken ct = default);

    /// <summary>
    /// Gets all clues associated with a specific target.
    /// </summary>
    /// <param name="targetId">The target identifier.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Collection of clues for the target.</returns>
    Task<IReadOnlyList<Clue>> GetCluesForTargetAsync(
        string targetId,
        InvestigationTarget targetType,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all clues in a specific category.
    /// </summary>
    /// <param name="category">The clue category.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Collection of clues in the category.</returns>
    Task<IReadOnlyList<Clue>> GetByCategoryAsync(ClueCategory category, CancellationToken ct = default);

    /// <summary>
    /// Gets all clues with a specific tag.
    /// </summary>
    /// <param name="tag">The tag to filter by.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Collection of clues with the tag.</returns>
    Task<IReadOnlyList<Clue>> GetByTagAsync(string tag, CancellationToken ct = default);

    /// <summary>
    /// Gets multiple clues by their identifiers.
    /// </summary>
    /// <param name="clueIds">Collection of clue identifiers.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Collection of found clues.</returns>
    Task<IReadOnlyList<Clue>> GetByIdsAsync(IEnumerable<string> clueIds, CancellationToken ct = default);
}
