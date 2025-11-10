namespace RuneAndRust.Core;

public class Room
{
    public int Id { get; set; } = 1; // Unique room identifier
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Exits { get; set; } = new(); // Direction -> Room Name

    // Combat
    public List<Enemy> Enemies { get; set; } = new();
    public bool HasBeenCleared { get; set; } = false;

    // Puzzle
    public bool HasPuzzle { get; set; } = false;
    public bool IsPuzzleSolved { get; set; } = false;
    public string PuzzleDescription { get; set; } = string.Empty;
    public int PuzzleSuccessThreshold { get; set; } = 3;
    public int PuzzleFailureDamage { get; set; } = 6; // d6 damage on failure

    // Special
    public bool IsBossRoom { get; set; } = false;
    public bool IsStartRoom { get; set; } = false;
}
