using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// JSON-based provider for ability tree definitions.
/// </summary>
/// <remarks>
/// <para>JsonAbilityTreeProvider loads ability tree definitions from a JSON configuration file
/// and provides thread-safe access with indexed lookups for fast retrieval.</para>
/// <para>Expected JSON format:</para>
/// <code>
/// {
///   "abilityTrees": [
///     {
///       "treeId": "warrior-tree",
///       "classId": "warrior",
///       "name": "Warrior Talents",
///       "description": "...",
///       "pointsPerLevel": 1,
///       "icon": "icons/trees/warrior.png",
///       "branches": [
///         {
///           "branchId": "berserker",
///           "name": "Berserker",
///           "description": "...",
///           "nodes": [...]
///         }
///       ]
///     }
///   ]
/// }
/// </code>
/// <para>The provider creates multiple indexes for efficient lookup:</para>
/// <list type="bullet">
///   <item><description>_treesByTreeId: TreeId → AbilityTreeDefinition</description></item>
///   <item><description>_treesByClassId: ClassId → AbilityTreeDefinition</description></item>
///   <item><description>_nodesById: NodeId → AbilityTreeNode</description></item>
///   <item><description>_branchesByNodeId: NodeId → AbilityTreeBranch</description></item>
///   <item><description>_treesByNodeId: NodeId → AbilityTreeDefinition</description></item>
/// </list>
/// </remarks>
public class JsonAbilityTreeProvider : IAbilityTreeProvider
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private readonly Dictionary<string, AbilityTreeDefinition> _treesByTreeId;
    private readonly Dictionary<string, AbilityTreeDefinition> _treesByClassId;
    private readonly Dictionary<string, AbilityTreeNode> _nodesById;
    private readonly Dictionary<string, AbilityTreeBranch> _branchesByNodeId;
    private readonly Dictionary<string, AbilityTreeDefinition> _treesByNodeId;
    private readonly ILogger<JsonAbilityTreeProvider> _logger;
    private readonly string _configPath;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new JsonAbilityTreeProvider instance.
    /// </summary>
    /// <param name="configPath">Path to the ability-trees.json configuration file.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when configPath or logger is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the configuration file does not exist.</exception>
    /// <exception cref="InvalidDataException">Thrown when the configuration file is invalid.</exception>
    public JsonAbilityTreeProvider(string configPath, ILogger<JsonAbilityTreeProvider> logger)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _treesByTreeId = new Dictionary<string, AbilityTreeDefinition>(StringComparer.OrdinalIgnoreCase);
        _treesByClassId = new Dictionary<string, AbilityTreeDefinition>(StringComparer.OrdinalIgnoreCase);
        _nodesById = new Dictionary<string, AbilityTreeNode>(StringComparer.OrdinalIgnoreCase);
        _branchesByNodeId = new Dictionary<string, AbilityTreeBranch>(StringComparer.OrdinalIgnoreCase);
        _treesByNodeId = new Dictionary<string, AbilityTreeDefinition>(StringComparer.OrdinalIgnoreCase);

        _logger.LogDebug(
            "Loading ability tree definitions from {ConfigPath}",
            configPath);

        LoadTrees();

        _logger.LogInformation(
            "JsonAbilityTreeProvider loaded {TreeCount} trees with {NodeCount} total nodes",
            TreeCount,
            TotalNodeCount);
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public AbilityTreeDefinition? GetTreeForClass(string classId)
    {
        if (string.IsNullOrWhiteSpace(classId))
        {
            _logger.LogWarning("GetTreeForClass called with null or empty classId");
            return null;
        }

        var result = _treesByClassId.GetValueOrDefault(classId);

        if (result is null)
        {
            _logger.LogDebug(
                "No ability tree found for class: {ClassId}",
                classId);
        }

        return result;
    }

    /// <inheritdoc />
    public AbilityTreeDefinition? GetTree(string treeId)
    {
        if (string.IsNullOrWhiteSpace(treeId))
        {
            _logger.LogWarning("GetTree called with null or empty treeId");
            return null;
        }

        var result = _treesByTreeId.GetValueOrDefault(treeId);

        if (result is null)
        {
            _logger.LogDebug(
                "No ability tree found with ID: {TreeId}",
                treeId);
        }

        return result;
    }

    /// <inheritdoc />
    public IReadOnlyList<AbilityTreeDefinition> GetAllTrees()
    {
        return _treesByTreeId.Values.ToList();
    }

    /// <inheritdoc />
    public AbilityTreeNode? FindNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            _logger.LogWarning("FindNode called with null or empty nodeId");
            return null;
        }

        var result = _nodesById.GetValueOrDefault(nodeId);

        if (result is null)
        {
            _logger.LogDebug(
                "No ability tree node found with ID: {NodeId}",
                nodeId);
        }

        return result;
    }

    /// <inheritdoc />
    public AbilityTreeDefinition? GetTreeContainingNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            _logger.LogWarning("GetTreeContainingNode called with null or empty nodeId");
            return null;
        }

        return _treesByNodeId.GetValueOrDefault(nodeId);
    }

    /// <inheritdoc />
    public AbilityTreeBranch? GetBranchContainingNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            _logger.LogWarning("GetBranchContainingNode called with null or empty nodeId");
            return null;
        }

        return _branchesByNodeId.GetValueOrDefault(nodeId);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetClassesWithTrees()
    {
        return _treesByClassId.Keys.ToList();
    }

    /// <inheritdoc />
    public bool HasTreeForClass(string classId)
    {
        if (string.IsNullOrWhiteSpace(classId))
            return false;

        return _treesByClassId.ContainsKey(classId);
    }

    /// <inheritdoc />
    public bool NodeExists(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return false;

        return _nodesById.ContainsKey(nodeId);
    }

    /// <inheritdoc />
    public int TreeCount => _treesByTreeId.Count;

    /// <inheritdoc />
    public int TotalNodeCount => _nodesById.Count;

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads ability tree definitions from the JSON configuration file.
    /// </summary>
    private void LoadTrees()
    {
        if (!File.Exists(_configPath))
        {
            _logger.LogError(
                "Ability tree configuration file not found: {Path}",
                _configPath);

            throw new FileNotFoundException(
                $"Ability tree configuration file not found: {_configPath}",
                _configPath);
        }

        var json = File.ReadAllText(_configPath);

        _logger.LogDebug(
            "Read {Length} bytes from ability tree configuration",
            json.Length);

        var config = JsonSerializer.Deserialize<AbilityTreesConfigDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config?.AbilityTrees is null || config.AbilityTrees.Count == 0)
        {
            _logger.LogWarning(
                "Ability tree configuration is empty or invalid - no trees loaded");
            return;
        }

        foreach (var treeDto in config.AbilityTrees)
        {
            try
            {
                var tree = MapToTree(treeDto);
                IndexTree(tree);

                _logger.LogDebug(
                    "Loaded ability tree: {TreeName} for {ClassId} ({BranchCount} branches, {NodeCount} nodes)",
                    tree.Name,
                    tree.ClassId,
                    tree.GetBranchCount(),
                    tree.GetTotalNodeCount());
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to load ability tree '{TreeId}': {Error}",
                    treeDto.TreeId,
                    ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// Maps a tree DTO from JSON to domain entities.
    /// </summary>
    /// <param name="dto">The tree DTO from JSON deserialization.</param>
    /// <returns>An AbilityTreeDefinition with all branches and nodes.</returns>
    private AbilityTreeDefinition MapToTree(AbilityTreeDto dto)
    {
        var tree = AbilityTreeDefinition.Create(
            dto.TreeId,
            dto.ClassId,
            dto.Name,
            dto.Description ?? string.Empty,
            dto.PointsPerLevel,
            dto.Icon);

        var branches = new List<AbilityTreeBranch>();

        foreach (var branchDto in dto.Branches ?? [])
        {
            var branch = MapToBranch(branchDto);
            branches.Add(branch);

            _logger.LogDebug(
                "Loaded branch: {BranchName} ({NodeCount} nodes, max tier {MaxTier})",
                branch.Name,
                branch.GetNodeCount(),
                branch.GetMaxTier());
        }

        tree.SetBranches(branches);
        return tree;
    }

    /// <summary>
    /// Maps a branch DTO from JSON to a domain entity.
    /// </summary>
    /// <param name="dto">The branch DTO from JSON deserialization.</param>
    /// <returns>An AbilityTreeBranch with all nodes.</returns>
    private AbilityTreeBranch MapToBranch(AbilityTreeBranchDto dto)
    {
        var nodes = new List<AbilityTreeNode>();

        foreach (var nodeDto in dto.Nodes ?? [])
        {
            var node = MapToNode(nodeDto);
            nodes.Add(node);

            _logger.LogDebug(
                "Loaded node: {NodeName} (T{Tier}, {PointCost}pt, {MaxRank} rank(s))",
                node.Name,
                node.Tier,
                node.PointCost,
                node.MaxRank);
        }

        return new AbilityTreeBranch
        {
            BranchId = dto.BranchId.ToLowerInvariant(),
            Name = dto.Name,
            Description = dto.Description ?? string.Empty,
            Nodes = nodes,
            IconPath = dto.Icon
        };
    }

    /// <summary>
    /// Maps a node DTO from JSON to a domain entity.
    /// </summary>
    /// <param name="dto">The node DTO from JSON deserialization.</param>
    /// <returns>An AbilityTreeNode.</returns>
    private static AbilityTreeNode MapToNode(AbilityTreeNodeDto dto)
    {
        var statPrereqs = dto.StatPrerequisites?
            .Select(sp => new StatPrerequisite(sp.Stat, sp.MinValue))
            .ToList() ?? [];

        var nodePrereqs = dto.Prerequisites?
            .Select(p => p.ToLowerInvariant())
            .ToList() ?? [];

        return new AbilityTreeNode
        {
            NodeId = dto.NodeId.ToLowerInvariant(),
            AbilityId = dto.AbilityId.ToLowerInvariant(),
            Name = dto.Name,
            Description = dto.Description ?? string.Empty,
            Tier = dto.Tier,
            PointCost = Math.Max(1, dto.PointCost),
            MaxRank = Math.Max(1, dto.MaxRank),
            PrerequisiteNodeIds = nodePrereqs,
            StatPrerequisites = statPrereqs,
            Position = new NodePosition(dto.Position?.X ?? 0, dto.Position?.Y ?? 0),
            IconPath = dto.Icon
        };
    }

    /// <summary>
    /// Indexes a tree and all its contents for fast lookup.
    /// </summary>
    /// <param name="tree">The tree to index.</param>
    private void IndexTree(AbilityTreeDefinition tree)
    {
        _treesByTreeId[tree.TreeId] = tree;
        _treesByClassId[tree.ClassId] = tree;

        foreach (var branch in tree.Branches)
        {
            foreach (var node in branch.Nodes)
            {
                if (_nodesById.ContainsKey(node.NodeId))
                {
                    _logger.LogWarning(
                        "Duplicate node ID detected: {NodeId} - overwriting previous entry",
                        node.NodeId);
                }

                _nodesById[node.NodeId] = node;
                _branchesByNodeId[node.NodeId] = branch;
                _treesByNodeId[node.NodeId] = tree;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERNAL DTO CLASSES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Root configuration DTO for ability-trees.json.
    /// </summary>
    private sealed class AbilityTreesConfigDto
    {
        public List<AbilityTreeDto>? AbilityTrees { get; set; }
    }

    /// <summary>
    /// DTO for individual ability tree definitions in JSON.
    /// </summary>
    private sealed class AbilityTreeDto
    {
        public string TreeId { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int PointsPerLevel { get; set; } = 1;
        public string? Icon { get; set; }
        public List<AbilityTreeBranchDto>? Branches { get; set; }
    }

    /// <summary>
    /// DTO for ability tree branches in JSON.
    /// </summary>
    private sealed class AbilityTreeBranchDto
    {
        public string BranchId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public List<AbilityTreeNodeDto>? Nodes { get; set; }
    }

    /// <summary>
    /// DTO for ability tree nodes in JSON.
    /// </summary>
    private sealed class AbilityTreeNodeDto
    {
        public string NodeId { get; set; } = string.Empty;
        public string AbilityId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Tier { get; set; } = 1;
        public int PointCost { get; set; } = 1;
        public int MaxRank { get; set; } = 1;
        public List<string>? Prerequisites { get; set; }
        public List<StatPrerequisiteDto>? StatPrerequisites { get; set; }
        public PositionDto? Position { get; set; }
        public string? Icon { get; set; }
    }

    /// <summary>
    /// DTO for stat prerequisites in JSON.
    /// </summary>
    private sealed class StatPrerequisiteDto
    {
        public string Stat { get; set; } = string.Empty;
        public int MinValue { get; set; }
    }

    /// <summary>
    /// DTO for node positions in JSON.
    /// </summary>
    private sealed class PositionDto
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
