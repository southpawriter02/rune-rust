// ═══════════════════════════════════════════════════════════════════════════════
// IStressHistoryRepository.cs
// Interface defining the contract for StressHistoryEntry persistence operations.
// Provides methods for adding stress event records and querying history by
// character ID with optional result limiting. Implementations may use in-memory
// storage (InMemoryStressHistoryRepository) or a database backend (future EF Core).
// Version: 0.18.0f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;

/// <summary>
/// Repository interface for <see cref="StressHistoryEntry"/> persistence operations.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IStressHistoryRepository"/> provides the persistence contract for stress
/// event history tracking. Every stress application event can be recorded as a
/// <see cref="StressHistoryEntry"/> for analytics, debugging, and UI display.
/// </para>
/// <para>
/// <strong>Recording Flow:</strong>
/// </para>
/// <list type="number">
///   <item><description><c>StressService.ApplyStress()</c> processes the stress event</description></item>
///   <item><description><c>StressHistoryEntry.FromApplicationResult()</c> creates the history record</description></item>
///   <item><description><c>IStressHistoryRepository.AddAsync()</c> persists the entry</description></item>
/// </list>
/// <para>
/// <strong>Query Support:</strong> Entries are returned ordered by <c>CreatedAt</c>
/// descending (most recent first), supporting both full history retrieval and
/// limited queries for UI display (e.g., "last 10 stress events").
/// </para>
/// <para>
/// <strong>Implementations:</strong>
/// </para>
/// <list type="bullet">
///   <item><description><c>InMemoryStressHistoryRepository</c> — Thread-safe in-memory storage using ConcurrentDictionary (current)</description></item>
///   <item><description>EF Core <c>StressHistoryRepository</c> — Database persistence with PostgreSQL (future)</description></item>
/// </list>
/// </remarks>
/// <seealso cref="StressHistoryEntry"/>
public interface IStressHistoryRepository
{
    /// <summary>
    /// Adds a new stress history entry to persistent storage.
    /// </summary>
    /// <param name="entry">
    /// The <see cref="StressHistoryEntry"/> to persist. Must not be null.
    /// The entry's <see cref="StressHistoryEntry.Id"/> is used as the unique identifier.
    /// </param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    /// <remarks>
    /// <para>
    /// Entries are append-only — once added, they cannot be modified or deleted
    /// through this interface. This ensures the integrity of the stress event audit trail.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = stressService.ApplyStress(characterId, 20, StressSource.Combat);
    /// var entry = StressHistoryEntry.FromApplicationResult(characterId, result);
    /// await repository.AddAsync(entry);
    /// </code>
    /// </example>
    Task AddAsync(StressHistoryEntry entry, CancellationToken ct = default);

    /// <summary>
    /// Gets all stress history entries for a character, ordered by most recent first.
    /// </summary>
    /// <param name="characterId">The unique identifier of the character to query.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>
    /// A read-only list of all <see cref="StressHistoryEntry"/> records for the specified
    /// character, ordered by <see cref="StressHistoryEntry.CreatedAt"/> descending
    /// (most recent first). Returns an empty list if no history exists for the character.
    /// </returns>
    /// <example>
    /// <code>
    /// var history = await repository.GetByCharacterIdAsync(characterId);
    /// foreach (var entry in history)
    ///     logger.LogDebug("Stress event: {Source} +{Amount} at {Time}",
    ///         entry.Source, entry.FinalAmount, entry.CreatedAt);
    /// </code>
    /// </example>
    Task<IReadOnlyList<StressHistoryEntry>> GetByCharacterIdAsync(
        Guid characterId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the most recent stress history entries for a character, up to a specified limit.
    /// </summary>
    /// <param name="characterId">The unique identifier of the character to query.</param>
    /// <param name="limit">
    /// The maximum number of entries to return. Must be greater than zero.
    /// </param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>
    /// A read-only list of the most recent <see cref="StressHistoryEntry"/> records
    /// for the specified character, ordered by <see cref="StressHistoryEntry.CreatedAt"/>
    /// descending (most recent first), containing at most <paramref name="limit"/> entries.
    /// Returns an empty list if no history exists for the character.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Use this overload for UI display where only the most recent events are needed
    /// (e.g., a stress activity feed showing the last 10 events). For full history
    /// retrieval, use the overload without the <paramref name="limit"/> parameter.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get last 5 stress events for the character
    /// var recentHistory = await repository.GetByCharacterIdAsync(characterId, limit: 5);
    /// </code>
    /// </example>
    Task<IReadOnlyList<StressHistoryEntry>> GetByCharacterIdAsync(
        Guid characterId,
        int limit,
        CancellationToken ct = default);
}
