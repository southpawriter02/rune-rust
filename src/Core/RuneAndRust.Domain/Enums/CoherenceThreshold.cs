namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the stages of Coherence for the Arcanist specialization.
/// </summary>
/// <remarks>
/// <para>
/// Coherence is a reality-stability meter representing how well the Arcanist
/// maintains focus on magical reality. Low coherence risks cascades; high coherence
/// enables apotheosis.
/// </para>
/// <para>
/// Threshold Ranges (based on Coherence 0-100):
/// <list type="bullet">
/// <item>Destabilized: 0-20 coherence — Dangerous! 25% cascade risk, -2 spell power</item>
/// <item>Unstable: 21-40 coherence — Risky, 10% cascade risk, -1 spell power</item>
/// <item>Balanced: 41-60 coherence — Ideal state, no risk, 5% crit chance</item>
/// <item>Focused: 61-80 coherence — Enhanced power, +2 spell power, 10% crit</item>
/// <item>Apotheosis: 81-100 coherence — Ultimate state, +5 spell power, 20% crit, 10 stress/turn</item>
/// </list>
/// </para>
/// </remarks>
public enum CoherenceThreshold
{
    /// <summary>
    /// Destabilized state (0-20 coherence).
    /// </summary>
    /// <remarks>
    /// Reality is fragmenting. 25% cascade risk per cast. Spell power -2.
    /// Casting in this state is extremely dangerous and should be avoided.
    /// </remarks>
    Destabilized = 0,

    /// <summary>
    /// Unstable state (21-40 coherence).
    /// </summary>
    /// <remarks>
    /// Reality flickering. 10% cascade risk per cast. Spell power -1.
    /// Meditation recommended to restore stability.
    /// </remarks>
    Unstable = 1,

    /// <summary>
    /// Balanced state (41-60 coherence).
    /// </summary>
    /// <remarks>
    /// Ideal magical state. No cascade risk. Normal spell power.
    /// 5% critical cast chance. Safe for sustained casting.
    /// </remarks>
    Balanced = 2,

    /// <summary>
    /// Focused state (61-80 coherence).
    /// </summary>
    /// <remarks>
    /// Enhanced magical focus. No risk. +2 spell power, +10% crit.
    /// Reduced mana costs. Optimal combat performance.
    /// </remarks>
    Focused = 3,

    /// <summary>
    /// Apotheosis state (81-100 coherence).
    /// </summary>
    /// <remarks>
    /// Ultimate magical state. +5 spell power, +20% crit, ultimate abilities unlocked.
    /// Costs 10 stress per turn to maintain. Auto-exits if stress reaches 100.
    /// Cannot voluntarily exit during combat.
    /// </remarks>
    Apotheosis = 4
}
