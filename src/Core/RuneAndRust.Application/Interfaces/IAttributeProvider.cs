// ═══════════════════════════════════════════════════════════════════════════════
// IAttributeProvider.cs
// Interface providing access to attribute definitions, recommended builds,
// and point-buy configuration for character creation.
// Version: 0.17.2e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.Exceptions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides access to attribute definitions, recommended builds, and point-buy configuration.
/// </summary>
/// <remarks>
/// <para>
/// IAttributeProvider is the central interface for all attribute-related data access
/// during character creation. Implementations load data from configuration files
/// (attributes.json) and cache it in memory for efficient retrieval.
/// All methods are synchronous as the data is loaded once and cached.
/// </para>
/// <para>
/// The provider exposes three categories of data:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Attribute Descriptions:</strong> Display information for each of the 5 core
///       attributes (Might, Finesse, Wits, Will, Sturdiness), including tooltips and
///       relationship mappings to derived stats and skills.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Recommended Builds:</strong> Pre-configured attribute allocations for Simple
///       mode, one per archetype (Warrior, Skirmisher, Mystic, Adept).
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Point-Buy Configuration:</strong> Starting point pools, attribute value
///       constraints, and tiered cost table for Advanced mode allocation.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Thread Safety:</strong> Implementations should be thread-safe for
/// concurrent reads. Configuration is loaded once on first access and never
/// modified thereafter.
/// </para>
/// <para>
/// <strong>Usage Examples:</strong>
/// <list type="bullet">
///   <item><description>Attribute allocation UI: <see cref="GetAllAttributeDescriptions"/> to display attribute tooltips</description></item>
///   <item><description>Simple mode selection: <see cref="GetRecommendedBuild"/> to apply archetype defaults</description></item>
///   <item><description>Advanced mode setup: <see cref="GetPointBuyConfiguration"/> to initialize point-buy UI</description></item>
///   <item><description>Point pool display: <see cref="GetStartingPoints"/> to show available points</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="AttributeDescription"/>
/// <seealso cref="AttributeAllocationState"/>
/// <seealso cref="PointBuyConfiguration"/>
/// <seealso cref="CoreAttribute"/>
public interface IAttributeProvider
{
    /// <summary>
    /// Gets the description for a specific core attribute.
    /// </summary>
    /// <param name="attribute">The core attribute to get the description for.</param>
    /// <returns>
    /// The attribute description containing display name, descriptions, and
    /// relationship mappings to derived stats and skills.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="attribute"/> is not found in the loaded configuration.
    /// This should not occur in normal operation as configuration is validated on startup.
    /// </exception>
    /// <exception cref="AttributeConfigurationException">
    /// Thrown if configuration cannot be loaded or validated.
    /// </exception>
    /// <example>
    /// <code>
    /// var description = attributeProvider.GetAttributeDescription(CoreAttribute.Might);
    /// Console.WriteLine($"{description.DisplayName}: {description.ShortDescription}");
    /// // Output: MIGHT: Physical power and raw strength
    /// </code>
    /// </example>
    AttributeDescription GetAttributeDescription(CoreAttribute attribute);

    /// <summary>
    /// Gets descriptions for all core attributes.
    /// </summary>
    /// <returns>
    /// A read-only list of all attribute descriptions, one for each
    /// <see cref="CoreAttribute"/> enum value. The list is guaranteed to contain
    /// exactly 5 descriptions when configuration is valid.
    /// </returns>
    /// <exception cref="AttributeConfigurationException">
    /// Thrown if configuration cannot be loaded or validated.
    /// </exception>
    /// <example>
    /// <code>
    /// var descriptions = attributeProvider.GetAllAttributeDescriptions();
    /// foreach (var desc in descriptions)
    /// {
    ///     Console.WriteLine($"{desc.DisplayName}: {desc.ShortDescription}");
    ///     Console.WriteLine($"  Affects Stats: {desc.GetAffectedStatsSummary()}");
    ///     Console.WriteLine($"  Affects Skills: {desc.GetAffectedSkillsSummary()}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<AttributeDescription> GetAllAttributeDescriptions();

    /// <summary>
    /// Gets the recommended attribute build for an archetype (Simple mode).
    /// </summary>
    /// <param name="archetypeId">
    /// The archetype identifier (e.g., "warrior", "skirmisher", "mystic", "adept").
    /// Comparison is case-insensitive.
    /// </param>
    /// <returns>
    /// An <see cref="AttributeAllocationState"/> in Simple mode with the archetype's
    /// recommended attribute values pre-applied and all points spent.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="archetypeId"/> is null, empty, whitespace,
    /// or does not match any configured archetype.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Recommended builds are used by Simple mode to provide new players with
    /// optimized attribute allocations for each archetype. The returned state
    /// has <see cref="AttributeAllocationState.IsComplete"/> set to <c>true</c>
    /// and <see cref="AttributeAllocationState.AllowsManualAdjustment"/> set to <c>false</c>.
    /// </para>
    /// <para>
    /// Standard recommended builds:
    /// <list type="table">
    ///   <listheader><term>Archetype</term><description>Build (M/F/Wi/Wl/S)</description></listheader>
    ///   <item><term>Warrior</term><description>4/3/2/2/4 (15 pts)</description></item>
    ///   <item><term>Skirmisher</term><description>3/4/3/2/3 (15 pts)</description></item>
    ///   <item><term>Mystic</term><description>2/3/4/4/2 (15 pts)</description></item>
    ///   <item><term>Adept</term><description>3/3/3/2/3 (14 pts)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var warriorBuild = attributeProvider.GetRecommendedBuild("warrior");
    /// // warriorBuild.CurrentMight == 4
    /// // warriorBuild.CurrentSturdiness == 4
    /// // warriorBuild.IsComplete == true
    /// // warriorBuild.Mode == AttributeAllocationMode.Simple
    /// </code>
    /// </example>
    AttributeAllocationState GetRecommendedBuild(string archetypeId);

    /// <summary>
    /// Gets the point-buy configuration for Advanced mode.
    /// </summary>
    /// <returns>
    /// A <see cref="PointBuyConfiguration"/> containing starting point pools,
    /// attribute value constraints (min 1, max 10), and the tiered cost table
    /// (1 point for values 2-8, 2 points for values 9-10).
    /// </returns>
    /// <exception cref="AttributeConfigurationException">
    /// Thrown if configuration cannot be loaded or validated.
    /// </exception>
    /// <example>
    /// <code>
    /// var config = attributeProvider.GetPointBuyConfiguration();
    /// Console.WriteLine($"Starting Points: {config.StartingPoints}");
    /// Console.WriteLine($"Adept Points: {config.AdeptStartingPoints}");
    /// Console.WriteLine($"Range: {config.MinAttributeValue}-{config.MaxAttributeValue}");
    /// Console.WriteLine($"Cost Table: {config.CostTableEntryCount} entries");
    /// // Output:
    /// // Starting Points: 15
    /// // Adept Points: 14
    /// // Range: 1-10
    /// // Cost Table: 9 entries
    /// </code>
    /// </example>
    PointBuyConfiguration GetPointBuyConfiguration();

    /// <summary>
    /// Gets the starting points for attribute allocation.
    /// </summary>
    /// <param name="archetypeId">
    /// Optional archetype identifier. When "adept" (case-insensitive), returns 14 points.
    /// For all other archetypes or when <c>null</c>, returns the standard 15 points.
    /// </param>
    /// <returns>
    /// The number of starting points available for attribute allocation.
    /// Returns 14 for the Adept archetype, 15 for all others.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The Adept archetype receives 1 fewer starting point (14 instead of 15) to balance
    /// their broader utility abilities and +20% consumable effectiveness bonus.
    /// All other archetypes (Warrior, Skirmisher, Mystic) share the standard pool of 15 points.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var defaultPoints = attributeProvider.GetStartingPoints();       // 15
    /// var warriorPoints = attributeProvider.GetStartingPoints("warrior"); // 15
    /// var adeptPoints = attributeProvider.GetStartingPoints("adept");     // 14
    /// var adeptUpper = attributeProvider.GetStartingPoints("ADEPT");      // 14 (case-insensitive)
    /// </code>
    /// </example>
    int GetStartingPoints(string? archetypeId = null);
}
