using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Rendering;
using Xunit;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Tests for SpecializationGridRenderer.
/// Validates rendering logic for the specialization grid UI.
/// </summary>
/// <remarks>See: v0.4.1d (The Grid) for implementation.</remarks>
public class SpecializationGridRendererTests
{
    private readonly IThemeService _themeService;
    private readonly ILogger<SpecializationGridRenderer> _logger;
    private readonly SpecializationGridRenderer _renderer;

    public SpecializationGridRendererTests()
    {
        _themeService = Substitute.For<IThemeService>();
        _logger = Substitute.For<ILogger<SpecializationGridRenderer>>();
        _renderer = new SpecializationGridRenderer(_themeService, _logger);
    }

    private static SpecializationGridViewModel CreateTestViewModel(int nodeCount = 4)
    {
        var nodes = new List<NodeViewModel>();
        for (int i = 0; i < nodeCount; i++)
        {
            nodes.Add(new NodeViewModel(
                NodeId: Guid.NewGuid(),
                AbilityId: Guid.NewGuid(),
                Name: $"Node{i + 1}",
                Description: $"Description for node {i + 1}",
                Tier: (i / 2) + 1,
                CostPP: i + 1,
                Status: i == 0 ? NodeStatus.Available : NodeStatus.Locked,
                PositionX: i,
                PositionY: 0,
                IsCapstone: i == 3,
                ParentNodeIds: new List<Guid>()));
        }

        var nodesByTier = nodes
            .GroupBy(n => n.Tier)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<NodeViewModel>)g.ToList());

        return new SpecializationGridViewModel
        {
            SpecializationId = Guid.NewGuid(),
            SpecializationName = "Iron Warden",
            SpecializationDescription = "A defensive specialization",
            ProgressionPoints = 10,
            CharacterName = "TestChar",
            NodesByTier = nodesByTier,
            AllNodes = nodes,
            SelectedNodeIndex = 0,
            CurrentSpecIndex = 0,
            TotalSpecCount = 1
        };
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Assert
        _renderer.Should().NotBeNull();
    }

    #endregion

    #region Render Tests

    [Fact]
    public void Render_WithValidViewModel_DoesNotThrow()
    {
        // Arrange
        var vm = CreateTestViewModel();

        // Act
        var act = () => _renderer.Render(vm);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Render_WithEmptyNodes_DoesNotThrow()
    {
        // Arrange
        var vm = new SpecializationGridViewModel
        {
            SpecializationId = Guid.NewGuid(),
            SpecializationName = "Empty Spec",
            ProgressionPoints = 10,
            CharacterName = "TestChar",
            NodesByTier = new Dictionary<int, IReadOnlyList<NodeViewModel>>(),
            AllNodes = new List<NodeViewModel>(),
            SelectedNodeIndex = 0,
            CurrentSpecIndex = 0,
            TotalSpecCount = 1
        };

        // Act
        var act = () => _renderer.Render(vm);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Render_WithNullSelectedNode_DoesNotThrow()
    {
        // Arrange
        var vm = new SpecializationGridViewModel
        {
            SpecializationId = Guid.NewGuid(),
            SpecializationName = "Empty Spec",
            ProgressionPoints = 10,
            CharacterName = "TestChar",
            NodesByTier = new Dictionary<int, IReadOnlyList<NodeViewModel>>(),
            AllNodes = new List<NodeViewModel>(),
            SelectedNodeIndex = 0, // No nodes, so no selection
            CurrentSpecIndex = 0,
            TotalSpecCount = 1
        };

        // Act
        var act = () => _renderer.Render(vm);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Render_WithFeedbackMessage_DoesNotThrow()
    {
        // Arrange
        var vm = CreateTestViewModel();
        vm.FeedbackMessage = "Node unlocked successfully!";
        vm.FeedbackIsSuccess = true;

        // Act
        var act = () => _renderer.Render(vm);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Render_WithErrorFeedbackMessage_DoesNotThrow()
    {
        // Arrange
        var vm = CreateTestViewModel();
        vm.FeedbackMessage = "Prerequisites not met";
        vm.FeedbackIsSuccess = false;

        // Act
        var act = () => _renderer.Render(vm);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Render_WithMultiSpec_DoesNotThrow()
    {
        // Arrange
        var vm = CreateTestViewModel();
        vm.CurrentSpecIndex = 1;
        vm.TotalSpecCount = 3;

        // Act
        var act = () => _renderer.Render(vm);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Render_WithLongNodeName_DoesNotThrow()
    {
        // Arrange
        var nodes = new List<NodeViewModel>
        {
            new(Guid.NewGuid(), Guid.NewGuid(),
                "This Is A Very Long Node Name That Should Be Truncated",
                "Description", 1, 1, NodeStatus.Available, 0, 0, false, new List<Guid>())
        };
        var vm = new SpecializationGridViewModel
        {
            SpecializationId = Guid.NewGuid(),
            SpecializationName = "Test",
            ProgressionPoints = 10,
            CharacterName = "TestChar",
            NodesByTier = new Dictionary<int, IReadOnlyList<NodeViewModel>> { { 1, nodes } },
            AllNodes = nodes,
            SelectedNodeIndex = 0,
            CurrentSpecIndex = 0,
            TotalSpecCount = 1
        };

        // Act
        var act = () => _renderer.Render(vm);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Render_WithAllNodeStatuses_DoesNotThrow()
    {
        // Arrange
        var nodes = new List<NodeViewModel>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Locked", "Desc", 1, 1,
                NodeStatus.Locked, 0, 0, false, new List<Guid>()),
            new(Guid.NewGuid(), Guid.NewGuid(), "Available", "Desc", 1, 1,
                NodeStatus.Available, 0, 1, false, new List<Guid>()),
            new(Guid.NewGuid(), Guid.NewGuid(), "Unlocked", "Desc", 2, 1,
                NodeStatus.Unlocked, 1, 0, false, new List<Guid>()),
            new(Guid.NewGuid(), Guid.NewGuid(), "Affordable", "Desc", 2, 1,
                NodeStatus.Affordable, 1, 1, false, new List<Guid>())
        };
        var nodesByTier = nodes
            .GroupBy(n => n.Tier)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<NodeViewModel>)g.ToList());
        var vm = new SpecializationGridViewModel
        {
            SpecializationId = Guid.NewGuid(),
            SpecializationName = "Test",
            ProgressionPoints = 10,
            CharacterName = "TestChar",
            NodesByTier = nodesByTier,
            AllNodes = nodes,
            SelectedNodeIndex = 0,
            CurrentSpecIndex = 0,
            TotalSpecCount = 1
        };

        // Act
        var act = () => _renderer.Render(vm);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Render_WithCapstoneNode_DoesNotThrow()
    {
        // Arrange
        var nodes = new List<NodeViewModel>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Capstone", "Ultimate ability", 4, 5,
                NodeStatus.Locked, 3, 0, true, new List<Guid>())
        };
        var vm = new SpecializationGridViewModel
        {
            SpecializationId = Guid.NewGuid(),
            SpecializationName = "Test",
            ProgressionPoints = 10,
            CharacterName = "TestChar",
            NodesByTier = new Dictionary<int, IReadOnlyList<NodeViewModel>> { { 4, nodes } },
            AllNodes = nodes,
            SelectedNodeIndex = 0,
            CurrentSpecIndex = 0,
            TotalSpecCount = 1
        };

        // Act
        var act = () => _renderer.Render(vm);

        // Assert
        act.Should().NotThrow();
    }

    #endregion
}
