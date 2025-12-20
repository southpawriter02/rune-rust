using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models.Combat;

/// <summary>
/// Represents an enemy's intended action during their turn.
/// Produced by EnemyAIService, consumed by CombatService.
/// </summary>
/// <param name="Type">The type of action to perform.</param>
/// <param name="SourceId">The acting combatant's ID.</param>
/// <param name="TargetId">The target combatant's ID (null for Defend/Flee/Pass).</param>
/// <param name="AttackType">The attack variant if Type is Attack (Light/Standard/Heavy).</param>
/// <param name="FlavorText">Optional AAM-VOICE compliant narrative description.</param>
public record CombatAction(
    ActionType Type,
    Guid SourceId,
    Guid? TargetId,
    AttackType? AttackType = null,
    string? FlavorText = null
);
