// ------------------------------------------------------------------------------
// <copyright file="GlitchObservationResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Result of attempting to observe and identify a glitch pattern.
// Part of v0.15.4f Glitch Exploitation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of attempting to observe and identify a glitch pattern.
/// </summary>
/// <remarks>
/// <para>
/// Pattern observation is a WITS check against DC 14 that takes 1 round.
/// On success, the character identifies the glitch cycle and can time their
/// actions to exploit the Permissive phase for a -4 DC bonus.
/// </para>
/// <para>
/// On failure, the character must either:
/// <list type="bullet">
///   <item><description>Retry the observation (spending another round)</description></item>
///   <item><description>Proceed blind and roll on the Chaos table</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Success">Whether the observation succeeded (net successes > 0).</param>
/// <param name="NetSuccesses">The net successes from the WITS check.</param>
/// <param name="GlitchState">The resulting glitch state (observed or unobserved).</param>
/// <param name="NarrativeText">Descriptive text for the observation attempt.</param>
public readonly record struct GlitchObservationResult(
    bool Success,
    int NetSuccesses,
    GlitchState GlitchState,
    string NarrativeText)
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// The DC for pattern observation checks.
    /// </summary>
    public const int ObservationDc = 14;

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a successful observation result.
    /// </summary>
    /// <param name="netSuccesses">The net successes from the WITS check.</param>
    /// <param name="glitchState">The observed glitch state.</param>
    /// <param name="mechanismType">The type of mechanism observed (for narrative).</param>
    /// <returns>A GlitchObservationResult for successful pattern identification.</returns>
    /// <remarks>
    /// On success, the character knows the current phase, exploit window timing,
    /// and can wait for the optimal moment to act.
    /// </remarks>
    public static GlitchObservationResult Succeeded(
        int netSuccesses,
        GlitchState glitchState,
        string mechanismType)
    {
        var phaseDescription = glitchState.CyclePhase switch
        {
            Enums.GlitchCyclePhase.Stable =>
                "The machine is currently in a stable phase, its corruption simmering beneath the surface.",
            Enums.GlitchCyclePhase.Unstable =>
                "The machine is becoming unstable—lights flicker and strange sounds emanate from within.",
            Enums.GlitchCyclePhase.Permissive =>
                "The machine's defenses are momentarily lapsed! This is your window to strike!",
            Enums.GlitchCyclePhase.Lockdown =>
                "The machine is in lockdown, compensating for its corruption. Wait for it to cycle.",
            _ => "The machine's state is unclear."
        };

        var narrative = $"You study the {mechanismType}'s erratic behavior, and gradually a pattern emerges. " +
                       "The corruption cycles through phases—stable, unstable, permissive, lockdown. " +
                       $"{phaseDescription} " +
                       "When it reaches the permissive phase, that's your moment to act for maximum advantage.";

        return new GlitchObservationResult(
            Success: true,
            NetSuccesses: netSuccesses,
            GlitchState: glitchState,
            NarrativeText: narrative);
    }

    /// <summary>
    /// Creates a failed observation result.
    /// </summary>
    /// <param name="netSuccesses">The net successes from the WITS check (0 or negative).</param>
    /// <param name="glitchState">The unobserved glitch state.</param>
    /// <returns>A GlitchObservationResult for failed pattern identification.</returns>
    /// <remarks>
    /// On failure, the character cannot identify the pattern and must either
    /// retry or proceed blind (rolling on the Chaos table).
    /// </remarks>
    public static GlitchObservationResult Failed(
        int netSuccesses,
        GlitchState glitchState)
    {
        return new GlitchObservationResult(
            Success: false,
            NetSuccesses: netSuccesses,
            GlitchState: glitchState,
            NarrativeText: "The machine's erratic behavior defies your attempts to find a pattern. " +
                          "Its glitches seem random, unpredictable—the corruption too deep to read. " +
                          "You'll have to proceed blind and hope the chaos favors you.");
    }

    /// <summary>
    /// Creates a result for skipped observation.
    /// </summary>
    /// <param name="glitchState">The unobserved glitch state.</param>
    /// <returns>A GlitchObservationResult for skipped observation.</returns>
    /// <remarks>
    /// Characters may choose to skip observation and proceed directly to
    /// exploitation, accepting the risk of the Chaos table.
    /// </remarks>
    public static GlitchObservationResult Skipped(GlitchState glitchState)
    {
        return new GlitchObservationResult(
            Success: false,
            NetSuccesses: 0,
            GlitchState: glitchState,
            NarrativeText: "You decide not to waste time observing the machine's behavior. " +
                          "Fortune favors the bold—or so you tell yourself as you proceed blindly.");
    }

    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets whether the observation was attempted (not skipped).
    /// </summary>
    public bool WasAttempted => NetSuccesses != 0 || Success;

    /// <summary>
    /// Gets whether pattern identification allows timing actions.
    /// </summary>
    /// <remarks>
    /// True when the pattern is identified, allowing the character to wait
    /// for the exploit window.
    /// </remarks>
    public bool CanTimeActions => Success && GlitchState.GlitchCycleIdentified;

    /// <summary>
    /// Gets whether a chaos roll will be required.
    /// </summary>
    /// <remarks>
    /// True when the pattern is not identified, meaning the character must
    /// roll on the Chaos table when attempting exploitation.
    /// </remarks>
    public bool RequiresChaosRoll => !Success;

    /// <summary>
    /// Gets the current exploit window status.
    /// </summary>
    /// <remarks>
    /// Returns whether the character is currently in the exploit window,
    /// or null if the pattern is not identified.
    /// </remarks>
    public bool? InExploitWindow => Success ? GlitchState.IsInExploitWindow : null;

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a formatted display string for the observation result.
    /// </summary>
    /// <returns>A human-readable summary of the observation attempt.</returns>
    public string ToDisplayString()
    {
        var result = Success ? "SUCCESS" : "FAILURE";
        var lines = new List<string>
        {
            $"Pattern Observation: {result} (Net {NetSuccesses} vs DC {ObservationDc})"
        };

        if (Success)
        {
            lines.Add($"Current Phase: {GlitchState.CyclePhase}");
            lines.Add($"Exploit Window: {GlitchState.ExploitWindow}");

            if (GlitchState.IsInExploitWindow)
            {
                lines.Add("*** IN EXPLOIT WINDOW (-4 DC) ***");
            }
        }
        else
        {
            lines.Add("Pattern unknown. Chaos roll required if proceeding.");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"GlitchObservationResult[Success={Success} Net={NetSuccesses} " +
               $"Phase={GlitchState.CyclePhase} InWindow={GlitchState.IsInExploitWindow}]";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}
