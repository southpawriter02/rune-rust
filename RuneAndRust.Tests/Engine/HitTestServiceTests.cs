using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for the HitTestService (v0.3.23c).
/// Tests screen coordinate to game element mapping.
/// </summary>
public class HitTestServiceTests
{
    private readonly ILogger<HitTestService> _logger;
    private readonly HitTestService _service;

    public HitTestServiceTests()
    {
        _logger = Substitute.For<ILogger<HitTestService>>();
        _service = new HitTestService(_logger);
    }

    #region HitTest Tests

    [Fact]
    public void HitTest_InsideRegion_ReturnsHit()
    {
        // Arrange - Register a region from (10,5) to (20,15)
        _service.RegisterRegion(
            HitTargetType.CombatGridCell,
            left: 10, top: 5, right: 20, bottom: 15,
            row: 2, column: 3);

        // Act - Click inside the region
        var result = _service.HitTest(15, 10, GamePhase.Combat);

        // Assert
        result.IsHit.Should().BeTrue();
        result.TargetType.Should().Be(HitTargetType.CombatGridCell);
        result.Row.Should().Be(2);
        result.Column.Should().Be(3);
    }

    [Fact]
    public void HitTest_OutsideRegion_ReturnsNone()
    {
        // Arrange
        _service.RegisterRegion(
            HitTargetType.CombatGridCell,
            left: 10, top: 5, right: 20, bottom: 15);

        // Act - Click outside all regions
        var result = _service.HitTest(5, 3, GamePhase.Combat);

        // Assert
        result.IsHit.Should().BeFalse();
        result.TargetType.Should().Be(HitTargetType.None);
        result.Should().Be(HitTestResult.None);
    }

    [Fact]
    public void HitTest_OnEdge_ReturnsHit()
    {
        // Arrange - Register a region from (10,5) to (20,15)
        _service.RegisterRegion(
            HitTargetType.MenuOption,
            left: 10, top: 5, right: 20, bottom: 15,
            index: 1);

        // Act - Click on the exact edge
        var result = _service.HitTest(10, 5, GamePhase.MainMenu);

        // Assert
        result.IsHit.Should().BeTrue();
        result.Index.Should().Be(1);
    }

    [Fact]
    public void HitTest_OverlappingRegions_ReturnsTopmost()
    {
        // Arrange - Register two overlapping regions
        _service.RegisterRegion(
            HitTargetType.CombatGridCell,
            left: 10, top: 5, right: 30, bottom: 25,
            index: 0);

        _service.RegisterRegion(
            HitTargetType.AbilityButton,
            left: 15, top: 10, right: 25, bottom: 20,
            index: 1);

        // Act - Click in the overlap area
        var result = _service.HitTest(20, 15, GamePhase.Combat);

        // Assert - Should return the later-registered (topmost) region
        result.IsHit.Should().BeTrue();
        result.TargetType.Should().Be(HitTargetType.AbilityButton);
        result.Index.Should().Be(1);
    }

    [Fact]
    public void HitTest_WithData_ReturnsData()
    {
        // Arrange - Register with custom data
        var customData = new { Name = "Test", Value = 42 };
        _service.RegisterRegion(
            HitTargetType.TurnOrderEntry,
            left: 1, top: 1, right: 10, bottom: 3,
            index: 0,
            data: customData);

        // Act
        var result = _service.HitTest(5, 2, GamePhase.Combat);

        // Assert
        result.Data.Should().Be(customData);
    }

    [Fact]
    public void HitTest_NoRegionsRegistered_ReturnsNone()
    {
        // Act - No regions registered
        var result = _service.HitTest(10, 10, GamePhase.Exploration);

        // Assert
        result.Should().Be(HitTestResult.None);
    }

    #endregion

    #region ClearRegions Tests

    [Fact]
    public void ClearRegions_RemovesAll()
    {
        // Arrange - Register some regions
        _service.RegisterRegion(HitTargetType.MenuOption, 1, 1, 10, 3, index: 0);
        _service.RegisterRegion(HitTargetType.MenuOption, 1, 5, 10, 7, index: 1);

        // Act
        _service.ClearRegions();

        // Assert - All hits should now return None
        var result1 = _service.HitTest(5, 2, GamePhase.MainMenu);
        var result2 = _service.HitTest(5, 6, GamePhase.MainMenu);

        result1.Should().Be(HitTestResult.None);
        result2.Should().Be(HitTestResult.None);
    }

    [Fact]
    public void ClearRegions_IsIdempotent()
    {
        // Arrange - Clear when already empty
        _service.ClearRegions();

        // Act - Clear again
        _service.ClearRegions();

        // Assert - Should not throw
        var result = _service.HitTest(5, 5, GamePhase.MainMenu);
        result.Should().Be(HitTestResult.None);
    }

    #endregion

    #region RegisterRegion Tests

    [Fact]
    public void RegisterRegion_AllParameters_StoresCorrectly()
    {
        // Arrange & Act
        _service.RegisterRegion(
            HitTargetType.MinimapCell,
            left: 50, top: 1, right: 70, bottom: 20,
            index: 5, row: 10, column: 15,
            data: "minimap-data");

        var result = _service.HitTest(60, 10, GamePhase.Exploration);

        // Assert
        result.TargetType.Should().Be(HitTargetType.MinimapCell);
        result.Index.Should().Be(5);
        result.Row.Should().Be(10);
        result.Column.Should().Be(15);
        result.Data.Should().Be("minimap-data");
    }

    [Fact]
    public void RegisterRegion_MultipleTypes_AllAccessible()
    {
        // Arrange
        _service.RegisterRegion(HitTargetType.CombatGridCell, 1, 1, 40, 20);
        _service.RegisterRegion(HitTargetType.TurnOrderEntry, 45, 1, 60, 5);
        _service.RegisterRegion(HitTargetType.LogEntry, 45, 8, 60, 20);

        // Act
        var gridResult = _service.HitTest(20, 10, GamePhase.Combat);
        var turnResult = _service.HitTest(50, 3, GamePhase.Combat);
        var logResult = _service.HitTest(55, 15, GamePhase.Combat);

        // Assert
        gridResult.TargetType.Should().Be(HitTargetType.CombatGridCell);
        turnResult.TargetType.Should().Be(HitTargetType.TurnOrderEntry);
        logResult.TargetType.Should().Be(HitTargetType.LogEntry);
    }

    #endregion

    #region HitTestResult Tests

    [Fact]
    public void HitTestResult_None_HasCorrectProperties()
    {
        // Assert
        HitTestResult.None.TargetType.Should().Be(HitTargetType.None);
        HitTestResult.None.IsHit.Should().BeFalse();
        HitTestResult.None.Index.Should().BeNull();
        HitTestResult.None.Row.Should().BeNull();
        HitTestResult.None.Column.Should().BeNull();
        HitTestResult.None.Data.Should().BeNull();
    }

    [Fact]
    public void HitTestResult_IsHit_TrueForNonNoneTypes()
    {
        // Arrange
        var result = new HitTestResult(HitTargetType.AbilityButton);

        // Assert
        result.IsHit.Should().BeTrue();
    }

    #endregion
}
