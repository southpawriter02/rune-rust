// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeArmorProficiencySet.cs
// Entity defining which armor categories an archetype is proficient with.
// Version: 0.16.2c
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Defines which armor categories an archetype is proficient with.
/// </summary>
/// <remarks>
/// <para>
/// ArchetypeArmorProficiencySet establishes the starting armor proficiencies for
/// each playable archetype in the game. It maps an archetype (Warrior, Skirmisher,
/// Mystic, Adept) to a set of armor categories they can use effectively.
/// </para>
/// <para>
/// Key responsibilities:
/// <list type="bullet">
///   <item><description>Define which armor categories an archetype starts proficient with</description></item>
///   <item><description>Define which categories are explicitly non-proficient</description></item>
///   <item><description>Store Galdr interference rules for caster archetypes</description></item>
///   <item><description>Document special restrictions for UI display</description></item>
/// </list>
/// </para>
/// <para>
/// Standard archetype proficiencies:
/// <list type="table">
///   <listheader>
///     <term>Archetype</term>
///     <description>Proficient Categories</description>
///   </listheader>
///   <item>
///     <term>Warrior</term>
///     <description>Light, Medium, Heavy, Shields</description>
///   </item>
///   <item>
///     <term>Skirmisher</term>
///     <description>Light, Medium, Shields</description>
///   </item>
///   <item>
///     <term>Mystic</term>
///     <description>Light only (Galdr interference)</description>
///   </item>
///   <item>
///     <term>Adept</term>
///     <description>Light only (WITS interference)</description>
///   </item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="ArmorCategory"/>
/// <seealso cref="ArmorProficiencyLevel"/>
/// <seealso cref="GaldrInterference"/>
public class ArchetypeArmorProficiencySet : IEntity
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this proficiency set.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies this entity.</value>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the archetype identifier this proficiency set applies to.
    /// </summary>
    /// <value>
    /// The lowercase kebab-case archetype ID (e.g., "warrior", "mystic").
    /// </value>
    /// <remarks>
    /// This ID corresponds to the archetype configuration in archetypes.json
    /// and is used for lookup operations by the provider.
    /// </remarks>
    public string ArchetypeId { get; private set; }

    /// <summary>
    /// Gets the display name of the archetype.
    /// </summary>
    /// <value>
    /// The human-readable archetype name (e.g., "Warrior", "Mystic").
    /// </value>
    /// <remarks>
    /// Used for UI display and logging messages.
    /// </remarks>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Gets the armor categories this archetype is proficient with.
    /// </summary>
    /// <value>
    /// A read-only list of <see cref="ArmorCategory"/> values the archetype
    /// has proficiency in at character creation.
    /// </value>
    /// <remarks>
    /// Characters start at <see cref="ArmorProficiencyLevel.Proficient"/> for
    /// these categories. Additional proficiency can be gained through training.
    /// </remarks>
    public IReadOnlyList<ArmorCategory> ProficientCategories { get; private set; }

    /// <summary>
    /// Gets the armor categories this archetype is explicitly non-proficient with.
    /// </summary>
    /// <value>
    /// A read-only list of <see cref="ArmorCategory"/> values the archetype
    /// cannot effectively use without training.
    /// </value>
    /// <remarks>
    /// Characters wearing non-proficient armor suffer doubled penalties,
    /// -2 attack modifier, and cannot activate special armor properties.
    /// </remarks>
    public IReadOnlyList<ArmorCategory> NonProficientCategories { get; private set; }

    /// <summary>
    /// Gets the Galdr interference rules for this archetype, if applicable.
    /// </summary>
    /// <value>
    /// A <see cref="GaldrInterference"/> instance for caster archetypes,
    /// or <c>null</c> for martial archetypes with no interference.
    /// </value>
    /// <remarks>
    /// Only Mystics and Adepts have Galdr interference rules. Warriors and
    /// Skirmishers have no magical traditions affected by armor.
    /// </remarks>
    public GaldrInterference? GaldrInterference { get; private set; }

    /// <summary>
    /// Gets special restrictions text for UI display.
    /// </summary>
    /// <value>
    /// A human-readable description of special restrictions, or <c>null</c>
    /// if there are no special restrictions beyond standard proficiency rules.
    /// </value>
    /// <remarks>
    /// Examples:
    /// <list type="bullet">
    ///   <item><description>"Cannot effectively use Tower Shields"</description></item>
    ///   <item><description>"Galdr blocked in Heavy armor"</description></item>
    /// </list>
    /// </remarks>
    public string? SpecialRestrictions { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private ArchetypeArmorProficiencySet()
    {
        ArchetypeId = null!;
        DisplayName = null!;
        ProficientCategories = null!;
        NonProficientCategories = null!;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new archetype armor proficiency set with validation.
    /// </summary>
    /// <param name="archetypeId">The archetype identifier (kebab-case).</param>
    /// <param name="displayName">The human-readable archetype name.</param>
    /// <param name="proficientCategories">Categories the archetype is proficient with.</param>
    /// <param name="nonProficientCategories">Categories the archetype is not proficient with.</param>
    /// <param name="galdrInterference">Optional Galdr interference rules for casters.</param>
    /// <param name="specialRestrictions">Optional special restrictions text.</param>
    /// <returns>A new validated <see cref="ArchetypeArmorProficiencySet"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="archetypeId"/> or <paramref name="displayName"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="proficientCategories"/> or <paramref name="nonProficientCategories"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// var warriorSet = ArchetypeArmorProficiencySet.Create(
    ///     "warrior",
    ///     "Warrior",
    ///     new[] { ArmorCategory.Light, ArmorCategory.Medium, ArmorCategory.Heavy, ArmorCategory.Shields },
    ///     new[] { ArmorCategory.Specialized });
    /// 
    /// var mysticSet = ArchetypeArmorProficiencySet.Create(
    ///     "mystic",
    ///     "Mystic",
    ///     new[] { ArmorCategory.Light },
    ///     new[] { ArmorCategory.Medium, ArmorCategory.Heavy, ArmorCategory.Shields, ArmorCategory.Specialized },
    ///     GaldrInterference.MysticRules,
    ///     "Galdr blocked in Heavy armor");
    /// </code>
    /// </example>
    public static ArchetypeArmorProficiencySet Create(
        string archetypeId,
        string displayName,
        IReadOnlyList<ArmorCategory> proficientCategories,
        IReadOnlyList<ArmorCategory> nonProficientCategories,
        GaldrInterference? galdrInterference = null,
        string? specialRestrictions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archetypeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(proficientCategories);
        ArgumentNullException.ThrowIfNull(nonProficientCategories);

        return new ArchetypeArmorProficiencySet
        {
            Id = Guid.NewGuid(),
            ArchetypeId = archetypeId.ToLowerInvariant(),
            DisplayName = displayName,
            ProficientCategories = proficientCategories,
            NonProficientCategories = nonProficientCategories,
            GaldrInterference = galdrInterference,
            SpecialRestrictions = specialRestrictions
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Query Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines if this archetype is proficient with the specified armor category.
    /// </summary>
    /// <param name="category">The armor category to check.</param>
    /// <returns>
    /// <c>true</c> if the archetype starts proficient with this category;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Proficiency means the character uses standard armor penalties (1.0x multiplier).
    /// Non-proficiency doubles all penalties and imposes attack penalties.
    /// </remarks>
    /// <example>
    /// <code>
    /// var warriorSet = GetWarriorProficiencySet();
    /// Console.WriteLine(warriorSet.IsProficientWith(ArmorCategory.Heavy));  // True
    /// Console.WriteLine(warriorSet.IsProficientWith(ArmorCategory.Specialized)); // False
    /// </code>
    /// </example>
    public bool IsProficientWith(ArmorCategory category) =>
        ProficientCategories.Contains(category);

    /// <summary>
    /// Gets the starting proficiency level for the specified armor category.
    /// </summary>
    /// <param name="category">The armor category to check.</param>
    /// <returns>
    /// <see cref="ArmorProficiencyLevel.Proficient"/> if the archetype is proficient;
    /// otherwise, <see cref="ArmorProficiencyLevel.NonProficient"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Starting proficiency determines the initial state for a new character.
    /// Characters can improve from NonProficient to Proficient through training.
    /// </para>
    /// <para>
    /// Note: Characters never start at Expert or Master proficiency; these
    /// levels require significant in-game advancement.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var mysticSet = GetMysticProficiencySet();
    /// Console.WriteLine(mysticSet.GetStartingProficiency(ArmorCategory.Light));  // Proficient
    /// Console.WriteLine(mysticSet.GetStartingProficiency(ArmorCategory.Heavy));  // NonProficient
    /// </code>
    /// </example>
    public ArmorProficiencyLevel GetStartingProficiency(ArmorCategory category) =>
        IsProficientWith(category)
            ? ArmorProficiencyLevel.Proficient
            : ArmorProficiencyLevel.NonProficient;

    /// <summary>
    /// Determines if the specified armor category blocks Galdr/WITS abilities.
    /// </summary>
    /// <param name="category">The armor category to check.</param>
    /// <returns>
    /// <c>true</c> if this archetype cannot use Galdr/WITS abilities while wearing
    /// armor of this category; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Returns <c>false</c> for archetypes without Galdr interference rules
    /// (Warriors, Skirmishers), even for heavy armor.
    /// </para>
    /// <para>
    /// For Mystics: Heavy armor and Shields are BLOCKED.
    /// For Adepts: Only Shields are BLOCKED (Heavy armor has penalties but not blocked).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var mysticSet = GetMysticProficiencySet();
    /// Console.WriteLine(mysticSet.IsGaldrBlockedBy(ArmorCategory.Heavy));   // True
    /// Console.WriteLine(mysticSet.IsGaldrBlockedBy(ArmorCategory.Medium));  // False (penalty only)
    /// </code>
    /// </example>
    public bool IsGaldrBlockedBy(ArmorCategory category)
    {
        if (GaldrInterference is null)
            return false;

        return GaldrInterference.Value.IsBlockedBy(category);
    }

    /// <summary>
    /// Gets the Galdr/WITS penalty for wearing the specified armor category.
    /// </summary>
    /// <param name="category">The armor category being worn.</param>
    /// <returns>
    /// The penalty value (negative number), or 0 if no penalty applies.
    /// Returns 0 for archetypes without Galdr interference or if blocked.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Penalties reduce Galdr or WITS check results. Common values:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>-2: Medium armor penalty</description></item>
    ///   <item><description>-4: Heavy armor penalty for Adepts</description></item>
    ///   <item><description>0: Light armor or no interference</description></item>
    /// </list>
    /// <para>
    /// If the category causes blocking, the penalty returned is 0 since
    /// the character cannot use Galdr/WITS at all.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var adeptSet = GetAdeptProficiencySet();
    /// Console.WriteLine(adeptSet.GetGaldrPenalty(ArmorCategory.Medium));  // -2
    /// Console.WriteLine(adeptSet.GetGaldrPenalty(ArmorCategory.Heavy));   // -4
    /// Console.WriteLine(adeptSet.GetGaldrPenalty(ArmorCategory.Light));   // 0
    /// </code>
    /// </example>
    public int GetGaldrPenalty(ArmorCategory category)
    {
        if (GaldrInterference is null)
            return 0;

        return GaldrInterference.Value.GetPenaltyFor(category);
    }

    /// <summary>
    /// Determines if this archetype can use Galdr/WITS abilities with the given armor.
    /// </summary>
    /// <param name="category">The armor category being worn.</param>
    /// <returns>
    /// <c>true</c> if Galdr/WITS abilities can be used (possibly with penalty);
    /// <c>false</c> if completely blocked.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Returns <c>true</c> for archetypes without Galdr interference rules
    /// since they don't use Galdr/WITS abilities that could be affected.
    /// </para>
    /// <para>
    /// For caster archetypes, this checks whether the armor category blocks
    /// their magical or mental abilities entirely.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var mysticSet = GetMysticProficiencySet();
    /// if (mysticSet.CanCastGaldr(ArmorCategory.Medium))
    /// {
    ///     var penalty = mysticSet.GetGaldrPenalty(ArmorCategory.Medium);
    ///     Console.WriteLine($"Can cast with {penalty} penalty"); // "Can cast with -2 penalty"
    /// }
    /// </code>
    /// </example>
    public bool CanCastGaldr(ArmorCategory category)
    {
        if (GaldrInterference is null)
            return true;  // Non-casters aren't affected

        return GaldrInterference.Value.CanCastWith(category);
    }

    /// <summary>
    /// Gets whether this archetype has any Galdr/WITS interference rules.
    /// </summary>
    /// <value>
    /// <c>true</c> if this archetype experiences Galdr/WITS interference from armor;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Mystics and Adepts return <c>true</c>; Warriors and Skirmishers return <c>false</c>.
    /// </remarks>
    public bool HasGaldrInterference => GaldrInterference.HasValue;

    // ═══════════════════════════════════════════════════════════════════════════
    // Formatting Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the proficient categories as a comma-separated display string.
    /// </summary>
    /// <returns>
    /// A formatted string listing proficient categories (e.g., "Light, Medium, Shields").
    /// </returns>
    public string FormatProficientCategories() =>
        string.Join(", ", ProficientCategories);

    /// <summary>
    /// Gets the non-proficient categories as a comma-separated display string.
    /// </summary>
    /// <returns>
    /// A formatted string listing non-proficient categories (e.g., "Heavy, Specialized").
    /// </returns>
    public string FormatNonProficientCategories() =>
        string.Join(", ", NonProficientCategories);

    /// <summary>
    /// Returns a string representation for debugging and logging.
    /// </summary>
    /// <returns>A formatted string summarizing the proficiency set.</returns>
    /// <example>
    /// <code>
    /// var mysticSet = GetMysticProficiencySet();
    /// Console.WriteLine(mysticSet.ToString());
    /// // Output: "ArchetypeArmorProficiencySet { Archetype: mystic, Proficient: [Light], HasGaldr: True }"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"ArchetypeArmorProficiencySet {{ " +
        $"Archetype: {ArchetypeId}, " +
        $"Proficient: [{FormatProficientCategories()}], " +
        $"HasGaldr: {HasGaldrInterference} }}";
}
