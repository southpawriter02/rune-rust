// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeSpecializationMapping.cs
// Value object representing the mapping between an archetype and its available
// specializations. Each archetype has access to a curated subset of
// specializations, with one recommended as the player's first choice.
// Specialization counts vary: Warrior (6), Adept (5), Skirmisher (4), Mystic (2).
// Version: 0.17.3d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Maps an archetype to its available specializations and recommended first choice.
/// </summary>
/// <remarks>
/// <para>
/// ArchetypeSpecializationMapping is an immutable value object that defines which
/// specializations a character can choose based on their archetype. Not all
/// specializations are available to all archetypes—each archetype has a curated
/// list that reflects its combat role and thematic identity.
/// </para>
/// <para>
/// Specialization counts vary by archetype, reflecting design intent:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <b>Warrior (6):</b> Guardian, Berserker, Weapon Master, Vanguard,
///       Juggernaut, Battle Commander — most versatile archetype
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Adept (5):</b> Alchemist, Artificer, Tactician, Herbalist,
///       Chronicler — broad utility focus
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Skirmisher (4):</b> Shadow Dancer, Duelist, Ranger, Acrobat —
///       focused mobility options
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Mystic (2):</b> Elementalist, Void Weaver — most focused archetype
///       with deep mastery paths
///     </description>
///   </item>
/// </list>
/// <para>
/// Each archetype has a <see cref="RecommendedFirst"/> specialization that serves
/// as the suggested starting point for new players. This recommendation is
/// highlighted in the character creation UI (Step 5).
/// </para>
/// <para>
/// Specialization IDs use kebab-case format (e.g., "shadow-dancer", "weapon-master")
/// and are normalized to lowercase during creation for consistent lookups against
/// the Specialization System (v0.17.4).
/// </para>
/// <para>
/// Use the <see cref="Create"/> factory method for validated construction from
/// arbitrary data (e.g., configuration loading). Static properties
/// <see cref="Warrior"/>, <see cref="Skirmisher"/>, <see cref="Mystic"/>, and
/// <see cref="Adept"/> provide canonical mappings for each archetype. The
/// <see cref="GetForArchetype"/> method retrieves the canonical mapping by enum value.
/// </para>
/// </remarks>
/// <param name="ArchetypeId">
/// The archetype this mapping applies to. Determines which specializations are
/// available during character creation Step 5.
/// </param>
/// <param name="AvailableSpecializations">
/// Read-only list of available specialization IDs in kebab-case format.
/// Each ID references a specialization definition in the Specialization System (v0.17.4).
/// List is normalized to lowercase during creation.
/// </param>
/// <param name="RecommendedFirst">
/// The recommended first specialization ID for new players. Must be present
/// in <see cref="AvailableSpecializations"/>. Highlighted in the character
/// creation UI with a ★ marker.
/// </param>
/// <seealso cref="RuneAndRust.Domain.Enums.Archetype"/>
/// <seealso cref="RuneAndRust.Domain.Entities.ArchetypeDefinition"/>
/// <seealso cref="ArchetypeResourceBonuses"/>
/// <seealso cref="ArchetypeAbilityGrant"/>
public readonly record struct ArchetypeSpecializationMapping(
    Archetype ArchetypeId,
    IReadOnlyList<string> AvailableSpecializations,
    string RecommendedFirst)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during specialization mapping
    /// creation, validation, and lookup operations.
    /// </summary>
    private static ILogger<ArchetypeSpecializationMapping>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the number of available specializations for this archetype.
    /// </summary>
    /// <value>
    /// An integer representing the count of specializations. Ranges from
    /// 2 (Mystic) to 6 (Warrior) for canonical archetype mappings.
    /// </value>
    /// <remarks>
    /// Specialization counts reflect archetype design philosophy:
    /// Warrior (6) is the most versatile, while Mystic (2) focuses on
    /// deep mastery of fewer paths. Adept (5) and Skirmisher (4) fall
    /// between these extremes.
    /// </remarks>
    public int Count => AvailableSpecializations.Count;

    /// <summary>
    /// Gets whether this archetype has any available specializations.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="Count"/> is greater than 0; otherwise, <c>false</c>.
    /// Always <c>true</c> for canonical archetype mappings since the
    /// <see cref="Create"/> factory method requires at least one specialization.
    /// </value>
    /// <remarks>
    /// This property is primarily useful as a guard check before iterating
    /// over <see cref="AvailableSpecializations"/> in UI code. For canonical
    /// mappings created via static properties, this always returns <c>true</c>.
    /// </remarks>
    public bool HasSpecializations => Count > 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the canonical specialization mapping for the Warrior archetype.
    /// </summary>
    /// <value>
    /// A mapping with 6 available specializations: guardian, berserker,
    /// weapon-master, vanguard, juggernaut, battle-commander.
    /// Recommended first: guardian.
    /// </value>
    /// <remarks>
    /// The Warrior has the most specializations (6), reflecting its versatility
    /// as the primary Tank / Melee DPS archetype. Guardian is recommended first
    /// as the core tanking specialization that reinforces the Warrior's
    /// defensive identity.
    /// </remarks>
    /// <example>
    /// <code>
    /// var mapping = ArchetypeSpecializationMapping.Warrior;
    /// // mapping.Count == 6
    /// // mapping.RecommendedFirst == "guardian"
    /// // mapping.IsSpecializationAvailable("berserker") == true
    /// </code>
    /// </example>
    public static ArchetypeSpecializationMapping Warrior => Create(
        Archetype.Warrior,
        ["guardian", "berserker", "weapon-master", "vanguard", "juggernaut", "battle-commander"],
        "guardian");

    /// <summary>
    /// Gets the canonical specialization mapping for the Skirmisher archetype.
    /// </summary>
    /// <value>
    /// A mapping with 4 available specializations: shadow-dancer, duelist,
    /// ranger, acrobat. Recommended first: shadow-dancer.
    /// </value>
    /// <remarks>
    /// The Skirmisher has 4 specializations focused on mobility and precision.
    /// Shadow Dancer is recommended first as the stealth/ambush specialization
    /// that complements the Skirmisher's hit-and-run playstyle.
    /// </remarks>
    /// <example>
    /// <code>
    /// var mapping = ArchetypeSpecializationMapping.Skirmisher;
    /// // mapping.Count == 4
    /// // mapping.RecommendedFirst == "shadow-dancer"
    /// </code>
    /// </example>
    public static ArchetypeSpecializationMapping Skirmisher => Create(
        Archetype.Skirmisher,
        ["shadow-dancer", "duelist", "ranger", "acrobat"],
        "shadow-dancer");

    /// <summary>
    /// Gets the canonical specialization mapping for the Mystic archetype.
    /// </summary>
    /// <value>
    /// A mapping with 2 available specializations: elementalist, void-weaver.
    /// Recommended first: elementalist.
    /// </value>
    /// <remarks>
    /// The Mystic has the fewest specializations (2), reflecting a design
    /// philosophy of deep mastery over breadth. Elementalist is recommended
    /// first as the straightforward elemental damage path, while Void Weaver
    /// offers a more complex dark Aether / control / debuff approach.
    /// </remarks>
    /// <example>
    /// <code>
    /// var mapping = ArchetypeSpecializationMapping.Mystic;
    /// // mapping.Count == 2
    /// // mapping.RecommendedFirst == "elementalist"
    /// </code>
    /// </example>
    public static ArchetypeSpecializationMapping Mystic => Create(
        Archetype.Mystic,
        ["elementalist", "void-weaver"],
        "elementalist");

    /// <summary>
    /// Gets the canonical specialization mapping for the Adept archetype.
    /// </summary>
    /// <value>
    /// A mapping with 5 available specializations: alchemist, artificer,
    /// tactician, herbalist, chronicler. Recommended first: alchemist.
    /// </value>
    /// <remarks>
    /// The Adept has 5 specializations spanning support, utility, and knowledge
    /// domains. Alchemist is recommended first as the consumable-crafting
    /// specialization that synergizes with the Adept's +20% consumable
    /// effectiveness special bonus.
    /// </remarks>
    /// <example>
    /// <code>
    /// var mapping = ArchetypeSpecializationMapping.Adept;
    /// // mapping.Count == 5
    /// // mapping.RecommendedFirst == "alchemist"
    /// </code>
    /// </example>
    public static ArchetypeSpecializationMapping Adept => Create(
        Archetype.Adept,
        ["alchemist", "artificer", "tactician", "herbalist", "chronicler"],
        "alchemist");

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a specialization mapping with validation and ID normalization.
    /// </summary>
    /// <param name="archetypeId">
    /// The archetype this mapping applies to.
    /// </param>
    /// <param name="availableSpecializations">
    /// Collection of available specialization IDs in kebab-case format.
    /// Must contain at least one entry. All IDs are normalized to lowercase.
    /// Must not be null.
    /// </param>
    /// <param name="recommendedFirst">
    /// The recommended first specialization ID. Must not be null or whitespace.
    /// Must be present in <paramref name="availableSpecializations"/> after
    /// normalization. Normalized to lowercase.
    /// </param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>
    /// A new <see cref="ArchetypeSpecializationMapping"/> instance with validated
    /// data and normalized (lowercase) specialization IDs.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="availableSpecializations"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="recommendedFirst"/> is null, empty, or whitespace,
    /// when <paramref name="availableSpecializations"/> is empty, or when
    /// <paramref name="recommendedFirst"/> is not present in the available list.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This factory method validates all parameters and normalizes specialization
    /// IDs to lowercase for consistent lookups against the Specialization System
    /// (v0.17.4). The <paramref name="recommendedFirst"/> value must exist in
    /// the <paramref name="availableSpecializations"/> collection after normalization.
    /// </para>
    /// <para>
    /// For canonical archetype mappings, prefer the static properties
    /// <see cref="Warrior"/>, <see cref="Skirmisher"/>, <see cref="Mystic"/>,
    /// and <see cref="Adept"/>, or use <see cref="GetForArchetype"/> for
    /// enum-based lookup.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a custom mapping for Warrior with a subset of specializations
    /// var mapping = ArchetypeSpecializationMapping.Create(
    ///     Archetype.Warrior,
    ///     new[] { "guardian", "berserker" },
    ///     "guardian");
    /// // mapping.Count == 2
    /// // mapping.RecommendedFirst == "guardian"
    /// </code>
    /// </example>
    public static ArchetypeSpecializationMapping Create(
        Archetype archetypeId,
        IEnumerable<string> availableSpecializations,
        string recommendedFirst,
        ILogger<ArchetypeSpecializationMapping>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug(
            "Creating ArchetypeSpecializationMapping with ArchetypeId={ArchetypeId}, " +
            "RecommendedFirst='{RecommendedFirst}'",
            archetypeId,
            recommendedFirst);

        // Validate that the specializations collection is not null
        ArgumentNullException.ThrowIfNull(availableSpecializations, nameof(availableSpecializations));

        // Validate that the recommended first specialization is not null or whitespace
        ArgumentException.ThrowIfNullOrWhiteSpace(recommendedFirst, nameof(recommendedFirst));

        // Normalize all specialization IDs to lowercase for consistent lookups
        var specList = availableSpecializations
            .Select(s => s.ToLowerInvariant())
            .ToList();

        // Validate that at least one specialization is available
        if (specList.Count == 0)
        {
            _logger?.LogWarning(
                "Validation failed for ArchetypeSpecializationMapping. ArchetypeId={ArchetypeId}: " +
                "at least one specialization must be available.",
                archetypeId);

            throw new ArgumentException(
                "At least one specialization must be available.",
                nameof(availableSpecializations));
        }

        // Normalize the recommended first specialization to lowercase
        var normalizedRecommended = recommendedFirst.ToLowerInvariant();

        // Validate that the recommended specialization exists in the available list
        if (!specList.Contains(normalizedRecommended))
        {
            _logger?.LogWarning(
                "Validation failed for ArchetypeSpecializationMapping. ArchetypeId={ArchetypeId}: " +
                "recommended specialization '{RecommendedFirst}' is not in the available list. " +
                "Available: [{AvailableSpecializations}]",
                archetypeId,
                recommendedFirst,
                string.Join(", ", specList));

            throw new ArgumentException(
                $"Recommended specialization '{recommendedFirst}' must be in available list.",
                nameof(recommendedFirst));
        }

        _logger?.LogDebug(
            "Validation passed for ArchetypeSpecializationMapping. ArchetypeId={ArchetypeId}, " +
            "SpecializationCount={Count}, RecommendedFirst='{RecommendedFirst}'. " +
            "All parameters validated successfully.",
            archetypeId,
            specList.Count,
            normalizedRecommended);

        var mapping = new ArchetypeSpecializationMapping(
            archetypeId,
            specList.AsReadOnly(),
            normalizedRecommended);

        _logger?.LogInformation(
            "Created ArchetypeSpecializationMapping. ArchetypeId={ArchetypeId}, " +
            "Count={Count}, RecommendedFirst='{RecommendedFirst}', " +
            "HasSpecializations={HasSpecializations}, " +
            "AvailableSpecializations=[{AvailableSpecializations}]",
            mapping.ArchetypeId,
            mapping.Count,
            mapping.RecommendedFirst,
            mapping.HasSpecializations,
            string.Join(", ", mapping.AvailableSpecializations));

        return mapping;
    }

    /// <summary>
    /// Gets the canonical specialization mapping for a specific archetype.
    /// </summary>
    /// <param name="archetype">
    /// The archetype to get the mapping for. Must be a valid <see cref="Archetype"/>
    /// enum value (Warrior, Skirmisher, Mystic, or Adept).
    /// </param>
    /// <returns>
    /// The canonical <see cref="ArchetypeSpecializationMapping"/> for the specified
    /// archetype, containing its full list of available specializations and
    /// recommended first choice.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="archetype"/> is not a recognized enum value.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method provides a convenient way to retrieve the canonical mapping
    /// for any archetype by enum value. It delegates to the corresponding static
    /// property (<see cref="Warrior"/>, <see cref="Skirmisher"/>,
    /// <see cref="Mystic"/>, or <see cref="Adept"/>).
    /// </para>
    /// <para>
    /// This method is primarily used by the Archetype Provider (v0.17.3e) and
    /// the character creation workflow (v0.17.5) to retrieve specialization
    /// options for the selected archetype.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var mapping = ArchetypeSpecializationMapping.GetForArchetype(Archetype.Warrior);
    /// // mapping.Count == 6
    /// // mapping.RecommendedFirst == "guardian"
    ///
    /// var mysticMapping = ArchetypeSpecializationMapping.GetForArchetype(Archetype.Mystic);
    /// // mysticMapping.Count == 2
    /// </code>
    /// </example>
    public static ArchetypeSpecializationMapping GetForArchetype(Archetype archetype) =>
        archetype switch
        {
            Archetype.Warrior => Warrior,
            Archetype.Skirmisher => Skirmisher,
            Archetype.Mystic => Mystic,
            Archetype.Adept => Adept,
            _ => throw new ArgumentOutOfRangeException(
                nameof(archetype),
                archetype,
                $"No specialization mapping defined for archetype '{archetype}'.")
        };

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks whether a specialization is available for this archetype.
    /// </summary>
    /// <param name="specializationId">
    /// The specialization ID to check in kebab-case format (e.g., "guardian",
    /// "shadow-dancer"). Comparison is case-insensitive.
    /// </param>
    /// <returns>
    /// <c>true</c> if the specified specialization is in the
    /// <see cref="AvailableSpecializations"/> list; <c>false</c> if it is not
    /// found, or if <paramref name="specializationId"/> is null or whitespace.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs a case-insensitive comparison against the available
    /// specializations list. It safely handles null or whitespace input by
    /// returning <c>false</c> rather than throwing an exception, making it
    /// suitable for direct use in UI validation logic.
    /// </para>
    /// <para>
    /// Used by the character creation workflow (v0.17.5) to validate that a
    /// player's specialization choice is valid for their selected archetype.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var mapping = ArchetypeSpecializationMapping.Warrior;
    /// mapping.IsSpecializationAvailable("guardian");        // true
    /// mapping.IsSpecializationAvailable("Guardian");        // true (case-insensitive)
    /// mapping.IsSpecializationAvailable("elementalist");    // false (Mystic only)
    /// mapping.IsSpecializationAvailable(null);              // false
    /// mapping.IsSpecializationAvailable("");                // false
    /// </code>
    /// </example>
    public bool IsSpecializationAvailable(string specializationId)
    {
        if (string.IsNullOrWhiteSpace(specializationId))
            return false;

        var normalizedId = specializationId.ToLowerInvariant();
        var specs = AvailableSpecializations;
        return specs.Any(s =>
            s.Equals(normalizedId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the available specialization IDs as an enumerable sequence.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of specialization ID strings in
    /// kebab-case format, in the order they were defined.
    /// </returns>
    /// <remarks>
    /// This method provides a convenient way to iterate over available
    /// specialization IDs without directly accessing the
    /// <see cref="AvailableSpecializations"/> list. The IDs are in lowercase
    /// kebab-case format (e.g., "guardian", "shadow-dancer").
    /// </remarks>
    /// <example>
    /// <code>
    /// var mapping = ArchetypeSpecializationMapping.Mystic;
    /// foreach (var id in mapping.GetAvailableIds())
    /// {
    ///     Console.WriteLine(id);
    /// }
    /// // Output: "elementalist", "void-weaver"
    /// </code>
    /// </example>
    public IEnumerable<string> GetAvailableIds() => AvailableSpecializations;

    /// <summary>
    /// Gets a display-friendly list of specializations for UI presentation.
    /// </summary>
    /// <param name="highlightRecommended">
    /// Whether to mark the recommended specialization with a ★ symbol and
    /// "(Recommended)" suffix. Defaults to <c>true</c>.
    /// </param>
    /// <returns>
    /// A read-only list of formatted specialization names. If
    /// <paramref name="highlightRecommended"/> is <c>true</c>, the recommended
    /// specialization is prefixed with "★ " and suffixed with " (Recommended)".
    /// All names are converted from kebab-case to Title Case for display.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is designed for the character creation UI (Step 5) where
    /// available specializations are displayed to the player. The recommended
    /// first specialization is visually distinguished to guide new players.
    /// </para>
    /// <para>
    /// Kebab-case IDs are converted to Title Case for display (e.g.,
    /// "shadow-dancer" becomes "Shadow Dancer", "weapon-master" becomes
    /// "Weapon Master").
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var mapping = ArchetypeSpecializationMapping.Skirmisher;
    /// var displayList = mapping.GetDisplayList();
    /// // ["★ Shadow Dancer (Recommended)", "Duelist", "Ranger", "Acrobat"]
    ///
    /// var plainList = mapping.GetDisplayList(highlightRecommended: false);
    /// // ["Shadow Dancer", "Duelist", "Ranger", "Acrobat"]
    /// </code>
    /// </example>
    public IReadOnlyList<string> GetDisplayList(bool highlightRecommended = true)
    {
        var recommended = RecommendedFirst;
        return AvailableSpecializations
            .Select(s => highlightRecommended && s == recommended
                ? $"★ {FormatForDisplay(s)} (Recommended)"
                : FormatForDisplay(s))
            .ToList();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Converts a kebab-case string to Title Case for display purposes.
    /// </summary>
    /// <param name="kebabCase">
    /// A kebab-case string (e.g., "shadow-dancer", "weapon-master").
    /// </param>
    /// <returns>
    /// A Title Case string with hyphens replaced by spaces
    /// (e.g., "Shadow Dancer", "Weapon Master").
    /// </returns>
    private static string FormatForDisplay(string kebabCase) =>
        string.Join(" ", kebabCase.Split('-').Select(word =>
            char.ToUpperInvariant(word[0]) + word[1..]));

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of this specialization mapping.
    /// </summary>
    /// <returns>
    /// A string in the format "{ArchetypeId}: {Count} specializations (recommended: {RecommendedFirst})"
    /// (e.g., "Warrior: 6 specializations (recommended: guardian)").
    /// </returns>
    public override string ToString() =>
        $"{ArchetypeId}: {Count} specializations (recommended: {RecommendedFirst})";
}
