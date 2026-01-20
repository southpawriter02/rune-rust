using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Tracking;

/// <summary>
/// Represents the result of a tactical move decision for a monster in a group (v0.10.4c).
/// </summary>
/// <remarks>
/// <para>
/// GroupMoveDecision is returned by <see cref="Interfaces.IMonsterGroupService.DetermineMove"/>
/// to indicate what movement action a group member should take based on the group's tactics.
/// </para>
/// <para>
/// Decision types:
/// <list type="bullet">
///   <item><description><see cref="GroupMoveDecisionType.NoGroup"/> - Monster is not in a group</description></item>
///   <item><description><see cref="GroupMoveDecisionType.NoTarget"/> - No valid target available</description></item>
///   <item><description><see cref="GroupMoveDecisionType.NoAction"/> - No tactical move needed</description></item>
///   <item><description><see cref="GroupMoveDecisionType.MoveTo"/> - Move to a specific position</description></item>
///   <item><description><see cref="GroupMoveDecisionType.HoldPosition"/> - Stay in current position (e.g., Ambush)</description></item>
/// </list>
/// </para>
/// <para>
/// The <see cref="Tactic"/> property indicates which tactic generated this decision,
/// useful for logging and debugging tactical behavior.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Get move decision from service
/// var decision = groupService.DetermineMove(monster);
///
/// // Handle based on decision type
/// switch (decision.Type)
/// {
///     case GroupMoveDecisionType.MoveTo:
///         MoveMonster(monster, decision.TargetPosition!.Value);
///         Log($"Moving for {decision.Tactic} tactic");
///         break;
///     case GroupMoveDecisionType.HoldPosition:
///         Log($"Holding position for {decision.Tactic} tactic");
///         break;
///     case GroupMoveDecisionType.NoGroup:
///         // Use default AI movement
///         break;
/// }
/// </code>
/// </example>
public readonly record struct GroupMoveDecision
{
    /// <summary>
    /// Gets the type of move decision.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Determines how the caller should interpret the decision:
    /// <list type="bullet">
    ///   <item><description><see cref="GroupMoveDecisionType.MoveTo"/> - Use <see cref="TargetPosition"/></description></item>
    ///   <item><description><see cref="GroupMoveDecisionType.HoldPosition"/> - Stay in place</description></item>
    ///   <item><description>Others - Fall back to default AI behavior</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public GroupMoveDecisionType Type { get; }

    /// <summary>
    /// Gets the target grid position for MoveTo decisions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Only valid when <see cref="Type"/> is <see cref="GroupMoveDecisionType.MoveTo"/>.
    /// Null for all other decision types.
    /// </para>
    /// </remarks>
    public GridPosition? TargetPosition { get; }

    /// <summary>
    /// Gets the tactic that generated this decision.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Indicates which <see cref="GroupTactic"/> from the group definition
    /// resulted in this movement decision. Null for non-group decisions.
    /// </para>
    /// <para>
    /// Useful for logging and debugging tactical behavior.
    /// </para>
    /// </remarks>
    public GroupTactic? Tactic { get; }

    /// <summary>
    /// Private constructor for factory methods.
    /// </summary>
    private GroupMoveDecision(GroupMoveDecisionType type, GridPosition? targetPosition, GroupTactic? tactic)
    {
        Type = type;
        TargetPosition = targetPosition;
        Tactic = tactic;
    }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this decision indicates the monster should move.
    /// </summary>
    public bool ShouldMove => Type == GroupMoveDecisionType.MoveTo && TargetPosition.HasValue;

    /// <summary>
    /// Gets whether this decision indicates the monster should hold position.
    /// </summary>
    public bool ShouldHold => Type == GroupMoveDecisionType.HoldPosition;

    /// <summary>
    /// Gets whether this is a valid tactical decision (not NoGroup/NoTarget/NoAction).
    /// </summary>
    public bool IsTacticalDecision => Type == GroupMoveDecisionType.MoveTo || Type == GroupMoveDecisionType.HoldPosition;

    /// <summary>
    /// Gets whether the caller should fall back to default AI behavior.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns true for <see cref="GroupMoveDecisionType.NoGroup"/>,
    /// <see cref="GroupMoveDecisionType.NoTarget"/>, and
    /// <see cref="GroupMoveDecisionType.NoAction"/>.
    /// </para>
    /// </remarks>
    public bool ShouldUseDefaultBehavior => !IsTacticalDecision;

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a decision indicating the monster should move to a position.
    /// </summary>
    /// <param name="position">The target position to move to.</param>
    /// <param name="tactic">The tactic that generated this decision.</param>
    /// <returns>A MoveTo decision with the specified position and tactic.</returns>
    /// <example>
    /// <code>
    /// var flankPosition = GetFlankingPosition(target, monster);
    /// return GroupMoveDecision.MoveTo(flankPosition, GroupTactic.Flank);
    /// </code>
    /// </example>
    public static GroupMoveDecision MoveTo(GridPosition position, GroupTactic tactic)
    {
        return new GroupMoveDecision(GroupMoveDecisionType.MoveTo, position, tactic);
    }

    /// <summary>
    /// Creates a decision indicating the monster is not in a group.
    /// </summary>
    /// <returns>A NoGroup decision.</returns>
    /// <remarks>
    /// <para>
    /// Returned when <see cref="Interfaces.IMonsterGroupService.DetermineMove"/> is called
    /// for a monster that is not registered in any group. The caller should use
    /// the monster's default AI behavior instead.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!IsInGroup(monster))
    /// {
    ///     return GroupMoveDecision.NoGroup();
    /// }
    /// </code>
    /// </example>
    public static GroupMoveDecision NoGroup()
    {
        return new GroupMoveDecision(GroupMoveDecisionType.NoGroup, null, null);
    }

    /// <summary>
    /// Creates a decision indicating no valid target is available.
    /// </summary>
    /// <returns>A NoTarget decision.</returns>
    /// <remarks>
    /// <para>
    /// Returned when the group has no valid targets to attack.
    /// The caller should use the monster's default AI behavior.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (!possibleTargets.Any())
    /// {
    ///     return GroupMoveDecision.NoTarget();
    /// }
    /// </code>
    /// </example>
    public static GroupMoveDecision NoTarget()
    {
        return new GroupMoveDecision(GroupMoveDecisionType.NoTarget, null, null);
    }

    /// <summary>
    /// Creates a decision indicating no tactical move is needed.
    /// </summary>
    /// <returns>A NoAction decision.</returns>
    /// <remarks>
    /// <para>
    /// Returned when none of the group's tactics produced a move decision.
    /// The caller should use the monster's default AI behavior.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // No tactic produced a movement decision
    /// return GroupMoveDecision.NoAction();
    /// </code>
    /// </example>
    public static GroupMoveDecision NoAction()
    {
        return new GroupMoveDecision(GroupMoveDecisionType.NoAction, null, null);
    }

    /// <summary>
    /// Creates a decision indicating the monster should hold its position.
    /// </summary>
    /// <param name="tactic">The tactic that requires holding position (e.g., Ambush).</param>
    /// <returns>A HoldPosition decision with the specified tactic.</returns>
    /// <remarks>
    /// <para>
    /// Typically used by the <see cref="GroupTactic.Ambush"/> tactic
    /// to keep monsters in position until a target enters range.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Ambush: hold position until target enters trigger range
    /// if (!IsTargetInAmbushRange(target))
    /// {
    ///     return GroupMoveDecision.HoldPosition(GroupTactic.Ambush);
    /// }
    /// </code>
    /// </example>
    public static GroupMoveDecision HoldPosition(GroupTactic tactic)
    {
        return new GroupMoveDecision(GroupMoveDecisionType.HoldPosition, null, tactic);
    }

    // ═══════════════════════════════════════════════════════════════
    // UTILITY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of this decision.
    /// </summary>
    /// <returns>A string describing the decision type and details.</returns>
    public override string ToString()
    {
        return Type switch
        {
            GroupMoveDecisionType.MoveTo => $"MoveTo({TargetPosition}) [{Tactic}]",
            GroupMoveDecisionType.HoldPosition => $"HoldPosition [{Tactic}]",
            GroupMoveDecisionType.NoGroup => "NoGroup",
            GroupMoveDecisionType.NoTarget => "NoTarget",
            GroupMoveDecisionType.NoAction => "NoAction",
            _ => $"Unknown({Type})"
        };
    }
}

/// <summary>
/// Defines the types of move decisions for group tactics (v0.10.4c).
/// </summary>
/// <remarks>
/// <para>
/// Used by <see cref="GroupMoveDecision"/> to indicate what action
/// a monster should take based on group tactical evaluation.
/// </para>
/// </remarks>
public enum GroupMoveDecisionType
{
    /// <summary>
    /// The monster is not part of any group.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returned when the monster is not registered with any active group.
    /// The caller should use the monster's default AI behavior.
    /// </para>
    /// </remarks>
    NoGroup = 0,

    /// <summary>
    /// No valid target is available for the group.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returned when there are no valid targets to attack.
    /// The caller should use the monster's default AI behavior.
    /// </para>
    /// </remarks>
    NoTarget = 1,

    /// <summary>
    /// No tactical move is needed at this time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returned when none of the group's tactics produced a move decision,
    /// or the monster is already in an optimal position.
    /// </para>
    /// </remarks>
    NoAction = 2,

    /// <summary>
    /// The monster should move to a specific position.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The target position is available in <see cref="GroupMoveDecision.TargetPosition"/>.
    /// The tactic that generated this decision is in <see cref="GroupMoveDecision.Tactic"/>.
    /// </para>
    /// </remarks>
    MoveTo = 3,

    /// <summary>
    /// The monster should hold its current position.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used by tactics like <see cref="GroupTactic.Ambush"/> to keep
    /// monsters in position until conditions are met.
    /// </para>
    /// </remarks>
    HoldPosition = 4
}
