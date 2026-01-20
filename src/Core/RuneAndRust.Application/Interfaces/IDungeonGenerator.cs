using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Orchestrates the dungeon generation pipeline.
/// </summary>
public interface IDungeonGenerator
{
    /// <summary>
    /// Generates a complete dungeon with rooms, connections, and monsters.
    /// </summary>
    /// <param name="name">The dungeon name.</param>
    /// <param name="biome">The biome type.</param>
    /// <param name="difficulty">The difficulty tier.</param>
    /// <param name="roomCount">Target number of rooms.</param>
    /// <param name="seed">Optional seed for reproducible generation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A fully generated dungeon.</returns>
    Task<Dungeon> GenerateDungeonAsync(
        string name,
        Biome biome,
        DifficultyTier difficulty,
        int roomCount = 15,
        int? seed = null,
        CancellationToken ct = default);

    /// <summary>
    /// Generates a dungeon synchronously.
    /// </summary>
    Dungeon GenerateDungeon(
        string name,
        Biome biome,
        DifficultyTier difficulty,
        int roomCount = 15,
        int? seed = null);
}
