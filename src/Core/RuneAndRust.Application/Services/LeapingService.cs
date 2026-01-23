using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Implements distance-based leaping mechanics for the Acrobatics skill.
/// </summary>
/// <remarks>
/// <para>
/// LeapingService orchestrates the leaping system introduced in v0.15.2b.
/// It handles distance-to-DC mapping, modifier calculations, and
/// integration with the fumble consequence system for [The Long Fall].
/// </para>
/// <para>
/// Dependencies:
/// <list type="bullet">
///   <item><description>IDiceService: For rolling leap checks</description></item>
///   <item><description>IFumbleConsequenceService: For triggering [The Long Fall]</description></item>
///   <item><description>ILogger: For structured logging</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class LeapingService : ILeapingService
{
    /// <summary>
    /// Margin required for a critical success in leaping.
    /// </summary>
    private const int CriticalSuccessMargin = 5;

    /// <summary>
    /// Stress gained from [The Long Fall] fumble.
    /// </summary>
    private const int TheLongFallStress = 2;

    /// <summary>
    /// Duration of [Disoriented] status from fumble in rounds.
    /// </summary>
    private const int DisorientedDuration = 2;

    private readonly IDiceService _diceService;
    private readonly IFumbleConsequenceService _fumbleService;
    private readonly ILogger<LeapingService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeapingService"/> class.
    /// </summary>
    /// <param name="diceService">The dice rolling service.</param>
    /// <param name="fumbleService">The fumble consequence service.</param>
    /// <param name="logger">The logger instance.</param>
    public LeapingService(
        IDiceService diceService,
        IFumbleConsequenceService fumbleService,
        ILogger<LeapingService> logger)
    {
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _fumbleService = fumbleService ?? throw new ArgumentNullException(nameof(fumbleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("LeapingService initialized");
    }

    /// <inheritdoc/>
    public LeapResult AttemptLeap(
        string characterId,
        LeapContext context,
        int baseDicePool,
        SkillContext? additionalContext = null)
    {
        _logger.LogInformation(
            "Character {CharacterId} attempting {Distance}ft leap (DC {Dc})",
            characterId,
            context.DistanceFeet,
            context.FinalDc);

        _logger.LogDebug(
            "Leap context: distance={Distance}ft ({Category}), baseDC={BaseDc}, " +
            "runningStart={RunningStart}, landing={Landing}, encumbered={Encumbered}, " +
            "lowGravity={LowGravity}, standingJump={StandingJump}, finalDC={FinalDc}",
            context.DistanceFeet,
            context.DistanceCategory,
            context.BaseDc,
            context.HasRunningStart,
            context.LandingType,
            context.IsEncumbered,
            context.HasLowGravity,
            context.IsStandingJump,
            context.FinalDc);

        // Apply additional context modifiers to dice pool
        var additionalDice = additionalContext?.TotalDiceModifier ?? 0;
        var totalDicePool = Math.Max(1, baseDicePool + additionalDice);

        _logger.LogDebug(
            "Leap dice pool: base {Base}d10, additional {Additional}d10 = {Total}d10",
            baseDicePool,
            additionalDice,
            totalDicePool);

        // Perform the dice roll
        var dicePool = DicePool.D10(totalDicePool);
        var rollResult = _diceService.Roll(
            dicePool,
            context: "acrobatics-leaping",
            actorId: null);

        // Calculate margin
        var margin = rollResult.NetSuccesses - context.FinalDc;
        var outcome = DetermineOutcome(rollResult, margin);

        _logger.LogInformation(
            "Leap result: {Successes} successes vs DC {Dc}, margin {Margin}, outcome {Outcome}",
            rollResult.NetSuccesses,
            context.FinalDc,
            margin,
            outcome);

        // Process the outcome
        var result = ProcessOutcome(characterId, context, outcome, rollResult.NetSuccesses, margin);

        // Handle fumble consequence
        if (result.IsFumble)
        {
            _logger.LogWarning(
                "Character {CharacterId} triggered [The Long Fall] - fell {Height}ft with bonus damage, " +
                "+{Stress} stress, [{Status}] for {Duration} rounds",
                characterId,
                context.FallDepth,
                TheLongFallStress,
                "Disoriented",
                DisorientedDuration);

            _fumbleService.CreateConsequence(
                characterId,
                "acrobatics-leaping",
                targetId: null,
                context: null);
        }

        // Log stamina cost for successful landings
        if (result.Landed)
        {
            _logger.LogDebug(
                "Character {CharacterId} landed successfully, stamina cost: {Stamina}",
                characterId,
                result.StaminaCost);
        }

        return result;
    }

    /// <inheritdoc/>
    public int CalculateDc(
        int distanceFeet,
        bool hasRunningStart = false,
        LandingType landingType = LandingType.Normal,
        bool isEncumbered = false,
        bool hasLowGravity = false)
    {
        var context = new LeapContext(
            DistanceFeet: distanceFeet,
            HasRunningStart: hasRunningStart,
            LandingType: landingType,
            IsEncumbered: isEncumbered,
            HasLowGravity: hasLowGravity);

        _logger.LogDebug(
            "Calculated DC for {Distance}ft leap: base={BaseDc}, modifiers={Modifiers}, final={FinalDc}",
            distanceFeet,
            context.BaseDc,
            context.TotalDcModifier,
            context.FinalDc);

        return context.FinalDc;
    }

    /// <inheritdoc/>
    public LeapDistance GetDistanceCategory(int distanceFeet)
    {
        return LeapDistanceExtensions.FromFeet(Math.Min(25, Math.Max(1, distanceFeet)));
    }

    /// <inheritdoc/>
    public int GetBaseDc(LeapDistance distance)
    {
        return distance.GetBaseDc();
    }

    /// <inheritdoc/>
    public int GetStaminaCost(LeapDistance distance, SkillOutcome outcome)
    {
        var baseCost = distance.GetBaseStaminaCost();

        var finalCost = outcome switch
        {
            SkillOutcome.CriticalSuccess => baseCost / 2,
            SkillOutcome.MarginalSuccess => baseCost + 1,
            SkillOutcome.Failure or SkillOutcome.CriticalFailure => 0, // Fall damage instead
            _ => baseCost
        };

        _logger.LogDebug(
            "Stamina cost for {Distance} leap with {Outcome}: base={Base}, final={Final}",
            distance,
            outcome,
            baseCost,
            finalCost);

        return finalCost;
    }

    /// <inheritdoc/>
    public bool RequiresMasterRank(int distanceFeet)
    {
        return distanceFeet > 20;
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
    /// Processes the outcome of a leap attempt and creates the result.
    /// </summary>
    /// <param name="characterId">The character ID (unused but kept for logging consistency).</param>
    /// <param name="context">The leap context.</param>
    /// <param name="outcome">The determined skill outcome.</param>
    /// <param name="netSuccesses">Net successes from the roll.</param>
    /// <param name="margin">The margin over DC.</param>
    /// <returns>The leap result with appropriate state.</returns>
    private static LeapResult ProcessOutcome(
        string characterId,
        LeapContext context,
        SkillOutcome outcome,
        int netSuccesses,
        int margin)
    {
        return outcome switch
        {
            SkillOutcome.CriticalFailure => LeapResult.Fumble(context, netSuccesses, margin),
            SkillOutcome.Failure => LeapResult.Failure(context, outcome, netSuccesses, margin),
            _ => LeapResult.Success(context, outcome, netSuccesses, margin)
        };
    }
}
