using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Repository for descriptor data access.
/// Abstracts data source (seeders, database, JSON files).
/// </summary>
public interface IDescriptorRepository
{
    /// <summary>
    /// Gets a base template by archetype.
    /// </summary>
    /// <param name="archetype">The room archetype to find a template for.</param>
    /// <returns>A matching template or null if not found.</returns>
    BaseDescriptorTemplate? GetBaseTemplate(RoomArchetype archetype);

    /// <summary>
    /// Gets all base templates.
    /// </summary>
    /// <returns>All available base descriptor templates.</returns>
    IReadOnlyList<BaseDescriptorTemplate> GetAllBaseTemplates();

    /// <summary>
    /// Gets the thematic modifier for a biome.
    /// </summary>
    /// <param name="biome">The biome to get the modifier for.</param>
    /// <returns>The thematic modifier for the biome.</returns>
    ThematicModifier GetModifier(Biome biome);

    /// <summary>
    /// Gets all thematic modifiers.
    /// </summary>
    /// <returns>All available thematic modifiers.</returns>
    IReadOnlyList<ThematicModifier> GetAllModifiers();

    /// <summary>
    /// Gets fragments filtered by category and optional biome/tags.
    /// </summary>
    /// <param name="category">The fragment category.</param>
    /// <param name="biomeAffinity">Optional biome filter (null = all biomes).</param>
    /// <param name="tags">Optional tag filter (null = all tags).</param>
    /// <returns>Matching descriptor fragments.</returns>
    IReadOnlyList<DescriptorFragment> GetFragments(
        FragmentCategory category,
        Biome? biomeAffinity = null,
        IEnumerable<string>? tags = null);

    /// <summary>
    /// Gets fragments filtered by category and subcategory.
    /// </summary>
    /// <param name="category">The fragment category.</param>
    /// <param name="subcategory">The subcategory string.</param>
    /// <param name="biomeAffinity">Optional biome filter.</param>
    /// <returns>Matching descriptor fragments.</returns>
    IReadOnlyList<DescriptorFragment> GetFragmentsBySubcategory(
        FragmentCategory category,
        string subcategory,
        Biome? biomeAffinity = null);

    /// <summary>
    /// Gets a room function by name.
    /// </summary>
    /// <param name="functionName">The function name to find.</param>
    /// <returns>The room function or null if not found.</returns>
    RoomFunction? GetFunction(string functionName);

    /// <summary>
    /// Gets room functions compatible with a biome.
    /// </summary>
    /// <param name="biome">The biome to get functions for.</param>
    /// <returns>Room functions with affinity for the biome.</returns>
    IReadOnlyList<RoomFunction> GetFunctionsByBiome(Biome biome);

    /// <summary>
    /// Gets all room functions.
    /// </summary>
    /// <returns>All available room functions.</returns>
    IReadOnlyList<RoomFunction> GetAllFunctions();
}
