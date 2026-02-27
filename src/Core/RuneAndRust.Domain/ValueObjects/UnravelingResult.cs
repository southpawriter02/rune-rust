namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of an Unraveling capstone ability execution.
/// Encapsulates the accumulated damage consumed, resource resets, guaranteed Corruption check,
/// and cooldown activation.
/// </summary>
/// <remarks>
/// <para>Unraveling is the Seiðkona's Capstone ability — the ultimate expression of Aetheric power:</para>
/// <list type="bullet">
/// <item>Cost: 5 AP (fixed — Resonance Cascade does NOT reduce Unraveling's cost)</item>
/// <item>Preconditions: Resonance must be exactly 10, Accumulated Aetheric Damage must be &gt; 0,
///   not already used this combat</item>
/// <item>Damage: Releases 100% of Accumulated Aetheric Damage as a single devastating burst</item>
/// <item>Resource Reset: Resonance resets from 10 to 0, Accumulated Damage resets to 0</item>
/// <item>Corruption: Guaranteed 20% check using <c>CapstoneActivation</c> trigger (+2 Corruption if triggered),
///   distinct from the standard Resonance-scaled risk (5%/15%/25%)</item>
/// <item>Cooldown: Once per combat (resets at combat end)</item>
/// </list>
/// <para>The Unraveling represents the culmination of the Seiðkona's Resonance-building playstyle.
/// Throughout combat, the Seiðkona accumulates Aetheric damage through abilities like Seiðr Bolt
/// (2d6 per cast) and Aether Storm (4d6 per cast). When Resonance reaches maximum (10) and
/// sufficient damage has been accumulated, the Unraveling releases ALL accumulated damage in
/// a single, devastating attack that resets the entire resource cycle.</para>
/// <para>The guaranteed 20% Corruption check is actually lower than the standard 25% risk at
/// Resonance 10 — a design choice rewarding bold capstone use. However, the Corruption amount
/// is elevated (+2 instead of the standard +1), reflecting the magnitude of Aether channeled.</para>
/// <para>This result object is returned by the ability service to provide
/// complete information for combat log display, state updates, and player feedback.</para>
/// </remarks>
public sealed record UnravelingResult
{
    /// <summary>
    /// Total Accumulated Aetheric Damage consumed by this Unraveling.
    /// This is the sum of all Aetheric damage dealt by the Seiðkona throughout the combat
    /// before the Unraveling was activated.
    /// </summary>
    public int AccumulatedDamageConsumed { get; init; }

    /// <summary>
    /// Total damage dealt by the Unraveling.
    /// Equal to <see cref="AccumulatedDamageConsumed"/> — the full accumulated payload is released.
    /// </summary>
    public int TotalDamage { get; init; }

    /// <summary>
    /// Aether Resonance value before the Unraveling (always 10 — required precondition).
    /// </summary>
    public int ResonanceBefore { get; init; }

    /// <summary>
    /// Aether Resonance value after the Unraveling (always 0 — fully consumed and reset).
    /// </summary>
    public int ResonanceAfter { get; init; }

    /// <summary>
    /// Whether this cast triggered a Corruption check.
    /// Always true for Unraveling — the Corruption check is guaranteed regardless of any other factors.
    /// </summary>
    public bool CorruptionCheckPerformed { get; init; }

    /// <summary>
    /// Whether the guaranteed Corruption check actually resulted in Corruption accumulation.
    /// True when the d100 roll fell within the 20% threshold (roll ≤ 20).
    /// </summary>
    public bool CorruptionTriggered { get; init; }

    /// <summary>
    /// Descriptive reason for the Corruption check outcome.
    /// Always includes roll context since the check is always performed.
    /// </summary>
    public string? CorruptionReason { get; init; }

    /// <summary>
    /// The d100 roll result for the guaranteed Corruption check.
    /// Always non-zero since the check is always performed.
    /// </summary>
    public int CorruptionRoll { get; init; }

    /// <summary>
    /// The Corruption risk percentage threshold (always 20% for Unraveling).
    /// This is a fixed capstone rate, not scaled to Resonance like standard abilities.
    /// </summary>
    public int CorruptionRiskPercent { get; init; }

    /// <summary>
    /// The actual AP cost paid for this Unraveling (always 5 AP).
    /// Unraveling is the only Seiðkona ability that is immune to Resonance Cascade reduction.
    /// </summary>
    public int ApCostPaid { get; init; }

    /// <summary>
    /// Whether the per-combat cooldown was activated after this Unraveling.
    /// Always true — Unraveling can only be used once per combat encounter.
    /// </summary>
    public bool CooldownActivated { get; init; }

    /// <summary>
    /// Gets a detailed breakdown of the damage delivered by the Unraveling.
    /// </summary>
    /// <returns>
    /// A formatted string describing the accumulated damage release, e.g.:
    /// "The Unraveling: 42 accumulated Aetheric damage released"
    /// </returns>
    public string GetDamageBreakdown()
    {
        return $"The Unraveling: {AccumulatedDamageConsumed} accumulated Aetheric damage released";
    }

    /// <summary>
    /// Gets a complete result string suitable for combat log display.
    /// Includes damage, Resonance reset, Corruption outcome, and cooldown activation.
    /// </summary>
    /// <returns>A formatted combat log entry for this Unraveling execution.</returns>
    public string GetDescription()
    {
        var corruptionTag = CorruptionTriggered
            ? $" [CORRUPTION +2 (d100={CorruptionRoll} ≤ {CorruptionRiskPercent}%)]"
            : $" [Corruption check: d100={CorruptionRoll} vs {CorruptionRiskPercent}% — safe]";

        return $"The Unraveling unleashes {TotalDamage} accumulated Aetheric damage! " +
               $"Resonance: {ResonanceBefore} → {ResonanceAfter}{corruptionTag}. " +
               $"Cooldown activated (once per combat).";
    }

    /// <summary>
    /// Gets a formatted string showing the Resonance reset from this Unraveling.
    /// </summary>
    /// <returns>A string in the format "Resonance: 10 → 0 (reset)".</returns>
    public string GetResonanceChange()
    {
        return $"Resonance: {ResonanceBefore} → {ResonanceAfter} (reset)";
    }
}
