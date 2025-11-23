using RuneAndRust.Core.Spatial;

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

    // v0.39.1: 3D Spatial Properties
    public RoomPosition Position { get; set; } = RoomPosition.Origin; // 3D coordinates (X, Y, Z)
    public VerticalLayer Layer { get; set; } = VerticalLayer.GroundLevel; // Vertical layer classification
    public List<VerticalConnection> VerticalConnections { get; set; } = new(); // Connections to rooms above/below

    // v0.39.2: Biome Transition & Blending
    public string PrimaryBiome { get; set; } = "TheRoots"; // Primary biome for this room
    public string? SecondaryBiome { get; set; } = null; // Secondary biome (null for pure single-biome rooms)
    public float BiomeBlendRatio { get; set; } = 0.0f; // 0.0 = 100% primary, 1.0 = 100% secondary
    public Dictionary<string, float> EnvironmentalProperties { get; set; } = new(); // Temperature, AethericIntensity, Scale, etc.

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
    public string? AtmosphericDescription { get; set; } = null; // v0.38.4
    public Descriptors.AtmosphericIntensity? AtmosphericIntensity { get; set; } = null; // v0.38.4
    public List<Descriptors.ResourceNode> ResourceNodes { get; set; } = new(); // v0.38.5

    // v0.11 Metadata
    public bool IsHandcrafted { get; set; } = false; // True for Quest Anchor rooms (skip procedural population)

    // v0.12 Coherent Glitch tracking
    public int CoherentGlitchRulesFired { get; set; } = 0;

    // v0.39.3: Content Density & Population Budget
    public Population.RoomDensity? DensityClassification { get; set; } = null; // Density type (Empty, Light, Medium, Heavy, Boss)
    public int AllocatedEnemyBudget { get; set; } = 0; // Enemies allocated to this room
    public int AllocatedHazardBudget { get; set; } = 0; // Hazards allocated to this room
    public int AllocatedLootBudget { get; set; } = 0; // Loot nodes allocated to this room
    public int ActualEnemiesSpawned { get; set; } = 0; // Enemies actually spawned (for validation)
    public int ActualHazardsSpawned { get; set; } = 0; // Hazards actually spawned (for validation)
    public int ActualLootSpawned { get; set; } = 0; // Loot nodes actually spawned (for validation)

    // Helper methods
    public bool HasAmbientCondition(string conditionName)
    {
        return AmbientConditions.Any(c => c.ConditionName.Contains(conditionName.Trim('[', ']'), StringComparison.OrdinalIgnoreCase));
    }

    // v0.39.1: Spatial helper methods
    public bool IsAtLayer(VerticalLayer layer) => Layer == layer;

    public bool HasVerticalConnectionTo(string roomId)
    {
        return VerticalConnections.Any(c => c.ToRoomId == roomId && !c.IsBlocked);
    }

    public int GetDepthInMeters()
    {
        return Layer.GetApproximateDepth();
    }

    public string GetDepthNarrative()
    {
        return Layer.GetDepthNarrative();
    }

    public VerticalConnection? GetVerticalConnectionTo(string roomId)
    {
        return VerticalConnections.FirstOrDefault(c =>
            (c.ToRoomId == roomId || (c.IsBidirectional && c.FromRoomId == roomId)) &&
            !c.IsBlocked);
    }

    public List<VerticalConnection> GetTraversableVerticalConnections()
    {
        return VerticalConnections.Where(c => c.CanTraverse()).ToList();
    }

    // v0.39.2: Biome blending helper methods
    public bool IsTransitionRoom() => SecondaryBiome != null && BiomeBlendRatio > 0.0f;

    public bool IsPureBiomeRoom() => SecondaryBiome == null || BiomeBlendRatio == 0.0f;

    public string GetDominantBiome()
    {
        if (SecondaryBiome == null || BiomeBlendRatio < 0.5f)
            return PrimaryBiome;
        return SecondaryBiome;
    }

    public (string biome, float weight)[] GetBiomeWeights()
    {
        if (SecondaryBiome == null)
            return new[] { (PrimaryBiome, 1.0f) };

        return new[]
        {
            (PrimaryBiome, 1.0f - BiomeBlendRatio),
            (SecondaryBiome, BiomeBlendRatio)
        };
    }

    public float? GetEnvironmentalProperty(string propertyName)
    {
        return EnvironmentalProperties.TryGetValue(propertyName, out var value) ? value : null;
    }

    public void SetEnvironmentalProperty(string propertyName, float value)
    {
        EnvironmentalProperties[propertyName] = value;
    }
}
