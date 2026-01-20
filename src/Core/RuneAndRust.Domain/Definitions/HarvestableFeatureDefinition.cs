using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a harvestable feature type that appears in rooms and yields resources when gathered.
/// </summary>
/// <remarks>
/// <para>
/// Harvestable feature definitions are data-driven templates loaded from JSON configuration.
/// They define the properties of environmental objects that players can gather resources from,
/// including the resource yielded, difficulty class, quantity ranges, optional tool requirements,
/// and replenishment behavior.
/// </para>
/// <para>
/// Features are identified by a unique string ID (e.g., "iron-ore-vein") and link to resource
/// definitions via the <see cref="ResourceId"/> property. The provider validates that referenced
/// resources exist during loading.
/// </para>
/// <para>
/// Key features:
/// <list type="bullet">
///   <item><description>Links to resource definitions for gathering yields</description></item>
///   <item><description>Configurable quantity ranges (min/max) for randomized yields</description></item>
///   <item><description>Difficulty class for gathering skill checks</description></item>
///   <item><description>Optional tool requirements for gathering</description></item>
///   <item><description>Replenishment system for renewable resource nodes</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a basic ore vein feature
/// var oreVein = HarvestableFeatureDefinition.Create(
///     "iron-ore-vein",
///     "Iron Ore Vein",
///     "A vein of iron ore embedded in the rock wall",
///     "iron-ore",
///     minQuantity: 1,
///     maxQuantity: 5,
///     difficultyClass: 12);
///
/// // Create a feature with tool requirement and replenishment
/// var herbPatch = HarvestableFeatureDefinition.Create(
///     "herb-patch",
///     "Herb Patch",
///     "A patch of healing herbs growing in the shade",
///     "healing-herb",
///     minQuantity: 2,
///     maxQuantity: 6,
///     difficultyClass: 10,
///     requiredToolId: null,
///     replenishTurns: 100);
/// </code>
/// </example>
public class HarvestableFeatureDefinition : IEntity
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this feature definition.
    /// </summary>
    /// <remarks>
    /// This is an internal GUID used by the entity system.
    /// For string-based lookups, use <see cref="FeatureId"/>.
    /// </remarks>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the string identifier used for lookups and configuration references.
    /// </summary>
    /// <remarks>
    /// Feature IDs use kebab-case formatting (lowercase with hyphens).
    /// The Create method automatically lowercases the provided ID.
    /// </remarks>
    /// <example>"iron-ore-vein", "herb-patch", "gem-cluster"</example>
    public string FeatureId { get; private set; } = null!;

    /// <summary>
    /// Gets the display name of the feature shown in room descriptions.
    /// </summary>
    /// <example>"Iron Ore Vein", "Herb Patch", "Gem Cluster"</example>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the description shown when examining the feature.
    /// </summary>
    /// <remarks>
    /// This provides flavor text and contextual information about the feature.
    /// </remarks>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Gets the ID of the resource this feature yields when gathered.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Must match a valid resource ID from the resource definitions.
    /// The provider validates this reference during loading.
    /// </para>
    /// <para>
    /// Resource IDs are stored in lowercase for case-insensitive matching.
    /// </para>
    /// </remarks>
    /// <example>"iron-ore", "healing-herb", "ruby"</example>
    public string ResourceId { get; private set; } = null!;

    /// <summary>
    /// Gets the minimum quantity of resources yielded on a successful gather.
    /// </summary>
    /// <remarks>
    /// Must be greater than or equal to 0. A value of 0 means the feature
    /// may sometimes yield nothing even on a successful check.
    /// </remarks>
    public int MinQuantity { get; private set; }

    /// <summary>
    /// Gets the maximum quantity of resources yielded on a successful gather.
    /// </summary>
    /// <remarks>
    /// Must be greater than or equal to <see cref="MinQuantity"/>.
    /// The actual yield is randomly determined between MinQuantity and MaxQuantity.
    /// </remarks>
    public int MaxQuantity { get; private set; }

    /// <summary>
    /// Gets the difficulty class for gathering from this feature.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Players must roll d20 + skill modifier >= DC to succeed.
    /// </para>
    /// <para>
    /// Typical range:
    /// <list type="bullet">
    ///   <item><description>10 (Easy) - Common resources, no special skill needed</description></item>
    ///   <item><description>12-13 (Moderate) - Standard resources</description></item>
    ///   <item><description>15 (Hard) - Valuable resources</description></item>
    ///   <item><description>16-18 (Very Hard) - Rare resources</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int DifficultyClass { get; private set; }

    /// <summary>
    /// Gets the optional tool ID required to gather from this feature.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If set, the player must have this tool in their inventory to attempt gathering.
    /// Without the tool, the player cannot interact with this feature.
    /// </para>
    /// <para>
    /// Common tool IDs: "pickaxe", "skinning-knife", "axe", "herbalism-kit"
    /// </para>
    /// </remarks>
    /// <example>"pickaxe", "skinning-knife", "axe"</example>
    public string? RequiredToolId { get; private set; }

    /// <summary>
    /// Gets the number of turns until this feature replenishes after depletion.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A value of 0 means the feature never replenishes (one-time harvest only).
    /// Features that replenish will restore their quantity after the specified
    /// number of turns have passed.
    /// </para>
    /// <para>
    /// Typical values:
    /// <list type="bullet">
    ///   <item><description>0 - Never replenishes (ore veins, gem clusters)</description></item>
    ///   <item><description>50-100 - Fast replenishment (common herbs)</description></item>
    ///   <item><description>100-200 - Slow replenishment (rare herbs, magical plants)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int ReplenishTurns { get; private set; }

    /// <summary>
    /// Gets the optional path to the feature's icon for UI display.
    /// </summary>
    /// <remarks>
    /// Path should be relative to the assets directory.
    /// </remarks>
    /// <example>"icons/features/ore_vein.png", "icons/features/herb_patch.png"</example>
    public string? IconPath { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for factory method and JSON deserialization.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Create"/> factory method to create new instances.
    /// </remarks>
    private HarvestableFeatureDefinition() { }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new harvestable feature definition with the specified properties.
    /// </summary>
    /// <param name="featureId">Unique string identifier (will be lowercased).</param>
    /// <param name="name">Display name shown in room descriptions.</param>
    /// <param name="description">Description text shown when examining.</param>
    /// <param name="resourceId">ID of the resource yielded (will be lowercased).</param>
    /// <param name="minQuantity">Minimum yield quantity (must be >= 0).</param>
    /// <param name="maxQuantity">Maximum yield quantity (must be >= minQuantity).</param>
    /// <param name="difficultyClass">DC for gathering check (must be > 0).</param>
    /// <param name="requiredToolId">Optional tool required for gathering (will be lowercased if provided).</param>
    /// <param name="replenishTurns">Turns to replenish after depletion (0 = never).</param>
    /// <returns>A new <see cref="HarvestableFeatureDefinition"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when featureId, name, or resourceId is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when:
    /// <list type="bullet">
    ///   <item><description>minQuantity is negative</description></item>
    ///   <item><description>maxQuantity is less than minQuantity</description></item>
    ///   <item><description>difficultyClass is not positive</description></item>
    ///   <item><description>replenishTurns is negative</description></item>
    /// </list>
    /// </exception>
    /// <example>
    /// <code>
    /// // Create a gold ore vein requiring a pickaxe
    /// var goldVein = HarvestableFeatureDefinition.Create(
    ///     "gold-ore-vein",
    ///     "Gold Ore Vein",
    ///     "A precious gold vein gleaming in the dark",
    ///     "gold-ore",
    ///     minQuantity: 1,
    ///     maxQuantity: 3,
    ///     difficultyClass: 15,
    ///     requiredToolId: "pickaxe");
    /// </code>
    /// </example>
    public static HarvestableFeatureDefinition Create(
        string featureId,
        string name,
        string description,
        string resourceId,
        int minQuantity,
        int maxQuantity,
        int difficultyClass,
        string? requiredToolId = null,
        int replenishTurns = 0)
    {
        // Validate required string parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(featureId, nameof(featureId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId, nameof(resourceId));

        // Validate numeric parameters
        ArgumentOutOfRangeException.ThrowIfNegative(minQuantity, nameof(minQuantity));
        ArgumentOutOfRangeException.ThrowIfLessThan(maxQuantity, minQuantity, nameof(maxQuantity));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(difficultyClass, nameof(difficultyClass));
        ArgumentOutOfRangeException.ThrowIfNegative(replenishTurns, nameof(replenishTurns));

        return new HarvestableFeatureDefinition
        {
            Id = Guid.NewGuid(),
            FeatureId = featureId.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            ResourceId = resourceId.ToLowerInvariant(),
            MinQuantity = minQuantity,
            MaxQuantity = maxQuantity,
            DifficultyClass = difficultyClass,
            RequiredToolId = requiredToolId?.ToLowerInvariant(),
            ReplenishTurns = replenishTurns
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // FLUENT BUILDER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Sets the icon path for this feature.
    /// </summary>
    /// <param name="iconPath">Path to the icon file relative to assets directory.</param>
    /// <returns>This instance for fluent chaining.</returns>
    /// <example>
    /// <code>
    /// var feature = HarvestableFeatureDefinition.Create(...)
    ///     .WithIcon("icons/features/ore_vein.png");
    /// </code>
    /// </example>
    public HarvestableFeatureDefinition WithIcon(string iconPath)
    {
        IconPath = iconPath;
        return this;
    }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this feature requires a tool to harvest.
    /// </summary>
    /// <remarks>
    /// Returns true if <see cref="RequiredToolId"/> is not null or empty.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (feature.RequiresTool)
    /// {
    ///     Console.WriteLine($"Requires: {feature.RequiredToolId}");
    /// }
    /// </code>
    /// </example>
    public bool RequiresTool => !string.IsNullOrEmpty(RequiredToolId);

    /// <summary>
    /// Gets whether this feature replenishes over time.
    /// </summary>
    /// <remarks>
    /// Returns true if <see cref="ReplenishTurns"/> is greater than 0.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (feature.Replenishes)
    /// {
    ///     Console.WriteLine($"Replenishes in {feature.ReplenishTurns} turns");
    /// }
    /// </code>
    /// </example>
    public bool Replenishes => ReplenishTurns > 0;

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the expected yield range as a display string.
    /// </summary>
    /// <returns>
    /// Formatted string like "1-5" or "3" if min equals max.
    /// </returns>
    /// <example>
    /// <code>
    /// var range = feature.GetYieldRangeDisplay();
    /// Console.WriteLine($"Yields: {range} {resourceName}");
    /// // Output: "Yields: 1-5 Iron Ore" or "Yields: 3 Iron Ore"
    /// </code>
    /// </example>
    public string GetYieldRangeDisplay()
    {
        return MinQuantity == MaxQuantity
            ? MinQuantity.ToString()
            : $"{MinQuantity}-{MaxQuantity}";
    }

    /// <summary>
    /// Gets a summary of gathering requirements for display.
    /// </summary>
    /// <returns>
    /// Formatted string with DC, yield, and optional tool/replenish info.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Format examples:
    /// <list type="bullet">
    ///   <item><description>"DC: 12 | Yield: 1-5"</description></item>
    ///   <item><description>"DC: 15 | Yield: 1-3 | Requires: pickaxe"</description></item>
    ///   <item><description>"DC: 10 | Yield: 2-6 | Replenishes: 100 turns"</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var summary = feature.GetRequirementsSummary();
    /// Console.WriteLine($"[{feature.Name}] {summary}");
    /// </code>
    /// </example>
    public string GetRequirementsSummary()
    {
        var summary = $"DC: {DifficultyClass} | Yield: {GetYieldRangeDisplay()}";

        if (RequiresTool)
        {
            summary += $" | Requires: {RequiredToolId}";
        }

        if (Replenishes)
        {
            summary += $" | Replenishes: {ReplenishTurns} turns";
        }

        return summary;
    }

    /// <summary>
    /// Returns a display string for this feature definition.
    /// </summary>
    /// <returns>Formatted string showing name, ID, and resource.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine(feature.ToString());
    /// // Output: "Iron Ore Vein (iron-ore-vein) - iron-ore"
    /// </code>
    /// </example>
    public override string ToString()
    {
        return $"{Name} ({FeatureId}) - {ResourceId}";
    }
}
