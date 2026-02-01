// ═══════════════════════════════════════════════════════════════════════════════
// InMemoryCorruptionRepository.cs
// In-memory implementation of ICorruptionRepository for development, testing,
// and scenarios where database persistence is not yet configured. Stores
// CorruptionTracker entities in a thread-safe ConcurrentDictionary keyed by
// character ID, and CorruptionHistoryEntry records in a separate dictionary
// keyed by character ID. History entries are returned ordered by CreatedAt
// descending (most recent first). Data is lost when the application stops.
// Version: 0.18.1e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Infrastructure.Repositories;

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;

/// <summary>
/// In-memory implementation of <see cref="ICorruptionRepository"/> for development and testing.
/// </summary>
/// <remarks>
/// <para>
/// This repository stores <see cref="CorruptionTracker"/> entities and <see cref="CorruptionHistoryEntry"/>
/// records in memory using thread-safe <see cref="ConcurrentDictionary{TKey,TValue}"/> collections.
/// Each character has at most one tracker (keyed by character ID) and zero or more history entries
/// (stored as a list per character ID). Data is lost when the application stops.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> Tracker operations are inherently thread-safe via
/// <see cref="ConcurrentDictionary{TKey,TValue}"/>. History entry lists are protected by
/// <c>lock</c> statements during write operations. Read operations snapshot the list contents
/// under lock to prevent enumeration issues.
/// </para>
/// <para>
/// <strong>Ordering:</strong> History entries are returned ordered by
/// <see cref="CorruptionHistoryEntry.CreatedAt"/> descending (most recent first), consistent
/// with the expected query pattern for corruption activity feeds.
/// </para>
/// <para>
/// <strong>Future:</strong> This implementation will be replaced by an EF Core-based
/// <c>CorruptionRepository</c> when full database persistence is implemented.
/// </para>
/// </remarks>
/// <seealso cref="ICorruptionRepository"/>
/// <seealso cref="CorruptionTracker"/>
/// <seealso cref="CorruptionHistoryEntry"/>
/// <seealso cref="InMemoryStressHistoryRepository"/>
public class InMemoryCorruptionRepository : ICorruptionRepository
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Thread-safe storage for corruption trackers, keyed by character ID.
    /// Each character has at most one tracker.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, CorruptionTracker> _trackers = new();

    /// <summary>
    /// Thread-safe storage for corruption history entries, keyed by character ID.
    /// Each character's entries are stored in a list that is locked during writes.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, List<CorruptionHistoryEntry>> _history = new();

    /// <summary>
    /// Logger for repository operations and diagnostics.
    /// </summary>
    private readonly ILogger<InMemoryCorruptionRepository> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new in-memory corruption repository instance.
    /// </summary>
    /// <param name="logger">
    /// Optional logger for diagnostics. If null, a no-op logger is used.
    /// </param>
    public InMemoryCorruptionRepository(ILogger<InMemoryCorruptionRepository>? logger = null)
    {
        _logger = logger ?? NullLogger<InMemoryCorruptionRepository>.Instance;
        _logger.LogDebug("InMemoryCorruptionRepository initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TRACKER OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public Task<CorruptionTracker?> GetByCharacterIdAsync(
        Guid characterId,
        CancellationToken ct = default)
    {
        _trackers.TryGetValue(characterId, out var tracker);

        _logger.LogDebug(
            "Retrieved corruption tracker for {CharacterId}: {Found}",
            characterId,
            tracker != null);

        return Task.FromResult(tracker);
    }

    /// <inheritdoc />
    public Task AddAsync(CorruptionTracker tracker, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(tracker);

        var added = _trackers.TryAdd(tracker.CharacterId, tracker);

        if (added)
        {
            _logger.LogInformation(
                "Created corruption tracker for {CharacterId}. " +
                "Initial corruption: {Corruption}. Tracker ID: {TrackerId}",
                tracker.CharacterId,
                tracker.CurrentCorruption,
                tracker.Id);
        }
        else
        {
            _logger.LogWarning(
                "Attempted to add duplicate corruption tracker for {CharacterId}. " +
                "Tracker already exists. Ignoring add operation",
                tracker.CharacterId);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateAsync(CorruptionTracker tracker, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(tracker);

        // ConcurrentDictionary indexer performs an add-or-update
        _trackers[tracker.CharacterId] = tracker;

        _logger.LogDebug(
            "Updated corruption tracker for {CharacterId}: " +
            "Corruption={Corruption}, Stage={Stage}, " +
            "Thresholds=[25={T25}, 50={T50}, 75={T75}]",
            tracker.CharacterId,
            tracker.CurrentCorruption,
            tracker.Stage,
            tracker.Threshold25Triggered,
            tracker.Threshold50Triggered,
            tracker.Threshold75Triggered);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteAsync(Guid characterId, CancellationToken ct = default)
    {
        var removed = _trackers.TryRemove(characterId, out _);

        if (removed)
        {
            _logger.LogInformation(
                "Deleted corruption tracker for {CharacterId}",
                characterId);
        }
        else
        {
            _logger.LogDebug(
                "No corruption tracker found to delete for {CharacterId}",
                characterId);
        }

        return Task.CompletedTask;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HISTORY OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public Task AddHistoryEntryAsync(CorruptionHistoryEntry entry, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        // Get or create the entry list for this character
        var characterEntries = _history.GetOrAdd(entry.CharacterId, _ => new List<CorruptionHistoryEntry>());

        // Lock the list to ensure thread-safe addition
        lock (characterEntries)
        {
            characterEntries.Add(entry);
        }

        _logger.LogDebug(
            "Corruption history entry added for character {CharacterId}: " +
            "{Source} {Amount:+#;-#;0} → {NewTotal}. " +
            "Entry ID: {EntryId}. Total entries for character: {EntryCount}",
            entry.CharacterId,
            entry.Source,
            entry.Amount,
            entry.NewTotal,
            entry.Id,
            characterEntries.Count);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<CorruptionHistoryEntry>> GetHistoryAsync(
        Guid characterId,
        int? limit = null,
        CancellationToken ct = default)
    {
        if (limit.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit.Value);
        }

        _logger.LogDebug(
            "Querying corruption history for character {CharacterId}{LimitClause}",
            characterId,
            limit.HasValue ? $" with limit {limit.Value}" : "");

        if (!_history.TryGetValue(characterId, out var characterEntries))
        {
            _logger.LogDebug(
                "No corruption history found for character {CharacterId}",
                characterId);
            return Task.FromResult<IReadOnlyList<CorruptionHistoryEntry>>(
                Array.Empty<CorruptionHistoryEntry>());
        }

        // Snapshot the list under lock, order by most recent first, and optionally apply limit
        List<CorruptionHistoryEntry> snapshot;
        lock (characterEntries)
        {
            var ordered = characterEntries
                .OrderByDescending(e => e.CreatedAt);

            snapshot = limit.HasValue
                ? ordered.Take(limit.Value).ToList()
                : ordered.ToList();
        }

        _logger.LogDebug(
            "Retrieved {Count} corruption history entries for character {CharacterId}{LimitClause}",
            snapshot.Count,
            characterId,
            limit.HasValue ? $" (limit: {limit.Value})" : "");

        return Task.FromResult<IReadOnlyList<CorruptionHistoryEntry>>(snapshot);
    }
}
