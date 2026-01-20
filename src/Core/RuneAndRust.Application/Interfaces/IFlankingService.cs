using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for flanking calculations in tactical combat.
/// </summary>
/// <remarks>
/// Flanking provides attack bonuses when allies coordinate attacks from opposite sides
/// of an enemy. Attacking from behind provides additional damage bonuses.
/// </remarks>
public interface IFlankingService
{
    /// <summary>
    /// Checks if an attacker is flanking a target.
    /// </summary>
    /// <param name="attackerId">The attacker's entity ID.</param>
    /// <param name="targetId">The target's entity ID.</param>
    /// <returns>A FlankingResult describing the flanking status and bonuses.</returns>
    FlankingResult CheckFlanking(Guid attackerId, Guid targetId);

    /// <summary>
    /// Checks if an attacker at a position is flanking a target.
    /// </summary>
    /// <param name="attackerPosition">The attacker's position.</param>
    /// <param name="targetPosition">The target's position.</param>
    /// <param name="targetFacing">The direction the target is facing.</param>
    /// <param name="allyPositions">Positions of allies who could provide flanking.</param>
    /// <returns>A FlankingResult describing the flanking status and bonuses.</returns>
    FlankingResult CheckFlanking(
        GridPosition attackerPosition,
        GridPosition targetPosition,
        FacingDirection targetFacing,
        IEnumerable<GridPosition> allyPositions);

    /// <summary>
    /// Gets the flanking attack bonus for an attack.
    /// </summary>
    /// <param name="attackerId">The attacker's entity ID.</param>
    /// <param name="targetId">The target's entity ID.</param>
    /// <returns>The attack bonus from flanking (0 if not flanking).</returns>
    int GetFlankingAttackBonus(Guid attackerId, Guid targetId);

    /// <summary>
    /// Gets the flanking damage bonus for an attack.
    /// </summary>
    /// <param name="attackerId">The attacker's entity ID.</param>
    /// <param name="targetId">The target's entity ID.</param>
    /// <returns>The damage bonus from flanking (0 if not flanking).</returns>
    int GetFlankingDamageBonus(Guid attackerId, Guid targetId);

    /// <summary>
    /// Gets all positions that would provide flanking against a target.
    /// </summary>
    /// <param name="targetId">The target's entity ID.</param>
    /// <returns>Positions where an attacker would have flanking advantage.</returns>
    IEnumerable<GridPosition> GetFlankingPositions(Guid targetId);

    /// <summary>
    /// Gets positions where allies are currently providing flanking.
    /// </summary>
    /// <param name="attackerId">The attacker's entity ID.</param>
    /// <param name="targetId">The target's entity ID.</param>
    /// <returns>Positions of allies that are flanking the target.</returns>
    IEnumerable<GridPosition> GetAllyFlankingPositions(Guid attackerId, Guid targetId);

    /// <summary>
    /// Checks if attacking from behind (opposite of target's facing).
    /// </summary>
    /// <param name="attackerId">The attacker's entity ID.</param>
    /// <param name="targetId">The target's entity ID.</param>
    /// <returns>True if the attacker is behind the target.</returns>
    bool IsAttackingFromBehind(Guid attackerId, Guid targetId);

    /// <summary>
    /// Checks if attacking from the side (perpendicular to target's facing).
    /// </summary>
    /// <param name="attackerId">The attacker's entity ID.</param>
    /// <param name="targetId">The target's entity ID.</param>
    /// <returns>True if the attacker is on the target's side.</returns>
    bool IsAttackingFromSide(Guid attackerId, Guid targetId);
}

/// <summary>
/// Result of a flanking check.
/// </summary>
/// <param name="IsFlanking">Whether a flanking bonus applies.</param>
/// <param name="FlankingType">The type of flanking (None, Side, Behind, Flanked).</param>
/// <param name="AttackBonus">Attack roll bonus from flanking.</param>
/// <param name="DamageBonus">Damage bonus from flanking.</param>
/// <param name="FlankingAllyId">The ally providing the flank, if any.</param>
/// <param name="Message">A descriptive message for the flanking status.</param>
public readonly record struct FlankingResult(
    bool IsFlanking,
    FlankingType FlankingType,
    int AttackBonus,
    int DamageBonus,
    Guid? FlankingAllyId,
    string Message)
{
    /// <summary>
    /// A result indicating no flanking advantage.
    /// </summary>
    public static FlankingResult None { get; } = new(
        false, FlankingType.None, 0, 0, null, "No flanking advantage.");

    /// <summary>
    /// Creates a result for when entities are not adjacent.
    /// </summary>
    public static FlankingResult NotAdjacent { get; } = new(
        false, FlankingType.None, 0, 0, null, "Must be adjacent to flank.");

    /// <summary>
    /// Creates a result for when no active grid exists.
    /// </summary>
    public static FlankingResult NoGrid { get; } = new(
        false, FlankingType.None, 0, 0, null, "No active combat grid.");

    /// <summary>
    /// Creates a result for when entity is not on grid.
    /// </summary>
    public static FlankingResult NotOnGrid { get; } = new(
        false, FlankingType.None, 0, 0, null, "Entity not on grid.");
}
