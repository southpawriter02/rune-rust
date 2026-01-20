using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Defines a potential feature that can spawn in a room template.
/// Used by the room instantiator to populate rooms with objects.
/// </summary>
public record RoomFeature(
    RoomFeatureType Type,
    string FeatureId,
    double SpawnChance,
    string? DescriptorOverride = null)
{
    /// <summary>
    /// Determines if this feature should spawn based on its spawn chance.
    /// </summary>
    public bool ShouldSpawn(Random random)
    {
        if (SpawnChance >= 1.0) return true;
        if (SpawnChance <= 0.0) return false;
        return random.NextDouble() < SpawnChance;
    }

    public override string ToString() => $"{Type}/{FeatureId} ({SpawnChance:P0})";
}
