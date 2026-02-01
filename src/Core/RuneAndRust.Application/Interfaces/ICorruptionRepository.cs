// ═══════════════════════════════════════════════════════════════════════════════
// ICorruptionRepository.cs
// Interface defining the contract for CorruptionTracker persistence operations.
// Provides methods for retrieving, creating, and updating per-character corruption
// tracker entities. Implementations may use in-memory storage
// (InMemoryCorruptionRepository) or a database backend (future EF Core).
// Version: 0.18.1d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;

/// <summary>
/// Repository interface for <see cref="CorruptionTracker"/> persistence operations.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ICorruptionRepository"/> provides the persistence contract for per-character
/// corruption tracking. Each character has at most one <see cref="CorruptionTracker"/> entity,
/// identified by <see cref="CorruptionTracker.CharacterId"/>. The repository supports
/// creation, retrieval by character ID, and updates.
/// </para>
/// <para>
/// <strong>Lifecycle Flow:</strong>
/// </para>
/// <list type="number">
///   <item><description><c>CorruptionService.GetOrCreateTracker()</c> retrieves or creates a tracker</description></item>
///   <item><description>Service methods modify the tracker via domain methods (<c>AddCorruption</c>, <c>SetCorruption</c>)</description></item>
///   <item><description><c>ICorruptionRepository.UpdateAsync()</c> persists the modified tracker</description></item>
/// </list>
/// <para>
/// <strong>One-to-One Relationship:</strong> Each character ID maps to exactly one
/// <see cref="CorruptionTracker"/>. Attempting to add a second tracker for the same
/// character should be prevented by the service layer (via <c>GetOrCreateTracker</c>).
/// </para>
/// <para>
/// <strong>Implementations:</strong>
/// </para>
/// <list type="bullet">
///   <item><description><c>InMemoryCorruptionRepository</c> — Thread-safe in-memory storage (current, v0.18.1e)</description></item>
///   <item><description>EF Core <c>CorruptionRepository</c> — Database persistence with PostgreSQL (future)</description></item>
/// </list>
/// </remarks>
/// <seealso cref="CorruptionTracker"/>
public interface ICorruptionRepository
{
    /// <summary>
    /// Gets the corruption tracker for a specific character.
    /// </summary>
    /// <param name="characterId">
    /// The unique identifier of the character whose corruption tracker is being retrieved.
    /// </param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>
    /// The <see cref="CorruptionTracker"/> for the specified character, or <c>null</c>
    /// if no tracker has been created for this character yet. A null return indicates
    /// the character has never accumulated corruption and a new tracker should be created
    /// via <see cref="AddAsync"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is the primary retrieval method. The service layer wraps this with a
    /// <c>GetOrCreateTracker</c> pattern that creates a new tracker if none exists,
    /// ensuring all service methods can assume a tracker is available.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var tracker = await repository.GetByCharacterIdAsync(characterId);
    /// if (tracker is null)
    /// {
    ///     tracker = CorruptionTracker.Create(characterId);
    ///     await repository.AddAsync(tracker);
    /// }
    /// </code>
    /// </example>
    Task<CorruptionTracker?> GetByCharacterIdAsync(
        Guid characterId,
        CancellationToken ct = default);

    /// <summary>
    /// Adds a new corruption tracker to persistent storage.
    /// </summary>
    /// <param name="tracker">
    /// The <see cref="CorruptionTracker"/> to persist. Must not be null. Should be
    /// a newly created tracker (via <see cref="CorruptionTracker.Create"/>) that does
    /// not yet exist in the repository.
    /// </param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    /// <remarks>
    /// <para>
    /// This method should only be called for new trackers. For updating existing
    /// trackers, use <see cref="UpdateAsync"/>. The service layer's <c>GetOrCreateTracker</c>
    /// method handles this distinction automatically.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var tracker = CorruptionTracker.Create(characterId);
    /// await repository.AddAsync(tracker);
    /// </code>
    /// </example>
    Task AddAsync(CorruptionTracker tracker, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing corruption tracker in persistent storage.
    /// </summary>
    /// <param name="tracker">
    /// The <see cref="CorruptionTracker"/> to update. Must not be null and must already
    /// exist in the repository (previously added via <see cref="AddAsync"/>).
    /// </param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    /// <remarks>
    /// <para>
    /// Called after any corruption state change (AddCorruption, SetCorruption, transfer,
    /// removal, Terminal Error survival). The tracker entity maintains its identity
    /// via <see cref="CorruptionTracker.Id"/> — updates are applied in-place.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = tracker.AddCorruption(15, CorruptionSource.HereticalAbility);
    /// await repository.UpdateAsync(tracker);
    /// </code>
    /// </example>
    Task UpdateAsync(CorruptionTracker tracker, CancellationToken ct = default);
}
