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
    public int HazardDamagePerTurn { get; set; } = 0; // Damage dealt each turn (legacy v0.4 - flat damage)
    public string HazardDescription { get; set; } = string.Empty;

    // Enhanced Hazard System (v0.6)
    public HazardType HazardType { get; set; } = HazardType.None;

    // Automatic hazards (damage every turn)
    public int HazardDamageDice { get; set; } = 0; // Number of dice to roll (e.g., 3 for 3d6)
    public int HazardDamageDieSize { get; set; } = 6; // Die size (default d6)
    public int HazardStressPerTurn { get; set; } = 0; // Psychic Stress dealt each turn

    // Check-based hazards (require attribute check)
    public bool HazardRequiresCheck { get; set; } = false; // True if hazard needs an attribute check
    public string HazardCheckAttribute { get; set; } = string.Empty; // Attribute to check (FINESSE, WITS, etc.)
    public int HazardCheckDC { get; set; } = 0; // Difficulty Class for the check
    public int HazardCheckFailureDice { get; set; } = 0; // Dice rolled on check failure (e.g., 2 for 2d6)
    public int HazardCheckFailureDieSize { get; set; } = 6; // Die size for failure damage

    // NPC Interaction (v0.4)
    public bool HasTalkableNPC { get; set; } = false; // Can negotiate instead of fighting
    public bool HasTalkedToNPC { get; set; } = false; // Track if player already talked

    // Trauma Economy (v0.5)
    public PsychicResonanceLevel PsychicResonance { get; set; } = PsychicResonanceLevel.None;
    public bool IsSanctuary { get; set; } = false; // Safe location for Sanctuary Rest
}
