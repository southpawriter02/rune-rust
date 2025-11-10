namespace RuneAndRust.Core;

/// <summary>
/// Represents the state of the game world (progress through rooms, puzzles, etc.)
/// </summary>
public class WorldState
{
    public int CurrentRoomId { get; set; } = 1; // Start room
    public List<int> ClearedRoomIds { get; set; } = new();
    public bool PuzzleSolved { get; set; } = false;
    public bool BossDefeated { get; set; } = false;

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
