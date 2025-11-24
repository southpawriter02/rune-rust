using RuneAndRust.Core.Descriptors;

namespace RuneAndRust.Core;

/// <summary>
/// v0.11 Handcrafted Room Definition
/// Defines a designer-created room with specific enemies, hazards, loot, and narrative elements
/// Used by Quest Anchors to insert story-critical rooms into procedural dungeons
/// </summary>
public class HandcraftedRoom
{
    // Identity
    public string RoomId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Classification
    public RoomArchetype Archetype { get; set; } = RoomArchetype.Chamber;
    public NodeType SuggestedNodeType { get; set; } = NodeType.Main;

    // Narrative
    public string? NarrativeText { get; set; } = null; // Cutscene or lore text
    public string? FlavorText { get; set; } = null; // Additional atmospheric description

    // Population (handcrafted, not procedural)
    public List<string> EnemyIds { get; set; } = new(); // Specific enemy IDs to spawn
    public List<string> HazardIds { get; set; } = new(); // Specific hazards
    public List<string> TerrainIds { get; set; } = new(); // Specific terrain
    public List<string> LootIds { get; set; } = new(); // Specific loot
    public List<string> ConditionIds { get; set; } = new(); // Ambient conditions

    // NPCs
    public List<string> NPCIds { get; set; } = new(); // NPCs present in this room

    // Special Properties
    public bool IsSafeZone { get; set; } = false; // No combat allowed
    public bool IsBossArena { get; set; } = false;
    public bool HasQuestObjective { get; set; } = false;
    public string? QuestObjectiveId { get; set; } = null;

    // Exits (optional - can be overridden by graph generation)
    public Dictionary<string, string> PreferredExits { get; set; } = new();
    // Key: Direction (North, South, etc.), Value: Preferred connection type (Main, Branch, etc.)

    /// <summary>
    /// Converts this handcrafted definition into an instantiated Room
    /// </summary>
    public Room Instantiate(string roomId)
    {
        var room = new Room
        {
            RoomId = roomId,
            Name = Name,
            Description = Description,
            IsHandcrafted = true,
            IsBossRoom = IsBossArena,
            IsSanctuary = IsSafeZone,
            // Exits will be set by graph generation
            Exits = new Dictionary<string, string>()
        };

        // Note: Actual enemy/hazard/loot instances are created by a HandcraftedRoomPopulator service
        // This just stores the IDs for later instantiation

        return room;
    }

    /// <summary>
    /// Validates the handcrafted room definition
    /// </summary>
    public (bool IsValid, List<string> Errors) Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(RoomId))
            errors.Add("RoomId is required");

        if (string.IsNullOrEmpty(Name))
            errors.Add("Name is required");

        if (string.IsNullOrEmpty(Description))
            errors.Add("Description is required");

        // Boss rooms should have enemies
        if (IsBossArena && EnemyIds.Count == 0)
            errors.Add("Boss arena must have at least one enemy");

        // Safe zones shouldn't have enemies
        if (IsSafeZone && EnemyIds.Count > 0)
            errors.Add("Safe zone cannot have enemies");

        return (errors.Count == 0, errors);
    }
}
