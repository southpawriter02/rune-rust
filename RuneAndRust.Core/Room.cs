namespace RuneAndRust.Core;

public class Room
{
    public int Id { get; set; } = 1; // Unique room identifier (legacy - for handcrafted rooms)
    public string RoomId { get; set; } = string.Empty; // String ID for procedurally generated rooms (v0.10)
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Exits { get; set; } = new(); // Direction -> Room ID/Name

    // Procedural Generation (v0.10)
    public string? TemplateId { get; set; } = null; // Template used to generate this room
    public NodeType? GeneratedNodeType { get; set; } = null; // Node type in generation graph
    public bool IsProcedurallyGenerated { get; set; } = false;
    public Population.RoomArchetype Archetype { get; set; } = Population.RoomArchetype.Chamber; // Room archetype (Chamber, Corridor, etc.)

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

    // NPC System (v0.8)
    public List<NPC> NPCs { get; set; } = new(); // NPCs present in this room

    // Trauma Economy (v0.5)
    public PsychicResonanceLevel PsychicResonance { get; set; } = PsychicResonanceLevel.None;
    public bool IsSanctuary { get; set; } = false; // Safe location for Sanctuary Rest

    // v0.11 Population Systems - Use Population namespace types
    public List<Population.DynamicHazard> DynamicHazards { get; set; } = new();
    public List<Population.StaticTerrain> StaticTerrain { get; set; } = new();
    public List<Population.LootNode> LootNodes { get; set; } = new();
    public List<Population.AmbientCondition> AmbientConditions { get; set; } = new();

    // v0.38.x Descriptor-based Population
    public List<Descriptors.StaticTerrainFeature> StaticTerrainFeatures { get; set; } = new(); // v0.38.2
    public List<Descriptors.DynamicHazard> DynamicHazardFeatures { get; set; } = new(); // v0.38.2
    public List<Descriptors.InteractiveObject> InteractiveObjects { get; set; } = new(); // v0.38.3

    // v0.11 Metadata
    public bool IsHandcrafted { get; set; } = false; // True for Quest Anchor rooms (skip procedural population)

    // v0.12 Coherent Glitch tracking
    public int CoherentGlitchRulesFired { get; set; } = 0;

    // Helper methods
    public bool HasAmbientCondition(string conditionName)
    {
        return AmbientConditions.Any(c => c.ConditionName.Contains(conditionName.Trim('[', ']'), StringComparison.OrdinalIgnoreCase));
    }
}
