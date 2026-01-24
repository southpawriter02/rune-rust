// ------------------------------------------------------------------------------
// <copyright file="LayerResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Result of a single layer attempt during terminal infiltration.
// Part of v0.15.4b Terminal Hacking System implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of a single layer attempt during terminal infiltration.
/// </summary>
/// <remarks>
/// <para>
/// Encapsulates the outcome of one layer check, including the skill check
/// result, any access level changes, and narrative description.
/// </para>
/// </remarks>
/// <param name="Layer">The infiltration layer this result applies to.</param>
/// <param name="SkillCheckResult">The underlying skill check result.</param>
/// <param name="Outcome">The skill outcome tier.</param>
/// <param name="DcUsed">The DC that was used for this layer.</param>
/// <param name="TimeRounds">Number of rounds this layer took.</param>
/// <param name="AccessLevelGranted">Access level achieved (if any).</param>
/// <param name="NarrativeDescription">Flavor text describing the outcome.</param>
public readonly record struct LayerResult(
    InfiltrationLayer Layer,
    SkillCheckResult SkillCheckResult,
    SkillOutcome Outcome,
    int DcUsed,
    int TimeRounds,
    AccessLevel? AccessLevelGranted,
    string NarrativeDescription)
{
    /// <summary>
    /// Whether this layer was passed successfully.
    /// </summary>
    public bool IsSuccess => Outcome is SkillOutcome.MarginalSuccess
        or SkillOutcome.FullSuccess
        or SkillOutcome.ExceptionalSuccess
        or SkillOutcome.CriticalSuccess;

    /// <summary>
    /// Whether this was a critical success.
    /// </summary>
    public bool IsCriticalSuccess => Outcome == SkillOutcome.CriticalSuccess;

    /// <summary>
    /// Whether this was a fumble (critical failure).
    /// </summary>
    public bool IsFumble => Outcome == SkillOutcome.CriticalFailure;

    /// <summary>
    /// Creates a success result for a layer.
    /// </summary>
    /// <param name="layer">The infiltration layer.</param>
    /// <param name="checkResult">The skill check result.</param>
    /// <param name="outcome">The skill outcome tier.</param>
    /// <param name="dc">The DC used for this layer.</param>
    /// <param name="timeRounds">Number of rounds this layer took.</param>
    /// <param name="accessGranted">Access level achieved (if any).</param>
    /// <param name="narrative">Narrative description of the outcome.</param>
    /// <returns>A new LayerResult representing success.</returns>
    public static LayerResult Success(
        InfiltrationLayer layer,
        SkillCheckResult checkResult,
        SkillOutcome outcome,
        int dc,
        int timeRounds,
        AccessLevel? accessGranted,
        string narrative)
    {
        return new LayerResult(
            layer,
            checkResult,
            outcome,
            dc,
            timeRounds,
            accessGranted,
            narrative);
    }

    /// <summary>
    /// Creates a failure result for a layer.
    /// </summary>
    /// <param name="layer">The infiltration layer.</param>
    /// <param name="checkResult">The skill check result.</param>
    /// <param name="outcome">The skill outcome tier.</param>
    /// <param name="dc">The DC used for this layer.</param>
    /// <param name="timeRounds">Number of rounds this layer took.</param>
    /// <param name="narrative">Narrative description of the failure.</param>
    /// <returns>A new LayerResult representing failure.</returns>
    public static LayerResult Failure(
        InfiltrationLayer layer,
        SkillCheckResult checkResult,
        SkillOutcome outcome,
        int dc,
        int timeRounds,
        string narrative)
    {
        return new LayerResult(
            layer,
            checkResult,
            outcome,
            dc,
            timeRounds,
            null,
            narrative);
    }

    /// <summary>
    /// Creates a fumble result for a layer.
    /// </summary>
    /// <param name="layer">The infiltration layer.</param>
    /// <param name="checkResult">The skill check result.</param>
    /// <param name="dc">The DC used for this layer.</param>
    /// <param name="narrative">Narrative description of the fumble.</param>
    /// <returns>A new LayerResult representing a fumble.</returns>
    public static LayerResult Fumble(
        InfiltrationLayer layer,
        SkillCheckResult checkResult,
        int dc,
        string narrative)
    {
        return new LayerResult(
            layer,
            checkResult,
            SkillOutcome.CriticalFailure,
            dc,
            0, // Fumble ends immediately
            AccessLevel.Lockout,
            narrative);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var result = IsSuccess ? "SUCCESS" : "FAILURE";
        if (IsCriticalSuccess) result = "CRITICAL SUCCESS";
        if (IsFumble) result = "FUMBLE";

        return $"{Layer} {result}: {NarrativeDescription}";
    }
}
