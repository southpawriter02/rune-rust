using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.UI;

namespace RuneAndRust.Application.UnitTests.Presentation.UI;

/// <summary>
/// Unit tests for <see cref="AsciiRoomRenderer"/>.
/// </summary>
[TestFixture]
public class AsciiRoomRendererTests
{
    private Mock<IRoomArtProvider> _mockArtProvider = null!;
    private Mock<ITerminalService> _mockTerminal = null!;
    private AsciiRoomRenderer _renderer = null!;

    [SetUp]
    public void Setup()
    {
        _mockArtProvider = new Mock<IRoomArtProvider>();
        _mockTerminal = new Mock<ITerminalService>();
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
        
        // Create actual ScreenLayout with mocked terminal and logger
        var mockLogger = new Mock<ILogger<ScreenLayout>>();
        var layout = new ScreenLayout(_mockTerminal.Object, mockLogger.Object);
        
        _renderer = new AsciiRoomRenderer(
            _mockArtProvider.Object,
            _mockTerminal.Object,
            layout);
    }

    #region RenderRoom Tests

    [Test]
    public void RenderRoom_WithCustomArt_ReturnsArtLines()
    {
        // Arrange
        var artDef = new RoomArtDefinition(
            "test-room",
            new[] { "Line1", "Line2", "Line3" },
            new Dictionary<char, string>());
        
        _mockArtProvider.Setup(p => p.GetArtForRoom("test-room"))
            .Returns(artDef);

        // Act
        var result = _renderer.RenderRoom("test-room");

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().Be("Line1");
    }

    [Test]
    public void RenderRoom_WithoutCustomArt_ReturnsDefaultArt()
    {
        // Arrange
        _mockArtProvider.Setup(p => p.GetArtForRoom("unknown-room"))
            .Returns((RoomArtDefinition?)null);

        // Act
        var result = _renderer.RenderRoom("unknown-room");

        // Assert
        result.Should().NotBeEmpty();
        result.Any(line => line.Contains("unknown-room")).Should().BeTrue();
    }

    #endregion

    #region ScaleArt Tests

    [Test]
    public void ScaleArt_NarrowerThanTarget_CentersArt()
    {
        // Arrange
        var art = new[] { "ABC", "DEF" };

        // Act
        var result = _renderer.ScaleArt(art, 10);

        // Assert
        result[0].Should().StartWith("   "); // Centered padding
        result[0].Should().Contain("ABC");
    }

    [Test]
    public void ScaleArt_WiderThanTarget_TruncatesArt()
    {
        // Arrange
        var art = new[] { "ABCDEFGHIJ" };

        // Act
        var result = _renderer.ScaleArt(art, 5);

        // Assert
        result[0].Should().Be("ABCDE");
    }

    [Test]
    public void ScaleArt_EmptyArt_ReturnsEmpty()
    {
        // Arrange
        var art = Array.Empty<string>();

        // Act
        var result = _renderer.ScaleArt(art, 10);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region OverlayEntities Tests

    [Test]
    public void OverlayEntities_OverlaysSymbolsAtPositions()
    {
        // Arrange
        var art = new[] { ".....", ".....", "....." };
        var entities = new List<(string, int, int, char)>
        {
            ("Player", 2, 1, '@'),
            ("Monster", 4, 2, 'M')
        };

        // Act
        var result = _renderer.OverlayEntities(art, entities);

        // Assert
        result[1][2].Should().Be('@');
        result[2][4].Should().Be('M');
    }

    [Test]
    public void OverlayEntities_OutOfBounds_DoesNotCrash()
    {
        // Arrange
        var art = new[] { "...", "..." };
        var entities = new List<(string, int, int, char)>
        {
            ("Invalid", 100, 100, 'X')
        };

        // Act
        var act = () => _renderer.OverlayEntities(art, entities);

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void OverlayEntities_NoEntities_ReturnsOriginalArt()
    {
        // Arrange
        var art = new[] { "ABC", "DEF" };
        var entities = new List<(string, int, int, char)>();

        // Act
        var result = _renderer.OverlayEntities(art, entities);

        // Assert
        result.Should().BeEquivalentTo(art);
    }

    #endregion

    #region RenderRoomWithEntities Tests

    [Test]
    public void RenderRoomWithEntities_OverlaysEntitiesOnArt()
    {
        // Arrange
        var artDef = new RoomArtDefinition(
            "test-room",
            new[] { ".....", ".....", "....." },
            new Dictionary<char, string>());
        
        _mockArtProvider.Setup(p => p.GetArtForRoom("test-room"))
            .Returns(artDef);
        
        var entities = new List<(string Name, int X, int Y, char Symbol)>
        {
            ("Player", 2, 1, '@')
        };

        // Act
        var result = _renderer.RenderRoomWithEntities("test-room", entities);

        // Assert
        result[1].Should().Contain("@");
    }

    #endregion
}
