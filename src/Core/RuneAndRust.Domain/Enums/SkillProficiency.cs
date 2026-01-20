namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Proficiency levels for player skills.
/// </summary>
/// <remarks>
/// <para>
/// Proficiency determines skill bonuses and is earned through experience.
/// Each level grants an additional bonus based on the skill's BonusPerLevel.
/// </para>
/// <list type="bullet">
///   <item><description>Untrained: Cannot use skills that require training</description></item>
///   <item><description>Novice: Basic competency, minor bonus</description></item>
///   <item><description>Apprentice: Developing skill, moderate bonus</description></item>
///   <item><description>Journeyman: Competent practitioner, good bonus</description></item>
///   <item><description>Expert: Highly skilled, excellent bonus</description></item>
///   <item><description>Master: Peak proficiency, maximum bonus</description></item>
/// </list>
/// </remarks>
public enum SkillProficiency
{
    /// <summary>
    /// No training - may not be able to attempt some skill checks.
    /// </summary>
    Untrained = 0,

    /// <summary>
    /// Basic training - can attempt simple checks.
    /// </summary>
    Novice = 1,

    /// <summary>
    /// Developing skill - moderate bonus.
    /// </summary>
    Apprentice = 2,

    /// <summary>
    /// Competent practitioner - good bonus.
    /// </summary>
    Journeyman = 3,

    /// <summary>
    /// Highly skilled - excellent bonus.
    /// </summary>
    Expert = 4,

    /// <summary>
    /// Mastery achieved - maximum bonus.
    /// </summary>
    Master = 5
}
