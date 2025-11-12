namespace RuneAndRust.Core;

/// <summary>
/// Represents the state of the game world (progress through rooms, puzzles, etc.)
/// </summary>
public class WorldState
{
    public int CurrentRoomId { get; set; } = 1; // Start room (legacy)
    public List<int> ClearedRoomIds { get; set; } = new();
    public bool PuzzleSolved { get; set; } = false;
    public bool BossDefeated { get; set; } = false;

    // v0.10: Procedural generation fields
    public string? CurrentRoomStringId { get; set; } = null; // String room ID for procedural dungeons
    public int DungeonsCompleted { get; set; } = 0; // Total dungeons completed

    public bool IsRoomCleared(int roomId)
    {
        return ClearedRoomIds.Contains(roomId);
    }

    public void MarkRoomCleared(int roomId)
    {
        if (!ClearedRoomIds.Contains(roomId))
        {
            ClearedRoomIds.Add(roomId);
        }
    }
}
