namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tracks unique (Myth-Forged) item drops per run to prevent duplicates.
/// Each run has its own registry that resets when a new run begins.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="UniqueItemRegistry"/> ensures that one-per-run unique items cannot drop
/// more than once during a game session. This maintains the legendary status of Myth-Forged
/// items by making each drop a memorable, non-repeatable event.
/// </para>
/// <para>
/// The registry uses a <see cref="HashSet{T}"/> for O(1) lookup performance when checking
/// for duplicate drops, making it suitable for frequent drop validation during gameplay.
/// </para>
/// <para>
/// All item ID comparisons are case-insensitive to prevent bypass through casing variations.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a new registry for a run
/// var runId = Guid.NewGuid();
/// var registry = UniqueItemRegistry.Create(runId);
///
/// // Check if an item has already dropped
/// if (!registry.HasDropped("shadowfang-blade"))
/// {
///     // Register the drop
///     var record = registry.RegisterDrop("shadowfang-blade", DropSourceType.Boss, "shadow-lord");
///     Console.WriteLine($"Dropped: {record.ItemId} at {record.DroppedAt}");
/// }
///
/// // Check drop count
/// Console.WriteLine($"Total unique drops this run: {registry.GetDroppedCount()}");
///
/// // Reset for a new run
/// var newRunId = Guid.NewGuid();
/// registry.Reset(newRunId);
/// </code>
/// </example>
public class UniqueItemRegistry : IEntity
{
    /// <summary>
    /// Internal set for O(1) drop lookup. Uses case-insensitive comparison.
    /// </summary>
    private readonly HashSet<string> _droppedItemIds;

    /// <summary>
    /// Internal list for chronological drop history.
    /// </summary>
    private readonly List<DropRecord> _dropHistory;

    /// <summary>
    /// Gets the database identifier.
    /// </summary>
    /// <value>A unique GUID for this registry instance.</value>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the identifier for the current game run.
    /// </summary>
    /// <value>
    /// A GUID representing the current run. Changes when <see cref="Reset"/> is called.
    /// </value>
    /// <remarks>
    /// The run ID enables correlation between the registry state and the game session,
    /// supporting run lifecycle management and potential future persistence.
    /// </remarks>
    public Guid RunId { get; private set; }

    /// <summary>
    /// Gets when this registry was created or last reset.
    /// </summary>
    /// <value>
    /// The UTC timestamp of registry creation or the most recent reset.
    /// </value>
    /// <remarks>
    /// Updated whenever <see cref="Reset"/> is called with a new run ID.
    /// </remarks>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets a read-only view of the drop history.
    /// </summary>
    /// <value>
    /// An immutable list of <see cref="DropRecord"/> instances in chronological order.
    /// </value>
    /// <remarks>
    /// The first element is the oldest drop; the last element is the most recent.
    /// Use <see cref="GetDropsBySourceType"/> for filtered access.
    /// </remarks>
    public IReadOnlyList<DropRecord> DropHistory => _dropHistory.AsReadOnly();

    /// <summary>
    /// Private constructor for EF Core materialization.
    /// </summary>
    /// <remarks>
    /// EF Core requires a parameterless constructor for entity materialization.
    /// The HashSet uses case-insensitive comparison for robust duplicate detection.
    /// </remarks>
    private UniqueItemRegistry()
    {
        _droppedItemIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _dropHistory = new List<DropRecord>();
    }

    /// <summary>
    /// Creates a new <see cref="UniqueItemRegistry"/> for a run.
    /// </summary>
    /// <param name="runId">The unique identifier for the game run.</param>
    /// <returns>A new registry instance initialized for the specified run.</returns>
    /// <remarks>
    /// The registry is created empty with no drops recorded. Call this method
    /// at the start of each new game run.
    /// </remarks>
    /// <example>
    /// <code>
    /// // At the start of a new game
    /// var runId = Guid.NewGuid();
    /// var registry = UniqueItemRegistry.Create(runId);
    ///
    /// // Registry is now ready to track drops for this run
    /// Console.WriteLine($"Registry created for run: {registry.RunId}");
    /// </code>
    /// </example>
    public static UniqueItemRegistry Create(Guid runId)
    {
        return new UniqueItemRegistry
        {
            Id = Guid.NewGuid(),
            RunId = runId,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Checks if a unique item has already dropped in this run.
    /// </summary>
    /// <param name="itemId">The unique item's configuration ID.</param>
    /// <returns>
    /// <c>true</c> if the item has already dropped; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="itemId"/> is null or whitespace.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method performs O(1) lookup using the internal HashSet.
    /// The comparison is case-insensitive.
    /// </para>
    /// <para>
    /// Use this method before generating loot to exclude already-dropped unique items
    /// from the available pool.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // During loot generation
    /// if (registry.HasDropped("shadowfang-blade"))
    /// {
    ///     // Skip this item, select another from the pool
    ///     Console.WriteLine("Shadowfang Blade already dropped this run");
    /// }
    /// </code>
    /// </example>
    public bool HasDropped(string itemId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId, nameof(itemId));
        return _droppedItemIds.Contains(itemId.ToLowerInvariant());
    }

    /// <summary>
    /// Registers that a unique item has dropped.
    /// </summary>
    /// <param name="itemId">The unique item's configuration ID.</param>
    /// <param name="sourceType">What type of source generated the drop.</param>
    /// <param name="sourceId">Specific source identifier (boss ID, container ID, etc.).</param>
    /// <returns>The created drop record with timestamp and normalized IDs.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="itemId"/> or <paramref name="sourceId"/> is null or whitespace.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the item has already dropped in this run.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method enforces the one-per-run constraint by throwing an exception
    /// if the item was already registered. Always call <see cref="HasDropped"/>
    /// before generating a unique drop to avoid this exception.
    /// </para>
    /// <para>
    /// The drop record is added to the history with a UTC timestamp,
    /// enabling chronological tracking and statistical analysis.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // After successfully rolling a unique drop
    /// try
    /// {
    ///     var record = registry.RegisterDrop(
    ///         "shadowfang-blade",
    ///         DropSourceType.Boss,
    ///         "shadow-lord");
    ///
    ///     Console.WriteLine($"Registered drop: {record}");
    /// }
    /// catch (InvalidOperationException ex)
    /// {
    ///     Console.WriteLine($"Duplicate drop prevented: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    public DropRecord RegisterDrop(string itemId, DropSourceType sourceType, string sourceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId, nameof(itemId));
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceId, nameof(sourceId));

        var normalizedId = itemId.ToLowerInvariant();

        if (_droppedItemIds.Contains(normalizedId))
        {
            throw new InvalidOperationException(
                $"Unique item '{itemId}' has already dropped in this run.");
        }

        _droppedItemIds.Add(normalizedId);

        var record = DropRecord.Create(normalizedId, sourceType, sourceId);
        _dropHistory.Add(record);

        return record;
    }

    /// <summary>
    /// Gets the count of unique items that have dropped in this run.
    /// </summary>
    /// <returns>The number of unique items registered as dropped.</returns>
    /// <remarks>
    /// This count represents the total number of Myth-Forged items
    /// acquired during the current run.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Unique items found: {registry.GetDroppedCount()}");
    /// // Output: Unique items found: 3
    /// </code>
    /// </example>
    public int GetDroppedCount() => _droppedItemIds.Count;

    /// <summary>
    /// Gets all item IDs that have dropped in this run.
    /// </summary>
    /// <returns>
    /// A read-only set of lowercase item IDs that have been registered as dropped.
    /// </returns>
    /// <remarks>
    /// Useful for bulk filtering of available unique items during loot generation.
    /// All IDs are lowercase for consistent comparison.
    /// </remarks>
    /// <example>
    /// <code>
    /// var droppedIds = registry.GetDroppedItemIds();
    /// var availableUniques = allUniques.Where(u => !droppedIds.Contains(u.ItemId));
    /// </code>
    /// </example>
    public IReadOnlySet<string> GetDroppedItemIds() => _droppedItemIds;

    /// <summary>
    /// Checks if this registry is for the specified run.
    /// </summary>
    /// <param name="runId">The run ID to check.</param>
    /// <returns>
    /// <c>true</c> if this registry's <see cref="RunId"/> matches the specified value;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Use this method to validate that the registry is current before
    /// performing drop checks or registrations.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!registry.IsCurrentRun(currentSession.RunId))
    /// {
    ///     registry.Reset(currentSession.RunId);
    /// }
    /// </code>
    /// </example>
    public bool IsCurrentRun(Guid runId) => RunId == runId;

    /// <summary>
    /// Resets the registry for a new run.
    /// </summary>
    /// <param name="newRunId">The new run's identifier.</param>
    /// <remarks>
    /// <para>
    /// This method clears all drop history and resets the internal state,
    /// effectively starting fresh for a new game run.
    /// </para>
    /// <para>
    /// Call this method at the start of each new game session to allow
    /// all unique items to drop again.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Player dies or completes the game
    /// var newRunId = Guid.NewGuid();
    /// registry.Reset(newRunId);
    ///
    /// // All unique items are now available again
    /// Console.WriteLine($"Registry reset for new run: {registry.RunId}");
    /// Console.WriteLine($"Drops cleared: {registry.GetDroppedCount()}"); // 0
    /// </code>
    /// </example>
    public void Reset(Guid newRunId)
    {
        RunId = newRunId;
        CreatedAt = DateTime.UtcNow;
        _droppedItemIds.Clear();
        _dropHistory.Clear();
    }

    /// <summary>
    /// Gets drop records for a specific source type.
    /// </summary>
    /// <param name="sourceType">The type of drop sources to retrieve.</param>
    /// <returns>
    /// An enumerable of <see cref="DropRecord"/> instances matching the specified source type.
    /// </returns>
    /// <remarks>
    /// Use this method for analysis of drops by source category,
    /// e.g., to see which items came from bosses vs. containers.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get all boss drops this run
    /// var bossDrops = registry.GetDropsBySourceType(DropSourceType.Boss);
    /// foreach (var drop in bossDrops)
    /// {
    ///     Console.WriteLine($"Boss drop: {drop.ItemId} from {drop.SourceId}");
    /// }
    /// </code>
    /// </example>
    public IEnumerable<DropRecord> GetDropsBySourceType(DropSourceType sourceType) =>
        _dropHistory.Where(r => r.SourceType == sourceType);

    /// <summary>
    /// Gets the most recent drop record, if any.
    /// </summary>
    /// <returns>
    /// The most recent <see cref="DropRecord"/> if drops exist; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// Useful for displaying the last acquired unique item or for
    /// presentation effects after a legendary drop.
    /// </remarks>
    /// <example>
    /// <code>
    /// var lastDrop = registry.GetLastDrop();
    /// if (lastDrop.HasValue)
    /// {
    ///     Console.WriteLine($"Last legendary: {lastDrop.Value.ItemId}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("No legendary items found yet this run.");
    /// }
    /// </code>
    /// </example>
    public DropRecord? GetLastDrop() =>
        _dropHistory.Count > 0 ? _dropHistory[^1] : null;
}
