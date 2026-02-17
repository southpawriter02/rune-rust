namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Enumeration of Seiðkona-specific Corruption triggers.
/// Each trigger represents a casting action or Resonance state that may accumulate
/// Corruption through the Heretical Path mechanics.
/// </summary>
/// <remarks>
/// <para>Unlike the Berserkr's deterministic Corruption system (always triggers at 80+ Rage),
/// the Seiðkona employs <strong>probability-based Corruption checks</strong> using a d100 roll
/// against a percentage threshold that scales with Aether Resonance level:</para>
/// <list type="bullet">
/// <item>Resonance 0–4 (Safe): 0% Corruption risk — no check performed</item>
/// <item>Resonance 5–7 (Risky): 5% Corruption risk — <see cref="SeidrBoltLowResonance"/></item>
/// <item>Resonance 8–9 (Dangerous): 15% Corruption risk — <see cref="SeidrBoltHighResonance"/></item>
/// <item>Resonance 10 (Critical): 25% Corruption risk — <see cref="SeidrBoltMaxResonance"/></item>
/// </list>
/// <para>Corruption triggers are evaluated BEFORE resource spending, consistent with the
/// Berserkr pattern established in v0.20.5a. The evaluation uses the current Resonance
/// level (before any gain from the cast).</para>
/// <para>Note: WyrdSight and AetherAttunement do NOT trigger Corruption checks — only
/// abilities that actively channel Aether (and thus build Resonance) carry Corruption risk.</para>
/// </remarks>
public enum SeidkonaCorruptionTrigger
{
    /// <summary>
    /// Casting Seiðr Bolt while Aether Resonance is at 5–7 (Risky range).
    /// Corruption: +1 if d100 roll ≤ 5% threshold. Introduced in v0.20.8a.
    /// </summary>
    SeidrBoltLowResonance = 1,

    /// <summary>
    /// Casting Seiðr Bolt while Aether Resonance is at 8–9 (Dangerous range).
    /// Corruption: +1 if d100 roll ≤ 15% threshold. Introduced in v0.20.8a.
    /// </summary>
    SeidrBoltHighResonance = 2,

    /// <summary>
    /// Casting Seiðr Bolt while Aether Resonance is at 10 (Critical — maximum).
    /// Corruption: +1 if d100 roll ≤ 25% threshold. Introduced in v0.20.8a.
    /// </summary>
    SeidrBoltMaxResonance = 3,

    /// <summary>
    /// Generic trigger for casting any ability while Aether Resonance is elevated.
    /// Used by Tier 2 and Tier 3 abilities that channel Aether (v0.20.8b/c).
    /// Corruption: +1 if d100 roll ≤ threshold at current Resonance level.
    /// </summary>
    CastingAtHighResonance = 4,

    /// <summary>
    /// Activating the Unraveling capstone ability.
    /// Guaranteed 20% Corruption check regardless of current Resonance level.
    /// Corruption: +2 if d100 roll ≤ 20% threshold (v0.20.8c).
    /// </summary>
    CapstoneActivation = 5
}
