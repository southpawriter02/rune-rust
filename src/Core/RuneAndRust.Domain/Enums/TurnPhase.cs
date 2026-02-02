// ═══════════════════════════════════════════════════════════════════════════════
// TurnPhase.cs
// Identifies whether turn processing is occurring at the start or end of a turn.
// Used by the UnifiedTurnHandler to differentiate processing phases.
// Version: 0.18.5d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Identifies the phase of turn processing.
/// </summary>
/// <remarks>
/// <para>
/// Turn-based effects are processed at different phases of a character's turn:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Start:</strong> Resource decay, Apotheosis stress cost,
///       auto-exit checks.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>End:</strong> Panic table checks, CPS effects, environmental
///       stress, trauma triggers.
///     </description>
///   </item>
/// </list>
/// </remarks>
/// <seealso cref="RuneAndRust.Domain.ValueObjects.TurnIntegrationResult"/>
public enum TurnPhase
{
    /// <summary>
    /// Processing occurs at the start of the character's turn.
    /// </summary>
    /// <remarks>
    /// Start-of-turn effects include:
    /// <list type="bullet">
    ///   <item><description>Resource decay (Rage, Momentum) when out of combat</description></item>
    ///   <item><description>Apotheosis stress cost (10/turn)</description></item>
    ///   <item><description>Apotheosis auto-exit check (stress >= 100)</description></item>
    /// </list>
    /// </remarks>
    Start = 0,

    /// <summary>
    /// Processing occurs at the end of the character's turn.
    /// </summary>
    /// <remarks>
    /// End-of-turn effects include:
    /// <list type="bullet">
    ///   <item><description>Panic table check (RuinMadness stage)</description></item>
    ///   <item><description>CPS stage effects</description></item>
    ///   <item><description>Environmental stress application</description></item>
    ///   <item><description>Trauma trigger checks</description></item>
    /// </list>
    /// </remarks>
    End = 1
}
