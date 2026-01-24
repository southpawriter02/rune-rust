using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service interface for counter-tracking (trail concealment) operations.
/// </summary>
/// <remarks>
/// <para>
/// Provides functionality for characters to conceal their trail from pursuers
/// using the Wasteland Survival skill. The counter-tracking system creates a
/// contested skill check where the concealer's roll becomes the DC that any
/// tracker must beat.
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
public interface ICounterTrackingService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIMARY CONCEALMENT OPERATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Attempts to conceal the character's trail using the specified techniques.
    /// </summary>
    /// <param name="player">The character attempting to conceal their trail.</param>
    /// <param name="context">The counter-tracking context with techniques and environment.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>
    /// A <see cref="CounterTrackingResult"/> containing the concealment DC, time multiplier,
    /// and other details of the concealment attempt.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="player"/> or <paramref name="context"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when techniques with unmet environmental requirements are used.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Performs a Wasteland Survival skill check and adds the technique bonuses
    /// to create the concealment DC. The result can be applied to a TrackingState
    /// to make the trail harder to follow.
    /// </para>
    /// <para>
    /// Invalid techniques (those with unmet requirements) are automatically
    /// filtered out. If all techniques are invalid, a basic concealment is
    /// attempted with no bonus.
    /// </para>
    /// </remarks>
    Task<CounterTrackingResult> AttemptConcealmentAsync(
        Player player,
        CounterTrackingContext context,
        CancellationToken cancellationToken = default);

    // ═══════════════════════════════════════════════════════════════════════════
    // BONUS CALCULATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates the total bonus from a list of techniques (additive stacking).
    /// </summary>
    /// <param name="techniques">The techniques to sum bonuses for.</param>
    /// <returns>The total bonus from all techniques.</returns>
    /// <remarks>
    /// <para>
    /// Technique bonuses:
    /// <list type="bullet">
    ///   <item><description>HardSurfaces: +2</description></item>
    ///   <item><description>BrushTracks: +4</description></item>
    ///   <item><description>FalseTrail: +6</description></item>
    ///   <item><description>WaterCrossing: +8</description></item>
    ///   <item><description>Backtracking: +4</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Example: BrushTracks (+4) + Backtracking (+4) = +8 total bonus.
    /// </para>
    /// </remarks>
    int CalculateTotalBonus(IEnumerable<ConcealmentTechnique> techniques);

    /// <summary>
    /// Calculates the combined time multiplier from techniques (multiplicative).
    /// </summary>
    /// <param name="techniques">The techniques to compound multipliers for.</param>
    /// <returns>The combined time multiplier.</returns>
    /// <remarks>
    /// <para>
    /// Time multipliers:
    /// <list type="bullet">
    ///   <item><description>HardSurfaces: x1.0</description></item>
    ///   <item><description>BrushTracks: x1.5</description></item>
    ///   <item><description>FalseTrail: x2.0</description></item>
    ///   <item><description>WaterCrossing: x1.0</description></item>
    ///   <item><description>Backtracking: x1.25</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Example: BrushTracks (x1.5) × Backtracking (x1.25) = x1.875 time.
    /// </para>
    /// </remarks>
    decimal CalculateTimeMultiplier(IEnumerable<ConcealmentTechnique> techniques);

    // ═══════════════════════════════════════════════════════════════════════════
    // TECHNIQUE INFORMATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validates that all techniques in the context can be used.
    /// </summary>
    /// <param name="context">The counter-tracking context to validate.</param>
    /// <returns>True if all techniques have their requirements met.</returns>
    /// <remarks>
    /// Checks environmental requirements:
    /// <list type="bullet">
    ///   <item><description>WaterCrossing requires HasWaterNearby</description></item>
    ///   <item><description>BrushTracks requires HasFoliageOrDebris</description></item>
    /// </list>
    /// </remarks>
    bool ValidateTechniques(CounterTrackingContext context);

    /// <summary>
    /// Gets the techniques available in the given environmental context.
    /// </summary>
    /// <param name="hasWaterNearby">Whether water is available.</param>
    /// <param name="hasFoliageOrDebris">Whether foliage or debris is available.</param>
    /// <returns>A list of techniques that can be used.</returns>
    /// <remarks>
    /// Always includes: HardSurfaces, FalseTrail, Backtracking.
    /// Conditionally includes: WaterCrossing (water), BrushTracks (foliage).
    /// </remarks>
    IReadOnlyList<ConcealmentTechnique> GetAvailableTechniques(
        bool hasWaterNearby,
        bool hasFoliageOrDebris);

    /// <summary>
    /// Gets the bonus value for a specific technique.
    /// </summary>
    /// <param name="technique">The technique to get the bonus for.</param>
    /// <returns>The bonus value (+2 to +8).</returns>
    int GetTechniqueBonus(ConcealmentTechnique technique);

    /// <summary>
    /// Gets the time multiplier for a specific technique.
    /// </summary>
    /// <param name="technique">The technique to get the multiplier for.</param>
    /// <returns>The time multiplier (1.0 to 2.0).</returns>
    decimal GetTechniqueTimeMultiplier(ConcealmentTechnique technique);

    /// <summary>
    /// Gets a human-readable description of a technique.
    /// </summary>
    /// <param name="technique">The technique to describe.</param>
    /// <returns>A display name and description for the technique.</returns>
    (string Name, string Description) GetTechniqueDescription(ConcealmentTechnique technique);

    // ═══════════════════════════════════════════════════════════════════════════
    // INTEGRATION HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Applies a concealment result to a tracking state.
    /// </summary>
    /// <param name="trackingState">The tracking state to modify.</param>
    /// <param name="result">The concealment result to apply.</param>
    /// <remarks>
    /// <para>
    /// Sets the contested DC and time multiplier on the tracking state.
    /// After calling this, trackers will need to beat the concealment DC
    /// instead of the normal trail age DC.
    /// </para>
    /// <para>
    /// This is a convenience method that calls <see cref="TrackingState.ApplyCounterTracking"/>.
    /// </para>
    /// </remarks>
    void ApplyToTrackingState(TrackingState trackingState, CounterTrackingResult result);

    /// <summary>
    /// Clears counter-tracking from a tracking state.
    /// </summary>
    /// <param name="trackingState">The tracking state to clear.</param>
    /// <remarks>
    /// Reverts to normal trail age DC and removes time multiplier.
    /// Call this when the tracker catches up to the concealed section.
    /// </remarks>
    void ClearFromTrackingState(TrackingState trackingState);
}
