// ═══════════════════════════════════════════════════════════════════════════════
// IArchetypeProficiencyProvider.cs
// Interface providing access to archetype proficiency matrix data.
// Version: 0.16.1c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Provides access to archetype proficiency matrix data.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts the source of archetype proficiency data, allowing
/// it to be loaded from configuration files, databases, or hardcoded defaults.
/// </para>
/// <para>
/// The archetype proficiency matrix defines which weapon categories each of the
/// four archetypes (Warrior, Skirmisher, Mystic, Adept) starts proficient with.
/// This information is critical during character creation to initialize weapon
/// proficiency levels.
/// </para>
/// <para>
/// Implementations should:
/// </para>
/// <list type="bullet">
///   <item><description>Cache loaded proficiency sets for performance</description></item>
///   <item><description>Validate that all four archetypes have corresponding entries</description></item>
///   <item><description>Normalize archetype IDs to lowercase for consistent lookups</description></item>
///   <item><description>Log configuration loading and validation results</description></item>
/// </list>
/// <para>
/// Proficiency matrix reference:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Archetype</term>
///     <description>Proficient Categories (Count)</description>
///   </listheader>
///   <item>
///     <term>Warrior</term>
///     <description>All 11 categories</description>
///   </item>
///   <item>
///     <term>Skirmisher</term>
///     <description>Daggers, Swords, Axes, Bows, Crossbows (5)</description>
///   </item>
///   <item>
///     <term>Mystic</term>
///     <description>Daggers, Staves, ArcaneImplements (3)</description>
///   </item>
///   <item>
///     <term>Adept</term>
///     <description>Daggers, Staves, Hammers, Crossbows (4)</description>
///   </item>
/// </list>
/// </remarks>
/// <seealso cref="ArchetypeProficiencySet"/>
/// <seealso cref="WeaponCategory"/>
/// <seealso cref="WeaponProficiencyLevel"/>
public interface IArchetypeProficiencyProvider
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Proficiency Set Retrieval
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the proficiency set for a specific archetype.
    /// </summary>
    /// <param name="archetypeId">
    /// The archetype identifier (e.g., "warrior", "mystic").
    /// Case-insensitive; will be normalized to lowercase.
    /// </param>
    /// <returns>
    /// The <see cref="ArchetypeProficiencySet"/> for the specified archetype.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when archetypeId is null, empty, or not found in configuration.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is the primary method for retrieving archetype proficiency data.
    /// The returned set contains all proficient categories and helper methods
    /// for proficiency checking.
    /// </para>
    /// <para>
    /// Logging: Implementations should log Debug-level messages for successful
    /// retrievals and Warning-level messages for missing archetypes.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get Warrior proficiency set
    /// var warriorSet = _proficiencyProvider.GetProficiencySet("warrior");
    /// Console.WriteLine($"{warriorSet.DisplayName}: {warriorSet.GetProficientCategoryCount()} proficiencies");
    /// // Output: "Warrior: 11 proficiencies"
    /// </code>
    /// </example>
    ArchetypeProficiencySet GetProficiencySet(string archetypeId);

    /// <summary>
    /// Gets proficiency sets for all configured archetypes.
    /// </summary>
    /// <returns>
    /// A read-only list of all <see cref="ArchetypeProficiencySet"/> instances.
    /// Should contain exactly 4 entries (Warrior, Skirmisher, Mystic, Adept).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is useful for UI elements that need to display proficiency
    /// comparisons across all archetypes (e.g., character creation screens).
    /// </para>
    /// <para>
    /// The returned list is ordered by configuration file order, typically:
    /// Warrior, Skirmisher, Mystic, Adept.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Display all archetype proficiencies
    /// foreach (var set in _proficiencyProvider.GetAllProficiencySets())
    /// {
    ///     Console.WriteLine($"{set.DisplayName}: {set.GetProficientCategoryCount()} categories");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<ArchetypeProficiencySet> GetAllProficiencySets();

    // ═══════════════════════════════════════════════════════════════════════════
    // Proficiency Queries
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if an archetype is proficient with a weapon category.
    /// </summary>
    /// <param name="archetypeId">
    /// The archetype identifier (e.g., "warrior", "mystic").
    /// Case-insensitive.
    /// </param>
    /// <param name="category">The weapon category to check.</param>
    /// <returns>
    /// <c>true</c> if the archetype is proficient with the category;
    /// <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when archetypeId is not found in configuration.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is a convenience method equivalent to:
    /// <c>GetProficiencySet(archetypeId).IsProficientWith(category)</c>
    /// </para>
    /// <para>
    /// Logging: Implementations should log Debug-level messages for
    /// proficiency checks including the archetype, category, and result.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Check if Mystic can use Swords effectively
    /// bool canUseSwords = _proficiencyProvider.IsProficient("mystic", WeaponCategory.Swords);
    /// // Returns: false (Mystics are not proficient with Swords)
    /// </code>
    /// </example>
    bool IsProficient(string archetypeId, WeaponCategory category);

    /// <summary>
    /// Gets the starting proficiency level for an archetype and category.
    /// </summary>
    /// <param name="archetypeId">
    /// The archetype identifier (e.g., "warrior", "mystic").
    /// Case-insensitive.
    /// </param>
    /// <param name="category">The weapon category to check.</param>
    /// <returns>
    /// <see cref="WeaponProficiencyLevel.Proficient"/> if the archetype is
    /// proficient with the category;
    /// <see cref="WeaponProficiencyLevel.NonProficient"/> otherwise.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when archetypeId is not found in configuration.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is used during character creation to initialize all weapon
    /// proficiency levels based on the selected archetype.
    /// </para>
    /// <para>
    /// Starting levels are binary. Higher levels (Expert, Master) must be
    /// acquired through gameplay (training, combat experience) as defined
    /// in v0.16.1e.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get starting proficiency level for Skirmisher with Bows
    /// var level = _proficiencyProvider.GetStartingProficiencyLevel("skirmisher", WeaponCategory.Bows);
    /// // Returns: WeaponProficiencyLevel.Proficient
    /// 
    /// // Initialize all proficiencies for a new character
    /// foreach (var category in Enum.GetValues&lt;WeaponCategory&gt;())
    /// {
    ///     var startingLevel = _proficiencyProvider.GetStartingProficiencyLevel(archetypeId, category);
    ///     character.SetProficiency(category, startingLevel);
    /// }
    /// </code>
    /// </example>
    WeaponProficiencyLevel GetStartingProficiencyLevel(
        string archetypeId,
        WeaponCategory category);

    // ═══════════════════════════════════════════════════════════════════════════
    // Category Listing
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all weapon categories an archetype is proficient with.
    /// </summary>
    /// <param name="archetypeId">
    /// The archetype identifier (e.g., "warrior", "mystic").
    /// Case-insensitive.
    /// </param>
    /// <returns>
    /// A read-only list of <see cref="WeaponCategory"/> values the archetype
    /// is proficient with.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when archetypeId is not found in configuration.
    /// </exception>
    /// <remarks>
    /// This method is useful for displaying proficiency lists in the UI
    /// or for equipment filtering (e.g., "show only weapons I can use").
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get all categories a Mystic can use effectively
    /// var proficientCategories = _proficiencyProvider.GetProficientCategories("mystic");
    /// // Returns: [Daggers, Staves, ArcaneImplements]
    /// </code>
    /// </example>
    IReadOnlyList<WeaponCategory> GetProficientCategories(string archetypeId);

    /// <summary>
    /// Gets all weapon categories an archetype is NOT proficient with.
    /// </summary>
    /// <param name="archetypeId">
    /// The archetype identifier (e.g., "warrior", "mystic").
    /// Case-insensitive.
    /// </param>
    /// <returns>
    /// A read-only list of <see cref="WeaponCategory"/> values the archetype
    /// is NOT proficient with (starts with NonProficient level).
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when archetypeId is not found in configuration.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is useful for displaying proficiency warnings or calculating
    /// training requirements.
    /// </para>
    /// <para>
    /// For the Warrior archetype, this returns an empty list since Warriors
    /// are proficient with all weapon categories.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get categories a Mystic needs to train for
    /// var nonProficientCategories = _proficiencyProvider.GetNonProficientCategories("mystic");
    /// // Returns: [Axes, Swords, Hammers, Polearms, Bows, Crossbows, Shields, Firearms]
    /// </code>
    /// </example>
    IReadOnlyList<WeaponCategory> GetNonProficientCategories(string archetypeId);

    /// <summary>
    /// Gets the count of weapon categories an archetype is proficient with.
    /// </summary>
    /// <param name="archetypeId">
    /// The archetype identifier (e.g., "warrior", "mystic").
    /// Case-insensitive.
    /// </param>
    /// <returns>
    /// The number of categories the archetype is proficient with (1-11).
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when archetypeId is not found in configuration.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Expected counts per archetype:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Warrior: 11</description></item>
    ///   <item><description>Skirmisher: 5</description></item>
    ///   <item><description>Adept: 4</description></item>
    ///   <item><description>Mystic: 3</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var count = _proficiencyProvider.GetProficientCategoryCount("skirmisher");
    /// Console.WriteLine($"Skirmisher is proficient with {count} weapon categories");
    /// // Output: "Skirmisher is proficient with 5 weapon categories"
    /// </code>
    /// </example>
    int GetProficientCategoryCount(string archetypeId);

    // ═══════════════════════════════════════════════════════════════════════════
    // Configuration Validation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates that all required archetypes have proficiency sets configured.
    /// </summary>
    /// <returns>
    /// <c>true</c> if all four archetypes (Warrior, Skirmisher, Mystic, Adept)
    /// are configured; <c>false</c> otherwise.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method should be called during application startup to ensure
    /// the archetype proficiency configuration is complete.
    /// </para>
    /// <para>
    /// Validation checks:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>All four archetypes have corresponding entries</description></item>
    ///   <item><description>Each archetype has at least one proficient category</description></item>
    ///   <item><description>Warrior has all 11 categories</description></item>
    /// </list>
    /// <para>
    /// Logging: Implementations should log Information-level messages for
    /// successful validation and Warning/Error-level messages for failures.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Validate configuration during startup
    /// if (!_proficiencyProvider.ValidateConfiguration())
    /// {
    ///     _logger.LogError("Archetype proficiency configuration is incomplete!");
    ///     throw new InvalidOperationException("Missing archetype proficiency data");
    /// }
    /// </code>
    /// </example>
    bool ValidateConfiguration();
}
