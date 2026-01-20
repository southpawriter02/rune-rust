using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service interface for handling entity movement on the combat grid.
/// </summary>
public interface IMovementService
{
    /// <summary>
    /// Attempts to move an entity in the specified direction.
    /// </summary>
    /// <param name="entityId">The entity to move.</param>
    /// <param name="direction">The direction to move.</param>
    /// <returns>A result indicating success/failure with details.</returns>
    MovementResult Move(Guid entityId, MovementDirection direction);

    /// <summary>
    /// Gets the remaining movement points for an entity.
    /// </summary>
    /// <param name="entityId">The entity ID.</param>
    /// <returns>Remaining movement points.</returns>
    int GetRemainingMovement(Guid entityId);

    /// <summary>
    /// Resets movement points for a specific entity (start of turn).
    /// </summary>
    /// <param name="entityId">The entity ID.</param>
    void ResetMovement(Guid entityId);

    /// <summary>
    /// Resets movement points for all entities (new turn).
    /// </summary>
    void ResetAllMovement();

    /// <summary>
    /// Checks if an entity can move in a direction.
    /// </summary>
    /// <param name="entityId">The entity ID.</param>
    /// <param name="direction">The direction to check.</param>
    /// <returns><c>true</c> if the move is valid; otherwise, <c>false</c>.</returns>
    bool CanMove(Guid entityId, MovementDirection direction);

    /// <summary>
    /// Gets all valid movement directions for an entity.
    /// </summary>
    /// <param name="entityId">The entity ID.</param>
    /// <returns>Collection of valid directions.</returns>
    IEnumerable<MovementDirection> GetValidDirections(Guid entityId);

    /// <summary>
    /// Gets the movement point cost for a direction.
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <returns>Movement point cost (2 for cardinal, 3 for diagonal).</returns>
    int GetMovementCost(MovementDirection direction);
}

/// <summary>
/// Result of a movement attempt.
/// </summary>
/// <param name="Success">Whether the move succeeded.</param>
/// <param name="OldPosition">The position before moving (null if failed).</param>
/// <param name="NewPosition">The position after moving (null if failed).</param>
/// <param name="MovementPointsUsed">Points used for this move.</param>
/// <param name="MovementPointsRemaining">Remaining points after move.</param>
/// <param name="Message">Human-readable result message.</param>
/// <param name="FailureReason">The reason for failure (null if successful).</param>
/// <param name="TerrainDamage">Damage taken from hazardous terrain (v0.5.2a).</param>
/// <param name="OpportunityAttacks">Opportunity attacks triggered by this move (v0.5.3b).</param>
public readonly record struct MovementResult(
    bool Success,
    GridPosition? OldPosition,
    GridPosition? NewPosition,
    int MovementPointsUsed,
    int MovementPointsRemaining,
    string Message,
    MovementFailureReason? FailureReason,
    TerrainDamageResult? TerrainDamage = null,
    IReadOnlyList<OpportunityAttackResult>? OpportunityAttacks = null)
{

    /// <summary>
    /// Creates a success result.
    /// </summary>
    public static MovementResult Succeed(
        GridPosition oldPos,
        GridPosition newPos,
        int pointsUsed,
        int pointsRemaining,
        string message,
        TerrainDamageResult? terrainDamage = null) =>
        new(true, oldPos, newPos, pointsUsed, pointsRemaining, message, null, terrainDamage);

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    public static MovementResult Fail(MovementFailureReason reason, string message) =>
        new(false, null, null, 0, 0, message, reason, null);
}
