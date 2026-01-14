namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Reasons why a movement attempt failed.
/// </summary>
public enum MovementFailureReason
{
    /// <summary>
    /// No combat grid is currently active.
    /// </summary>
    NoActiveGrid,

    /// <summary>
    /// The entity is not placed on the active grid.
    /// </summary>
    EntityNotOnGrid,

    /// <summary>
    /// The target position is outside the grid boundaries.
    /// </summary>
    OutOfBounds,

    /// <summary>
    /// The target cell is already occupied by another entity.
    /// </summary>
    CellOccupied,

    /// <summary>
    /// The target cell is impassable terrain.
    /// </summary>
    CellImpassable,

    /// <summary>
    /// The entity does not have enough movement points remaining.
    /// </summary>
    InsufficientMovementPoints,

    /// <summary>
    /// The entity is not currently in combat.
    /// </summary>
    NotInCombat
}
