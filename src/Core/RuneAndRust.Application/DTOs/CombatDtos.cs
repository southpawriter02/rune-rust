using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// DTO for combat round results displayed to the player.
/// </summary>
public record CombatRoundResultDto(
    DiceRollDto AttackRoll,
    int AttackTotal,
    bool IsHit,
    bool IsCriticalHit,
    bool IsCriticalMiss,
    DiceRollDto? DamageRoll,
    int DamageDealt,
    MonsterCounterAttackResultDto? MonsterCounterAttack,
    bool MonsterDefeated,
    bool PlayerDefeated,
    string AttackSuccessLevel,
    string? Descriptor = null)
{
    /// <summary>
    /// Creates a DTO from a domain combat round result.
    /// </summary>
    public static CombatRoundResultDto FromDomainResult(
        CombatRoundResult result,
        string? descriptor = null)
    {
        return new CombatRoundResultDto(
            DiceRollDto.FromDomainResult(result.AttackRoll),
            result.AttackTotal,
            result.IsHit,
            result.IsCriticalHit,
            result.IsCriticalMiss,
            result.DamageRoll != null ? DiceRollDto.FromDomainResult(result.DamageRoll.Value) : null,
            result.DamageDealt,
            result.MonsterCounterAttack != null
                ? MonsterCounterAttackResultDto.FromDomainResult(result.MonsterCounterAttack)
                : null,
            result.MonsterDefeated,
            result.PlayerDefeated,
            result.AttackSuccessLevel.ToString(),
            descriptor);
    }

    /// <summary>
    /// Gets the total damage received from the monster's counterattack.
    /// </summary>
    public int DamageReceived => MonsterCounterAttack?.DamageDealt ?? 0;

    /// <summary>
    /// Gets whether combat ended this round.
    /// </summary>
    public bool CombatEnded => MonsterDefeated || PlayerDefeated;
}

/// <summary>
/// DTO for monster counterattack results.
/// </summary>
public record MonsterCounterAttackResultDto(
    DiceRollDto AttackRoll,
    int AttackTotal,
    bool IsHit,
    bool IsCriticalHit,
    bool IsCriticalMiss,
    DiceRollDto? DamageRoll,
    int DamageDealt,
    bool PlayerDefeated,
    string AttackSuccessLevel)
{
    /// <summary>
    /// Creates a DTO from a domain monster counterattack result.
    /// </summary>
    public static MonsterCounterAttackResultDto FromDomainResult(MonsterCounterAttackResult result)
    {
        return new MonsterCounterAttackResultDto(
            DiceRollDto.FromDomainResult(result.AttackRoll),
            result.AttackTotal,
            result.IsHit,
            result.IsCriticalHit,
            result.IsCriticalMiss,
            result.DamageRoll != null ? DiceRollDto.FromDomainResult(result.DamageRoll.Value) : null,
            result.DamageDealt,
            result.PlayerDefeated,
            result.AttackSuccessLevel.ToString());
    }
}
