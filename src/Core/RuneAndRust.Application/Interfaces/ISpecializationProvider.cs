// ═══════════════════════════════════════════════════════════════════════════════
// ISpecializationProvider.cs
// Interface defining the contract for accessing specialization definitions,
// ability tiers, special resources, and path type classifications from
// configuration. This is the central access point for all specialization-
// related data throughout the application.
// Version: 0.17.4d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides access to specialization definitions loaded from configuration.
/// </summary>
/// <remarks>
/// <para>
/// ISpecializationProvider is the primary interface for accessing specialization
/// data throughout the application. It abstracts the loading and caching of
/// specialization definitions from the <c>specializations.json</c> configuration file.
/// </para>
/// <para>
/// The provider aggregates all specialization data from v0.17.4a through v0.17.4c:
/// <list type="bullet">
///   <item><description>Specialization enums and path types (v0.17.4a): SpecializationId, SpecializationPathType</description></item>
///   <item><description>Specialization definitions (v0.17.4b): Display metadata, parent archetype, unlock cost</description></item>
///   <item><description>Special resources (v0.17.4b): Unique per-specialization combat resources</description></item>
///   <item><description>Ability tiers (v0.17.4c): Three-tier ability progression with unlock costs</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> Implementations must be thread-safe for
/// concurrent reads after initialization. The recommended pattern is
/// double-checked locking for lazy initialization.
/// </para>
/// <para>
/// <strong>Error Handling:</strong> Implementations throw
/// <see cref="Exceptions.SpecializationConfigurationException"/> when configuration
/// loading or validation fails. Individual lookup methods return null or
/// empty collections for missing data rather than throwing.
/// </para>
/// <para>
/// <strong>Consumers:</strong>
/// <list type="bullet">
///   <item><description>Character Creation UI (v0.17.5) — displays specialization selection</description></item>
///   <item><description>Specialization Application Service (v0.17.4e) — applies specializations to characters</description></item>
///   <item><description>Combat System — looks up ability details during combat execution</description></item>
///   <item><description>Progression System — validates tier unlock eligibility</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="SpecializationDefinition"/>
/// <seealso cref="SpecializationAbilityTier"/>
/// <seealso cref="SpecializationAbility"/>
/// <seealso cref="SpecialResourceDefinition"/>
/// <seealso cref="SpecializationId"/>
/// <seealso cref="SpecializationPathType"/>
public interface ISpecializationProvider
{
    /// <summary>
    /// Gets a specialization definition by its enum identifier.
    /// </summary>
    /// <param name="specializationId">The specialization identifier to look up.</param>
    /// <returns>
    /// The <see cref="SpecializationDefinition"/> containing display metadata,
    /// path type, special resource, and ability tiers; or <c>null</c> if the
    /// specialization is not found in the configuration.
    /// </returns>
    /// <remarks>
    /// Returns null rather than throwing for missing specializations to support
    /// graceful degradation in UI code. The caller should check for null
    /// and handle accordingly.
    /// </remarks>
    /// <example>
    /// <code>
    /// var berserkr = provider.GetBySpecializationId(SpecializationId.Berserkr);
    /// if (berserkr != null)
    /// {
    ///     Console.WriteLine($"Found: {berserkr.DisplayName} ({berserkr.PathType})");
    ///     // Output: "Found: Berserkr (Heretical)"
    /// }
    /// </code>
    /// </example>
    SpecializationDefinition? GetBySpecializationId(SpecializationId specializationId);

    /// <summary>
    /// Gets all specialization definitions for a specific archetype.
    /// </summary>
    /// <param name="archetype">The parent archetype to filter by.</param>
    /// <returns>
    /// A read-only list of specializations belonging to the archetype.
    /// Returns an empty list if no specializations are found for the archetype.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Used during character creation Step 5 to show available specializations
    /// based on the player's chosen archetype. Expected counts:
    /// <list type="bullet">
    ///   <item><description>Warrior: 6 specializations (Berserkr, Iron-Bane, Skjaldmaer, Skar-Horde, Atgeir-Wielder, Gorge-Maw)</description></item>
    ///   <item><description>Skirmisher: 4 specializations (Veiðimaðr, Myrk-gengr, Strandhögg, Hlekkr-master)</description></item>
    ///   <item><description>Mystic: 2 specializations (Seiðkona, Echo-Caller)</description></item>
    ///   <item><description>Adept: 5 specializations (Bone-Setter, Jötun-Reader, Skald, Scrap-Tinker, Einbúi)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var warriorSpecs = provider.GetByArchetype(Archetype.Warrior);
    /// Console.WriteLine($"Warrior has {warriorSpecs.Count} specializations");
    /// // Output: "Warrior has 6 specializations"
    /// foreach (var spec in warriorSpecs)
    /// {
    ///     Console.WriteLine($"  [{spec.PathType}] {spec.DisplayName}: {spec.Tagline}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<SpecializationDefinition> GetByArchetype(Archetype archetype);

    /// <summary>
    /// Gets all Heretical (Corruption-risk) specializations.
    /// </summary>
    /// <returns>
    /// A read-only list of all 5 Heretical path specializations: Berserkr,
    /// Gorge-Maw, Myrk-gengr, Seiðkona, and Echo-Caller.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Heretical specializations interface with corrupted Aether and have
    /// abilities that may trigger Corruption gain. The Corruption risk
    /// description for each is available via
    /// <see cref="SpecializationDefinition.GetCorruptionWarning"/>.
    /// </para>
    /// <para>
    /// All Mystic specializations (2 of 2) are Heretical. No Adept
    /// specializations are Heretical.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var heretical = provider.GetHereticalSpecializations();
    /// Console.WriteLine($"{heretical.Count} Heretical specializations:");
    /// // Output: "5 Heretical specializations:"
    /// foreach (var spec in heretical)
    /// {
    ///     Console.WriteLine($"  {spec.DisplayName}: {spec.GetCorruptionWarning()}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<SpecializationDefinition> GetHereticalSpecializations();

    /// <summary>
    /// Gets all Coherent (no Corruption-risk) specializations.
    /// </summary>
    /// <returns>
    /// A read-only list of all 12 Coherent path specializations that work
    /// within stable reality and do not risk Corruption from ability use.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Coherent specializations work within stable reality and do not risk
    /// Corruption from ability use. All Adept specializations (5 of 5) are
    /// Coherent.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var coherent = provider.GetCoherentSpecializations();
    /// Console.WriteLine($"{coherent.Count} Coherent specializations");
    /// // Output: "12 Coherent specializations"
    /// </code>
    /// </example>
    IReadOnlyList<SpecializationDefinition> GetCoherentSpecializations();

    /// <summary>
    /// Gets all loaded specialization definitions.
    /// </summary>
    /// <returns>
    /// A read-only list of all 17 specialization definitions across
    /// all 4 archetypes.
    /// </returns>
    /// <example>
    /// <code>
    /// var all = provider.GetAll();
    /// foreach (var spec in all)
    /// {
    ///     Console.WriteLine($"[{spec.ParentArchetype}] {spec.DisplayName} ({spec.PathType})");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<SpecializationDefinition> GetAll();

    /// <summary>
    /// Gets the total count of loaded specializations.
    /// </summary>
    /// <returns>The number of loaded definitions (expected: 17).</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine($"Loaded {provider.Count} specializations");
    /// // Output: "Loaded 17 specializations"
    /// </code>
    /// </example>
    int Count { get; }

    /// <summary>
    /// Checks if a specialization definition exists in the loaded configuration.
    /// </summary>
    /// <param name="specializationId">The specialization identifier to check.</param>
    /// <returns>
    /// <c>true</c> if the definition is loaded; otherwise, <c>false</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// bool exists = provider.Exists(SpecializationId.Berserkr);
    /// // Returns: true
    /// </code>
    /// </example>
    bool Exists(SpecializationId specializationId);

    /// <summary>
    /// Gets specializations that have a special resource definition.
    /// </summary>
    /// <returns>
    /// A read-only list of the 5 specializations with special resource
    /// definitions: Berserkr (Rage), Skjaldmaer (Block Charges), Iron-Bane
    /// (Righteous Fervor), Seiðkona (Aether Resonance), and Echo-Caller (Echoes).
    /// </returns>
    /// <remarks>
    /// Useful for combat systems that need to initialize special resource tracking
    /// when a character enters combat with one of these specializations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var withResource = provider.GetWithSpecialResource();
    /// Console.WriteLine($"{withResource.Count} specializations with special resources:");
    /// // Output: "5 specializations with special resources:"
    /// foreach (var spec in withResource)
    /// {
    ///     Console.WriteLine($"  {spec.DisplayName}: {spec.SpecialResource}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<SpecializationDefinition> GetWithSpecialResource();

    /// <summary>
    /// Gets a specific ability by ID across all specializations.
    /// </summary>
    /// <param name="abilityId">
    /// The ability identifier to search for (kebab-case format, e.g., "rage-strike").
    /// The search is case-insensitive.
    /// </param>
    /// <returns>
    /// A tuple containing the parent <see cref="SpecializationDefinition"/> and
    /// the matching <see cref="SpecializationAbility"/>, or <c>null</c> if no
    /// ability with the given ID exists in any specialization.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="abilityId"/> is null, empty, or whitespace.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Used by combat systems to look up ability details during execution.
    /// Searches all specializations and all tiers for the matching ability ID.
    /// </para>
    /// <para>
    /// Performance: O(n) where n = total abilities across all specializations.
    /// For 17 specializations with up to 9 abilities each (max ~153), this is
    /// acceptable. A dedicated ability index can be added if performance
    /// becomes an issue.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = provider.GetAbility("rage-strike");
    /// if (result.HasValue)
    /// {
    ///     var (specialization, ability) = result.Value;
    ///     Console.WriteLine($"Found {ability.DisplayName} in {specialization.DisplayName}");
    ///     // Output: "Found Rage Strike in Berserkr"
    /// }
    /// </code>
    /// </example>
    (SpecializationDefinition Specialization, SpecializationAbility Ability)? GetAbility(string abilityId);
}
