using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.UI;

namespace RuneAndRust.Application.UnitTests.Presentation.UI;

/// <summary>
/// Unit tests for <see cref="StatusBar"/>.
/// </summary>
[TestFixture]
public class StatusBarTests
{
    private Mock<IStatusBarDataProvider> _mockDataProvider = null!;
    private Mock<ITerminalService> _mockTerminal = null!;
    private Mock<ILogger<ScreenLayout>> _mockLayoutLogger = null!;
    private Mock<ILogger<StatusBar>> _mockStatusBarLogger = null!;
    private ScreenLayout _layout = null!;

    [SetUp]
    public void Setup()
    {
        _mockDataProvider = new Mock<IStatusBarDataProvider>();
        _mockTerminal = new Mock<ITerminalService>();
        _mockLayoutLogger = new Mock<ILogger<ScreenLayout>>();
        _mockStatusBarLogger = new Mock<ILogger<StatusBar>>();
        
        // Default terminal size - 100x30 for expanded format
        _mockTerminal.Setup(t => t.GetSize()).Returns((100, 30));
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
        
        // Default data provider values
        _mockDataProvider.Setup(d => d.Health).Returns((80, 100));
        _mockDataProvider.Setup(d => d.PrimaryResource).Returns((60, 100, "Mana"));
        _mockDataProvider.Setup(d => d.Experience).Returns((450, 1000));
        _mockDataProvider.Setup(d => d.Gold).Returns(150);
        _mockDataProvider.Setup(d => d.Location).Returns("Dark Cave");
        _mockDataProvider.Setup(d => d.CombatInfo).Returns((ValueTuple<int, bool>?)null);
        
        _layout = new ScreenLayout(_mockTerminal.Object, _mockLayoutLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _layout.Dispose();
    }

    #region Format Content Tests

    [Test]
    public void FormatContent_WithNarrowWidth_ReturnsCompactFormat()
    {
        // Arrange
        using var statusBar = new StatusBar(_mockDataProvider.Object, _layout, _mockStatusBarLogger.Object);

        // Act - 80 chars is below threshold
        var content = statusBar.FormatContent(80);

        // Assert - compact format uses HP: and MP: abbreviations
        content.Should().Contain("HP:");
        content.Should().Contain("MP:"); // Mana abbreviated
        content.Should().NotContain("Health:");
    }

    [Test]
    public void FormatContent_WithWideWidth_ReturnsExpandedFormat()
    {
        // Arrange
        using var statusBar = new StatusBar(_mockDataProvider.Object, _layout, _mockStatusBarLogger.Object);

        // Act - 100+ chars uses expanded format
        var content = statusBar.FormatContent(120);

        // Assert - expanded format uses full labels
        content.Should().Contain("Health:");
        content.Should().Contain("Mana:");
        content.Should().Contain("Location:");
    }

    [Test]
    public void FormatContent_InCombat_ReturnsCombatFormat()
    {
        // Arrange
        _mockDataProvider.Setup(d => d.CombatInfo).Returns((3, true));
        using var statusBar = new StatusBar(_mockDataProvider.Object, _layout, _mockStatusBarLogger.Object);

        // Act
        var content = statusBar.FormatContent(100);

        // Assert - combat format includes round and turn
        content.Should().Contain("Round 3");
        content.Should().Contain("YOUR TURN");
        content.Should().NotContain("XP:"); // XP hidden in combat
        content.Should().NotContain("Gold:"); // Gold hidden in combat
    }

    [Test]
    public void FormatContent_EnemyTurn_ShowsEnemyTurnIndicator()
    {
        // Arrange
        _mockDataProvider.Setup(d => d.CombatInfo).Returns((2, false));
        using var statusBar = new StatusBar(_mockDataProvider.Object, _layout, _mockStatusBarLogger.Object);

        // Act
        var content = statusBar.FormatContent(100);

        // Assert
        content.Should().Contain("ENEMY TURN");
        content.Should().NotContain("YOUR TURN");
    }

    [Test]
    public void FormatContent_WithLowHp_IncludesWarningSymbol()
    {
        // Arrange - HP at 25% or below triggers warning
        _mockDataProvider.Setup(d => d.Health).Returns((25, 100));
        using var statusBar = new StatusBar(_mockDataProvider.Object, _layout, _mockStatusBarLogger.Object);

        // Act
        var content = statusBar.FormatContent(80);

        // Assert
        content.Should().Contain("⚠");
    }

    [Test]
    public void FormatContent_WithNormalHp_NoWarningSymbol()
    {
        // Arrange - HP above 25%
        _mockDataProvider.Setup(d => d.Health).Returns((50, 100));
        using var statusBar = new StatusBar(_mockDataProvider.Object, _layout, _mockStatusBarLogger.Object);

        // Act
        var content = statusBar.FormatContent(80);

        // Assert
        content.Should().NotContain("⚠");
    }

    [Test]
    public void FormatContent_WithNoResource_OmitsResourceSection()
    {
        // Arrange
        _mockDataProvider.Setup(d => d.PrimaryResource).Returns((ValueTuple<int, int, string>?)null);
        using var statusBar = new StatusBar(_mockDataProvider.Object, _layout, _mockStatusBarLogger.Object);

        // Act
        var content = statusBar.FormatContent(80);

        // Assert
        content.Should().NotContain("MP:");
        content.Should().NotContain("Mana:");
    }

    #endregion

    #region Render and Refresh Tests

    [Test]
    public void Render_WritesToFooterPanel()
    {
        // Arrange
        using var statusBar = new StatusBar(_mockDataProvider.Object, _layout, _mockStatusBarLogger.Object);

        // Act
        statusBar.Render();

        // Assert - should have written to terminal
        _mockTerminal.Verify(t => t.SetCursorPosition(It.IsAny<int>(), It.IsAny<int>()), Times.AtLeastOnce());
        _mockTerminal.Verify(t => t.Write(It.IsAny<string>()), Times.AtLeastOnce());
    }

    [Test]
    public void Refresh_CallsRender()
    {
        // Arrange
        using var statusBar = new StatusBar(_mockDataProvider.Object, _layout, _mockStatusBarLogger.Object);

        // Act
        statusBar.Refresh();

        // Assert - terminal should have been written to
        _mockTerminal.Verify(t => t.Write(It.IsAny<string>()), Times.AtLeastOnce());
    }

    [Test]
    public void Constructor_SubscribesToOnDataChanged()
    {
        // Arrange
        Action? capturedHandler = null;
        _mockDataProvider.SetupAdd(d => d.OnDataChanged += It.IsAny<Action?>())
            .Callback<Action?>(h => capturedHandler = h);

        // Act
        using var statusBar = new StatusBar(_mockDataProvider.Object, _layout, _mockStatusBarLogger.Object);

        // Assert
        capturedHandler.Should().NotBeNull();
    }

    [Test]
    public void Dispose_UnsubscribesFromOnDataChanged()
    {
        // Arrange
        Action? removedHandler = null;
        _mockDataProvider.SetupRemove(d => d.OnDataChanged -= It.IsAny<Action?>())
            .Callback<Action?>(h => removedHandler = h);

        var statusBar = new StatusBar(_mockDataProvider.Object, _layout, _mockStatusBarLogger.Object);

        // Act
        statusBar.Dispose();

        // Assert
        removedHandler.Should().NotBeNull();
    }

    #endregion

    #region Helper Method Tests

    [Test]
    [TestCase("Mana", "MP")]
    [TestCase("Rage", "RG")]
    [TestCase("Energy", "EN")]
    [TestCase("Stamina", "ST")]
    [TestCase("Focus", "FO")]
    [TestCase("CustomResource", "CU")]
    public void GetResourceAbbrev_ReturnsCorrectAbbreviation(string resourceName, string expected)
    {
        // Act
        var abbrev = StatusBar.GetResourceAbbrev(resourceName);

        // Assert
        abbrev.Should().Be(expected);
    }

    [Test]
    [TestCase("Dark Cave", 20, "Dark Cave")]
    [TestCase("Very Long Location Name", 10, "Very Lo...")]
    [TestCase("Test", 2, "Te")]
    [TestCase("Any", 0, "")]
    public void TruncateLocation_TruncatesCorrectly(string location, int maxLength, string expected)
    {
        // Act
        var result = StatusBar.TruncateLocation(location, maxLength);

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void IsLowHp_AtThreshold_ReturnsTrue()
    {
        // Arrange - exactly 25%
        _mockDataProvider.Setup(d => d.Health).Returns((25, 100));
        using var statusBar = new StatusBar(_mockDataProvider.Object, _layout, _mockStatusBarLogger.Object);

        // Act & Assert
        statusBar.IsLowHp().Should().BeTrue();
    }

    [Test]
    public void IsLowHp_AboveThreshold_ReturnsFalse()
    {
        // Arrange - 26%
        _mockDataProvider.Setup(d => d.Health).Returns((26, 100));
        using var statusBar = new StatusBar(_mockDataProvider.Object, _layout, _mockStatusBarLogger.Object);

        // Act & Assert
        statusBar.IsLowHp().Should().BeFalse();
    }

    #endregion
}
