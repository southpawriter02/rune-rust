using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Events;

/// <summary>
/// Published when a combatant is pushed by another combatant via an opposed STR check.
/// </summary>
/// <remarks>
/// <para>Push events occur when one combatant actively pushes another using abilities
/// or actions that trigger an opposed STR check.</para>
/// <para>The target wins ties, so they are only pushed if the pusher's roll exceeds the target's roll.</para>
/// </remarks>
/// <param name="PusherId">The unique identifier of the combatant performing the push.</param>
/// <param name="PusherName">The display name of the pusher for logging purposes.</param>
/// <param name="TargetId">The unique identifier of the combatant being pushed.</param>
/// <param name="TargetName">The display name of the target for logging purposes.</param>
/// <param name="StartPosition">The target's position before the push.</param>
/// <param name="EndPosition">The target's position after the push.</param>
/// <param name="Direction">The direction the target was pushed.</param>
/// <param name="CellsMoved">The number of cells the target was moved.</param>
/// <param name="PusherRoll">The pusher's STR check roll (1d20 + STR modifier).</param>
/// <param name="TargetRoll">The target's STR check roll (1d20 + STR modifier).</param>
public record CombatantPushedEvent(
    Guid PusherId,
    string PusherName,
    Guid TargetId,
    string TargetName,
    GridPosition StartPosition,
    GridPosition EndPosition,
    MovementDirection Direction,
    int CellsMoved,
    int PusherRoll,
    int TargetRoll);

/// <summary>
/// Published when a combatant successfully resists a push attempt.
/// </summary>
/// <remarks>
/// <para>Resistance occurs when the target's STR check roll is greater than or equal to
/// the pusher's roll (target wins ties).</para>
/// </remarks>
/// <param name="PusherId">The unique identifier of the combatant attempting the push.</param>
/// <param name="PusherName">The display name of the pusher for logging purposes.</param>
/// <param name="TargetId">The unique identifier of the combatant who resisted.</param>
/// <param name="TargetName">The display name of the target for logging purposes.</param>
/// <param name="Position">The target's position (unchanged).</param>
/// <param name="PusherRoll">The pusher's STR check roll.</param>
/// <param name="TargetRoll">The target's STR check roll.</param>
public record PushResistedEvent(
    Guid PusherId,
    string PusherName,
    Guid TargetId,
    string TargetName,
    GridPosition Position,
    int PusherRoll,
    int TargetRoll);

/// <summary>
/// Published when a combatant is knocked back (forced movement without opposed check).
/// </summary>
/// <remarks>
/// <para>Knockback is forced movement that does not allow an opposed STR check.
/// Common triggers include critical hits and certain special abilities.</para>
/// </remarks>
/// <param name="SourceId">The unique identifier of the source causing the knockback (may be a combatant or effect).</param>
/// <param name="SourceName">The display name of the knockback source for logging purposes.</param>
/// <param name="TargetId">The unique identifier of the combatant being knocked back.</param>
/// <param name="TargetName">The display name of the target for logging purposes.</param>
/// <param name="StartPosition">The target's position before the knockback.</param>
/// <param name="EndPosition">The target's position after the knockback.</param>
/// <param name="Direction">The direction the target was knocked.</param>
/// <param name="CellsMoved">The number of cells the target was moved.</param>
/// <param name="TriggerReason">The reason for the knockback (e.g., "Critical Hit", "Ability Effect").</param>
public record CombatantKnockedBackEvent(
    Guid SourceId,
    string SourceName,
    Guid TargetId,
    string TargetName,
    GridPosition StartPosition,
    GridPosition EndPosition,
    MovementDirection Direction,
    int CellsMoved,
    string TriggerReason);

/// <summary>
/// Published when a push or knockback is blocked by a wall or other entity.
/// </summary>
/// <remarks>
/// <para>Blocking occurs when the destination cell is impassable (wall) or occupied
/// by another entity.</para>
/// </remarks>
/// <param name="TargetId">The unique identifier of the combatant who was blocked.</param>
/// <param name="TargetName">The display name of the target for logging purposes.</param>
/// <param name="StartPosition">The target's starting position.</param>
/// <param name="BlockedAtPosition">The position where movement was blocked.</param>
/// <param name="Direction">The direction of attempted movement.</param>
/// <param name="CellsMovedBeforeBlock">The number of cells moved before being blocked.</param>
/// <param name="BlockedBy">Description of what caused the block (e.g., "wall", "another combatant").</param>
public record MovementBlockedEvent(
    Guid TargetId,
    string TargetName,
    GridPosition StartPosition,
    GridPosition BlockedAtPosition,
    MovementDirection Direction,
    int CellsMovedBeforeBlock,
    string BlockedBy);

/// <summary>
/// Published when a combatant enters a hazardous cell.
/// </summary>
/// <remarks>
/// <para>This event is triggered whenever a combatant moves into, is pushed into,
/// or is knocked back into a cell containing an environmental hazard.</para>
/// </remarks>
/// <param name="CombatantId">The unique identifier of the combatant entering the hazard.</param>
/// <param name="CombatantName">The display name of the combatant for logging purposes.</param>
/// <param name="HazardType">The type of hazard entered.</param>
/// <param name="HazardName">The display name of the hazard.</param>
/// <param name="Position">The grid position of the hazard.</param>
/// <param name="WasPushed">Whether the combatant was pushed/knocked into the hazard.</param>
/// <param name="PusherName">The name of the combatant who pushed them, if applicable.</param>
public record HazardEnteredEvent(
    Guid CombatantId,
    string CombatantName,
    HazardType HazardType,
    string HazardName,
    GridPosition Position,
    bool WasPushed,
    string? PusherName);

/// <summary>
/// Published when an environmental hazard deals damage to a combatant.
/// </summary>
/// <remarks>
/// <para>Hazard damage can occur on entry (when first entering the hazard cell)
/// or per-turn (at the start of each turn while remaining in the hazard).</para>
/// </remarks>
/// <param name="CombatantId">The unique identifier of the combatant taking damage.</param>
/// <param name="CombatantName">The display name of the combatant for logging purposes.</param>
/// <param name="HazardType">The type of hazard dealing damage.</param>
/// <param name="HazardName">The display name of the hazard.</param>
/// <param name="Position">The grid position of the hazard.</param>
/// <param name="DamageDealt">The amount of damage dealt.</param>
/// <param name="DamageType">The type of damage (e.g., "fire", "piercing").</param>
/// <param name="DamageDice">The dice notation that was rolled (e.g., "2d6").</param>
/// <param name="WasEntryDamage">Whether this was entry damage or per-turn damage.</param>
/// <param name="RemainingHealth">The combatant's health after the damage.</param>
public record HazardDamageDealtEvent(
    Guid CombatantId,
    string CombatantName,
    HazardType HazardType,
    string HazardName,
    GridPosition Position,
    int DamageDealt,
    string DamageType,
    string DamageDice,
    bool WasEntryDamage,
    int RemainingHealth);

/// <summary>
/// Published when a hazard applies a status effect to a combatant.
/// </summary>
/// <remarks>
/// <para>Some hazards apply status effects in addition to dealing damage.
/// Examples include burning from lava, bleeding from spikes, and prone from pits.</para>
/// </remarks>
/// <param name="CombatantId">The unique identifier of the combatant receiving the status effect.</param>
/// <param name="CombatantName">The display name of the combatant for logging purposes.</param>
/// <param name="HazardType">The type of hazard applying the effect.</param>
/// <param name="HazardName">The display name of the hazard.</param>
/// <param name="StatusEffectId">The identifier of the applied status effect.</param>
/// <param name="Position">The grid position where the effect was applied.</param>
public record HazardStatusEffectAppliedEvent(
    Guid CombatantId,
    string CombatantName,
    HazardType HazardType,
    string HazardName,
    string StatusEffectId,
    GridPosition Position);

/// <summary>
/// Published when a combatant falls into a pit and becomes trapped.
/// </summary>
/// <remarks>
/// <para>Trapped combatants must use a climb action or receive help to escape.
/// They cannot simply walk out of the hazard.</para>
/// </remarks>
/// <param name="CombatantId">The unique identifier of the trapped combatant.</param>
/// <param name="CombatantName">The display name of the combatant for logging purposes.</param>
/// <param name="HazardType">The type of hazard (typically Pit).</param>
/// <param name="HazardName">The display name of the hazard.</param>
/// <param name="Position">The grid position of the pit.</param>
/// <param name="FallDamage">The amount of fall damage dealt.</param>
public record CombatantTrappedEvent(
    Guid CombatantId,
    string CombatantName,
    HazardType HazardType,
    string HazardName,
    GridPosition Position,
    int FallDamage);

/// <summary>
/// Published when a hazard degrades a combatant's armor (acid pools).
/// </summary>
/// <remarks>
/// <para>Acid pools can degrade armor over time, reducing its effectiveness.
/// This event tracks when such degradation occurs.</para>
/// </remarks>
/// <param name="CombatantId">The unique identifier of the combatant whose armor is degraded.</param>
/// <param name="CombatantName">The display name of the combatant for logging purposes.</param>
/// <param name="HazardType">The type of hazard (typically AcidPool).</param>
/// <param name="Position">The grid position of the hazard.</param>
/// <param name="ArmorName">The name of the degraded armor piece.</param>
/// <param name="DegradationAmount">The amount of defense reduction.</param>
public record ArmorDegradedEvent(
    Guid CombatantId,
    string CombatantName,
    HazardType HazardType,
    GridPosition Position,
    string ArmorName,
    int DegradationAmount);

/// <summary>
/// Published when hazard tick damage is processed at the start of a round.
/// </summary>
/// <remarks>
/// <para>This event summarizes all per-turn hazard damage dealt to combatants
/// who remain in hazardous cells at the start of a new round.</para>
/// </remarks>
/// <param name="Round">The combat round number.</param>
/// <param name="CombatantsAffected">The number of combatants who took tick damage.</param>
/// <param name="TotalDamageDealt">The total damage dealt across all combatants.</param>
public record HazardTickProcessedEvent(
    int Round,
    int CombatantsAffected,
    int TotalDamageDealt);

/// <summary>
/// Published when a combatant is killed by environmental hazard damage.
/// </summary>
/// <remarks>
/// <para>This event indicates that hazard damage reduced a combatant's health to zero or below.</para>
/// </remarks>
/// <param name="CombatantId">The unique identifier of the killed combatant.</param>
/// <param name="CombatantName">The display name of the combatant for logging purposes.</param>
/// <param name="HazardType">The type of hazard that killed the combatant.</param>
/// <param name="HazardName">The display name of the hazard.</param>
/// <param name="Position">The grid position where the combatant died.</param>
/// <param name="FinalDamage">The amount of damage that killed the combatant.</param>
public record CombatantKilledByHazardEvent(
    Guid CombatantId,
    string CombatantName,
    HazardType HazardType,
    string HazardName,
    GridPosition Position,
    int FinalDamage);

/// <summary>
/// Published when a critical hit triggers knockback.
/// </summary>
/// <remarks>
/// <para>Critical hits can trigger a 1-cell knockback away from the attacker.
/// This event is raised before the knockback itself occurs.</para>
/// </remarks>
/// <param name="AttackerId">The unique identifier of the attacker.</param>
/// <param name="AttackerName">The display name of the attacker for logging purposes.</param>
/// <param name="TargetId">The unique identifier of the target.</param>
/// <param name="TargetName">The display name of the target for logging purposes.</param>
/// <param name="AttackRoll">The critical attack roll value.</param>
public record CriticalKnockbackTriggeredEvent(
    Guid AttackerId,
    string AttackerName,
    Guid TargetId,
    string TargetName,
    int AttackRoll);
