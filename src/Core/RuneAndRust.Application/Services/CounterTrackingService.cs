using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service implementation for counter-tracking (trail concealment) operations.
/// </summary>
/// <remarks>
/// <para>
/// Implements the Counter-Tracking System mechanics for the Wasteland Survival skill.
/// Allows characters to conceal their trail from pursuers using various techniques.
/// The concealer's roll becomes the DC that any tracker must beat.
/// </para>
/// <para>
/// Key mechanics:
/// <list type="bullet">
///   <item><description>Technique bonuses stack additively (+2 to +8 per technique)</description></item>
///   <item><description>Time multipliers compound multiplicatively (x1.0 to x2.0)</description></item>
///   <item><description>Concealment DC = NetSuccesses + TotalBonus, clamped 10-30</description></item>
///   <item><description>Environmental requirements must be met for certain techniques</description></item>
/// </list>
/// </para>
/// </remarks>
public class CounterTrackingService : ICounterTrackingService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The skill ID for Wasteland Survival used in concealment checks.
    /// </summary>
    private const string WastelandSurvivalSkillId = "wasteland-survival";

    /// <summary>
    /// Base DC for the concealment check (0 = use net successes directly).
    /// </summary>
    private const int BaseConcealmentDc = 0;

    /// <summary>
    /// Minimum concealment DC (even a poor roll provides some protection).
    /// </summary>
    private const int MinConcealmentDc = 10;

    /// <summary>
    /// Maximum concealment DC (even the best concealment has limits).
    /// </summary>
    private const int MaxConcealmentDc = 30;

    // ═══════════════════════════════════════════════════════════════════════════
    // TECHNIQUE CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Bonus for walking on hard surfaces.
    /// </summary>
    private const int HardSurfacesBonus = 2;

    /// <summary>
    /// Bonus for brushing away tracks.
    /// </summary>
    private const int BrushTracksBonus = 4;

    /// <summary>
    /// Bonus for creating a false trail.
    /// </summary>
    private const int FalseTrailBonus = 6;

    /// <summary>
    /// Bonus for crossing through water.
    /// </summary>
    private const int WaterCrossingBonus = 8;

    /// <summary>
    /// Bonus for backtracking over own trail.
    /// </summary>
    private const int BacktrackingBonus = 4;

    /// <summary>
    /// Time multiplier for hard surfaces (no penalty).
    /// </summary>
    private const decimal HardSurfacesTimeMultiplier = 1.0m;

    /// <summary>
    /// Time multiplier for brushing tracks (+50% time).
    /// </summary>
    private const decimal BrushTracksTimeMultiplier = 1.5m;

    /// <summary>
    /// Time multiplier for false trail (+100% time).
    /// </summary>
    private const decimal FalseTrailTimeMultiplier = 2.0m;

    /// <summary>
    /// Time multiplier for water crossing (no penalty).
    /// </summary>
    private const decimal WaterCrossingTimeMultiplier = 1.0m;

    /// <summary>
    /// Time multiplier for backtracking (+25% time).
    /// </summary>
    private const decimal BacktrackingTimeMultiplier = 1.25m;

    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly SkillCheckService _skillCheckService;
    private readonly ILogger<CounterTrackingService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the CounterTrackingService.
    /// </summary>
    /// <param name="skillCheckService">Service for performing skill checks.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required dependency is null.
    /// </exception>
    public CounterTrackingService(
        SkillCheckService skillCheckService,
        ILogger<CounterTrackingService> logger)
    {
        _skillCheckService = skillCheckService ?? throw new ArgumentNullException(nameof(skillCheckService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("CounterTrackingService initialized successfully");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIMARY CONCEALMENT OPERATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public Task<CounterTrackingResult> AttemptConcealmentAsync(
        Player player,
        CounterTrackingContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(player, nameof(player));

        _logger.LogInformation(
            "Attempting concealment for ConcealerId={ConcealerId}, Techniques={TechniqueCount}",
            context.ConcealerId, context.TechniqueCount);

        // Get only valid techniques
        var validTechniques = context.GetValidTechniques();
        var invalidTechniques = context.GetInvalidTechniques();

        if (invalidTechniques.Count > 0)
        {
            _logger.LogWarning(
                "Invalid techniques filtered out for ConcealerId={ConcealerId}: {InvalidTechniques}",
                context.ConcealerId, string.Join(", ", invalidTechniques));
        }

        _logger.LogDebug(
            "Valid techniques: {ValidTechniques}",
            validTechniques.Count > 0 ? string.Join(", ", validTechniques) : "None");

        // Calculate bonuses
        var totalBonus = CalculateTotalBonus(validTechniques);
        var timeMultiplier = CalculateTimeMultiplier(validTechniques);

        _logger.LogDebug(
            "Calculated bonuses: TotalBonus=+{Bonus}, TimeMultiplier=x{Multiplier:F2}",
            totalBonus, timeMultiplier);

        // Build skill context with technique information
        var skillContext = context.ToSkillContext();

        // Perform the concealment check
        var checkResult = _skillCheckService.PerformCheckWithContext(
            player,
            WastelandSurvivalSkillId,
            BaseConcealmentDc,
            "Counter-Tracking Concealment",
            skillContext);

        var netSuccesses = checkResult.NetSuccesses;

        _logger.LogDebug(
            "Skill check result: NetSuccesses={NetSuccesses}, Outcome={Outcome}",
            netSuccesses, checkResult.Outcome);

        // Calculate concealment DC
        var rawDc = netSuccesses + totalBonus;
        var clampedDc = Math.Clamp(rawDc, MinConcealmentDc, MaxConcealmentDc);

        _logger.LogInformation(
            "Concealment result for ConcealerId={ConcealerId}: RawDc={RawDc}, ClampedDc={ClampedDc}",
            context.ConcealerId, rawDc, clampedDc);

        // Build roll details string
        var rollDetails = BuildRollDetails(checkResult, totalBonus, validTechniques);

        // Create result
        var result = CounterTrackingResult.Create(
            netSuccesses,
            totalBonus,
            timeMultiplier,
            validTechniques,
            rollDetails);

        _logger.LogDebug(
            "Concealment complete: DC={Dc}, Effectiveness={Effectiveness}, Time=x{Time:F2}",
            result.ConcealmentDc, result.EffectivenessRating, result.TimeMultiplier);

        return Task.FromResult(result);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // BONUS CALCULATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public int CalculateTotalBonus(IEnumerable<ConcealmentTechnique> techniques)
    {
        var bonus = 0;

        foreach (var technique in techniques)
        {
            bonus += GetTechniqueBonus(technique);
        }

        _logger.LogDebug("Calculated total technique bonus: +{Bonus}", bonus);

        return bonus;
    }

    /// <inheritdoc/>
    public decimal CalculateTimeMultiplier(IEnumerable<ConcealmentTechnique> techniques)
    {
        var multiplier = 1.0m;

        foreach (var technique in techniques)
        {
            multiplier *= GetTechniqueTimeMultiplier(technique);
        }

        _logger.LogDebug("Calculated combined time multiplier: x{Multiplier:F3}", multiplier);

        return multiplier;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TECHNIQUE INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public bool ValidateTechniques(CounterTrackingContext context)
    {
        var isValid = context.AreAllTechniquesValid();

        if (!isValid)
        {
            var invalid = context.GetInvalidTechniques();
            _logger.LogDebug(
                "Technique validation failed: {InvalidTechniques}",
                string.Join(", ", invalid));
        }

        return isValid;
    }

    /// <inheritdoc/>
    public IReadOnlyList<ConcealmentTechnique> GetAvailableTechniques(
        bool hasWaterNearby,
        bool hasFoliageOrDebris)
    {
        var available = new List<ConcealmentTechnique>
        {
            // Always available
            ConcealmentTechnique.HardSurfaces,
            ConcealmentTechnique.FalseTrail,
            ConcealmentTechnique.Backtracking
        };

        // Conditionally available
        if (hasWaterNearby)
        {
            available.Add(ConcealmentTechnique.WaterCrossing);
        }

        if (hasFoliageOrDebris)
        {
            available.Add(ConcealmentTechnique.BrushTracks);
        }

        _logger.LogDebug(
            "Available techniques (Water={Water}, Foliage={Foliage}): {Techniques}",
            hasWaterNearby, hasFoliageOrDebris, string.Join(", ", available));

        return available;
    }

    /// <inheritdoc/>
    public int GetTechniqueBonus(ConcealmentTechnique technique)
    {
        return technique switch
        {
            ConcealmentTechnique.HardSurfaces => HardSurfacesBonus,
            ConcealmentTechnique.BrushTracks => BrushTracksBonus,
            ConcealmentTechnique.FalseTrail => FalseTrailBonus,
            ConcealmentTechnique.WaterCrossing => WaterCrossingBonus,
            ConcealmentTechnique.Backtracking => BacktrackingBonus,
            _ => 0
        };
    }

    /// <inheritdoc/>
    public decimal GetTechniqueTimeMultiplier(ConcealmentTechnique technique)
    {
        return technique switch
        {
            ConcealmentTechnique.HardSurfaces => HardSurfacesTimeMultiplier,
            ConcealmentTechnique.BrushTracks => BrushTracksTimeMultiplier,
            ConcealmentTechnique.FalseTrail => FalseTrailTimeMultiplier,
            ConcealmentTechnique.WaterCrossing => WaterCrossingTimeMultiplier,
            ConcealmentTechnique.Backtracking => BacktrackingTimeMultiplier,
            _ => 1.0m
        };
    }

    /// <inheritdoc/>
    public (string Name, string Description) GetTechniqueDescription(ConcealmentTechnique technique)
    {
        return technique switch
        {
            ConcealmentTechnique.HardSurfaces => (
                "Hard Surfaces",
                "Walk on stone, metal, or packed earth that doesn't hold tracks well."),
            ConcealmentTechnique.BrushTracks => (
                "Brush Tracks",
                "Use branches or debris to sweep away your footprints as you go."),
            ConcealmentTechnique.FalseTrail => (
                "False Trail",
                "Create misleading tracks leading in a different direction, then double back."),
            ConcealmentTechnique.WaterCrossing => (
                "Water Crossing",
                "Wade through a stream or pond to break your scent and visual trail."),
            ConcealmentTechnique.Backtracking => (
                "Backtracking",
                "Walk backwards over your own trail to confuse pursuers about your direction."),
            _ => ("Unknown", "Unknown concealment technique.")
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INTEGRATION HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public void ApplyToTrackingState(TrackingState trackingState, CounterTrackingResult result)
    {
        ArgumentNullException.ThrowIfNull(trackingState, nameof(trackingState));

        _logger.LogInformation(
            "Applying counter-tracking to TrackingId={TrackingId}: DC={Dc}, Time=x{Time:F2}",
            trackingState.TrackingId, result.ConcealmentDc, result.TimeMultiplier);

        trackingState.ApplyCounterTracking(result);

        _logger.LogDebug(
            "Counter-tracking applied. TrackingState.ContestedDc={ContestedDc}",
            trackingState.ContestedDc);
    }

    /// <inheritdoc/>
    public void ClearFromTrackingState(TrackingState trackingState)
    {
        ArgumentNullException.ThrowIfNull(trackingState, nameof(trackingState));

        _logger.LogInformation(
            "Clearing counter-tracking from TrackingId={TrackingId}",
            trackingState.TrackingId);

        trackingState.ClearCounterTracking();

        _logger.LogDebug(
            "Counter-tracking cleared. TrackingState.HasCounterTracking={HasCT}",
            trackingState.HasCounterTracking);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Builds a human-readable roll details string.
    /// </summary>
    /// <param name="checkResult">The skill check result.</param>
    /// <param name="totalBonus">The total technique bonus.</param>
    /// <param name="techniques">The techniques used.</param>
    /// <returns>A formatted string describing the roll.</returns>
    private static string BuildRollDetails(
        SkillCheckResult checkResult,
        int totalBonus,
        IReadOnlyList<ConcealmentTechnique> techniques)
    {
        var techList = techniques.Count > 0
            ? string.Join(", ", techniques)
            : "None";

        var bonusStr = totalBonus > 0 ? $"+{totalBonus}" : "0";

        return $"Roll: {checkResult.DiceResult.TotalSuccesses} successes, " +
               $"{checkResult.DiceResult.TotalBotches} botches, " +
               $"Net: {checkResult.NetSuccesses} | " +
               $"Techniques: {techList} ({bonusStr})";
    }
}
