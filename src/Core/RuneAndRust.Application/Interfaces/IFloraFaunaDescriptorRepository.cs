namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Repository interface for loading and querying flora and fauna descriptors.
/// </summary>
/// <remarks>
/// <para>
/// Provides specialized data access for FloraFaunaDescriptor entities with
/// filtering by species name, category, and biome.
/// </para>
/// <para>
/// Unlike the general IExaminationRepository, this interface focuses specifically
/// on flora/fauna species queries for the observation system.
/// </para>
/// </remarks>
public interface IFloraFaunaDescriptorRepository
{
    /// <summary>
    /// Gets a species descriptor by its unique ID.
    /// </summary>
    /// <param name="descriptorId">The descriptor ID.</param>
    /// <returns>The descriptor, or null if not found.</returns>
    FloraFaunaDescriptor? GetById(Guid descriptorId);

    /// <summary>
    /// Gets all descriptors for a species by common name.
    /// </summary>
    /// <param name="speciesName">The species common name.</param>
    /// <returns>All layer descriptors for this species.</returns>
    /// <exception cref="ArgumentException">Thrown when speciesName is null or whitespace.</exception>
    IReadOnlyList<FloraFaunaDescriptor> GetBySpeciesName(string speciesName);

    /// <summary>
    /// Gets all descriptors matching a category (Flora or Fauna).
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    /// <returns>All descriptors in this category.</returns>
    IReadOnlyList<FloraFaunaDescriptor> GetByCategory(FloraFaunaCategory category);

    /// <summary>
    /// Gets all descriptors for a specific biome.
    /// </summary>
    /// <param name="biome">The biome.</param>
    /// <returns>All descriptors for this biome.</returns>
    IReadOnlyList<FloraFaunaDescriptor> GetByBiome(Biome biome);

    /// <summary>
    /// Gets all harvestable flora descriptors.
    /// </summary>
    /// <returns>All flora with harvest DC defined.</returns>
    IReadOnlyList<FloraFaunaDescriptor> GetHarvestable();

    /// <summary>
    /// Gets all loaded descriptors.
    /// </summary>
    /// <returns>All descriptors in the repository.</returns>
    IReadOnlyList<FloraFaunaDescriptor> GetAll();

    /// <summary>
    /// Gets the count of descriptors.
    /// </summary>
    int Count { get; }
}
