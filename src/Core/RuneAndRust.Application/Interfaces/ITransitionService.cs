using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for managing biome transitions.
/// </summary>
public interface ITransitionService
{
    /// <summary>
    /// Gets a transition between two biomes.
    /// </summary>
    BiomeTransition? GetTransition(string sourceBiomeId, string targetBiomeId);

    /// <summary>
    /// Gets all biomes that can connect from the source at the given depth.
    /// </summary>
    IReadOnlyList<string> GetConnectableBiomes(string sourceBiomeId, int depth);

    /// <summary>
    /// Creates a transition blend for a position within a transition.
    /// </summary>
    TransitionBlend CreateBlend(string sourceBiomeId, string targetBiomeId, int roomIndex, int totalRooms);

    /// <summary>
    /// Checks if a transition between biomes is allowed.
    /// </summary>
    bool IsTransitionAllowed(string sourceBiomeId, string targetBiomeId, int depth);

    /// <summary>
    /// Registers a transition.
    /// </summary>
    void RegisterTransition(BiomeTransition transition);

    /// <summary>
    /// Gets all registered transitions.
    /// </summary>
    IReadOnlyList<BiomeTransition> GetAllTransitions();
}
