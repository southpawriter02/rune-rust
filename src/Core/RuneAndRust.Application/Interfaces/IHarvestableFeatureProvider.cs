using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides access to harvestable feature definitions loaded from configuration.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts the loading and retrieval of harvestable feature
/// definitions, allowing for different implementations (JSON, database, etc.)
/// while maintaining a consistent API for the application layer.
/// </para>
/// <para>
/// Feature definitions are loaded once at construction and cached for the
/// lifetime of the provider. Resource ID references are validated during
/// loading to ensure all features reference valid resources.
/// </para>
/// <para>
/// Key capabilities:
/// <list type="bullet">
///   <item><description>Retrieve features by ID (case-insensitive)</description></item>
///   <item><description>Filter features by resource, tool, or difficulty</description></item>
///   <item><description>Create runtime instances from definitions</description></item>
///   <item><description>Check feature existence</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get a specific feature definition
/// var oreVein = featureProvider.GetFeature("iron-ore-vein");
/// if (oreVein is not null)
/// {
///     Console.WriteLine($"Found: {oreVein.Name}, DC: {oreVein.DifficultyClass}");
/// }
///
/// // Get all features that yield a specific resource
/// var ironFeatures = featureProvider.GetFeaturesByResource("iron-ore");
///
/// // Create a runtime instance for a room
/// var feature = featureProvider.CreateFeatureInstance("herb-patch");
/// if (feature is not null)
/// {
///     room.AddHarvestableFeature(feature);
/// }
/// </code>
/// </example>
public interface IHarvestableFeatureProvider
{
    // ═══════════════════════════════════════════════════════════════
    // RETRIEVAL METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a harvestable feature definition by its unique string identifier.
    /// </summary>
    /// <param name="featureId">The feature identifier (case-insensitive).</param>
    /// <returns>The feature definition, or null if not found.</returns>
    /// <remarks>
    /// <para>
    /// Lookups are case-insensitive, so "Iron-Ore-Vein", "iron-ore-vein",
    /// and "IRON-ORE-VEIN" will all return the same definition.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var oreVein = featureProvider.GetFeature("iron-ore-vein");
    /// if (oreVein is not null)
    /// {
    ///     Console.WriteLine($"Found: {oreVein.Name}, DC: {oreVein.DifficultyClass}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Feature not found");
    /// }
    /// </code>
    /// </example>
    HarvestableFeatureDefinition? GetFeature(string featureId);

    /// <summary>
    /// Gets all registered harvestable feature definitions.
    /// </summary>
    /// <returns>A read-only list of all feature definitions.</returns>
    /// <remarks>
    /// <para>
    /// Returns all features loaded from configuration, regardless of
    /// resource type, difficulty, or tool requirements.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var allFeatures = featureProvider.GetAllFeatures();
    /// foreach (var feature in allFeatures)
    /// {
    ///     Console.WriteLine($"- {feature.Name} (DC: {feature.DifficultyClass})");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<HarvestableFeatureDefinition> GetAllFeatures();

    // ═══════════════════════════════════════════════════════════════
    // FILTERING METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all features that yield a specific resource.
    /// </summary>
    /// <param name="resourceId">The resource identifier to filter by (case-insensitive).</param>
    /// <returns>A read-only list of matching feature definitions.</returns>
    /// <remarks>
    /// <para>
    /// Useful for finding all sources of a particular resource type.
    /// For example, finding all features that yield iron ore.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var ironFeatures = featureProvider.GetFeaturesByResource("iron-ore");
    /// Console.WriteLine($"Found {ironFeatures.Count} iron ore sources:");
    /// foreach (var feature in ironFeatures)
    /// {
    ///     Console.WriteLine($"  - {feature.Name}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<HarvestableFeatureDefinition> GetFeaturesByResource(string resourceId);

    /// <summary>
    /// Gets all features that require a specific tool.
    /// </summary>
    /// <param name="toolId">The tool identifier to filter by (case-insensitive).</param>
    /// <returns>A read-only list of matching feature definitions.</returns>
    /// <remarks>
    /// <para>
    /// Returns only features where <see cref="HarvestableFeatureDefinition.RequiredToolId"/>
    /// matches the provided tool ID.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Find all features that need a pickaxe
    /// var pickaxeFeatures = featureProvider.GetFeaturesByTool("pickaxe");
    /// Console.WriteLine($"{pickaxeFeatures.Count} features require a pickaxe");
    /// </code>
    /// </example>
    IReadOnlyList<HarvestableFeatureDefinition> GetFeaturesByTool(string toolId);

    /// <summary>
    /// Gets all features within a difficulty class range.
    /// </summary>
    /// <param name="minDC">Minimum difficulty class (inclusive).</param>
    /// <param name="maxDC">Maximum difficulty class (inclusive).</param>
    /// <returns>A read-only list of matching feature definitions.</returns>
    /// <remarks>
    /// <para>
    /// Useful for spawning features appropriate to player level or area difficulty.
    /// </para>
    /// <para>
    /// Both minDC and maxDC are inclusive, so GetFeaturesByDifficulty(10, 12)
    /// returns features with DC 10, 11, or 12.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get easy features for beginner area (DC 10-12)
    /// var easyFeatures = featureProvider.GetFeaturesByDifficulty(10, 12);
    ///
    /// // Get hard features for advanced area (DC 15-18)
    /// var hardFeatures = featureProvider.GetFeaturesByDifficulty(15, 18);
    /// </code>
    /// </example>
    IReadOnlyList<HarvestableFeatureDefinition> GetFeaturesByDifficulty(int minDC, int maxDC);

    // ═══════════════════════════════════════════════════════════════
    // EXISTENCE AND ENUMERATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a feature with the specified ID exists.
    /// </summary>
    /// <param name="featureId">The feature identifier to check (case-insensitive).</param>
    /// <returns>True if the feature exists, false otherwise.</returns>
    /// <remarks>
    /// <para>
    /// More efficient than calling GetFeature and checking for null when
    /// you only need to verify existence.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (featureProvider.Exists("iron-ore-vein"))
    /// {
    ///     // Feature is available for spawning
    /// }
    /// </code>
    /// </example>
    bool Exists(string featureId);

    /// <summary>
    /// Gets all registered feature IDs.
    /// </summary>
    /// <returns>A read-only list of all feature identifiers.</returns>
    /// <remarks>
    /// <para>
    /// Useful for iterating over all available features without
    /// loading the full definition objects.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var featureIds = featureProvider.GetFeatureIds();
    /// Console.WriteLine($"Available features: {string.Join(", ", featureIds)}");
    /// </code>
    /// </example>
    IReadOnlyList<string> GetFeatureIds();

    /// <summary>
    /// Gets the total count of registered features.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns the number of feature definitions loaded from configuration.
    /// </para>
    /// </remarks>
    int Count { get; }

    // ═══════════════════════════════════════════════════════════════
    // INSTANCE CREATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new harvestable feature instance from a definition.
    /// </summary>
    /// <param name="featureId">The feature definition ID.</param>
    /// <param name="random">Optional random generator for quantity. Uses <see cref="Random.Shared"/> if null.</param>
    /// <returns>A new feature instance, or null if definition not found.</returns>
    /// <remarks>
    /// <para>
    /// This is a convenience method that combines:
    /// <list type="number">
    ///   <item><description>Looking up the definition by ID</description></item>
    ///   <item><description>Creating a feature instance with random quantity</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The created instance has its quantity randomly set between the
    /// definition's MinQuantity and MaxQuantity (inclusive).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Spawn a feature in a room
    /// var feature = featureProvider.CreateFeatureInstance("iron-ore-vein");
    /// if (feature is not null)
    /// {
    ///     room.AddHarvestableFeature(feature);
    ///     Console.WriteLine($"Spawned {feature.Name} with {feature.RemainingQuantity} resources");
    /// }
    /// </code>
    /// </example>
    HarvestableFeature? CreateFeatureInstance(string featureId, Random? random = null);
}
