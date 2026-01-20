using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Validates and fixes thematic coherence in generated dungeons.
/// Handles tag conflicts, path validation, and glitch injection.
/// </summary>
public interface ICoherenceValidator
{
    /// <summary>
    /// Validates and fixes coherence issues in a generated sector.
    /// </summary>
    /// <param name="sector">The sector topology.</param>
    /// <param name="rooms">Dictionary mapping node IDs to rooms.</param>
    /// <param name="random">Random instance for deterministic operations.</param>
    void ValidateAndFix(Sector sector, IReadOnlyDictionary<Guid, Room> rooms, Random random);

    /// <summary>
    /// Validates that a path exists from start to boss.
    /// </summary>
    /// <param name="sector">The sector to validate.</param>
    /// <returns>True if path exists, false otherwise.</returns>
    bool ValidatePath(Sector sector);

    /// <summary>
    /// Resolves tag conflicts in a room (e.g., Hot + Cold = Steam).
    /// </summary>
    /// <param name="room">The room to fix.</param>
    /// <param name="random">Random instance for conflict resolution.</param>
    void ResolveTagConflicts(Room room, Random random);
}
