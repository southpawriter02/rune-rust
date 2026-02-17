namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the current state of the Resonance Cascade passive ability.
/// Evaluates whether the Cascade is active and provides AP cost reduction calculations.
/// </summary>
/// <remarks>
/// <para>Resonance Cascade is the Seiðkona's Tier 2 passive modifier ability:</para>
/// <list type="bullet">
/// <item>Type: Passive (no AP cost, no activation required)</item>
/// <item>Trigger: Automatically active when Aether Resonance is 5 or higher</item>
/// <item>Effect: Reduces all Seiðkona ability AP costs by 1 (minimum 1)</item>
/// <item>Exclusion: Does NOT affect the Unraveling capstone (special handling in v0.20.8c)</item>
/// <item>Resonance: Does NOT build Resonance (passive ability)</item>
/// <item>Corruption: Does NOT trigger Corruption checks (no Aether channeled)</item>
/// </list>
/// <para>The Cascade creates a strategic "feedback loop" — higher Resonance means cheaper abilities,
/// which enables more casts per turn, which builds more Resonance, which increases Corruption risk.
/// This reward-risk tension is the core Seiðkona gameplay mechanic.</para>
/// <para>Cost reduction examples at Resonance 5+:</para>
/// <list type="bullet">
/// <item>Seiðr Bolt: 1 AP → 1 AP (already at minimum, no change)</item>
/// <item>Wyrd Sight: 2 AP → 1 AP (reduced by 1)</item>
/// <item>Fate's Thread: 2 AP → 1 AP (reduced by 1)</item>
/// <item>Weave Disruption: 3 AP → 2 AP (reduced by 1)</item>
/// </list>
/// </remarks>
public sealed record ResonanceCascadeState
{
    /// <summary>
    /// Resonance level at which Cascade becomes active.
    /// </summary>
    private const int CascadeActivationThreshold = 5;

    /// <summary>
    /// Amount of AP cost reduction when Cascade is active.
    /// </summary>
    private const int CascadeReduction = 1;

    /// <summary>
    /// Minimum AP cost enforced after Cascade reduction. No ability can cost less than this.
    /// </summary>
    private const int MinCost = 1;

    /// <summary>
    /// Whether the Resonance Cascade is currently active.
    /// True when the ability is unlocked AND current Resonance is at or above
    /// the activation threshold (5).
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// The AP cost reduction applied when Cascade is active.
    /// Always 1 when active, 0 when inactive.
    /// </summary>
    public int CostReduction { get; init; }

    /// <summary>
    /// The minimum AP cost enforced after Cascade reduction.
    /// </summary>
    public int MinimumApCost { get; init; } = MinCost;

    /// <summary>
    /// The player's current Aether Resonance at time of evaluation.
    /// </summary>
    public int CurrentResonance { get; init; }

    /// <summary>
    /// Whether the Resonance Cascade ability is unlocked by the player.
    /// </summary>
    public bool IsUnlocked { get; init; }

    /// <summary>
    /// Evaluates the Resonance Cascade state for the given Resonance level and unlock status.
    /// </summary>
    /// <param name="currentResonance">The player's current Aether Resonance value (0-10).</param>
    /// <param name="cascadeUnlocked">Whether the player has unlocked the Resonance Cascade ability.</param>
    /// <returns>A <see cref="ResonanceCascadeState"/> reflecting the current Cascade status.</returns>
    public static ResonanceCascadeState Evaluate(int currentResonance, bool cascadeUnlocked)
    {
        var isActive = cascadeUnlocked && currentResonance >= CascadeActivationThreshold;

        return new ResonanceCascadeState
        {
            IsActive = isActive,
            CostReduction = isActive ? CascadeReduction : 0,
            MinimumApCost = MinCost,
            CurrentResonance = currentResonance,
            IsUnlocked = cascadeUnlocked
        };
    }

    /// <summary>
    /// Calculates the effective AP cost after applying Cascade reduction.
    /// Returns the base cost unchanged if Cascade is not active.
    /// </summary>
    /// <param name="baseCost">The base AP cost of the ability before Cascade reduction.</param>
    /// <returns>
    /// The reduced AP cost (minimum 1) if Cascade is active;
    /// the base cost unchanged if Cascade is inactive.
    /// </returns>
    public int GetReducedCost(int baseCost)
    {
        if (!IsActive)
            return baseCost;

        return Math.Max(baseCost - CostReduction, MinimumApCost);
    }

    /// <summary>
    /// Gets a formatted description of the Cascade state for combat log display.
    /// </summary>
    /// <returns>A description string including activation status and Resonance context.</returns>
    public string GetDescription()
    {
        if (!IsUnlocked)
            return "Resonance Cascade: not unlocked";

        if (!IsActive)
            return $"Resonance Cascade: inactive (Resonance {CurrentResonance}/{CascadeActivationThreshold} required)";

        return $"Resonance Cascade: ACTIVE — all Seiðkona abilities cost {CostReduction} less AP " +
               $"(min {MinimumApCost}). Resonance {CurrentResonance}/10.";
    }

    /// <summary>
    /// Gets a short status string for UI display.
    /// </summary>
    /// <returns>A compact status string.</returns>
    public string GetStatusString()
    {
        if (!IsUnlocked)
            return "Cascade: locked";

        return IsActive
            ? $"Cascade: ACTIVE (-{CostReduction} AP, Resonance {CurrentResonance})"
            : $"Cascade: inactive (need Resonance {CascadeActivationThreshold}+)";
    }
}
