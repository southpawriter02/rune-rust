// ═══════════════════════════════════════════════════════════════════════════════
// CounterShieldResult.cs
// Immutable value object capturing the result of a Counter-Shield reaction,
// including the damage roll and attacker information.
// Version: 0.20.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Captures the result of a Counter-Shield reaction triggered by a Skjaldmær.
/// </summary>
/// <remarks>
/// <para>
/// Counter-Shield is a Tier 2 reaction ability that triggers automatically when
/// the Skjaldmær successfully blocks a melee attack. It deals 1d6 damage to the
/// attacker. The reaction does not trigger on ranged attacks.
/// </para>
/// <para>
/// This is an immutable value object created via the <see cref="Create"/> factory
/// method after the damage roll has been resolved.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = CounterShieldResult.Create(skjaldmaerId, attackerId, damageRoll: 4);
/// // result.DamageRoll == 4, result.ShouldApplyDamage() == true
/// </code>
/// </example>
public sealed record CounterShieldResult
{
    /// <summary>Minimum damage roll for Counter-Shield (1d6).</summary>
    public const int MinDamageRoll = 1;

    /// <summary>Maximum damage roll for Counter-Shield (1d6).</summary>
    public const int MaxDamageRoll = 6;

    /// <summary>Gets the ID of the Skjaldmær who triggered Counter-Shield.</summary>
    public Guid SkjaldmaerId { get; init; }

    /// <summary>Gets the ID of the attacker who was countered.</summary>
    public Guid AttackerId { get; init; }

    /// <summary>Gets the damage rolled (1d6).</summary>
    public int DamageRoll { get; init; }

    /// <summary>Gets the timestamp when the counter-shield was triggered.</summary>
    public DateTime ExecutedAt { get; init; }

    /// <summary>
    /// Creates a new Counter-Shield result after the damage roll has been resolved.
    /// </summary>
    /// <param name="skjaldmaerId">ID of the Skjaldmær triggering the reaction.</param>
    /// <param name="attackerId">ID of the melee attacker being countered.</param>
    /// <param name="damageRoll">The 1d6 damage roll result (1–6).</param>
    /// <returns>A new CounterShieldResult with the specified values.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="damageRoll"/> is outside the 1–6 range.
    /// </exception>
    public static CounterShieldResult Create(
        Guid skjaldmaerId,
        Guid attackerId,
        int damageRoll)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(damageRoll, MinDamageRoll);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(damageRoll, MaxDamageRoll);

        return new CounterShieldResult
        {
            SkjaldmaerId = skjaldmaerId,
            AttackerId = attackerId,
            DamageRoll = damageRoll,
            ExecutedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Determines whether counter-shield damage should be applied.
    /// Always returns true — counter-shield damage is guaranteed on trigger.
    /// </summary>
    /// <returns>Always true; counter-shield damage is unconditional once triggered.</returns>
    public bool ShouldApplyDamage() => true;

    /// <inheritdoc />
    public override string ToString() =>
        $"Counter-Shield: {SkjaldmaerId} dealt {DamageRoll} damage to {AttackerId}";
}
