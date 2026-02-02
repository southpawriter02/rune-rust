// ═══════════════════════════════════════════════════════════════════════════════
// CoherenceConfigurationSection.cs
// Configuration records for Coherence resource system (Arcanist specialization).
// Version: 0.18.4f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Configuration section for the Coherence resource system.
/// </summary>
/// <remarks>
/// <para>
/// Coherence is a reality-stability meter for the Arcanist specialization
/// with both power bonuses at high levels and dangerous instability at low levels.
/// Unlike Rage and Momentum, Coherence starts at a balanced midpoint (50).
/// </para>
/// </remarks>
public record CoherenceConfigurationSection
{
    /// <summary>
    /// Gets the maximum coherence value.
    /// </summary>
    /// <remarks>Default: 100.</remarks>
    public int MaxValue { get; init; } = 100;

    /// <summary>
    /// Gets the minimum coherence value.
    /// </summary>
    /// <remarks>Default: 0.</remarks>
    public int MinValue { get; init; } = 0;

    /// <summary>
    /// Gets the default starting coherence value.
    /// </summary>
    /// <remarks>Default: 50 (Balanced state). Unique among specialization resources.</remarks>
    public int DefaultValue { get; init; } = 50;

    /// <summary>
    /// Gets the coherence gained from meditation action.
    /// </summary>
    /// <remarks>Default: 20 per action. Only available outside combat.</remarks>
    public int MeditationGain { get; init; } = 20;

    /// <summary>
    /// Gets the coherence gained from successful spell cast.
    /// </summary>
    /// <remarks>Default: 5 per completed spell.</remarks>
    public int CastGain { get; init; } = 5;

    /// <summary>
    /// Gets the coherence gained per turn while channeling.
    /// </summary>
    /// <remarks>Default: 3 per turn of maintained channeling.</remarks>
    public int ChannelGainPerTurn { get; init; } = 3;

    /// <summary>
    /// Gets the stress cost per turn while in Apotheosis state.
    /// </summary>
    /// <remarks>
    /// Default: 10 stress per turn. Apotheosis auto-exits if stress reaches 100.
    /// </remarks>
    public int ApotheosisStressCost { get; init; } = 10;

    /// <summary>
    /// Gets the list of coherence threshold configurations.
    /// </summary>
    /// <remarks>
    /// Contains 5 thresholds: Destabilized, Unstable, Balanced, Focused, Apotheosis.
    /// </remarks>
    public IReadOnlyList<CoherenceThresholdConfig> Thresholds { get; init; } = [];

    /// <summary>
    /// Gets the dictionary of coherence source configurations.
    /// </summary>
    /// <remarks>
    /// Keys: successfulCast, controlledChannel, meditation, stabilityField.
    /// </remarks>
    public IReadOnlyDictionary<string, ResourceSourceConfig> Sources { get; init; } =
        new Dictionary<string, ResourceSourceConfig>();

    /// <summary>
    /// Gets the threshold configuration for a given coherence value.
    /// </summary>
    /// <param name="coherenceValue">The current coherence value (0-100).</param>
    /// <returns>The threshold config for that value, or null if not found.</returns>
    public CoherenceThresholdConfig? GetThresholdForValue(int coherenceValue)
    {
        return Thresholds.FirstOrDefault(t =>
            coherenceValue >= t.MinValue && coherenceValue <= t.MaxValue);
    }
}

/// <summary>
/// Configuration for a single coherence threshold tier.
/// </summary>
/// <remarks>
/// Each threshold defines the spell power bonuses and cascade risks at that level.
/// Low coherence is dangerous (cascade risk), high coherence is powerful (Apotheosis).
/// </remarks>
public record CoherenceThresholdConfig
{
    /// <summary>
    /// Gets the threshold name.
    /// </summary>
    /// <remarks>
    /// One of: Destabilized, Unstable, Balanced, Focused, Apotheosis.
    /// </remarks>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the minimum coherence value for this threshold.
    /// </summary>
    public int MinValue { get; init; }

    /// <summary>
    /// Gets the maximum coherence value for this threshold.
    /// </summary>
    public int MaxValue { get; init; }

    /// <summary>
    /// Gets the spell power bonus at this threshold.
    /// </summary>
    /// <remarks>
    /// Negative at low coherence (Destabilized: -2, Unstable: -1),
    /// positive at high coherence (Focused: +2, Apotheosis: +5).
    /// </remarks>
    public int SpellPowerBonus { get; init; }

    /// <summary>
    /// Gets the critical cast chance at this threshold.
    /// </summary>
    /// <remarks>
    /// 0% at low coherence, 5% at Balanced, 10% at Focused, 20% at Apotheosis.
    /// </remarks>
    public int CriticalCastChance { get; init; }

    /// <summary>
    /// Gets the cascade risk percentage at this threshold.
    /// </summary>
    /// <remarks>
    /// 25% at Destabilized, 10% at Unstable, 0% at Balanced and above.
    /// Cascade can cause self-damage, stress, corruption, or spell disruption.
    /// </remarks>
    public int CascadeRisk { get; init; }

    /// <summary>
    /// Gets whether ultimate abilities are enabled at this threshold.
    /// </summary>
    /// <remarks>Only true at Apotheosis (81-100).</remarks>
    public bool UltimateAbilitiesEnabled { get; init; }
}
