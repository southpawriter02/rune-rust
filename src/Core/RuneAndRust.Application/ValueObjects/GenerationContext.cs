using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.ValueObjects;

/// <summary>
/// Context for room generation including seed and modifiers.
/// </summary>
/// <param name="Position">The 3D position being generated.</param>
/// <param name="Seed">The generation seed for this room.</param>
/// <param name="BiomeId">The biome for template selection.</param>
/// <param name="DifficultyModifier">The depth-based difficulty multiplier.</param>
/// <param name="GuaranteedExitDirection">Direction that must have an exit.</param>
public readonly record struct GenerationContext(
    Position3D Position,
    int Seed,
    string BiomeId,
    float DifficultyModifier,
    Direction? GuaranteedExitDirection = null)
{
    /// <summary>
    /// Gets the depth (Z-level) from the position.
    /// </summary>
    public int Depth => Position.Z;

    /// <summary>
    /// Creates a Random instance seeded for this context.
    /// </summary>
    public Random CreateRandom() => new(Seed);

    /// <summary>
    /// Creates a position-specific seed by combining position and base seed.
    /// </summary>
    /// <param name="position">The 3D position.</param>
    /// <param name="baseSeed">The dungeon's base seed.</param>
    /// <returns>A deterministic seed unique to this position.</returns>
    public static int CreatePositionSeed(Position3D position, int baseSeed) =>
        HashCode.Combine(position.X, position.Y, position.Z, baseSeed);
}
