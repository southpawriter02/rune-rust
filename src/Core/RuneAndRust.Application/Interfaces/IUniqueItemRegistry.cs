namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Application-layer interface for managing the unique item registry.
/// Provides filtering logic and integration with unique item definitions.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IUniqueItemRegistry"/> extends the domain-level <see cref="UniqueItemRegistry"/>
/// functionality with application concerns such as filtering available unique items
/// based on drop source compatibility and player class affinity.
/// </para>
/// <para>
/// Implementations coordinate between the domain registry and configuration providers
/// to deliver filtered pools of droppable unique items during loot generation.
/// </para>
/// <para>
/// The interface is designed for dependency injection and supports the run lifecycle
/// pattern where the registry resets at the start of each new game session.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MythForgedService : IMythForgedService
/// {
///     private readonly IUniqueItemRegistry _registry;
///
///     public MythForgedService(IUniqueItemRegistry registry)
///     {
///         _registry = registry;
///     }
///
///     public UniqueItem? TryGenerateDrop(DropSourceType sourceType, string sourceId, string playerClass)
///     {
///         var available = _registry.GetAvailableUniques(sourceType, sourceId, playerClass);
///         if (available.Count == 0) return null;
///
///         var selected = available[Random.Shared.Next(available.Count)];
///         _registry.RegisterDrop(selected.ItemId, sourceType, sourceId);
///         return selected;
///     }
/// }
/// </code>
/// </example>
public interface IUniqueItemRegistry
{
    /// <summary>
    /// Gets the current run ID.
    /// </summary>
    /// <value>
    /// The GUID identifying the current game run.
    /// </value>
    /// <remarks>
    /// Use this property to validate that the registry is synchronized
    /// with the current game session before performing operations.
    /// </remarks>
    Guid CurrentRunId { get; }

    /// <summary>
    /// Checks if a unique item has already dropped in the current run.
    /// </summary>
    /// <param name="itemId">The unique item's configuration ID.</param>
    /// <returns>
    /// <c>true</c> if the item has already dropped; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs O(1) lookup and is case-insensitive.
    /// </para>
    /// <para>
    /// Used by the loot system to exclude already-dropped items from
    /// the available pool before selection.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!registry.HasDropped("shadowfang-blade"))
    /// {
    ///     // Item is available for this drop
    /// }
    /// </code>
    /// </example>
    bool HasDropped(string itemId);

    /// <summary>
    /// Registers that a unique item has dropped.
    /// </summary>
    /// <param name="itemId">The unique item's configuration ID.</param>
    /// <param name="sourceType">What type of source generated the drop.</param>
    /// <param name="sourceId">Specific source identifier (boss ID, container ID, etc.).</param>
    /// <returns>The created drop record with timestamp and normalized IDs.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the item has already dropped in this run.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Call this method after successfully rolling and awarding a unique item
    /// to prevent it from appearing again this run.
    /// </para>
    /// <para>
    /// The returned <see cref="DropRecord"/> can be used for presentation,
    /// logging, or achievement tracking.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var record = registry.RegisterDrop("shadowfang-blade", DropSourceType.Boss, "shadow-lord");
    /// Console.WriteLine($"Legendary acquired: {record.ItemId} at {record.DroppedAt}");
    /// </code>
    /// </example>
    DropRecord RegisterDrop(string itemId, DropSourceType sourceType, string sourceId);

    /// <summary>
    /// Gets unique items available for a drop source, excluding already-dropped items.
    /// </summary>
    /// <param name="sourceType">The type of source generating loot.</param>
    /// <param name="sourceId">Specific source identifier.</param>
    /// <param name="classId">Optional player class for affinity filtering.</param>
    /// <returns>
    /// A list of <see cref="UniqueItem"/> instances that can still drop from this source.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method applies a multi-step filter:
    /// </para>
    /// <list type="number">
    /// <item>Load all unique items from configuration.</item>
    /// <item>Filter by <see cref="UniqueItem.CanDropFrom"/> for the source.</item>
    /// <item>If <paramref name="classId"/> is provided, filter by <see cref="UniqueItem.HasAffinityFor"/>.</item>
    /// <item>Exclude items where <see cref="HasDropped"/> returns <c>true</c>.</item>
    /// </list>
    /// <para>
    /// If the returned list is empty, the loot system should fall back to
    /// generating a Tier 3 (Optimized) item instead.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var available = registry.GetAvailableUniques(
    ///     DropSourceType.Boss,
    ///     "shadow-lord",
    ///     "warrior");
    ///
    /// if (available.Count == 0)
    /// {
    ///     // Fall back to Tier 3 loot
    /// }
    /// else
    /// {
    ///     // Select from available pool
    ///     var selected = available[Random.Shared.Next(available.Count)];
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<UniqueItem> GetAvailableUniques(
        DropSourceType sourceType,
        string sourceId,
        string? classId = null);

    /// <summary>
    /// Gets the full drop history for the current run.
    /// </summary>
    /// <returns>
    /// An immutable list of <see cref="DropRecord"/> instances in chronological order.
    /// </returns>
    /// <remarks>
    /// Use this for displaying run statistics, achievement tracking,
    /// or end-of-run summary screens.
    /// </remarks>
    /// <example>
    /// <code>
    /// var history = registry.GetDropHistory();
    /// Console.WriteLine($"Found {history.Count} legendary items this run:");
    /// foreach (var drop in history)
    /// {
    ///     Console.WriteLine($"  - {drop.ItemId} from {drop.SourceType}:{drop.SourceId}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<DropRecord> GetDropHistory();

    /// <summary>
    /// Gets the count of unique items dropped in the current run.
    /// </summary>
    /// <returns>The number of unique items that have been registered as dropped.</returns>
    /// <remarks>
    /// Equivalent to <c>GetDropHistory().Count</c> but more efficient.
    /// </remarks>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Legendary items found: {registry.GetDroppedCount()}/15");
    /// </code>
    /// </example>
    int GetDroppedCount();

    /// <summary>
    /// Resets the registry for a new run.
    /// </summary>
    /// <param name="newRunId">The new run's identifier.</param>
    /// <remarks>
    /// <para>
    /// Call this method at the start of each new game session to allow
    /// all unique items to become available again.
    /// </para>
    /// <para>
    /// This is typically triggered by game lifecycle events such as:
    /// </para>
    /// <list type="bullet">
    /// <item>Player death (permadeath roguelike mode)</item>
    /// <item>New game started</item>
    /// <item>Run completion (victory or defeat)</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // On new game or player death
    /// var newRunId = Guid.NewGuid();
    /// registry.Reset(newRunId);
    /// Console.WriteLine($"New run started: {registry.CurrentRunId}");
    /// </code>
    /// </example>
    void Reset(Guid newRunId);
}
