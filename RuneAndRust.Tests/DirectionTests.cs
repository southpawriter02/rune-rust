using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// Tests for Direction enum and DirectionAssigner (v0.10)
/// </summary>
[TestFixture]
public class DirectionTests
{
    #region Direction Extension Tests

    [Test]
    public void GetOpposite_ReturnsCorrectOpposite()
    {
        Assert.That(Direction.North.GetOpposite(), Is.EqualTo(Direction.South));
        Assert.That(Direction.South.GetOpposite(), Is.EqualTo(Direction.North));
        Assert.That(Direction.East.GetOpposite(), Is.EqualTo(Direction.West));
        Assert.That(Direction.West.GetOpposite(), Is.EqualTo(Direction.East));
    }

    [Test]
    public void GetOpposite_IsReversible()
    {
        foreach (var direction in DirectionExtensions.GetAllDirections())
        {
            var opposite = direction.GetOpposite();
            var reverse = opposite.GetOpposite();
            Assert.That(reverse, Is.EqualTo(direction));
        }
    }

    [Test]
    public void ToNavigationString_ReturnsLowercaseString()
    {
        Assert.That(Direction.North.ToNavigationString(), Is.EqualTo("north"));
        Assert.That(Direction.South.ToNavigationString(), Is.EqualTo("south"));
        Assert.That(Direction.East.ToNavigationString(), Is.EqualTo("east"));
        Assert.That(Direction.West.ToNavigationString(), Is.EqualTo("west"));
    }

    [Test]
    public void FromString_ParsesFullNames()
    {
        Assert.That(DirectionExtensions.FromString("north"), Is.EqualTo(Direction.North));
        Assert.That(DirectionExtensions.FromString("south"), Is.EqualTo(Direction.South));
        Assert.That(DirectionExtensions.FromString("east"), Is.EqualTo(Direction.East));
        Assert.That(DirectionExtensions.FromString("west"), Is.EqualTo(Direction.West));
    }

    [Test]
    public void FromString_ParsesAbbreviations()
    {
        Assert.That(DirectionExtensions.FromString("n"), Is.EqualTo(Direction.North));
        Assert.That(DirectionExtensions.FromString("s"), Is.EqualTo(Direction.South));
        Assert.That(DirectionExtensions.FromString("e"), Is.EqualTo(Direction.East));
        Assert.That(DirectionExtensions.FromString("w"), Is.EqualTo(Direction.West));
    }

    [Test]
    public void FromString_IsCaseInsensitive()
    {
        Assert.That(DirectionExtensions.FromString("NORTH"), Is.EqualTo(Direction.North));
        Assert.That(DirectionExtensions.FromString("North"), Is.EqualTo(Direction.North));
        Assert.That(DirectionExtensions.FromString("NoRtH"), Is.EqualTo(Direction.North));
    }

    [Test]
    public void FromString_WithInvalidString_ReturnsNull()
    {
        Assert.That(DirectionExtensions.FromString("invalid"), Is.Null);
        Assert.That(DirectionExtensions.FromString("up"), Is.Null);
        Assert.That(DirectionExtensions.FromString(""), Is.Null);
    }

    [Test]
    public void GetAllDirections_ReturnsFourDirections()
    {
        var directions = DirectionExtensions.GetAllDirections();

        Assert.That(directions.Length, Is.EqualTo(4));
        Assert.That(directions, Contains.Item(Direction.North));
        Assert.That(directions, Contains.Item(Direction.South));
        Assert.That(directions, Contains.Item(Direction.East));
        Assert.That(directions, Contains.Item(Direction.West));
    }

    #endregion

    #region DirectionAssigner Tests

    [Test]
    public void AssignDirections_AssignsDirectionsToAllEdges()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var assigner = new DirectionAssigner();
        var rng = new Random(42);

        // Act
        assigner.AssignDirections(graph, rng);

        // Assert
        foreach (var edge in graph.GetEdges())
        {
            Assert.That(edge.HasDirections(), Is.True,
                $"Edge {edge.From.Id} -> {edge.To.Id} missing directions");
        }
    }

    [Test]
    public void AssignDirections_UsesOppositeDirections()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var assigner = new DirectionAssigner();
        var rng = new Random(42);

        // Act
        assigner.AssignDirections(graph, rng);

        // Assert
        foreach (var edge in graph.GetEdges())
        {
            if (edge.IsBidirectional)
            {
                var expectedOpposite = edge.FromDirection!.Value.GetOpposite();
                Assert.That(edge.ToDirection, Is.EqualTo(expectedOpposite),
                    $"Edge {edge.From.Id} -> {edge.To.Id} has inconsistent directions");
            }
        }
    }

    [Test]
    public void AssignDirections_NoNodeHasDuplicateDirections()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var assigner = new DirectionAssigner();
        var rng = new Random(42);

        // Act
        assigner.AssignDirections(graph, rng);

        // Assert
        foreach (var node in graph.GetNodes())
        {
            var edgesFromNode = graph.GetEdgesFrom(node);
            var directions = edgesFromNode
                .Select(e => e.FromDirection)
                .Where(d => d.HasValue)
                .Select(d => d!.Value)
                .ToList();

            var distinctCount = directions.Distinct().Count();
            Assert.That(distinctCount, Is.EqualTo(directions.Count),
                $"Node {node.Id} has duplicate directions");
        }
    }

    [Test]
    public void AssignDirections_WithSameSeed_AssignsSameDirections()
    {
        // Arrange
        var graph1 = CreateSimpleGraph();
        var graph2 = CreateSimpleGraph();
        var assigner = new DirectionAssigner();

        // Act
        assigner.AssignDirections(graph1, new Random(12345));
        assigner.AssignDirections(graph2, new Random(12345));

        // Assert
        var edges1 = graph1.GetEdges().OrderBy(e => e.From.Id).ThenBy(e => e.To.Id).ToList();
        var edges2 = graph2.GetEdges().OrderBy(e => e.From.Id).ThenBy(e => e.To.Id).ToList();

        for (int i = 0; i < edges1.Count; i++)
        {
            Assert.That(edges1[i].FromDirection, Is.EqualTo(edges2[i].FromDirection));
            Assert.That(edges1[i].ToDirection, Is.EqualTo(edges2[i].ToDirection));
        }
    }

    [Test]
    public void AssignDirections_WithComplexGraph_AssignsValidDirections()
    {
        // Arrange
        var graph = CreateComplexGraph();
        var assigner = new DirectionAssigner();
        var rng = new Random(42);

        // Act
        assigner.AssignDirections(graph, rng);

        // Assert
        var (isValid, errors) = assigner.ValidateDirections(graph);
        Assert.That(isValid, Is.True, $"Direction validation failed: {string.Join(", ", errors)}");
    }

    [Test]
    public void ValidateDirections_WithValidGraph_ReturnsTrue()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var assigner = new DirectionAssigner();
        var rng = new Random(42);
        assigner.AssignDirections(graph, rng);

        // Act
        var (isValid, errors) = assigner.ValidateDirections(graph);

        // Assert
        Assert.That(isValid, Is.True);
        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void ValidateDirections_WithUnassignedEdges_ReturnsError()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var assigner = new DirectionAssigner();

        // Act (don't assign directions)
        var (isValid, errors) = assigner.ValidateDirections(graph);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors, Has.Some.Contains("no directions assigned"));
    }

    [Test]
    public void GetDirectionStatistics_ReturnsCorrectCounts()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var assigner = new DirectionAssigner();
        var rng = new Random(42);
        assigner.AssignDirections(graph, rng);

        // Act
        var stats = assigner.GetDirectionStatistics(graph);

        // Assert
        Assert.That(stats["TotalEdges"], Is.EqualTo(graph.EdgeCount));
        Assert.That(stats["AssignedEdges"], Is.EqualTo(graph.EdgeCount));
        Assert.That(stats["UnassignedEdges"], Is.EqualTo(0));

        // All edges should be accounted for in directional counts
        var directionalTotal = stats["NorthEdges"] + stats["SouthEdges"] +
                              stats["EastEdges"] + stats["WestEdges"];
        Assert.That(directionalTotal, Is.EqualTo(graph.EdgeCount));
    }

    #endregion

    #region Helper Methods

    private DungeonGraph CreateSimpleGraph()
    {
        var graph = new DungeonGraph();
        var template = CreateTestTemplate();

        // Create linear path: Start -> A -> B -> Boss
        var start = CreateNode(template, NodeType.Start);
        var a = CreateNode(template, NodeType.Main);
        var b = CreateNode(template, NodeType.Main);
        var boss = CreateNode(template, NodeType.Boss);

        graph.AddNode(start);
        graph.AddNode(a);
        graph.AddNode(b);
        graph.AddNode(boss);

        graph.AddEdge(start, a);
        graph.AddEdge(a, b);
        graph.AddEdge(b, boss);

        return graph;
    }

    private DungeonGraph CreateComplexGraph()
    {
        var graph = new DungeonGraph();
        var template = CreateTestTemplate();

        // Create branching path:
        //     Start -> A -> B -> Boss
        //               \-> C /
        var start = CreateNode(template, NodeType.Start);
        var a = CreateNode(template, NodeType.Main);
        var b = CreateNode(template, NodeType.Main);
        var c = CreateNode(template, NodeType.Branch);
        var boss = CreateNode(template, NodeType.Boss);

        graph.AddNode(start);
        graph.AddNode(a);
        graph.AddNode(b);
        graph.AddNode(c);
        graph.AddNode(boss);

        graph.AddEdge(start, a);
        graph.AddEdge(a, b);
        graph.AddEdge(b, boss);
        graph.AddEdge(a, c); // Branch
        graph.AddEdge(c, boss); // Rejoin

        return graph;
    }

    private DungeonNode CreateNode(RoomTemplate template, NodeType type)
    {
        return new DungeonNode
        {
            Template = template,
            Type = type,
            Name = $"Test {type} Node"
        };
    }

    private RoomTemplate CreateTestTemplate()
    {
        return new RoomTemplate
        {
            TemplateId = "test_template",
            Archetype = RoomArchetype.Chamber,
            NameTemplates = new List<string> { "Test Room" },
            Adjectives = new List<string> { "Test" },
            DescriptionTemplates = new List<string> { "Test description" },
            Details = new List<string> { "Test detail" },
            ValidConnections = new List<RoomArchetype> { RoomArchetype.Corridor },
            MinConnectionPoints = 1,
            MaxConnectionPoints = 4
        };
    }

    #endregion
}
