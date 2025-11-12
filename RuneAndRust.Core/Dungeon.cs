namespace RuneAndRust.Core;

/// <summary>
/// Represents a complete procedurally generated dungeon (v0.10)
/// </summary>
public class Dungeon
{
    // Identity
    public int DungeonId { get; set; }
    public int Seed { get; set; }
    public string Biome { get; set; } = "the_roots";

    // Rooms
    public Dictionary<string, Room> Rooms { get; set; } = new(); // RoomId -> Room
    public string StartRoomId { get; set; } = string.Empty;
    public string BossRoomId { get; set; } = string.Empty;

    // Metadata
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public int TotalRoomCount => Rooms.Count;

    /// <summary>
    /// Gets the start room
    /// </summary>
    public Room? GetStartRoom()
    {
        return Rooms.GetValueOrDefault(StartRoomId);
    }

    /// <summary>
    /// Gets the boss room
    /// </summary>
    public Room? GetBossRoom()
    {
        return Rooms.GetValueOrDefault(BossRoomId);
    }

    /// <summary>
    /// Gets a room by ID
    /// </summary>
    public Room? GetRoom(string roomId)
    {
        return Rooms.GetValueOrDefault(roomId);
    }

    /// <summary>
    /// Gets all secret rooms in the dungeon
    /// </summary>
    public List<Room> GetSecretRooms()
    {
        return Rooms.Values
            .Where(r => r.GeneratedNodeType == NodeType.Secret)
            .ToList();
    }

    /// <summary>
    /// Gets all branch rooms in the dungeon
    /// </summary>
    public List<Room> GetBranchRooms()
    {
        return Rooms.Values
            .Where(r => r.GeneratedNodeType == NodeType.Branch)
            .ToList();
    }

    /// <summary>
    /// Gets statistics about the dungeon
    /// </summary>
    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            ["TotalRooms"] = TotalRoomCount,
            ["StartRooms"] = Rooms.Values.Count(r => r.GeneratedNodeType == NodeType.Start),
            ["MainRooms"] = Rooms.Values.Count(r => r.GeneratedNodeType == NodeType.Main),
            ["BranchRooms"] = Rooms.Values.Count(r => r.GeneratedNodeType == NodeType.Branch),
            ["SecretRooms"] = Rooms.Values.Count(r => r.GeneratedNodeType == NodeType.Secret),
            ["BossRooms"] = Rooms.Values.Count(r => r.GeneratedNodeType == NodeType.Boss)
        };
    }

    /// <summary>
    /// Validates that the dungeon structure is valid
    /// </summary>
    public (bool IsValid, List<string> Errors) Validate()
    {
        var errors = new List<string>();

        // Check for start room
        if (string.IsNullOrEmpty(StartRoomId))
        {
            errors.Add("Dungeon has no start room ID");
        }
        else if (GetStartRoom() == null)
        {
            errors.Add($"Start room '{StartRoomId}' not found in dungeon");
        }

        // Check for boss room
        if (string.IsNullOrEmpty(BossRoomId))
        {
            errors.Add("Dungeon has no boss room ID");
        }
        else if (GetBossRoom() == null)
        {
            errors.Add($"Boss room '{BossRoomId}' not found in dungeon");
        }

        // Check that all rooms have valid exits
        foreach (var room in Rooms.Values)
        {
            foreach (var (direction, targetRoomId) in room.Exits)
            {
                if (!Rooms.ContainsKey(targetRoomId))
                {
                    errors.Add($"Room '{room.RoomId}' has exit to non-existent room '{targetRoomId}'");
                }
            }
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Gets a debug-friendly representation
    /// </summary>
    public override string ToString()
    {
        return $"Dungeon {DungeonId}: Seed={Seed}, Rooms={TotalRoomCount}, Biome={Biome}";
    }
}
