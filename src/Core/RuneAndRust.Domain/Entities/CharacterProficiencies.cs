// ═══════════════════════════════════════════════════════════════════════════════
// CharacterProficiencies.cs
// Entity tracking weapon category proficiency levels and progress for a character.
// Version: 0.16.1d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tracks weapon category proficiency levels and progress for a character.
/// </summary>
/// <remarks>
/// <para>
/// CharacterProficiencies maintains the current proficiency state for all 11
/// weapon categories. It is initialized from the character's archetype
/// proficiency matrix (v0.16.1c) and updated as the character gains combat
/// experience through gameplay.
/// </para>
/// <para>
/// Each category tracks both:
/// </para>
/// <list type="bullet">
///   <item><description>Current proficiency level (NonProficient, Proficient, Expert, Master)</description></item>
///   <item><description>Combat experience progress toward the next level</description></item>
/// </list>
/// <para>
/// Characters starting with archetype proficiencies begin at Proficient level
/// with 0 experience. Non-proficient categories start at NonProficient level
/// with -3 attack and -2 damage penalties until 10 combats are completed.
/// </para>
/// <para>
/// This entity is owned by the Character entity and should not be persisted
/// independently. Proficiency advancement is handled by the
/// IProficiencyAcquisitionService (v0.16.1e).
/// </para>
/// </remarks>
/// <seealso cref="ArchetypeProficiencySet"/>
/// <seealso cref="ProficiencyProgress"/>
/// <seealso cref="WeaponCategory"/>
/// <seealso cref="WeaponProficiencyLevel"/>
public class CharacterProficiencies : IEntity
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Fields
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Internal dictionary mapping weapon categories to their progress state.
    /// </summary>
    private readonly Dictionary<WeaponCategory, ProficiencyProgress> _proficiencies;

    // ═══════════════════════════════════════════════════════════════════════════
    // Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this proficiency tracking entity.
    /// </summary>
    /// <remarks>
    /// Auto-generated during creation. Used for entity tracking and persistence.
    /// </remarks>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the identifier of the character this proficiency state belongs to.
    /// </summary>
    /// <remarks>
    /// This references the owning Character entity's Id. It is set during
    /// creation via <see cref="CreateFromArchetype"/> and cannot be changed.
    /// </remarks>
    public Guid CharacterId { get; private set; }

    /// <summary>
    /// Gets all proficiency progress entries as a read-only dictionary.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This dictionary contains exactly 11 entries, one for each
    /// <see cref="WeaponCategory"/>. Access individual entries using
    /// <see cref="GetProgress"/> or <see cref="GetLevel"/>.
    /// </para>
    /// <para>
    /// The dictionary is read-only to enforce immutability. Progress updates
    /// must go through <see cref="UpdateProgress"/> method.
    /// </para>
    /// </remarks>
    public IReadOnlyDictionary<WeaponCategory, ProficiencyProgress> Proficiencies =>
        _proficiencies;

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private parameterless constructor for EF Core materialization.
    /// </summary>
    /// <remarks>
    /// This constructor initializes the dictionary as empty. EF Core will
    /// populate it via property setters during materialization.
    /// </remarks>
    private CharacterProficiencies()
    {
        _proficiencies = new Dictionary<WeaponCategory, ProficiencyProgress>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new CharacterProficiencies entity from an archetype proficiency set.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <param name="archetypeProficiencySet">
    /// The archetype proficiency matrix defining starting proficiencies.
    /// </param>
    /// <param name="thresholds">
    /// Experience thresholds configuration. Use <see cref="ProficiencyThresholds.Default"/>
    /// for standard progression.
    /// </param>
    /// <returns>Initialized CharacterProficiencies instance with all 11 categories.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when archetypeProficiencySet or thresholds is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Characters start with Proficient level (0 experience) in archetype categories
    /// and NonProficient level (0 experience) in all others.
    /// </para>
    /// <para>
    /// Example initialization for Warrior archetype:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>All 11 categories: Proficient (0/25 to Expert)</description></item>
    /// </list>
    /// <para>
    /// Example initialization for Mystic archetype:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Daggers, Staves, ArcaneImplements: Proficient (0/25 to Expert)</description></item>
    ///   <item><description>8 other categories: NonProficient (0/10 to Proficient)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create proficiencies for a new Mystic character
    /// var archetypeSet = archetypeProficiencyProvider.GetProficiencySet("mystic");
    /// var thresholds = ProficiencyThresholds.Default;
    /// 
    /// var proficiencies = CharacterProficiencies.CreateFromArchetype(
    ///     characterId: character.Id,
    ///     archetypeProficiencySet: archetypeSet,
    ///     thresholds: thresholds);
    /// 
    /// // Mystic starts with 3 proficiencies, 8 non-proficiencies
    /// var proficientCount = proficiencies.GetCountAtLevel(WeaponProficiencyLevel.Proficient);
    /// // proficientCount == 3
    /// </code>
    /// </example>
    public static CharacterProficiencies CreateFromArchetype(
        Guid characterId,
        ArchetypeProficiencySet archetypeProficiencySet,
        ProficiencyThresholds thresholds)
    {
        // Validate parameters
        ArgumentNullException.ThrowIfNull(archetypeProficiencySet);

        // Create the entity
        var proficiencies = new CharacterProficiencies
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId
        };

        // Initialize all weapon categories from archetype matrix
        foreach (var category in Enum.GetValues<WeaponCategory>())
        {
            // Get starting level from archetype (Proficient or NonProficient)
            var startingLevel = archetypeProficiencySet.GetStartingLevel(category);

            // Create progress with 0 experience
            var progress = ProficiencyProgress.Create(
                category,
                startingLevel,
                combatExperience: 0,
                thresholds);

            proficiencies._proficiencies[category] = progress;
        }

        return proficiencies;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Query Methods - Individual Category
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current proficiency level for a weapon category.
    /// </summary>
    /// <param name="category">The weapon category to query.</param>
    /// <returns>
    /// The current proficiency level. Returns NonProficient if category not found.
    /// </returns>
    /// <remarks>
    /// This is the primary method for combat systems to check proficiency.
    /// The returned level determines attack/damage modifiers:
    /// <list type="table">
    ///   <listheader>
    ///     <term>Level</term>
    ///     <description>Modifiers</description>
    ///   </listheader>
    ///   <item>
    ///     <term>NonProficient</term>
    ///     <description>-3 attack, -2 damage</description>
    ///   </item>
    ///   <item>
    ///     <term>Proficient</term>
    ///     <description>+0 attack, +0 damage</description>
    ///   </item>
    ///   <item>
    ///     <term>Expert</term>
    ///     <description>+1 attack, +0 damage</description>
    ///   </item>
    ///   <item>
    ///     <term>Master</term>
    ///     <description>+2 attack, +1 damage</description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var level = proficiencies.GetLevel(WeaponCategory.Swords);
    /// if (level == WeaponProficiencyLevel.NonProficient)
    /// {
    ///     attackRoll -= 3;
    ///     damageRoll -= 2;
    /// }
    /// </code>
    /// </example>
    public WeaponProficiencyLevel GetLevel(WeaponCategory category)
    {
        return _proficiencies.TryGetValue(category, out var progress)
            ? progress.CurrentLevel
            : WeaponProficiencyLevel.NonProficient;
    }

    /// <summary>
    /// Gets the full progress details for a weapon category.
    /// </summary>
    /// <param name="category">The weapon category to query.</param>
    /// <returns>The proficiency progress including level and experience.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the category is not found in the proficiencies dictionary.
    /// </exception>
    /// <remarks>
    /// Use this method when you need detailed progress information such as
    /// combat experience, threshold values, or progress percentage.
    /// </remarks>
    /// <example>
    /// <code>
    /// var progress = proficiencies.GetProgress(WeaponCategory.Swords);
    /// Console.WriteLine($"Swords: {progress.CurrentLevel}");
    /// Console.WriteLine($"Progress: {progress.ProgressPercentage:F1}%");
    /// Console.WriteLine($"Combats to next: {progress.ExperienceToNextLevel}");
    /// </code>
    /// </example>
    public ProficiencyProgress GetProgress(WeaponCategory category)
    {
        if (!_proficiencies.TryGetValue(category, out var progress))
        {
            throw new ArgumentException(
                $"Proficiency not found for category: {category}",
                nameof(category));
        }

        return progress;
    }

    /// <summary>
    /// Gets the combat experience for a weapon category.
    /// </summary>
    /// <param name="category">The weapon category to query.</param>
    /// <returns>
    /// Number of combats using this category at the current level.
    /// Returns 0 if category not found.
    /// </returns>
    /// <remarks>
    /// Experience is tracked per-level. When a proficiency advances,
    /// the experience resets to 0 for the new level.
    /// </remarks>
    public int GetCombatExperience(WeaponCategory category)
    {
        return _proficiencies.TryGetValue(category, out var progress)
            ? progress.CombatExperience
            : 0;
    }

    /// <summary>
    /// Checks if a category can advance to the next proficiency level.
    /// </summary>
    /// <param name="category">The weapon category to check.</param>
    /// <returns>
    /// True if not at max level (Master), false otherwise or if category not found.
    /// </returns>
    /// <remarks>
    /// This does not check if the experience threshold has been reached.
    /// Use <see cref="ProficiencyProgress.HasReachedNextThreshold"/> for that.
    /// </remarks>
    public bool CanAdvance(WeaponCategory category)
    {
        return _proficiencies.TryGetValue(category, out var progress)
            && progress.CanAdvance;
    }

    /// <summary>
    /// Checks if a category is at maximum proficiency level (Master).
    /// </summary>
    /// <param name="category">The weapon category to check.</param>
    /// <returns>
    /// True if at Master level, false otherwise or if category not found.
    /// </returns>
    public bool IsAtMaxLevel(WeaponCategory category)
    {
        return _proficiencies.TryGetValue(category, out var progress)
            && progress.IsAtMaxLevel;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Query Methods - Aggregate
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all categories at a specific proficiency level.
    /// </summary>
    /// <param name="level">The proficiency level to filter by.</param>
    /// <returns>
    /// Read-only list of categories at the specified level.
    /// </returns>
    /// <remarks>
    /// Useful for UI displays showing categories grouped by proficiency level.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Show all mastered categories
    /// var mastered = proficiencies.GetCategoriesAtLevel(WeaponProficiencyLevel.Master);
    /// foreach (var category in mastered)
    /// {
    ///     Console.WriteLine($"Mastered: {category}");
    /// }
    /// </code>
    /// </example>
    public IReadOnlyList<WeaponCategory> GetCategoriesAtLevel(
        WeaponProficiencyLevel level)
    {
        return _proficiencies
            .Where(kvp => kvp.Value.CurrentLevel == level)
            .Select(kvp => kvp.Key)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the count of categories at a specific proficiency level.
    /// </summary>
    /// <param name="level">The proficiency level to count.</param>
    /// <returns>Number of categories at that level (0-11).</returns>
    /// <example>
    /// <code>
    /// // Check how many categories are mastered
    /// var masterCount = proficiencies.GetCountAtLevel(WeaponProficiencyLevel.Master);
    /// if (masterCount >= 5)
    /// {
    ///     AwardAchievement("weapon-master");
    /// }
    /// </code>
    /// </example>
    public int GetCountAtLevel(WeaponProficiencyLevel level)
    {
        return _proficiencies.Count(kvp => kvp.Value.CurrentLevel == level);
    }

    /// <summary>
    /// Gets the total combat experience across all weapon categories.
    /// </summary>
    /// <returns>Sum of combat experience from all categories.</returns>
    /// <remarks>
    /// This represents total weapon combat engagement, not progression points.
    /// Experience is counted per-level, so advancing resets individual category
    /// experience but this total still reflects cumulative usage.
    /// </remarks>
    public int GetTotalCombatExperience()
    {
        return _proficiencies.Values.Sum(p => p.CombatExperience);
    }

    /// <summary>
    /// Gets all categories that have reached their advancement threshold.
    /// </summary>
    /// <returns>
    /// Read-only list of categories eligible for advancement (excluding Master).
    /// </returns>
    /// <remarks>
    /// These categories have accumulated enough combat experience to advance
    /// but have not yet been promoted. The IProficiencyAcquisitionService
    /// (v0.16.1e) uses this to handle batch advancements.
    /// </remarks>
    public IReadOnlyList<WeaponCategory> GetCategoriesReadyToAdvance()
    {
        return _proficiencies
            .Where(kvp => kvp.Value.HasReachedNextThreshold)
            .Select(kvp => kvp.Key)
            .ToList()
            .AsReadOnly();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Update Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Updates the proficiency progress for a category.
    /// </summary>
    /// <param name="category">The weapon category to update.</param>
    /// <param name="newProgress">The updated progress value object.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when newProgress is null (for reference types, not applicable to record struct).
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the progress category doesn't match the specified category.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is internal to domain logic. External proficiency updates
    /// should go through IProficiencyAcquisitionService (v0.16.1e) to ensure
    /// proper validation, logging, and event publishing.
    /// </para>
    /// <para>
    /// The method validates that the progress category matches to prevent
    /// accidentally storing the wrong progress data.
    /// </para>
    /// </remarks>
    internal void UpdateProgress(WeaponCategory category, ProficiencyProgress newProgress)
    {
        // Validate category matches
        if (newProgress.Category != category)
        {
            throw new ArgumentException(
                $"Progress category mismatch. Expected {category}, got {newProgress.Category}.",
                nameof(newProgress));
        }

        _proficiencies[category] = newProgress;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Object Overrides
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of this proficiency state.
    /// </summary>
    /// <returns>
    /// A summary showing the character ID and count of proficiencies at each level.
    /// Example: "Proficiencies for Character {id}: Proficient=3, Expert=1, Master=0"
    /// </returns>
    public override string ToString()
    {
        var nonProfCount = GetCountAtLevel(WeaponProficiencyLevel.NonProficient);
        var profCount = GetCountAtLevel(WeaponProficiencyLevel.Proficient);
        var expertCount = GetCountAtLevel(WeaponProficiencyLevel.Expert);
        var masterCount = GetCountAtLevel(WeaponProficiencyLevel.Master);

        return $"Proficiencies for Character {CharacterId}: " +
               $"NonProficient={nonProfCount}, Proficient={profCount}, " +
               $"Expert={expertCount}, Master={masterCount}";
    }
}
