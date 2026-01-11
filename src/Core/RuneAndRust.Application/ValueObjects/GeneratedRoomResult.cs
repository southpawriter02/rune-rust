using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.ValueObjects;

/// <summary>
/// Result of procedural room generation.
/// </summary>
/// <param name="Room">The generated room entity.</param>
/// <param name="GeneratedExits">Directions with exits (not yet connected).</param>
/// <param name="TemplateId">The template used for generation.</param>
/// <param name="BiomeId">The biome applied to the room.</param>
/// <param name="DifficultyModifier">The depth-based difficulty multiplier.</param>
public readonly record struct GeneratedRoomResult(
    Room Room,
    IReadOnlyList<Direction> GeneratedExits,
    string TemplateId,
    string BiomeId,
    float DifficultyModifier)
{
    /// <summary>
    /// Checks if the generation produced any exits.
    /// </summary>
    public bool HasExits => GeneratedExits.Count > 0;

    /// <summary>
    /// Checks if a specific direction has an exit.
    /// </summary>
    public bool HasExit(Direction direction) => GeneratedExits.Contains(direction);
}
