using RuneAndRust.Core.Entities;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Service for evaluating spawn rules and adjusting weights for biome elements.
/// Part of the Dynamic Room Engine (v0.4.0).
/// </summary>
public interface IElementSpawnEvaluator
{
    /// <summary>
    /// Determines if a biome element can spawn in the given room based on its spawn rules.
    /// Evaluates rules like NeverInEntryHall, OnlyInLargeRooms, RequiresRoomNameContains, etc.
    /// </summary>
    /// <param name="element">The biome element to evaluate.</param>
    /// <param name="room">The target room for spawning.</param>
    /// <param name="template">The room template (provides archetype, size metadata).</param>
    /// <param name="context">The spawn context tracking already-spawned entities.</param>
    /// <returns>True if the element can spawn; false otherwise.</returns>
    bool CanSpawn(BiomeElement element, Room room, RoomTemplate template, SpawnContext context);

    /// <summary>
    /// Calculates the adjusted spawn weight for a biome element based on room conditions.
    /// Applies multipliers for special cases (e.g., HigherWeightInSecretRooms).
    /// </summary>
    /// <param name="element">The biome element to evaluate.</param>
    /// <param name="room">The target room for spawning.</param>
    /// <param name="template">The room template (provides metadata for weight adjustments).</param>
    /// <returns>The adjusted weight (base weight * multipliers).</returns>
    float GetAdjustedWeight(BiomeElement element, Room room, RoomTemplate template);
}
