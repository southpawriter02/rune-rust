using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a Seiðr Bolt ability execution.
/// Encapsulates the full damage breakdown, Resonance changes, accumulated damage tracking,
/// and Corruption evaluation results.
/// </summary>
/// <remarks>
/// <para>Seiðr Bolt is the Seiðkona's signature Tier 1 active ability:</para>
/// <list type="bullet">
/// <item>Cost: 1 AP</item>
/// <item>Damage: 2d6 Aetheric damage</item>
/// <item>Resonance: +1 Aether Resonance per cast (builds, not consumed)</item>
/// <item>Accumulation: damage dealt is added to Accumulated Aetheric Damage tracker</item>
/// <item>Corruption: probability-based check at Resonance 5+ (5%/15%/25%)</item>
/// </list>
/// <para>This result object is returned by the ability service to provide
/// complete information for combat log display, state updates, and player feedback.</para>
/// </remarks>
public sealed record SeidrBoltResult
{
    /// <summary>
    /// The damage roll result (2d6) before any modifiers.
    /// </summary>
    public int DamageRoll { get; init; }

    /// <summary>
    /// Total Aetheric damage dealt by this Seiðr Bolt.
    /// Currently equal to <see cref="DamageRoll"/> but may diverge with future modifiers.
    /// </summary>
    public int TotalDamage { get; init; }

    /// <summary>
    /// Aether Resonance value before this cast.
    /// Used to show the player the Resonance state at time of Corruption evaluation.
    /// </summary>
    public int ResonanceBefore { get; init; }

    /// <summary>
    /// Aether Resonance value after this cast (+1 from Seiðr Bolt).
    /// </summary>
    public int ResonanceAfter { get; init; }

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
    /// Gets a detailed breakdown of the damage components for combat log display.
    /// </summary>
    /// <returns>
    /// A formatted string showing the damage roll, e.g.:
    /// "Seiðr Bolt: 2d6 = 9 Aetheric damage"
    /// </returns>
    public string GetDamageBreakdown()
    {
        return $"Seiðr Bolt: 2d6 = {DamageRoll} Aetheric damage (Total: {TotalDamage})";
    }

    /// <summary>
    /// Gets a complete result string suitable for combat log display.
    /// </summary>
    /// <returns>A formatted combat log entry for this Seiðr Bolt cast.</returns>
    public string GetDescription()
    {
        var corruptionTag = CorruptionTriggered ? " [CORRUPTION +1]" : "";
        var checkTag = CorruptionCheckPerformed && !CorruptionTriggered
            ? $" [Corruption check: d100={CorruptionRoll} vs {CorruptionRiskPercent}% — safe]"
            : "";

        return $"Seiðr Bolt deals {TotalDamage} Aetheric damage. " +
               $"Resonance: {ResonanceBefore} → {ResonanceAfter}{corruptionTag}{checkTag}";
    }

    /// <summary>
    /// Gets a formatted string showing the Resonance change from this cast.
    /// </summary>
    /// <returns>A string in the format "Resonance: 4 → 5 (+1)".</returns>
    public string GetResonanceChange()
    {
        var delta = ResonanceAfter - ResonanceBefore;
        return $"Resonance: {ResonanceBefore} → {ResonanceAfter} (+{delta})";
    }
}
