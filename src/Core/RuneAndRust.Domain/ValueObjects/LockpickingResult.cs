// ------------------------------------------------------------------------------
// <copyright file="LockpickingResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Value object encapsulating the complete result of a lockpicking attempt,
// including success/failure, salvaged components, and fumble consequences.
// Part of v0.15.4a Lockpicking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Result of a lockpicking attempt.
/// </summary>
/// <remarks>
/// <para>
/// Encapsulates the complete outcome of a lockpicking check including:
/// <list type="bullet">
///   <item><description>Whether the lock was opened</description></item>
///   <item><description>The skill check outcome tier</description></item>
///   <item><description>Any salvaged components (on critical success)</description></item>
///   <item><description>Fumble consequence details (if applicable)</description></item>
///   <item><description>Updated lock context (if state changed)</description></item>
/// </list>
/// </para>
/// <para>
/// Lockpicking outcomes follow the standard skill outcome tiers:
/// <list type="bullet">
///   <item><description>CriticalFailure (Fumble): Lock jammed, tools may break</description></item>
///   <item><description>Failure: Lock remains closed, can retry</description></item>
///   <item><description>MarginalSuccess: Lock opens after extra time</description></item>
///   <item><description>FullSuccess: Lock opens cleanly</description></item>
///   <item><description>ExceptionalSuccess: Lock opens quickly</description></item>
///   <item><description>CriticalSuccess: Lock opens + salvage component</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="IsSuccess">Whether the lock was successfully opened.</param>
/// <param name="Outcome">The skill check outcome tier.</param>
/// <param name="LockContext">The lock context used for the attempt.</param>
/// <param name="SkillCheckResult">The underlying skill check result.</param>
/// <param name="SalvagedComponent">Component salvaged on critical success.</param>
/// <param name="FumbleConsequence">Consequence created on fumble.</param>
/// <param name="UpdatedContext">Context with updated state (jammed, attempts).</param>
/// <param name="NarrativeDescription">Flavor text describing the outcome.</param>
/// <param name="ToolBroken">Whether improvised tools broke on fumble.</param>
public readonly record struct LockpickingResult(
    bool IsSuccess,
    SkillOutcome Outcome,
    LockContext LockContext,
    SkillCheckResult SkillCheckResult,
    SalvageableComponent? SalvagedComponent,
    FumbleConsequence? FumbleConsequence,
    LockContext? UpdatedContext,
    string NarrativeDescription,
    bool ToolBroken = false)
{
    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets whether this was a critical success that yielded salvage.
    /// </summary>
    public bool HasSalvage => SalvagedComponent.HasValue &&
                              SalvagedComponent.Value.Quantity > 0;

    /// <summary>
    /// Gets whether this attempt caused a fumble.
    /// </summary>
    public bool IsFumble => Outcome == SkillOutcome.CriticalFailure;

    /// <summary>
    /// Gets whether this was a critical success.
    /// </summary>
    public bool IsCriticalSuccess => Outcome == SkillOutcome.CriticalSuccess;

    /// <summary>
    /// Gets whether the lock state changed (jammed, attempt count increased).
    /// </summary>
    public bool LockStateChanged => UpdatedContext != null;

    /// <summary>
    /// Gets the final lock context (updated if changed, original if not).
    /// </summary>
    public LockContext FinalContext => UpdatedContext ?? LockContext;

    /// <summary>
    /// Gets whether the lock is now jammed (from this or previous fumble).
    /// </summary>
    public bool IsLockJammed => FinalContext.IsJammed;

    /// <summary>
    /// Gets the net successes from the skill check.
    /// </summary>
    public int NetSuccesses => SkillCheckResult.NetSuccesses;

    /// <summary>
    /// Gets the margin of success (or failure).
    /// </summary>
    public int Margin => SkillCheckResult.Margin;

    /// <summary>
    /// Gets whether this result has consequences (fumble or tool breakage).
    /// </summary>
    public bool HasConsequences => FumbleConsequence != null || ToolBroken;

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a formatted string describing the result.
    /// </summary>
    /// <returns>A formatted result string.</returns>
    public override string ToString()
    {
        var result = IsSuccess ? "SUCCESS" : "FAILURE";
        if (IsCriticalSuccess) result = "CRITICAL SUCCESS";
        if (IsFumble) result = "FUMBLE";

        return $"Lockpicking {result}: {NarrativeDescription}";
    }

    /// <summary>
    /// Returns a detailed string including mechanics.
    /// </summary>
    /// <returns>A detailed result string.</returns>
    public string ToDetailedString()
    {
        var parts = new List<string>
        {
            $"Lock: {LockContext.DisplayName} (DC {LockContext.EffectiveDc})",
            $"Roll: {NetSuccesses} net successes",
            $"Outcome: {Outcome}",
            $"Result: {(IsSuccess ? "OPENED" : "LOCKED")}"
        };

        if (HasSalvage)
            parts.Add($"Salvage: {SalvagedComponent!.Value.ToDisplayString()}");

        if (IsFumble)
            parts.Add("[MECHANISM JAMMED]");

        if (ToolBroken)
            parts.Add("[TOOLS BROKEN]");

        parts.Add($"Narrative: {NarrativeDescription}");

        return string.Join("\n", parts);
    }

    /// <summary>
    /// Returns a log-friendly string representation.
    /// </summary>
    /// <returns>A string suitable for logging.</returns>
    public string ToLogString()
    {
        var salvageStr = HasSalvage ? $" salvage={SalvagedComponent!.Value.ComponentId}" : "";
        var consequenceStr = IsFumble ? " JAMMED" : "";
        var toolStr = ToolBroken ? " TOOLS_BROKEN" : "";

        return $"Lockpick[{LockContext.LockId}] {Outcome} " +
               $"net={NetSuccesses} dc={LockContext.EffectiveDc}{salvageStr}{consequenceStr}{toolStr}";
    }

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a success result.
    /// </summary>
    /// <param name="outcome">The skill outcome tier.</param>
    /// <param name="context">The lock context.</param>
    /// <param name="checkResult">The skill check result.</param>
    /// <param name="narrative">Flavor text for the result.</param>
    /// <param name="salvage">Optional salvage component (for critical success).</param>
    /// <returns>A new LockpickingResult indicating success.</returns>
    public static LockpickingResult Success(
        SkillOutcome outcome,
        LockContext context,
        SkillCheckResult checkResult,
        string narrative,
        SalvageableComponent? salvage = null)
    {
        return new LockpickingResult(
            IsSuccess: true,
            Outcome: outcome,
            LockContext: context,
            SkillCheckResult: checkResult,
            SalvagedComponent: salvage,
            FumbleConsequence: null,
            UpdatedContext: null,
            NarrativeDescription: narrative);
    }

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    /// <param name="outcome">The skill outcome tier.</param>
    /// <param name="context">The lock context.</param>
    /// <param name="checkResult">The skill check result.</param>
    /// <param name="narrative">Flavor text for the result.</param>
    /// <param name="updatedContext">Updated context with incremented attempt count.</param>
    /// <returns>A new LockpickingResult indicating failure.</returns>
    public static LockpickingResult Failure(
        SkillOutcome outcome,
        LockContext context,
        SkillCheckResult checkResult,
        string narrative,
        LockContext? updatedContext = null)
    {
        return new LockpickingResult(
            IsSuccess: false,
            Outcome: outcome,
            LockContext: context,
            SkillCheckResult: checkResult,
            SalvagedComponent: null,
            FumbleConsequence: null,
            UpdatedContext: updatedContext,
            NarrativeDescription: narrative);
    }

    /// <summary>
    /// Creates a fumble result with [Mechanism Jammed] consequence.
    /// </summary>
    /// <param name="context">The lock context.</param>
    /// <param name="checkResult">The skill check result.</param>
    /// <param name="consequence">The fumble consequence entity.</param>
    /// <param name="updatedContext">Updated context with jammed status.</param>
    /// <param name="narrative">Flavor text for the result.</param>
    /// <param name="toolBroken">Whether improvised tools broke.</param>
    /// <returns>A new LockpickingResult indicating a fumble.</returns>
    public static LockpickingResult Fumble(
        LockContext context,
        SkillCheckResult checkResult,
        FumbleConsequence consequence,
        LockContext updatedContext,
        string narrative,
        bool toolBroken = false)
    {
        return new LockpickingResult(
            IsSuccess: false,
            Outcome: SkillOutcome.CriticalFailure,
            LockContext: context,
            SkillCheckResult: checkResult,
            SalvagedComponent: null,
            FumbleConsequence: consequence,
            UpdatedContext: updatedContext,
            NarrativeDescription: narrative,
            ToolBroken: toolBroken);
    }

    /// <summary>
    /// Creates a blocked result when attempt prerequisites are not met.
    /// </summary>
    /// <param name="context">The lock context.</param>
    /// <param name="blockedReason">Reason why the attempt was blocked.</param>
    /// <returns>A new LockpickingResult indicating a blocked attempt.</returns>
    public static LockpickingResult Blocked(
        LockContext context,
        string blockedReason)
    {
        // Create a minimal dice result for blocked attempts (no actual roll made)
        // DicePool requires count >= 1, so use a minimal pool with empty rolls
        var minimalPool = DicePool.D10(1);
        var emptyDiceResult = new DiceRollResult(
            pool: minimalPool,
            rolls: Array.Empty<int>());

        var checkResult = new SkillCheckResult(
            skillId: "lockpicking",
            skillName: "Lockpicking",
            diceResult: emptyDiceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: context.EffectiveDc,
            difficultyName: context.LockType.ToString());

        return new LockpickingResult(
            IsSuccess: false,
            Outcome: SkillOutcome.Failure,
            LockContext: context,
            SkillCheckResult: checkResult,
            SalvagedComponent: null,
            FumbleConsequence: null,
            UpdatedContext: null,
            NarrativeDescription: blockedReason);
    }

    // -------------------------------------------------------------------------
    // Narrative Templates
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a standard narrative template based on outcome.
    /// </summary>
    /// <param name="outcome">The skill outcome.</param>
    /// <param name="lockName">Optional lock name for the narrative.</param>
    /// <returns>A narrative string for the outcome.</returns>
    public static string GetNarrativeTemplate(SkillOutcome outcome, string? lockName = null)
    {
        var lockDesc = string.IsNullOrEmpty(lockName) ? "the lock" : lockName;

        return outcome switch
        {
            SkillOutcome.CriticalFailure =>
                $"Your pick catches on an internal mechanism and snaps. {lockDesc.ToUpperInvariant()} shudders, " +
                "emitting a grinding sound as something inside jams. The mechanism is now harder " +
                "to manipulate, and any key that once fit will no longer work.",

            SkillOutcome.Failure =>
                $"Your tools probe {lockDesc} without success. The mechanism resists your efforts.",

            SkillOutcome.MarginalSuccess =>
                $"With careful manipulation, {lockDesc} finally yields. " +
                "The mechanism clicks open after some effort.",

            SkillOutcome.FullSuccess =>
                $"Your practiced movements find the right combination. " +
                $"{char.ToUpper(lockDesc[0]) + lockDesc[1..]} opens smoothly.",

            SkillOutcome.ExceptionalSuccess =>
                $"Your fingers dance across the mechanism with expert precision. " +
                $"{char.ToUpper(lockDesc[0]) + lockDesc[1..]} surrenders immediately.",

            SkillOutcome.CriticalSuccess =>
                $"The mechanism yields effortlessly to your masterful touch. " +
                $"As {lockDesc} opens, you notice a valuable component that you carefully extract.",

            _ => $"{char.ToUpper(lockDesc[0]) + lockDesc[1..]} opens."
        };
    }
}
