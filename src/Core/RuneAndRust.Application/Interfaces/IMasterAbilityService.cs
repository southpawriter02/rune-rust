namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for evaluating and applying master abilities during skill checks.
/// </summary>
/// <remarks>
/// <para>
/// This service is called by the skill check service before performing
/// a skill check to determine if any master abilities apply. The service handles:
/// </para>
/// <list type="bullet">
///   <item><description>Checking if the character has Rank 5 proficiency</description></item>
///   <item><description>Finding applicable abilities for the skill and subtype</description></item>
///   <item><description>Evaluating auto-succeed conditions</description></item>
///   <item><description>Applying dice bonuses to the skill context</description></item>
///   <item><description>Tracking re-roll usage</description></item>
/// </list>
/// </remarks>
public interface IMasterAbilityService
{
    /// <summary>
    /// Evaluates master abilities for a skill check and returns the evaluation result.
    /// </summary>
    /// <param name="player">The player making the check.</param>
    /// <param name="skillId">The skill being checked.</param>
    /// <param name="subType">Optional skill subtype (e.g., "climbing", "stealth").</param>
    /// <param name="difficultyClass">The check's difficulty class.</param>
    /// <returns>
    /// A result indicating whether auto-succeed triggers, dice bonuses to apply,
    /// and any special effects that are active.
    /// </returns>
    MasterAbilityEvaluationResult EvaluateForCheck(
        Player player,
        string skillId,
        string? subType,
        int difficultyClass);

    /// <summary>
    /// Gets all applicable master abilities for a character's skill check.
    /// </summary>
    /// <param name="player">The player making the check.</param>
    /// <param name="skillId">The skill being checked.</param>
    /// <param name="subType">Optional skill subtype.</param>
    /// <returns>
    /// Abilities that apply if the character has Rank 5 proficiency.
    /// Returns empty if character is not master rank.
    /// </returns>
    IReadOnlyList<MasterAbility> GetApplicableAbilities(
        Player player,
        string skillId,
        string? subType);

    /// <summary>
    /// Checks if a character can use a re-roll ability.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="abilityId">The re-roll ability ID.</param>
    /// <returns><c>true</c> if the re-roll is available; otherwise <c>false</c>.</returns>
    bool CanUseReroll(Player player, string abilityId);

    /// <summary>
    /// Marks a re-roll ability as used for the current period.
    /// </summary>
    /// <param name="player">The player using the re-roll.</param>
    /// <param name="abilityId">The re-roll ability ID.</param>
    void UseReroll(Player player, string abilityId);

    /// <summary>
    /// Resets re-roll usage for abilities with the specified period.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="period">The period that has elapsed (e.g., Conversation, Scene).</param>
    void ResetRerollsForPeriod(Player player, RerollPeriod period);
}
