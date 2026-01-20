using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.ValueObjects;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for procedural room generation.
/// </summary>
public interface IRoomGeneratorService
{
    /// <summary>
    /// Generates a new room at the specified position.
    /// </summary>
    /// <param name="position">The 3D position for the new room.</param>
    /// <param name="biomeId">The biome identifier for template selection.</param>
    /// <param name="seed">Seed for deterministic generation.</param>
    /// <returns>A result containing the generated room and metadata.</returns>
    GeneratedRoomResult GenerateRoom(Position3D position, string biomeId, int seed);

    /// <summary>
    /// Gets an existing room or generates a new one at the specified position.
    /// </summary>
    /// <param name="dungeon">The dungeon to check/add the room to.</param>
    /// <param name="position">The 3D position to check.</param>
    /// <param name="fromDirection">The direction the player came from (for guaranteed return exit).</param>
    /// <returns>The existing or newly generated room.</returns>
    Room GetOrGenerateRoom(Dungeon dungeon, Position3D position, Direction? fromDirection = null);

    /// <summary>
    /// Determines which exits a generated room should have.
    /// </summary>
    /// <param name="template">The selected room template.</param>
    /// <param name="seed">Seed for deterministic generation.</param>
    /// <param name="guaranteedExit">A direction that must have an exit (for return path).</param>
    /// <returns>Collection of directions that should have exits.</returns>
    IEnumerable<Direction> DetermineExits(
        RoomTemplate template,
        int seed,
        Direction? guaranteedExit = null);

    /// <summary>
    /// Calculates the difficulty modifier for a given depth.
    /// </summary>
    /// <param name="depth">The Z-level depth.</param>
    /// <returns>A multiplier applied to monster stats and loot quality.</returns>
    float GetDepthDifficultyModifier(int depth);

    /// <summary>
    /// Determines the biome for a given depth based on transition rules.
    /// </summary>
    /// <param name="depth">The Z-level depth.</param>
    /// <param name="seed">Seed for deterministic biome selection.</param>
    /// <returns>The biome ID for this depth.</returns>
    string DetermineBiomeForDepth(int depth, int seed);
}
