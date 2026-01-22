using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// DTO for skill check results displayed to the player.
/// </summary>
/// <remarks>
/// v0.15.0c: Added NetSuccesses, IsFumble, and Outcome for success-counting mechanics.
/// TotalResult and SuccessLevel preserved for backward compatibility.
/// </remarks>
public record SkillCheckResultDto(
    string SkillId,
    string SkillName,
    DiceRollDto DiceRoll,
    int AttributeBonus,
    int OtherBonus,
    int TotalResult,
    int NetSuccesses,
    int DifficultyClass,
    string DifficultyName,
    string SuccessLevel,
    string Outcome,
    int Margin,
    bool IsSuccess,
    bool IsCritical,
    bool IsFumble,
    string? Descriptor = null)
{
    /// <summary>
    /// Creates a DTO from a domain skill check result.
    /// </summary>
    /// <remarks>
    /// v0.15.0c: Now includes NetSuccesses and Outcome for success-counting mechanics.
    /// Uses pragma to suppress obsolete warnings for backward compatibility properties.
    /// </remarks>
#pragma warning disable CS0618 // Type or member is obsolete
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
            result.TotalResult,          // Legacy: preserved for backward compatibility
            result.NetSuccesses,         // v0.15.0c: New success-counting property
            result.DifficultyClass,
            result.DifficultyName,
            result.SuccessLevel.ToString(), // Legacy: preserved for backward compatibility
            result.Outcome.ToString(),   // v0.15.0c: New 6-tier classification
            result.Margin,
            result.IsSuccess,
            result.IsCritical,
            result.IsFumble,
            descriptor);
    }
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Gets a formatted summary of the check result.
    /// </summary>
    /// <remarks>
    /// v0.15.0c: Updated to show NetSuccesses and Outcome (success-counting format).
    /// </remarks>
    public string Summary =>
        $"{SkillName}: {NetSuccesses} net vs DC {DifficultyClass} - {Outcome}";

    /// <summary>
    /// Gets whether this was a critical success.
    /// </summary>
    public bool IsCriticalSuccess =>
        Outcome == nameof(Domain.Enums.SkillOutcome.CriticalSuccess);

    /// <summary>
    /// Gets whether this was a critical failure.
    /// </summary>
    public bool IsCriticalFailure =>
        Outcome == nameof(Domain.Enums.SkillOutcome.CriticalFailure);
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
