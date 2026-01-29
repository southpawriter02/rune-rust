// ═══════════════════════════════════════════════════════════════════════════════
// IEquipmentClassMappingProvider.cs
// Interface for accessing equipment-to-class mapping data.
// Version: 0.16.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides access to equipment class mapping data for smart loot generation.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts access to equipment-to-class affinity mappings,
/// enabling the smart loot system to filter drops appropriately for the
/// player's archetype.
/// </para>
/// <para>
/// Implementations typically load mapping data from JSON configuration files
/// and cache the results for efficient lookups during loot generation.
/// </para>
/// <para>
/// Key operations include:
/// <list type="bullet">
///   <item><description>Looking up the affinity for a specific equipment category</description></item>
///   <item><description>Finding all categories appropriate for a given archetype</description></item>
///   <item><description>Checking if a category is appropriate for an archetype</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="EquipmentClassMapping"/>
/// <seealso cref="EquipmentClassAffinity"/>
public interface IEquipmentClassMappingProvider
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Single Mapping Retrieval
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the mapping for a specific category.
    /// </summary>
    /// <param name="categoryId">The equipment category identifier (e.g., "axes", "daggers").</param>
    /// <returns>
    /// The <see cref="EquipmentClassMapping"/> for the specified category,
    /// or <see cref="EquipmentClassMapping.Empty"/> if the category is not found.
    /// </returns>
    /// <remarks>
    /// Category IDs are normalized to lowercase for case-insensitive matching.
    /// </remarks>
    EquipmentClassMapping GetMapping(string categoryId);

    /// <summary>
    /// Gets the affinity for a category.
    /// </summary>
    /// <param name="categoryId">The equipment category identifier.</param>
    /// <returns>
    /// The <see cref="EquipmentClassAffinity"/> for the specified category,
    /// or <see cref="EquipmentClassAffinity.Universal"/> if the category is not found.
    /// </returns>
    EquipmentClassAffinity GetAffinityForCategory(string categoryId);

    // ═══════════════════════════════════════════════════════════════════════════
    // Bulk Mapping Retrieval
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all mappings for a specific affinity.
    /// </summary>
    /// <param name="affinity">The equipment class affinity to filter by.</param>
    /// <returns>A read-only list of mappings with the specified affinity.</returns>
    IReadOnlyList<EquipmentClassMapping> GetMappingsForAffinity(EquipmentClassAffinity affinity);

    /// <summary>
    /// Gets all configured mappings.
    /// </summary>
    /// <returns>A read-only list of all equipment class mappings.</returns>
    IReadOnlyList<EquipmentClassMapping> GetAllMappings();

    /// <summary>
    /// Gets categories appropriate for an archetype (includes Universal).
    /// </summary>
    /// <param name="archetypeId">The archetype to filter for (e.g., "warrior", "mystic").</param>
    /// <returns>
    /// A read-only list of mappings that are appropriate for the specified archetype.
    /// This includes both archetype-specific categories and <see cref="EquipmentClassAffinity.Universal"/> categories.
    /// </returns>
    /// <remarks>
    /// <para>
    /// For example, calling <c>GetAppropriateCategories("mystic")</c> returns:
    /// <list type="bullet">
    ///   <item><description>Mystic-affinity weapons (staves, foci, orbs)</description></item>
    ///   <item><description>Universal categories (light armor, accessories)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    IReadOnlyList<EquipmentClassMapping> GetAppropriateCategories(string archetypeId);

    // ═══════════════════════════════════════════════════════════════════════════
    // Validation Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a category is appropriate for an archetype.
    /// </summary>
    /// <param name="categoryId">The equipment category identifier.</param>
    /// <param name="archetypeId">The archetype to check against.</param>
    /// <returns>
    /// <c>true</c> if the category is appropriate for the archetype
    /// (including Universal categories); otherwise, <c>false</c>.
    /// </returns>
    bool IsClassAppropriate(string categoryId, string archetypeId);

    /// <summary>
    /// Validates configuration completeness.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the configuration is valid and complete;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Validation checks include:
    /// <list type="bullet">
    ///   <item><description>At least one mapping exists</description></item>
    ///   <item><description>All required fields are populated</description></item>
    ///   <item><description>No duplicate category IDs</description></item>
    /// </list>
    /// </remarks>
    bool ValidateConfiguration();
}
