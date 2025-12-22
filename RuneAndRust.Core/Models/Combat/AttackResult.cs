using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models.Combat;

/// <summary>
/// Represents the result of an attack resolution.
/// Contains all information needed to apply and display the attack outcome.
/// </summary>
/// <param name="Outcome">The quality of the hit (Fumble, Miss, Glancing, Solid, Critical).</param>
/// <param name="NetSuccesses">The number of successes after subtracting defender's Defense.</param>
/// <param name="RawDamage">Damage before applying soak and modifiers.</param>
/// <param name="FinalDamage">Damage after soak reduction. Minimum 1 on hit, 0 on miss.</param>
/// <param name="IsHit">Whether the attack connected (NetSuccesses > 0).</param>
/// <param name="DamageType">The type of damage dealt (v0.3.6b). Defaults to Physical.</param>
public record AttackResult(
    AttackOutcome Outcome,
    int NetSuccesses,
    int RawDamage,
    int FinalDamage,
    bool IsHit,
    DamageType DamageType = DamageType.Physical
);
