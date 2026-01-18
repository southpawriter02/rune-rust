namespace RuneAndRust.Application.DTOs;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Result of a block defensive action.
/// </summary>
/// <remarks>
/// <para>Block reduces incoming damage by a base percentage (50%) plus
/// the equipped shield's defense bonus.</para>
/// <para>Block does not require or consume a reaction, and can be used
/// as either an action or a reaction.</para>
/// </remarks>
public record BlockResult
{
    /// <summary>
    /// Gets whether the block action was successfully executed.
    /// </summary>
    /// <remarks>
    /// Returns false if the combatant cannot block (e.g., no shield equipped).
    /// </remarks>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the final damage after block reduction.
    /// </summary>
    /// <remarks>
    /// Calculated as: (IncomingDamage * BaseReduction) - ShieldBonus.
    /// Minimum value is 0.
    /// </remarks>
    public int FinalDamage { get; init; }

    /// <summary>
    /// Gets the amount of damage prevented by the block.
    /// </summary>
    /// <remarks>
    /// Calculated as: IncomingDamage - FinalDamage.
    /// </remarks>
    public int DamagePrevented { get; init; }

    /// <summary>
    /// Gets the shield defense bonus applied.
    /// </summary>
    public int ShieldBonus { get; init; }

    /// <summary>
    /// Gets the failure reason if the block was not successful.
    /// </summary>
    public string? FailureReason { get; init; }

    /// <summary>
    /// Creates a successful block result.
    /// </summary>
    /// <param name="finalDamage">The final damage after reduction.</param>
    /// <param name="prevented">The amount of damage prevented.</param>
    /// <param name="shieldBonus">The shield defense bonus applied.</param>
    /// <returns>A successful block result.</returns>
    public static BlockResult Success(int finalDamage, int prevented, int shieldBonus)
        => new()
        {
            IsSuccess = true,
            FinalDamage = finalDamage,
            DamagePrevented = prevented,
            ShieldBonus = shieldBonus
        };

    /// <summary>
    /// Creates a failed block result.
    /// </summary>
    /// <param name="reason">The reason the block failed.</param>
    /// <returns>A failed block result.</returns>
    public static BlockResult Failed(string reason)
        => new() { IsSuccess = false, FailureReason = reason };
}

/// <summary>
/// Result of a dodge defensive action.
/// </summary>
/// <remarks>
/// <para>Dodge is a DEX-based roll against the attacker's attack roll.
/// On success, the attack is completely avoided.</para>
/// <para>Dodge requires and consumes the combatant's reaction for the round
/// and cannot be used while wearing heavy armor.</para>
/// </remarks>
public record DodgeResult
{
    /// <summary>
    /// Gets whether the dodge action was successfully executed.
    /// </summary>
    /// <remarks>
    /// Returns false if the combatant cannot dodge (e.g., no reaction, heavy armor).
    /// This is separate from whether the dodge avoided the attack.
    /// </remarks>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets whether the attack was completely avoided.
    /// </summary>
    /// <remarks>
    /// True when the dodge roll meets or exceeds the attack roll.
    /// Only relevant if IsSuccess is true.
    /// </remarks>
    public bool AvoidedAttack { get; init; }

    /// <summary>
    /// Gets the dodge roll result (1d20 + DEX modifier).
    /// </summary>
    public int DodgeRoll { get; init; }

    /// <summary>
    /// Gets the attack roll that was dodged against.
    /// </summary>
    public int AttackRoll { get; init; }

    /// <summary>
    /// Gets the failure reason if the dodge was not allowed.
    /// </summary>
    public string? FailureReason { get; init; }

    /// <summary>
    /// Creates a successful dodge result where the attack was avoided.
    /// </summary>
    /// <param name="dodgeRoll">The dodge roll result.</param>
    /// <param name="attackRoll">The attack roll that was dodged.</param>
    /// <returns>A dodge result indicating the attack was avoided.</returns>
    public static DodgeResult Success(int dodgeRoll, int attackRoll)
        => new()
        {
            IsSuccess = true,
            AvoidedAttack = true,
            DodgeRoll = dodgeRoll,
            AttackRoll = attackRoll
        };

    /// <summary>
    /// Creates a dodge result where the dodge was attempted but failed to avoid the attack.
    /// </summary>
    /// <param name="dodgeRoll">The dodge roll result.</param>
    /// <param name="attackRoll">The attack roll that was not dodged.</param>
    /// <returns>A dodge result indicating the attack was not avoided.</returns>
    public static DodgeResult Failure(int dodgeRoll, int attackRoll)
        => new()
        {
            IsSuccess = true,
            AvoidedAttack = false,
            DodgeRoll = dodgeRoll,
            AttackRoll = attackRoll
        };

    /// <summary>
    /// Creates a result indicating the dodge was not allowed.
    /// </summary>
    /// <param name="reason">The reason the dodge was not allowed.</param>
    /// <returns>A dodge result indicating it was not allowed.</returns>
    public static DodgeResult NotAllowed(string reason)
        => new() { IsSuccess = false, FailureReason = reason };
}

/// <summary>
/// Result of a counter-attack triggered by a successful parry.
/// </summary>
/// <remarks>
/// A simplified attack result for the parry counter-attack mechanic.
/// </remarks>
public record CounterAttackResult
{
    /// <summary>
    /// Gets whether the counter-attack hit.
    /// </summary>
    public bool IsHit { get; init; }

    /// <summary>
    /// Gets the attack roll result.
    /// </summary>
    public int AttackRoll { get; init; }

    /// <summary>
    /// Gets the damage dealt by the counter-attack.
    /// </summary>
    /// <remarks>
    /// Zero if the counter-attack missed.
    /// </remarks>
    public int Damage { get; init; }

    /// <summary>
    /// Gets whether the counter-attack was a critical hit.
    /// </summary>
    public bool IsCritical { get; init; }

    /// <summary>
    /// Creates a successful counter-attack result.
    /// </summary>
    /// <param name="attackRoll">The attack roll result.</param>
    /// <param name="damage">The damage dealt.</param>
    /// <param name="isCritical">Whether it was a critical hit.</param>
    /// <returns>A counter-attack result.</returns>
    public static CounterAttackResult Hit(int attackRoll, int damage, bool isCritical = false)
        => new() { IsHit = true, AttackRoll = attackRoll, Damage = damage, IsCritical = isCritical };

    /// <summary>
    /// Creates a missed counter-attack result.
    /// </summary>
    /// <param name="attackRoll">The attack roll result.</param>
    /// <returns>A counter-attack result indicating a miss.</returns>
    public static CounterAttackResult Miss(int attackRoll)
        => new() { IsHit = false, AttackRoll = attackRoll, Damage = 0 };
}

/// <summary>
/// Result of a parry defensive action.
/// </summary>
/// <remarks>
/// <para>Parry is a DEX-based roll against the attacker's attack roll plus a DC bonus (+2).
/// On success, the attack is deflected and a counter-attack is made.</para>
/// <para>Parry requires and consumes the combatant's reaction for the round
/// and requires a melee weapon equipped.</para>
/// </remarks>
public record ParryResult
{
    /// <summary>
    /// Gets whether the parry action was successfully executed.
    /// </summary>
    /// <remarks>
    /// Returns false if the combatant cannot parry (e.g., no reaction, no melee weapon).
    /// This is separate from whether the parry deflected the attack.
    /// </remarks>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets whether the attack was deflected.
    /// </summary>
    /// <remarks>
    /// True when the parry roll meets or exceeds the attack roll + DC bonus.
    /// Only relevant if IsSuccess is true.
    /// </remarks>
    public bool Deflected { get; init; }

    /// <summary>
    /// Gets the parry roll result (1d20 + DEX modifier).
    /// </summary>
    public int ParryRoll { get; init; }

    /// <summary>
    /// Gets the difficulty class for the parry (attack roll + DC bonus).
    /// </summary>
    public int DC { get; init; }

    /// <summary>
    /// Gets the counter-attack result (only present if parry succeeded).
    /// </summary>
    /// <remarks>
    /// Null if the parry failed or was not allowed.
    /// </remarks>
    public CounterAttackResult? CounterAttack { get; init; }

    /// <summary>
    /// Gets the failure reason if the parry was not allowed.
    /// </summary>
    public string? FailureReason { get; init; }

    /// <summary>
    /// Creates a successful parry result with a counter-attack.
    /// </summary>
    /// <param name="parryRoll">The parry roll result.</param>
    /// <param name="dc">The difficulty class.</param>
    /// <param name="counter">The counter-attack result.</param>
    /// <returns>A parry result indicating success with counter-attack.</returns>
    public static ParryResult SuccessWithCounter(int parryRoll, int dc, CounterAttackResult counter)
        => new()
        {
            IsSuccess = true,
            Deflected = true,
            ParryRoll = parryRoll,
            DC = dc,
            CounterAttack = counter
        };

    /// <summary>
    /// Creates a parry result where the parry was attempted but failed to deflect.
    /// </summary>
    /// <param name="parryRoll">The parry roll result.</param>
    /// <param name="dc">The difficulty class.</param>
    /// <returns>A parry result indicating the attack was not deflected.</returns>
    public static ParryResult Failure(int parryRoll, int dc)
        => new()
        {
            IsSuccess = true,
            Deflected = false,
            ParryRoll = parryRoll,
            DC = dc
        };

    /// <summary>
    /// Creates a result indicating the parry was not allowed.
    /// </summary>
    /// <param name="reason">The reason the parry was not allowed.</param>
    /// <returns>A parry result indicating it was not allowed.</returns>
    public static ParryResult NotAllowed(string reason)
        => new() { IsSuccess = false, FailureReason = reason };
}
