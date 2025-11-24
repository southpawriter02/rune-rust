using RuneAndRust.Core.Descriptors;

namespace RuneAndRust.Core;

/// <summary>
/// v0.11 Quest Anchor System
/// Represents a handcrafted room that must appear in a generated Sector for narrative purposes
/// From v2.0 spec: "A quest can require the engine to generate a Sector with specific parameters"
/// </summary>
public class QuestAnchor
{
    // Identity
    public string AnchorId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Placement
    public RoomArchetype PreferredArchetype { get; set; } = RoomArchetype.Chamber;
    public NodeType PreferredNodeType { get; set; } = NodeType.Main;
    public bool IsMandatory { get; set; } = true; // If false, anchor is optional

    // Handcrafted Room Reference
    public string HandcraftedRoomId { get; set; } = string.Empty; // Reference to room definition file

    // Constraints
    public QuestAnchorConstraints? Constraints { get; set; } = null;

    // Quest Integration
    public string? QuestId { get; set; } = null; // Associated quest (if any)
    public QuestAnchorPurpose Purpose { get; set; } = QuestAnchorPurpose.QuestObjective;
}

/// <summary>
/// Constraints for Quest Anchor placement in the dungeon graph
/// </summary>
public class QuestAnchorConstraints
{
    // Depth constraints (distance from start)
    public int? MinDepth { get; set; } = null; // Earliest the anchor can appear
    public int? MaxDepth { get; set; } = null; // Latest the anchor can appear

    // Position constraints
    public bool MustBeOnMainPath { get; set; } = false; // If true, anchor must be on critical path
    public bool CanBeOnBranchPath { get; set; } = true; // If true, anchor can be on optional branch
    public bool CanBeSecret { get; set; } = false; // If true, anchor can be a secret room

    // Never constraints (for safety/narrative)
    public bool NeverAsStartRoom { get; set; } = true; // Almost always true
    public bool NeverAsBossRoom { get; set; } = false; // Sometimes anchors ARE boss rooms

    // Co-location constraints (for multi-room quests)
    public List<string> MustAppearBefore { get; set; } = new(); // Anchor IDs that must come after this
    public List<string> MustAppearAfter { get; set; } = new(); // Anchor IDs that must come before this
}

/// <summary>
/// Purpose of the Quest Anchor (for generation hints)
/// </summary>
public enum QuestAnchorPurpose
{
    QuestObjective,     // Location where quest goal is found (e.g., retrieve schematics)
    BossEncounter,      // Special boss fight required by quest
    NPCEncounter,       // Meet an NPC for quest progression
    NarrativeMoment,    // Cutscene or lore revelation
    Checkpoint,         // Safe zone or save point
    CraftingStation,    // Special crafting/upgrade location
    Discovery           // Environmental storytelling location
}

/// <summary>
/// Blueprint for a dungeon that includes Quest Anchors
/// Extends the generation parameters from v0.10
/// </summary>
public class DungeonBlueprint
{
    // Generation Parameters (from v0.10)
    public int Seed { get; set; }
    public int TargetRoomCount { get; set; } = 7;
    public string BiomeId { get; set; } = "the_roots";

    // Quest Anchors (v0.11)
    public List<QuestAnchor> RequiredAnchors { get; set; } = new();

    // Metadata
    public string? QuestId { get; set; } = null; // If this dungeon is for a specific quest
    public string? NarrativeContext { get; set; } = null; // Story context for generation

    /// <summary>
    /// Gets all mandatory anchors
    /// </summary>
    public List<QuestAnchor> GetMandatoryAnchors()
    {
        return RequiredAnchors.Where(a => a.IsMandatory).ToList();
    }

    /// <summary>
    /// Gets all optional anchors
    /// </summary>
    public List<QuestAnchor> GetOptionalAnchors()
    {
        return RequiredAnchors.Where(a => !a.IsMandatory).ToList();
    }

    /// <summary>
    /// Validates that the blueprint is feasible
    /// </summary>
    public (bool IsValid, List<string> Errors) Validate()
    {
        var errors = new List<string>();

        // Check that we have enough room budget for anchors
        int mandatoryAnchorCount = RequiredAnchors.Count(a => a.IsMandatory);
        if (mandatoryAnchorCount >= TargetRoomCount)
        {
            errors.Add($"Too many mandatory anchors ({mandatoryAnchorCount}) for target room count ({TargetRoomCount})");
        }

        // Check for circular dependencies
        foreach (var anchor in RequiredAnchors)
        {
            if (anchor.Constraints != null)
            {
                // If A must appear before B, and B must appear before A -> circular
                foreach (var beforeId in anchor.Constraints.MustAppearBefore)
                {
                    var otherAnchor = RequiredAnchors.FirstOrDefault(a => a.AnchorId == beforeId);
                    if (otherAnchor?.Constraints?.MustAppearBefore.Contains(anchor.AnchorId) == true)
                    {
                        errors.Add($"Circular dependency between anchors {anchor.AnchorId} and {beforeId}");
                    }
                }
            }
        }

        // Check that HandcraftedRoomId is specified
        foreach (var anchor in RequiredAnchors.Where(a => string.IsNullOrEmpty(a.HandcraftedRoomId)))
        {
            errors.Add($"Anchor {anchor.AnchorId} has no HandcraftedRoomId");
        }

        return (errors.Count == 0, errors);
    }
}
