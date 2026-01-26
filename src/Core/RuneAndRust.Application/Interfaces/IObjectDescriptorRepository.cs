namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Repository for object examination descriptors.
/// </summary>
/// <remarks>
/// <para>
/// Provides data access for ObjectDescriptor entities with filtering by:
/// </para>
/// <list type="bullet">
///   <item><description>Object category (Door, Machinery, etc.)</description></item>
///   <item><description>Object type within category (LockedDoor, Console, etc.)</description></item>
///   <item><description>Examination layer (1, 2, or 3)</description></item>
/// </list>
/// <para>
/// Biome filtering is handled by the service layer, not the repository.
/// </para>
/// </remarks>
public interface IObjectDescriptorRepository
{
    /// <summary>
    /// Gets all descriptors in a category.
    /// </summary>
    /// <param name="category">The object category.</param>
    /// <returns>All descriptors in the category.</returns>
    /// <remarks>
    /// Returns all descriptors regardless of type or layer.
    /// Useful for category statistics or validation.
    /// </remarks>
    IReadOnlyList<ObjectDescriptor> GetByCategory(ObjectCategory category);

    /// <summary>
    /// Gets descriptors for a specific type and layer.
    /// </summary>
    /// <param name="category">The object category.</param>
    /// <param name="objectType">The specific object type.</param>
    /// <param name="layer">The examination layer (1, 2, or 3).</param>
    /// <returns>Matching descriptors (may include multiple variants).</returns>
    /// <remarks>
    /// Returns all descriptors matching the criteria, including
    /// both universal and biome-specific variants.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when objectType is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when layer is not 1, 2, or 3.</exception>
    IReadOnlyList<ObjectDescriptor> GetByTypeAndLayer(
        ObjectCategory category,
        string objectType,
        int layer);

    /// <summary>
    /// Gets all loaded descriptors.
    /// </summary>
    /// <returns>All descriptors in the repository.</returns>
    IReadOnlyList<ObjectDescriptor> GetAll();

    /// <summary>
    /// Gets the count of descriptors in the repository.
    /// </summary>
    /// <returns>The total number of descriptors.</returns>
    int Count { get; }

    /// <summary>
    /// Gets descriptors by object type across all layers.
    /// </summary>
    /// <param name="category">The object category.</param>
    /// <param name="objectType">The specific object type.</param>
    /// <returns>All descriptors for the type (all layers).</returns>
    /// <exception cref="ArgumentException">Thrown when objectType is null or whitespace.</exception>
    IReadOnlyList<ObjectDescriptor> GetByType(ObjectCategory category, string objectType);
}
