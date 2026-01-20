using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Presentation.UI;

namespace RuneAndRust.Application.UnitTests.Presentation.UI;

/// <summary>
/// Unit tests for <see cref="ScreenLayout"/>.
/// </summary>
[TestFixture]
public class ScreenLayoutTests
{
    private Mock<ITerminalService> _mockTerminal = null!;
    private Mock<ILogger<ScreenLayout>> _mockLogger = null!;

    [SetUp]
    public void Setup()
    {
        _mockTerminal = new Mock<ITerminalService>();
        _mockLogger = new Mock<ILogger<ScreenLayout>>();
        
        // Default terminal size
        _mockTerminal.Setup(t => t.GetSize()).Returns((100, 30));
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
    }

    #region RecalculateLayout Tests

    [Test]
    public void RecalculateLayout_CreatesAllStandardPanels()
    {
        // Arrange & Act
        using var layout = new ScreenLayout(_mockTerminal.Object, _mockLogger.Object);

        // Assert
        layout.GetPanel(PanelPosition.MainContent).Should().NotBeNull();
        layout.GetPanel(PanelPosition.Sidebar).Should().NotBeNull();
        layout.GetPanel(PanelPosition.Footer).Should().NotBeNull();
        layout.GetPanel(PanelPosition.Input).Should().NotBeNull();
    }

    [Test]
    public void RecalculateLayout_WithMinimumSize_SetsIsBelowMinimumSizeFalse()
    {
        // Arrange
        _mockTerminal.Setup(t => t.GetSize()).Returns((80, 24));

        // Act
        using var layout = new ScreenLayout(_mockTerminal.Object, _mockLogger.Object);

        // Assert
        layout.IsBelowMinimumSize.Should().BeFalse();
    }

    [Test]
    public void RecalculateLayout_BelowMinimumWidth_SetsIsBelowMinimumSizeTrue()
    {
        // Arrange
        _mockTerminal.Setup(t => t.GetSize()).Returns((60, 24));

        // Act
        using var layout = new ScreenLayout(_mockTerminal.Object, _mockLogger.Object);

        // Assert
        layout.IsBelowMinimumSize.Should().BeTrue();
    }

    [Test]
    public void RecalculateLayout_BelowMinimumHeight_SetsIsBelowMinimumSizeTrue()
    {
        // Arrange
        _mockTerminal.Setup(t => t.GetSize()).Returns((80, 20));

        // Act
        using var layout = new ScreenLayout(_mockTerminal.Object, _mockLogger.Object);

        // Assert
        layout.IsBelowMinimumSize.Should().BeTrue();
    }

    [Test]
    public void RecalculateLayout_MainContentHasCorrectWidth()
    {
        // Arrange
        _mockTerminal.Setup(t => t.GetSize()).Returns((100, 30));

        // Act
        using var layout = new ScreenLayout(_mockTerminal.Object, _mockLogger.Object);
        var mainPanel = layout.GetPanel(PanelPosition.MainContent);

        // Assert - sidebar is 25% of 100 = 25, so main = 100 - 25 - 1 = 74
        mainPanel.Should().NotBeNull();
        mainPanel!.Value.Width.Should().Be(74);
    }

    [Test]
    public void RecalculateLayout_SidebarMaxWidth30()
    {
        // Arrange - large terminal 200 wide
        _mockTerminal.Setup(t => t.GetSize()).Returns((200, 50));

        // Act
        using var layout = new ScreenLayout(_mockTerminal.Object, _mockLogger.Object);
        var sidebar = layout.GetPanel(PanelPosition.Sidebar);

        // Assert - sidebar capped at 30 + 1 = 31
        sidebar.Should().NotBeNull();
        sidebar!.Value.Width.Should().BeLessOrEqualTo(31);
    }

    #endregion

    #region GetPanel Tests

    [Test]
    public void GetPanel_WithValidPosition_ReturnsPanel()
    {
        // Arrange
        using var layout = new ScreenLayout(_mockTerminal.Object, _mockLogger.Object);

        // Act
        var panel = layout.GetPanel(PanelPosition.MainContent);

        // Assert
        panel.Should().NotBeNull();
        panel!.Value.Position.Should().Be(PanelPosition.MainContent);
    }

    [Test]
    public void GetPanel_WithPopupPosition_ReturnsNull()
    {
        // Arrange
        using var layout = new ScreenLayout(_mockTerminal.Object, _mockLogger.Object);

        // Act - Popup is not in standard layout
        var panel = layout.GetPanel(PanelPosition.Popup);

        // Assert
        panel.Should().BeNull();
    }

    #endregion

    #region RenderToPanel Tests

    [Test]
    public void RenderToPanel_WritesLinesToTerminal()
    {
        // Arrange
        using var layout = new ScreenLayout(_mockTerminal.Object, _mockLogger.Object);
        var lines = new[] { "Line 1", "Line 2", "Line 3" };

        // Act
        layout.RenderToPanel(PanelPosition.MainContent, lines);

        // Assert
        _mockTerminal.Verify(t => t.SetCursorPosition(It.IsAny<int>(), It.IsAny<int>()), Times.AtLeast(3));
        _mockTerminal.Verify(t => t.Write(It.IsAny<string>()), Times.AtLeast(3));
    }

    [Test]
    public void RenderToPanel_InvalidPosition_DoesNotThrow()
    {
        // Arrange
        using var layout = new ScreenLayout(_mockTerminal.Object, _mockLogger.Object);
        var lines = new[] { "Test" };

        // Act & Assert
        var act = () => layout.RenderToPanel(PanelPosition.Popup, lines);
        act.Should().NotThrow();
    }

    #endregion

    #region DrawBorders Tests

    [Test]
    public void DrawBorders_WritesToTerminal()
    {
        // Arrange
        using var layout = new ScreenLayout(_mockTerminal.Object, _mockLogger.Object);

        // Act
        layout.DrawBorders();

        // Assert - borders write to terminal
        _mockTerminal.Verify(t => t.Write(It.IsAny<string>()), Times.AtLeast(4));
    }

    [Test]
    public void DrawBorders_UsesUnicodeWhenSupported()
    {
        // Arrange
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(true);
        using var layout = new ScreenLayout(_mockTerminal.Object, _mockLogger.Object);

        // Act
        layout.DrawBorders();

        // Assert - should use Unicode box characters
        _mockTerminal.Verify(t => t.Write(It.Is<string>(s => s.Contains('┌') || s.Contains('─'))), Times.AtLeastOnce());
    }

    [Test]
    public void DrawBorders_UsesAsciiWhenUnicodeNotSupported()
    {
        // Arrange
        _mockTerminal.Setup(t => t.SupportsUnicode).Returns(false);
        using var layout = new ScreenLayout(_mockTerminal.Object, _mockLogger.Object);

        // Act
        layout.DrawBorders();

        // Assert - should use ASCII fallback
        _mockTerminal.Verify(t => t.Write(It.Is<string>(s => s.Contains('+') || s.Contains('-'))), Times.AtLeastOnce());
    }

    #endregion

    #region Event Tests

    [Test]
    public void OnLayoutChanged_FiresAfterRecalculate()
    {
        // Arrange
        using var layout = new ScreenLayout(_mockTerminal.Object, _mockLogger.Object);
        var eventFired = false;
        layout.OnLayoutChanged += () => eventFired = true;

        // Act
        layout.RecalculateLayout();

        // Assert
        eventFired.Should().BeTrue();
    }

    [Test]
    public void OnResize_TriggersRecalculate()
    {
        // Arrange
        using var layout = new ScreenLayout(_mockTerminal.Object, _mockLogger.Object);
        
        // Act - simulate resize
        _mockTerminal.Raise(t => t.OnResize += null!, (120, 40));

        // Assert - should have cleared and redrawn
        _mockTerminal.Verify(t => t.Clear(), Times.AtLeastOnce());
    }

    #endregion

    #region Dispose Tests

    [Test]
    public void Dispose_UnsubscribesFromResizeEvent()
    {
        // Arrange
        var layout = new ScreenLayout(_mockTerminal.Object, _mockLogger.Object);

        // Act
        layout.Dispose();
        
        // Raise resize event after dispose - should not throw
        var act = () => _mockTerminal.Raise(t => t.OnResize += null!, (50, 50));

        // Assert - event handling should not throw after dispose
        act.Should().NotThrow();
    }

    #endregion
}
