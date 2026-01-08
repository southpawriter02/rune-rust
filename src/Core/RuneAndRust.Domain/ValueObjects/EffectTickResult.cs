namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of ticking a status effect at the start of a turn.
/// </summary>
/// <remarks>
/// <para>Captures DoT damage, HoT healing, and expiration for combat log and UI.</para>
/// </remarks>
/// <param name="EffectId">The effect definition ID.</param>
/// <param name="EffectName">The effect display name.</param>
/// <param name="DamageDealt">Damage dealt by DoT (0 if none).</param>
/// <param name="HealingDone">Healing from HoT (0 if none).</param>
/// <param name="Expired">Whether the effect expired this tick.</param>
/// <param name="RemainingDuration">Turns remaining (null if not turn-based).</param>
public readonly record struct EffectTickResult(
    string EffectId,
    string EffectName,
    int DamageDealt,
    int HealingDone,
    bool Expired,
    int? RemainingDuration)
{
    /// <summary>
    /// Creates a tick result for a DoT effect.
    /// </summary>
    public static EffectTickResult WithDamage(
        string effectId,
        string effectName,
        int damage,
        bool expired,
        int? remaining) =>
        new(effectId, effectName, damage, 0, expired, remaining);

    /// <summary>
    /// Creates a tick result for a HoT effect.
    /// </summary>
    public static EffectTickResult WithHealing(
        string effectId,
        string effectName,
        int healing,
        bool expired,
        int? remaining) =>
        new(effectId, effectName, 0, healing, expired, remaining);

    /// <summary>
    /// Creates a tick result for a non-DoT/HoT effect.
    /// </summary>
    public static EffectTickResult Ticked(
        string effectId,
        string effectName,
        bool expired,
        int? remaining) =>
        new(effectId, effectName, 0, 0, expired, remaining);

    /// <summary>
    /// Whether this tick caused any damage or healing.
    /// </summary>
    public bool HadEffect => DamageDealt > 0 || HealingDone > 0;

    /// <summary>
    /// Returns a display string for this tick result.
    /// </summary>
    public override string ToString()
    {
        if (DamageDealt > 0)
            return $"{EffectName}: {DamageDealt} damage{(Expired ? " (expired)" : "")}";
        if (HealingDone > 0)
            return $"{EffectName}: {HealingDone} healing{(Expired ? " (expired)" : "")}";
        if (Expired)
            return $"{EffectName} expired";
        return $"{EffectName}: {RemainingDuration} turns remaining";
    }
}
