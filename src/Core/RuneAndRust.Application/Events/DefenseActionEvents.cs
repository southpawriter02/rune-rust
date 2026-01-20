namespace RuneAndRust.Application.Events;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Published when a combatant successfully blocks an incoming attack.
/// </summary>
/// <remarks>
/// <para>Block reduces damage by 50% plus the equipped shield's defense bonus.</para>
/// <para>Block does not consume a reaction and can be used multiple times per round.</para>
/// </remarks>
/// <param name="DefenderId">The unique identifier of the blocking combatant.</param>
/// <param name="IncomingDamage">The original damage before block reduction.</param>
/// <param name="FinalDamage">The final damage after block reduction.</param>
/// <param name="DamagePrevented">The amount of damage prevented by the block.</param>
public record BlockedEvent(
    Guid DefenderId,
    int IncomingDamage,
    int FinalDamage,
    int DamagePrevented);

/// <summary>
/// Published when a combatant attempts to dodge an incoming attack.
/// </summary>
/// <remarks>
/// <para>Dodge is a DEX-based roll against the attacker's attack roll.</para>
/// <para>A successful dodge completely avoids the attack.</para>
/// <para>Dodge consumes the combatant's reaction for the round.</para>
/// </remarks>
/// <param name="DefenderId">The unique identifier of the dodging combatant.</param>
/// <param name="DodgeRoll">The result of the dodge roll (1d20 + DEX modifier).</param>
/// <param name="AttackRoll">The attacker's attack roll that was dodged against.</param>
/// <param name="Success">Whether the dodge successfully avoided the attack.</param>
public record DodgeAttemptedEvent(
    Guid DefenderId,
    int DodgeRoll,
    int AttackRoll,
    bool Success);

/// <summary>
/// Published when a combatant successfully parries an incoming attack.
/// </summary>
/// <remarks>
/// <para>Parry is a DEX-based roll against the attacker's attack roll + 2 (DC bonus).</para>
/// <para>A successful parry deflects the attack and triggers a counter-attack.</para>
/// <para>Parry consumes the combatant's reaction for the round.</para>
/// </remarks>
/// <param name="DefenderId">The unique identifier of the parrying combatant.</param>
/// <param name="AttackerId">The unique identifier of the attacker who was parried.</param>
/// <param name="ParryRoll">The result of the parry roll (1d20 + DEX modifier).</param>
/// <param name="DC">The difficulty class (attack roll + DC bonus).</param>
/// <param name="CounterDamage">The damage dealt by the counter-attack.</param>
public record ParrySuccessEvent(
    Guid DefenderId,
    Guid AttackerId,
    int ParryRoll,
    int DC,
    int CounterDamage);

/// <summary>
/// Published when a combatant fails to parry an incoming attack.
/// </summary>
/// <remarks>
/// <para>The parry attempt was made but failed to meet the DC.</para>
/// <para>The combatant's reaction is still consumed.</para>
/// </remarks>
/// <param name="DefenderId">The unique identifier of the combatant who attempted to parry.</param>
/// <param name="ParryRoll">The result of the parry roll (1d20 + DEX modifier).</param>
/// <param name="DC">The difficulty class that was not met.</param>
public record ParryFailedEvent(
    Guid DefenderId,
    int ParryRoll,
    int DC);

/// <summary>
/// Published when a combatant's reaction is consumed.
/// </summary>
/// <remarks>
/// <para>Reactions are consumed by defensive actions like Dodge and Parry.</para>
/// <para>Once consumed, no more reaction-based defenses can be used until the reaction is reset.</para>
/// </remarks>
/// <param name="CombatantId">The unique identifier of the combatant whose reaction was used.</param>
/// <param name="ActionType">The type of defensive action that consumed the reaction.</param>
public record ReactionUsedEvent(
    Guid CombatantId,
    DefenseActionType ActionType);

/// <summary>
/// Published when a combatant's reaction is reset at the start of their turn.
/// </summary>
/// <remarks>
/// <para>Reactions are reset at the start of each combatant's turn.</para>
/// </remarks>
/// <param name="CombatantId">The unique identifier of the combatant whose reaction was reset.</param>
public record ReactionResetEvent(
    Guid CombatantId);
