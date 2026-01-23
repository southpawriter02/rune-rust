namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a skill bonus granted by a character's specialization (archetype).
/// </summary>
/// <remarks>
/// <para>
/// Specializations provide unique bonuses to specific skills. These bonuses
/// may be unconditional (always apply) or conditional (require specific context
/// such as biome, target type, or equipment).
/// </para>
/// <para>
/// Example specialization bonuses:
/// <list type="bullet">
///   <item>Gantry-Runner: +2d10 to climbing, +1d10 to leaping</item>
///   <item>Myr-Stalker: +2d10 to navigation in swamp biomes</item>
///   <item>Thul: +1d10 to persuasion, no reputation loss on failure</item>
/// </list>
/// </para>
/// </remarks>
/// <param name="SpecializationId">The ID of the specialization granting this bonus.</param>
/// <param name="SkillId">The ID of the skill receiving the bonus.</param>
/// <param name="DiceBonus">The dice pool bonus (positive or negative).</param>
/// <param name="DcModifier">Optional DC modifier (positive = harder, negative = easier).</param>
/// <param name="Description">Human-readable description of the bonus.</param>
/// <param name="IsConditional">Whether this bonus requires specific conditions to apply.</param>
/// <param name="ConditionMet">Whether the conditions for this bonus were met.</param>
/// <param name="SpecialAbility">Optional special ability flag (e.g., "no-reputation-loss-on-failure").</param>
public readonly record struct SpecializationSkillBonus(
    string SpecializationId,
    string SkillId,
    int DiceBonus,
    int DcModifier = 0,
    string Description = "",
    bool IsConditional = false,
    bool ConditionMet = true,
    string? SpecialAbility = null)
{
    /// <summary>
    /// Gets a value indicating whether this bonus has any effect.
    /// </summary>
    public bool HasEffect => DiceBonus != 0 || DcModifier != 0 || SpecialAbility != null;

    /// <summary>
    /// Gets a value indicating whether this bonus should be applied.
    /// </summary>
    /// <remarks>
    /// A bonus applies if it has an effect and either is unconditional or has its condition met.
    /// </remarks>
    public bool ShouldApply => HasEffect && (!IsConditional || ConditionMet);

    /// <summary>
    /// Gets a value indicating whether this bonus has a special ability component.
    /// </summary>
    public bool HasSpecialAbility => !string.IsNullOrEmpty(SpecialAbility);

    /// <summary>
    /// Creates a SpecializationSkillBonus with no effect.
    /// </summary>
    /// <param name="specializationId">The specialization ID.</param>
    /// <param name="skillId">The skill ID.</param>
    /// <returns>A bonus with no effect.</returns>
    public static SpecializationSkillBonus None(string specializationId, string skillId)
    {
        return new SpecializationSkillBonus(
            SpecializationId: specializationId,
            SkillId: skillId,
            DiceBonus: 0,
            Description: "No specialization bonus");
    }

    /// <summary>
    /// Creates a copy of this bonus with the condition evaluation result.
    /// </summary>
    /// <param name="conditionMet">Whether the condition was met.</param>
    /// <returns>A new bonus with the condition result set.</returns>
    public SpecializationSkillBonus WithConditionResult(bool conditionMet)
    {
        return this with { ConditionMet = conditionMet };
    }

    /// <summary>
    /// Returns a human-readable description of the bonus.
    /// </summary>
    /// <returns>A formatted string describing the bonus.</returns>
    public string ToDisplayString()
    {
        if (!ShouldApply)
            return IsConditional && !ConditionMet
                ? $"{Description} (condition not met)"
                : "No specialization bonus";

        var parts = new List<string>();

        if (DiceBonus > 0)
            parts.Add($"+{DiceBonus}d10");
        else if (DiceBonus < 0)
            parts.Add($"{DiceBonus}d10");

        if (DcModifier > 0)
            parts.Add($"DC +{DcModifier}");
        else if (DcModifier < 0)
            parts.Add($"DC {DcModifier}");

        var effect = parts.Count > 0 ? string.Join(", ", parts) : "special ability";
        return $"{Description}: {effect}";
    }
}
