using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// DTO for skill check results displayed to the player.
/// </summary>
public record SkillCheckResultDto(
    string SkillId,
    string SkillName,
    DiceRollDto DiceRoll,
    int AttributeBonus,
    int OtherBonus,
    int TotalResult,
    int DifficultyClass,
    string DifficultyName,
    string SuccessLevel,
    int Margin,
    bool IsSuccess,
    bool IsCritical,
    string? Descriptor = null)
{
    /// <summary>
    /// Creates a DTO from a domain skill check result.
    /// </summary>
    public static SkillCheckResultDto FromDomainResult(
        SkillCheckResult result,
        string? descriptor = null)
    {
        return new SkillCheckResultDto(
            result.SkillId,
            result.SkillName,
            DiceRollDto.FromDomainResult(result.DiceResult),
            result.AttributeBonus,
            result.OtherBonus,
            result.TotalResult,
            result.DifficultyClass,
            result.DifficultyName,
            result.SuccessLevel.ToString(),
            result.Margin,
            result.IsSuccess,
            result.IsCritical,
            descriptor);
    }

    /// <summary>
    /// Gets a formatted summary of the check result.
    /// </summary>
    public string Summary =>
        $"{SkillName}: {TotalResult} vs DC {DifficultyClass} - {SuccessLevel}";

    /// <summary>
    /// Gets whether this was a critical success.
    /// </summary>
    public bool IsCriticalSuccess =>
        SuccessLevel == nameof(Domain.Enums.SuccessLevel.CriticalSuccess);

    /// <summary>
    /// Gets whether this was a critical failure.
    /// </summary>
    public bool IsCriticalFailure =>
        SuccessLevel == nameof(Domain.Enums.SuccessLevel.CriticalFailure);
}

/// <summary>
/// DTO for skill definitions.
/// </summary>
public record SkillDefinitionDto(
    string Id,
    string Name,
    string Description,
    string PrimaryAttribute,
    string? SecondaryAttribute,
    string BaseDicePool,
    bool AllowUntrained,
    int UntrainedPenalty,
    string Category,
    IReadOnlyList<string> Tags)
{
    /// <summary>
    /// Creates a DTO from a domain skill definition.
    /// </summary>
    public static SkillDefinitionDto FromDomain(SkillDefinition skill)
    {
        return new SkillDefinitionDto(
            skill.Id,
            skill.Name,
            skill.Description,
            skill.PrimaryAttribute,
            skill.SecondaryAttribute,
            skill.BaseDicePool,
            skill.AllowUntrained,
            skill.UntrainedPenalty,
            skill.Category,
            skill.Tags);
    }

    /// <summary>
    /// Gets whether this skill requires training.
    /// </summary>
    public bool RequiresTraining => !AllowUntrained || UntrainedPenalty > 0;

    /// <summary>
    /// Gets whether this skill has a secondary attribute.
    /// </summary>
    public bool HasSecondaryAttribute => !string.IsNullOrEmpty(SecondaryAttribute);
}

/// <summary>
/// DTO for difficulty class definitions.
/// </summary>
public record DifficultyClassDto(
    string Id,
    string Name,
    string Description,
    int TargetNumber,
    string Color)
{
    /// <summary>
    /// Creates a DTO from a domain difficulty class definition.
    /// </summary>
    public static DifficultyClassDto FromDomain(DifficultyClassDefinition dc)
    {
        return new DifficultyClassDto(
            dc.Id,
            dc.Name,
            dc.Description,
            dc.TargetNumber,
            dc.Color);
    }
}

/// <summary>
/// DTO for contested skill check results.
/// </summary>
public record ContestedCheckResultDto(
    string ActivePlayerName,
    string PassivePlayerName,
    SkillCheckResultDto ActiveResult,
    SkillCheckResultDto PassiveResult,
    string Winner)
{
    /// <summary>
    /// Gets whether the active player won.
    /// </summary>
    public bool ActivePlayerWon => Winner == ActivePlayerName;

    /// <summary>
    /// Gets the margin of victory.
    /// </summary>
    public int VictoryMargin => Math.Abs(ActiveResult.TotalResult - PassiveResult.TotalResult);
}
