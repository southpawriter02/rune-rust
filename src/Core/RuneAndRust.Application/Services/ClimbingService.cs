using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service implementation for managing multi-stage climbing operations.
/// </summary>
/// <remarks>
/// <para>
/// ClimbingService orchestrates the climbing system introduced in v0.15.2a.
/// It handles stage-by-stage progression, dice rolling, outcome determination,
/// and integration with the fumble consequence system.
/// </para>
/// <para>
/// Dependencies:
/// <list type="bullet">
///   <item><description>IDiceService: For rolling climb checks</description></item>
///   <item><description>IFumbleConsequenceService: For triggering [The Slip]</description></item>
///   <item><description>ILogger: For structured logging</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class ClimbingService : IClimbingService
{
    /// <summary>
    /// Margin required for a critical success in climbing.
    /// </summary>
    private const int CriticalSuccessMargin = 5;

    private readonly IDiceService _diceService;
    private readonly IFumbleConsequenceService _fumbleService;
    private readonly ILogger<ClimbingService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClimbingService"/> class.
    /// </summary>
    /// <param name="diceService">The dice rolling service.</param>
    /// <param name="fumbleService">The fumble consequence service.</param>
    /// <param name="logger">The logger instance.</param>
    public ClimbingService(
        IDiceService diceService,
        IFumbleConsequenceService fumbleService,
        ILogger<ClimbingService> logger)
    {
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _fumbleService = fumbleService ?? throw new ArgumentNullException(nameof(fumbleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("ClimbingService initialized");
    }

    /// <inheritdoc/>
    public ClimbState StartClimb(string characterId, ClimbContext context)
    {
        _logger.LogInformation(
            "Character {CharacterId} starting climb: {Height}ft, {Stages} stages, {Surface} surface",
            characterId,
            context.TotalHeight,
            context.StagesRequired,
            context.SurfaceType);

        _logger.LogDebug(
            "Climb context details: BaseDC={BaseDc}, EffectiveDC={EffectiveDc}, " +
            "SurfaceMod={SurfaceMod}d10, EquipMod={EquipMod}d10, ArmorPen={ArmorPen}d10, " +
            "TotalMod={TotalMod}d10",
            context.BaseDc,
            context.EffectiveDc,
            context.SurfaceDiceModifier,
            context.EquipmentDiceModifier,
            context.ArmorPenalty,
            context.TotalDiceModifier);

        var climbState = ClimbState.StartClimb(characterId, context);

        _logger.LogInformation(
            "Climb started with ID {ClimbId}",
            climbState.ClimbId);

        return climbState;
    }

    /// <inheritdoc/>
    public ClimbStageResult AttemptStage(
        ClimbState climbState,
        int baseDicePool,
        SkillContext? additionalContext = null)
    {
        // Validate climb is in progress
        if (!climbState.IsInProgress)
        {
            throw new InvalidOperationException(
                $"Cannot attempt stage for climb with status {climbState.Status}.");
        }

        // Get the next stage to attempt
        var nextStage = climbState.GetNextStage();
        if (nextStage is null)
        {
            throw new InvalidOperationException(
                "No more stages to attempt - climb should already be complete.");
        }

        var stage = nextStage.Value;

        _logger.LogDebug(
            "Character {CharacterId} attempting stage {Stage} of climb {ClimbId}",
            climbState.CharacterId,
            stage.StageNumber,
            climbState.ClimbId);

        // Calculate total dice pool
        var contextModifier = climbState.Context.TotalDiceModifier;
        var additionalModifier = additionalContext?.TotalDiceModifier ?? 0;
        var totalDicePool = Math.Max(1, baseDicePool + contextModifier + additionalModifier);

        _logger.LogDebug(
            "Attempting stage {Stage}: base {Base}d10, context {Context}d10, additional {Additional}d10 = {Total}d10",
            stage.StageNumber,
            baseDicePool,
            contextModifier,
            additionalModifier,
            totalDicePool);

        // Perform the dice roll using d10 pool
        var dicePool = DicePool.D10(totalDicePool);
        var rollResult = _diceService.Roll(
            dicePool,
            context: "acrobatics-climbing",
            actorId: null);

        // Determine outcome
        var dc = stage.StageDc;
        var margin = rollResult.NetSuccesses - dc;
        var outcome = DetermineOutcome(rollResult, margin);

        _logger.LogInformation(
            "Stage {Stage} result: {Successes} successes vs DC {Dc}, margin {Margin}, outcome {Outcome}",
            stage.StageNumber,
            rollResult.NetSuccesses,
            dc,
            margin,
            outcome);

        // Process the outcome
        var result = ProcessStageOutcome(climbState, stage, outcome, rollResult.NetSuccesses, margin);

        // Record the attempt in climb state
        climbState.RecordStageAttempt(result);

        // Handle fumble consequence
        if (result.IsFumble)
        {
            _logger.LogWarning(
                "Character {CharacterId} fell from {Height}ft during climb (fumble)",
                climbState.CharacterId,
                result.FallHeight);

            // Trigger TheSlip fumble consequence
            _fumbleService.CreateConsequence(
                climbState.CharacterId,
                "acrobatics-climbing",
                targetId: null,
                context: null);
        }

        // Log completion or continuation
        if (result.ClimbCompleted)
        {
            _logger.LogInformation(
                "Character {CharacterId} completed climb to {Height}ft in {Stages} stages",
                climbState.CharacterId,
                climbState.Context.TotalHeight,
                climbState.StageHistory.Count);
        }

        return result;
    }

    /// <inheritdoc/>
    public FallResult ProcessFall(ClimbState climbState)
    {
        if (climbState.Status != ClimbStatus.Fallen)
        {
            throw new InvalidOperationException(
                $"Cannot process fall for climb with status {climbState.Status}.");
        }

        // Get the last attempted stage to determine fall height
        var lastStage = climbState.StageHistory.LastOrDefault();
        var fallHeight = lastStage.FallHeight;

        _logger.LogInformation(
            "Processing fall for {CharacterId}: {Height}ft",
            climbState.CharacterId,
            fallHeight);

        return FallResult.FromHeight(fallHeight, FallSource.Climbing);
    }

    /// <inheritdoc/>
    public void AbandonClimb(ClimbState climbState)
    {
        _logger.LogInformation(
            "Character {CharacterId} abandoning climb at stage {Stage}",
            climbState.CharacterId,
            climbState.CurrentStage);

        climbState.Abandon();

        _logger.LogDebug(
            "Climb {ClimbId} abandoned, character returned to ground safely",
            climbState.ClimbId);
    }

    /// <inheritdoc/>
    public int CalculateStagesRequired(int heightFeet)
    {
        return heightFeet switch
        {
            <= 0 => 0,
            <= 20 => 1,
            <= 40 => 2,
            _ => 3
        };
    }

    /// <inheritdoc/>
    public int GetSurfaceDiceModifier(SurfaceType surfaceType)
    {
        return surfaceType.GetDiceModifier();
    }

    /// <inheritdoc/>
    public int GetSurfaceDcModifier(SurfaceType surfaceType)
    {
        return surfaceType.GetDcModifier();
    }

    /// <summary>
    /// Determines the skill outcome based on roll result and margin.
    /// </summary>
    /// <param name="roll">The dice roll result.</param>
    /// <param name="margin">The margin (net successes - DC).</param>
    /// <returns>The determined skill outcome.</returns>
    private static SkillOutcome DetermineOutcome(DiceRollResult roll, int margin)
    {
        // Check for fumble first (0 successes + ≥1 botch)
        if (roll.IsFumble)
        {
            return SkillOutcome.CriticalFailure;
        }

        // Check for critical success (margin ≥ 5 OR dice roll critical)
        if (margin >= CriticalSuccessMargin || roll.IsCriticalSuccess)
        {
            return SkillOutcome.CriticalSuccess;
        }

        // Standard success/failure based on margin
        return margin switch
        {
            >= 3 => SkillOutcome.ExceptionalSuccess,
            >= 1 => SkillOutcome.FullSuccess,
            0 => SkillOutcome.MarginalSuccess,
            _ => SkillOutcome.Failure
        };
    }

    /// <summary>
    /// Processes the outcome of a stage attempt and creates the result.
    /// </summary>
    /// <param name="climbState">The current climb state.</param>
    /// <param name="stage">The stage that was attempted.</param>
    /// <param name="outcome">The determined skill outcome.</param>
    /// <param name="netSuccesses">Net successes from the roll.</param>
    /// <param name="margin">The margin over DC.</param>
    /// <returns>The stage result with updated status.</returns>
    private ClimbStageResult ProcessStageOutcome(
        ClimbState climbState,
        ClimbingStage stage,
        SkillOutcome outcome,
        int netSuccesses,
        int margin)
    {
        var currentStage = climbState.CurrentStage;
        var totalStages = climbState.Context.StagesRequired;

        switch (outcome)
        {
            case SkillOutcome.CriticalFailure:
                // Fumble - fall from current height
                _logger.LogDebug(
                    "Stage {Stage} fumble: falling from {Height}ft",
                    stage.StageNumber,
                    stage.FallHeight);

                return ClimbStageResult.Fumble(
                    stage,
                    netSuccesses,
                    margin,
                    stage.FallHeight);

            case SkillOutcome.Failure:
                // Slip back one stage (minimum 0)
                var newStageAfterFailure = Math.Max(0, currentStage);

                _logger.LogDebug(
                    "Stage {Stage} failure: slipping to stage {NewStage}",
                    stage.StageNumber,
                    newStageAfterFailure);

                return ClimbStageResult.Failure(
                    stage,
                    outcome,
                    netSuccesses,
                    margin,
                    newStageAfterFailure);

            case SkillOutcome.CriticalSuccess:
                // Advance 2 stages or reach top
                var stagesAdvanced = Math.Min(2, totalStages - currentStage);
                var newStageAfterCritical = currentStage + stagesAdvanced;

                _logger.LogDebug(
                    "Stage {Stage} critical success: advancing {Stages} stages to stage {NewStage}",
                    stage.StageNumber,
                    stagesAdvanced,
                    newStageAfterCritical);

                return ClimbStageResult.Success(
                    stage,
                    outcome,
                    netSuccesses,
                    margin,
                    newStageAfterCritical,
                    totalStages,
                    stagesAdvanced);

            default:
                // Normal success - advance 1 stage
                var newStageAfterSuccess = currentStage + 1;

                _logger.LogDebug(
                    "Stage {Stage} success: advancing to stage {NewStage}",
                    stage.StageNumber,
                    newStageAfterSuccess);

                return ClimbStageResult.Success(
                    stage,
                    outcome,
                    netSuccesses,
                    margin,
                    newStageAfterSuccess,
                    totalStages,
                    stagesAdvanced: 1);
        }
    }
}
