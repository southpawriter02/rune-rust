namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides investigation functionality for discovering clues and forming
/// deductions from crime scenes, remains, wreckage, trails, and documents.
/// </summary>
/// <remarks>
/// <para>
/// The investigation service manages the analytical examination of targets
/// to uncover clues. Unlike examination (which reveals descriptions) or
/// searching (which finds hidden elements), investigation focuses on
/// piecing together evidence to understand what happened.
/// </para>
/// <para>
/// Investigations use WITS + Investigation skill for checks.
/// Expert successes (net >= 5) reveal hidden connections.
/// </para>
/// </remarks>
public interface IInvestigationService
{
    /// <summary>
    /// Investigates a target to discover clues and form deductions.
    /// </summary>
    /// <param name="characterId">The character performing the investigation.</param>
    /// <param name="targetId">The target to investigate.</param>
    /// <param name="targetType">The type of target being investigated.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Complete investigation results including clues and deductions.</returns>
    Task<InvestigationResult> InvestigateAsync(
        string characterId,
        string targetId,
        InvestigationTarget targetType,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the current investigation context for a character and target.
    /// Returns null if no investigation is in progress.
    /// </summary>
    /// <param name="characterId">The character to check.</param>
    /// <param name="targetId">The target to check.</param>
    /// <returns>Current investigation context or null.</returns>
    InvestigationContext? GetActiveInvestigation(string characterId, string targetId);

    /// <summary>
    /// Gets all clues a character has discovered across all investigations.
    /// </summary>
    /// <param name="characterId">The character to query.</param>
    /// <returns>All discovered clues for this character.</returns>
    IReadOnlyList<Clue> GetDiscoveredClues(string characterId);

    /// <summary>
    /// Gets all deductions a character has made across all investigations.
    /// </summary>
    /// <param name="characterId">The character to query.</param>
    /// <returns>All deductions made by this character.</returns>
    IReadOnlyList<Deduction> GetMadeDeductions(string characterId);

    /// <summary>
    /// Checks what deductions are now available based on gathered clues.
    /// </summary>
    /// <param name="characterId">The character to check.</param>
    /// <returns>Deductions that can now be made.</returns>
    IReadOnlyList<Deduction> GetAvailableDeductions(string characterId);

    /// <summary>
    /// Attempts to make a specific deduction if all required clues are present.
    /// </summary>
    /// <param name="characterId">The character making the deduction.</param>
    /// <param name="deductionId">The deduction to attempt.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if deduction was successful, false if missing clues.</returns>
    Task<bool> AttemptDeductionAsync(string characterId, string deductionId, CancellationToken ct = default);

    /// <summary>
    /// Gets all investigable targets in the current room.
    /// </summary>
    /// <param name="roomId">The room to check.</param>
    /// <returns>List of investigable targets with their types.</returns>
    IReadOnlyList<(string TargetId, InvestigationTarget Type, string Name)> GetInvestigableTargets(string roomId);
}
