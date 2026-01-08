namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of processing effects at the start of a combatant's turn.
/// </summary>
/// <param name="TotalDamage">Total DoT damage dealt.</param>
/// <param name="TotalHealing">Total HoT healing done.</param>
/// <param name="ExpiredEffects">Effect IDs that expired this turn.</param>
/// <param name="CanAct">Whether the combatant can take actions.</param>
/// <param name="MustFlee">Whether the combatant is forced to flee.</param>
/// <param name="PreventionReason">Message explaining why actions are prevented.</param>
public readonly record struct TurnStartEffectResult(
    int TotalDamage,
    int TotalHealing,
    IReadOnlyList<string> ExpiredEffects,
    bool CanAct,
    bool MustFlee,
    string? PreventionReason)
{
    /// <summary>Creates a result for a combatant with no effects.</summary>
    public static TurnStartEffectResult NoEffects() =>
        new(0, 0, Array.Empty<string>(), true, false, null);

    /// <summary>Creates a result where the combatant can act normally.</summary>
    public static TurnStartEffectResult CanActNormally(
        int damage,
        int healing,
        IReadOnlyList<string> expired) =>
        new(damage, healing, expired, true, false, null);

    /// <summary>Creates a result where actions are prevented.</summary>
    public static TurnStartEffectResult ActionsPrevented(
        int damage,
        int healing,
        IReadOnlyList<string> expired,
        string reason,
        bool mustFlee = false) =>
        new(damage, healing, expired, false, mustFlee, reason);

    /// <summary>Whether any DoT/HoT occurred.</summary>
    public bool HadTickEffects => TotalDamage > 0 || TotalHealing > 0;

    /// <summary>Whether any effects expired.</summary>
    public bool HadExpirations => ExpiredEffects.Count > 0;
}
