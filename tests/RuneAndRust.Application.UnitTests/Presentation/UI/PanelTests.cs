using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.UI;

namespace RuneAndRust.Application.UnitTests.Presentation.UI;

/// <summary>
/// Unit tests for <see cref="Panel"/>.
/// </summary>
[TestFixture]
public class PanelTests
{
    #region ContentArea Tests

    [Test]
    public void ContentArea_WithBorder_ReducesDimensionsByTwo()
    {
        // Arrange
        var panel = new Panel(
            PanelPosition.MainContent,
            X: 0,
            Y: 0,
            Width: 80,
            Height: 24,
            Title: null,
            HasBorder: true);

        // Act
        var content = panel.ContentArea;

        // Assert
        content.X.Should().Be(1);
        content.Y.Should().Be(1);
        content.Width.Should().Be(78); // 80 - 2
        content.Height.Should().Be(22); // 24 - 2
    }

    [Test]
    public void ContentArea_WithoutBorder_ReturnsSameDimensions()
    {
        // Arrange
        var panel = new Panel(
            PanelPosition.Input,
            X: 5,
            Y: 10,
            Width: 60,
            Height: 20,
            Title: null,
            HasBorder: false);

        // Act
        var content = panel.ContentArea;

        // Assert
        content.X.Should().Be(5);
        content.Y.Should().Be(10);
        content.Width.Should().Be(60);
        content.Height.Should().Be(20);
    }

    [Test]
    public void ContentArea_WithSmallPanel_DoesNotGoNegative()
    {
        // Arrange
        var panel = new Panel(
            PanelPosition.Popup,
            X: 0,
            Y: 0,
            Width: 1,
            Height: 1,
            Title: null,
            HasBorder: true);

        // Act
        var content = panel.ContentArea;

        // Assert - should be clamped to 0
        content.Width.Should().Be(0);
        content.Height.Should().Be(0);
    }

    #endregion

    #region ContainsPoint Tests

    [Test]
    public void ContainsPoint_WithPointInside_ReturnsTrue()
    {
        // Arrange
        var panel = new Panel(
            PanelPosition.MainContent,
            X: 10,
            Y: 5,
            Width: 40,
            Height: 20,
            Title: null,
            HasBorder: true);

        // Act - test point inside content area (accounting for border)
        var inside = panel.ContainsPoint(15, 10);

        // Assert
        inside.Should().BeTrue();
    }

    [Test]
    public void ContainsPoint_WithPointOnBorder_ReturnsFalse()
    {
        // Arrange
        var panel = new Panel(
            PanelPosition.MainContent,
            X: 10,
            Y: 5,
            Width: 40,
            Height: 20,
            Title: null,
            HasBorder: true);

        // Act - test point on border (X=10 is border)
        var onBorder = panel.ContainsPoint(10, 10);

        // Assert
        onBorder.Should().BeFalse();
    }

    [Test]
    public void ContainsPoint_WithPointOutside_ReturnsFalse()
    {
        // Arrange
        var panel = new Panel(
            PanelPosition.MainContent,
            X: 10,
            Y: 5,
            Width: 40,
            Height: 20,
            Title: null,
            HasBorder: true);

        // Act
        var outside = panel.ContainsPoint(100, 100);

        // Assert
        outside.Should().BeFalse();
    }

    #endregion

    #region Property Tests

    [Test]
    public void ContentLines_ReturnsContentHeight()
    {
        // Arrange
        var panel = new Panel(
            PanelPosition.MainContent,
            X: 0,
            Y: 0,
            Width: 80,
            Height: 24,
            Title: null,
            HasBorder: true);

        // Act & Assert
        panel.ContentLines.Should().Be(22); // 24 - 2 for borders
    }

    [Test]
    public void ContentWidth_ReturnsContentWidth()
    {
        // Arrange
        var panel = new Panel(
            PanelPosition.MainContent,
            X: 0,
            Y: 0,
            Width: 80,
            Height: 24,
            Title: null,
            HasBorder: true);

        // Act & Assert
        panel.ContentWidth.Should().Be(78); // 80 - 2 for borders
    }

    [Test]
    public void Empty_CreatesEmptyPanel()
    {
        // Act
        var panel = Panel.Empty(PanelPosition.Popup);

        // Assert
        panel.Position.Should().Be(PanelPosition.Popup);
        panel.Width.Should().Be(0);
        panel.Height.Should().Be(0);
        panel.HasBorder.Should().BeFalse();
    }

    #endregion
}
