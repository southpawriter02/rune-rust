using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Rendering;
using Xunit;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Tests for the SpecializationScreenRenderer class.
/// Validates rendering logic and proper handling of view models.
/// </summary>
/// <remarks>See: v0.4.1c (The Tree of Runes) for implementation.</remarks>
public class SpecializationScreenRendererTests
{
    private readonly Mock<ILogger<SpecializationScreenRenderer>> _mockLogger;
    private readonly Mock<IThemeService> _mockTheme;
    private readonly SpecializationScreenRenderer _renderer;

    public SpecializationScreenRendererTests()
    {
        _mockLogger = new Mock<ILogger<SpecializationScreenRenderer>>();
        _mockTheme = new Mock<IThemeService>();
        _renderer = new SpecializationScreenRenderer(_mockLogger.Object, _mockTheme.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_ShouldNotThrow()
    {
        // Arrange & Act
        var action = () => new SpecializationScreenRenderer(_mockLogger.Object, _mockTheme.Object);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region Render Tests - Basic

    [Fact]
    public void Render_WithEmptySpecializationList_ShouldNotThrow()
    {
        // Arrange
        var viewModel = new SpecializationViewModel(
            "TestHero",
            15,
            SpecializationViewMode.SpecList,
            new List<SpecializationListItem>(),
            0,
            null,
            0);

        // Act
        var action = () => _renderer.Render(viewModel);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithNullTree_ShouldNotThrow()
    {
        // Arrange
        var viewModel = new SpecializationViewModel(
            "TestHero",
            15,
            SpecializationViewMode.SpecList,
            new List<SpecializationListItem>
            {
                new(Guid.NewGuid(), "Iron Warden", "Tank", true, true, 10, 3, 12)
            },
            0,
            null, // No tree selected
            0);

        // Act
        var action = () => _renderer.Render(viewModel);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithValidSpecializationList_ShouldNotThrow()
    {
        // Arrange
        var viewModel = CreateTestViewModel();

        // Act
        var action = () => _renderer.Render(viewModel);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region Render Tests - ViewMode

    [Fact]
    public void Render_InSpecListMode_ShouldLogCorrectViewMode()
    {
        // Arrange
        var viewModel = CreateTestViewModel(SpecializationViewMode.SpecList);

        // Act
        _renderer.Render(viewModel);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ViewMode=SpecList")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Render_InTreeDetailMode_ShouldLogCorrectViewMode()
    {
        // Arrange
        var viewModel = CreateTestViewModel(SpecializationViewMode.TreeDetail);

        // Act
        _renderer.Render(viewModel);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ViewMode=TreeDetail")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Render Tests - Status Message

    [Fact]
    public void Render_WithStatusMessage_ShouldNotThrow()
    {
        // Arrange
        var viewModel = CreateTestViewModel() with { StatusMessage = "Unlocked Iron Warden!" };

        // Act
        var action = () => _renderer.Render(viewModel);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithNullStatusMessage_ShouldNotThrow()
    {
        // Arrange
        var viewModel = CreateTestViewModel() with { StatusMessage = null };

        // Act
        var action = () => _renderer.Render(viewModel);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithEmptyStatusMessage_ShouldNotThrow()
    {
        // Arrange
        var viewModel = CreateTestViewModel() with { StatusMessage = "" };

        // Act
        var action = () => _renderer.Render(viewModel);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region Render Tests - Node Statuses

    [Theory]
    [InlineData(NodeStatus.Locked)]
    [InlineData(NodeStatus.InsufficientPP)]
    [InlineData(NodeStatus.Available)]
    [InlineData(NodeStatus.Unlocked)]
    public void Render_WithNodeStatus_ShouldNotThrow(NodeStatus status)
    {
        // Arrange
        var nodes = new List<NodeDisplayItem>
        {
            new(Guid.NewGuid(), "Test Node", "Test", "Description", 1, 1, status, 0, 0, false, new List<Guid>())
        };

        var tree = new SpecializationTreeView(Guid.NewGuid(), "Test Spec", nodes);

        var viewModel = new SpecializationViewModel(
            "Hero", 10, SpecializationViewMode.TreeDetail,
            new List<SpecializationListItem>(),
            0, tree, 0);

        // Act
        var action = () => _renderer.Render(viewModel);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region Render Tests - Tiers

    [Fact]
    public void Render_WithMultipleTiers_ShouldNotThrow()
    {
        // Arrange
        var nodes = new List<NodeDisplayItem>
        {
            CreateNode("Tier1 Node", 1, NodeStatus.Unlocked),
            CreateNode("Tier2 Node", 2, NodeStatus.Available),
            CreateNode("Tier3 Node", 3, NodeStatus.Locked),
            CreateNode("Capstone", 4, NodeStatus.Locked, isCapstone: true)
        };

        var tree = new SpecializationTreeView(Guid.NewGuid(), "Full Tree", nodes);

        var viewModel = new SpecializationViewModel(
            "Hero", 10, SpecializationViewMode.TreeDetail,
            new List<SpecializationListItem>(),
            0, tree, 0);

        // Act
        var action = () => _renderer.Render(viewModel);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region Render Tests - Special Characters

    [Fact]
    public void Render_WithSpecialCharactersInName_ShouldEscapeMarkup()
    {
        // Arrange
        var specs = new List<SpecializationListItem>
        {
            new(Guid.NewGuid(), "[Bold] Test", "Description", false, true, 10, 0, 5)
        };

        var viewModel = new SpecializationViewModel(
            "Hero [1]", 10, SpecializationViewMode.SpecList,
            specs, 0, null, 0);

        // Act
        var action = () => _renderer.Render(viewModel);

        // Assert - Should not throw due to markup escaping
        action.Should().NotThrow();
    }

    #endregion

    #region Render Tests - Selection

    [Fact]
    public void Render_WithSelectedSpec_ShouldNotThrow()
    {
        // Arrange
        var specs = new List<SpecializationListItem>
        {
            new(Guid.NewGuid(), "Spec 1", "Desc", true, true, 10, 2, 5),
            new(Guid.NewGuid(), "Spec 2", "Desc", false, true, 10, 0, 5),
            new(Guid.NewGuid(), "Spec 3", "Desc", false, false, 10, 0, 5)
        };

        var viewModel = new SpecializationViewModel(
            "Hero", 10, SpecializationViewMode.SpecList,
            specs, 1, null, 0); // Index 1 selected

        // Act
        var action = () => _renderer.Render(viewModel);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithSelectedNode_ShouldNotThrow()
    {
        // Arrange
        var nodes = new List<NodeDisplayItem>
        {
            CreateNode("Node 1", 1, NodeStatus.Unlocked),
            CreateNode("Node 2", 1, NodeStatus.Available),
            CreateNode("Node 3", 2, NodeStatus.Locked)
        };

        var tree = new SpecializationTreeView(Guid.NewGuid(), "Test", nodes);

        var viewModel = new SpecializationViewModel(
            "Hero", 10, SpecializationViewMode.TreeDetail,
            new List<SpecializationListItem>(),
            0, tree, 1); // Node index 1 selected

        // Act
        var action = () => _renderer.Render(viewModel);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region Render Tests - Edge Cases

    [Fact]
    public void Render_WithZeroProgressionPoints_ShouldNotThrow()
    {
        // Arrange
        var viewModel = new SpecializationViewModel(
            "Broke Hero", 0, SpecializationViewMode.SpecList,
            new List<SpecializationListItem>(),
            0, null, 0);

        // Act
        var action = () => _renderer.Render(viewModel);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithLargeProgressionPoints_ShouldNotThrow()
    {
        // Arrange
        var viewModel = new SpecializationViewModel(
            "Rich Hero", 999, SpecializationViewMode.SpecList,
            new List<SpecializationListItem>(),
            0, null, 0);

        // Act
        var action = () => _renderer.Render(viewModel);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithLongCharacterName_ShouldNotThrow()
    {
        // Arrange
        var longName = new string('A', 100);
        var viewModel = new SpecializationViewModel(
            longName, 10, SpecializationViewMode.SpecList,
            new List<SpecializationListItem>(),
            0, null, 0);

        // Act
        var action = () => _renderer.Render(viewModel);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithManySpecializations_ShouldNotThrow()
    {
        // Arrange
        var specs = Enumerable.Range(1, 20)
            .Select(i => new SpecializationListItem(
                Guid.NewGuid(), $"Spec {i}", $"Description {i}",
                i % 3 == 0, i % 2 == 0, 10, i % 5, 12))
            .ToList();

        var viewModel = new SpecializationViewModel(
            "Hero", 50, SpecializationViewMode.SpecList,
            specs, 10, null, 0);

        // Act
        var action = () => _renderer.Render(viewModel);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Render_WithManyNodes_ShouldNotThrow()
    {
        // Arrange
        var nodes = new List<NodeDisplayItem>();
        for (int tier = 1; tier <= 4; tier++)
        {
            for (int i = 0; i < 5; i++)
            {
                nodes.Add(CreateNode($"T{tier}N{i}", tier,
                    tier == 1 ? NodeStatus.Unlocked : NodeStatus.Locked,
                    tier == 4));
            }
        }

        var tree = new SpecializationTreeView(Guid.NewGuid(), "Big Tree", nodes);

        var viewModel = new SpecializationViewModel(
            "Hero", 10, SpecializationViewMode.TreeDetail,
            new List<SpecializationListItem>(),
            0, tree, 5);

        // Act
        var action = () => _renderer.Render(viewModel);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region Helper Methods

    private SpecializationViewModel CreateTestViewModel(
        SpecializationViewMode viewMode = SpecializationViewMode.SpecList)
    {
        var specs = new List<SpecializationListItem>
        {
            new(Guid.NewGuid(), "Iron Warden", "Tank specialization", true, true, 10, 3, 12),
            new(Guid.NewGuid(), "Storm Caller", "Lightning magic", false, true, 10, 0, 10),
            new(Guid.NewGuid(), "Shadow Dancer", "Stealth combat", false, false, 10, 0, 8)
        };

        var nodes = new List<NodeDisplayItem>
        {
            CreateNode("Shield Wall", 1, NodeStatus.Unlocked),
            CreateNode("Iron Skin", 1, NodeStatus.Unlocked),
            CreateNode("Bash", 2, NodeStatus.Available),
            CreateNode("Fortress", 4, NodeStatus.Locked, true)
        };

        var tree = new SpecializationTreeView(specs[0].Id, "Iron Warden", nodes);

        return new SpecializationViewModel(
            "TestHero",
            15,
            viewMode,
            specs,
            0,
            tree,
            2);
    }

    private static NodeDisplayItem CreateNode(
        string name,
        int tier,
        NodeStatus status,
        bool isCapstone = false)
    {
        var cost = tier switch
        {
            1 => 1,
            2 => 2,
            3 => 3,
            4 => 5,
            _ => 1
        };

        return new NodeDisplayItem(
            Guid.NewGuid(),
            name,
            name,
            $"Description of {name}",
            tier,
            cost,
            status,
            0,
            tier - 1,
            isCapstone || tier == 4,
            new List<Guid>());
    }

    #endregion
}
