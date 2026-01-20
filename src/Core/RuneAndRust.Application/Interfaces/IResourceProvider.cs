using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides access to resource definitions loaded from configuration.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts the loading and retrieval of resource definitions,
/// allowing for different implementations (JSON, database, etc.) while
/// maintaining a consistent API for the application layer.
/// </para>
/// <para>
/// Resource providers are typically registered as singletons in the DI container,
/// loading all definitions once at startup and caching them for efficient access.
/// </para>
/// <para>
/// Key features:
/// <list type="bullet">
///   <item><description>Case-insensitive resource ID lookups</description></item>
///   <item><description>Filtering by category for recipe matching</description></item>
///   <item><description>Filtering by quality tier</description></item>
///   <item><description>Combined category and minimum quality filtering</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get a specific resource
/// var ironOre = resourceProvider.GetResource("iron-ore");
///
/// // Get all ore resources
/// var ores = resourceProvider.GetResourcesByCategory(ResourceCategory.Ore);
///
/// // Get all rare or better quality resources
/// var rareResources = resourceProvider.GetResourcesByQuality(ResourceQuality.Rare);
///
/// // Get fine or better herbs
/// var qualityHerbs = resourceProvider.GetResources(ResourceCategory.Herb, ResourceQuality.Fine);
/// </code>
/// </example>
public interface IResourceProvider
{
    /// <summary>
    /// Gets a resource definition by its unique string identifier.
    /// </summary>
    /// <param name="resourceId">The resource identifier (case-insensitive).</param>
    /// <returns>The resource definition, or null if not found.</returns>
    /// <example>
    /// <code>
    /// var ironOre = resourceProvider.GetResource("iron-ore");
    /// if (ironOre is not null)
    /// {
    ///     Console.WriteLine($"Found: {ironOre.Name}, Value: {ironOre.GetActualValue()}");
    /// }
    /// </code>
    /// </example>
    ResourceDefinition? GetResource(string resourceId);

    /// <summary>
    /// Gets all registered resource definitions.
    /// </summary>
    /// <returns>A read-only list of all resource definitions.</returns>
    /// <remarks>
    /// The returned list is a snapshot and will not reflect any subsequent changes
    /// if the provider supports dynamic reloading.
    /// </remarks>
    IReadOnlyList<ResourceDefinition> GetAllResources();

    /// <summary>
    /// Gets all resources belonging to a specific category.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    /// <returns>A read-only list of matching resource definitions.</returns>
    /// <example>
    /// <code>
    /// var ores = resourceProvider.GetResourcesByCategory(ResourceCategory.Ore);
    /// foreach (var ore in ores)
    /// {
    ///     Console.WriteLine($"- {ore.Name}: {ore.GetActualValue()} gold");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<ResourceDefinition> GetResourcesByCategory(ResourceCategory category);

    /// <summary>
    /// Gets all resources of a specific quality tier.
    /// </summary>
    /// <param name="quality">The quality tier to filter by.</param>
    /// <returns>A read-only list of matching resource definitions.</returns>
    /// <example>
    /// <code>
    /// var rareResources = resourceProvider.GetResourcesByQuality(ResourceQuality.Rare);
    /// Console.WriteLine($"Found {rareResources.Count} rare resources");
    /// </code>
    /// </example>
    IReadOnlyList<ResourceDefinition> GetResourcesByQuality(ResourceQuality quality);

    /// <summary>
    /// Gets all resources matching both category and minimum quality.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    /// <param name="minimumQuality">The minimum quality tier required.</param>
    /// <returns>A read-only list of matching resource definitions.</returns>
    /// <remarks>
    /// This method returns resources where Quality >= minimumQuality,
    /// so requesting Fine will also include Rare and Legendary.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get all herbs that are at least Fine quality
    /// var qualityHerbs = resourceProvider.GetResources(
    ///     ResourceCategory.Herb,
    ///     ResourceQuality.Fine);
    /// </code>
    /// </example>
    IReadOnlyList<ResourceDefinition> GetResources(ResourceCategory category, ResourceQuality minimumQuality);

    /// <summary>
    /// Checks if a resource with the specified ID exists.
    /// </summary>
    /// <param name="resourceId">The resource identifier to check.</param>
    /// <returns>True if the resource exists, false otherwise.</returns>
    /// <example>
    /// <code>
    /// if (resourceProvider.Exists("dragon-scale"))
    /// {
    ///     Console.WriteLine("Dragon scales are available!");
    /// }
    /// </code>
    /// </example>
    bool Exists(string resourceId);

    /// <summary>
    /// Gets all registered resource IDs.
    /// </summary>
    /// <returns>A read-only list of all resource identifiers.</returns>
    /// <remarks>
    /// Useful for validation, auto-completion, or displaying available options.
    /// </remarks>
    IReadOnlyList<string> GetResourceIds();

    /// <summary>
    /// Gets the total count of registered resources.
    /// </summary>
    /// <returns>The number of resource definitions.</returns>
    int Count { get; }
}
