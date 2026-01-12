using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for architectural style management.
/// </summary>
public interface IArchitecturalStyleService
{
    /// <summary>
    /// Gets a style by its string ID.
    /// </summary>
    ArchitecturalStyle? GetStyle(string styleId);

    /// <summary>
    /// Gets all styles compatible with a biome.
    /// </summary>
    IReadOnlyList<ArchitecturalStyle> GetStylesForBiome(string biomeId);

    /// <summary>
    /// Gets all styles valid at the specified depth.
    /// </summary>
    IReadOnlyList<ArchitecturalStyle> GetStylesForDepth(int depth);

    /// <summary>
    /// Selects a style for a position using weighted selection.
    /// </summary>
    string SelectStyleForPosition(Position3D position, string biomeId);

    /// <summary>
    /// Gets a random descriptor from a style.
    /// </summary>
    string? GetRandomDescriptor(string styleId, string category, Position3D position);

    /// <summary>
    /// Registers a style.
    /// </summary>
    void RegisterStyle(ArchitecturalStyle style);

    /// <summary>
    /// Gets all registered styles.
    /// </summary>
    IReadOnlyList<ArchitecturalStyle> GetAllStyles();
}
