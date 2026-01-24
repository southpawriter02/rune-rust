// ------------------------------------------------------------------------------
// <copyright file="GlitchState.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Captures the current state of a glitch cycle for a [Glitched] mechanism,
// including whether the pattern has been identified and the current phase.
// Part of v0.15.4f Glitch Exploitation System implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Captures the current state of a glitch cycle for a [Glitched] mechanism,
/// including whether the pattern has been identified and the current phase.
/// </summary>
/// <remarks>
/// <para>
/// Glitched mechanisms cycle through phases: Stable → Unstable → Permissive → Lockdown.
/// Characters who observe the pattern (WITS DC 14) can time their actions to exploit
/// the Permissive phase for a -4 DC bonus. Those who don't observe must roll on the
/// Chaos table for a random modifier (+4, +0, or -2).
/// </para>
/// <para>
/// DC Modifier calculation:
/// <list type="bullet">
///   <item><description>Pattern identified + in Permissive phase: -4 DC</description></item>
///   <item><description>Pattern identified + not in Permissive: +0 DC</description></item>
///   <item><description>Pattern not identified: Uses Chaos roll (+4, +0, or -2)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="GlitchCycleIdentified">True if character has successfully observed the pattern.</param>
/// <param name="CyclePhase">Current phase of the glitch cycle.</param>
/// <param name="ExploitWindow">The phase during which exploitation grants -4 DC.</param>
/// <param name="ChaosRoll">The d6 chaos roll result (null if pattern was identified).</param>
/// <param name="PhaseDuration">How many rounds the current phase lasts.</param>
/// <param name="RoundsInCurrentPhase">How many rounds have passed in current phase.</param>
public readonly record struct GlitchState(
    bool GlitchCycleIdentified,
    GlitchCyclePhase CyclePhase,
    GlitchCyclePhase ExploitWindow,
    int? ChaosRoll,
    int PhaseDuration,
    int RoundsInCurrentPhase)
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// The default exploit window phase (Permissive grants -4 DC).
    /// </summary>
    public static readonly GlitchCyclePhase DefaultExploitWindow = GlitchCyclePhase.Permissive;

    /// <summary>
    /// DC bonus when in the exploit window (-4).
    /// </summary>
    private const int ExploitWindowDcModifier = -4;

    /// <summary>
    /// DC penalty when in Lockdown phase (+2).
    /// </summary>
    private const int LockdownDcModifier = 2;

    /// <summary>
    /// Default phase duration when not specified (2 rounds).
    /// </summary>
    private const int DefaultPhaseDuration = 2;

    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets whether the character is currently in the exploit window.
    /// </summary>
    /// <remarks>
    /// True only when the pattern is identified AND the current phase matches the exploit window.
    /// If the pattern is not identified, this is always false (the character doesn't know when to act).
    /// </remarks>
    public bool IsInExploitWindow => GlitchCycleIdentified && CyclePhase == ExploitWindow;

    /// <summary>
    /// Gets the DC modifier based on current glitch state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DC Modifier calculation:
    /// <list type="bullet">
    ///   <item><description>Pattern identified + in exploit window: -4 DC</description></item>
    ///   <item><description>Pattern identified + outside window: +0 DC</description></item>
    ///   <item><description>Pattern not identified + Chaos roll 1-2: +4 DC</description></item>
    ///   <item><description>Pattern not identified + Chaos roll 3-4: +0 DC</description></item>
    ///   <item><description>Pattern not identified + Chaos roll 5-6: -2 DC</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int DcModifier
    {
        get
        {
            if (GlitchCycleIdentified)
            {
                // Pattern is identified - modifier depends on current phase
                return IsInExploitWindow ? ExploitWindowDcModifier : 0;
            }

            // Pattern not identified - use chaos roll
            return ChaosRoll switch
            {
                1 or 2 => 4,   // Glitch works against you
                3 or 4 => 0,   // Neutral effect
                5 or 6 => -2,  // Glitch helps
                _ => 0         // Fallback (shouldn't happen)
            };
        }
    }

    /// <summary>
    /// Gets a narrative description of the current glitch state.
    /// </summary>
    /// <remarks>
    /// Provides flavor text appropriate for the current phase.
    /// </remarks>
    public string StateDescription => CyclePhase switch
    {
        GlitchCyclePhase.Stable => "The mechanism operates with its usual erratic hum.",
        GlitchCyclePhase.Unstable => "Lights flicker and strange sounds emanate from within.",
        GlitchCyclePhase.Permissive => "The mechanism's defenses momentarily lapse!",
        GlitchCyclePhase.Lockdown => "The system compensates, becoming more resistant.",
        _ => "The glitch state is indeterminate."
    };

    /// <summary>
    /// Gets how many rounds until the next phase transition.
    /// </summary>
    public int RoundsUntilPhaseChange => Math.Max(0, PhaseDuration - RoundsInCurrentPhase);

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates an initial glitch state for an unobserved mechanism.
    /// </summary>
    /// <param name="initialPhase">The starting phase of the cycle.</param>
    /// <param name="phaseDuration">Duration of each phase in rounds (default 2).</param>
    /// <returns>A new unobserved GlitchState.</returns>
    /// <remarks>
    /// Use this factory when creating a glitch state before the character has
    /// attempted to observe the pattern. No chaos roll is assigned yet.
    /// </remarks>
    public static GlitchState CreateUnobserved(
        GlitchCyclePhase initialPhase = GlitchCyclePhase.Stable,
        int phaseDuration = DefaultPhaseDuration)
    {
        return new GlitchState(
            GlitchCycleIdentified: false,
            CyclePhase: initialPhase,
            ExploitWindow: DefaultExploitWindow,
            ChaosRoll: null,
            PhaseDuration: phaseDuration,
            RoundsInCurrentPhase: 0);
    }

    /// <summary>
    /// Creates a glitch state after successful pattern observation.
    /// </summary>
    /// <param name="currentPhase">The observed current phase.</param>
    /// <param name="exploitWindow">The identified exploit window phase.</param>
    /// <param name="phaseDuration">Duration of each phase in rounds.</param>
    /// <returns>A new observed GlitchState.</returns>
    /// <remarks>
    /// Use this factory after a character successfully observes the pattern (WITS DC 14).
    /// The character now knows when the exploit window occurs and can time their actions.
    /// </remarks>
    public static GlitchState CreateObserved(
        GlitchCyclePhase currentPhase,
        GlitchCyclePhase exploitWindow,
        int phaseDuration)
    {
        return new GlitchState(
            GlitchCycleIdentified: true,
            CyclePhase: currentPhase,
            ExploitWindow: exploitWindow,
            ChaosRoll: null,
            PhaseDuration: phaseDuration,
            RoundsInCurrentPhase: 0);
    }

    /// <summary>
    /// Creates a glitch state with a chaos roll (pattern not identified).
    /// </summary>
    /// <param name="chaosRoll">The d6 chaos roll result (1-6).</param>
    /// <param name="currentPhase">The current phase (unknown to character).</param>
    /// <returns>A new GlitchState with chaos modifier.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when chaosRoll is not 1-6.</exception>
    /// <remarks>
    /// Use this factory when a character proceeds without observing the pattern.
    /// The chaos roll determines the DC modifier: 1-2 = +4, 3-4 = 0, 5-6 = -2.
    /// </remarks>
    public static GlitchState CreateWithChaos(int chaosRoll, GlitchCyclePhase currentPhase)
    {
        if (chaosRoll < 1 || chaosRoll > 6)
        {
            throw new ArgumentOutOfRangeException(
                nameof(chaosRoll),
                chaosRoll,
                "Chaos roll must be between 1 and 6 (d6 result).");
        }

        return new GlitchState(
            GlitchCycleIdentified: false,
            CyclePhase: currentPhase,
            ExploitWindow: DefaultExploitWindow,
            ChaosRoll: chaosRoll,
            PhaseDuration: 0,
            RoundsInCurrentPhase: 0);
    }

    // -------------------------------------------------------------------------
    // Instance Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Advances to the next phase in the cycle.
    /// </summary>
    /// <returns>A new GlitchState in the next phase.</returns>
    /// <remarks>
    /// Phase order: Stable → Unstable → Permissive → Lockdown → Stable (repeat).
    /// The round counter is reset to 0 when advancing phases.
    /// </remarks>
    public GlitchState AdvancePhase()
    {
        var nextPhase = CyclePhase switch
        {
            GlitchCyclePhase.Stable => GlitchCyclePhase.Unstable,
            GlitchCyclePhase.Unstable => GlitchCyclePhase.Permissive,
            GlitchCyclePhase.Permissive => GlitchCyclePhase.Lockdown,
            GlitchCyclePhase.Lockdown => GlitchCyclePhase.Stable,
            _ => GlitchCyclePhase.Stable
        };

        return this with
        {
            CyclePhase = nextPhase,
            RoundsInCurrentPhase = 0
        };
    }

    /// <summary>
    /// Advances the round counter within the current phase.
    /// </summary>
    /// <returns>A new GlitchState with incremented round count, or advanced phase if duration exceeded.</returns>
    /// <remarks>
    /// If the round counter reaches or exceeds the phase duration, the phase automatically advances.
    /// This simulates the continuous cycling of glitched mechanism behavior.
    /// </remarks>
    public GlitchState AdvanceRound()
    {
        if (RoundsInCurrentPhase + 1 >= PhaseDuration)
        {
            return AdvancePhase();
        }

        return this with { RoundsInCurrentPhase = RoundsInCurrentPhase + 1 };
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a formatted display string for the glitch state.
    /// </summary>
    /// <returns>A human-readable summary of the glitch state.</returns>
    /// <remarks>
    /// Includes pattern status, current phase, exploit window timing, and DC modifier.
    /// </remarks>
    public string ToDisplayString()
    {
        var lines = new List<string>();

        if (GlitchCycleIdentified)
        {
            lines.Add("Pattern: IDENTIFIED");
            lines.Add($"Current Phase: {CyclePhase}");
            lines.Add($"Exploit Window: {ExploitWindow}");

            if (IsInExploitWindow)
            {
                lines.Add($"*** IN EXPLOIT WINDOW ({ExploitWindowDcModifier} DC) ***");
            }
            else
            {
                lines.Add($"Rounds until {ExploitWindow}: ~{EstimateRoundsToExploitWindow()}");
            }
        }
        else if (ChaosRoll.HasValue)
        {
            lines.Add($"Pattern: UNKNOWN (Chaos Roll: {ChaosRoll})");
            lines.Add($"Effect: {GetChaosDescription()}");
        }
        else
        {
            lines.Add("Pattern: UNOBSERVED");
            lines.Add("Observe pattern (WITS DC 14) or proceed blind.");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"GlitchState[Identified={GlitchCycleIdentified} Phase={CyclePhase} " +
               $"InWindow={IsInExploitWindow} Chaos={ChaosRoll} Modifier={DcModifier}]";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }

    // -------------------------------------------------------------------------
    // Private Helper Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Estimates rounds until the exploit window based on current phase.
    /// </summary>
    /// <returns>Estimated rounds until the Permissive phase.</returns>
    private int EstimateRoundsToExploitWindow()
    {
        var phases = new[]
        {
            GlitchCyclePhase.Stable,
            GlitchCyclePhase.Unstable,
            GlitchCyclePhase.Permissive,
            GlitchCyclePhase.Lockdown
        };

        var currentIndex = Array.IndexOf(phases, CyclePhase);
        var targetIndex = Array.IndexOf(phases, ExploitWindow);

        // Calculate phases to wait (wrapping around the cycle)
        var phasesToWait = (targetIndex - currentIndex + 4) % 4;

        // Estimate based on phase duration and remaining rounds in current phase
        return (phasesToWait * PhaseDuration) - RoundsInCurrentPhase;
    }

    /// <summary>
    /// Gets a description of the chaos roll effect.
    /// </summary>
    /// <returns>Description of the chaos effect.</returns>
    private string GetChaosDescription() => ChaosRoll switch
    {
        1 or 2 => "Glitch works AGAINST you (+4 DC)",
        3 or 4 => "Neutral effect (+0 DC)",
        5 or 6 => "Glitch HELPS you (-2 DC)",
        _ => "Unknown effect"
    };
}
