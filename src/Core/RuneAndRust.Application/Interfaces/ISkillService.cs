using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for managing player skills and skill checks.
/// </summary>
public interface ISkillService
{
    /// <summary>
    /// Performs a skill check against a difficulty class.
    /// </summary>
    /// <param name="player">The player making the check.</param>
    /// <param name="skillId">The skill to check.</param>
    /// <param name="dc">The difficulty class to beat.</param>
    /// <returns>The result of the skill check.</returns>
    SkillCheckOutcome PerformSkillCheck(Player player, string skillId, int dc);

    /// <summary>
    /// Gets the total skill bonus for a player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="skillId">The skill ID.</param>
    /// <returns>The skill bonus.</returns>
    int GetSkillBonus(Player player, string skillId);

    /// <summary>
    /// Awards skill experience to a player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="skillId">The skill that gains experience.</param>
    /// <param name="amount">The experience amount.</param>
    /// <returns>The result of the experience award.</returns>
    SkillExperienceResult AwardSkillExperience(Player player, string skillId, int amount);

    /// <summary>
    /// Gets information about all skills for a player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>Enumerable of skill info records.</returns>
    IEnumerable<PlayerSkillInfo> GetPlayerSkills(Player player);

    /// <summary>
    /// Gets all skill definitions.
    /// </summary>
    /// <returns>Enumerable of skill definitions.</returns>
    IEnumerable<SkillDefinition> GetAllSkillDefinitions();

    /// <summary>
    /// Initializes all skills for a player with Untrained proficiency.
    /// </summary>
    /// <param name="player">The player to initialize.</param>
    void InitializePlayerSkills(Player player);
}

/// <summary>
/// Result of a skill check.
/// </summary>
/// <param name="SkillId">The skill checked.</param>
/// <param name="Success">Whether the check succeeded.</param>
/// <param name="Roll">The dice roll result.</param>
/// <param name="SkillBonus">The bonus from skill proficiency.</param>
/// <param name="Total">Roll + SkillBonus.</param>
/// <param name="DC">The difficulty class.</param>
/// <param name="Margin">Total - DC (positive = success margin).</param>
/// <param name="Message">A message describing the result.</param>
public readonly record struct SkillCheckOutcome(
    string SkillId,
    bool Success,
    int Roll,
    int SkillBonus,
    int Total,
    int DC,
    int Margin,
    string Message);

/// <summary>
/// Result of awarding skill experience.
/// </summary>
/// <param name="SkillId">The skill that gained experience.</param>
/// <param name="ExperienceGained">Amount of experience gained.</param>
/// <param name="TotalExperience">New total experience.</param>
/// <param name="LeveledUp">Whether proficiency increased.</param>
/// <param name="NewProficiency">Current proficiency level.</param>
/// <param name="Message">A message describing the result.</param>
public readonly record struct SkillExperienceResult(
    string SkillId,
    int ExperienceGained,
    int TotalExperience,
    bool LeveledUp,
    SkillProficiency NewProficiency,
    string Message);

/// <summary>
/// Information about a player's skill.
/// </summary>
/// <param name="SkillId">The skill ID.</param>
/// <param name="Name">The display name.</param>
/// <param name="Proficiency">Current proficiency level.</param>
/// <param name="Experience">Total experience.</param>
/// <param name="ExperienceToNext">Experience needed for next level.</param>
/// <param name="Bonus">Current skill bonus.</param>
/// <param name="TimesUsed">Number of times used.</param>
/// <param name="SuccessRate">Success rate percentage.</param>
public readonly record struct PlayerSkillInfo(
    string SkillId,
    string Name,
    SkillProficiency Proficiency,
    int Experience,
    int ExperienceToNext,
    int Bonus,
    int TimesUsed,
    int SuccessRate);
