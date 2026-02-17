namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a Fate's Thread ability execution.
/// Encapsulates Resonance changes, Corruption evaluation results, and AP cost tracking
/// (including Resonance Cascade reduction).
/// </summary>
/// <remarks>
/// <para>Fate's Thread is the Seiðkona's Tier 2 active divination ability:</para>
/// <list type="bullet">
/// <item>Cost: 2 AP (1 AP with Resonance Cascade at Resonance 5+)</item>
/// <item>Resonance: +2 Aether Resonance per cast (higher than Tier 1's +1)</item>
/// <item>Corruption: probability-based check at Resonance 5+ (5%/15%/25%)</item>
/// <item>Effect: Glimpse the target's next action (resolution deferred to combat system)</item>
/// <item>Duration: 1 action (information persists for next turn only)</item>
/// </list>
/// <para>Unlike Seiðr Bolt, Fate's Thread deals no damage and does not add to
/// the Accumulated Aetheric Damage tracker. The +2 Resonance gain represents
/// a significant escalation risk — two casts can push from Safe (0-4) to
/// Dangerous (8-9) range.</para>
/// <para>This result object is returned by the ability service to provide
/// complete information for combat log display, state updates, and player feedback.</para>
/// </remarks>
public sealed record FatesThreadResult
{
    /// <summary>
    /// Aether Resonance value before this cast.
    /// Used to show the player the Resonance state at time of Corruption evaluation.
    /// </summary>
    public int ResonanceBefore { get; init; }

    /// <summary>
    /// Aether Resonance value after this cast (+2 from Fate's Thread).
    /// </summary>
    public int ResonanceAfter { get; init; }

    /// <summary>
    /// Actual Resonance gained from this cast.
    /// May be less than 2 if Resonance was near maximum (10).
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
    /// May be reduced from the base cost (2 AP) if Resonance Cascade was active (1 AP).
    /// </summary>
    public int ApCostPaid { get; init; }

    /// <summary>
    /// Whether Resonance Cascade was active and reduced the AP cost for this cast.
    /// </summary>
    public bool CascadeApplied { get; init; }

    /// <summary>
    /// Gets a complete result string suitable for combat log display.
    /// </summary>
    /// <returns>A formatted combat log entry for this Fate's Thread cast.</returns>
    public string GetDescription()
    {
        var cascadeTag = CascadeApplied ? " [Cascade: -1 AP]" : "";
        var corruptionTag = CorruptionTriggered ? " [CORRUPTION +1]" : "";
        var checkTag = CorruptionCheckPerformed && !CorruptionTriggered
            ? $" [Corruption check: d100={CorruptionRoll} vs {CorruptionRiskPercent}% — safe]"
            : "";

        return $"Fate's Thread cast ({ApCostPaid} AP{cascadeTag}). " +
               $"Resonance: {ResonanceBefore} → {ResonanceAfter}{corruptionTag}{checkTag}";
    }

    /// <summary>
    /// Gets a formatted string showing the Resonance change from this cast.
    /// </summary>
    /// <returns>A string in the format "Resonance: 4 → 6 (+2)".</returns>
    public string GetResonanceChange()
    {
        return $"Resonance: {ResonanceBefore} → {ResonanceAfter} (+{ResonanceGained})";
    }
}
