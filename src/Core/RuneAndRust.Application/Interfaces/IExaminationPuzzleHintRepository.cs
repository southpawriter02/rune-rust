namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Repository interface for loading examination puzzle hints from configuration.
/// </summary>
/// <remarks>
/// This repository provides access to puzzle hints that are revealed through
/// the examination system. It is separate from the riddle hint system.
/// </remarks>
public interface IExaminationPuzzleHintRepository
{
    /// <summary>
    /// Gets a hint by its unique ID.
    /// </summary>
    /// <param name="hintId">The hint ID.</param>
    /// <returns>The hint, or null if not found.</returns>
    ExaminationPuzzleHint? GetById(string hintId);

    /// <summary>
    /// Gets all hints for a specific puzzle.
    /// </summary>
    /// <param name="puzzleId">The puzzle ID.</param>
    /// <returns>All hints for this puzzle.</returns>
    IReadOnlyList<ExaminationPuzzleHint> GetByPuzzleId(string puzzleId);

    /// <summary>
    /// Gets all hints revealed by examination.
    /// </summary>
    /// <returns>All hints with "examination" as reveal method.</returns>
    IReadOnlyList<ExaminationPuzzleHint> GetExaminationHints();

    /// <summary>
    /// Gets all hints in the system.
    /// </summary>
    /// <returns>All configured hints.</returns>
    IReadOnlyList<ExaminationPuzzleHint> GetAll();

    /// <summary>
    /// Gets the count of hints.
    /// </summary>
    int Count { get; }
}
