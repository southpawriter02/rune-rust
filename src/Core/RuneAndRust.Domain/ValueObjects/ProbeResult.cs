// ------------------------------------------------------------------------------
// <copyright file="ProbeResult.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// The result of the Probe step in the jury-rigging procedure.
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// The result of the Probe step in the jury-rigging procedure.
/// </summary>
/// <remarks>
/// <para>
/// The Probe step is automatic (no roll required) and establishes baseline
/// behavior by trying obvious buttons and levers:
/// <list type="bullet">
///   <item><description>Machine may beep, light up, or do nothing</description></item>
///   <item><description>Reveals clues about mechanism state (glitched, locked, etc.)</description></item>
///   <item><description>Required step before Pattern Recognition or Method Selection</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Reactions">Observable reactions from pressing buttons/levers.</param>
/// <param name="IsGlitched">Whether the mechanism appears to be in a glitched state.</param>
/// <param name="IsLocked">Whether the mechanism appears to be locked.</param>
/// <param name="IsPowered">Whether the mechanism appears to have power.</param>
/// <param name="NarrativeText">Flavor text describing the probe results.</param>
public readonly record struct ProbeResult(
    IReadOnlyList<string> Reactions,
    bool IsGlitched,
    bool IsLocked,
    bool IsPowered,
    string NarrativeText)
{
    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether the mechanism showed any reaction.
    /// </summary>
    /// <remarks>
    /// Some mechanisms are completely unresponsive to probing, which may
    /// indicate power failure, severe damage, or dormant state.
    /// </remarks>
    public bool HasReactions => Reactions.Count > 0;

    /// <summary>
    /// Gets a value indicating whether Glitch Exploitation is possible.
    /// </summary>
    /// <remarks>
    /// Glitch Exploitation (-4 DC) is only available if the mechanism
    /// exhibits [Glitched] behavior detected during probing.
    /// </remarks>
    public bool CanExploitGlitch => IsGlitched;

    /// <summary>
    /// Gets a value indicating whether Power Cycling might be effective.
    /// </summary>
    /// <remarks>
    /// Power Cycling is most effective on powered mechanisms that are
    /// locked or in an error state. Unpowered mechanisms won't respond.
    /// </remarks>
    public bool PowerCyclingRelevant => IsPowered;

    /// <summary>
    /// Gets a value indicating whether the mechanism state suggests caution.
    /// </summary>
    /// <remarks>
    /// A locked mechanism may engage permanent lockout on failed attempts,
    /// suggesting careful approach over brute force.
    /// </remarks>
    public bool SuggestionCaution => IsLocked;

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a probe result for a responsive mechanism.
    /// </summary>
    /// <param name="reactions">The observed reactions.</param>
    /// <param name="isGlitched">Whether the mechanism exhibits glitched behavior.</param>
    /// <param name="isLocked">Whether the mechanism appears locked.</param>
    /// <param name="isPowered">Whether the mechanism has power.</param>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>A ProbeResult for a responsive mechanism.</returns>
    public static ProbeResult Responsive(
        IReadOnlyList<string> reactions,
        bool isGlitched,
        bool isLocked,
        bool isPowered,
        string? narrative = null)
    {
        var defaultNarrative = BuildDefaultNarrative(reactions, isGlitched, isLocked, isPowered);

        return new ProbeResult(
            Reactions: reactions,
            IsGlitched: isGlitched,
            IsLocked: isLocked,
            IsPowered: isPowered,
            NarrativeText: narrative ?? defaultNarrative);
    }

    /// <summary>
    /// Creates a probe result for an unresponsive mechanism.
    /// </summary>
    /// <param name="isPowered">Whether the mechanism has power despite being unresponsive.</param>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>A ProbeResult for an unresponsive mechanism.</returns>
    /// <remarks>
    /// Unresponsive mechanisms may be dormant, damaged, or simply designed
    /// to ignore probing attempts. Power state is still detectable.
    /// </remarks>
    public static ProbeResult Unresponsive(bool isPowered, string? narrative = null)
    {
        return new ProbeResult(
            Reactions: Array.Empty<string>(),
            IsGlitched: false,
            IsLocked: false,
            IsPowered: isPowered,
            NarrativeText: narrative ?? (isPowered
                ? "The mechanism hums quietly but ignores your prodding. It's powered but dormant."
                : "The mechanism sits dark and silent. No response to any input."));
    }

    /// <summary>
    /// Creates a probe result for a glitched mechanism.
    /// </summary>
    /// <param name="reactions">The erratic reactions observed.</param>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>A ProbeResult for a glitched mechanism.</returns>
    /// <remarks>
    /// Glitched mechanisms exhibit erratic behavior: flickering displays,
    /// random beeps, unexpected movements. This enables Glitch Exploitation.
    /// </remarks>
    public static ProbeResult Glitched(
        IReadOnlyList<string> reactions,
        string? narrative = null)
    {
        return new ProbeResult(
            Reactions: reactions,
            IsGlitched: true,
            IsLocked: false,
            IsPowered: true,
            NarrativeText: narrative ?? "The mechanism behaves erratically! Displays flicker, " +
                                        "lights flash in random patterns. Its corruption may work in your favor.");
    }

    /// <summary>
    /// Creates a probe result for a locked mechanism.
    /// </summary>
    /// <param name="reactions">The reactions observed.</param>
    /// <param name="narrative">Optional narrative text.</param>
    /// <returns>A ProbeResult for a locked mechanism.</returns>
    /// <remarks>
    /// A locked mechanism displays warning indicators and refuses access.
    /// Care should be taken as further tampering may trigger permanent lockout.
    /// </remarks>
    public static ProbeResult Locked(
        IReadOnlyList<string> reactions,
        string? narrative = null)
    {
        return new ProbeResult(
            Reactions: reactions,
            IsGlitched: false,
            IsLocked: true,
            IsPowered: true,
            NarrativeText: narrative ?? "A harsh tone sounds and a red light blinks insistently. " +
                                        "The mechanism is in a locked state—proceed with caution.");
    }

    // -------------------------------------------------------------------------
    // Private Helpers
    // -------------------------------------------------------------------------

    private static string BuildDefaultNarrative(
        IReadOnlyList<string> reactions,
        bool isGlitched,
        bool isLocked,
        bool isPowered)
    {
        var parts = new List<string>();

        if (!isPowered)
        {
            parts.Add("The mechanism appears unpowered");
        }
        else if (isGlitched)
        {
            parts.Add("The mechanism responds erratically");
        }
        else if (isLocked)
        {
            parts.Add("The mechanism is in a locked state");
        }
        else
        {
            parts.Add("The mechanism responds to your probing");
        }

        if (reactions.Count > 0)
        {
            parts.Add($"Observed: {string.Join(", ", reactions)}");
        }

        return string.Join(". ", parts) + ".";
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a formatted display string for the result.
    /// </summary>
    /// <returns>A human-readable summary of the probe result.</returns>
    public string ToDisplayString()
    {
        var flags = new List<string>();

        if (IsPowered) flags.Add("Powered");
        if (IsGlitched) flags.Add("Glitched");
        if (IsLocked) flags.Add("Locked");

        var flagStr = flags.Count > 0 ? $" [{string.Join(", ", flags)}]" : "";
        var reactionCount = HasReactions ? $" ({Reactions.Count} reactions)" : " (no response)";

        return $"Probe: Complete{flagStr}{reactionCount}";
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"ProbeResult[Powered={IsPowered} Glitched={IsGlitched} " +
               $"Locked={IsLocked} Reactions={Reactions.Count}]";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}
