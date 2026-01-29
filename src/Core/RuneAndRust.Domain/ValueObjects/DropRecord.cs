namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Immutable record of a unique item drop event.
/// Tracks what dropped, when, and from what source.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DropRecord"/> captures the complete context of a unique item drop event,
/// enabling duplicate prevention, drop history tracking, and statistical analysis.
/// </para>
/// <para>
/// All identifiers are normalized to lowercase during creation to ensure consistent
/// matching regardless of input casing.
/// </para>
/// <para>
/// This value object is immutable and uses <c>readonly record struct</c> for efficient
/// allocation and built-in equality semantics.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a drop record for a boss drop
/// var bossDropRecord = DropRecord.Create(
///     "shadowfang-blade",
///     DropSourceType.Boss,
///     "shadow-lord");
///
/// // Create a drop record for a container with a specific timestamp
/// var containerRecord = DropRecord.Create(
///     "phoenix-amulet",
///     DropSourceType.Container,
///     "legendary-chest",
///     new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
/// </code>
/// </example>
public readonly record struct DropRecord
{
    /// <summary>
    /// Gets the unique item's configuration ID.
    /// </summary>
    /// <value>
    /// The normalized (lowercase) item identifier, e.g., "shadowfang-blade".
    /// </value>
    /// <remarks>
    /// This ID corresponds to the <see cref="Entities.UniqueItem.ItemId"/> property
    /// and is used for duplicate detection in the registry.
    /// </remarks>
    public string ItemId { get; }

    /// <summary>
    /// Gets when the item dropped.
    /// </summary>
    /// <value>
    /// The UTC timestamp of the drop event.
    /// </value>
    /// <remarks>
    /// Defaults to <see cref="DateTime.UtcNow"/> if not explicitly specified during creation.
    /// Used for chronological ordering and statistical analysis.
    /// </remarks>
    public DateTime DroppedAt { get; }

    /// <summary>
    /// Gets the type of source that generated the drop.
    /// </summary>
    /// <value>
    /// The <see cref="DropSourceType"/> enum value indicating the drop origin category.
    /// </value>
    /// <remarks>
    /// Examples include <see cref="DropSourceType.Boss"/> for boss kills,
    /// <see cref="DropSourceType.Container"/> for chest looting, etc.
    /// </remarks>
    public DropSourceType SourceType { get; }

    /// <summary>
    /// Gets the specific source identifier.
    /// </summary>
    /// <value>
    /// The normalized (lowercase) source ID, e.g., "shadow-lord" for a boss or "legendary-chest" for a container.
    /// </value>
    /// <remarks>
    /// This ID enables tracking of drop sources for detailed statistics and lore integration.
    /// Combined with <see cref="SourceType"/>, it uniquely identifies the drop origin.
    /// </remarks>
    public string SourceId { get; }

    /// <summary>
    /// Private constructor for controlled instantiation.
    /// </summary>
    /// <param name="itemId">The normalized item identifier.</param>
    /// <param name="droppedAt">The drop timestamp.</param>
    /// <param name="sourceType">The type of drop source.</param>
    /// <param name="sourceId">The normalized source identifier.</param>
    private DropRecord(
        string itemId,
        DateTime droppedAt,
        DropSourceType sourceType,
        string sourceId)
    {
        ItemId = itemId;
        DroppedAt = droppedAt;
        SourceType = sourceType;
        SourceId = sourceId;
    }

    /// <summary>
    /// Creates a new <see cref="DropRecord"/> with validation and ID normalization.
    /// </summary>
    /// <param name="itemId">
    /// The unique item's configuration ID. Will be normalized to lowercase.
    /// </param>
    /// <param name="sourceType">
    /// The type of source that generated the drop.
    /// </param>
    /// <param name="sourceId">
    /// The specific source identifier (e.g., "shadow-lord", "legendary-chest").
    /// Will be normalized to lowercase.
    /// </param>
    /// <param name="droppedAt">
    /// Optional timestamp for the drop. Defaults to <see cref="DateTime.UtcNow"/>.
    /// </param>
    /// <returns>A new <see cref="DropRecord"/> instance with validated and normalized data.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="itemId"/> or <paramref name="sourceId"/> is null or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// // Typical drop from a boss
    /// var record = DropRecord.Create("shadowfang-blade", DropSourceType.Boss, "shadow-lord");
    ///
    /// // Historical drop with explicit timestamp
    /// var historical = DropRecord.Create(
    ///     "epic-ring",
    ///     DropSourceType.Container,
    ///     "boss-chest",
    ///     new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));
    /// </code>
    /// </example>
    public static DropRecord Create(
        string itemId,
        DropSourceType sourceType,
        string sourceId,
        DateTime? droppedAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId, nameof(itemId));
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceId, nameof(sourceId));

        var normalizedItemId = itemId.ToLowerInvariant();
        var normalizedSourceId = sourceId.ToLowerInvariant();
        var timestamp = droppedAt ?? DateTime.UtcNow;

        return new DropRecord(
            normalizedItemId,
            timestamp,
            sourceType,
            normalizedSourceId);
    }

    /// <summary>
    /// Returns a string representation of this drop record for debugging purposes.
    /// </summary>
    /// <returns>
    /// A formatted string showing the item ID, source type, source ID, and timestamp.
    /// </returns>
    /// <example>
    /// <code>
    /// var record = DropRecord.Create("shadowfang-blade", DropSourceType.Boss, "shadow-lord");
    /// Console.WriteLine(record.ToString());
    /// // Output: DropRecord[shadowfang-blade from Boss:shadow-lord @ 2026-01-29 12:00:00Z]
    /// </code>
    /// </example>
    public override string ToString() =>
        $"DropRecord[{ItemId} from {SourceType}:{SourceId} @ {DroppedAt:u}]";
}
