using RuneAndRust.Core;
using RuneAndRust.Core.SkillUsageFlavor;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.38.10: Skill Usage Flavor Text Service
/// Generates rich flavor text for skill checks and fumbles
/// Philosophy: Every skill check tells a story of expertise, struggle, and consequence
/// </summary>
public class SkillUsageFlavorTextService
{
    private readonly DescriptorRepository _repository;
    private static readonly ILogger _log = Log.ForContext<SkillUsageFlavorTextService>();

    public SkillUsageFlavorTextService(DescriptorRepository repository)
    {
        _repository = repository;
    }

    #region Skill Check Flavor Text Generation

    /// <summary>
    /// Generates flavor text for a skill check attempt (setup/context)
    /// </summary>
    public string GenerateAttemptDescription(
        string skillType,
        string actionType,
        string? environmentalContext = null,
        string? biomeContext = null)
    {
        var descriptor = _repository.GetRandomSkillCheckDescriptor(
            skillType,
            actionType,
            SkillCheckDescriptor.CheckPhases.Attempt,
            resultDegree: null,
            environmentalContext,
            biomeContext);

        if (descriptor == null)
        {
            _log.Warning("No attempt descriptor found for {Skill} {Action}", skillType, actionType);
            return GetFallbackAttemptDescription(skillType, actionType);
        }

        return descriptor.DescriptorText;
    }

    /// <summary>
    /// Generates flavor text for a successful skill check
    /// </summary>
    public string GenerateSuccessDescription(
        string skillType,
        string actionType,
        int roll,
        int dc,
        string? environmentalContext = null,
        string? biomeContext = null)
    {
        var margin = roll - dc;
        var resultDegree = DetermineResultDegree(margin);
        var checkPhase = margin >= 6
            ? SkillCheckDescriptor.CheckPhases.CriticalSuccess
            : SkillCheckDescriptor.CheckPhases.Success;

        var descriptor = _repository.GetRandomSkillCheckDescriptor(
            skillType,
            actionType,
            checkPhase,
            resultDegree,
            environmentalContext,
            biomeContext);

        if (descriptor == null)
        {
            _log.Warning("No success descriptor found for {Skill} {Action} ({Phase} {Degree})",
                skillType, actionType, checkPhase, resultDegree);
            return GetFallbackSuccessDescription(skillType, actionType, margin);
        }

        return ProcessVariables(descriptor.DescriptorText, roll, dc, margin);
    }

    /// <summary>
    /// Generates flavor text for a failed skill check
    /// </summary>
    public string GenerateFailureDescription(
        string skillType,
        string actionType,
        int roll,
        int dc,
        string? environmentalContext = null,
        string? biomeContext = null)
    {
        var margin = dc - roll; // Positive value for how much under DC
        var resultDegree = DetermineResultDegree(margin);

        var descriptor = _repository.GetRandomSkillCheckDescriptor(
            skillType,
            actionType,
            SkillCheckDescriptor.CheckPhases.Failure,
            resultDegree,
            environmentalContext,
            biomeContext);

        if (descriptor == null)
        {
            _log.Warning("No failure descriptor found for {Skill} {Action} ({Degree})",
                skillType, actionType, resultDegree);
            return GetFallbackFailureDescription(skillType, actionType, margin);
        }

        return ProcessVariables(descriptor.DescriptorText, roll, dc, -margin);
    }

    /// <summary>
    /// Generates flavor text for a fumble (catastrophic failure)
    /// </summary>
    public SkillFumbleResult GenerateFumbleDescription(
        string skillType,
        string actionType,
        string? preferredConsequenceType = null)
    {
        var fumble = _repository.GetRandomSkillFumbleDescriptor(
            skillType,
            actionType,
            preferredConsequenceType);

        if (fumble == null)
        {
            _log.Warning("No fumble descriptor found for {Skill} {Action}", skillType, actionType);
            return new SkillFumbleResult
            {
                Description = GetFallbackFumbleDescription(skillType, actionType),
                ConsequenceType = "InjuryTaken",
                Severity = "Moderate"
            };
        }

        return new SkillFumbleResult
        {
            Description = fumble.DescriptorText,
            ConsequenceType = fumble.ConsequenceType,
            Severity = fumble.Severity,
            DamageFormula = fumble.DamageFormula,
            StatusEffectApplied = fumble.StatusEffectApplied,
            NextAttemptDCModifier = fumble.NextAttemptDCModifier,
            TimePenaltyMinutes = fumble.TimePenaltyMinutes,
            PreventsRetry = fumble.PreventsRetry
        };
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Determines result degree based on margin of success/failure
    /// </summary>
    private string DetermineResultDegree(int margin)
    {
        var absMargin = Math.Abs(margin);

        if (absMargin <= 2)
            return SkillCheckDescriptor.ResultDegrees.Minimal;
        else if (absMargin <= 5)
            return SkillCheckDescriptor.ResultDegrees.Solid;
        else
            return SkillCheckDescriptor.ResultDegrees.Critical;
    }

    /// <summary>
    /// Processes variable placeholders in descriptor text
    /// </summary>
    private string ProcessVariables(string text, int roll, int dc, int margin)
    {
        return text
            .Replace("{Roll}", roll.ToString())
            .Replace("{DC}", dc.ToString())
            .Replace("{Margin}", Math.Abs(margin).ToString());
    }

    #endregion

    #region Fallback Descriptions

    private string GetFallbackAttemptDescription(string skillType, string actionType)
    {
        return skillType switch
        {
            "SystemBypass" => actionType switch
            {
                "Lockpicking" => "You examine the lock, preparing your picks.",
                "TerminalHacking" => "You begin to interface with the terminal.",
                "TrapDisarm" => "You carefully examine the mechanism.",
                _ => "You prepare to use your technical skills."
            },
            "Acrobatics" => actionType switch
            {
                "Climbing" => "You prepare to climb.",
                "Leaping" => "You prepare to jump.",
                "Stealth" => "You move quietly.",
                _ => "You prepare your acrobatic maneuver."
            },
            "WastelandSurvival" => actionType switch
            {
                "Tracking" => "You examine the tracks.",
                "Foraging" => "You search for resources.",
                "Navigation" => "You plot a course.",
                _ => "You use your survival skills."
            },
            "Rhetoric" => actionType switch
            {
                "Persuasion" => "You make your case.",
                "Deception" => "You prepare your deception.",
                "Intimidation" => "You adopt a threatening posture.",
                _ => "You prepare to speak."
            },
            _ => "You attempt the action."
        };
    }

    private string GetFallbackSuccessDescription(string skillType, string actionType, int margin)
    {
        if (margin >= 6)
            return $"{actionType} successful! Masterful execution.";
        else if (margin >= 3)
            return $"{actionType} successful!";
        else
            return $"{actionType} successful, but it was close.";
    }

    private string GetFallbackFailureDescription(string skillType, string actionType, int margin)
    {
        if (margin >= 3)
            return $"{actionType} failed. You couldn't manage it.";
        else
            return $"{actionType} failed by a narrow margin. Almost had it.";
    }

    private string GetFallbackFumbleDescription(string skillType, string actionType)
    {
        return $"{actionType} fumbled catastrophically! Something has gone terribly wrong.";
    }

    #endregion

    #region Public API for Full Skill Check

    /// <summary>
    /// Generates complete skill check narrative (attempt + result)
    /// </summary>
    public SkillCheckNarrative GenerateSkillCheckNarrative(
        string skillType,
        string actionType,
        int roll,
        int dc,
        bool isFumble = false,
        string? environmentalContext = null,
        string? biomeContext = null)
    {
        var narrative = new SkillCheckNarrative
        {
            SkillType = skillType,
            ActionType = actionType,
            Roll = roll,
            DC = dc
        };

        // Generate attempt description
        narrative.AttemptDescription = GenerateAttemptDescription(
            skillType, actionType, environmentalContext, biomeContext);

        // Generate result description
        if (isFumble)
        {
            var fumble = GenerateFumbleDescription(skillType, actionType);
            narrative.ResultDescription = fumble.Description;
            narrative.IsFumble = true;
            narrative.FumbleResult = fumble;
        }
        else if (roll >= dc)
        {
            narrative.ResultDescription = GenerateSuccessDescription(
                skillType, actionType, roll, dc, environmentalContext, biomeContext);
            narrative.IsSuccess = true;
        }
        else
        {
            narrative.ResultDescription = GenerateFailureDescription(
                skillType, actionType, roll, dc, environmentalContext, biomeContext);
            narrative.IsSuccess = false;
        }

        return narrative;
    }

    #endregion
}

#region Result Classes

/// <summary>
/// Complete skill check narrative with attempt and result
/// </summary>
public class SkillCheckNarrative
{
    public string SkillType { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public int Roll { get; set; }
    public int DC { get; set; }
    public string AttemptDescription { get; set; } = string.Empty;
    public string ResultDescription { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public bool IsFumble { get; set; }
    public SkillFumbleResult? FumbleResult { get; set; }

    /// <summary>
    /// Gets the full narrative text (attempt + result)
    /// </summary>
    public string GetFullNarrative()
    {
        return $"{AttemptDescription}\n\n{ResultDescription}";
    }
}

/// <summary>
/// Fumble result with mechanical consequences
/// </summary>
public class SkillFumbleResult
{
    public string Description { get; set; } = string.Empty;
    public string ConsequenceType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? DamageFormula { get; set; }
    public string? StatusEffectApplied { get; set; }
    public int? NextAttemptDCModifier { get; set; }
    public int? TimePenaltyMinutes { get; set; }
    public bool PreventsRetry { get; set; }
}

#endregion
