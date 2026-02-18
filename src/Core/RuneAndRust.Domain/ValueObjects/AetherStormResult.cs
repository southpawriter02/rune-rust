namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of an Aether Storm ability execution.
/// Encapsulates the full damage breakdown, Resonance changes, accumulated damage tracking,
/// Corruption evaluation results, and AP cost tracking (including Resonance Cascade reduction).
/// </summary>
/// <remarks>
/// <para>Aether Storm is the Seiðkona's Tier 3 active area-of-effect damage ability:</para>
/// <list type="bullet">
/// <item>Cost: 5 AP (4 AP with Resonance Cascade at Resonance 5+)</item>
/// <item>Damage: 4d6 Aetheric damage (cone area-of-effect)</item>
/// <item>Resonance: +2 Aether Resonance per cast (high buildup, shared with Völva's Vision)</item>
/// <item>Accumulation: 4d6 damage dealt is added to Accumulated Aetheric Damage tracker for Unraveling</item>
/// <item>Corruption: probability-based check at Resonance 5+ (5%/15%/25%)</item>
/// </list>
/// <para>Aether Storm is the highest single-ability damage source in the Seiðkona arsenal before
/// the Unraveling capstone. Its 4d6 damage (average 14, range 4-24) combined with +2 Resonance
/// makes it the fastest path to building both Accumulated Aetheric Damage and Resonance toward
/// capstone readiness.</para>
/// <para>The 5 AP base cost (highest in the ability tree) means Aether Storm typically cannot
/// be cast multiple times per turn. With Resonance Cascade active, the cost reduces to 4 AP,
/// enabling more aggressive usage at the expense of increased Corruption risk.</para>
/// <para>This result object is returned by the ability service to provide
/// complete information for combat log display, state updates, and player feedback.</para>
/// </remarks>
public sealed record AetherStormResult
{
    /// <summary>
    /// The damage roll result (4d6) before any modifiers.
    /// Range: 4-24, average 14.
    /// </summary>
    public int DamageRoll { get; init; }

    /// <summary>
    /// Total Aetheric damage dealt by this Aether Storm.
    /// Currently equal to <see cref="DamageRoll"/> but may diverge with future modifiers
    /// (e.g., area-of-effect resistance, save-for-half mechanics).
    /// </summary>
    public int TotalDamage { get; init; }

    /// <summary>
    /// Aether Resonance value before this cast.
    /// Used to show the player the Resonance state at time of Corruption evaluation.
    /// </summary>
    public int ResonanceBefore { get; init; }

    /// <summary>
    /// Aether Resonance value after this cast (+2 from Aether Storm).
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
    /// May be reduced from the base cost (5 AP) if Resonance Cascade was active (4 AP).
    /// </summary>
    public int ApCostPaid { get; init; }

    /// <summary>
    /// Whether Resonance Cascade was active and reduced the AP cost for this cast.
    /// </summary>
    public bool CascadeApplied { get; init; }

    /// <summary>
    /// Gets a detailed breakdown of the damage components for combat log display.
    /// </summary>
    /// <returns>
    /// A formatted string showing the damage roll, e.g.:
    /// "Aether Storm: 4d6 = 16 Aetheric damage (Total: 16)"
    /// </returns>
    public string GetDamageBreakdown()
    {
        return $"Aether Storm: 4d6 = {DamageRoll} Aetheric damage (Total: {TotalDamage})";
    }

    /// <summary>
    /// Gets a complete result string suitable for combat log display.
    /// Includes Cascade reduction tag, Corruption trigger/check tags, damage, and Resonance state.
    /// </summary>
    /// <returns>A formatted combat log entry for this Aether Storm cast.</returns>
    public string GetDescription()
    {
        var cascadeTag = CascadeApplied ? " [Cascade: -1 AP]" : "";
        var corruptionTag = CorruptionTriggered ? " [CORRUPTION +1]" : "";
        var checkTag = CorruptionCheckPerformed && !CorruptionTriggered
            ? $" [Corruption check: d100={CorruptionRoll} vs {CorruptionRiskPercent}% — safe]"
            : "";

        return $"Aether Storm deals {TotalDamage} Aetheric damage ({ApCostPaid} AP{cascadeTag}). " +
               $"Resonance: {ResonanceBefore} → {ResonanceAfter}{corruptionTag}{checkTag}";
    }

    /// <summary>
    /// Gets a formatted string showing the Resonance change from this cast.
    /// </summary>
    /// <returns>A string in the format "Resonance: 6 → 8 (+2)".</returns>
    public string GetResonanceChange()
    {
        var delta = ResonanceAfter - ResonanceBefore;
        return $"Resonance: {ResonanceBefore} → {ResonanceAfter} (+{delta})";
    }
}
