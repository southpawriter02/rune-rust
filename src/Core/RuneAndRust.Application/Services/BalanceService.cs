namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Constants;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Implements balance check mechanics for traversing narrow surfaces.
/// </summary>
/// <remarks>
/// <para>
/// BalanceService coordinates width-based DC calculation, skill check
/// resolution, and fall damage processing for balance failures.
/// </para>
/// <para>
/// <b>Width Categories:</b>
/// <list type="bullet">
///   <item><description>Wide (2+ ft / 24+ in): DC 2</description></item>
///   <item><description>Narrow (~1 ft / 6-12 in): DC 3</description></item>
///   <item><description>Cable (~6 in / 3-6 in): DC 4</description></item>
///   <item><description>RazorEdge (&lt;6 in / &lt;3 in): DC 5</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Modifiers:</b>
/// <list type="bullet">
///   <item><description>Unstable: +1 DC</description></item>
///   <item><description>Swaying: +2 DC</description></item>
///   <item><description>Wet: +1 DC</description></item>
///   <item><description>Icy: +2 DC</description></item>
///   <item><description>Strong Wind: +1 DC</description></item>
///   <item><description>Severe Wind: +2 DC</description></item>
///   <item><description>Balance Pole: -1 DC</description></item>
/// </list>
/// </para>
/// <para>
/// <b>v0.15.2f:</b> Initial implementation of balance service.
/// </para>
/// </remarks>
public sealed class BalanceService : IBalanceService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Width threshold (in inches) below which balance checks are required.
    /// Surfaces 24+ inches wide don't require checks.
    /// </summary>
    private const int BalanceRequiredThresholdInches = 24;

    /// <summary>
    /// Margin required for a critical success (5+ over DC).
    /// </summary>
    private const int CriticalSuccessMargin = 5;

    /// <summary>
    /// Margin required for exceptional success (3-4 over DC).
    /// </summary>
    private const int ExceptionalSuccessMargin = 3;

    // ═══════════════════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly IDiceService _diceService;
    private readonly ILogger<BalanceService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="BalanceService"/> class.
    /// </summary>
    /// <param name="diceService">Service for performing dice rolls.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public BalanceService(
        IDiceService diceService,
        ILogger<BalanceService> logger)
    {
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("BalanceService initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public BalanceCheckResult AttemptBalance(
        BalanceSurface surface,
        int dicePool,
        int windModifier = 0,
        int encumbranceModifier = 0,
        bool hasBalancePole = false)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dicePool, nameof(dicePool));
        ArgumentOutOfRangeException.ThrowIfNegative(windModifier, nameof(windModifier));
        ArgumentOutOfRangeException.ThrowIfNegative(encumbranceModifier, nameof(encumbranceModifier));

        // Build context
        var context = CreateContext(surface, windModifier, encumbranceModifier, hasBalancePole);

        _logger.LogDebug(
            "Balance attempt: {Width} surface (DC {Dc}), pool size {PoolSize}",
            surface.Width, context.FinalDc, dicePool);

        // Roll the check
        var pool = DicePool.D10(dicePool);
        var roll = _diceService.Roll(pool, context: RollContexts.Skill("Acrobatics"));

        // Calculate margin and classify outcome
        var margin = roll.NetSuccesses - context.FinalDc;
        var outcome = ClassifyOutcome(margin, roll);

        _logger.LogDebug(
            "Balance check: rolled {Successes} successes vs DC {Dc}, margin {Margin}, outcome {Outcome}",
            roll.NetSuccesses, context.FinalDc, margin, outcome);

        // Determine result
        if (outcome >= SkillOutcome.MarginalSuccess)
        {
            _logger.LogInformation(
                "Balance success: {Width} surface crossed, outcome {Outcome}",
                surface.Width, outcome);

            return BalanceCheckResult.Success(context, roll.NetSuccesses, outcome);
        }

        // Failed - character falls
        _logger.LogInformation(
            "Balance failed: fall from {Height} ft ({Width} surface), outcome {Outcome}",
            surface.HeightAboveGround, surface.Width, outcome);

        return BalanceCheckResult.Failure(context, roll.NetSuccesses, outcome);
    }

    /// <inheritdoc/>
    public BalanceContext CreateContext(
        BalanceSurface surface,
        int windModifier = 0,
        int encumbranceModifier = 0,
        bool hasBalancePole = false)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(windModifier, nameof(windModifier));
        ArgumentOutOfRangeException.ThrowIfNegative(encumbranceModifier, nameof(encumbranceModifier));

        var context = new BalanceContext(
            surface,
            windModifier,
            encumbranceModifier,
            hasBalancePole);

        _logger.LogDebug(
            "Balance context created: base DC {BaseDc}, stability +{Stability}, " +
            "condition +{Condition}, wind +{Wind}, encumbrance +{Encumbrance}, " +
            "pole {Pole}, final DC {FinalDc}",
            surface.BaseDc,
            surface.StabilityModifier,
            surface.ConditionModifier,
            windModifier,
            encumbranceModifier,
            hasBalancePole ? "-1" : "N/A",
            context.FinalDc);

        return context;
    }

    /// <inheritdoc/>
    public int CalculateDc(
        BalanceSurface surface,
        int windModifier = 0,
        int encumbranceModifier = 0,
        bool hasBalancePole = false)
    {
        var context = CreateContext(surface, windModifier, encumbranceModifier, hasBalancePole);
        return context.FinalDc;
    }

    /// <inheritdoc/>
    public bool RequiresBalanceCheck(int widthInches)
    {
        var required = widthInches < BalanceRequiredThresholdInches;

        _logger.LogDebug(
            "Balance check required for {Width} in surface: {Required}",
            widthInches, required);

        return required;
    }

    /// <inheritdoc/>
    public BalanceWidth GetWidthCategory(int widthInches)
    {
        var category = widthInches switch
        {
            >= 24 => BalanceWidth.Wide,     // 2+ feet = wide
            >= 12 => BalanceWidth.Wide,     // 1-2 feet = still wide
            >= 6 => BalanceWidth.Narrow,    // 6-12 inches = narrow
            >= 3 => BalanceWidth.Cable,     // 3-6 inches = cable
            _ => BalanceWidth.RazorEdge     // < 3 inches = razor
        };

        _logger.LogDebug(
            "Width category for {Width} in: {Category}",
            widthInches, category);

        return category;
    }

    /// <inheritdoc/>
    public int GetBaseDc(BalanceWidth width)
    {
        return width switch
        {
            BalanceWidth.Wide => 2,
            BalanceWidth.Narrow => 3,
            BalanceWidth.Cable => 4,
            BalanceWidth.RazorEdge => 5,
            _ => 3
        };
    }

    /// <inheritdoc/>
    public int GetStabilityModifier(SurfaceStability stability)
    {
        return stability switch
        {
            SurfaceStability.Stable => 0,
            SurfaceStability.Unstable => 1,
            SurfaceStability.Swaying => 2,
            _ => 0
        };
    }

    /// <inheritdoc/>
    public int GetConditionModifier(SurfaceCondition condition)
    {
        return condition switch
        {
            SurfaceCondition.Dry => 0,
            SurfaceCondition.Wet => 1,
            SurfaceCondition.Icy => 2,
            _ => 0
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Classifies the skill outcome based on margin and roll properties.
    /// </summary>
    /// <param name="margin">The margin (successes - DC).</param>
    /// <param name="roll">The dice roll result.</param>
    /// <returns>The classified skill outcome.</returns>
    private static SkillOutcome ClassifyOutcome(int margin, DiceRollResult roll)
    {
        // Fumble check first (0 successes + at least 1 botch)
        if (roll.IsFumble)
        {
            return SkillOutcome.CriticalFailure;
        }

        // Classify based on margin
        return margin switch
        {
            >= CriticalSuccessMargin => SkillOutcome.CriticalSuccess,
            >= ExceptionalSuccessMargin => SkillOutcome.ExceptionalSuccess,
            >= 1 => SkillOutcome.FullSuccess,
            0 => SkillOutcome.MarginalSuccess,
            _ => SkillOutcome.Failure
        };
    }
}
