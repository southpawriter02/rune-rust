using Avalonia.Controls;
using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.Services;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.Services;

/// <summary>
/// Unit tests for <see cref="WindowLayoutService"/>.
/// </summary>
[TestFixture]
public class WindowLayoutServiceTests
{
    private WindowLayoutService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new WindowLayoutService();
    }

    /// <summary>
    /// Verifies that RegisterPanel adds a panel and GetPanel returns it.
    /// </summary>
    [Test]
    public void RegisterPanel_AddsPanel_GetPanelReturnsIt()
    {
        // Arrange
        var panel = new Border { Name = "TestPanel" };
        var region = PanelRegion.MainContent;

        // Act
        _service.RegisterPanel(region, panel);
        var result = _service.GetPanel(region);

        // Assert
        result.Should().BeSameAs(panel);
    }

    /// <summary>
    /// Verifies that UnregisterPanel removes a panel and GetPanel returns null.
    /// </summary>
    [Test]
    public void UnregisterPanel_RemovesPanel_GetPanelReturnsNull()
    {
        // Arrange
        var panel = new Border { Name = "TestPanel" };
        var region = PanelRegion.MainContent;
        _service.RegisterPanel(region, panel);

        // Act
        _service.UnregisterPanel(region);
        var result = _service.GetPanel(region);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetPanel returns null for an unregistered region.
    /// </summary>
    [Test]
    public void GetPanel_WhenNotRegistered_ReturnsNull()
    {
        // Act
        var result = _service.GetPanel(PanelRegion.CombatOverlay);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that OnLayoutChanged is fired when a panel is registered.
    /// </summary>
    [Test]
    public void RegisterPanel_RaisesOnLayoutChanged()
    {
        // Arrange
        var panel = new Border();
        PanelRegion? changedRegion = null;
        _service.OnLayoutChanged += region => changedRegion = region;

        // Act
        _service.RegisterPanel(PanelRegion.BottomContent, panel);

        // Assert
        changedRegion.Should().Be(PanelRegion.BottomContent);
    }

    /// <summary>
    /// Verifies that RegisterPanel throws when panel is null.
    /// </summary>
    [Test]
    public void RegisterPanel_WhenPanelIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _service.RegisterPanel(PanelRegion.Input, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("panel");
    }
}
