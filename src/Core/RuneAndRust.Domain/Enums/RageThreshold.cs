namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the stages of Rage for the Berserker specialization.
/// </summary>
/// <remarks>
/// <para>
/// Rage is a volatile combat resource powered by violence and pain.
/// Unlike controlled resources, Rage grows from aggression and provides
/// both offensive and defensive bonuses, but with a cost: forced
/// engagement at the highest threshold.
/// </para>
/// <para>
/// Threshold Ranges (based on Rage 0-100):
/// <list type="bullet">
/// <item>Calm: 0-20 rage — Clear mind, minimal bonuses</item>
/// <item>Simmering: 21-40 rage — Building power</item>
/// <item>Burning: 41-60 rage — Significant aggression</item>
/// <item>BerserkFury: 61-80 rage — Overwhelming power</item>
/// <item>FrenzyBeyondReason: 81-100 rage — Ultimate fury with consequences</item>
/// </list>
/// </para>
/// </remarks>
public enum RageThreshold
{
    /// <summary>
    /// Calm state (0-20 rage).
    /// </summary>
    /// <remarks>
    /// No significant bonuses. Mind is relatively clear.
    /// </remarks>
    Calm = 0,

    /// <summary>
    /// Simmering state (21-40 rage).
    /// </summary>
    /// <remarks>
    /// Building power. Minor damage and soak bonuses.
    /// </remarks>
    Simmering = 1,

    /// <summary>
    /// Burning state (41-60 rage).
    /// </summary>
    /// <remarks>
    /// Significant aggression. Moderate bonuses and intimidation effects.
    /// </remarks>
    Burning = 2,

    /// <summary>
    /// Berserk Fury state (61-80 rage).
    /// </summary>
    /// <remarks>
    /// Overwhelming power. Strong bonuses and fear resistance.
    /// Must attack nearest enemy.
    /// </remarks>
    BerserkFury = 3,

    /// <summary>
    /// Frenzy Beyond Reason state (81-100 rage).
    /// </summary>
    /// <remarks>
    /// Ultimate fury with severe consequences. Maximum bonuses,
    /// fear immunity, forced aggression, and party stress reduction.
    /// </remarks>
    FrenzyBeyondReason = 4
}
