// ═══════════════════════════════════════════════════════════════════════════════
// InMemoryStressHistoryRepository.cs
// In-memory implementation of IStressHistoryRepository for development, testing,
// and scenarios where database persistence is not yet configured. Stores
// StressHistoryEntry entities in a thread-safe ConcurrentDictionary keyed by
// character ID. Entries are returned ordered by CreatedAt descending (most
// recent first). Data is lost when the application stops.
// Version: 0.18.0f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Infrastructure.Repositories;

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;

/// <summary>
/// In-memory implementation of <see cref="IStressHistoryRepository"/> for development and testing.
/// </summary>
/// <remarks>
/// <para>
/// This repository stores <see cref="StressHistoryEntry"/> entities in memory using a thread-safe
/// <see cref="ConcurrentDictionary{TKey,TValue}"/> keyed by character ID. Each character's entries
/// are stored in a list that is locked during write operations to ensure thread safety.
/// Data is lost when the application stops.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> Character-level entry lists are protected by <c>lock</c>
/// statements during write operations. The outer <see cref="ConcurrentDictionary{TKey,TValue}"/>
/// provides thread-safe key-level access. Read operations snapshot the list contents under lock
/// to prevent enumeration issues.
/// </para>
/// <para>
/// <strong>Ordering:</strong> Entries are returned ordered by <see cref="StressHistoryEntry.CreatedAt"/>
/// descending (most recent first), consistent with the expected query pattern for stress activity feeds.
/// </para>
/// <para>
/// <strong>Future:</strong> This implementation will be replaced by an EF Core-based
/// <c>StressHistoryRepository</c> when full database persistence is implemented.
/// </para>
/// </remarks>
/// <seealso cref="IStressHistoryRepository"/>
/// <seealso cref="StressHistoryEntry"/>
/// <seealso cref="InMemoryPlayerRepository"/>
public class InMemoryStressHistoryRepository : IStressHistoryRepository
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Thread-safe storage for stress history entries, keyed by character ID.
    /// Each character's entries are stored in a list that is locked during writes.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, List<StressHistoryEntry>> _entries = new();

    /// <summary>
    /// Logger for repository operations and diagnostics.
    /// </summary>
    private readonly ILogger<InMemoryStressHistoryRepository> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new in-memory stress history repository instance.
    /// </summary>
    /// <param name="logger">
    /// Optional logger for diagnostics. If null, a no-op logger is used.
    /// </param>
    public InMemoryStressHistoryRepository(ILogger<InMemoryStressHistoryRepository>? logger = null)
    {
        _logger = logger ?? NullLogger<InMemoryStressHistoryRepository>.Instance;
        _logger.LogDebug("InMemoryStressHistoryRepository initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IStressHistoryRepository IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public Task AddAsync(StressHistoryEntry entry, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        // Get or create the entry list for this character
        var characterEntries = _entries.GetOrAdd(entry.CharacterId, _ => new List<StressHistoryEntry>());

        // Lock the list to ensure thread-safe addition
        lock (characterEntries)
        {
            characterEntries.Add(entry);
        }

        _logger.LogDebug(
            "Stress history entry added for character {CharacterId}: {Source} {Amount}→{FinalAmount} ({PreviousStress}→{NewStress}). " +
            "Entry ID: {EntryId}. Total entries for character: {EntryCount}",
            entry.CharacterId,
            entry.Source,
            entry.Amount,
            entry.FinalAmount,
            entry.PreviousStress,
            entry.NewStress,
            entry.Id,
            characterEntries.Count);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<StressHistoryEntry>> GetByCharacterIdAsync(
        Guid characterId,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "Querying all stress history for character {CharacterId}",
            characterId);

        if (!_entries.TryGetValue(characterId, out var characterEntries))
        {
            _logger.LogDebug(
                "No stress history found for character {CharacterId}",
                characterId);
            return Task.FromResult<IReadOnlyList<StressHistoryEntry>>(Array.Empty<StressHistoryEntry>());
        }

        // Snapshot the list under lock and order by most recent first
        List<StressHistoryEntry> snapshot;
        lock (characterEntries)
        {
            snapshot = characterEntries
                .OrderByDescending(e => e.CreatedAt)
                .ToList();
        }

        _logger.LogDebug(
            "Retrieved {Count} stress history entries for character {CharacterId}",
            snapshot.Count,
            characterId);

        return Task.FromResult<IReadOnlyList<StressHistoryEntry>>(snapshot);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<StressHistoryEntry>> GetByCharacterIdAsync(
        Guid characterId,
        int limit,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        _logger.LogDebug(
            "Querying stress history for character {CharacterId} with limit {Limit}",
            characterId,
            limit);

        if (!_entries.TryGetValue(characterId, out var characterEntries))
        {
            _logger.LogDebug(
                "No stress history found for character {CharacterId}",
                characterId);
            return Task.FromResult<IReadOnlyList<StressHistoryEntry>>(Array.Empty<StressHistoryEntry>());
        }

        // Snapshot the list under lock, order by most recent first, and apply limit
        List<StressHistoryEntry> snapshot;
        lock (characterEntries)
        {
            snapshot = characterEntries
                .OrderByDescending(e => e.CreatedAt)
                .Take(limit)
                .ToList();
        }

        _logger.LogDebug(
            "Retrieved {Count} stress history entries for character {CharacterId} (limit: {Limit})",
            snapshot.Count,
            characterId,
            limit);

        return Task.FromResult<IReadOnlyList<StressHistoryEntry>>(snapshot);
    }
}
