// ═══════════════════════════════════════════════════════════════════════════════
// BackgroundSkillGrant.cs
// Value object representing a skill bonus granted by a background during
// character creation. Each background typically grants a primary (+2) and
// secondary (+1) skill bonus reflecting pre-Silence professional expertise.
// Version: 0.17.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents a skill bonus granted by a background.
/// </summary>
/// <remarks>
/// <para>
/// BackgroundSkillGrant is a value object pairing a skill identifier with a bonus
/// amount and grant type. Skills are identified by lowercase string IDs to support
/// configuration-driven skill systems.
/// </para>
/// <para>
/// Each background typically grants two skill bonuses:
/// </para>
/// <list type="bullet">
///   <item><description>Primary skill (+2): Core professional expertise from pre-Silence work</description></item>
///   <item><description>Secondary skill (+1): Related knowledge gained through the profession</description></item>
/// </list>
/// <para>
/// Skill grants are defined in configuration (backgrounds.json) and loaded via the
/// IBackgroundProvider (v0.17.1d). The grant type determines how the bonus is applied
/// to the character during creation.
/// </para>
/// </remarks>
/// <param name="SkillId">The lowercase skill identifier (e.g., "craft", "medicine", "combat").</param>
/// <param name="BonusAmount">The bonus amount to add (typically +1 or +2).</param>
/// <param name="GrantType">How the bonus is applied (Permanent, StartingBonus, Proficiency).</param>
/// <seealso cref="SkillGrantType"/>
/// <seealso cref="RuneAndRust.Domain.Entities.BackgroundDefinition"/>
public readonly record struct BackgroundSkillGrant(
    string SkillId,
    int BonusAmount,
    SkillGrantType GrantType)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Logger for detailed diagnostic output during grant creation.
    /// </summary>
    private static ILogger<BackgroundSkillGrant>? _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this is a permanent skill bonus.
    /// </summary>
    /// <value>
    /// <c>true</c> if the grant type is <see cref="SkillGrantType.Permanent"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Permanent bonuses are added to the base skill value and persist throughout
    /// the entire game. All standard background grants use this type.
    /// </remarks>
    public bool IsPermanent => GrantType == SkillGrantType.Permanent;

    /// <summary>
    /// Gets whether this is a starting bonus only.
    /// </summary>
    /// <value>
    /// <c>true</c> if the grant type is <see cref="SkillGrantType.StartingBonus"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Starting bonuses provide an initial advantage but are subsumed by
    /// later skill training if it exceeds the bonus amount.
    /// </remarks>
    public bool IsStartingBonus => GrantType == SkillGrantType.StartingBonus;

    /// <summary>
    /// Gets whether this grants proficiency (removes untrained penalty).
    /// </summary>
    /// <value>
    /// <c>true</c> if the grant type is <see cref="SkillGrantType.Proficiency"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Proficiency grants do not provide a numeric bonus but remove the -2
    /// untrained penalty when using the skill.
    /// </remarks>
    public bool IsProficiency => GrantType == SkillGrantType.Proficiency;

    /// <summary>
    /// Gets whether this grant provides a numeric bonus greater than zero.
    /// </summary>
    /// <value>
    /// <c>true</c> if the bonus amount is positive; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Proficiency grants typically have a bonus amount of 0 since they only
    /// remove the untrained penalty. Permanent and StartingBonus grants
    /// typically have positive values (+1 or +2).
    /// </remarks>
    public bool HasNumericBonus => BonusAmount > 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new BackgroundSkillGrant with validation.
    /// </summary>
    /// <param name="skillId">The skill identifier (will be normalized to lowercase).</param>
    /// <param name="bonusAmount">The bonus amount (must be non-negative).</param>
    /// <param name="grantType">The grant type (defaults to <see cref="SkillGrantType.Permanent"/>).</param>
    /// <param name="logger">Optional logger for diagnostic output during creation.</param>
    /// <returns>A new <see cref="BackgroundSkillGrant"/> instance with the validated parameters.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="skillId"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="bonusAmount"/> is negative.</exception>
    /// <example>
    /// <code>
    /// // Create a permanent +2 Craft bonus for Village Smith
    /// var craftGrant = BackgroundSkillGrant.Create("craft", 2, SkillGrantType.Permanent);
    ///
    /// // Create with default Permanent type
    /// var mightGrant = BackgroundSkillGrant.Create("might", 1);
    /// </code>
    /// </example>
    public static BackgroundSkillGrant Create(
        string skillId,
        int bonusAmount,
        SkillGrantType grantType = SkillGrantType.Permanent,
        ILogger<BackgroundSkillGrant>? logger = null)
    {
        _logger = logger;

        _logger?.LogDebug(
            "Creating BackgroundSkillGrant. SkillId='{SkillId}', BonusAmount={BonusAmount}, GrantType={GrantType}",
            skillId,
            bonusAmount,
            grantType);

        // Validate required parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(skillId, nameof(skillId));
        ArgumentOutOfRangeException.ThrowIfNegative(bonusAmount, nameof(bonusAmount));

        var normalizedSkillId = skillId.ToLowerInvariant();

        _logger?.LogDebug(
            "Validation passed for BackgroundSkillGrant. " +
            "NormalizedSkillId='{NormalizedSkillId}', BonusAmount={BonusAmount}, GrantType={GrantType}",
            normalizedSkillId,
            bonusAmount,
            grantType);

        var grant = new BackgroundSkillGrant(
            normalizedSkillId,
            bonusAmount,
            grantType);

        _logger?.LogInformation(
            "Created BackgroundSkillGrant: {Grant}",
            grant);

        return grant;
    }

    /// <summary>
    /// Creates a permanent skill bonus (most common type for background grants).
    /// </summary>
    /// <param name="skillId">The skill identifier (will be normalized to lowercase).</param>
    /// <param name="bonusAmount">The bonus amount (typically +1 or +2).</param>
    /// <returns>A new <see cref="BackgroundSkillGrant"/> with <see cref="SkillGrantType.Permanent"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="skillId"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="bonusAmount"/> is negative.</exception>
    /// <example>
    /// <code>
    /// var craftGrant = BackgroundSkillGrant.Permanent("craft", 2);
    /// // Equivalent to: Create("craft", 2, SkillGrantType.Permanent)
    /// </code>
    /// </example>
    public static BackgroundSkillGrant Permanent(string skillId, int bonusAmount) =>
        Create(skillId, bonusAmount, SkillGrantType.Permanent);

    /// <summary>
    /// Creates a starting bonus skill grant.
    /// </summary>
    /// <param name="skillId">The skill identifier (will be normalized to lowercase).</param>
    /// <param name="bonusAmount">The bonus amount.</param>
    /// <returns>A new <see cref="BackgroundSkillGrant"/> with <see cref="SkillGrantType.StartingBonus"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="skillId"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="bonusAmount"/> is negative.</exception>
    public static BackgroundSkillGrant Starting(string skillId, int bonusAmount) =>
        Create(skillId, bonusAmount, SkillGrantType.StartingBonus);

    /// <summary>
    /// Creates a proficiency grant (no numeric bonus, removes untrained penalty).
    /// </summary>
    /// <param name="skillId">The skill identifier (will be normalized to lowercase).</param>
    /// <returns>
    /// A new <see cref="BackgroundSkillGrant"/> with <see cref="SkillGrantType.Proficiency"/>
    /// and a bonus amount of 0.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="skillId"/> is null or whitespace.</exception>
    public static BackgroundSkillGrant Proficient(string skillId) =>
        Create(skillId, 0, SkillGrantType.Proficiency);

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of this skill grant.
    /// </summary>
    /// <returns>
    /// A string in the format "skillId +N" for numeric bonuses,
    /// "skillId +N (starting)" for starting bonuses,
    /// or "skillId (proficient)" for proficiency grants.
    /// </returns>
    /// <example>
    /// <code>
    /// BackgroundSkillGrant.Permanent("craft", 2).ToString();    // "craft +2"
    /// BackgroundSkillGrant.Starting("might", 1).ToString();     // "might +1 (starting)"
    /// BackgroundSkillGrant.Proficient("traps").ToString();      // "traps (proficient)"
    /// </code>
    /// </example>
    public override string ToString()
    {
        if (GrantType == SkillGrantType.Proficiency)
            return $"{SkillId} (proficient)";

        var sign = BonusAmount >= 0 ? "+" : "";
        var typeLabel = GrantType == SkillGrantType.StartingBonus ? " (starting)" : "";
        return $"{SkillId} {sign}{BonusAmount}{typeLabel}";
    }
}
