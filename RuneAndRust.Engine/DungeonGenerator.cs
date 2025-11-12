using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Generates procedural dungeon layouts using graph-based algorithm (v0.10)
/// </summary>
public class DungeonGenerator
{
    private static readonly ILogger _log = Log.ForContext<DungeonGenerator>();
    private readonly TemplateLibrary _templateLibrary;
    private Random _rng = null!;
    private int _seed;

    public DungeonGenerator(TemplateLibrary templateLibrary)
    {
        _templateLibrary = templateLibrary;
    }

    /// <summary>
    /// Generates a complete dungeon graph from a seed
    /// </summary>
    public DungeonGraph Generate(int seed, int targetRoomCount = 7)
    {
        _seed = seed;
        _rng = new Random(seed);

        _log.Information("Generating dungeon: Seed={Seed}, TargetRoomCount={TargetRoomCount}",
            seed, targetRoomCount);

        // Step 1: Create graph structure
        var graph = new DungeonGraph();

        // Step 2: Generate main path
        _log.Debug("Generating main path...");
        GenerateMainPath(graph, targetRoomCount);

        // Step 3: Add branching paths (optional)
        int branchCount = _rng.NextDouble() < 0.6 ? _rng.Next(1, 3) : 0; // 60% chance of 1-2 branches
        if (branchCount > 0)
        {
            _log.Debug("Adding {BranchCount} branching paths...", branchCount);
            AddBranchingPaths(graph, branchCount);
        }

        // Step 4: Add secret rooms (optional)
        int secretCount = _rng.NextDouble() < 0.3 ? 1 : 0; // 30% chance of 1 secret
        if (secretCount > 0)
        {
            _log.Debug("Adding {SecretCount} secret rooms...", secretCount);
            AddSecretRooms(graph, secretCount);
        }

        // Step 5: Calculate node depths
        CalculateNodeDepths(graph);

        // Step 6: Assign directions to edges
        _log.Debug("Assigning directions to edges...");
        var directionAssigner = new DirectionAssigner();
        directionAssigner.AssignDirections(graph, _rng);

        // Step 7: Validate connectivity
        var (isValid, errors) = graph.Validate();
        if (!isValid)
        {
            _log.Error("Dungeon generation failed validation: {Errors}", string.Join(", ", errors));
            throw new InvalidOperationException($"Dungeon generation failed: {string.Join(", ", errors)}");
        }

        var stats = graph.GetStatistics();
        _log.Information("Dungeon generated successfully: Seed={Seed}, Nodes={Nodes}, Edges={Edges}, Branches={Branches}, Secrets={Secrets}",
            seed, stats["TotalNodes"], stats["TotalEdges"], stats["BranchNodes"], stats["SecretNodes"]);

        return graph;
    }

    /// <summary>
    /// Generates a complete playable dungeon (graph + instantiated rooms) from a seed
    /// </summary>
    public Dungeon GenerateComplete(int seed, int dungeonId = 1, int targetRoomCount = 7)
    {
        // Step 1: Generate graph
        var graph = Generate(seed, targetRoomCount);

        // Step 2: Instantiate rooms
        _log.Debug("Instantiating rooms...");
        var instantiator = new RoomInstantiator();
        var dungeon = instantiator.Instantiate(graph, dungeonId, seed);

        _log.Information("Complete dungeon generated: DungeonId={DungeonId}, Seed={Seed}, Rooms={RoomCount}",
            dungeonId, seed, dungeon.TotalRoomCount);

        return dungeon;
    }

    #region Main Path Generation

    /// <summary>
    /// Generates the main path: Start -> N rooms -> Boss
    /// </summary>
    private void GenerateMainPath(DungeonGraph graph, int targetRoomCount)
    {
        // Create start node (Entry Hall)
        var startTemplate = SelectTemplateByArchetype(RoomArchetype.EntryHall);
        var startNode = CreateNode(startTemplate, NodeType.Start, "Start Room");
        graph.AddNode(startNode);

        // Generate intermediate rooms (targetRoomCount - 2, excluding start and boss)
        var currentNode = startNode;
        int intermediateRoomCount = Math.Max(3, targetRoomCount - 2);

        for (int i = 0; i < intermediateRoomCount; i++)
        {
            var nextTemplate = SelectNextTemplate(currentNode, graph);
            var nextNode = CreateNode(nextTemplate, NodeType.Main, $"Main Path Room {i + 1}");
            graph.AddNode(nextNode);
            graph.AddEdge(currentNode, nextNode);
            currentNode = nextNode;

            _log.Debug("Added main path node: {NodeId} ({TemplateId})", nextNode.Id, nextTemplate.TemplateId);
        }

        // Create boss node
        var bossTemplate = SelectTemplateByArchetype(RoomArchetype.BossArena);
        var bossNode = CreateNode(bossTemplate, NodeType.Boss, "Boss Room");
        graph.AddNode(bossNode);
        graph.AddEdge(currentNode, bossNode);

        _log.Debug("Main path complete: {RoomCount} rooms", graph.NodeCount);
    }

    #endregion

    #region Branching Paths

    /// <summary>
    /// Adds branching paths that split from and optionally rejoin the main path
    /// </summary>
    private void AddBranchingPaths(DungeonGraph graph, int branchCount)
    {
        for (int i = 0; i < branchCount; i++)
        {
            // Pick random node on main path (not start or boss) as branch point
            var mainNodes = graph.GetMainPathNodes()
                .Where(n => n.Type != NodeType.Start && n.Type != NodeType.Boss)
                .ToList();

            if (mainNodes.Count == 0)
            {
                _log.Warning("Cannot add branch: no suitable branch points");
                continue;
            }

            var branchPoint = mainNodes[_rng.Next(mainNodes.Count)];

            // Create 1-2 room branch
            int branchLength = _rng.Next(1, 3);
            var branchNodes = new List<DungeonNode>();

            DungeonNode? previousNode = branchPoint;
            for (int j = 0; j < branchLength; j++)
            {
                var template = SelectNextTemplate(previousNode, graph);
                var branchNode = CreateNode(template, NodeType.Branch, $"Branch {i + 1} Room {j + 1}");
                graph.AddNode(branchNode);
                graph.AddEdge(previousNode, branchNode);
                branchNodes.Add(branchNode);
                previousNode = branchNode;

                _log.Debug("Added branch node: {NodeId} ({TemplateId}) from {BranchPoint}",
                    branchNode.Id, template.TemplateId, branchPoint.Id);
            }

            // 50% chance: branch rejoins main path (creates loop)
            // 50% chance: branch is dead end (optional exploration)
            if (_rng.NextDouble() < 0.5)
            {
                var rejoinPoint = FindRejoinPoint(graph, branchPoint);
                if (rejoinPoint != null && branchNodes.Count > 0)
                {
                    graph.AddEdge(branchNodes[^1], rejoinPoint);
                    _log.Debug("Branch rejoins at node {RejoinId}", rejoinPoint.Id);
                }
            }
            else
            {
                _log.Debug("Branch is dead-end (optional exploration)");
            }
        }
    }

    /// <summary>
    /// Finds a suitable rejoin point on the main path after the branch point
    /// </summary>
    private DungeonNode? FindRejoinPoint(DungeonGraph graph, DungeonNode branchPoint)
    {
        var mainPath = graph.GetMainPath();
        if (mainPath == null) return null;

        // Find branch point index in main path
        int branchIndex = mainPath.IndexOf(branchPoint);
        if (branchIndex == -1 || branchIndex >= mainPath.Count - 2)
            return null;

        // Rejoin at a later point on the main path (at least 2 nodes ahead)
        var eligibleRejoinPoints = mainPath
            .Skip(branchIndex + 2)
            .Where(n => n.Type != NodeType.Boss)
            .ToList();

        if (eligibleRejoinPoints.Count == 0)
            return null;

        return eligibleRejoinPoints[_rng.Next(eligibleRejoinPoints.Count)];
    }

    #endregion

    #region Secret Rooms

    /// <summary>
    /// Adds secret rooms connected via hidden passages
    /// </summary>
    private void AddSecretRooms(DungeonGraph graph, int secretCount)
    {
        for (int i = 0; i < secretCount; i++)
        {
            // Pick random non-boss room as parent
            var eligibleParents = graph.GetNodes()
                .Where(n => n.Type != NodeType.Boss && n.Type != NodeType.Secret)
                .ToList();

            if (eligibleParents.Count == 0)
            {
                _log.Warning("Cannot add secret room: no suitable parent nodes");
                continue;
            }

            var parentNode = eligibleParents[_rng.Next(eligibleParents.Count)];

            // Create secret room
            var template = SelectTemplateByArchetype(RoomArchetype.SecretRoom);
            var secretNode = CreateNode(template, NodeType.Secret, $"Secret Room {i + 1}");
            graph.AddNode(secretNode);

            // Connect with SECRET edge type
            graph.AddEdge(parentNode, secretNode, EdgeType.Secret);

            _log.Debug("Added secret room: {NodeId} ({TemplateId}) connected to {ParentId}",
                secretNode.Id, template.TemplateId, parentNode.Id);
        }
    }

    #endregion

    #region Template Selection

    /// <summary>
    /// Selects the next template based on current node and graph state
    /// </summary>
    private RoomTemplate SelectNextTemplate(DungeonNode currentNode, DungeonGraph graph)
    {
        // Get valid next archetypes based on current template's connection rules
        var validArchetypes = currentNode.Template.ValidConnections;

        // Filter by what hasn't been used recently (avoid repetition)
        var recentTemplates = GetRecentTemplates(graph, windowSize: 3);
        var availableTemplates = new List<RoomTemplate>();

        foreach (var archetype in validArchetypes)
        {
            // Skip BossArena (only used explicitly for final room)
            if (archetype == RoomArchetype.BossArena)
                continue;

            var templates = _templateLibrary.GetTemplatesByArchetype(archetype);

            // Prefer templates not recently used
            var freshTemplates = templates
                .Where(t => !recentTemplates.Contains(t.TemplateId))
                .ToList();

            availableTemplates.AddRange(freshTemplates.Count > 0 ? freshTemplates : templates);
        }

        // Fallback: if no valid templates, use any chamber/corridor
        if (availableTemplates.Count == 0)
        {
            _log.Warning("No valid templates found, falling back to Corridor");
            availableTemplates.AddRange(_templateLibrary.GetTemplatesByArchetype(RoomArchetype.Corridor));
        }

        if (availableTemplates.Count == 0)
        {
            throw new InvalidOperationException("Template library has no valid templates for generation");
        }

        // Weight by difficulty curve (future enhancement - for now just random)
        return availableTemplates[_rng.Next(availableTemplates.Count)];
    }

    /// <summary>
    /// Selects a template by archetype (for start/boss rooms)
    /// </summary>
    private RoomTemplate SelectTemplateByArchetype(RoomArchetype archetype)
    {
        var template = _templateLibrary.GetRandomTemplate(_rng, archetype);

        if (template == null)
        {
            throw new InvalidOperationException($"No templates found for archetype: {archetype}");
        }

        return template;
    }

    /// <summary>
    /// Gets template IDs used in the last N nodes
    /// </summary>
    private HashSet<string> GetRecentTemplates(DungeonGraph graph, int windowSize)
    {
        var nodes = graph.GetNodes();
        return nodes
            .TakeLast(windowSize)
            .Select(n => n.Template.TemplateId)
            .ToHashSet();
    }

    #endregion

    #region Node Creation & Utilities

    /// <summary>
    /// Creates a dungeon node from a template
    /// </summary>
    private DungeonNode CreateNode(RoomTemplate template, NodeType type, string name)
    {
        return new DungeonNode
        {
            Template = template,
            Type = type,
            Name = name,
            Depth = 0 // Will be calculated later
        };
    }

    /// <summary>
    /// Calculates depth (distance from start) for all nodes using BFS
    /// </summary>
    private void CalculateNodeDepths(DungeonGraph graph)
    {
        var startNode = graph.StartNode;
        if (startNode == null) return;

        var visited = new HashSet<DungeonNode>();
        var queue = new Queue<(DungeonNode Node, int Depth)>();

        queue.Enqueue((startNode, 0));
        visited.Add(startNode);
        startNode.Depth = 0;

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();

            foreach (var neighbor in graph.GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    neighbor.Depth = depth + 1;
                    queue.Enqueue((neighbor, depth + 1));
                }
            }
        }

        _log.Debug("Node depths calculated. Max depth: {MaxDepth}",
            graph.GetNodes().Max(n => n.Depth));
    }

    #endregion
}
