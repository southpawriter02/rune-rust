using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Record of a single tracking check performed during pursuit.
/// </summary>
/// <remarks>
/// <para>
/// Captures all details of an individual tracking check including the phase,
/// base and effective difficulty classes, roll result, terrain, and any
/// modifiers that were applied. This immutable record serves as an audit
/// trail for tracking attempts.
/// </para>
/// <para>
/// Tracking checks are created by the TrackingService when a character
/// attempts to:
/// <list type="bullet">
///   <item><description>Acquire a trail (initial discovery)</description></item>
///   <item><description>Continue pursuit (maintain trail over distance)</description></item>
///   <item><description>Close in on target (final approach)</description></item>
///   <item><description>Recover a lost trail (backtrack, spiral search, etc.)</description></item>
/// </list>
/// </para>
/// <para>
/// The check history is preserved in <see cref="Entities.TrackingState"/> to
/// support narrative replay and debugging of tracking procedures.
/// </para>
/// </remarks>
/// <param name="CheckId">Unique identifier for this check.</param>
/// <param name="Phase">Tracking phase when check was made.</param>
/// <param name="BaseDc">Base DC from trail age classification.</param>
/// <param name="EffectiveDc">Final DC after all modifiers have been applied.</param>
/// <param name="NetSuccesses">Net successes rolled (successes minus botches).</param>
/// <param name="Outcome">Skill check outcome tier classification.</param>
/// <param name="Terrain">Navigation terrain type at time of check.</param>
/// <param name="DistanceAtCheck">Total distance covered when check was made (in miles).</param>
/// <param name="Timestamp">UTC timestamp when the check was performed.</param>
/// <param name="ModifiersApplied">Human-readable description of modifiers used.</param>
public readonly record struct TrackingCheck(
    string CheckId,
    TrackingPhase Phase,
    int BaseDc,
    int EffectiveDc,
    int NetSuccesses,
    SkillOutcome Outcome,
    NavigationTerrainType Terrain,
    float DistanceAtCheck,
    DateTime Timestamp,
    string ModifiersApplied)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DERIVED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Whether this check was successful.
    /// </summary>
    /// <remarks>
    /// Returns true for MarginalSuccess, FullSuccess, ExceptionalSuccess,
    /// or CriticalSuccess outcomes. A marginal success (margin = 0) is still
    /// considered a success, though it may have complications.
    /// </remarks>
    public bool IsSuccess => Outcome is SkillOutcome.MarginalSuccess
        or SkillOutcome.FullSuccess
        or SkillOutcome.ExceptionalSuccess
        or SkillOutcome.CriticalSuccess;

    /// <summary>
    /// Whether this was a critical success (margin ≥ 5).
    /// </summary>
    /// <remarks>
    /// Critical successes in tracking may provide additional benefits such as:
    /// <list type="bullet">
    ///   <item><description>Discovering additional information about the target</description></item>
    ///   <item><description>Gaining bonus to subsequent checks</description></item>
    ///   <item><description>Closing additional distance</description></item>
    /// </list>
    /// </remarks>
    public bool IsCriticalSuccess => Outcome == SkillOutcome.CriticalSuccess;

    /// <summary>
    /// Whether this was a fumble (critical failure due to 0 successes with ≥1 botch).
    /// </summary>
    /// <remarks>
    /// Fumbles in tracking result in severe consequences:
    /// <list type="bullet">
    ///   <item><description>Acquisition: Immediate transition to Cold (trail unfindable)</description></item>
    ///   <item><description>Pursuit: Trail is completely lost with +4 DC to recovery</description></item>
    ///   <item><description>Closing In: Target alerted and may ambush or flee</description></item>
    /// </list>
    /// </remarks>
    public bool IsFumble => Outcome == SkillOutcome.CriticalFailure;

    /// <summary>
    /// Whether this check resulted in a failure (not meeting DC).
    /// </summary>
    /// <remarks>
    /// Normal failures transition to Lost phase but allow recovery attempts.
    /// Contrast with fumbles which have more severe consequences.
    /// </remarks>
    public bool IsFailure => Outcome == SkillOutcome.Failure || Outcome == SkillOutcome.CriticalFailure;

    /// <summary>
    /// Success margin (positive) or failure margin (negative).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Calculated as NetSuccesses - EffectiveDc. This differs from the design spec's
    /// rough conversion (EffectiveDc / 5) because the DC in success-counting represents
    /// the number of net successes required, not a sum threshold.
    /// </para>
    /// <para>
    /// Margin thresholds for outcome classification:
    /// <list type="bullet">
    ///   <item><description>margin &lt; 0: Failure</description></item>
    ///   <item><description>margin = 0: MarginalSuccess</description></item>
    ///   <item><description>margin 1-2: FullSuccess</description></item>
    ///   <item><description>margin 3-4: ExceptionalSuccess</description></item>
    ///   <item><description>margin ≥ 5: CriticalSuccess</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int Margin => NetSuccesses - EffectiveDc;

    /// <summary>
    /// The DC modifier applied to the base DC.
    /// </summary>
    /// <remarks>
    /// Represents the total modifier from conditions, time elapsed,
    /// and other factors. Can be positive (harder) or negative (easier).
    /// </remarks>
    public int DcModifierApplied => EffectiveDc - BaseDc;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a tracking check record from a skill check result.
    /// </summary>
    /// <param name="phase">The tracking phase during which this check was made.</param>
    /// <param name="baseDc">The base DC from trail age classification.</param>
    /// <param name="effectiveDc">The effective DC after all modifiers.</param>
    /// <param name="checkResult">The skill check result from SkillCheckService.</param>
    /// <param name="terrain">The navigation terrain type at the check location.</param>
    /// <param name="distance">The total distance covered at time of check (in miles).</param>
    /// <param name="modifiers">Human-readable description of applied modifiers.</param>
    /// <returns>A new TrackingCheck record with a generated unique identifier.</returns>
    /// <remarks>
    /// This factory method ensures consistent creation of tracking checks from
    /// skill check results. It extracts the necessary data from the check result
    /// and records the current UTC timestamp.
    /// </remarks>
    public static TrackingCheck FromSkillCheck(
        TrackingPhase phase,
        int baseDc,
        int effectiveDc,
        SkillCheckResult checkResult,
        NavigationTerrainType terrain,
        float distance,
        string modifiers)
    {
        return new TrackingCheck(
            CheckId: Guid.NewGuid().ToString(),
            Phase: phase,
            BaseDc: baseDc,
            EffectiveDc: effectiveDc,
            NetSuccesses: checkResult.DiceResult.NetSuccesses,
            Outcome: checkResult.Outcome,
            Terrain: terrain,
            DistanceAtCheck: distance,
            Timestamp: DateTime.UtcNow,
            ModifiersApplied: modifiers);
    }

    /// <summary>
    /// Creates a tracking check record with explicit values (for testing or reconstruction).
    /// </summary>
    /// <param name="checkId">The unique identifier for the check.</param>
    /// <param name="phase">The tracking phase during which this check was made.</param>
    /// <param name="baseDc">The base DC from trail age classification.</param>
    /// <param name="effectiveDc">The effective DC after all modifiers.</param>
    /// <param name="netSuccesses">The net successes from the dice roll.</param>
    /// <param name="outcome">The skill outcome classification.</param>
    /// <param name="terrain">The navigation terrain type at the check location.</param>
    /// <param name="distance">The total distance covered at time of check (in miles).</param>
    /// <param name="timestamp">The UTC timestamp when the check was performed.</param>
    /// <param name="modifiers">Human-readable description of applied modifiers.</param>
    /// <returns>A new TrackingCheck record with the specified values.</returns>
    /// <remarks>
    /// Use this factory method when reconstructing checks from persistence
    /// or creating checks with specific values for unit testing.
    /// </remarks>
    public static TrackingCheck Create(
        string checkId,
        TrackingPhase phase,
        int baseDc,
        int effectiveDc,
        int netSuccesses,
        SkillOutcome outcome,
        NavigationTerrainType terrain,
        float distance,
        DateTime timestamp,
        string modifiers)
    {
        return new TrackingCheck(
            CheckId: checkId,
            Phase: phase,
            BaseDc: baseDc,
            EffectiveDc: effectiveDc,
            NetSuccesses: netSuccesses,
            Outcome: outcome,
            Terrain: terrain,
            DistanceAtCheck: distance,
            Timestamp: timestamp,
            ModifiersApplied: modifiers);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a formatted summary of the check for display and logging.
    /// </summary>
    /// <returns>
    /// A string in the format: "[Phase] RESULT: NetSuccesses vs DC EffectiveDc (ModifiersApplied)"
    /// </returns>
    /// <example>
    /// <code>
    /// // Examples of ToString output:
    /// "[Acquisition] SUCCESS: 4 vs DC 12 (Fresh trail, Blood Trail -4)"
    /// "[Pursuit] FAILURE: 1 vs DC 16 (Standard trail, Rain +4)"
    /// "[ClosingIn] CRITICAL: 8 vs DC 8 (Within 500ft -4)"
    /// "[Lost] FUMBLE: 0 vs DC 20 (Spiral search +4)"
    /// </code>
    /// </example>
    public override string ToString()
    {
        var result = IsSuccess ? "SUCCESS" : "FAILURE";

        if (IsCriticalSuccess)
        {
            result = "CRITICAL";
        }

        if (IsFumble)
        {
            result = "FUMBLE";
        }

        return $"[{Phase}] {result}: {NetSuccesses} vs DC {EffectiveDc} ({ModifiersApplied})";
    }

    /// <summary>
    /// Returns a detailed diagnostic string including all check parameters.
    /// </summary>
    /// <returns>A multi-line string with all check details for debugging.</returns>
    /// <remarks>
    /// Intended for logging and debugging purposes. Includes all properties
    /// in a readable format suitable for log files.
    /// </remarks>
    public string ToDetailedString()
    {
        return $"TrackingCheck [{CheckId}]\n" +
               $"  Phase: {Phase}\n" +
               $"  Terrain: {Terrain}\n" +
               $"  Base DC: {BaseDc}, Effective DC: {EffectiveDc} (modifier: {DcModifierApplied:+#;-#;0})\n" +
               $"  Net Successes: {NetSuccesses}, Margin: {Margin:+#;-#;0}\n" +
               $"  Outcome: {Outcome} (Success: {IsSuccess}, Fumble: {IsFumble})\n" +
               $"  Distance: {DistanceAtCheck:F2} miles\n" +
               $"  Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss} UTC\n" +
               $"  Modifiers: {ModifiersApplied}";
    }
}
