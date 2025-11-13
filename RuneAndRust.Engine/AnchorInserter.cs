using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.11 Quest Anchor Inserter
/// Inserts handcrafted Quest Anchor rooms into procedurally generated dungeon graphs
/// </summary>
public class AnchorInserter
{
    private static readonly ILogger _log = Log.ForContext<AnchorInserter>();
    private readonly HandcraftedRoomLibrary _roomLibrary;

    public AnchorInserter(HandcraftedRoomLibrary roomLibrary)
    {
        _roomLibrary = roomLibrary;
    }

    /// <summary>
    /// Inserts Quest Anchors into a dungeon graph according to blueprint requirements
    /// </summary>
    public void InsertAnchors(DungeonGraph graph, DungeonBlueprint blueprint, Random rng)
    {
        if (blueprint.RequiredAnchors.Count == 0)
        {
            _log.Debug("No Quest Anchors to insert");
            return;
        }

        _log.Information("Inserting {AnchorCount} Quest Anchors into dungeon graph", blueprint.RequiredAnchors.Count);

        // Step 1: Sort anchors by priority (mandatory first, then by depth constraints)
        var sortedAnchors = SortAnchorsByPriority(blueprint.RequiredAnchors);

        // Step 2: Insert each anchor
        int insertedCount = 0;
        int skippedCount = 0;

        foreach (var anchor in sortedAnchors)
        {
            bool inserted = InsertAnchor(graph, anchor, rng);
            if (inserted)
            {
                insertedCount++;
                _log.Information("Inserted Quest Anchor: {AnchorId} ({AnchorName})", anchor.AnchorId, anchor.Name);
            }
            else
            {
                if (anchor.IsMandatory)
                {
                    _log.Error("Failed to insert mandatory Quest Anchor: {AnchorId}", anchor.AnchorId);
                    throw new InvalidOperationException($"Could not insert mandatory Quest Anchor: {anchor.AnchorId}");
                }
                else
                {
                    _log.Warning("Skipped optional Quest Anchor: {AnchorId} (no suitable location)", anchor.AnchorId);
                    skippedCount++;
                }
            }
        }

        _log.Information("Quest Anchor insertion complete: {InsertedCount} inserted, {SkippedCount} skipped",
            insertedCount, skippedCount);
    }

    /// <summary>
    /// Inserts a single Quest Anchor into the graph
    /// </summary>
    private bool InsertAnchor(DungeonGraph graph, QuestAnchor anchor, Random rng)
    {
        // Step 1: Find eligible nodes for replacement
        var eligibleNodes = FindEligibleNodes(graph, anchor);

        if (eligibleNodes.Count == 0)
        {
            _log.Warning("No eligible nodes found for anchor {AnchorId}", anchor.AnchorId);
            return false;
        }

        // Step 2: Select a node to replace
        var targetNode = SelectBestNode(eligibleNodes, anchor, rng);

        // Step 3: Get the handcrafted room definition
        var handcraftedRoom = _roomLibrary.GetRoom(anchor.HandcraftedRoomId);
        if (handcraftedRoom == null)
        {
            _log.Error("Handcrafted room not found: {RoomId}", anchor.HandcraftedRoomId);
            return false;
        }

        // Step 4: Replace the node's template with the handcrafted room
        ReplaceNodeWithAnchor(targetNode, anchor, handcraftedRoom);

        _log.Debug("Replaced node {NodeId} with Quest Anchor {AnchorId} at depth {Depth}",
            targetNode.Id, anchor.AnchorId, targetNode.Depth);

        return true;
    }

    /// <summary>
    /// Finds nodes that are eligible for Quest Anchor replacement
    /// </summary>
    private List<DungeonNode> FindEligibleNodes(DungeonGraph graph, QuestAnchor anchor)
    {
        var nodes = graph.GetNodes().ToList();
        var eligible = new List<DungeonNode>();

        foreach (var node in nodes)
        {
            // Check constraints
            if (!MeetsConstraints(node, anchor))
                continue;

            // Skip already replaced nodes
            if (node.IsQuestAnchor)
                continue;

            eligible.Add(node);
        }

        return eligible;
    }

    /// <summary>
    /// Checks if a node meets the Quest Anchor's constraints
    /// </summary>
    private bool MeetsConstraints(DungeonNode node, QuestAnchor anchor)
    {
        var constraints = anchor.Constraints;
        if (constraints == null)
            return true; // No constraints = all nodes eligible

        // Depth constraints
        if (constraints.MinDepth.HasValue && node.Depth < constraints.MinDepth.Value)
            return false;

        if (constraints.MaxDepth.HasValue && node.Depth > constraints.MaxDepth.Value)
            return false;

        // Position constraints
        if (constraints.MustBeOnMainPath && node.Type != NodeType.Main && node.Type != NodeType.Start && node.Type != NodeType.Boss)
            return false;

        if (!constraints.CanBeOnBranchPath && node.Type == NodeType.Branch)
            return false;

        if (!constraints.CanBeSecret && node.Type == NodeType.Secret)
            return false;

        // Never constraints
        if (constraints.NeverAsStartRoom && node.Type == NodeType.Start)
            return false;

        if (constraints.NeverAsBossRoom && node.Type == NodeType.Boss)
            return false;

        return true;
    }

    /// <summary>
    /// Selects the best node from eligible candidates
    /// </summary>
    private DungeonNode SelectBestNode(List<DungeonNode> eligibleNodes, QuestAnchor anchor, Random rng)
    {
        // Prefer nodes that match the preferred node type
        var preferredNodes = eligibleNodes.Where(n => n.Type == anchor.PreferredNodeType).ToList();
        if (preferredNodes.Count > 0)
        {
            return preferredNodes[rng.Next(preferredNodes.Count)];
        }

        // Otherwise, pick a random eligible node
        return eligibleNodes[rng.Next(eligibleNodes.Count)];
    }

    /// <summary>
    /// Replaces a node with a Quest Anchor
    /// </summary>
    private void ReplaceNodeWithAnchor(DungeonNode node, QuestAnchor anchor, HandcraftedRoom handcraftedRoom)
    {
        // Store original template (for debugging/logging)
        var originalTemplateId = node.Template.TemplateId;

        // Create a new RoomTemplate from the handcrafted room
        var anchorTemplate = CreateAnchorTemplate(handcraftedRoom);

        // Replace the template
        node.Template = anchorTemplate;
        node.Name = anchor.Name;

        // Mark as Quest Anchor
        node.IsQuestAnchor = true;
        node.QuestAnchorId = anchor.AnchorId;
        node.HandcraftedRoomId = anchor.HandcraftedRoomId;

        _log.Debug("Replaced template {OriginalTemplate} with Quest Anchor template {AnchorTemplate}",
            originalTemplateId, anchorTemplate.TemplateId);
    }

    /// <summary>
    /// Creates a RoomTemplate from a HandcraftedRoom definition
    /// </summary>
    private RoomTemplate CreateAnchorTemplate(HandcraftedRoom room)
    {
        return new RoomTemplate
        {
            TemplateId = $"quest_anchor_{room.RoomId}",
            Archetype = room.Archetype,
            ValidConnections = new List<RoomArchetype>
            {
                // Quest Anchors can connect to anything
                RoomArchetype.Corridor,
                RoomArchetype.Chamber,
                RoomArchetype.Junction,
                RoomArchetype.EntryHall,
                RoomArchetype.BossArena,
                RoomArchetype.SecretRoom
            }
        };
    }

    /// <summary>
    /// Sorts anchors by priority (mandatory first, then by depth)
    /// </summary>
    private List<QuestAnchor> SortAnchorsByPriority(List<QuestAnchor> anchors)
    {
        return anchors
            .OrderByDescending(a => a.IsMandatory) // Mandatory first
            .ThenBy(a => a.Constraints?.MinDepth ?? 0) // Then by earliest depth
            .ToList();
    }
}
