// ═══════════════════════════════════════════════════════════════════════════════
// ILineageProvider.cs
// Interface providing access to lineage definitions and their components.
// Version: 0.17.0e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.Exceptions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides access to lineage definitions and their components.
/// </summary>
/// <remarks>
/// <para>
/// ILineageProvider is the primary interface for accessing lineage data.
/// Implementations load lineage definitions from configuration and cache
/// them for efficient access. All methods are synchronous as the data
/// is loaded once and cached in memory.
/// </para>
/// <para>
/// The provider exposes both full LineageDefinition access and convenience
/// methods for accessing individual components (attributes, bonuses, traits,
/// trauma baseline). This allows consumers to request only the data they need.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> Implementations should be thread-safe for
/// concurrent reads. Configuration is loaded once on first access and never
/// modified thereafter.
/// </para>
/// <para>
/// <strong>Usage Examples:</strong>
/// <list type="bullet">
///   <item><description>Character creation UI: GetAllLineages() to display options</description></item>
///   <item><description>Character factory: GetLineage() to apply full lineage</description></item>
///   <item><description>Combat service: GetUniqueTrait() for trait-based bonuses</description></item>
///   <item><description>Trauma system: GetTraumaBaseline() for resistance modifiers</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="LineageDefinition"/>
/// <seealso cref="Lineage"/>
public interface ILineageProvider
{
    /// <summary>
    /// Gets all available lineage definitions.
    /// </summary>
    /// <returns>
    /// A read-only list of all lineage definitions, one for each <see cref="Lineage"/>
    /// enum value. The list is guaranteed to contain exactly 4 lineages when
    /// configuration is valid.
    /// </returns>
    /// <exception cref="LineageConfigurationException">
    /// Thrown if configuration cannot be loaded or validated.
    /// </exception>
    /// <example>
    /// <code>
    /// var lineages = lineageProvider.GetAllLineages();
    /// foreach (var lineage in lineages)
    /// {
    ///     Console.WriteLine($"{lineage.DisplayName}: {lineage.SelectionText}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<LineageDefinition> GetAllLineages();

    /// <summary>
    /// Gets the lineage definition for a specific lineage.
    /// </summary>
    /// <param name="lineage">The lineage to retrieve.</param>
    /// <returns>
    /// The lineage definition, or <c>null</c> if not found (should not happen
    /// in normal operation as configuration is validated on startup).
    /// </returns>
    /// <example>
    /// <code>
    /// var clanBorn = lineageProvider.GetLineage(Lineage.ClanBorn);
    /// if (clanBorn != null)
    /// {
    ///     Console.WriteLine($"HP Bonus: {clanBorn.PassiveBonuses.MaxHpBonus}");
    /// }
    /// </code>
    /// </example>
    LineageDefinition? GetLineage(Lineage lineage);

    /// <summary>
    /// Gets the attribute modifiers for a specific lineage.
    /// </summary>
    /// <param name="lineage">The lineage to get modifiers for.</param>
    /// <returns>
    /// The <see cref="LineageAttributeModifiers"/> containing all attribute
    /// bonuses and penalties for the specified lineage.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the lineage is not found in the cache.
    /// </exception>
    /// <example>
    /// <code>
    /// var modifiers = lineageProvider.GetAttributeModifiers(Lineage.RuneMarked);
    /// Console.WriteLine($"WILL: {modifiers.WillModifier:+#;-#;0}");
    /// // Output: WILL: +2
    /// </code>
    /// </example>
    LineageAttributeModifiers GetAttributeModifiers(Lineage lineage);

    /// <summary>
    /// Gets the passive bonuses for a specific lineage.
    /// </summary>
    /// <param name="lineage">The lineage to get bonuses for.</param>
    /// <returns>
    /// The <see cref="LineagePassiveBonuses"/> containing HP, AP, Soak, Movement,
    /// and skill bonuses for the specified lineage.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the lineage is not found in the cache.
    /// </exception>
    /// <example>
    /// <code>
    /// var bonuses = lineageProvider.GetPassiveBonuses(Lineage.ClanBorn);
    /// Console.WriteLine($"Max HP Bonus: +{bonuses.MaxHpBonus}");
    /// // Output: Max HP Bonus: +5
    /// </code>
    /// </example>
    LineagePassiveBonuses GetPassiveBonuses(Lineage lineage);

    /// <summary>
    /// Gets the unique trait for a specific lineage.
    /// </summary>
    /// <param name="lineage">The lineage to get the trait for.</param>
    /// <returns>
    /// The <see cref="LineageTrait"/> representing the signature ability
    /// unique to this bloodline.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the lineage is not found in the cache.
    /// </exception>
    /// <example>
    /// <code>
    /// var trait = lineageProvider.GetUniqueTrait(Lineage.IronBlooded);
    /// Console.WriteLine($"{trait.TraitName}: {trait.Description}");
    /// // Output: [Hazard Acclimation]: Gain +1d10 to Sturdiness Resolve checks...
    /// </code>
    /// </example>
    LineageTrait GetUniqueTrait(Lineage lineage);

    /// <summary>
    /// Gets the trauma baseline for a specific lineage.
    /// </summary>
    /// <param name="lineage">The lineage to get the baseline for.</param>
    /// <returns>
    /// The <see cref="LineageTraumaBaseline"/> containing starting Corruption/Stress
    /// values and resistance modifiers for the specified lineage.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the lineage is not found in the cache.
    /// </exception>
    /// <example>
    /// <code>
    /// var baseline = lineageProvider.GetTraumaBaseline(Lineage.RuneMarked);
    /// Console.WriteLine($"Starting Corruption: {baseline.StartingCorruption}");
    /// Console.WriteLine($"Corruption Resist: {baseline.CorruptionResistanceModifier:+#;-#;0}");
    /// // Output: Starting Corruption: 5
    /// // Output: Corruption Resist: -1
    /// </code>
    /// </example>
    LineageTraumaBaseline GetTraumaBaseline(Lineage lineage);
}
