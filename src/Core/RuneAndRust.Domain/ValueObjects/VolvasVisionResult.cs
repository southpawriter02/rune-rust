namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a Völva's Vision ability execution.
/// Encapsulates the reveal radius, Resonance changes, Corruption evaluation results,
/// and AP cost tracking (including Resonance Cascade reduction).
/// </summary>
/// <remarks>
/// <para>Völva's Vision is the Seiðkona's Tier 3 active mass-detection ability:</para>
/// <list type="bullet">
/// <item>Cost: 3 AP (2 AP with Resonance Cascade at Resonance 5+)</item>
/// <item>Resonance: +2 Aether Resonance per cast (high buildup, shared with Aether Storm)</item>
/// <item>Corruption: probability-based check at Resonance 5+ (5%/15%/25%)</item>
/// <item>Effect: Reveals all hidden, invisible, and concealed enemies within a 15-space radius</item>
/// <item>Duration: Instant revelation (effects persist until scene changes)</item>
/// </list>
/// <para>Unlike Seiðr Bolt and Aether Storm, Völva's Vision deals no damage and does not
/// contribute to the Accumulated Aetheric Damage tracker. The +2 Resonance gain represents
/// significant escalation risk — a single cast can cross from Safe (0-4) into Risky (5-7)
/// or from Risky into Dangerous (8-9) territory.</para>
/// <para>As a non-combat ability, Völva's Vision provides strategic value through information
/// gathering rather than direct damage. It is the enhanced version of the Tier 1 Wyrd Sight
/// ability, offering mass revelation instead of individual detection.</para>
/// <para>This result object is returned by the ability service to provide
/// complete information for combat log display, state updates, and player feedback.</para>
/// </remarks>
public sealed record VolvasVisionResult
{
    /// <summary>
    /// The detection radius for the Völva's Vision reveal effect (always 15 spaces).
    /// This is the largest detection radius of any Seiðkona ability.
    /// </summary>
    public int RevealRadius { get; init; } = 15;

    /// <summary>
    /// Aether Resonance value before this cast.
    /// Used to show the player the Resonance state at time of Corruption evaluation.
    /// </summary>
    public int ResonanceBefore { get; init; }

    /// <summary>
    /// Aether Resonance value after this cast (+2 from Völva's Vision).
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
    /// May be reduced from the base cost (3 AP) if Resonance Cascade was active (2 AP).
    /// </summary>
    public int ApCostPaid { get; init; }

    /// <summary>
    /// Whether Resonance Cascade was active and reduced the AP cost for this cast.
    /// </summary>
    public bool CascadeApplied { get; init; }

    /// <summary>
    /// Gets a complete result string suitable for combat log display.
    /// Includes Cascade reduction tag, Corruption trigger/check tags, and Resonance state.
    /// </summary>
    /// <returns>A formatted combat log entry for this Völva's Vision cast.</returns>
    public string GetDescription()
    {
        var cascadeTag = CascadeApplied ? " [Cascade: -1 AP]" : "";
        var corruptionTag = CorruptionTriggered ? " [CORRUPTION +1]" : "";
        var checkTag = CorruptionCheckPerformed && !CorruptionTriggered
            ? $" [Corruption check: d100={CorruptionRoll} vs {CorruptionRiskPercent}% — safe]"
            : "";

        return $"Völva's Vision reveals hidden enemies in {RevealRadius}-space radius ({ApCostPaid} AP{cascadeTag}). " +
               $"Resonance: {ResonanceBefore} → {ResonanceAfter}{corruptionTag}{checkTag}";
    }

    /// <summary>
    /// Gets a formatted string showing the Resonance change from this cast.
    /// </summary>
    /// <returns>A string in the format "Resonance: 6 → 8 (+2)".</returns>
    public string GetResonanceChange()
    {
        return $"Resonance: {ResonanceBefore} → {ResonanceAfter} (+{ResonanceGained})";
    }
}
