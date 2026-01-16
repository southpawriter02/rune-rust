namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

using Avalonia;
using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.ViewModels;

[TestFixture]
public class MapViewPanelViewModelTests
{
    // ============================================================================
    // RoomNodeViewModel Tests
    // ============================================================================

    [Test]
    public void RoomNodeViewModel_ExploredRoom_ShowsName()
    {
        // Arrange & Act
        var vm = new RoomNodeViewModel(Guid.NewGuid(), "Test Room", "Standard", 0, 0, true, false);

        // Assert
        vm.DisplayName.Should().Be("Test Room");
        vm.IsExplored.Should().BeTrue();
    }

    [Test]
    public void RoomNodeViewModel_UnexploredRoom_ShowsQuestionMarks()
    {
        // Arrange & Act
        var vm = new RoomNodeViewModel(Guid.NewGuid(), "Secret Room", "Standard", 0, 0, false, false);

        // Assert
        vm.DisplayName.Should().Be("???");
        vm.IsExplored.Should().BeFalse();
    }

    [Test]
    public void RoomNodeViewModel_ShopRoom_HasShopIcon()
    {
        // Arrange & Act
        var vm = new RoomNodeViewModel(Guid.NewGuid(), "Merchant", "Shop", 0, 0, true, false);

        // Assert
        vm.Icon.Should().Be("💰");
    }

    // ============================================================================
    // ConnectionViewModel Tests
    // ============================================================================

    [Test]
    public void ConnectionViewModel_StoresPoints()
    {
        // Arrange & Act
        var vm = new ConnectionViewModel(new Point(10, 20), new Point(100, 200));

        // Assert
        vm.StartPoint.Should().Be(new Point(10, 20));
        vm.EndPoint.Should().Be(new Point(100, 200));
        vm.IsLocked.Should().BeFalse();
    }

    [Test]
    public void ConnectionViewModel_LockedConnection_HasLockedFlag()
    {
        // Arrange & Act
        var vm = new ConnectionViewModel(new Point(0, 0), new Point(50, 50), isLocked: true);

        // Assert
        vm.IsLocked.Should().BeTrue();
    }

    // ============================================================================
    // MapViewPanelViewModel Tests
    // ============================================================================

    [Test]
    public void MapViewPanelViewModel_Constructor_LoadsSampleData()
    {
        // Arrange & Act
        var vm = new MapViewPanelViewModel();

        // Assert
        vm.Rooms.Should().NotBeEmpty();
        vm.Connections.Should().NotBeEmpty();
    }

    [Test]
    public void MapViewPanelViewModel_ZoomIn_IncreasesZoomLevel()
    {
        // Arrange
        var vm = new MapViewPanelViewModel();
        var initialZoom = vm.ZoomLevel;

        // Act
        vm.ZoomInCommand.Execute(null);

        // Assert
        vm.ZoomLevel.Should().BeGreaterThan(initialZoom);
    }

    [Test]
    public void MapViewPanelViewModel_ZoomOut_DecreasesZoomLevel()
    {
        // Arrange
        var vm = new MapViewPanelViewModel();
        var initialZoom = vm.ZoomLevel;

        // Act
        vm.ZoomOutCommand.Execute(null);

        // Assert
        vm.ZoomLevel.Should().BeLessThan(initialZoom);
    }

    [Test]
    public void MapViewPanelViewModel_ZoomIn_CapsAtMaximum()
    {
        // Arrange
        var vm = new MapViewPanelViewModel();

        // Act - zoom in many times
        for (int i = 0; i < 10; i++)
            vm.ZoomInCommand.Execute(null);

        // Assert
        vm.ZoomLevel.Should().BeLessOrEqualTo(2.0);
    }

    [Test]
    public void MapViewPanelViewModel_ZoomOut_CapsAtMinimum()
    {
        // Arrange
        var vm = new MapViewPanelViewModel();

        // Act - zoom out many times
        for (int i = 0; i < 10; i++)
            vm.ZoomOutCommand.Execute(null);

        // Assert
        vm.ZoomLevel.Should().BeGreaterOrEqualTo(0.5);
    }

    [Test]
    public void MapViewPanelViewModel_ToggleVisibility_TogglesIsVisible()
    {
        // Arrange
        var vm = new MapViewPanelViewModel();
        vm.IsVisible.Should().BeFalse();

        // Act
        vm.ToggleVisibility();

        // Assert
        vm.IsVisible.Should().BeTrue();

        // Act again
        vm.ToggleVisibility();

        // Assert
        vm.IsVisible.Should().BeFalse();
    }

    [Test]
    public void MapViewPanelViewModel_ResetZoom_RestoresDefaults()
    {
        // Arrange
        var vm = new MapViewPanelViewModel();
        vm.ZoomInCommand.Execute(null);
        vm.PanOffset = new Point(100, 100);

        // Act
        vm.ResetZoomCommand.Execute(null);

        // Assert
        vm.ZoomLevel.Should().Be(1.0);
        vm.PanOffset.Should().Be(new Point(0, 0));
    }
}
