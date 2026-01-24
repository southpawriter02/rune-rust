// ------------------------------------------------------------------------------
// <copyright file="JuryRigResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// The complete result of a jury-rigging experiment attempt, including all
// mechanical outcomes and consequences.
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// The complete result of a jury-rigging experiment attempt, including all
/// mechanical outcomes and consequences.
/// </summary>
/// <remarks>
/// <para>
/// This value object captures every outcome detail from an experiment attempt:
/// <list type="bullet">
///   <item><description>Outcome category (success, critical, failure, fumble, etc.)</description></item>
///   <item><description>Net successes from the roll</description></item>
///   <item><description>Complication roll and effect (on failure)</description></item>
///   <item><description>Damage dealt and type (if applicable)</description></item>
///   <item><description>Salvaged components (on critical success or Brute Disassembly)</description></item>
///   <item><description>Narrative description for display</description></item>
/// </list>
/// </para>
/// <para>
/// Use the static factory methods to create appropriately-configured results
/// for each outcome type.
/// </para>
/// </remarks>
/// <param name="Outcome">The outcome category of the experiment.</param>
/// <param name="NetSuccesses">The net successes from the roll.</param>
/// <param name="EffectiveDc">The DC the roll was against.</param>
/// <param name="ComplicationRoll">The d10 complication roll, if failure occurred.</param>
/// <param name="ComplicationEffect">The complication effect, if failure occurred.</param>
/// <param name="DamageDealt">Damage dealt if sparks fly or electrocution.</param>
/// <param name="DamageType">Type of damage dealt (e.g., "electrical", "none").</param>
/// <param name="AlertTriggered">Whether an alarm was triggered (complication 2-3).</param>
/// <param name="SalvagedComponents">Components salvaged on critical success.</param>
/// <param name="NarrativeText">Flavor text for the outcome.</param>
/// <param name="MethodUsed">The bypass method that was attempted.</param>
public readonly record struct JuryRigResult(
    JuryRigOutcome Outcome,
    int NetSuccesses,
    int EffectiveDc,
    int? ComplicationRoll,
    ComplicationEffect? ComplicationEffect,
    int DamageDealt,
    string DamageType,
    bool AlertTriggered,
    IReadOnlyList<string> SalvagedComponents,
    string NarrativeText,
    BypassMethod MethodUsed)
{
    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether the bypass was successful.
    /// </summary>
    /// <remarks>
    /// True for Success, CriticalSuccess, and GlitchInFavor complication.
    /// </remarks>
    public bool IsSuccess => Outcome == JuryRigOutcome.Success ||
                             Outcome == JuryRigOutcome.CriticalSuccess ||
                             ComplicationEffect == Enums.ComplicationEffect.GlitchInFavor;

    /// <summary>
    /// Gets a value indicating whether the attempt was a critical success.
    /// </summary>
    /// <remarks>
    /// Critical success occurs when net successes >= 5, yielding salvage components.
    /// </remarks>
    public bool IsCritical => Outcome == JuryRigOutcome.CriticalSuccess;

    /// <summary>
    /// Gets a value indicating whether the attempt was a fumble.
    /// </summary>
    /// <remarks>
    /// Fumble occurs when the roll produces 0 successes and at least 1 botch,
    /// destroying the mechanism completely.
    /// </remarks>
    public bool IsFumble => Outcome == JuryRigOutcome.Fumble;

    /// <summary>
    /// Gets a value indicating whether salvage was obtained.
    /// </summary>
    /// <remarks>
    /// Salvage is obtained on critical success (net >= 5) or successful
    /// Brute Disassembly (which guarantees salvage but destroys the mechanism).
    /// </remarks>
    public bool HasSalvage => SalvagedComponents.Count > 0;

    /// <summary>
    /// Gets a value indicating whether damage was taken.
    /// </summary>
    /// <remarks>
    /// Damage sources:
    /// <list type="bullet">
    ///   <item><description>Electrocution (Wire Manipulation failed save): 2d10 lightning</description></item>
    ///   <item><description>Sparks Fly complication (d10 = 4-5): 1d6 electrical</description></item>
    /// </list>
    /// </remarks>
    public bool TookDamage => DamageDealt > 0;

    /// <summary>
    /// Gets a value indicating whether the character can retry the bypass.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Retry is possible unless a terminal outcome occurred:
    /// <list type="bullet">
    ///   <item><description>Fumble: Mechanism destroyed</description></item>
    ///   <item><description>PermanentLock: Mechanism locked permanently</description></item>
    ///   <item><description>Success/CriticalSuccess: Already bypassed</description></item>
    ///   <item><description>GlitchInFavor: Already bypassed</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public bool CanRetry => !IsSuccess &&
                            Outcome != JuryRigOutcome.Fumble &&
                            Outcome != JuryRigOutcome.PermanentLock;

    /// <summary>
    /// Gets a value indicating whether the mechanism was destroyed.
    /// </summary>
    /// <remarks>
    /// Destruction occurs on fumble (accidental) or successful Brute Disassembly (intentional).
    /// Only Brute Disassembly yields salvage; fumble destruction yields nothing.
    /// </remarks>
    public bool MechanismDestroyed => IsFumble ||
                                      (IsSuccess && MethodUsed.DestroysMechanism());

    /// <summary>
    /// Gets a value indicating whether partial success was achieved.
    /// </summary>
    /// <remarks>
    /// Partial success (complication roll 8-9) grants access to one function
    /// of the mechanism, which may be sufficient for the character's purposes.
    /// </remarks>
    public bool IsPartialSuccess => Outcome == JuryRigOutcome.PartialSuccess;

    // -------------------------------------------------------------------------
    // Factory Methods - Success Outcomes
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a standard success result.
    /// </summary>
    /// <param name="netSuccesses">The net successes from the roll.</param>
    /// <param name="dc">The effective DC.</param>
    /// <param name="method">The bypass method used.</param>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>A JuryRigResult for standard success.</returns>
    /// <remarks>
    /// Standard success: net successes > 0 but &lt; 5.
    /// Mechanism is bypassed but no salvage is obtained (unless Brute Disassembly).
    /// </remarks>
    public static JuryRigResult Success(
        int netSuccesses,
        int dc,
        BypassMethod method,
        string? narrative = null)
    {
        return new JuryRigResult(
            Outcome: JuryRigOutcome.Success,
            NetSuccesses: netSuccesses,
            EffectiveDc: dc,
            ComplicationRoll: null,
            ComplicationEffect: null,
            DamageDealt: 0,
            DamageType: "none",
            AlertTriggered: false,
            SalvagedComponents: Array.Empty<string>(),
            NarrativeText: narrative ?? "The mechanism responds to your manipulation. " +
                                        "With a satisfying click, access is granted.",
            MethodUsed: method);
    }

    /// <summary>
    /// Creates a critical success result with salvage.
    /// </summary>
    /// <param name="netSuccesses">The net successes from the roll (>= 5).</param>
    /// <param name="dc">The effective DC.</param>
    /// <param name="method">The bypass method used.</param>
    /// <param name="salvage">The salvaged components.</param>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>A JuryRigResult for critical success.</returns>
    /// <remarks>
    /// Critical success: net successes >= 5.
    /// In addition to bypassing the mechanism, salvageable components are identified.
    /// </remarks>
    public static JuryRigResult CriticalSuccess(
        int netSuccesses,
        int dc,
        BypassMethod method,
        IReadOnlyList<string> salvage,
        string? narrative = null)
    {
        return new JuryRigResult(
            Outcome: JuryRigOutcome.CriticalSuccess,
            NetSuccesses: netSuccesses,
            EffectiveDc: dc,
            ComplicationRoll: null,
            ComplicationEffect: null,
            DamageDealt: 0,
            DamageType: "none",
            AlertTriggered: false,
            SalvagedComponents: salvage,
            NarrativeText: narrative ?? "Your masterful touch not only bypasses the mechanism, " +
                                        "but reveals salvageable components within.",
            MethodUsed: method);
    }

    /// <summary>
    /// Creates a success result for Brute Disassembly (guaranteed salvage, mechanism destroyed).
    /// </summary>
    /// <param name="netSuccesses">The net successes from the roll.</param>
    /// <param name="dc">The effective DC.</param>
    /// <param name="salvage">The salvaged components.</param>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>A JuryRigResult for Brute Disassembly success.</returns>
    /// <remarks>
    /// Brute Disassembly always yields salvage but destroys the mechanism.
    /// The outcome is technically Success (not Critical) but with guaranteed salvage.
    /// </remarks>
    public static JuryRigResult BruteDisassemblySuccess(
        int netSuccesses,
        int dc,
        IReadOnlyList<string> salvage,
        string? narrative = null)
    {
        var outcome = netSuccesses >= 5
            ? JuryRigOutcome.CriticalSuccess
            : JuryRigOutcome.Success;

        return new JuryRigResult(
            Outcome: outcome,
            NetSuccesses: netSuccesses,
            EffectiveDc: dc,
            ComplicationRoll: null,
            ComplicationEffect: null,
            DamageDealt: 0,
            DamageType: "none",
            AlertTriggered: false,
            SalvagedComponents: salvage,
            NarrativeText: narrative ?? "You tear the mechanism apart piece by piece. " +
                                        "What remains is useless, but you've secured valuable components.",
            MethodUsed: BypassMethod.BruteDisassembly);
    }

    // -------------------------------------------------------------------------
    // Factory Methods - Failure Outcomes
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a failure result with complication.
    /// </summary>
    /// <param name="netSuccesses">The net successes from the roll.</param>
    /// <param name="dc">The effective DC.</param>
    /// <param name="method">The bypass method used.</param>
    /// <param name="complicationRoll">The d10 complication roll.</param>
    /// <param name="effect">The resulting complication effect.</param>
    /// <param name="damage">Damage dealt (for SparksFly).</param>
    /// <param name="alert">Whether an alarm was triggered.</param>
    /// <param name="narrative">Narrative text describing the failure.</param>
    /// <returns>A JuryRigResult for failure with complication.</returns>
    /// <remarks>
    /// Failure occurs when net successes &lt;= 0 without a fumble.
    /// The complication table determines the consequence.
    /// </remarks>
    public static JuryRigResult Failure(
        int netSuccesses,
        int dc,
        BypassMethod method,
        int complicationRoll,
        ComplicationEffect effect,
        int damage,
        bool alert,
        string narrative)
    {
        var outcome = effect switch
        {
            Enums.ComplicationEffect.PermanentLock => JuryRigOutcome.PermanentLock,
            Enums.ComplicationEffect.PartialSuccess => JuryRigOutcome.PartialSuccess,
            Enums.ComplicationEffect.GlitchInFavor => JuryRigOutcome.Success,
            _ => JuryRigOutcome.Failure
        };

        return new JuryRigResult(
            Outcome: outcome,
            NetSuccesses: netSuccesses,
            EffectiveDc: dc,
            ComplicationRoll: complicationRoll,
            ComplicationEffect: effect,
            DamageDealt: damage,
            DamageType: damage > 0 ? "electrical" : "none",
            AlertTriggered: alert,
            SalvagedComponents: Array.Empty<string>(),
            NarrativeText: narrative,
            MethodUsed: method);
    }

    /// <summary>
    /// Creates a partial success result (complication 8-9).
    /// </summary>
    /// <param name="netSuccesses">The net successes from the roll.</param>
    /// <param name="dc">The effective DC.</param>
    /// <param name="method">The bypass method used.</param>
    /// <param name="partialFunction">Description of the function that works.</param>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>A JuryRigResult for partial success.</returns>
    /// <remarks>
    /// Partial success grants access to one function of the mechanism.
    /// The character may continue attempting for full access.
    /// </remarks>
    public static JuryRigResult PartialSuccess(
        int netSuccesses,
        int dc,
        BypassMethod method,
        string partialFunction,
        string? narrative = null)
    {
        return new JuryRigResult(
            Outcome: JuryRigOutcome.PartialSuccess,
            NetSuccesses: netSuccesses,
            EffectiveDc: dc,
            ComplicationRoll: 8, // 8 or 9 triggers partial success
            ComplicationEffect: Enums.ComplicationEffect.PartialSuccess,
            DamageDealt: 0,
            DamageType: "none",
            AlertTriggered: false,
            SalvagedComponents: Array.Empty<string>(),
            NarrativeText: narrative ?? $"The mechanism partially responds. {partialFunction}",
            MethodUsed: method);
    }

    /// <summary>
    /// Creates a fumble result (mechanism destroyed).
    /// </summary>
    /// <param name="netSuccesses">The net successes from the roll (typically negative or 0).</param>
    /// <param name="dc">The effective DC.</param>
    /// <param name="method">The bypass method used.</param>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>A JuryRigResult for fumble.</returns>
    /// <remarks>
    /// <para>
    /// Fumble occurs when the roll produces 0 successes and at least 1 botch.
    /// The mechanism is destroyed beyond repair; no salvage is possible.
    /// </para>
    /// </remarks>
    public static JuryRigResult Fumble(
        int netSuccesses,
        int dc,
        BypassMethod method,
        string? narrative = null)
    {
        return new JuryRigResult(
            Outcome: JuryRigOutcome.Fumble,
            NetSuccesses: netSuccesses,
            EffectiveDc: dc,
            ComplicationRoll: null,
            ComplicationEffect: null,
            DamageDealt: 0,
            DamageType: "none",
            AlertTriggered: false,
            SalvagedComponents: Array.Empty<string>(),
            NarrativeText: narrative ?? "The mechanism sparks violently and falls silent. " +
                                        "Smoke rises from its ruined internals. It is beyond saving.",
            MethodUsed: method);
    }

    /// <summary>
    /// Creates a permanent lock result (complication 1).
    /// </summary>
    /// <param name="netSuccesses">The net successes from the roll.</param>
    /// <param name="dc">The effective DC.</param>
    /// <param name="method">The bypass method used.</param>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>A JuryRigResult for permanent lock.</returns>
    /// <remarks>
    /// Permanent lock occurs on complication roll of 1.
    /// No further jury-rigging attempts are possible.
    /// </remarks>
    public static JuryRigResult PermanentLock(
        int netSuccesses,
        int dc,
        BypassMethod method,
        string? narrative = null)
    {
        return new JuryRigResult(
            Outcome: JuryRigOutcome.PermanentLock,
            NetSuccesses: netSuccesses,
            EffectiveDc: dc,
            ComplicationRoll: 1,
            ComplicationEffect: Enums.ComplicationEffect.PermanentLock,
            DamageDealt: 0,
            DamageType: "none",
            AlertTriggered: false,
            SalvagedComponents: Array.Empty<string>(),
            NarrativeText: narrative ?? "A deep clunk echoes from within as internal bolts slide into place. " +
                                        "The mechanism has locked permanently—no amount of tinkering will open it now.",
            MethodUsed: method);
    }

    /// <summary>
    /// Creates a glitch-in-favor result (complication 10).
    /// </summary>
    /// <param name="netSuccesses">The net successes from the roll.</param>
    /// <param name="dc">The effective DC.</param>
    /// <param name="method">The bypass method used.</param>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>A JuryRigResult for glitch in favor (auto-success).</returns>
    /// <remarks>
    /// Glitch in favor occurs on complication roll of 10.
    /// The mechanism's own instability causes it to bypass itself.
    /// </remarks>
    public static JuryRigResult GlitchInFavor(
        int netSuccesses,
        int dc,
        BypassMethod method,
        string? narrative = null)
    {
        return new JuryRigResult(
            Outcome: JuryRigOutcome.Success,
            NetSuccesses: netSuccesses,
            EffectiveDc: dc,
            ComplicationRoll: 10,
            ComplicationEffect: Enums.ComplicationEffect.GlitchInFavor,
            DamageDealt: 0,
            DamageType: "none",
            AlertTriggered: false,
            SalvagedComponents: Array.Empty<string>(),
            NarrativeText: narrative ?? "The machine's own instability works in your favor! " +
                                        "A fortuitous glitch causes it to bypass itself, granting access.",
            MethodUsed: method);
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a formatted display string for the result.
    /// </summary>
    /// <returns>A human-readable summary of the jury-rig result.</returns>
    public string ToDisplayString()
    {
        var outcomeStr = Outcome switch
        {
            JuryRigOutcome.CriticalSuccess => "CRITICAL SUCCESS",
            JuryRigOutcome.Success => "SUCCESS",
            JuryRigOutcome.PartialSuccess => "PARTIAL SUCCESS",
            JuryRigOutcome.Failure => "FAILURE",
            JuryRigOutcome.Fumble => "FUMBLE",
            JuryRigOutcome.PermanentLock => "PERMANENTLY LOCKED",
            _ => Outcome.ToString().ToUpperInvariant()
        };

        var consequences = new List<string>();

        if (TookDamage)
        {
            consequences.Add($"{DamageDealt} {DamageType} damage");
        }

        if (AlertTriggered)
        {
            consequences.Add("alarm triggered");
        }

        if (MechanismDestroyed && !HasSalvage)
        {
            consequences.Add("mechanism destroyed");
        }

        if (HasSalvage)
        {
            consequences.Add($"salvaged {SalvagedComponents.Count} components");
        }

        if (ComplicationEffect.HasValue && Outcome == JuryRigOutcome.Failure)
        {
            consequences.Add($"complication: {ComplicationEffect.Value}");
        }

        var consequenceStr = consequences.Count > 0
            ? $" [{string.Join(", ", consequences)}]"
            : "";

        return $"Jury-Rig {MethodUsed.GetDisplayName()}: {outcomeStr} " +
               $"(Roll {NetSuccesses} vs DC {EffectiveDc}){consequenceStr}";
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"JuryRigResult[Outcome={Outcome} Method={MethodUsed} " +
               $"Net={NetSuccesses} DC={EffectiveDc} Dmg={DamageDealt} " +
               $"Salvage={SalvagedComponents.Count} Alert={AlertTriggered}]";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}
