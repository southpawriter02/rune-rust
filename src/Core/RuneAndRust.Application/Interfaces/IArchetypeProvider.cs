// ═══════════════════════════════════════════════════════════════════════════════
// IArchetypeProvider.cs
// Interface defining the contract for accessing archetype definitions and
// related data from configuration. This is the central access point for all
// archetype-related data throughout the application.
// Version: 0.17.3e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides access to archetype definitions and related data.
/// </summary>
/// <remarks>
/// <para>
/// IArchetypeProvider is the central access point for all archetype-related
/// data. It aggregates definitions (v0.17.3a), resource bonuses (v0.17.3b),
/// starting abilities (v0.17.3c), and specialization mappings (v0.17.3d)
/// into a single cohesive API.
/// </para>
/// <para>
/// Implementations load data from the <c>archetypes.json</c> configuration file
/// and cache it for performance. All methods are safe to call multiple times
/// without performance penalty.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> Implementations must be thread-safe for
/// concurrent reads after initialization. The recommended pattern is
/// double-checked locking for lazy initialization.
/// </para>
/// <para>
/// <strong>Error Handling:</strong> Implementations throw
/// <see cref="Exceptions.ArchetypeConfigurationException"/> when configuration
/// loading or validation fails. Individual lookup methods return null or
/// empty collections for missing data rather than throwing.
/// </para>
/// <para>
/// <strong>Consumers:</strong>
/// <list type="bullet">
///   <item><description>Character Creation UI (v0.17.5) — displays archetype selection</description></item>
///   <item><description>Archetype Application Service (v0.17.3f) — applies archetype to characters</description></item>
///   <item><description>Derived Stat Calculator (v0.17.2g) — uses resource bonuses in formulas</description></item>
///   <item><description>Specialization System (v0.17.4) — validates specialization availability</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="ArchetypeDefinition"/>
/// <seealso cref="ArchetypeResourceBonuses"/>
/// <seealso cref="ArchetypeAbilityGrant"/>
/// <seealso cref="ArchetypeSpecializationMapping"/>
/// <seealso cref="RecommendedBuild"/>
public interface IArchetypeProvider
{
    /// <summary>
    /// Gets the definition for a specific archetype.
    /// </summary>
    /// <param name="archetype">The archetype to look up.</param>
    /// <returns>
    /// The archetype definition containing display name, tagline, description,
    /// combat role, and other metadata; or <c>null</c> if the archetype is not
    /// found in the configuration.
    /// </returns>
    /// <remarks>
    /// Returns null rather than throwing for missing archetypes to support
    /// graceful degradation in UI code. The caller should check for null
    /// and handle accordingly.
    /// </remarks>
    /// <example>
    /// <code>
    /// var definition = provider.GetArchetype(Archetype.Warrior);
    /// if (definition != null)
    /// {
    ///     Console.WriteLine($"{definition.DisplayName}: {definition.Tagline}");
    /// }
    /// </code>
    /// </example>
    ArchetypeDefinition? GetArchetype(Archetype archetype);

    /// <summary>
    /// Gets all archetype definitions.
    /// </summary>
    /// <returns>
    /// List of all 4 archetype definitions, ordered by their enum integer
    /// value: Warrior (0), Skirmisher (1), Mystic (2), Adept (3).
    /// </returns>
    /// <remarks>
    /// The returned list is always ordered by <see cref="Archetype"/> enum value
    /// to ensure consistent display order in the character creation UI.
    /// </remarks>
    /// <example>
    /// <code>
    /// var archetypes = provider.GetAllArchetypes();
    /// foreach (var archetype in archetypes)
    /// {
    ///     Console.WriteLine($"[{archetype.ArchetypeId}] {archetype.DisplayName}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<ArchetypeDefinition> GetAllArchetypes();

    /// <summary>
    /// Gets the resource bonuses for an archetype.
    /// </summary>
    /// <param name="archetype">The archetype to look up.</param>
    /// <returns>
    /// The resource bonuses (HP, Stamina, Aether Pool, Movement, Special)
    /// for the archetype. Returns <see cref="ArchetypeResourceBonuses.None"/>
    /// if the archetype is not found.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Resource bonuses are applied during character creation and affect
    /// derived statistics calculated by the DerivedStatCalculator.
    /// </para>
    /// <para>
    /// Falls back to <see cref="ArchetypeResourceBonuses.None"/> rather than
    /// throwing for missing archetypes to support safe default behavior.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var bonuses = provider.GetResourceBonuses(Archetype.Warrior);
    /// Console.WriteLine($"HP Bonus: +{bonuses.MaxHpBonus}");
    /// // Output: "HP Bonus: +49"
    /// </code>
    /// </example>
    ArchetypeResourceBonuses GetResourceBonuses(Archetype archetype);

    /// <summary>
    /// Gets the starting abilities for an archetype.
    /// </summary>
    /// <param name="archetype">The archetype to look up.</param>
    /// <returns>
    /// List of 3 starting ability grants for the archetype. Each grant
    /// identifies an ability by ID, name, type (Active/Passive/Stance),
    /// and description. Returns an empty list if the archetype is not found.
    /// </returns>
    /// <remarks>
    /// Starting abilities are granted during character creation Step 4
    /// (Archetype selection). Each archetype provides exactly 3 abilities
    /// that define the character's initial combat capabilities.
    /// </remarks>
    /// <example>
    /// <code>
    /// var abilities = provider.GetStartingAbilities(Archetype.Warrior);
    /// foreach (var ability in abilities)
    /// {
    ///     Console.WriteLine($"  {ability.GetShortDisplay()}");
    /// }
    /// // Output:
    /// //   Power Strike [ACTIVE]
    /// //   Defensive Stance [STANCE]
    /// //   Iron Will [PASSIVE]
    /// </code>
    /// </example>
    IReadOnlyList<ArchetypeAbilityGrant> GetStartingAbilities(Archetype archetype);

    /// <summary>
    /// Gets the specialization mapping for an archetype.
    /// </summary>
    /// <param name="archetype">The archetype to look up.</param>
    /// <returns>
    /// The specialization mapping containing available specialization IDs
    /// and the recommended first choice. Falls back to the static
    /// <see cref="ArchetypeSpecializationMapping.GetForArchetype"/> if the
    /// archetype is not found in configuration.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The specialization mapping determines which specializations are
    /// available during character creation Step 5 (Specialization selection).
    /// Only specializations listed in the mapping can be selected for the
    /// given archetype.
    /// </para>
    /// <para>
    /// Falls back to static canonical mappings rather than returning empty
    /// data, since specialization availability is critical for the creation
    /// workflow.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var mapping = provider.GetSpecializationMapping(Archetype.Warrior);
    /// Console.WriteLine($"Available: {mapping.Count} specializations");
    /// Console.WriteLine($"Recommended: {mapping.RecommendedFirst}");
    /// // Output:
    /// //   Available: 6 specializations
    /// //   Recommended: guardian
    /// </code>
    /// </example>
    ArchetypeSpecializationMapping GetSpecializationMapping(Archetype archetype);

    /// <summary>
    /// Gets the selection flavor text for an archetype.
    /// </summary>
    /// <param name="archetype">The archetype to look up.</param>
    /// <returns>
    /// The second-person selection text for character creation display,
    /// or an empty string if the archetype is not found.
    /// </returns>
    /// <remarks>
    /// Selection text provides evocative narrative during character creation
    /// to help players understand the archetype's thematic identity. The text
    /// is written in second person (e.g., "You are the shield...").
    /// </remarks>
    /// <example>
    /// <code>
    /// var text = provider.GetSelectionText(Archetype.Warrior);
    /// // Returns: "You are the shield between the innocent and the horror..."
    /// </code>
    /// </example>
    string GetSelectionText(Archetype archetype);

    /// <summary>
    /// Gets a recommended attribute build for an archetype and optional lineage.
    /// </summary>
    /// <param name="archetype">The archetype for the build.</param>
    /// <param name="lineage">
    /// Optional lineage for build optimization. When provided, attempts to find
    /// a lineage-specific optimized build before falling back to the default build.
    /// </param>
    /// <returns>
    /// A recommended build with attribute allocation, or <c>null</c> if no
    /// recommendation is available for the combination.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Recommended builds suggest optimal attribute distributions based on
    /// archetype role and lineage synergies. The lookup order is:
    /// </para>
    /// <list type="number">
    ///   <item><description>Lineage-specific build (if lineage provided and match exists)</description></item>
    ///   <item><description>Default build (OptimalLineage == null)</description></item>
    ///   <item><description>null (no builds configured)</description></item>
    /// </list>
    /// <para>
    /// Returns null when no recommended builds are configured for the archetype,
    /// which is valid for archetypes where all builds are managed via
    /// <c>attributes.json</c> instead.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var build = provider.GetRecommendedBuild(Archetype.Warrior, Lineage.IronBlooded);
    /// if (build != null)
    /// {
    ///     Console.WriteLine(build.Value.GetDisplaySummary());
    /// }
    /// </code>
    /// </example>
    RecommendedBuild? GetRecommendedBuild(Archetype archetype, Lineage? lineage = null);

    /// <summary>
    /// Checks if a specialization is available for an archetype.
    /// </summary>
    /// <param name="archetype">The archetype to check.</param>
    /// <param name="specializationId">
    /// The specialization ID to check (kebab-case format, e.g., "guardian").
    /// Case-insensitive matching is used.
    /// </param>
    /// <returns>
    /// <c>true</c> if the specialization is available for the archetype;
    /// <c>false</c> otherwise, including for null or whitespace IDs.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that delegates to
    /// <see cref="GetSpecializationMapping"/> and
    /// <see cref="ArchetypeSpecializationMapping.IsSpecializationAvailable"/>.
    /// It performs case-insensitive matching and returns false for invalid
    /// inputs rather than throwing.
    /// </remarks>
    /// <example>
    /// <code>
    /// bool available = provider.IsSpecializationAvailable(Archetype.Warrior, "guardian");
    /// // Returns: true
    ///
    /// bool cross = provider.IsSpecializationAvailable(Archetype.Warrior, "elementalist");
    /// // Returns: false (Mystic-only specialization)
    /// </code>
    /// </example>
    bool IsSpecializationAvailable(Archetype archetype, string specializationId);
}
