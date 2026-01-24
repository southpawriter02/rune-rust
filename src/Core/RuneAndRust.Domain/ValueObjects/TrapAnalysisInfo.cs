// ------------------------------------------------------------------------------
// <copyright file="TrapAnalysisInfo.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Information revealed through Analysis of a trap, including DC,
// consequences, and disarmament hints.
// Part of v0.15.4d Trap Disarmament System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Information revealed through Analysis of a trap.
/// </summary>
/// <remarks>
/// <para>
/// Analysis reveals information based on check result against tiered thresholds:
/// <list type="bullet">
///   <item><description>DC - 2: Reveal disarm DC</description></item>
///   <item><description>DC: Reveal failure consequences</description></item>
///   <item><description>DC + 2: Reveal hint (+1d10 on disarmament)</description></item>
/// </list>
/// </para>
/// <para>
/// Use the static factory methods to create appropriately-configured results.
/// </para>
/// </remarks>
/// <param name="DisarmDcRevealed">Whether the disarm DC was revealed.</param>
/// <param name="DisarmDc">The revealed disarm DC (null if not revealed).</param>
/// <param name="ConsequencesRevealed">Whether failure consequences were revealed.</param>
/// <param name="ConsequenceDescription">Description of failure consequences (null if not revealed).</param>
/// <param name="HintRevealed">Whether a disarm hint was revealed.</param>
/// <param name="HintDescription">The revealed hint for disarmament (null if not revealed).</param>
public readonly record struct TrapAnalysisInfo(
    bool DisarmDcRevealed,
    int? DisarmDc,
    bool ConsequencesRevealed,
    string? ConsequenceDescription,
    bool HintRevealed,
    string? HintDescription)
{
    // -------------------------------------------------------------------------
    // Static Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Empty analysis info (no information revealed).
    /// </summary>
    /// <remarks>
    /// Used when analysis fails completely or is skipped.
    /// </remarks>
    public static TrapAnalysisInfo Empty => new(
        DisarmDcRevealed: false,
        DisarmDc: null,
        ConsequencesRevealed: false,
        ConsequenceDescription: null,
        HintRevealed: false,
        HintDescription: null);

    /// <summary>
    /// Creates analysis info with all information revealed.
    /// </summary>
    /// <param name="disarmDc">The disarm DC to reveal.</param>
    /// <param name="consequences">Description of failure consequences.</param>
    /// <param name="hint">The disarmament hint.</param>
    /// <returns>A fully-populated TrapAnalysisInfo.</returns>
    /// <remarks>
    /// Used for exceptional analysis results that reveal everything.
    /// </remarks>
    public static TrapAnalysisInfo Full(int disarmDc, string consequences, string hint)
    {
        return new TrapAnalysisInfo(
            DisarmDcRevealed: true,
            DisarmDc: disarmDc,
            ConsequencesRevealed: true,
            ConsequenceDescription: consequences,
            HintRevealed: true,
            HintDescription: hint);
    }

    /// <summary>
    /// Creates analysis info with partial information based on check result.
    /// </summary>
    /// <param name="netSuccesses">The net successes from the analysis check.</param>
    /// <param name="baseDc">The trap's base DC for analysis thresholds.</param>
    /// <param name="disarmDc">The actual disarm DC (revealed if threshold met).</param>
    /// <param name="consequences">Description of failure consequences.</param>
    /// <param name="hint">The disarmament hint.</param>
    /// <returns>A TrapAnalysisInfo with appropriate fields revealed.</returns>
    /// <remarks>
    /// <para>
    /// Threshold calculation uses success-counting DC conversion (DC / 6):
    /// <list type="bullet">
    ///   <item><description>DC - 2: Reveal disarm DC</description></item>
    ///   <item><description>DC: Reveal consequences</description></item>
    ///   <item><description>DC + 2: Reveal hint</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static TrapAnalysisInfo FromCheckResult(
        int netSuccesses,
        int baseDc,
        int disarmDc,
        string consequences,
        string hint)
    {
        // Convert traditional DC to success-counting DC (DC / 6, minimum 1)
        // DC - 2: Reveal disarm DC
        var revealDcThreshold = Math.Max(1, (int)Math.Ceiling((baseDc - 2) / 6.0));
        var dcRevealed = netSuccesses >= revealDcThreshold;

        // DC: Reveal consequences
        var consequenceThreshold = Math.Max(1, (int)Math.Ceiling(baseDc / 6.0));
        var consequencesRevealed = netSuccesses >= consequenceThreshold;

        // DC + 2: Reveal hint
        var hintThreshold = Math.Max(1, (int)Math.Ceiling((baseDc + 2) / 6.0));
        var hintRevealed = netSuccesses >= hintThreshold;

        return new TrapAnalysisInfo(
            DisarmDcRevealed: dcRevealed,
            DisarmDc: dcRevealed ? disarmDc : null,
            ConsequencesRevealed: consequencesRevealed,
            ConsequenceDescription: consequencesRevealed ? consequences : null,
            HintRevealed: hintRevealed,
            HintDescription: hintRevealed ? hint : null);
    }

    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Whether any information was revealed.
    /// </summary>
    /// <remarks>
    /// Returns true if at least one piece of information was revealed.
    /// </remarks>
    public bool HasAnyInfo => DisarmDcRevealed || ConsequencesRevealed || HintRevealed;

    /// <summary>
    /// Whether a hint was revealed (grants +1d10 on disarmament).
    /// </summary>
    /// <remarks>
    /// If true, the character gains a +1d10 bonus on subsequent disarmament attempts.
    /// </remarks>
    public bool GrantsHintBonus => HintRevealed;

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a formatted display string for the analysis info.
    /// </summary>
    /// <returns>A human-readable summary of revealed information.</returns>
    public string ToDisplayString()
    {
        if (!HasAnyInfo)
        {
            return "Analysis revealed nothing useful.";
        }

        var parts = new List<string>();

        if (DisarmDcRevealed && DisarmDc.HasValue)
        {
            parts.Add($"DC: {DisarmDc.Value}");
        }

        if (ConsequencesRevealed && ConsequenceDescription is not null)
        {
            parts.Add($"Consequences: {ConsequenceDescription}");
        }

        if (HintRevealed && HintDescription is not null)
        {
            parts.Add($"Hint: {HintDescription}");
        }

        return string.Join(" | ", parts);
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"Analysis[DC={DisarmDcRevealed}, Consequences={ConsequencesRevealed}, Hint={HintRevealed}]";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}
