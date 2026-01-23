using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Constants;
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
        int additionalBonus = 0)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentException.ThrowIfNullOrWhiteSpace(skillId, nameof(skillId));
        ArgumentException.ThrowIfNullOrWhiteSpace(difficultyClassId, nameof(difficultyClassId));

        _logger.LogDebug(
            "Performing skill check: Player={Player}, Skill={Skill}, DC={DC}, Advantage={Advantage}",
            player.Name, skillId, difficultyClassId, advantageType);

        var skill = _configProvider.GetSkillById(skillId)
            ?? throw new ArgumentException($"Unknown skill: {skillId}", nameof(skillId));

        var dc = _configProvider.GetDifficultyClassById(difficultyClassId)
            ?? throw new ArgumentException($"Unknown difficulty class: {difficultyClassId}", nameof(difficultyClassId));

        return PerformCheckInternal(player, skill, dc, advantageType, additionalBonus);
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
        int additionalBonus = 0)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentException.ThrowIfNullOrWhiteSpace(skillId, nameof(skillId));

        if (difficultyClass < 1)
            throw new ArgumentOutOfRangeException(nameof(difficultyClass), "Difficulty class must be at least 1");

        _logger.LogDebug(
            "Performing skill check: Player={Player}, Skill={Skill}, DC={DC}, Advantage={Advantage}",
            player.Name, skillId, difficultyClass, advantageType);

        var skill = _configProvider.GetSkillById(skillId)
            ?? throw new ArgumentException($"Unknown skill: {skillId}", nameof(skillId));

        var attributeBonus = CalculateAttributeBonus(player, skill);
        var otherBonus = CalculateOtherBonus(player, skill, additionalBonus);

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

        LogCheckResult(result);
        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONTEXT-AWARE SKILL CHECK (v0.15.1a)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Performs a skill check with full context modifiers.
    /// </summary>
    /// <param name="player">The player making the check.</param>
    /// <param name="skillId">The skill identifier.</param>
    /// <param name="difficultyClass">Base difficulty class.</param>
    /// <param name="difficultyName">Difficulty name for display.</param>
    /// <param name="context">Skill context with all modifiers.</param>
    /// <param name="advantageType">Advantage or disadvantage.</param>
    /// <returns>The complete skill check result.</returns>
    /// <remarks>
    /// <para>
    /// v0.15.1a: New method that accepts a <see cref="SkillContext"/> containing
    /// equipment, situational, environment, and target modifiers.
    /// </para>
    /// <para>
    /// Context modifiers affect:
    /// <list type="bullet">
    ///   <item><description>Dice pool: TotalDiceModifier added to base pool (minimum 1 die)</description></item>
    ///   <item><description>Difficulty class: TotalDcModifier added to base DC (minimum 0)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public SkillCheckResult PerformCheckWithContext(
        Player player,
        string skillId,
        int difficultyClass,
        string difficultyName,
        SkillContext context,
        AdvantageType advantageType = AdvantageType.Normal)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentException.ThrowIfNullOrWhiteSpace(skillId, nameof(skillId));
        ArgumentNullException.ThrowIfNull(context);

        if (difficultyClass < 0)
            throw new ArgumentOutOfRangeException(nameof(difficultyClass), "Difficulty class must be non-negative");

        _logger.LogDebug(
            "Performing skill check with context: Player={Player}, Skill={Skill}, BaseDC={DC}, Context={Context}",
            player.Name, skillId, difficultyClass, context);

        var skill = _configProvider.GetSkillById(skillId)
            ?? throw new ArgumentException($"Unknown skill: {skillId}", nameof(skillId));

        var attributeBonus = CalculateAttributeBonus(player, skill);
        var otherBonus = CalculateOtherBonus(player, skill, 0);

        // Apply context modifiers
        var contextDiceBonus = context.TotalDiceModifier;
        var contextDcMod = context.TotalDcModifier;

        // Build dice pool: base pool + context bonus (minimum 1 die)
        var baseDicePool = DicePool.Parse(skill.BaseDicePool);
        var totalDice = baseDicePool.Count + contextDiceBonus;
        var finalPoolSize = Math.Max(1, totalDice);
        var dicePool = new DicePool(finalPoolSize, baseDicePool.DiceType);

        // Apply DC modification (minimum 0)
        var finalDc = Math.Max(0, difficultyClass + contextDcMod);
        var finalDcName = contextDcMod != 0
            ? $"{difficultyName} (modified)"
            : difficultyName;

        _logger.LogDebug(
            "Final check parameters: Pool={Pool} (base {Base} + context {ContextDice}), DC={DC} (base {BaseDC} + context {ContextDC})",
            finalPoolSize, baseDicePool.Count, contextDiceBonus,
            finalDc, difficultyClass, contextDcMod);

        var rollResult = _diceService.Roll(dicePool, advantageType);

        var result = new SkillCheckResult(
            skill.Id,
            skill.Name,
            rollResult,
            attributeBonus,
            otherBonus + contextDiceBonus,  // Include context bonus in "other"
            finalDc,
            finalDcName);

        LogCheckResult(result, context);
        return result;
    }

    /// <summary>
    /// Logs the check result with context information.
    /// </summary>
    private void LogCheckResult(SkillCheckResult result, SkillContext? context)
    {
        var level = result.IsCritical ? LogLevel.Information : LogLevel.Debug;

        _logger.Log(level,
            "Skill check complete: Skill={Skill} Net={Net} ({Successes}S-{Botches}B) DC={DC} Outcome={Outcome} Margin={Margin}",
            result.SkillName,
            result.NetSuccesses,
            result.DiceResult.TotalSuccesses,
            result.DiceResult.TotalBotches,
            result.DifficultyClass,
            result.Outcome,
            result.Margin);

        if (context?.HasModifiers == true)
        {
            _logger.LogDebug("Applied modifiers:\n{Modifiers}", context.ToDescription());
        }

        if (result.IsFumble)
            _logger.LogInformation("FUMBLE on {Skill}!", result.SkillName);
        else if (result.IsCriticalSuccess)
            _logger.LogInformation("CRITICAL SUCCESS on {Skill}!", result.SkillName);
    }

    /// <summary>
    /// Performs a contested skill check between two players.
    /// </summary>
    /// <remarks>
    /// v0.15.0c: Contested checks now compare NetSuccesses (success-counting mechanics)
    /// instead of TotalResult. The player with more net successes wins. Ties favor the active player.
    /// </remarks>
    public (SkillCheckResult ActiveResult, SkillCheckResult PassiveResult, string Winner) PerformContestedCheck(
        Player activePlayer,
        Player passivePlayer,
        string activeSkillId,
        string passiveSkillId,
        AdvantageType activeAdvantage = AdvantageType.Normal,
        AdvantageType passiveAdvantage = AdvantageType.Normal)
    {
        ArgumentNullException.ThrowIfNull(activePlayer);
        ArgumentNullException.ThrowIfNull(passivePlayer);

        _logger.LogDebug(
            "Performing contested check: {Active} ({ActiveSkill}) vs {Passive} ({PassiveSkill})",
            activePlayer.Name, activeSkillId, passivePlayer.Name, passiveSkillId);

        var activeResult = PerformCheckWithDC(activePlayer, activeSkillId, 0, "Contested", activeAdvantage);
        var passiveResult = PerformCheckWithDC(passivePlayer, passiveSkillId, 0, "Contested", passiveAdvantage);

        // v0.15.0c: Compare NetSuccesses for contested checks (success-counting mechanics)
        var winner = activeResult.NetSuccesses >= passiveResult.NetSuccesses
            ? activePlayer.Name
            : passivePlayer.Name;

        _logger.LogInformation(
            "Contested check: {Active} ({ActiveNet} net) vs {Passive} ({PassiveNet} net) -> {Winner} wins",
            activePlayer.Name, activeResult.NetSuccesses,
            passivePlayer.Name, passiveResult.NetSuccesses,
            winner);

        return (activeResult, passiveResult, winner);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONTESTED CHECK METHODS (v0.15.0d)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Resolves a contested skill check between two characters using success-counting mechanics.
    /// </summary>
    /// <param name="initiatorId">ID of the initiating character.</param>
    /// <param name="defenderId">ID of the defending character.</param>
    /// <param name="initiatorSkillId">Skill ID for the initiator.</param>
    /// <param name="defenderSkillId">Skill ID for the defender.</param>
    /// <param name="initiatorAdvantage">Advantage type for initiator roll.</param>
    /// <param name="defenderAdvantage">Advantage type for defender roll.</param>
    /// <returns>Complete contested check result with outcome and margin.</returns>
    /// <remarks>
    /// <para>
    /// v0.15.0d: New method returning a structured ContestedCheckResult value object.
    /// Resolution priority:
    /// <list type="bullet">
    ///   <item><description>Both fumble → BothFumble outcome</description></item>
    ///   <item><description>Initiator fumbles → InitiatorFumble (defender auto-wins)</description></item>
    ///   <item><description>Defender fumbles → DefenderFumble (initiator auto-wins)</description></item>
    ///   <item><description>Compare net successes → higher wins, equal = Tie</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public ContestedCheckResult ResolveContestedCheck(
        string initiatorId,
        string defenderId,
        string initiatorSkillId,
        string defenderSkillId,
        AdvantageType initiatorAdvantage = AdvantageType.Normal,
        AdvantageType defenderAdvantage = AdvantageType.Normal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(initiatorId, nameof(initiatorId));
        ArgumentException.ThrowIfNullOrWhiteSpace(defenderId, nameof(defenderId));
        ArgumentException.ThrowIfNullOrWhiteSpace(initiatorSkillId, nameof(initiatorSkillId));
        ArgumentException.ThrowIfNullOrWhiteSpace(defenderSkillId, nameof(defenderSkillId));

        _logger.LogDebug(
            "Resolving contested check: {Initiator} ({InitiatorSkill}) vs {Defender} ({DefenderSkill})",
            initiatorId, initiatorSkillId, defenderId, defenderSkillId);

        // Roll for both parties
        var initiatorRoll = RollForSkill(initiatorSkillId, initiatorAdvantage);
        var defenderRoll = RollForSkill(defenderSkillId, defenderAdvantage);

        // Determine outcome using static helper
        var (outcome, margin) = ContestedCheckResult.DetermineOutcome(initiatorRoll, defenderRoll);

        var result = new ContestedCheckResult
        {
            InitiatorId = initiatorId,
            DefenderId = defenderId,
            InitiatorSkillId = initiatorSkillId,
            DefenderSkillId = defenderSkillId,
            InitiatorRoll = initiatorRoll,
            DefenderRoll = defenderRoll,
            Outcome = outcome,
            Margin = margin
        };

        LogContestedResult(result);

        return result;
    }

    /// <summary>
    /// Rolls for a skill check without comparing to a DC.
    /// </summary>
    /// <param name="skillId">The skill ID to roll for.</param>
    /// <param name="advantageType">Advantage type for the roll.</param>
    /// <returns>The dice roll result.</returns>
    private DiceRollResult RollForSkill(string skillId, AdvantageType advantageType)
    {
        var skill = _configProvider.GetSkillById(skillId)
            ?? throw new ArgumentException($"Unknown skill: {skillId}", nameof(skillId));

        var dicePool = DicePool.Parse(skill.BaseDicePool);
        return _diceService.Roll(dicePool, advantageType);
    }

    /// <summary>
    /// Logs the contested check result.
    /// </summary>
    private void LogContestedResult(ContestedCheckResult result)
    {
        var logLevel = result.HadFumble ? LogLevel.Information : LogLevel.Debug;

        _logger.Log(logLevel,
            "Contested check: {Initiator} ({INet}) vs {Defender} ({DNet}) → {Outcome} (margin: {Margin})",
            result.InitiatorId, result.InitiatorNetSuccesses,
            result.DefenderId, result.DefenderNetSuccesses,
            result.Outcome, result.Margin);

        _eventLogger?.LogDice("ContestedCheck", result.ToString(),
            data: new Dictionary<string, object>
            {
                ["initiatorId"] = result.InitiatorId,
                ["defenderId"] = result.DefenderId,
                ["initiatorSkillId"] = result.InitiatorSkillId,
                ["defenderSkillId"] = result.DefenderSkillId,
                ["initiatorNet"] = result.InitiatorNetSuccesses,
                ["defenderNet"] = result.DefenderNetSuccesses,
                ["outcome"] = result.Outcome.ToString(),
                ["margin"] = result.Margin,
                ["winnerId"] = result.WinnerId ?? "none",
                ["hadFumble"] = result.HadFumble
            });
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EXTENDED CHECK METHODS (v0.15.0d)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Starts a new extended skill check.
    /// </summary>
    /// <param name="characterId">ID of the character.</param>
    /// <param name="skillId">ID of the skill to use.</param>
    /// <param name="targetSuccesses">Successes required to complete.</param>
    /// <param name="maxRounds">Maximum rounds allowed (default: 10).</param>
    /// <returns>The initial ExtendedCheckState.</returns>
    /// <remarks>
    /// v0.15.0d: Creates a new extended check that tracks accumulated successes
    /// across multiple rounds. Use PerformExtendedCheckRound() to advance.
    /// </remarks>
    public ExtendedCheckState StartExtendedCheck(
        string characterId,
        string skillId,
        int targetSuccesses,
        int maxRounds = ExtendedCheckConstants.DefaultMaxRounds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(skillId, nameof(skillId));

        // Validate skill exists
        var skill = _configProvider.GetSkillById(skillId)
            ?? throw new ArgumentException($"Unknown skill: {skillId}", nameof(skillId));

        var checkId = Guid.NewGuid().ToString("N")[..8]; // Short unique ID

        var state = ExtendedCheckState.Create(
            checkId,
            characterId,
            skillId,
            targetSuccesses,
            maxRounds);

        _logger.LogInformation(
            "Started extended check {CheckId}: {Character} using {Skill}, " +
            "target {Target} successes in {MaxRounds} rounds",
            checkId, characterId, skill.Name, targetSuccesses, maxRounds);

        _eventLogger?.LogDice("ExtendedCheckStarted", $"Started: {targetSuccesses} successes in {maxRounds} rounds",
            data: new Dictionary<string, object>
            {
                ["checkId"] = checkId,
                ["characterId"] = characterId,
                ["skillId"] = skillId,
                ["targetSuccesses"] = targetSuccesses,
                ["maxRounds"] = maxRounds
            });

        return state;
    }

    /// <summary>
    /// Performs one round of an extended check.
    /// </summary>
    /// <param name="state">The current extended check state.</param>
    /// <param name="advantageType">Advantage type for this round's roll.</param>
    /// <returns>The updated state after processing this round.</returns>
    /// <exception cref="InvalidOperationException">If the check is not in progress.</exception>
    /// <remarks>
    /// v0.15.0d: Each round adds net successes to accumulated total.
    /// Fumbles subtract 2 accumulated successes (min 0).
    /// 3 consecutive fumbles trigger catastrophic failure.
    /// </remarks>
    public ExtendedCheckState PerformExtendedCheckRound(
        ExtendedCheckState state,
        AdvantageType advantageType = AdvantageType.Normal)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (!state.IsActive)
            throw new InvalidOperationException(
                $"Cannot perform round: check is {state.Status}");

        // Roll for this round
        var rollResult = RollForSkill(state.SkillId, advantageType);

        // Process the round (mutates state)
        state.ProcessRound(rollResult);

        // Log the result
        LogExtendedCheckRound(state, rollResult);

        return state;
    }

    /// <summary>
    /// Abandons an in-progress extended check.
    /// </summary>
    /// <param name="state">The extended check state to abandon.</param>
    /// <returns>The updated state with Abandoned status.</returns>
    /// <exception cref="InvalidOperationException">If the check is not in progress.</exception>
    public ExtendedCheckState AbandonExtendedCheck(ExtendedCheckState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.Abandon();

        _logger.LogInformation(
            "Extended check {CheckId} abandoned by {Character} with {Accumulated}/{Target} successes",
            state.CheckId, state.CharacterId, state.AccumulatedSuccesses, state.TargetSuccesses);

        _eventLogger?.LogDice("ExtendedCheckAbandoned", $"Abandoned with {state.AccumulatedSuccesses}/{state.TargetSuccesses}",
            data: new Dictionary<string, object>
            {
                ["checkId"] = state.CheckId,
                ["characterId"] = state.CharacterId,
                ["accumulated"] = state.AccumulatedSuccesses,
                ["target"] = state.TargetSuccesses,
                ["roundsCompleted"] = state.RoundsCompleted
            });

        return state;
    }

    /// <summary>
    /// Logs an extended check round result.
    /// </summary>
    private void LogExtendedCheckRound(ExtendedCheckState state, DiceRollResult rollResult)
    {
        var logLevel = rollResult.IsFumble ? LogLevel.Warning : LogLevel.Debug;
        var riskNote = state.IsAtRisk ? " [AT RISK - one more fumble = catastrophic!]" : "";

        _logger.Log(logLevel,
            "Extended check {CheckId} round {Round}: {Net} net successes " +
            "(accumulated: {Accumulated}/{Target}, remaining: {Remaining}){Risk}",
            state.CheckId, state.RoundsCompleted, rollResult.NetSuccesses,
            state.AccumulatedSuccesses, state.TargetSuccesses, state.RoundsRemaining, riskNote);

        if (state.IsComplete)
        {
            _logger.LogInformation(
                "Extended check {CheckId} completed with status: {Status} " +
                "(accumulated: {Accumulated}, rounds: {Rounds}, fumbles: {Fumbles})",
                state.CheckId, state.Status,
                state.AccumulatedSuccesses, state.RoundsCompleted, state.TotalFumbles);
        }

        _eventLogger?.LogDice("ExtendedCheckRound", $"Round {state.RoundsCompleted}: {rollResult.NetSuccesses} net",
            data: new Dictionary<string, object>
            {
                ["checkId"] = state.CheckId,
                ["characterId"] = state.CharacterId,
                ["skillId"] = state.SkillId,
                ["round"] = state.RoundsCompleted,
                ["netSuccesses"] = rollResult.NetSuccesses,
                ["isFumble"] = rollResult.IsFumble,
                ["accumulated"] = state.AccumulatedSuccesses,
                ["target"] = state.TargetSuccesses,
                ["remaining"] = state.RoundsRemaining,
                ["consecutiveFumbles"] = state.ConsecutiveFumbles,
                ["status"] = state.Status.ToString()
            });
    }

    private SkillCheckResult PerformCheckInternal(
        Player player,
        SkillDefinition skill,
        DifficultyClassDefinition dc,
        AdvantageType advantageType,
        int additionalBonus)
    {
        var attributeBonus = CalculateAttributeBonus(player, skill);
        var otherBonus = CalculateOtherBonus(player, skill, additionalBonus);

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

        LogCheckResult(result);
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

    private int CalculateOtherBonus(Player player, SkillDefinition skill, int additionalBonus)
    {
        var otherBonus = additionalBonus;

        if (skill.RequiresTraining && !PlayerHasSkillTraining(player, skill.Id))
        {
            otherBonus -= skill.UntrainedPenalty;
            _logger.LogDebug(
                "Applied untrained penalty of -{Penalty} for {Skill}",
                skill.UntrainedPenalty, skill.Name);
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

    /// <summary>
    /// Logs the skill check result with appropriate detail level.
    /// </summary>
    /// <remarks>
    /// v0.15.0c: Uses Outcome (6-tier) and NetSuccesses for success-counting mechanics.
    /// </remarks>
    private void LogCheckResult(SkillCheckResult result)
    {
        var level = result.IsCritical ? LogLevel.Information : LogLevel.Debug;

        // v0.15.0c: Log using NetSuccesses and Outcome (success-counting mechanics)
        _logger.Log(level,
            "Skill check complete: Skill={Skill} NetSuccesses={Net} Margin={Margin} DC={DC} Outcome={Outcome}",
            result.SkillName,
            result.NetSuccesses,
            result.Margin,
            result.DifficultyClass,
            result.Outcome);

        // v0.15.0c: Event logging uses new success-counting properties
        _eventLogger?.LogDice("SkillCheck", $"{result.SkillName}: {result.Outcome}",
            data: new Dictionary<string, object>
            {
                ["skillId"] = result.SkillId,
                ["skillName"] = result.SkillName,
                ["netSuccesses"] = result.NetSuccesses,
                ["margin"] = result.Margin,
                ["dc"] = result.DifficultyClass,
                ["dcName"] = result.DifficultyName,
                ["success"] = result.IsSuccess,
                ["critical"] = result.IsCritical,
                ["isFumble"] = result.IsFumble,
                ["outcome"] = result.Outcome.ToString()
            });

        if (result.IsCriticalSuccess)
            _logger.LogInformation("Critical success on {Skill}!", result.SkillName);
        else if (result.IsCriticalFailure)
            _logger.LogInformation("Critical failure on {Skill}!", result.SkillName);
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
