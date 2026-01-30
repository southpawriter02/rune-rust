// ═══════════════════════════════════════════════════════════════════════════════
// RecommendedBuild.cs
// Value object representing a recommended attribute allocation build for a
// specific archetype during character creation. Each build defines optimal
// attribute distributions based on the archetype's combat role, with optional
// lineage-specific optimization.
// Version: 0.17.3e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents a recommended attribute allocation build for character creation.
/// </summary>
/// <remarks>
/// <para>
/// RecommendedBuild provides pre-configured attribute distributions optimized
/// for each archetype's combat role and playstyle. These builds are used in
/// Simple mode (quick-start) during character creation Step 3 (Attributes).
/// </para>
/// <para>
/// Each build defines the five core attribute values (Might, Finesse, Wits,
/// Will, Sturdiness) that sum to the archetype's point budget (15 for most
/// archetypes, 14 for Adept). An optional <see cref="OptimalLineage"/>
/// indicates which lineage synergizes best with this particular build.
/// </para>
/// <para>
/// Standard recommended builds per archetype:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <b>Warrior</b>: MIGHT 4, FINESSE 3, WITS 2, WILL 2, STURDINESS 4 (15 pts)
///       — Emphasizes physical power and endurance for tanking
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Skirmisher</b>: MIGHT 3, FINESSE 4, WITS 3, WILL 2, STURDINESS 3 (15 pts)
///       — Emphasizes agility and precision for mobile combat
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Mystic</b>: MIGHT 2, FINESSE 3, WITS 4, WILL 4, STURDINESS 2 (15 pts)
///       — Emphasizes mental attributes for Aether channeling
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Adept</b>: MIGHT 3, FINESSE 3, WITS 3, WILL 2, STURDINESS 3 (14 pts)
///       — Balanced allocation reflecting versatile support role
///     </description>
///   </item>
/// </list>
/// <para>
/// RecommendedBuild instances are immutable value objects. Use the
/// <see cref="Create(string, int, int, int, int, int, Lineage?, ILogger{RecommendedBuild}?)"/>
/// factory method to create validated instances.
/// </para>
/// </remarks>
/// <param name="Name">
/// Display name for this build (e.g., "Standard Warrior", "Mystic ClanBorn Optimized").
/// </param>
/// <param name="Might">
/// Recommended Might attribute value. Range: 1-10.
/// Affects melee damage, carrying capacity, and physical feats.
/// </param>
/// <param name="Finesse">
/// Recommended Finesse attribute value. Range: 1-10.
/// Affects agility, precision, ranged accuracy, and evasion.
/// </param>
/// <param name="Wits">
/// Recommended Wits attribute value. Range: 1-10.
/// Affects perception, knowledge, crafting, and Aether Pool.
/// </param>
/// <param name="Will">
/// Recommended Will attribute value. Range: 1-10.
/// Affects mental fortitude, Aether channeling, and resistance to Corruption.
/// </param>
/// <param name="Sturdiness">
/// Recommended Sturdiness attribute value. Range: 1-10.
/// Affects endurance, HP, resilience, and Soak.
/// </param>
/// <param name="OptimalLineage">
/// Optional lineage that synergizes best with this build. Null for the
/// default (lineage-agnostic) build.
/// </param>
/// <seealso cref="Archetype"/>
/// <seealso cref="Lineage"/>
/// <seealso cref="ArchetypeResourceBonuses"/>
public readonly record struct RecommendedBuild(
    string Name,
    int Might,
    int Finesse,
    int Wits,
    int Will,
    int Sturdiness,
    Lineage? OptimalLineage)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for diagnostic output during RecommendedBuild operations.
    /// </summary>
    private static ILogger<RecommendedBuild>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Minimum value for any individual attribute.
    /// </summary>
    /// <value>1</value>
    private const int MinAttributeValue = 1;

    /// <summary>
    /// Maximum value for any individual attribute.
    /// </summary>
    /// <value>10</value>
    private const int MaxAttributeValue = 10;

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the total attribute points allocated in this build.
    /// </summary>
    /// <value>
    /// Sum of all five attribute values. Typically 15 for most archetypes
    /// or 14 for the Adept archetype, plus 5 for the base (each attribute
    /// starts at 1, contributing 5 total from base values).
    /// </value>
    /// <remarks>
    /// This is the raw sum of attribute values, not the point-buy cost.
    /// All attributes start at 1, so a build with values {4, 3, 2, 2, 4}
    /// has a total of 15. The point-buy cost is calculated separately by
    /// the IAttributeAllocationService in the Application layer.
    /// </remarks>
    public int TotalAttributePoints => Might + Finesse + Wits + Will + Sturdiness;

    /// <summary>
    /// Gets whether this build is optimized for a specific lineage.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="OptimalLineage"/> has a value;
    /// <c>false</c> if this is a lineage-agnostic default build.
    /// </value>
    public bool HasOptimalLineage => OptimalLineage.HasValue;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new validated <see cref="RecommendedBuild"/> instance.
    /// </summary>
    /// <param name="name">Display name for this build.</param>
    /// <param name="might">Recommended Might value (1-10).</param>
    /// <param name="finesse">Recommended Finesse value (1-10).</param>
    /// <param name="wits">Recommended Wits value (1-10).</param>
    /// <param name="will">Recommended Will value (1-10).</param>
    /// <param name="sturdiness">Recommended Sturdiness value (1-10).</param>
    /// <param name="optimalLineage">Optional lineage for build optimization.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A validated <see cref="RecommendedBuild"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="name"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any attribute value is outside the range [1, 10].
    /// </exception>
    /// <example>
    /// <code>
    /// var build = RecommendedBuild.Create(
    ///     "Standard Warrior",
    ///     might: 4, finesse: 3, wits: 2, will: 2, sturdiness: 4);
    /// </code>
    /// </example>
    public static RecommendedBuild Create(
        string name,
        int might,
        int finesse,
        int wits,
        int will,
        int sturdiness,
        Lineage? optimalLineage = null,
        ILogger<RecommendedBuild>? logger = null)
    {
        _logger = logger ?? _logger;

        _logger?.LogDebug(
            "Creating RecommendedBuild: Name={Name}, M={Might}, F={Finesse}, " +
            "Wi={Wits}, Wl={Will}, S={Sturdiness}, OptimalLineage={OptimalLineage}",
            name, might, finesse, wits, will, sturdiness, optimalLineage);

        // Validate name
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // Validate attribute ranges
        ValidateAttributeRange(might, nameof(might));
        ValidateAttributeRange(finesse, nameof(finesse));
        ValidateAttributeRange(wits, nameof(wits));
        ValidateAttributeRange(will, nameof(will));
        ValidateAttributeRange(sturdiness, nameof(sturdiness));

        _logger?.LogDebug(
            "RecommendedBuild validation passed for '{Name}'",
            name);

        var build = new RecommendedBuild(
            name.Trim(),
            might,
            finesse,
            wits,
            will,
            sturdiness,
            optimalLineage);

        _logger?.LogInformation(
            "Created RecommendedBuild: '{Name}' [M:{Might} F:{Finesse} Wi:{Wits} " +
            "Wl:{Will} S:{Sturdiness}] Total={Total}, OptimalLineage={OptimalLineage}",
            build.Name,
            build.Might,
            build.Finesse,
            build.Wits,
            build.Will,
            build.Sturdiness,
            build.TotalAttributePoints,
            build.OptimalLineage?.ToString() ?? "None");

        return build;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a display summary of this build's attribute allocation.
    /// </summary>
    /// <returns>
    /// A formatted string showing all attribute values, e.g.,
    /// "Standard Warrior: M4 F3 Wi2 Wl2 S4 (15 pts)".
    /// </returns>
    /// <example>
    /// <code>
    /// var build = RecommendedBuild.Create("Standard Warrior", 4, 3, 2, 2, 4);
    /// var summary = build.GetDisplaySummary();
    /// // Returns: "Standard Warrior: M4 F3 Wi2 Wl2 S4 (15 pts)"
    /// </code>
    /// </example>
    public string GetDisplaySummary()
    {
        var lineageInfo = HasOptimalLineage
            ? $" [Optimal: {OptimalLineage}]"
            : string.Empty;

        return $"{Name}: M{Might} F{Finesse} Wi{Wits} Wl{Will} S{Sturdiness} " +
               $"({TotalAttributePoints} pts){lineageInfo}";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates that an attribute value is within the allowed range [1, 10].
    /// </summary>
    /// <param name="value">The attribute value to validate.</param>
    /// <param name="paramName">The parameter name for error messages.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is less than 1 or greater than 10.
    /// </exception>
    private static void ValidateAttributeRange(int value, string paramName)
    {
        if (value < MinAttributeValue || value > MaxAttributeValue)
        {
            _logger?.LogWarning(
                "Attribute value {Value} for '{ParamName}' is outside valid range [{Min}, {Max}]",
                value, paramName, MinAttributeValue, MaxAttributeValue);

            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                $"Attribute value must be between {MinAttributeValue} and {MaxAttributeValue}.");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a debug-friendly string representation of this build.
    /// </summary>
    /// <returns>
    /// A formatted string like "RecommendedBuild: Standard Warrior [M4 F3 Wi2 Wl2 S4]".
    /// </returns>
    public override string ToString()
    {
        return $"RecommendedBuild: {Name} [M{Might} F{Finesse} Wi{Wits} Wl{Will} S{Sturdiness}]";
    }
}
