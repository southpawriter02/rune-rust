namespace RuneAndRust.Core.Quests;

/// <summary>
/// v0.14: Specifies requirements for procedurally generating a dungeon that satisfies quest objectives
/// Quest Anchors ensure specific handcrafted rooms appear in generated content
/// </summary>
public class QuestGenerationRequirements
{
    // Biome/Location
    public string BiomeId { get; set; } = "the_roots";
    public int MinDepth { get; set; } = 1;
    public int MaxDepth { get; set; } = 10;
    public int TargetRoomCount { get; set; } = 7;

    // Quest Anchors - Handcrafted rooms that must appear
    public List<string> RequiredAnchorIds { get; set; } = new();

    // Enemy requirements for kill objectives
    public Dictionary<string, int> RequiredEnemies { get; set; } = new(); // EnemyType => MinCount

    // Item requirements for collection objectives
    public List<string> RequiredLootNodes { get; set; } = new();

    /// <summary>
    /// Validates if a generated dungeon meets quest requirements
    /// </summary>
    public bool ValidateDungeon(DungeonGraph dungeon)
    {
        // Check room count
        if (dungeon.GetNodes().Count() < TargetRoomCount)
            return false;

        // Check required anchors present
        var anchorNodes = dungeon.GetNodes().Where(n => n.IsQuestAnchor).ToList();
        foreach (var requiredAnchorId in RequiredAnchorIds)
        {
            if (!anchorNodes.Any(n => n.QuestAnchorId == requiredAnchorId))
                return false;
        }

        // Enemy and loot validation would happen during population phase
        return true;
    }

    /// <summary>
    /// Creates a DungeonBlueprint from these quest requirements
    /// </summary>
    public DungeonBlueprint ToBlueprint(int seed, List<QuestAnchor> anchors)
    {
        return new DungeonBlueprint
        {
            Seed = seed,
            TargetRoomCount = TargetRoomCount,
            BiomeId = BiomeId,
            RequiredAnchors = anchors.Where(a => RequiredAnchorIds.Contains(a.AnchorId)).ToList()
        };
    }
}
