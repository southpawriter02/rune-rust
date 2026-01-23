using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Implementation of <see cref="IFallDamageService"/> for processing fall damage.
/// </summary>
/// <remarks>
/// <para>
/// Handles the complete fall damage workflow including:
/// <list type="bullet">
///   <item><description>Damage dice calculation from height (1d10 per 10ft, max 10d10)</description></item>
///   <item><description>Crash Landing Acrobatics checks for damage reduction</description></item>
///   <item><description>Damage dice rolling (sum of d10s)</description></item>
///   <item><description>Result aggregation for display</description></item>
/// </list>
/// </para>
/// <para>
/// Dependencies:
/// <list type="bullet">
///   <item><description>IDiceService: For rolling Crash Landing checks and damage dice</description></item>
///   <item><description>ILogger: For structured logging</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class FallDamageService : IFallDamageService
{
    /// <summary>
    /// Margin required for a critical success in Crash Landing.
    /// </summary>
    private const int CriticalSuccessMargin = 5;

    private readonly IDiceService _diceService;
    private readonly ILogger<FallDamageService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FallDamageService"/> class.
    /// </summary>
    /// <param name="diceService">The dice rolling service.</param>
    /// <param name="logger">The logger instance.</param>
    public FallDamageService(
        IDiceService diceService,
        ILogger<FallDamageService> logger)
    {
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("FallDamageService initialized");
    }

    /// <inheritdoc/>
    public FallDamage CalculateFallDamage(
        int heightFeet,
        FallSource source = FallSource.Environmental,
        int bonusDice = 0)
    {
        _logger.LogDebug(
            "Calculating fall damage for {Height}ft fall from {Source} with {BonusDice} bonus dice",
            heightFeet, source, bonusDice);

        var fallDamage = FallDamage.FromHeight(heightFeet, source, bonusDice);

        _logger.LogInformation(
            "Fall damage calculated: {Height}ft = {Dice}d10 + {Bonus}d10 bonus = {Total}d10 (DC {Dc})",
            fallDamage.FallHeight,
            fallDamage.DamageDice,
            fallDamage.BonusDamageDice,
            fallDamage.TotalDamageDice,
            fallDamage.CrashLandingDc);

        return fallDamage;
    }

    /// <inheritdoc/>
    public FallDamage CalculateFallDamage(FallResult fallResult)
    {
        _logger.LogDebug(
            "Calculating fall damage from FallResult: {Height}ft from {Source}",
            fallResult.FallHeight, fallResult.Source);

        return FallDamage.FromFallResult(fallResult);
    }

    /// <inheritdoc/>
    public CrashLandingResult AttemptCrashLanding(
        string characterId,
        FallDamage fallDamage,
        int baseDicePool,
        SkillContext? context = null)
    {
        // Check if Crash Landing is available
        if (!fallDamage.CanCrashLand)
        {
            _logger.LogDebug(
                "Crash Landing not available for {Height}ft fall (below {Min}ft threshold)",
                fallDamage.FallHeight, FallDamage.MinDamageHeight);

            return CrashLandingResult.NoAttempt(fallDamage.TotalDamageDice);
        }

        var crashDc = fallDamage.CrashLandingDc;

        _logger.LogInformation(
            "Character {CharacterId} attempting Crash Landing: DC {Dc} for {Height}ft fall",
            characterId, crashDc, fallDamage.FallHeight);

        // Apply additional context modifiers to dice pool
        var additionalDice = context?.TotalDiceModifier ?? 0;
        var totalDicePool = Math.Max(1, baseDicePool + additionalDice);

        _logger.LogDebug(
            "Crash Landing dice pool: base {Base}d10, modifiers {Mod}d10 = {Total}d10",
            baseDicePool, additionalDice, totalDicePool);

        // Perform the Acrobatics check
        var dicePool = DicePool.D10(totalDicePool);
        var rollResult = _diceService.Roll(
            dicePool,
            context: "acrobatics-crash-landing",
            actorId: null);

        // Determine outcome
        var margin = rollResult.NetSuccesses - crashDc;
        var outcome = DetermineOutcome(rollResult, margin);

        // Create result
        var crashResult = CrashLandingResult.FromRoll(
            crashDc: crashDc,
            successes: rollResult.NetSuccesses,
            originalDamageDice: fallDamage.TotalDamageDice,
            outcome: outcome);

        // Log the result
        if (crashResult.NegatedAllDamage)
        {
            _logger.LogInformation(
                "Crash Landing PERFECT: {CharacterId} negated all damage! " +
                "({Successes} successes vs DC {Dc})",
                characterId, rollResult.NetSuccesses, crashDc);
        }
        else if (crashResult.ReducedDamage)
        {
            _logger.LogInformation(
                "Crash Landing SUCCESS: {CharacterId} reduced damage by {Reduced}d10 " +
                "({Successes} successes vs DC {Dc}, {Original}d10 → {Final}d10)",
                characterId,
                crashResult.DiceReduced,
                rollResult.NetSuccesses,
                crashDc,
                crashResult.OriginalDamageDice,
                crashResult.FinalDamageDice);
        }
        else
        {
            _logger.LogInformation(
                "Crash Landing FAILED: {CharacterId} takes full damage " +
                "({Successes} successes vs DC {Dc})",
                characterId, rollResult.NetSuccesses, crashDc);
        }

        return crashResult;
    }

    /// <inheritdoc/>
    public int RollDamage(int damageDice)
    {
        if (damageDice <= 0)
        {
            _logger.LogDebug("No damage dice to roll");
            return 0;
        }

        _logger.LogDebug("Rolling {Dice}d10 fall damage", damageDice);

        // Roll each die and sum
        var dicePool = DicePool.D10(damageDice);
        var rollResult = _diceService.Roll(
            dicePool,
            context: "fall-damage");

        // Sum all dice values for damage (not success counting)
        var total = rollResult.Rolls.Sum();

        _logger.LogInformation(
            "Fall damage rolled: {Dice}d10 = [{Rolls}] = {Total}",
            damageDice,
            string.Join(", ", rollResult.Rolls),
            total);

        return total;
    }

    /// <inheritdoc/>
    public FallDamageResult ProcessFall(
        string characterId,
        FallResult fallResult,
        int baseDicePool,
        bool attemptCrashLanding = true,
        SkillContext? context = null)
    {
        _logger.LogInformation(
            "Processing fall for character {CharacterId}: {Height}ft from {Source}",
            characterId, fallResult.FallHeight, fallResult.Source);

        // Step 1: Calculate base fall damage
        var fallDamage = CalculateFallDamage(fallResult);

        // Step 2: Check for no-damage fall (below threshold)
        if (!fallDamage.CausesDamage)
        {
            _logger.LogInformation(
                "Fall from {Height}ft causes no damage (below {Threshold}ft threshold)",
                fallDamage.FallHeight, FallDamage.MinDamageHeight);

            return FallDamageResult.NoDamage(fallDamage, characterId);
        }

        // Step 3: Attempt Crash Landing if requested and available
        CrashLandingResult crashResult;
        int finalDamageDice;

        if (attemptCrashLanding && fallDamage.CanCrashLand)
        {
            crashResult = AttemptCrashLanding(characterId, fallDamage, baseDicePool, context);
            finalDamageDice = crashResult.FinalDamageDice;
        }
        else
        {
            _logger.LogDebug(
                "Crash Landing skipped: attemptCrashLanding={Attempt}, canCrashLand={CanCrash}",
                attemptCrashLanding, fallDamage.CanCrashLand);

            crashResult = CrashLandingResult.NoAttempt(fallDamage.TotalDamageDice);
            finalDamageDice = fallDamage.TotalDamageDice;
        }

        // Step 4: Roll final damage
        var damageRolled = RollDamage(finalDamageDice);

        // Step 5: Create result
        var result = FallDamageResult.Create(
            fallDamage: fallDamage,
            crashLanding: crashResult,
            damageRolled: damageRolled,
            characterId: characterId);

        // Log warning for severe damage
        if (damageRolled >= 50)
        {
            _logger.LogWarning(
                "SEVERE FALL: Character {CharacterId} took {Damage} damage from {Height}ft fall!",
                characterId, damageRolled, fallDamage.FallHeight);
        }
        else
        {
            _logger.LogInformation(
                "Fall damage result: {CharacterId} takes {Damage} {Type} damage from {Height}ft fall",
                characterId, damageRolled, fallDamage.DamageType, fallDamage.FallHeight);
        }

        return result;
    }

    /// <inheritdoc/>
    public int GetCrashLandingDc(int heightFeet)
    {
        if (heightFeet < FallDamage.MinDamageHeight)
        {
            return 0; // No Crash Landing needed for safe falls
        }

        var dc = FallDamage.CrashLandingBaseDc + (heightFeet / FallDamage.HeightPerDie);

        _logger.LogDebug(
            "Crash Landing DC for {Height}ft: base {Base} + height factor {Factor} = DC {Dc}",
            heightFeet,
            FallDamage.CrashLandingBaseDc,
            heightFeet / FallDamage.HeightPerDie,
            dc);

        return dc;
    }

    /// <inheritdoc/>
    public int GetDamageDice(int heightFeet)
    {
        var dice = Math.Min(FallDamage.MaxDamageDice, heightFeet / FallDamage.HeightPerDie);

        _logger.LogDebug(
            "Damage dice for {Height}ft fall: {Dice}d10 (max {Max}d10)",
            heightFeet, dice, FallDamage.MaxDamageDice);

        return dice;
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
}
