using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for generating branching dungeon paths.
/// </summary>
public interface IBranchGeneratorService
{
    /// <summary>
    /// Decides branching for a room's potential exits.
    /// </summary>
    /// <param name="position">The room's position.</param>
    /// <param name="potentialExits">Available exit directions.</param>
    /// <param name="hasRoomAt">Function to check if a room exists at a position.</param>
    /// <returns>Branch decisions for each exit.</returns>
    BranchDecision DecideBranching(
        Position3D position,
        IEnumerable<Direction> potentialExits,
        Func<Position3D, bool> hasRoomAt);

    /// <summary>
    /// Generates the content type for a dead end room.
    /// </summary>
    /// <param name="position">The dead end position.</param>
    /// <param name="difficulty">The room's difficulty rating.</param>
    /// <returns>The type of special content for the dead end.</returns>
    DeadEndContent GenerateDeadEndContent(Position3D position, DifficultyRating difficulty);
}
