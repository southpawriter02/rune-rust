namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a Myth-Forged (unique/legendary) item with fixed stats and special properties.
/// </summary>
/// <remarks>
/// <para>
/// Unique items are configuration-defined legendary equipment that provide fixed stats,
/// special effects, and rich lore. Unlike randomly generated equipment, unique items
/// have predetermined properties that make them memorable and run-defining.
/// </para>
/// <para>
/// All unique items are automatically assigned the <see cref="QualityTier.MythForged"/>
/// quality tier. They may have class affinities that restrict which character classes
/// benefit from the item, and can only drop once per game run to maintain their
/// legendary status.
/// </para>
/// <para>
/// Unique items are loaded from the <c>unique-items.json</c> configuration file.
/// The <see cref="SpecialEffectIds"/> property contains references to effects
/// processed in v0.16.5b, and <see cref="SetId"/> enables future set bonus features.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var stats = ItemStats.Create(might: 5, agility: 2, bonusDamage: 15);
/// var dropSources = new[]
/// {
///     DropSource.Create(DropSourceType.Boss, "shadow-lord", 5.0m),
///     DropSource.Create(DropSourceType.Container, "legendary-chest", 0.5m)
/// };
/// 
/// var blade = UniqueItem.Create(
///     itemId: "shadowfang-blade",
///     name: "Shadowfang Blade",
///     description: "A blade forged in eternal darkness.",
///     flavorText: "\"The shadows remember what the light forgets.\"",
///     category: EquipmentCategory.Weapon,
///     stats: stats,
///     dropSources: dropSources,
///     classAffinities: new[] { "warrior", "rogue" },
///     requiredLevel: 10,
///     specialEffectIds: new[] { "life-drain", "shadow-damage" });
/// </code>
/// </example>
public class UniqueItem : IEntity
{
    /// <summary>
    /// Gets the unique database identifier for this item.
    /// </summary>
    /// <value>A GUID that uniquely identifies this entity instance.</value>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the configuration identifier for this unique item.
    /// </summary>
    /// <value>A lowercase kebab-case string (e.g., "shadowfang-blade").</value>
    /// <remarks>
    /// The item ID is normalized to lowercase during creation to ensure
    /// consistent matching across configuration and runtime lookups.
    /// </remarks>
    public string ItemId { get; private set; }

    /// <summary>
    /// Gets the display name for this item.
    /// </summary>
    /// <value>The human-readable name shown to players.</value>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the functional description of this item.
    /// </summary>
    /// <value>A description explaining what the item does and its properties.</value>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the lore/flavor text for atmospheric presentation.
    /// </summary>
    /// <value>Optional atmospheric text, often a quote or historical note.</value>
    public string FlavorText { get; private set; }

    /// <summary>
    /// Gets the equipment category (weapon, armor, accessory, tool).
    /// </summary>
    /// <value>The <see cref="EquipmentCategory"/> this item belongs to.</value>
    public EquipmentCategory Category { get; private set; }

    /// <summary>
    /// Gets the quality tier, which is always Legendary for unique items.
    /// </summary>
    /// <value>Always <see cref="QualityTier.MythForged"/>.</value>
    public QualityTier QualityTier { get; private set; }

    /// <summary>
    /// Gets the stat bonuses provided by this item.
    /// </summary>
    /// <value>An <see cref="ValueObjects.ItemStats"/> containing all stat modifiers.</value>
    public ItemStats Stats { get; private set; }

    /// <summary>
    /// Gets the collection of sources from which this item can drop.
    /// </summary>
    /// <value>A read-only list of <see cref="DropSource"/> entries.</value>
    /// <remarks>
    /// Each unique item must have at least one drop source. Multiple sources
    /// allow the same item to be obtainable from different game activities.
    /// </remarks>
    public IReadOnlyList<DropSource> DropSources { get; private set; }

    /// <summary>
    /// Gets the class IDs that have affinity for this item.
    /// </summary>
    /// <value>A list of class identifiers, or empty if all classes can use the item.</value>
    /// <remarks>
    /// When this list is empty, all character classes can effectively use the item.
    /// When populated, only characters of the specified classes receive full benefits.
    /// Class IDs are normalized to lowercase.
    /// </remarks>
    public IReadOnlyList<string> ClassAffinities { get; private set; }

    /// <summary>
    /// Gets the minimum character level required to equip this item.
    /// </summary>
    /// <value>A positive integer representing the level requirement (minimum 1).</value>
    public int RequiredLevel { get; private set; }

    /// <summary>
    /// Gets the IDs of special effects granted by this item.
    /// </summary>
    /// <value>A list of effect identifiers processed in v0.16.5b.</value>
    /// <remarks>
    /// Special effect IDs are normalized to lowercase. Effect processing and
    /// application is handled by the special effect system in v0.16.5b.
    /// </remarks>
    public IReadOnlyList<string> SpecialEffectIds { get; private set; }

    /// <summary>
    /// Gets the set identifier if this item belongs to an equipment set.
    /// </summary>
    /// <value>The set ID if part of a set, or null if standalone.</value>
    /// <remarks>
    /// Set bonuses are a future feature. This property is prepared for
    /// forward compatibility but not processed in v0.16.5a.
    /// </remarks>
    public string? SetId { get; private set; }

    /// <summary>
    /// Private constructor for EF Core materialization.
    /// </summary>
    private UniqueItem()
    {
        ItemId = null!;
        Name = null!;
        Description = null!;
        FlavorText = null!;
        Stats = ItemStats.Empty;
        DropSources = Array.Empty<DropSource>();
        ClassAffinities = Array.Empty<string>();
        SpecialEffectIds = Array.Empty<string>();
    }

    /// <summary>
    /// Creates a new UniqueItem with validation.
    /// </summary>
    /// <param name="itemId">The configuration identifier (will be normalized to lowercase).</param>
    /// <param name="name">The display name for the item.</param>
    /// <param name="description">The functional description of the item.</param>
    /// <param name="flavorText">Optional lore/flavor text.</param>
    /// <param name="category">The equipment category.</param>
    /// <param name="stats">The stat bonuses provided by the item.</param>
    /// <param name="dropSources">The sources from which this item can drop (at least one required).</param>
    /// <param name="classAffinities">Optional list of class IDs that have affinity for this item.</param>
    /// <param name="requiredLevel">The minimum level required to equip (default 1).</param>
    /// <param name="specialEffectIds">Optional list of special effect IDs.</param>
    /// <param name="setId">Optional set identifier for equipment set membership.</param>
    /// <returns>A new validated UniqueItem instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="itemId"/>, <paramref name="name"/>, or 
    /// <paramref name="description"/> is null or whitespace, or when 
    /// <paramref name="dropSources"/> is empty.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="dropSources"/> is null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="requiredLevel"/> is less than 1.
    /// </exception>
    public static UniqueItem Create(
        string itemId,
        string name,
        string description,
        string flavorText,
        EquipmentCategory category,
        ItemStats stats,
        IReadOnlyList<DropSource> dropSources,
        IReadOnlyList<string>? classAffinities = null,
        int requiredLevel = 1,
        IReadOnlyList<string>? specialEffectIds = null,
        string? setId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId, nameof(itemId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));
        ArgumentNullException.ThrowIfNull(dropSources, nameof(dropSources));
        ArgumentOutOfRangeException.ThrowIfLessThan(requiredLevel, 1, nameof(requiredLevel));

        if (dropSources.Count == 0)
        {
            throw new ArgumentException("At least one drop source is required.", nameof(dropSources));
        }

        return new UniqueItem
        {
            Id = Guid.NewGuid(),
            ItemId = itemId.ToLowerInvariant(),
            Name = name,
            Description = description,
            FlavorText = flavorText ?? string.Empty,
            Category = category,
            QualityTier = QualityTier.MythForged, // Always legendary
            Stats = stats,
            DropSources = dropSources.ToList().AsReadOnly(),
            ClassAffinities = (classAffinities ?? Array.Empty<string>())
                .Select(c => c.ToLowerInvariant()).ToList().AsReadOnly(),
            RequiredLevel = requiredLevel,
            SpecialEffectIds = (specialEffectIds ?? Array.Empty<string>())
                .Select(e => e.ToLowerInvariant()).ToList().AsReadOnly(),
            SetId = setId?.ToLowerInvariant()
        };
    }

    /// <summary>
    /// Checks if this item has affinity for a specific character class.
    /// </summary>
    /// <param name="classId">The class identifier to check.</param>
    /// <returns>
    /// <c>true</c> if no class restrictions exist (empty affinities list) or 
    /// if the specified class is in the affinity list; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Class comparison is case-insensitive as all IDs are normalized to lowercase.
    /// An empty <see cref="ClassAffinities"/> list means all classes have affinity.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Item with warrior and rogue affinity
    /// var item = UniqueItem.Create(..., classAffinities: new[] { "warrior", "rogue" });
    /// item.HasAffinityFor("warrior"); // true
    /// item.HasAffinityFor("mage");    // false
    /// 
    /// // Item with no restrictions (all classes)
    /// var universalItem = UniqueItem.Create(..., classAffinities: null);
    /// universalItem.HasAffinityFor("mage"); // true
    /// </code>
    /// </example>
    public bool HasAffinityFor(string classId)
    {
        if (ClassAffinities.Count == 0)
        {
            return true;
        }

        return ClassAffinities.Contains(classId.ToLowerInvariant());
    }

    /// <summary>
    /// Gets all drop sources of a specific type.
    /// </summary>
    /// <param name="sourceType">The type of drop sources to retrieve.</param>
    /// <returns>An enumerable of matching drop sources.</returns>
    /// <example>
    /// <code>
    /// var bossSources = item.GetDropSourcesByType(DropSourceType.Boss);
    /// foreach (var source in bossSources)
    /// {
    ///     Console.WriteLine($"Drops from {source.SourceId} @ {source.DropChance}%");
    /// }
    /// </code>
    /// </example>
    public IEnumerable<DropSource> GetDropSourcesByType(DropSourceType sourceType) =>
        DropSources.Where(ds => ds.SourceType == sourceType);

    /// <summary>
    /// Checks if this item can drop from a specific source.
    /// </summary>
    /// <param name="sourceType">The type of drop source.</param>
    /// <param name="sourceId">The specific source identifier.</param>
    /// <returns><c>true</c> if a matching drop source exists; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Source ID comparison is case-insensitive as all IDs are normalized to lowercase.
    /// </remarks>
    public bool CanDropFrom(DropSourceType sourceType, string sourceId) =>
        DropSources.Any(ds =>
            ds.SourceType == sourceType &&
            ds.SourceId.Equals(sourceId.ToLowerInvariant(), StringComparison.Ordinal));

    /// <summary>
    /// Gets the total number of drop sources for this item.
    /// </summary>
    /// <value>The count of drop sources.</value>
    public int DropSourceCount => DropSources.Count;

    /// <summary>
    /// Gets a value indicating whether this item has any special effects.
    /// </summary>
    /// <value><c>true</c> if the item has at least one special effect; otherwise, <c>false</c>.</value>
    public bool HasSpecialEffects => SpecialEffectIds.Count > 0;

    /// <summary>
    /// Gets a value indicating whether this item is part of an equipment set.
    /// </summary>
    /// <value><c>true</c> if the item belongs to a set; otherwise, <c>false</c>.</value>
    public bool IsPartOfSet => !string.IsNullOrEmpty(SetId);
}
