namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Repository interface for accessing deduction definitions.
/// </summary>
public interface IDeductionRepository
{
    /// <summary>
    /// Gets a deduction by its identifier.
    /// </summary>
    /// <param name="deductionId">The deduction identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The deduction if found, null otherwise.</returns>
    Task<Deduction?> GetByIdAsync(string deductionId, CancellationToken ct = default);

    /// <summary>
    /// Gets all deductions that can be made from a set of clues.
    /// </summary>
    /// <param name="clueIds">Collection of discovered clue identifiers.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Collection of deductions that can now be made.</returns>
    Task<IReadOnlyList<Deduction>> GetAvailableDeductionsAsync(
        IEnumerable<string> clueIds,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all deductions that require a specific clue.
    /// </summary>
    /// <param name="clueId">The clue identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Collection of deductions requiring this clue.</returns>
    Task<IReadOnlyList<Deduction>> GetDeductionsRequiringClueAsync(
        string clueId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all registered deductions.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>All deduction definitions.</returns>
    Task<IReadOnlyList<Deduction>> GetAllAsync(CancellationToken ct = default);
}
