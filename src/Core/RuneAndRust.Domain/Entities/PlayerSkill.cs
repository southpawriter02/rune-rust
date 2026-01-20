using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a player's proficiency in a specific skill.
/// </summary>
/// <remarks>
/// <para>
/// Tracks the player's progress in a skill including:
/// <list type="bullet">
///   <item><description>Proficiency level (Untrained through Master)</description></item>
///   <item><description>Total experience earned</description></item>
///   <item><description>Usage statistics (times used, success rate)</description></item>
/// </list>
/// </para>
/// <para>
/// Experience is earned through skill usage. When experience crosses
/// thresholds defined in SkillDefinition, proficiency increases.
/// </para>
/// </remarks>
public class PlayerSkill : IEntity
{
    #region Core Properties

    /// <summary>
    /// Gets the unique identifier for this player skill instance.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the skill definition ID this tracks.
    /// </summary>
    public string SkillId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the player ID who owns this skill.
    /// </summary>
    public Guid PlayerId { get; private set; }

    #endregion

    #region Proficiency Properties

    /// <summary>
    /// Gets the current proficiency level.
    /// </summary>
    public SkillProficiency Proficiency { get; private set; } = SkillProficiency.Untrained;

    /// <summary>
    /// Gets the total experience earned in this skill.
    /// </summary>
    public int Experience { get; private set; }

    #endregion

    #region Statistics Properties

    /// <summary>
    /// Gets the number of times this skill has been used.
    /// </summary>
    public int TimesUsed { get; private set; }

    /// <summary>
    /// Gets the number of successful skill checks.
    /// </summary>
    public int SuccessfulChecks { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private PlayerSkill() { }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new player skill.
    /// </summary>
    /// <param name="skillId">The skill definition ID.</param>
    /// <param name="playerId">The player ID.</param>
    /// <param name="startingProficiency">Optional starting proficiency.</param>
    /// <param name="startingExp">Optional starting experience.</param>
    /// <returns>A new PlayerSkill instance.</returns>
    public static PlayerSkill Create(
        string skillId,
        Guid playerId,
        SkillProficiency startingProficiency = SkillProficiency.Untrained,
        int startingExp = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(skillId);

        return new PlayerSkill
        {
            Id = Guid.NewGuid(),
            SkillId = skillId.ToLowerInvariant(),
            PlayerId = playerId,
            Proficiency = startingProficiency,
            Experience = startingExp
        };
    }

    #endregion

    #region Experience Methods

    /// <summary>
    /// Adds experience to this skill, potentially leveling up.
    /// </summary>
    /// <param name="amount">The experience amount to add.</param>
    /// <param name="definition">The skill definition for threshold lookup.</param>
    /// <returns>True if proficiency increased.</returns>
    public bool AddExperience(int amount, SkillDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (amount <= 0)
            return false;

        var oldProficiency = Proficiency;
        Experience += amount;
        Proficiency = definition.GetProficiencyForExperience(Experience);

        return Proficiency > oldProficiency;
    }

    /// <summary>
    /// Records a skill usage attempt.
    /// </summary>
    /// <param name="success">Whether the check was successful.</param>
    public void RecordUsage(bool success)
    {
        TimesUsed++;
        if (success)
            SuccessfulChecks++;
    }

    /// <summary>
    /// Gets the success rate percentage (0-100).
    /// </summary>
    /// <returns>Success rate as integer percentage.</returns>
    public int GetSuccessRate()
    {
        if (TimesUsed == 0)
            return 0;

        return (int)((SuccessfulChecks / (float)TimesUsed) * 100);
    }

    #endregion

    #region Proficiency Methods

    /// <summary>
    /// Sets the proficiency level directly, adjusting experience if needed.
    /// </summary>
    /// <param name="proficiency">The proficiency to set.</param>
    /// <param name="definition">The skill definition for threshold lookup.</param>
    public void SetProficiency(SkillProficiency proficiency, SkillDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        Proficiency = proficiency;

        // Ensure experience matches the minimum for this level
        if (definition.ExperienceThresholds.TryGetValue(proficiency, out var threshold))
        {
            Experience = Math.Max(Experience, threshold);
        }
    }

    #endregion

    /// <summary>
    /// Returns a string representation of this player skill.
    /// </summary>
    public override string ToString() =>
        $"PlayerSkill({SkillId}: {Proficiency}, {Experience}xp)";
}
