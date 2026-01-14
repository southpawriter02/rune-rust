using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Result of starting combat with grid initialization.
/// </summary>
/// <param name="Success">Whether combat started successfully.</param>
/// <param name="Grid">The initialized combat grid.</param>
/// <param name="PlayerPosition">The player's starting position.</param>
/// <param name="MonsterPositions">Map of monster IDs to their positions.</param>
/// <param name="Message">Human-readable result message.</param>
public readonly record struct CombatStartResult(
    bool Success,
    CombatGrid? Grid,
    GridPosition PlayerPosition,
    IReadOnlyDictionary<Guid, GridPosition> MonsterPositions,
    string Message)
{
    /// <summary>
    /// Creates a success result.
    /// </summary>
    public static CombatStartResult Succeed(
        CombatGrid grid,
        GridPosition playerPosition,
        IReadOnlyDictionary<Guid, GridPosition> monsterPositions,
        string message = "Combat begins!") =>
        new(true, grid, playerPosition, monsterPositions, message);

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    public static CombatStartResult Fail(string message) =>
        new(false, null, default, new Dictionary<Guid, GridPosition>(), message);
}
