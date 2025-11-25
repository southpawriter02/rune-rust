using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using Serilog;

// Resolve ambiguous RoomArchetype reference
using PopulationRoomArchetype = RuneAndRust.Core.Population.RoomArchetype;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.39.3: Room density classifier
/// Assigns density types (Empty, Light, Medium, Heavy, Boss) to create pacing variety
/// Philosophy: Not every room needs combat - variety is key
/// </summary>
public class DensityClassificationService
{
    private static readonly ILogger _log = Log.ForContext<DensityClassificationService>();

    // Target distribution percentages
    private const float EmptyRoomPercentage = 0.12f;   // 10-15%
    private const float LightRoomPercentage = 0.45f;   // 40-50%
    private const float MediumRoomPercentage = 0.30f;  // 25-35%
    // Heavy is calculated as remainder

    /// <summary>
    /// Classifies all rooms in a sector with density types
    /// </summary>
    /// <param name="rooms">Rooms to classify</param>
    /// <param name="rng">Random number generator</param>
    /// <returns>Dictionary mapping rooms to density classifications</returns>
    public Dictionary<Room, RoomDensity> ClassifyRooms(List<Room> rooms, Random rng)
    {
        if (rooms == null || rooms.Count == 0)
        {
            _log.Warning("No rooms to classify");
            return new Dictionary<Room, RoomDensity>();
        }

        var classifications = new Dictionary<Room, RoomDensity>();

        // Step 1: Fixed classifications based on room archetype
        foreach (var room in rooms)
        {
            if (room.Archetype == PopulationRoomArchetype.BossArena || room.IsBossRoom)
            {
                classifications[room] = RoomDensity.Boss;
                _log.Debug("Room {RoomId} classified as Boss (archetype)", room.RoomId);
            }
            else if (room.Archetype == PopulationRoomArchetype.EntryHall || room.IsStartRoom)
            {
                classifications[room] = RoomDensity.Light; // Safe start
                _log.Debug("Room {RoomId} classified as Light (entry hall)", room.RoomId);
            }
            else if (room.Archetype == PopulationRoomArchetype.SecretRoom ||
                     room.GeneratedNodeType == NodeType.Secret)
            {
                classifications[room] = RoomDensity.Empty; // Reward exploration
                _log.Debug("Room {RoomId} classified as Empty (secret room)", room.RoomId);
            }
        }

        // Step 2: Classify remaining rooms
        var unclassifiedRooms = rooms
            .Where(r => !classifications.ContainsKey(r))
            .ToList();

        if (unclassifiedRooms.Count == 0)
        {
            _log.Information("All {Count} rooms have fixed classifications", rooms.Count);
            return classifications;
        }

        // Calculate target counts
        var emptyCount = Math.Max(0, (int)(unclassifiedRooms.Count * EmptyRoomPercentage));
        var lightCount = Math.Max(1, (int)(unclassifiedRooms.Count * LightRoomPercentage));
        var mediumCount = Math.Max(0, (int)(unclassifiedRooms.Count * MediumRoomPercentage));
        var heavyCount = Math.Max(0, unclassifiedRooms.Count - emptyCount - lightCount - mediumCount);

        _log.Debug(
            "Distribution targets for {Total} unclassified rooms: Empty={Empty}, Light={Light}, Medium={Medium}, Heavy={Heavy}",
            unclassifiedRooms.Count, emptyCount, lightCount, mediumCount, heavyCount);

        // Step 3: Shuffle and assign densities
        var shuffled = unclassifiedRooms.OrderBy(r => rng.Next()).ToList();

        int index = 0;

        // Assign Empty
        for (int i = 0; i < emptyCount && index < shuffled.Count; i++, index++)
        {
            classifications[shuffled[index]] = RoomDensity.Empty;
        }

        // Assign Light
        for (int i = 0; i < lightCount && index < shuffled.Count; i++, index++)
        {
            classifications[shuffled[index]] = RoomDensity.Light;
        }

        // Assign Medium
        for (int i = 0; i < mediumCount && index < shuffled.Count; i++, index++)
        {
            classifications[shuffled[index]] = RoomDensity.Medium;
        }

        // Assign Heavy (remaining)
        while (index < shuffled.Count)
        {
            classifications[shuffled[index]] = RoomDensity.Heavy;
            index++;
        }

        // Log final distribution
        var distribution = classifications.Values
            .GroupBy(d => d)
            .ToDictionary(g => g.Key, g => g.Count());

        _log.Information(
            "Density classification complete: {Total} rooms → " +
            "Empty={Empty}, Light={Light}, Medium={Medium}, Heavy={Heavy}, Boss={Boss}",
            rooms.Count,
            distribution.GetValueOrDefault(RoomDensity.Empty, 0),
            distribution.GetValueOrDefault(RoomDensity.Light, 0),
            distribution.GetValueOrDefault(RoomDensity.Medium, 0),
            distribution.GetValueOrDefault(RoomDensity.Heavy, 0),
            distribution.GetValueOrDefault(RoomDensity.Boss, 0));

        return classifications;
    }
}
