// ------------------------------------------------------------------------------
// <copyright file="TrapDisarmResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// The complete result of a trap disarmament attempt, including all
// mechanical outcomes and consequences.
// Part of v0.15.4d Trap Disarmament System implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// The complete result of a trap disarmament attempt, including all
/// mechanical outcomes and consequences.
/// </summary>
/// <remarks>
/// <para>
/// This value object captures every outcome detail from a disarmament attempt:
/// <list type="bullet">
///   <item><description>Step completed and success status</description></item>
///   <item><description>Damage and damage type (if trap triggered)</description></item>
///   <item><description>Alert and lockdown status</description></item>
///   <item><description>Salvaged components (on critical success)</description></item>
///   <item><description>Narrative description for display</description></item>
/// </list>
/// </para>
/// <para>
/// Use the static factory methods to create appropriately-configured results
/// for each step and outcome combination.
/// </para>
/// </remarks>
/// <param name="State">The updated disarmament state.</param>
/// <param name="Step">The step that was attempted.</param>
/// <param name="Success">Whether the attempt succeeded.</param>
/// <param name="IsCritical">Whether it was a critical success (net ≥ 5).</param>
/// <param name="IsFumble">Whether it was a fumble (0 successes + botch).</param>
/// <param name="NetSuccesses">The net successes from the roll.</param>
/// <param name="EffectiveDc">The DC the roll was against.</param>
/// <param name="DamageDealt">Damage dealt if trap triggered.</param>
/// <param name="DamageType">Type of damage dealt (e.g., "physical", "lightning", "none").</param>
/// <param name="AlertTriggered">Whether an alarm was triggered.</param>
/// <param name="LockdownTriggered">Whether a lockdown was triggered.</param>
/// <param name="SalvagedComponents">Components salvaged on critical success.</param>
/// <param name="NarrativeDescription">Flavor text for the outcome.</param>
public readonly record struct TrapDisarmResult(
    TrapDisarmState State,
    DisarmStep Step,
    bool Success,
    bool IsCritical,
    bool IsFumble,
    int NetSuccesses,
    int EffectiveDc,
    int DamageDealt,
    string DamageType,
    bool AlertTriggered,
    bool LockdownTriggered,
    IReadOnlyList<string> SalvagedComponents,
    string NarrativeDescription)
{
    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Whether the trap was triggered (detection failure or disarmament fumble).
    /// </summary>
    /// <remarks>
    /// Traps trigger on:
    /// <list type="bullet">
    ///   <item><description>Failed detection (walked into trap)</description></item>
    ///   <item><description>Fumbled disarmament ([Forced Execution])</description></item>
    /// </list>
    /// </remarks>
    public bool TrapTriggered => (Step == DisarmStep.Detection && !Success) || IsFumble;

    /// <summary>
    /// Whether the trap is now neutralized (either disarmed or triggered).
    /// </summary>
    /// <remarks>
    /// A neutralized trap is no longer a threat—either successfully disabled
    /// or already triggered and spent.
    /// </remarks>
    public bool TrapNeutralized => (Step == DisarmStep.Disarmament && Success) || TrapTriggered;

    /// <summary>
    /// Whether salvage was obtained.
    /// </summary>
    /// <remarks>
    /// Salvage is only obtained on critical success during disarmament.
    /// </remarks>
    public bool HasSalvage => SalvagedComponents.Count > 0;

    /// <summary>
    /// Whether damage was dealt.
    /// </summary>
    public bool HasDamage => DamageDealt > 0;

    // -------------------------------------------------------------------------
    // Detection Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a successful detection result.
    /// </summary>
    /// <param name="state">The updated disarmament state.</param>
    /// <param name="netSuccesses">The net successes from the roll.</param>
    /// <param name="dc">The detection DC.</param>
    /// <returns>A TrapDisarmResult for successful trap detection.</returns>
    /// <remarks>
    /// On successful detection:
    /// <list type="bullet">
    ///   <item><description>Trap found before triggering</description></item>
    ///   <item><description>Character may proceed to Analysis or Disarmament</description></item>
    /// </list>
    /// </remarks>
    public static TrapDisarmResult DetectionSuccess(TrapDisarmState state, int netSuccesses, int dc)
    {
        return new TrapDisarmResult(
            State: state,
            Step: DisarmStep.Detection,
            Success: true,
            IsCritical: netSuccesses >= 5,
            IsFumble: false,
            NetSuccesses: netSuccesses,
            EffectiveDc: dc,
            DamageDealt: 0,
            DamageType: "none",
            AlertTriggered: false,
            LockdownTriggered: false,
            SalvagedComponents: Array.Empty<string>(),
            NarrativeDescription: "You spot the trap before it spots you. The mechanism " +
                                  "lies dormant, waiting for its next victim.");
    }

    /// <summary>
    /// Creates a failed detection result (trap triggers).
    /// </summary>
    /// <param name="state">The updated disarmament state.</param>
    /// <param name="netSuccesses">The net successes from the roll.</param>
    /// <param name="dc">The detection DC.</param>
    /// <param name="damage">Damage dealt by the trap.</param>
    /// <param name="damageType">Type of damage dealt.</param>
    /// <param name="alert">Whether an alert was triggered.</param>
    /// <param name="lockdown">Whether a lockdown was triggered.</param>
    /// <param name="narrative">Narrative description of the trap trigger.</param>
    /// <returns>A TrapDisarmResult for failed trap detection.</returns>
    /// <remarks>
    /// On failed detection:
    /// <list type="bullet">
    ///   <item><description>Character walks into the trap</description></item>
    ///   <item><description>Trap effects applied immediately</description></item>
    ///   <item><description>Trap is spent (no longer a threat)</description></item>
    /// </list>
    /// </remarks>
    public static TrapDisarmResult DetectionFailure(
        TrapDisarmState state,
        int netSuccesses,
        int dc,
        int damage,
        string damageType,
        bool alert,
        bool lockdown,
        string narrative)
    {
        return new TrapDisarmResult(
            State: state,
            Step: DisarmStep.Detection,
            Success: false,
            IsCritical: false,
            IsFumble: false,
            NetSuccesses: netSuccesses,
            EffectiveDc: dc,
            DamageDealt: damage,
            DamageType: damageType,
            AlertTriggered: alert,
            LockdownTriggered: lockdown,
            SalvagedComponents: Array.Empty<string>(),
            NarrativeDescription: narrative);
    }

    // -------------------------------------------------------------------------
    // Disarmament Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a successful disarmament result.
    /// </summary>
    /// <param name="state">The updated disarmament state.</param>
    /// <param name="netSuccesses">The net successes from the roll.</param>
    /// <param name="dc">The effective disarm DC.</param>
    /// <param name="isCritical">Whether this was a critical success.</param>
    /// <param name="salvage">Components salvaged (if critical).</param>
    /// <param name="narrative">Narrative description of the success.</param>
    /// <returns>A TrapDisarmResult for successful disarmament.</returns>
    /// <remarks>
    /// On successful disarmament:
    /// <list type="bullet">
    ///   <item><description>Trap disabled safely</description></item>
    ///   <item><description>Critical success yields salvage components</description></item>
    /// </list>
    /// </remarks>
    public static TrapDisarmResult DisarmSuccess(
        TrapDisarmState state,
        int netSuccesses,
        int dc,
        bool isCritical,
        IReadOnlyList<string> salvage,
        string narrative)
    {
        return new TrapDisarmResult(
            State: state,
            Step: DisarmStep.Disarmament,
            Success: true,
            IsCritical: isCritical,
            IsFumble: false,
            NetSuccesses: netSuccesses,
            EffectiveDc: dc,
            DamageDealt: 0,
            DamageType: "none",
            AlertTriggered: false,
            LockdownTriggered: false,
            SalvagedComponents: salvage,
            NarrativeDescription: narrative);
    }

    /// <summary>
    /// Creates a failed disarmament result (DC escalation).
    /// </summary>
    /// <param name="state">The updated disarmament state.</param>
    /// <param name="netSuccesses">The net successes from the roll.</param>
    /// <param name="dc">The effective disarm DC.</param>
    /// <returns>A TrapDisarmResult for failed disarmament.</returns>
    /// <remarks>
    /// On failed disarmament:
    /// <list type="bullet">
    ///   <item><description>Trap remains active</description></item>
    ///   <item><description>DC increases by +1 for next attempt</description></item>
    ///   <item><description>Character may retry</description></item>
    /// </list>
    /// </remarks>
    public static TrapDisarmResult DisarmFailure(TrapDisarmState state, int netSuccesses, int dc)
    {
        return new TrapDisarmResult(
            State: state,
            Step: DisarmStep.Disarmament,
            Success: false,
            IsCritical: false,
            IsFumble: false,
            NetSuccesses: netSuccesses,
            EffectiveDc: dc,
            DamageDealt: 0,
            DamageType: "none",
            AlertTriggered: false,
            LockdownTriggered: false,
            SalvagedComponents: Array.Empty<string>(),
            NarrativeDescription: "The mechanism resists your efforts. You'll need to try " +
                                  "a different approach—and it won't be any easier.");
    }

    /// <summary>
    /// Creates a fumble result ([Forced Execution]).
    /// </summary>
    /// <param name="state">The updated disarmament state.</param>
    /// <param name="netSuccesses">The net successes from the roll.</param>
    /// <param name="dc">The effective disarm DC.</param>
    /// <param name="damage">Damage dealt by the trap.</param>
    /// <param name="damageType">Type of damage dealt.</param>
    /// <param name="alert">Whether an alert was triggered.</param>
    /// <param name="lockdown">Whether a lockdown was triggered.</param>
    /// <param name="narrative">Narrative description of the fumble.</param>
    /// <returns>A TrapDisarmResult for fumbled disarmament.</returns>
    /// <remarks>
    /// On fumble ([Forced Execution]):
    /// <list type="bullet">
    ///   <item><description>Trap triggers on the disarmer</description></item>
    ///   <item><description>Full trap effects applied</description></item>
    ///   <item><description>Trap is destroyed (no salvage possible)</description></item>
    /// </list>
    /// </remarks>
    public static TrapDisarmResult Fumble(
        TrapDisarmState state,
        int netSuccesses,
        int dc,
        int damage,
        string damageType,
        bool alert,
        bool lockdown,
        string narrative)
    {
        return new TrapDisarmResult(
            State: state,
            Step: DisarmStep.Disarmament,
            Success: false,
            IsCritical: false,
            IsFumble: true,
            NetSuccesses: netSuccesses,
            EffectiveDc: dc,
            DamageDealt: damage,
            DamageType: damageType,
            AlertTriggered: alert,
            LockdownTriggered: lockdown,
            SalvagedComponents: Array.Empty<string>(),
            NarrativeDescription: narrative);
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a formatted display string for the result.
    /// </summary>
    /// <returns>A human-readable summary of the disarmament result.</returns>
    public string ToDisplayString()
    {
        var outcomeStr = Success ? "SUCCESS" : (IsFumble ? "FUMBLE" : "FAILURE");
        var consequences = new List<string>();

        if (HasDamage)
        {
            consequences.Add($"{DamageDealt} {DamageType} damage");
        }

        if (AlertTriggered)
        {
            consequences.Add("alert triggered");
        }

        if (LockdownTriggered)
        {
            consequences.Add("lockdown triggered");
        }

        if (HasSalvage)
        {
            consequences.Add($"salvaged {SalvagedComponents.Count} components");
        }

        var consequenceStr = consequences.Count > 0
            ? $" [{string.Join(", ", consequences)}]"
            : "";

        return $"Trap {Step}: {outcomeStr} (Roll {NetSuccesses} vs DC {EffectiveDc}){consequenceStr}";
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"TrapDisarmResult[{State.DisarmId[..Math.Min(8, State.DisarmId.Length)]}] " +
               $"Step={Step} Success={Success} Fumble={IsFumble} " +
               $"Net={NetSuccesses} DC={EffectiveDc} Dmg={DamageDealt}";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}
