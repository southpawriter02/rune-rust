// ═══════════════════════════════════════════════════════════════════════════════
// SkillBonus.cs
// Value object representing a bonus to a specific skill.
// Version: 0.17.0b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a bonus to a specific skill granted by a lineage or other source.
/// </summary>
/// <remarks>
/// <para>
/// SkillBonus is a simple value object pairing a skill identifier with a bonus amount.
/// Skills are identified by kebab-case string IDs (e.g., "social", "lore", "survival")
/// to allow configuration-driven skill systems and future extensibility.
/// </para>
/// <para>
/// Currently used for lineage passive bonuses, but the design supports reuse for:
/// </para>
/// <list type="bullet">
///   <item><description>Background bonuses (v0.17.1)</description></item>
///   <item><description>Equipment effects</description></item>
///   <item><description>Status effects</description></item>
///   <item><description>Temporary buffs/debuffs</description></item>
/// </list>
/// <para>
/// Skill IDs are normalized to lowercase for consistent comparison. The bonus amount
/// can be positive (buff) or negative (penalty), though lineage bonuses are always positive.
/// </para>
/// </remarks>
/// <param name="SkillId">The kebab-case skill identifier (e.g., "social", "lore").</param>
/// <param name="BonusAmount">The bonus amount to add to skill checks.</param>
/// <seealso cref="LineagePassiveBonuses"/>
public readonly record struct SkillBonus(string SkillId, int BonusAmount)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the skill ID normalized to lowercase for consistent comparison.
    /// </summary>
    /// <value>The lowercase version of the skill identifier.</value>
    /// <remarks>
    /// This property ensures that skill lookups are case-insensitive,
    /// regardless of how the skill ID was originally provided.
    /// </remarks>
    public string NormalizedSkillId => SkillId.ToLowerInvariant();

    /// <summary>
    /// Gets whether this represents a positive bonus (buff).
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="BonusAmount"/> is greater than zero;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool IsPositive => BonusAmount > 0;

    /// <summary>
    /// Gets whether this represents a penalty (negative bonus).
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="BonusAmount"/> is less than zero;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool IsPenalty => BonusAmount < 0;

    /// <summary>
    /// Gets whether this bonus has no effect (zero amount).
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="BonusAmount"/> equals zero;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool IsNeutral => BonusAmount == 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new <see cref="SkillBonus"/> with validation.
    /// </summary>
    /// <param name="skillId">The skill identifier (kebab-case preferred).</param>
    /// <param name="bonusAmount">The bonus amount to apply.</param>
    /// <returns>A new <see cref="SkillBonus"/> instance with normalized skill ID.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="skillId"/> is null or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// // Create a +1 Social skill bonus
    /// var socialBonus = SkillBonus.Create("social", 1);
    /// 
    /// // Create a -2 Stealth penalty
    /// var stealthPenalty = SkillBonus.Create("stealth", -2);
    /// </code>
    /// </example>
    public static SkillBonus Create(string skillId, int bonusAmount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(skillId, nameof(skillId));

        return new SkillBonus(skillId.ToLowerInvariant(), bonusAmount);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if this bonus applies to the specified skill.
    /// </summary>
    /// <param name="targetSkillId">The skill identifier to check against.</param>
    /// <returns>
    /// <c>true</c> if this bonus applies to the target skill;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// The comparison is case-insensitive. Returns <c>false</c> if
    /// <paramref name="targetSkillId"/> is null or whitespace.
    /// </remarks>
    /// <example>
    /// <code>
    /// var bonus = SkillBonus.Create("social", 1);
    /// 
    /// bonus.AppliesTo("social");   // Returns true
    /// bonus.AppliesTo("SOCIAL");   // Returns true (case-insensitive)
    /// bonus.AppliesTo("lore");     // Returns false
    /// bonus.AppliesTo(null);       // Returns false
    /// </code>
    /// </example>
    public bool AppliesTo(string? targetSkillId)
    {
        if (string.IsNullOrWhiteSpace(targetSkillId))
        {
            return false;
        }

        return NormalizedSkillId.Equals(
            targetSkillId.ToLowerInvariant(),
            StringComparison.Ordinal);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING OVERRIDE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted string representation of the skill bonus.
    /// </summary>
    /// <returns>
    /// A string in the format "SkillId +Amount" for positive bonuses,
    /// or "SkillId -Amount" for penalties.
    /// </returns>
    /// <example>
    /// <code>
    /// var bonus = SkillBonus.Create("social", 1);
    /// Console.WriteLine(bonus.ToString()); // Output: "social +1"
    /// 
    /// var penalty = SkillBonus.Create("stealth", -2);
    /// Console.WriteLine(penalty.ToString()); // Output: "stealth -2"
    /// </code>
    /// </example>
    public override string ToString() =>
        BonusAmount >= 0
            ? $"{SkillId} +{BonusAmount}"
            : $"{SkillId} {BonusAmount}";
}
