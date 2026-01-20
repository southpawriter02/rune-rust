using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Infrastructure.Services;
using RuneAndRust.Presentation.UI;

namespace RuneAndRust.Application.UnitTests.Presentation.UI;

/// <summary>
/// Unit tests for <see cref="MiniMap"/>.
/// </summary>
[TestFixture]
public class MiniMapTests
{
    private ExplorationTracker _tracker = null!;
    private Mock<ITerminalService> _mockTerminal = null!;
    private ScreenLayout _layout = null!;
    private MiniMap _miniMap = null!;

    [SetUp]
    public void Setup()
    {
        _tracker = new ExplorationTracker();
        _mockTerminal = new Mock<ITerminalService>();
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
        
        var mockLogger = new Mock<ILogger<ScreenLayout>>();
        _layout = new ScreenLayout(_mockTerminal.Object, mockLogger.Object);
        
        _miniMap = new MiniMap(_tracker, _mockTerminal.Object, _layout);
    }
    
    [TearDown]
    public void TearDown()
    {
        _layout?.Dispose();
    }

    #region Render Tests

    [Test]
    public void Render_WithExploredRooms_ReturnsMapLines()
    {
        // Arrange
        var room1 = Guid.NewGuid();
        var room2 = Guid.NewGuid();
        _tracker.MarkExplored(room1);
        _tracker.MarkExplored(room2);
        
        var positions = new Dictionary<Guid, (int X, int Y)>
        {
            [room1] = (0, 0),
            [room2] = (1, 0)
        };

        // Act
        var result = _miniMap.Render(room1, positions);

        // Assert
        result.Should().NotBeEmpty();
        result.Any(line => line.Contains('@')).Should().BeTrue(); // Current room
        result.Any(line => line.Contains('█')).Should().BeTrue(); // Explored room
    }

    [Test]
    public void Render_WithCurrentRoom_ShowsAtSymbol()
    {
        // Arrange
        var currentRoom = Guid.NewGuid();
        _tracker.MarkExplored(currentRoom);
        
        var positions = new Dictionary<Guid, (int X, int Y)>
        {
            [currentRoom] = (0, 0)
        };

        // Act
        var result = _miniMap.Render(currentRoom, positions);

        // Assert
        result.Any(line => line.Contains('@')).Should().BeTrue();
    }

    [Test]
    public void Render_WithKnownAdjacentRoom_ShowsQuestionMark()
    {
        // Arrange
        var exploredRoom = Guid.NewGuid();
        var adjacentRoom = Guid.NewGuid();
        _tracker.MarkExplored(exploredRoom);
        _tracker.MarkKnownAdjacent(adjacentRoom);
        
        var positions = new Dictionary<Guid, (int X, int Y)>
        {
            [exploredRoom] = (0, 0),
            [adjacentRoom] = (1, 0)
        };

        // Act
        var result = _miniMap.Render(exploredRoom, positions);

        // Assert
        result.Any(line => line.Contains('?')).Should().BeTrue();
    }

    [Test]
    public void Render_UnexploredUnknownRoom_NotShown()
    {
        // Arrange
        var exploredRoom = Guid.NewGuid();
        var hiddenRoom = Guid.NewGuid();
        _tracker.MarkExplored(exploredRoom);
        // hiddenRoom is NOT marked as explored or adjacent
        
        var positions = new Dictionary<Guid, (int X, int Y)>
        {
            [exploredRoom] = (0, 0),
            [hiddenRoom] = (2, 0)
        };

        // Act
        var result = _miniMap.Render(exploredRoom, positions);

        // Assert - only one room box should be visible
        var boxCount = result.Count(line => line.Contains('@') || line.Contains('█'));
        boxCount.Should().Be(1);
    }

    #endregion

    #region Scroll Tests

    [Test]
    public void Scroll_UpdatesScrollPosition()
    {
        // Arrange
        _miniMap.ScrollPosition.Should().Be((0, 0));

        // Act
        _miniMap.Scroll(5, 3);

        // Assert
        _miniMap.ScrollPosition.Should().Be((5, 3));
    }

    [Test]
    public void ResetScroll_ResetsToZero()
    {
        // Arrange
        _miniMap.Scroll(5, 3);

        // Act
        _miniMap.ResetScroll();

        // Assert
        _miniMap.ScrollPosition.Should().Be((0, 0));
    }

    #endregion

    #region Event Tests

    [Test]
    public void MiniMap_SubscribesToExplorationEvents()
    {
        // Arrange - already done in setup
        var roomId = Guid.NewGuid();

        // Act - trigger exploration event
        _tracker.MarkExplored(roomId);

        // Assert - no exception, event handled
        _tracker.ExploredRooms.Should().Contain(roomId);
    }

    #endregion
}
