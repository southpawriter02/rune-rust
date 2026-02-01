// ═══════════════════════════════════════════════════════════════════════════════
// CpsRecoveryProtocol.cs
// Represents the recovery protocol for a CPS stage. Each CPS stage (except
// HollowShell) has a defined recovery protocol that players can follow to
// reduce their character's stress and potentially recover to a lower stage.
// This provides both narrative context and actionable steps for players.
// Version: 0.18.2b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the recovery protocol for a CPS stage.
/// </summary>
/// <remarks>
/// <para>
/// Each CPS stage (except HollowShell) has a defined recovery protocol
/// that players can follow to reduce their character's stress and
/// potentially recover to a lower stage.
/// </para>
/// <para>
/// Recovery Urgency Levels:
/// <list type="bullet">
/// <item>None: No immediate action needed</item>
/// <item>Low: Recovery recommended when convenient</item>
/// <item>Moderate: Recovery should be prioritized</item>
/// <item>Critical: Immediate recovery required</item>
/// <item>Terminal: Recovery may be impossible</item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Stage">The CPS stage this protocol applies to.</param>
/// <param name="IsRecoverable">Whether recovery is possible from this stage.</param>
/// <param name="ProtocolName">Name of the recovery protocol.</param>
/// <param name="Steps">Ordered list of recovery steps.</param>
/// <param name="RecoveryTime">Estimated time for full recovery.</param>
/// <param name="Urgency">Urgency level for seeking recovery.</param>
/// <seealso cref="CpsStage"/>
/// <seealso cref="CpsState"/>
public readonly record struct CpsRecoveryProtocol(
    CpsStage Stage,
    bool IsRecoverable,
    string ProtocolName,
    IReadOnlyList<string> Steps,
    string RecoveryTime,
    string Urgency)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ARROW-EXPRESSION PROPERTIES — Protocol Indicators
    // ═══════════════════════════════════════════════════════════════════════════

    #region Arrow-Expression Properties

    /// <summary>
    /// Gets whether this is a terminal state with no recovery.
    /// </summary>
    /// <value>
    /// <c>true</c> if IsRecoverable is false; otherwise, <c>false</c>.
    /// </value>
    public bool IsTerminal => !IsRecoverable;

    /// <summary>
    /// Gets the number of recovery steps.
    /// </summary>
    /// <value>
    /// The count of steps in the recovery protocol.
    /// </value>
    public int StepCount => Steps.Count;

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS — Stage-Specific Protocols
    // ═══════════════════════════════════════════════════════════════════════════

    #region Factory Methods

    /// <summary>
    /// Creates recovery protocol for the None stage.
    /// </summary>
    /// <returns>A CpsRecoveryProtocol indicating no recovery needed.</returns>
    /// <remarks>
    /// Clear-minded state. Mind is functioning normally, no protocol required.
    /// </remarks>
    public static CpsRecoveryProtocol ForNone() => new(
        Stage: CpsStage.None,
        IsRecoverable: true,
        ProtocolName: "Standard Mental Health",
        Steps: new[] { "No recovery needed — mind is clear" },
        RecoveryTime: "N/A",
        Urgency: "None");

    /// <summary>
    /// Creates recovery protocol for the WeightOfKnowing stage.
    /// </summary>
    /// <returns>A CpsRecoveryProtocol for mild CPS symptoms.</returns>
    /// <remarks>
    /// <para>
    /// Cognitive Hygiene protocol focuses on rest and avoiding paradox
    /// sources. Recovery is relatively quick (12-24 hours).
    /// </para>
    /// <para>
    /// Low urgency — character can continue adventuring but should
    /// address symptoms when convenient.
    /// </para>
    /// </remarks>
    public static CpsRecoveryProtocol ForWeightOfKnowing() => new(
        Stage: CpsStage.WeightOfKnowing,
        IsRecoverable: true,
        ProtocolName: "Cognitive Hygiene",
        Steps: new[]
        {
            "Find a safe location away from anomalies",
            "Rest in silence and darkness for at least 4 hours",
            "Avoid exposure to paradox sources",
            "Engage in grounding activities (routine tasks)",
            "Reduce stress through normal rest mechanics"
        },
        RecoveryTime: "12-24 hours",
        Urgency: "Low");

    /// <summary>
    /// Creates recovery protocol for the GlimmerMadness stage.
    /// </summary>
    /// <returns>A CpsRecoveryProtocol for moderate CPS symptoms.</returns>
    /// <remarks>
    /// <para>
    /// Silent Room Protocol requires more intensive isolation and extended
    /// rest. Written materials must be removed due to The Writhing Text symptom.
    /// </para>
    /// <para>
    /// Critical urgency — continuing to adventure risks progression to
    /// RuinMadness, which triggers Panic Table rolls.
    /// </para>
    /// </remarks>
    public static CpsRecoveryProtocol ForGlimmerMadness() => new(
        Stage: CpsStage.GlimmerMadness,
        IsRecoverable: true,
        ProtocolName: "Silent Room Protocol",
        Steps: new[]
        {
            "Immediately cease all paradox exposure",
            "Isolate in a dark, quiet room",
            "Remove all written materials (The Writhing Text symptom)",
            "Do not attempt complex reasoning or logic puzzles",
            "Have an ally monitor for deterioration",
            "Long Rest required — Short Rest insufficient",
            "Sanctuary Rest recommended if available"
        },
        RecoveryTime: "48+ hours",
        Urgency: "Critical");

    /// <summary>
    /// Creates recovery protocol for the RuinMadness stage.
    /// </summary>
    /// <returns>A CpsRecoveryProtocol indicating limited recovery options.</returns>
    /// <remarks>
    /// <para>
    /// Terminal Protocol — the mind has crossed a threshold from which
    /// normal recovery is not possible. The character is functionally
    /// broken at this stage.
    /// </para>
    /// <para>
    /// GM discretion allows Sanctuary Rest to potentially reduce to
    /// GlimmerMadness, but this is not guaranteed. Character retirement
    /// may be necessary.
    /// </para>
    /// </remarks>
    public static CpsRecoveryProtocol ForRuinMadness() => new(
        Stage: CpsStage.RuinMadness,
        IsRecoverable: false,
        ProtocolName: "Terminal Protocol",
        Steps: new[]
        {
            "WARNING: Recovery from Ruin-Madness is not possible",
            "The mind has crossed a threshold from which there is no return",
            "Focus on managing symptoms and preventing HollowShell",
            "Sanctuary Rest may reduce to GlimmerMadness (GM discretion)",
            "Character may need to be retired if condition persists"
        },
        RecoveryTime: "N/A",
        Urgency: "Terminal");

    /// <summary>
    /// Creates recovery protocol for the HollowShell stage.
    /// </summary>
    /// <returns>A CpsRecoveryProtocol indicating character loss.</returns>
    /// <remarks>
    /// <para>
    /// The character's mind has been erased. There is no recovery protocol.
    /// The character becomes an unplayable NPC and the player must create
    /// a new character if they wish to continue.
    /// </para>
    /// </remarks>
    public static CpsRecoveryProtocol ForHollowShell() => new(
        Stage: CpsStage.HollowShell,
        IsRecoverable: false,
        ProtocolName: "None — Character Lost",
        Steps: new[]
        {
            "The character's mind has been erased",
            "No recovery is possible",
            "Character becomes unplayable NPC",
            "Begin new character creation if desired"
        },
        RecoveryTime: "N/A",
        Urgency: "Terminal");

    /// <summary>
    /// Creates recovery protocol for a given CPS stage.
    /// </summary>
    /// <param name="stage">The CPS stage.</param>
    /// <returns>A CpsRecoveryProtocol appropriate for the specified stage.</returns>
    /// <remarks>
    /// Dispatcher method that returns the appropriate protocol for each stage.
    /// Unknown stages default to ForNone().
    /// </remarks>
    public static CpsRecoveryProtocol ForStage(CpsStage stage) => stage switch
    {
        CpsStage.None => ForNone(),
        CpsStage.WeightOfKnowing => ForWeightOfKnowing(),
        CpsStage.GlimmerMadness => ForGlimmerMadness(),
        CpsStage.RuinMadness => ForRuinMadness(),
        CpsStage.HollowShell => ForHollowShell(),
        _ => ForNone()
    };

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY — String Representation
    // ═══════════════════════════════════════════════════════════════════════════

    #region Display

    /// <summary>
    /// Returns a string representation of the recovery protocol for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string showing stage, protocol name, urgency, and recoverability.
    /// </returns>
    /// <example>
    /// <code>
    /// var protocol = CpsRecoveryProtocol.ForGlimmerMadness();
    /// // Returns "CpsRecovery[GlimmerMadness]: Silent Room Protocol — Urgency=Critical, Recoverable=True"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"CpsRecovery[{Stage}]: {ProtocolName} — Urgency={Urgency}, Recoverable={IsRecoverable}";

    #endregion
}
