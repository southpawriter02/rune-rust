using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using System.IO;
using System.Text.Json;

namespace RuneAndRust.Tests;

/// <summary>
/// Tests for DungeonGenerator service (v0.10)
/// </summary>
[TestFixture]
public class DungeonGeneratorTests
{
    private string _testDataPath = string.Empty;
    private TemplateLibrary _library = null!;
    private DungeonGenerator _generator = null!;

    [SetUp]
    public void Setup()
    {
        // Create temporary test data directory with templates
        _testDataPath = Path.Combine(Path.GetTempPath(), $"RuneRustGenTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataPath);

        // Create minimal template set for testing
        CreateMinimalTemplateSet();

        // Initialize library and generator
        _library = new TemplateLibrary(_testDataPath);
        _library.LoadTemplates();
        _generator = new DungeonGenerator(_library);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDataPath))
        {
            Directory.Delete(_testDataPath, recursive: true);
        }
    }

    #region Basic Generation Tests

    [Test]
    public void Generate_CreatesValidDungeon()
    {
        // Act
        var graph = _generator.Generate(seed: 42, targetRoomCount: 5);

        // Assert
        Assert.That(graph, Is.Not.Null);
        Assert.That(graph.NodeCount, Is.GreaterThanOrEqualTo(5));

        var (isValid, errors) = graph.Validate();
        Assert.That(isValid, Is.True, $"Validation errors: {string.Join(", ", errors)}");
    }

    [Test]
    public void Generate_HasStartNode()
    {
        // Act
        var graph = _generator.Generate(seed: 42);

        // Assert
        Assert.That(graph.StartNode, Is.Not.Null);
        Assert.That(graph.StartNode!.Type, Is.EqualTo(NodeType.Start));
        Assert.That(graph.StartNode.Template.Archetype, Is.EqualTo(RoomArchetype.EntryHall));
    }

    [Test]
    public void Generate_HasBossNode()
    {
        // Act
        var graph = _generator.Generate(seed: 42);

        // Assert
        Assert.That(graph.BossNode, Is.Not.Null);
        Assert.That(graph.BossNode!.Type, Is.EqualTo(NodeType.Boss));
        Assert.That(graph.BossNode.Template.Archetype, Is.EqualTo(RoomArchetype.BossArena));
    }

    [Test]
    public void Generate_HasPathFromStartToBoss()
    {
        // Act
        var graph = _generator.Generate(seed: 42);

        // Assert
        var path = graph.GetMainPath();
        Assert.That(path, Is.Not.Null);
        Assert.That(path!.Count, Is.GreaterThanOrEqualTo(5)); // Minimum path length
        Assert.That(path[0], Is.EqualTo(graph.StartNode));
        Assert.That(path[^1], Is.EqualTo(graph.BossNode));
    }

    [Test]
    public void Generate_WithSameSeed_GeneratesIdenticalDungeons()
    {
        // Act
        var graph1 = _generator.Generate(seed: 12345);
        var graph2 = _generator.Generate(seed: 12345);

        // Assert
        Assert.That(graph1.NodeCount, Is.EqualTo(graph2.NodeCount));
        Assert.That(graph1.EdgeCount, Is.EqualTo(graph2.EdgeCount));

        // Check that templates match
        var nodes1 = graph1.GetNodes().OrderBy(n => n.Id).ToList();
        var nodes2 = graph2.GetNodes().OrderBy(n => n.Id).ToList();

        for (int i = 0; i < nodes1.Count; i++)
        {
            Assert.That(nodes1[i].Template.TemplateId, Is.EqualTo(nodes2[i].Template.TemplateId));
        }
    }

    [Test]
    public void Generate_WithDifferentSeeds_GeneratesDifferentDungeons()
    {
        // Act
        var graph1 = _generator.Generate(seed: 111);
        var graph2 = _generator.Generate(seed: 222);

        // Assert
        var nodes1 = graph1.GetNodes().Select(n => n.Template.TemplateId).ToList();
        var nodes2 = graph2.GetNodes().Select(n => n.Template.TemplateId).ToList();

        // At least some templates should be different (very high probability)
        Assert.That(nodes1, Is.Not.EqualTo(nodes2));
    }

    #endregion

    #region Room Count Tests

    [Test]
    public void Generate_RespectsTargetRoomCount()
    {
        // Act
        var graph = _generator.Generate(seed: 42, targetRoomCount: 7);

        // Assert
        // Should have at least target count (may have more due to branches/secrets)
        Assert.That(graph.NodeCount, Is.GreaterThanOrEqualTo(7));
    }

    [Test]
    public void Generate_WithSmallCount_GeneratesMinimumRooms()
    {
        // Act
        var graph = _generator.Generate(seed: 42, targetRoomCount: 3);

        // Assert
        // Should have at least Start + 3 intermediate + Boss = 5 rooms minimum
        Assert.That(graph.NodeCount, Is.GreaterThanOrEqualTo(5));
    }

    [Test]
    public void Generate_WithLargeCount_GeneratesMoreRooms()
    {
        // Act
        var graph = _generator.Generate(seed: 42, targetRoomCount: 10);

        // Assert
        Assert.That(graph.NodeCount, Is.GreaterThanOrEqualTo(10));
    }

    #endregion

    #region Branching Tests

    [Test]
    public void Generate_MayIncludeBranchNodes()
    {
        // Act - Generate multiple dungeons to find one with branches
        bool foundBranch = false;
        for (int seed = 0; seed < 20; seed++)
        {
            var graph = _generator.Generate(seed);
            var branchNodes = graph.GetNodesByType(NodeType.Branch);
            if (branchNodes.Count > 0)
            {
                foundBranch = true;
                Assert.That(branchNodes.Count, Is.GreaterThan(0));
                break;
            }
        }

        // With 20 attempts, very high probability of finding at least one branch
        Assert.That(foundBranch, Is.True, "No branches found in 20 dungeons (probabilistic failure)");
    }

    [Test]
    public void Generate_BranchNodesAreReachable()
    {
        // Act
        var graph = _generator.Generate(seed: 42);
        var branchNodes = graph.GetNodesByType(NodeType.Branch);

        // Assert
        foreach (var branch in branchNodes)
        {
            Assert.That(graph.IsReachable(graph.StartNode!, branch), Is.True);
        }
    }

    #endregion

    #region Secret Room Tests

    [Test]
    public void Generate_MayIncludeSecretNodes()
    {
        // Act - Generate multiple dungeons to find one with secrets
        bool foundSecret = false;
        for (int seed = 0; seed < 30; seed++)
        {
            var graph = _generator.Generate(seed);
            var secretNodes = graph.GetNodesByType(NodeType.Secret);
            if (secretNodes.Count > 0)
            {
                foundSecret = true;
                Assert.That(secretNodes.Count, Is.GreaterThan(0));
                break;
            }
        }

        // 30% chance per dungeon, so 30 attempts should find at least one
        Assert.That(foundSecret, Is.True, "No secrets found in 30 dungeons (probabilistic failure)");
    }

    [Test]
    public void Generate_SecretRoomsUseSecretEdges()
    {
        // Act - Find a dungeon with a secret room
        DungeonGraph? graphWithSecret = null;
        for (int seed = 0; seed < 30; seed++)
        {
            var graph = _generator.Generate(seed);
            if (graph.GetNodesByType(NodeType.Secret).Count > 0)
            {
                graphWithSecret = graph;
                break;
            }
        }

        // Assert
        if (graphWithSecret != null)
        {
            var secretNodes = graphWithSecret.GetNodesByType(NodeType.Secret);
            var secretEdges = graphWithSecret.GetEdges()
                .Where(e => e.Type == EdgeType.Secret)
                .ToList();

            Assert.That(secretEdges.Count, Is.GreaterThan(0), "Secret nodes exist but no secret edges");
        }
    }

    #endregion

    #region Node Depth Tests

    [Test]
    public void Generate_CalculatesNodeDepths()
    {
        // Act
        var graph = _generator.Generate(seed: 42);

        // Assert
        Assert.That(graph.StartNode!.Depth, Is.EqualTo(0));

        foreach (var node in graph.GetNodes())
        {
            Assert.That(node.Depth, Is.GreaterThanOrEqualTo(0));
        }

        // Boss should be deeper than start
        Assert.That(graph.BossNode!.Depth, Is.GreaterThan(graph.StartNode.Depth));
    }

    [Test]
    public void Generate_DepthIncreasesAlongMainPath()
    {
        // Act
        var graph = _generator.Generate(seed: 42);
        var mainPath = graph.GetMainPath();

        // Assert
        Assert.That(mainPath, Is.Not.Null);

        for (int i = 1; i < mainPath!.Count; i++)
        {
            Assert.That(mainPath[i].Depth, Is.GreaterThanOrEqualTo(mainPath[i - 1].Depth));
        }
    }

    #endregion

    #region Template Variety Tests

    [Test]
    public void Generate_UsesVarietyOfTemplates()
    {
        // Act
        var graph = _generator.Generate(seed: 42, targetRoomCount: 10);
        var templateIds = graph.GetNodes()
            .Select(n => n.Template.TemplateId)
            .Distinct()
            .ToList();

        // Assert - Should use more than one template
        Assert.That(templateIds.Count, Is.GreaterThan(1));
    }

    [Test]
    public void Generate_DoesNotOveruseTemplates()
    {
        // Act
        var graph = _generator.Generate(seed: 42, targetRoomCount: 10);
        var templateCounts = graph.GetNodes()
            .GroupBy(n => n.Template.TemplateId)
            .Select(g => g.Count())
            .ToList();

        // Assert - No single template should dominate (more than 60% usage)
        var maxUsage = templateCounts.Max();
        var totalNodes = graph.NodeCount;

        Assert.That(maxUsage / (double)totalNodes, Is.LessThan(0.6),
            "Template variety is low - one template used too frequently");
    }

    #endregion

    #region Statistics Tests

    [Test]
    public void Generate_ProducesReasonableStatistics()
    {
        // Act
        var graph = _generator.Generate(seed: 42, targetRoomCount: 7);
        var stats = graph.GetStatistics();

        // Assert
        Assert.That(stats["StartNodes"], Is.EqualTo(1));
        Assert.That(stats["BossNodes"], Is.EqualTo(1));
        Assert.That(stats["MainNodes"], Is.GreaterThanOrEqualTo(1));
        Assert.That(stats["TotalNodes"], Is.GreaterThanOrEqualTo(5));
        Assert.That(stats["TotalEdges"], Is.GreaterThanOrEqualTo(4)); // Minimum for connected path
    }

    #endregion

    #region Validation Tests

    [Test]
    public void Generate_AlwaysPassesValidation()
    {
        // Act & Assert - Generate multiple dungeons
        for (int seed = 0; seed < 10; seed++)
        {
            var graph = _generator.Generate(seed);
            var (isValid, errors) = graph.Validate();

            Assert.That(isValid, Is.True,
                $"Seed {seed} failed validation: {string.Join(", ", errors)}");
        }
    }

    [Test]
    public void Generate_AllNodesAreReachable()
    {
        // Act
        var graph = _generator.Generate(seed: 42);

        // Assert
        var reachable = graph.GetReachableNodes(graph.StartNode!);
        Assert.That(reachable.Count, Is.EqualTo(graph.NodeCount),
            "Some nodes are unreachable from start");
    }

    #endregion

    #region Helper Methods

    private void CreateMinimalTemplateSet()
    {
        // Create Entry Hall template
        SaveTemplate(new RoomTemplate
        {
            TemplateId = "test_entry",
            Archetype = RoomArchetype.EntryHall,
            NameTemplates = new List<string> { "Test Entry" },
            Adjectives = new List<string> { "Test" },
            DescriptionTemplates = new List<string> { "Test entry hall" },
            Details = new List<string> { "Detail" },
            ValidConnections = new List<RoomArchetype> { RoomArchetype.Corridor, RoomArchetype.Chamber },
            MinConnectionPoints = 1,
            MaxConnectionPoints = 2
        }, "test_entry.json");

        // Create Corridor template
        SaveTemplate(new RoomTemplate
        {
            TemplateId = "test_corridor",
            Archetype = RoomArchetype.Corridor,
            NameTemplates = new List<string> { "Test Corridor" },
            Adjectives = new List<string> { "Test" },
            DescriptionTemplates = new List<string> { "Test corridor" },
            Details = new List<string> { "Detail" },
            ValidConnections = new List<RoomArchetype> { RoomArchetype.Corridor, RoomArchetype.Chamber, RoomArchetype.Junction },
            MinConnectionPoints = 2,
            MaxConnectionPoints = 3
        }, "test_corridor.json");

        // Create Chamber template
        SaveTemplate(new RoomTemplate
        {
            TemplateId = "test_chamber",
            Archetype = RoomArchetype.Chamber,
            NameTemplates = new List<string> { "Test Chamber" },
            Adjectives = new List<string> { "Test" },
            DescriptionTemplates = new List<string> { "Test chamber" },
            Details = new List<string> { "Detail" },
            ValidConnections = new List<RoomArchetype> { RoomArchetype.Corridor, RoomArchetype.Chamber },
            MinConnectionPoints = 1,
            MaxConnectionPoints = 3
        }, "test_chamber.json");

        // Create Boss Arena template
        SaveTemplate(new RoomTemplate
        {
            TemplateId = "test_boss",
            Archetype = RoomArchetype.BossArena,
            NameTemplates = new List<string> { "Test Boss Arena" },
            Adjectives = new List<string> { "Test" },
            DescriptionTemplates = new List<string> { "Test boss arena" },
            Details = new List<string> { "Detail" },
            ValidConnections = new List<RoomArchetype> { RoomArchetype.Corridor },
            MinConnectionPoints = 1,
            MaxConnectionPoints = 2
        }, "test_boss.json");

        // Create Secret Room template
        SaveTemplate(new RoomTemplate
        {
            TemplateId = "test_secret",
            Archetype = RoomArchetype.SecretRoom,
            NameTemplates = new List<string> { "Test Secret" },
            Adjectives = new List<string> { "Test" },
            DescriptionTemplates = new List<string> { "Test secret room" },
            Details = new List<string> { "Detail" },
            ValidConnections = new List<RoomArchetype> { RoomArchetype.Corridor },
            MinConnectionPoints = 1,
            MaxConnectionPoints = 1
        }, "test_secret.json");
    }

    private void SaveTemplate(RoomTemplate template, string fileName)
    {
        var json = JsonSerializer.Serialize(template, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(Path.Combine(_testDataPath, fileName), json);
    }

    #endregion
}
