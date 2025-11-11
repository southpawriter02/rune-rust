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

    // Equipment & Loot (v0.3)
    public List<Equipment> ItemsOnGround { get; set; } = new();

    // Puzzle
    public bool HasPuzzle { get; set; } = false;
    public bool IsPuzzleSolved { get; set; } = false;
    public string PuzzleDescription { get; set; } = string.Empty;
    public int PuzzleSuccessThreshold { get; set; } = 3;
    public int PuzzleFailureDamage { get; set; } = 6; // d6 damage on failure

    // Special
    public bool IsBossRoom { get; set; } = false;
    public bool IsStartRoom { get; set; } = false;

    // Environmental Hazards (v0.4)
    public bool HasEnvironmentalHazard { get; set; } = false;
    public bool IsHazardActive { get; set; } = true; // Can be disabled via puzzle
    public int HazardDamagePerTurn { get; set; } = 0; // Damage dealt each turn
    public string HazardDescription { get; set; } = string.Empty;

    // NPC Interaction (v0.4)
    public bool HasTalkableNPC { get; set; } = false; // Can negotiate instead of fighting
    public bool HasTalkedToNPC { get; set; } = false; // Track if player already talked
}
