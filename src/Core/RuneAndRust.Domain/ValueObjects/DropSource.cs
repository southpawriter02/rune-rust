namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Immutable value object representing a source from which a unique item can drop.
/// </summary>
/// <remarks>
/// <para>
/// DropSource defines where a <see cref="Entities.UniqueItem"/> can be obtained,
/// combining the source type, specific source identifier, and base drop chance.
/// </para>
/// <para>
/// A unique item may have multiple drop sources with varying drop chances.
/// For example, a legendary sword might drop from a specific boss (5%) or
/// from a legendary chest container (0.5%).
/// </para>
/// <para>
/// The source ID is normalized to lowercase to ensure consistent matching
/// regardless of case in configuration or runtime lookups.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a boss drop source with 5% chance
/// var bossSource = DropSource.Create(DropSourceType.Boss, "shadow-lord", 5.0m);
/// 
/// // Create a quest reward with 100% chance
/// var questSource = DropSource.Create(DropSourceType.Quest, "fallen-order", 100.0m);
/// 
/// Console.WriteLine(bossSource); // "DropSource[Boss:shadow-lord @ 5.00%]"
/// </code>
/// </example>
public readonly record struct DropSource
{
    /// <summary>
    /// Gets the type of drop source.
    /// </summary>
    /// <value>The <see cref="DropSourceType"/> indicating what kind of source this is.</value>
    public DropSourceType SourceType { get; }

    /// <summary>
    /// Gets the identifier for the specific source (monster ID, container type, etc.).
    /// </summary>
    /// <value>A lowercase string identifier for the specific source entity.</value>
    /// <remarks>
    /// The source ID is normalized to lowercase during creation to ensure
    /// consistent matching. For example, "Shadow-Lord" becomes "shadow-lord".
    /// </remarks>
    public string SourceId { get; }

    /// <summary>
    /// Gets the base drop chance as a percentage (0.0 to 100.0).
    /// </summary>
    /// <value>The probability of the item dropping from this source.</value>
    /// <remarks>
    /// Drop chance represents the base probability before any modifiers are applied.
    /// A value of 100.0 represents a guaranteed drop (used for quest rewards).
    /// </remarks>
    public decimal DropChance { get; }

    /// <summary>
    /// Private constructor for creating DropSource instances.
    /// </summary>
    /// <param name="sourceType">The type of drop source.</param>
    /// <param name="sourceId">The normalized source identifier.</param>
    /// <param name="dropChance">The validated drop chance percentage.</param>
    private DropSource(DropSourceType sourceType, string sourceId, decimal dropChance)
    {
        SourceType = sourceType;
        SourceId = sourceId;
        DropChance = dropChance;
    }

    /// <summary>
    /// Creates a new DropSource with validation.
    /// </summary>
    /// <param name="sourceType">The type of drop source.</param>
    /// <param name="sourceId">The identifier for the specific source. Will be normalized to lowercase.</param>
    /// <param name="dropChance">The drop chance as a percentage (0.0 to 100.0).</param>
    /// <returns>A new DropSource instance with validated and normalized values.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sourceId"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="dropChance"/> is negative or greater than 100.
    /// </exception>
    /// <example>
    /// <code>
    /// // Valid creation
    /// var source = DropSource.Create(DropSourceType.Boss, "goblin-king", 3.5m);
    /// 
    /// // These will throw exceptions:
    /// // DropSource.Create(DropSourceType.Monster, "", 1.0m);     // Empty sourceId
    /// // DropSource.Create(DropSourceType.Monster, "orc", -5.0m); // Negative chance
    /// // DropSource.Create(DropSourceType.Monster, "orc", 150m);  // Over 100%
    /// </code>
    /// </example>
    public static DropSource Create(DropSourceType sourceType, string sourceId, decimal dropChance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceId, nameof(sourceId));
        ArgumentOutOfRangeException.ThrowIfNegative(dropChance, nameof(dropChance));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(dropChance, 100m, nameof(dropChance));

        return new DropSource(sourceType, sourceId.ToLowerInvariant(), dropChance);
    }

    /// <summary>
    /// Gets a value indicating whether this source guarantees a drop.
    /// </summary>
    /// <value><c>true</c> if drop chance is 100%; otherwise, <c>false</c>.</value>
    public bool IsGuaranteed => DropChance >= 100m;

    /// <summary>
    /// Returns a formatted string representation of the drop source.
    /// </summary>
    /// <returns>A string in the format "DropSource[Type:Id @ Chance%]".</returns>
    public override string ToString() =>
        $"DropSource[{SourceType}:{SourceId} @ {DropChance:F2}%]";
}
