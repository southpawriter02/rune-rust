namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides specialization-based skill bonuses for archetype-specific modifiers.
/// </summary>
/// <remarks>
/// <para>
/// Each specialization in Rune &amp; Rust grants unique bonuses to specific skills.
/// This provider looks up and evaluates these bonuses based on the character's
/// specialization and the current skill check context.
/// </para>
/// <para>
/// Specialization bonuses may be:
/// <list type="bullet">
///   <item>Unconditional: Always apply when using the skill</item>
///   <item>Conditional: Require specific context (biome, equipment, target)</item>
/// </list>
/// </para>
/// <para>
/// Some specializations also grant special abilities that modify skill behavior
/// beyond simple dice bonuses, such as the Thul's "no reputation loss on failure"
/// for persuasion checks.
/// </para>
/// </remarks>
public interface ISpecializationBonusProvider
{
    /// <summary>
    /// Gets the skill bonus for a character's specialization and skill combination.
    /// </summary>
    /// <param name="specializationId">The ID of the character's specialization.</param>
    /// <param name="skillId">The ID of the skill being used.</param>
    /// <param name="context">The skill context for conditional evaluation.</param>
    /// <returns>
    /// A <see cref="SpecializationSkillBonus"/> with the calculated bonus,
    /// or a zero bonus if no bonus applies.
    /// </returns>
    SpecializationSkillBonus GetSkillBonus(
        string specializationId,
        string skillId,
        SkillContext context);

    /// <summary>
    /// Gets all skill bonuses for a specialization.
    /// </summary>
    /// <param name="specializationId">The ID of the specialization to look up.</param>
    /// <returns>A collection of all skill bonuses for the specialization.</returns>
    IReadOnlyList<SpecializationSkillBonus> GetAllBonuses(string specializationId);

    /// <summary>
    /// Checks if a specialization has any bonus for a specific skill.
    /// </summary>
    /// <param name="specializationId">The ID of the specialization.</param>
    /// <param name="skillId">The ID of the skill.</param>
    /// <returns>True if the specialization grants a bonus to this skill.</returns>
    bool HasBonusForSkill(string specializationId, string skillId);

    /// <summary>
    /// Evaluates whether a conditional bonus's requirements are met.
    /// </summary>
    /// <param name="bonus">The bonus to evaluate.</param>
    /// <param name="context">The skill context for evaluation.</param>
    /// <returns>True if the bonus conditions are met.</returns>
    bool EvaluateCondition(SpecializationSkillBonus bonus, SkillContext context);

    /// <summary>
    /// Gets the list of all specialization IDs that have skill bonuses configured.
    /// </summary>
    /// <returns>A collection of specialization IDs.</returns>
    IReadOnlyList<string> GetSpecializationsWithBonuses();
}
