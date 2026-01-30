// ═══════════════════════════════════════════════════════════════════════════════
// AttributeDescription.cs
// Value object encapsulating all display and relationship data for a single
// core attribute, including human-readable names, descriptions, and the lists
// of derived stats and skills each attribute affects.
// Version: 0.17.2a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Describes a core attribute with display properties and relationship mappings.
/// </summary>
/// <remarks>
/// <para>
/// AttributeDescription is a value object containing all information needed to
/// present an attribute in the UI and understand its mechanical relationships.
/// Descriptions are loaded from configuration for easy customization and
/// potential localization.
/// </para>
/// <para>
/// Each description includes:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>Display name (uppercase, e.g., "MIGHT") for consistent UI presentation</description>
///   </item>
///   <item>
///     <description>Short description (one-line summary for tooltips)</description>
///   </item>
///   <item>
///     <description>Detailed description (full explanation for help screens)</description>
///   </item>
///   <item>
///     <description>Affected stats (derived stats influenced by this attribute)</description>
///   </item>
///   <item>
///     <description>Affected skills (skills that use this attribute for checks)</description>
///   </item>
/// </list>
/// <para>
/// AttributeDescription instances are immutable value objects created via the
/// <see cref="Create"/> factory method, which validates all required parameters
/// and normalizes the display name to uppercase.
/// </para>
/// </remarks>
/// <param name="Attribute">The core attribute this description applies to.</param>
/// <param name="DisplayName">The uppercase display name (e.g., "MIGHT").</param>
/// <param name="ShortDescription">A one-line summary of the attribute for tooltips.</param>
/// <param name="DetailedDescription">A full explanation of the attribute's role and effects.</param>
/// <param name="AffectedStats">Derived stats influenced by this attribute (e.g., "Melee Damage", "Max HP").</param>
/// <param name="AffectedSkills">Skills that use this attribute for checks (e.g., "Combat", "Lore").</param>
/// <seealso cref="CoreAttribute"/>
public readonly record struct AttributeDescription(
    CoreAttribute Attribute,
    string DisplayName,
    string ShortDescription,
    string DetailedDescription,
    IReadOnlyList<string> AffectedStats,
    IReadOnlyList<string> AffectedSkills)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during description creation and access.
    /// </summary>
    private static ILogger<AttributeDescription>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this attribute affects any derived stats.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="AffectedStats"/> contains at least one entry;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// All five core attributes should affect at least one derived stat.
    /// A description with no affected stats may indicate incomplete configuration.
    /// </remarks>
    public bool HasAffectedStats => AffectedStats.Count > 0;

    /// <summary>
    /// Gets whether this attribute has any associated skills.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="AffectedSkills"/> contains at least one entry;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// All five core attributes should have at least one associated skill.
    /// A description with no affected skills may indicate incomplete configuration.
    /// </remarks>
    public bool HasAffectedSkills => AffectedSkills.Count > 0;

    /// <summary>
    /// Gets the number of derived stats affected by this attribute.
    /// </summary>
    /// <value>The count of entries in <see cref="AffectedStats"/>.</value>
    public int AffectedStatCount => AffectedStats.Count;

    /// <summary>
    /// Gets the number of skills using this attribute.
    /// </summary>
    /// <value>The count of entries in <see cref="AffectedSkills"/>.</value>
    public int AffectedSkillCount => AffectedSkills.Count;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new <see cref="AttributeDescription"/> with validation.
    /// </summary>
    /// <param name="attribute">The core attribute this description applies to.</param>
    /// <param name="displayName">
    /// The display name for the attribute. Will be normalized to uppercase
    /// (e.g., "might" becomes "MIGHT").
    /// </param>
    /// <param name="shortDescription">A one-line summary for tooltips (10-100 characters recommended).</param>
    /// <param name="detailedDescription">A full explanation for help screens (50-500 characters recommended).</param>
    /// <param name="affectedStats">
    /// Optional list of derived stats this attribute influences.
    /// Defaults to an empty list if null.
    /// </param>
    /// <param name="affectedSkills">
    /// Optional list of skills that use this attribute for checks.
    /// Defaults to an empty list if null.
    /// </param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>A new <see cref="AttributeDescription"/> instance with validated and normalized data.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="displayName"/>, <paramref name="shortDescription"/>,
    /// or <paramref name="detailedDescription"/> is null, empty, or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create a full attribute description for Might
    /// var mightDesc = AttributeDescription.Create(
    ///     CoreAttribute.Might,
    ///     "MIGHT",
    ///     "Physical power and raw strength",
    ///     "Might represents your character's physical power. It affects melee damage, carrying capacity, and physical feats of strength.",
    ///     new List&lt;string&gt; { "Melee Damage", "Carrying Capacity", "Stamina" },
    ///     new List&lt;string&gt; { "Combat", "Athletics", "Intimidation" });
    ///
    /// // Create a minimal description (no stats/skills)
    /// var minimal = AttributeDescription.Create(
    ///     CoreAttribute.Wits,
    ///     "WITS",
    ///     "Perception and knowledge",
    ///     "Wits represents mental acuity...");
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// This factory method is the primary way to create attribute descriptions.
    /// All string parameters are validated for null/whitespace, and the display
    /// name is automatically normalized to uppercase via
    /// <see cref="string.ToUpperInvariant"/>.
    /// </para>
    /// <para>
    /// Null lists for affected stats and skills are converted to empty arrays
    /// to prevent null reference issues in downstream code.
    /// </para>
    /// </remarks>
    public static AttributeDescription Create(
        CoreAttribute attribute,
        string displayName,
        string shortDescription,
        string detailedDescription,
        IReadOnlyList<string>? affectedStats = null,
        IReadOnlyList<string>? affectedSkills = null,
        ILogger<AttributeDescription>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug(
            "Creating AttributeDescription. Attribute={Attribute}, DisplayName='{DisplayName}', " +
            "ShortDescription='{ShortDescription}', AffectedStatsCount={AffectedStatsCount}, " +
            "AffectedSkillsCount={AffectedSkillsCount}",
            attribute,
            displayName,
            shortDescription,
            affectedStats?.Count ?? 0,
            affectedSkills?.Count ?? 0);

        // Validate required string parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName, nameof(displayName));
        ArgumentException.ThrowIfNullOrWhiteSpace(shortDescription, nameof(shortDescription));
        ArgumentException.ThrowIfNullOrWhiteSpace(detailedDescription, nameof(detailedDescription));

        // Normalize display name to uppercase for consistent UI presentation
        var normalizedDisplayName = displayName.ToUpperInvariant();

        // Convert null lists to empty arrays to prevent null reference issues
        var resolvedStats = affectedStats ?? Array.Empty<string>();
        var resolvedSkills = affectedSkills ?? Array.Empty<string>();

        _logger?.LogDebug(
            "Validation passed for AttributeDescription. " +
            "Attribute={Attribute}, NormalizedDisplayName='{NormalizedDisplayName}', " +
            "AffectedStatsCount={AffectedStatsCount}, AffectedSkillsCount={AffectedSkillsCount}",
            attribute,
            normalizedDisplayName,
            resolvedStats.Count,
            resolvedSkills.Count);

        var description = new AttributeDescription(
            attribute,
            normalizedDisplayName,
            shortDescription,
            detailedDescription,
            resolvedStats,
            resolvedSkills);

        _logger?.LogInformation(
            "Created AttributeDescription for {Attribute}: {DisplayName}. " +
            "ShortDescription='{ShortDescription}', " +
            "AffectedStats=[{AffectedStatsSummary}], " +
            "AffectedSkills=[{AffectedSkillsSummary}]",
            attribute,
            normalizedDisplayName,
            shortDescription,
            description.GetAffectedStatsSummary(),
            description.GetAffectedSkillsSummary());

        return description;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a formatted string of affected stats for display.
    /// </summary>
    /// <returns>
    /// A comma-separated list of affected stats (e.g., "Melee Damage, Carrying Capacity, Stamina"),
    /// or "None" if the attribute has no affected stats.
    /// </returns>
    /// <example>
    /// <code>
    /// var desc = AttributeDescription.Create(
    ///     CoreAttribute.Might, "MIGHT", "Power", "Detailed...",
    ///     new List&lt;string&gt; { "Melee Damage", "Carrying Capacity" });
    /// desc.GetAffectedStatsSummary(); // "Melee Damage, Carrying Capacity"
    /// </code>
    /// </example>
    public string GetAffectedStatsSummary() =>
        AffectedStats.Count > 0
            ? string.Join(", ", AffectedStats)
            : "None";

    /// <summary>
    /// Gets a formatted string of affected skills for display.
    /// </summary>
    /// <returns>
    /// A comma-separated list of affected skills (e.g., "Combat, Athletics, Intimidation"),
    /// or "None" if the attribute has no affected skills.
    /// </returns>
    /// <example>
    /// <code>
    /// var desc = AttributeDescription.Create(
    ///     CoreAttribute.Might, "MIGHT", "Power", "Detailed...",
    ///     affectedSkills: new List&lt;string&gt; { "Combat", "Athletics" });
    /// desc.GetAffectedSkillsSummary(); // "Combat, Athletics"
    /// </code>
    /// </example>
    public string GetAffectedSkillsSummary() =>
        AffectedSkills.Count > 0
            ? string.Join(", ", AffectedSkills)
            : "None";

    /// <summary>
    /// Checks if this attribute affects a specific derived stat.
    /// </summary>
    /// <param name="statName">The stat name to check (case-insensitive).</param>
    /// <returns>
    /// <c>true</c> if this attribute affects the specified stat;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// var desc = AttributeDescription.Create(
    ///     CoreAttribute.Sturdiness, "STURDINESS", "Endurance", "Detailed...",
    ///     new List&lt;string&gt; { "Max HP", "Soak" });
    /// desc.AffectsStat("Max HP");     // true
    /// desc.AffectsStat("max hp");     // true (case-insensitive)
    /// desc.AffectsStat("Initiative"); // false
    /// </code>
    /// </example>
    public bool AffectsStat(string statName) =>
        AffectedStats.Any(s => s.Equals(statName, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Checks if this attribute is used by a specific skill.
    /// </summary>
    /// <param name="skillName">The skill name to check (case-insensitive).</param>
    /// <returns>
    /// <c>true</c> if this attribute is used by the specified skill;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// var desc = AttributeDescription.Create(
    ///     CoreAttribute.Finesse, "FINESSE", "Agility", "Detailed...",
    ///     affectedSkills: new List&lt;string&gt; { "Stealth", "Acrobatics", "Lockpicking" });
    /// desc.AffectsSkill("Stealth");   // true
    /// desc.AffectsSkill("stealth");   // true (case-insensitive)
    /// desc.AffectsSkill("Combat");    // false
    /// </code>
    /// </example>
    public bool AffectsSkill(string skillName) =>
        AffectedSkills.Any(s => s.Equals(skillName, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Gets a full tooltip text combining display name, short description, and detailed description.
    /// </summary>
    /// <returns>
    /// A formatted tooltip string with the display name on the first line,
    /// short description on the second line, and detailed description after
    /// a blank line separator.
    /// </returns>
    /// <example>
    /// <code>
    /// var desc = AttributeDescription.Create(
    ///     CoreAttribute.Will, "WILL", "Mental fortitude",
    ///     "Full detailed explanation here.");
    /// var tooltip = desc.GetTooltipText();
    /// // "WILL\nMental fortitude\n\nFull detailed explanation here."
    /// </code>
    /// </example>
    public string GetTooltipText() =>
        $"{DisplayName}\n{ShortDescription}\n\n{DetailedDescription}";

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of this attribute description.
    /// </summary>
    /// <returns>
    /// A string in the format "DISPLAYNAME: Short description"
    /// (e.g., "MIGHT: Physical power and raw strength").
    /// </returns>
    public override string ToString() =>
        $"{DisplayName}: {ShortDescription}";
}
