using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the complete result of a skill check, including dice roll breakdown,
/// attribute modifiers, and success determination.
/// </summary>
/// <remarks>
/// Success level is automatically determined based on the dice roll and total result:
/// <list type="bullet">
///   <item><description>Critical Success: Natural max on first die</description></item>
///   <item><description>Critical Failure: Natural 1 on first die</description></item>
///   <item><description>Success: Total result >= Difficulty Class</description></item>
///   <item><description>Failure: Total result &lt; Difficulty Class</description></item>
/// </list>
/// </remarks>
public readonly record struct SkillCheckResult
{
    /// <summary>
    /// Gets the ID of the skill that was checked.
    /// </summary>
    public string SkillId { get; init; }

    /// <summary>
    /// Gets the display name of the skill.
    /// </summary>
    public string SkillName { get; init; }

    /// <summary>
    /// Gets the underlying dice roll result.
    /// </summary>
    public DiceRollResult DiceResult { get; init; }

    /// <summary>
    /// Gets the bonus from the player's primary (and optionally secondary) attribute.
    /// </summary>
    public int AttributeBonus { get; init; }

    /// <summary>
    /// Gets the bonus from other sources (equipment, abilities, situational).
    /// </summary>
    public int OtherBonus { get; init; }

    /// <summary>
    /// Gets the total result of the check (dice + all bonuses).
    /// </summary>
    public int TotalResult { get; init; }

    /// <summary>
    /// Gets the difficulty class that was checked against.
    /// </summary>
    public int DifficultyClass { get; init; }

    /// <summary>
    /// Gets the name of the difficulty level.
    /// </summary>
    public string DifficultyName { get; init; }

    /// <summary>
    /// Gets the degree of success or failure.
    /// </summary>
    public SuccessLevel SuccessLevel { get; init; }

    /// <summary>
    /// Gets the margin by which the check succeeded or failed.
    /// </summary>
    public int Margin => TotalResult - DifficultyClass;

    /// <summary>
    /// Gets whether the check succeeded (Success or CriticalSuccess).
    /// </summary>
    public bool IsSuccess => SuccessLevel is SuccessLevel.Success or SuccessLevel.CriticalSuccess;

    /// <summary>
    /// Gets whether the check was a critical result.
    /// </summary>
    public bool IsCritical => SuccessLevel is SuccessLevel.CriticalSuccess or SuccessLevel.CriticalFailure;

    /// <summary>
    /// Gets whether this was a critical success.
    /// </summary>
    public bool IsCriticalSuccess => SuccessLevel == SuccessLevel.CriticalSuccess;

    /// <summary>
    /// Gets whether this was a critical failure.
    /// </summary>
    public bool IsCriticalFailure => SuccessLevel == SuccessLevel.CriticalFailure;

    /// <summary>
    /// Gets the raw dice total before bonuses.
    /// </summary>
    public int RawDiceTotal => DiceResult.Total;

    /// <summary>
    /// Gets the total bonus applied (attribute + other).
    /// </summary>
    public int TotalBonus => AttributeBonus + OtherBonus;

    /// <summary>
    /// Creates a skill check result with automatic success level determination.
    /// </summary>
    public SkillCheckResult(
        string skillId,
        string skillName,
        DiceRollResult diceResult,
        int attributeBonus,
        int otherBonus,
        int difficultyClass,
        string difficultyName)
    {
        SkillId = skillId;
        SkillName = skillName;
        DiceResult = diceResult;
        AttributeBonus = attributeBonus;
        OtherBonus = otherBonus;
        TotalResult = diceResult.Total + attributeBonus + otherBonus;
        DifficultyClass = difficultyClass;
        DifficultyName = difficultyName;

        SuccessLevel = DetermineSuccessLevel(diceResult, TotalResult, difficultyClass);
    }

    private static SuccessLevel DetermineSuccessLevel(
        DiceRollResult diceResult,
        int totalResult,
        int difficultyClass)
    {
        if (diceResult.IsNaturalOne)
            return SuccessLevel.CriticalFailure;

        if (diceResult.IsNaturalMax)
            return SuccessLevel.CriticalSuccess;

        return totalResult >= difficultyClass
            ? SuccessLevel.Success
            : SuccessLevel.Failure;
    }

    /// <summary>
    /// Returns a formatted string showing the check breakdown.
    /// </summary>
    public override string ToString()
    {
        var bonusStr = new List<string>();
        if (AttributeBonus != 0)
            bonusStr.Add($"{(AttributeBonus > 0 ? "+" : "")}{AttributeBonus} (attr)");
        if (OtherBonus != 0)
            bonusStr.Add($"{(OtherBonus > 0 ? "+" : "")}{OtherBonus} (bonus)");

        var bonusPart = bonusStr.Count > 0 ? " " + string.Join(" ", bonusStr) : "";

        var criticalNote = SuccessLevel switch
        {
            SuccessLevel.CriticalSuccess => " [CRITICAL SUCCESS!]",
            SuccessLevel.CriticalFailure => " [CRITICAL FAILURE!]",
            _ => ""
        };

        return $"{SkillName}: [{DiceResult.DiceTotal}]{bonusPart} = {TotalResult} vs DC {DifficultyClass} ({DifficultyName}) -> {SuccessLevel}{criticalNote}";
    }
}
