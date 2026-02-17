namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a Weave Disruption ability execution.
/// Encapsulates the dispel roll, Resonance changes, Corruption evaluation results,
/// and AP cost tracking (including Resonance Cascade reduction).
/// </summary>
/// <remarks>
/// <para>Weave Disruption is the Seiðkona's Tier 2 active dispel/counterspell ability:</para>
/// <list type="bullet">
/// <item>Cost: 3 AP (2 AP with Resonance Cascade at Resonance 5+)</item>
/// <item>Dispel: Roll d20 + current Resonance as bonus for dispel attempt</item>
/// <item>Resonance: +1 Aether Resonance per cast (lower than Fate's Thread's +2)</item>
/// <item>Corruption: probability-based check at Resonance 5+ (5%/15%/25%)</item>
/// <item>Effect: Attempt to dispel magical effects on target (resolution deferred to combat system)</item>
/// </list>
/// <para>Unlike Seiðr Bolt, Weave Disruption deals no direct damage and does not add to
/// the Accumulated Aetheric Damage tracker. The d20 roll + Resonance bonus is stored
/// for the combat system to resolve against effect DCs.</para>
/// <para>This result object is returned by the ability service to provide
/// complete information for combat log display, state updates, and player feedback.</para>
/// </remarks>
public sealed record WeaveDisruptionResult
{
    /// <summary>
    /// The raw d20 roll for the dispel attempt.
    /// </summary>
    public int DispelRoll { get; init; }

    /// <summary>
    /// The Resonance bonus added to the dispel roll.
    /// Equal to the player's Resonance at time of cast (before the +1 gain).
    /// </summary>
    public int ResonanceBonus { get; init; }

    /// <summary>
    /// Total dispel check result (DispelRoll + ResonanceBonus).
    /// Compared against effect DCs by the combat system.
    /// </summary>
    public int TotalRoll { get; init; }

    /// <summary>
    /// Aether Resonance value before this cast.
    /// Used to show the player the Resonance state at time of Corruption evaluation
    /// and as the Resonance bonus for the dispel roll.
    /// </summary>
    public int ResonanceBefore { get; init; }

    /// <summary>
    /// Aether Resonance value after this cast (+1 from Weave Disruption).
    /// </summary>
    public int ResonanceAfter { get; init; }

    /// <summary>
    /// Actual Resonance gained from this cast.
    /// May be less than 1 if Resonance was at maximum (10).
    /// </summary>
    public int ResonanceGained { get; init; }

    /// <summary>
    /// Whether this cast triggered a Corruption check (Resonance was 5+ before cast).
    /// True even if the check passed (no Corruption applied) — indicates a check was performed.
    /// </summary>
    public bool CorruptionCheckPerformed { get; init; }

    /// <summary>
    /// Whether this cast actually triggered Corruption accumulation.
    /// True only when a d100 check was performed AND the roll fell within the threshold.
    /// </summary>
    public bool CorruptionTriggered { get; init; }

    /// <summary>
    /// Descriptive reason for the Corruption check outcome.
    /// Includes roll context when a check was performed.
    /// </summary>
    public string? CorruptionReason { get; init; }

    /// <summary>
    /// The d100 roll result if a Corruption check was performed. Zero if no check needed.
    /// </summary>
    public int CorruptionRoll { get; init; }

    /// <summary>
    /// The Corruption risk percentage threshold at the time of the check.
    /// Zero if no check was needed (Resonance below 5).
    /// </summary>
    public int CorruptionRiskPercent { get; init; }

    /// <summary>
    /// The actual AP cost paid for this cast.
    /// May be reduced from the base cost (3 AP) if Resonance Cascade was active (2 AP).
    /// </summary>
    public int ApCostPaid { get; init; }

    /// <summary>
    /// Whether Resonance Cascade was active and reduced the AP cost for this cast.
    /// </summary>
    public bool CascadeApplied { get; init; }

    /// <summary>
    /// Gets a detailed breakdown of the dispel check for combat log display.
    /// </summary>
    /// <returns>
    /// A formatted string showing the dispel roll components, e.g.:
    /// "Weave Disruption: d20 = 14 + Resonance 6 = 20 (total)"
    /// </returns>
    public string GetDispelBreakdown()
    {
        return $"Weave Disruption: d20 = {DispelRoll} + Resonance {ResonanceBonus} = {TotalRoll} (total)";
    }

    /// <summary>
    /// Gets a complete result string suitable for combat log display.
    /// </summary>
    /// <returns>A formatted combat log entry for this Weave Disruption cast.</returns>
    public string GetDescription()
    {
        var cascadeTag = CascadeApplied ? " [Cascade: -1 AP]" : "";
        var corruptionTag = CorruptionTriggered ? " [CORRUPTION +1]" : "";
        var checkTag = CorruptionCheckPerformed && !CorruptionTriggered
            ? $" [Corruption check: d100={CorruptionRoll} vs {CorruptionRiskPercent}% — safe]"
            : "";

        return $"Weave Disruption cast ({ApCostPaid} AP{cascadeTag}). " +
               $"{GetDispelBreakdown()}. " +
               $"Resonance: {ResonanceBefore} → {ResonanceAfter}{corruptionTag}{checkTag}";
    }

    /// <summary>
    /// Gets a formatted string showing the Resonance change from this cast.
    /// </summary>
    /// <returns>A string in the format "Resonance: 6 → 7 (+1)".</returns>
    public string GetResonanceChange()
    {
        return $"Resonance: {ResonanceBefore} → {ResonanceAfter} (+{ResonanceGained})";
    }
}
