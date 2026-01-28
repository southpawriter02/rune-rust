// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeProficiencySet.cs
// Entity defining weapon category proficiencies for a character archetype.
// Version: 0.16.1c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

/// <summary>
/// Defines weapon category proficiencies for a character archetype.
/// </summary>
/// <remarks>
/// <para>
/// ArchetypeProficiencySet determines which weapon categories a character
/// is naturally proficient with based on their archetype choice. Characters
/// start with <see cref="WeaponProficiencyLevel.Proficient"/> level in listed
/// categories and <see cref="WeaponProficiencyLevel.NonProficient"/> in all others.
/// </para>
/// <para>
/// The four archetypes have distinctive weapon access:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Archetype</term>
///     <description>Proficient Categories</description>
///   </listheader>
///   <item>
///     <term>Warrior</term>
///     <description>All 11 categories (master of arms)</description>
///   </item>
///   <item>
///     <term>Skirmisher</term>
///     <description>5 categories: Daggers, Swords, Axes, Bows, Crossbows</description>
///   </item>
///   <item>
///     <term>Mystic</term>
///     <description>3 categories: Daggers, Staves, ArcaneImplements</description>
///   </item>
///   <item>
///     <term>Adept</term>
///     <description>4 categories: Daggers, Staves, Hammers, Crossbows</description>
///   </item>
/// </list>
/// <para>
/// This entity is loaded from configuration and is immutable after creation.
/// The archetype proficiency matrix is central to the character creation system
/// and determines initial combat capabilities.
/// </para>
/// </remarks>
/// <seealso cref="WeaponCategory"/>
/// <seealso cref="WeaponProficiencyLevel"/>
public class ArchetypeProficiencySet : IEntity
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Constants
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Threshold for versatile archetype classification (7+ proficiencies).
    /// </summary>
    private const int VersatileThreshold = 7;

    /// <summary>
    /// Threshold for specialist archetype classification (3 or fewer proficiencies).
    /// </summary>
    private const int SpecialistThreshold = 3;

    // ═══════════════════════════════════════════════════════════════════════════
    // Fields
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Internal list of weapon categories this archetype is proficient with.
    /// </summary>
    private readonly List<WeaponCategory> _proficientCategories;

    // ═══════════════════════════════════════════════════════════════════════════
    // Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this proficiency set.
    /// </summary>
    /// <remarks>
    /// This identifier is auto-generated during creation via <see cref="Create"/>
    /// and is used for entity tracking and persistence.
    /// </remarks>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the archetype identifier (e.g., "warrior", "mystic").
    /// </summary>
    /// <remarks>
    /// <para>
    /// This identifier matches the archetype ID used in the character creation system.
    /// It is normalized to lowercase during creation for consistent lookups.
    /// </para>
    /// <para>
    /// Valid archetype IDs: "warrior", "skirmisher", "mystic", "adept".
    /// </para>
    /// </remarks>
    public string ArchetypeId { get; private set; }

    /// <summary>
    /// Gets the human-readable archetype name (e.g., "Warrior").
    /// </summary>
    /// <remarks>
    /// This display name is used in UI elements and logs to provide
    /// user-friendly archetype identification.
    /// </remarks>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Gets the description of this archetype's combat style.
    /// </summary>
    /// <remarks>
    /// This description provides flavor text explaining the archetype's
    /// relationship with weapons and combat approach.
    /// </remarks>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the weapon categories this archetype is proficient with.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Characters of this archetype start with <see cref="WeaponProficiencyLevel.Proficient"/>
    /// in these categories. For weapons outside this list, they start with
    /// <see cref="WeaponProficiencyLevel.NonProficient"/>.
    /// </para>
    /// <para>
    /// The list is returned as read-only to maintain immutability.
    /// </para>
    /// </remarks>
    public IReadOnlyList<WeaponCategory> ProficientCategories =>
        _proficientCategories.AsReadOnly();

    // ═══════════════════════════════════════════════════════════════════════════
    // Derived Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this is a versatile archetype (proficient with 7+ categories).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Versatile archetypes like Warrior have broad weapon access, allowing
    /// flexibility in equipment choices and combat styles.
    /// </para>
    /// <para>
    /// Threshold: <see cref="VersatileThreshold"/> (7 categories).
    /// </para>
    /// </remarks>
    public bool IsVersatile => GetProficientCategoryCount() >= VersatileThreshold;

    /// <summary>
    /// Gets whether this is a specialist archetype (proficient with 3 or fewer categories).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Specialist archetypes like Mystic focus on a narrow set of weapons,
    /// typically those that complement their primary abilities (e.g., spellcasting).
    /// </para>
    /// <para>
    /// Threshold: <see cref="SpecialistThreshold"/> (3 categories).
    /// </para>
    /// </remarks>
    public bool IsSpecialist => GetProficientCategoryCount() <= SpecialistThreshold;

    /// <summary>
    /// Gets whether this archetype is proficient with all weapon categories.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Currently only the Warrior archetype is proficient with all 11 categories.
    /// This property is useful for combat systems that need to check for
    /// universal weapon mastery.
    /// </para>
    /// </remarks>
    public bool IsProficientWithAllWeapons =>
        GetProficientCategoryCount() == Enum.GetValues<WeaponCategory>().Length;

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private parameterless constructor for EF Core materialization.
    /// </summary>
    /// <remarks>
    /// This constructor initializes fields with null-forgiving operators
    /// as required by EF Core's materialization process.
    /// </remarks>
    private ArchetypeProficiencySet()
    {
        ArchetypeId = null!;
        DisplayName = null!;
        Description = null!;
        _proficientCategories = new List<WeaponCategory>();
    }

    /// <summary>
    /// Private constructor for factory method.
    /// </summary>
    /// <param name="archetypeId">Normalized archetype identifier.</param>
    /// <param name="displayName">Display name for UI.</param>
    /// <param name="description">Description of combat style.</param>
    /// <param name="categories">List of proficient weapon categories.</param>
    private ArchetypeProficiencySet(
        string archetypeId,
        string displayName,
        string description,
        List<WeaponCategory> categories)
    {
        Id = Guid.NewGuid();
        ArchetypeId = archetypeId;
        DisplayName = displayName;
        Description = description;
        _proficientCategories = categories;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new ArchetypeProficiencySet with the specified parameters.
    /// </summary>
    /// <param name="archetypeId">
    /// The archetype identifier (e.g., "warrior"). Will be normalized to lowercase.
    /// </param>
    /// <param name="displayName">
    /// The human-readable display name (e.g., "Warrior").
    /// </param>
    /// <param name="description">
    /// Description of the archetype's combat style.
    /// </param>
    /// <param name="proficientCategories">
    /// Collection of weapon categories the archetype is proficient with.
    /// Must contain at least one category.
    /// </param>
    /// <returns>A new ArchetypeProficiencySet instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when archetypeId, displayName, or description is null or whitespace,
    /// or when proficientCategories is empty.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when proficientCategories is null.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create a Mystic proficiency set
    /// var mysticSet = ArchetypeProficiencySet.Create(
    ///     archetypeId: "mystic",
    ///     displayName: "Mystic",
    ///     description: "Arcane practitioner proficient with magical implements.",
    ///     proficientCategories: new[]
    ///     {
    ///         WeaponCategory.Daggers,
    ///         WeaponCategory.Staves,
    ///         WeaponCategory.ArcaneImplements
    ///     });
    /// </code>
    /// </example>
    public static ArchetypeProficiencySet Create(
        string archetypeId,
        string displayName,
        string description,
        IEnumerable<WeaponCategory> proficientCategories)
    {
        // Validate string parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(archetypeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        // Validate categories collection
        ArgumentNullException.ThrowIfNull(proficientCategories);

        var categoriesList = proficientCategories.Distinct().ToList();
        if (categoriesList.Count == 0)
        {
            throw new ArgumentException(
                "Archetype must be proficient with at least one weapon category.",
                nameof(proficientCategories));
        }

        // Create and return the proficiency set using private constructor
        return new ArchetypeProficiencySet(
            archetypeId.ToLowerInvariant(),
            displayName,
            description,
            categoriesList);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Public Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if this archetype is proficient with a specified weapon category.
    /// </summary>
    /// <param name="category">The weapon category to check.</param>
    /// <returns>
    /// <c>true</c> if the archetype is proficient with the category;
    /// <c>false</c> otherwise.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is the primary way to determine if a character of this archetype
    /// can use a weapon category without penalties.
    /// </para>
    /// <para>
    /// Proficiency lookup is O(n) where n is the number of proficient categories
    /// (typically 3-11). For performance-critical code paths, consider caching
    /// the result per character.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var mysticSet = ... // Load mystic proficiency set
    /// 
    /// // Check proficiency
    /// bool canUseDagger = mysticSet.IsProficientWith(WeaponCategory.Daggers); // true
    /// bool canUseSword = mysticSet.IsProficientWith(WeaponCategory.Swords);   // false
    /// </code>
    /// </example>
    public bool IsProficientWith(WeaponCategory category)
    {
        return _proficientCategories.Contains(category);
    }

    /// <summary>
    /// Gets the starting proficiency level for a weapon category.
    /// </summary>
    /// <param name="category">The weapon category to check.</param>
    /// <returns>
    /// <see cref="WeaponProficiencyLevel.Proficient"/> if the archetype is proficient
    /// with the category; <see cref="WeaponProficiencyLevel.NonProficient"/> otherwise.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is used during character creation to initialize proficiency
    /// levels for all weapon categories based on the selected archetype.
    /// </para>
    /// <para>
    /// Starting levels are binary (Proficient or NonProficient). Higher levels
    /// (Expert, Master) must be acquired through gameplay (v0.16.1e).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var warriorSet = ... // Load warrior proficiency set
    /// 
    /// // Initialize character proficiencies
    /// foreach (var category in Enum.GetValues&lt;WeaponCategory&gt;())
    /// {
    ///     var startingLevel = warriorSet.GetStartingLevel(category);
    ///     character.SetProficiency(category, startingLevel);
    /// }
    /// </code>
    /// </example>
    public WeaponProficiencyLevel GetStartingLevel(WeaponCategory category)
    {
        return IsProficientWith(category)
            ? WeaponProficiencyLevel.Proficient
            : WeaponProficiencyLevel.NonProficient;
    }

    /// <summary>
    /// Gets the count of weapon categories this archetype is proficient with.
    /// </summary>
    /// <returns>The number of proficient categories (1-11).</returns>
    /// <remarks>
    /// This count is used to determine archetype classification
    /// (<see cref="IsVersatile"/>, <see cref="IsSpecialist"/>).
    /// </remarks>
    public int GetProficientCategoryCount() => _proficientCategories.Count;

    /// <summary>
    /// Gets all weapon categories this archetype is NOT proficient with.
    /// </summary>
    /// <returns>
    /// A list of weapon categories where the archetype starts with
    /// <see cref="WeaponProficiencyLevel.NonProficient"/>.
    /// </returns>
    /// <remarks>
    /// This method is useful for displaying proficiency warnings in the UI
    /// or for calculating training requirements.
    /// </remarks>
    public IReadOnlyList<WeaponCategory> GetNonProficientCategories()
    {
        return Enum.GetValues<WeaponCategory>()
            .Where(c => !IsProficientWith(c))
            .ToList()
            .AsReadOnly();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Object Overrides
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of this proficiency set.
    /// </summary>
    /// <returns>
    /// A string containing the display name, archetype ID, and proficiency count.
    /// </returns>
    /// <example>
    /// Returns: "Warrior (warrior): 11 proficiencies"
    /// </example>
    public override string ToString() =>
        $"{DisplayName} ({ArchetypeId}): {GetProficientCategoryCount()} proficiencies";
}
