using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Detailed result of a single combat round with complete dice breakdown.
/// </summary>
/// <remarks>
/// <para>Critical hits occur on natural 10 and double the damage dice.</para>
/// <para>Critical misses occur on natural 1 and always miss.</para>
/// </remarks>
public record CombatRoundResult
{
    // ===== Player Attack Phase =====

    /// <summary>
    /// Gets the player's attack roll result (1d10).
    /// </summary>
    public DiceRollResult AttackRoll { get; init; }

    /// <summary>
    /// Gets the total attack value (roll + Finesse modifier).
    /// </summary>
    public int AttackTotal { get; init; }

    /// <summary>
    /// Gets whether the attack hit the target.
    /// </summary>
    public bool IsHit { get; init; }

    /// <summary>
    /// Gets whether this was a critical hit (natural 10).
    /// </summary>
    public bool IsCriticalHit { get; init; }

    /// <summary>
    /// Gets whether this was a critical miss (natural 1).
    /// </summary>
    public bool IsCriticalMiss { get; init; }

    /// <summary>
    /// Gets the player's damage roll result (if attack hit).
    /// </summary>
    public DiceRollResult? DamageRoll { get; init; }

    /// <summary>
    /// Gets the final damage dealt to the monster.
    /// </summary>
    public int DamageDealt { get; init; }

    // ===== Monster Counterattack Phase =====

    /// <summary>
    /// Gets the monster's counterattack result (if monster survived).
    /// </summary>
    public MonsterCounterAttackResult? MonsterCounterAttack { get; init; }

    // ===== Combat Outcome =====

    /// <summary>
    /// Gets whether the monster was defeated this round.
    /// </summary>
    public bool MonsterDefeated { get; init; }

    /// <summary>
    /// Gets whether the player was defeated this round.
    /// </summary>
    public bool PlayerDefeated { get; init; }

    // ===== Computed Properties =====

    /// <summary>
    /// Gets the total damage received from the monster's counterattack.
    /// </summary>
    public int DamageReceived => MonsterCounterAttack?.DamageDealt ?? 0;

    /// <summary>
    /// Gets whether any damage was dealt to the monster.
    /// </summary>
    public bool DealtDamage => DamageDealt > 0;

    /// <summary>
    /// Gets whether any damage was received from the monster.
    /// </summary>
    public bool ReceivedDamage => DamageReceived > 0;

    /// <summary>
    /// Gets whether either combatant was defeated.
    /// </summary>
    public bool CombatEnded => MonsterDefeated || PlayerDefeated;

    /// <summary>
    /// Gets the success level of the player's attack.
    /// </summary>
    public SuccessLevel AttackSuccessLevel
    {
        get
        {
            if (IsCriticalMiss) return SuccessLevel.CriticalFailure;
            if (IsCriticalHit) return SuccessLevel.CriticalSuccess;
            if (IsHit) return SuccessLevel.Success;
            return SuccessLevel.Failure;
        }
    }

    /// <summary>
    /// Creates a new combat round result.
    /// </summary>
    public CombatRoundResult(
        DiceRollResult attackRoll,
        int attackTotal,
        bool isHit,
        bool isCriticalHit,
        bool isCriticalMiss,
        DiceRollResult? damageRoll,
        int damageDealt,
        MonsterCounterAttackResult? monsterCounterAttack,
        bool monsterDefeated,
        bool playerDefeated)
    {
        AttackRoll = attackRoll;
        AttackTotal = attackTotal;
        IsHit = isHit;
        IsCriticalHit = isCriticalHit;
        IsCriticalMiss = isCriticalMiss;
        DamageRoll = damageRoll;
        DamageDealt = damageDealt;
        MonsterCounterAttack = monsterCounterAttack;
        MonsterDefeated = monsterDefeated;
        PlayerDefeated = playerDefeated;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var attackDesc = IsCriticalHit ? "CRITICAL HIT!" :
                        IsCriticalMiss ? "CRITICAL MISS!" :
                        IsHit ? "Hit" : "Miss";

        var damageDesc = IsHit ? $" for {DamageDealt} damage" : "";
        var counterDesc = MonsterCounterAttack != null ?
            $" | Monster: {(MonsterCounterAttack.IsHit ? $"Hit for {MonsterCounterAttack.DamageDealt}" : "Miss")}" : "";

        return $"Attack: [{AttackRoll.Rolls[0]}] + mod = {AttackTotal} -> {attackDesc}{damageDesc}{counterDesc}";
    }
}

/// <summary>
/// Detailed result of a monster's counterattack with dice breakdown.
/// </summary>
public record MonsterCounterAttackResult
{
    /// <summary>
    /// Gets the monster's attack roll result (1d10).
    /// </summary>
    public DiceRollResult AttackRoll { get; init; }

    /// <summary>
    /// Gets the total attack value (roll + monster's Attack stat).
    /// </summary>
    public int AttackTotal { get; init; }

    /// <summary>
    /// Gets whether the monster's attack hit.
    /// </summary>
    public bool IsHit { get; init; }

    /// <summary>
    /// Gets whether this was a critical hit (natural 10).
    /// </summary>
    public bool IsCriticalHit { get; init; }

    /// <summary>
    /// Gets whether this was a critical miss (natural 1).
    /// </summary>
    public bool IsCriticalMiss { get; init; }

    /// <summary>
    /// Gets the monster's damage roll result (if attack hit).
    /// </summary>
    public DiceRollResult? DamageRoll { get; init; }

    /// <summary>
    /// Gets the final damage dealt to the player.
    /// </summary>
    public int DamageDealt { get; init; }

    /// <summary>
    /// Gets whether the player was defeated by this attack.
    /// </summary>
    public bool PlayerDefeated { get; init; }

    /// <summary>
    /// Gets the success level of the monster's attack.
    /// </summary>
    public SuccessLevel AttackSuccessLevel
    {
        get
        {
            if (IsCriticalMiss) return SuccessLevel.CriticalFailure;
            if (IsCriticalHit) return SuccessLevel.CriticalSuccess;
            if (IsHit) return SuccessLevel.Success;
            return SuccessLevel.Failure;
        }
    }

    /// <summary>
    /// Creates a new monster counterattack result.
    /// </summary>
    public MonsterCounterAttackResult(
        DiceRollResult attackRoll,
        int attackTotal,
        bool isHit,
        bool isCriticalHit,
        bool isCriticalMiss,
        DiceRollResult? damageRoll,
        int damageDealt,
        bool playerDefeated)
    {
        AttackRoll = attackRoll;
        AttackTotal = attackTotal;
        IsHit = isHit;
        IsCriticalHit = isCriticalHit;
        IsCriticalMiss = isCriticalMiss;
        DamageRoll = damageRoll;
        DamageDealt = damageDealt;
        PlayerDefeated = playerDefeated;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var attackDesc = IsCriticalHit ? "CRITICAL HIT!" :
                        IsCriticalMiss ? "CRITICAL MISS!" :
                        IsHit ? "Hit" : "Miss";

        var damageDesc = IsHit ? $" for {DamageDealt} damage" : "";

        return $"Monster Attack: [{AttackRoll.Rolls[0]}] + mod = {AttackTotal} -> {attackDesc}{damageDesc}";
    }
}
