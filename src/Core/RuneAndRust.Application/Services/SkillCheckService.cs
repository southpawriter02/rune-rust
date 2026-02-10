using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for performing skill checks using dice rolls and player attributes.
/// </summary>
/// <remarks>
/// Integrates dice rolling with player attributes and configuration-driven
/// skill definitions and difficulty classes.
/// </remarks>
public class SkillCheckService
{
    private readonly DiceService _diceService;
    private readonly IGameConfigurationProvider _configProvider;
    private readonly ILogger<SkillCheckService> _logger;
    private readonly IGameEventLogger? _eventLogger;

    /// <summary>
    /// Initializes a new instance of the SkillCheckService.
    /// </summary>
    public SkillCheckService(
        DiceService diceService,
        IGameConfigurationProvider configProvider,
        ILogger<SkillCheckService> logger,
        IGameEventLogger? eventLogger = null)
    {
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventLogger = eventLogger;
        _logger.LogInformation("SkillCheckService initialized");
    }

    /// <summary>
    /// Performs a skill check for a player against a named difficulty class.
    /// </summary>
    public SkillCheckResult PerformCheck(
        Player player,
        string skillId,
        string difficultyClassId,
        AdvantageType advantageType = AdvantageType.Normal,
        int additionalBonus = 0,
        string context = "Unspecified")
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentException.ThrowIfNullOrWhiteSpace(skillId, nameof(skillId));
        ArgumentException.ThrowIfNullOrWhiteSpace(difficultyClassId, nameof(difficultyClassId));

        _logger.LogTrace(
            "Performing skill check: Player={Player}, Skill={Skill}, DC={DC}, Advantage={Advantage} Context={Context}",
            player.Name, skillId, difficultyClassId, advantageType, context);

        var skill = _configProvider.GetSkillById(skillId)
            ?? throw new ArgumentException($"Unknown skill: {skillId}", nameof(skillId));

        var dc = _configProvider.GetDifficultyClassById(difficultyClassId)
            ?? throw new ArgumentException($"Unknown difficulty class: {difficultyClassId}", nameof(difficultyClassId));

        return PerformCheckInternal(player, skill, dc, advantageType, additionalBonus, context);
    }

    /// <summary>
    /// Performs a skill check for a player against a specific DC value.
    /// </summary>
    public SkillCheckResult PerformCheckWithDC(
        Player player,
        string skillId,
        int difficultyClass,
        string difficultyName = "Custom",
        AdvantageType advantageType = AdvantageType.Normal,
        int additionalBonus = 0,
        string context = "Unspecified")
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentException.ThrowIfNullOrWhiteSpace(skillId, nameof(skillId));

        if (difficultyClass < 1)
            throw new ArgumentOutOfRangeException(nameof(difficultyClass), "Difficulty class must be at least 1");

        _logger.LogTrace(
            "Performing skill check: Player={Player}, Skill={Skill}, DC={DC}, Advantage={Advantage} Context={Context}",
            player.Name, skillId, difficultyClass, advantageType, context);

        var skill = _configProvider.GetSkillById(skillId)
            ?? throw new ArgumentException($"Unknown skill: {skillId}", nameof(skillId));

        var attributeBonus = CalculateAttributeBonus(player, skill);
        var otherBonus = CalculateOtherBonus(player, skill, additionalBonus, context);

        var dicePool = DicePool.Parse(skill.BaseDicePool);
        var rollResult = _diceService.Roll(dicePool, advantageType);

        var result = new SkillCheckResult(
            skillId,
            skill.Name,
            rollResult,
            attributeBonus,
            otherBonus,
            difficultyClass,
            difficultyName);

        LogCheckResult(result, context);
        return result;
    }

    /// <summary>
    /// Performs a contested skill check between two players.
    /// </summary>
    public (SkillCheckResult ActiveResult, SkillCheckResult PassiveResult, string Winner) PerformContestedCheck(
        Player activePlayer,
        Player passivePlayer,
        string activeSkillId,
        string passiveSkillId,
        AdvantageType activeAdvantage = AdvantageType.Normal,
        AdvantageType passiveAdvantage = AdvantageType.Normal,
        string context = "Unspecified")
    {
        ArgumentNullException.ThrowIfNull(activePlayer);
        ArgumentNullException.ThrowIfNull(passivePlayer);

        _logger.LogTrace(
            "Performing contested check: {Active} ({ActiveSkill}) vs {Passive} ({PassiveSkill}) Context={Context}",
            activePlayer.Name, activeSkillId, passivePlayer.Name, passiveSkillId, context);

        var activeResult = PerformCheckWithDC(activePlayer, activeSkillId, 0, "Contested", activeAdvantage, 0, context);
        var passiveResult = PerformCheckWithDC(passivePlayer, passiveSkillId, 0, "Contested", passiveAdvantage, 0, context);

        var winner = activeResult.TotalResult >= passiveResult.TotalResult
            ? activePlayer.Name
            : passivePlayer.Name;

        _logger.LogInformation(
            "Contested check: {Active} ({ActiveTotal}) vs {Passive} ({PassiveTotal}) -> {Winner} wins Context={Context}",
            activePlayer.Name, activeResult.TotalResult,
            passivePlayer.Name, passiveResult.TotalResult,
            winner, context);

        return (activeResult, passiveResult, winner);
    }

    private SkillCheckResult PerformCheckInternal(
        Player player,
        SkillDefinition skill,
        DifficultyClassDefinition dc,
        AdvantageType advantageType,
        int additionalBonus,
        string context)
    {
        var attributeBonus = CalculateAttributeBonus(player, skill);
        var otherBonus = CalculateOtherBonus(player, skill, additionalBonus, context);

        var dicePool = DicePool.Parse(skill.BaseDicePool);
        var rollResult = _diceService.Roll(dicePool, advantageType);

        var result = new SkillCheckResult(
            skill.Id,
            skill.Name,
            rollResult,
            attributeBonus,
            otherBonus,
            dc.TargetNumber,
            dc.Name);

        LogCheckResult(result, context);
        return result;
    }

    private int CalculateAttributeBonus(Player player, SkillDefinition skill)
    {
        var primaryBonus = GetAttributeValue(player, skill.PrimaryAttribute);

        if (skill.HasSecondaryAttribute)
        {
            var secondaryBonus = GetAttributeValue(player, skill.SecondaryAttribute!) / 2;
            return primaryBonus + secondaryBonus;
        }

        return primaryBonus;
    }

    private int CalculateOtherBonus(Player player, SkillDefinition skill, int additionalBonus, string context)
    {
        var otherBonus = additionalBonus;

        if (skill.RequiresTraining && !PlayerHasSkillTraining(player, skill.Id))
        {
            otherBonus -= skill.UntrainedPenalty;
            _logger.LogDebug(
                "Applied untrained penalty of -{Penalty} for {Skill} Context={Context}",
                skill.UntrainedPenalty, skill.Name, context);
        }

        return otherBonus;
    }

    private static int GetAttributeValue(Player player, string attributeId)
    {
        return attributeId.ToLowerInvariant() switch
        {
            "might" => player.Attributes.Might,
            "fortitude" => player.Attributes.Fortitude,
            "will" => player.Attributes.Will,
            "wits" => player.Attributes.Wits,
            "finesse" => player.Attributes.Finesse,
            _ => 0
        };
    }

    private static bool PlayerHasSkillTraining(Player player, string skillId)
    {
        // TODO: Implement skill training system in future version
        return true;
    }

    private void LogCheckResult(SkillCheckResult result, string context)
    {
        var level = result.IsCritical ? LogLevel.Information : LogLevel.Debug;

        _logger.Log(level,
            "Skill check complete: Skill={Skill} Roll={Roll} Bonus={Bonus} Total={Total} DC={DC} Result={Result} Context={Context}",
            result.SkillName,
            result.DiceResult.Total,
            result.TotalBonus,
            result.TotalResult,
            result.DifficultyClass,
            result.SuccessLevel,
            context);

        _eventLogger?.LogDice("SkillCheck", $"{result.SkillName}: {result.SuccessLevel}",
            data: new Dictionary<string, object>
            {
                ["skillId"] = result.SkillId,
                ["skillName"] = result.SkillName,
                ["roll"] = result.DiceResult.Total,
                ["bonus"] = result.TotalBonus,
                ["total"] = result.TotalResult,
                ["dc"] = result.DifficultyClass,
                ["dcName"] = result.DifficultyName,
                ["success"] = result.IsSuccess,
                ["critical"] = result.IsCritical,
                ["successLevel"] = result.SuccessLevel.ToString(),
                ["context"] = context
            });

        if (result.IsCriticalSuccess)
            _logger.LogInformation("Critical success on {Skill}! Context={Context}", result.SkillName, context);
        else if (result.IsCriticalFailure)
            _logger.LogInformation("Critical failure on {Skill}! Context={Context}", result.SkillName, context);
    }

    /// <summary>
    /// Gets all available skill definitions.
    /// </summary>
    public IReadOnlyList<SkillDefinition> GetAllSkills() => _configProvider.GetSkills();

    /// <summary>
    /// Gets skills filtered by category.
    /// </summary>
    public IReadOnlyList<SkillDefinition> GetSkillsByCategory(string category)
    {
        return _configProvider.GetSkills()
            .Where(s => s.IsInCategory(category))
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .ToList();
    }

    /// <summary>
    /// Gets all difficulty class definitions.
    /// </summary>
    public IReadOnlyList<DifficultyClassDefinition> GetDifficultyClasses()
    {
        return _configProvider.GetDifficultyClasses()
            .OrderBy(dc => dc.SortOrder)
            .ToList();
    }

    /// <summary>
    /// Gets a difficulty class by ID.
    /// </summary>
    public DifficultyClassDefinition? GetDifficultyClass(string id) =>
        _configProvider.GetDifficultyClassById(id);

    /// <summary>
    /// Finds the nearest difficulty class for a given DC value.
    /// </summary>
    public DifficultyClassDefinition? GetNearestDifficultyClass(int targetNumber)
    {
        var allDCs = _configProvider.GetDifficultyClasses();
        return allDCs
            .OrderBy(dc => Math.Abs(dc.TargetNumber - targetNumber))
            .FirstOrDefault();
    }
}
