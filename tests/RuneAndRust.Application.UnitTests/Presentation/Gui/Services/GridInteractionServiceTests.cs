namespace RuneAndRust.Application.UnitTests.Presentation.Gui.Services;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Presentation.Gui.Models;
using RuneAndRust.Presentation.Gui.Services;

/// <summary>
/// Unit tests for the GridInteractionService.
/// </summary>
[TestFixture]
public class GridInteractionServiceTests
{
    private GridInteractionService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new GridInteractionService();
    }

    [Test]
    public void EnterMovementMode_SetsCorrectState()
    {
        // Arrange
        var position = new GridPosition(4, 4);

        // Act
        _service.EnterMovementMode(Guid.NewGuid(), position, 4);

        // Assert
        _service.CurrentMode.Should().Be(GridInteractionMode.Movement);
        _service.IsTargeting.Should().BeTrue();
    }

    [Test]
    public void EnterMovementMode_CalculatesReachableCells()
    {
        // Arrange
        var position = new GridPosition(4, 4);

        // Act
        _service.EnterMovementMode(Guid.NewGuid(), position, 2);
        var highlights = _service.GetHighlightedCells();

        // Assert
        highlights.Should().NotBeEmpty();
        highlights.Should().AllSatisfy(h => h.Type.Should().Be(HighlightType.Movement));
    }

    [Test]
    public void EnterAttackMode_SetsCorrectState()
    {
        // Arrange
        var position = new GridPosition(4, 4);

        // Act
        _service.EnterAttackMode(Guid.NewGuid(), position, 1);

        // Assert
        _service.CurrentMode.Should().Be(GridInteractionMode.Attack);
        _service.IsTargeting.Should().BeTrue();
    }

    [Test]
    public void CancelTargeting_ClearsState()
    {
        // Arrange
        var position = new GridPosition(4, 4);
        _service.EnterMovementMode(Guid.NewGuid(), position, 4);

        // Act
        _service.CancelTargeting();

        // Assert
        _service.CurrentMode.Should().Be(GridInteractionMode.None);
        _service.IsTargeting.Should().BeFalse();
        _service.GetHighlightedCells().Should().BeEmpty();
    }

    [Test]
    public void HandleCellClick_OnValidTarget_RaisesCompletionEvent()
    {
        // Arrange
        var position = new GridPosition(4, 4);
        var targetPos = new GridPosition(5, 4);
        _service.EnterMovementMode(Guid.NewGuid(), position, 2);

        TargetingResult? result = null;
        _service.OnTargetingComplete += r => result = r;

        // Act
        _service.HandleCellClick(targetPos);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Mode.Should().Be(GridInteractionMode.Movement);
    }

    [Test]
    public void HandleCellClick_OnInvalidTarget_ReturnsFailure()
    {
        // Arrange
        var position = new GridPosition(4, 4);
        var invalidPos = new GridPosition(10, 10); // Outside movement range
        _service.EnterMovementMode(Guid.NewGuid(), position, 2);

        TargetingResult? result = null;
        _service.OnTargetingComplete += r => result = r;

        // Act
        _service.HandleCellClick(invalidPos);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }

    [Test]
    public void GetHighlightedCells_ReturnsCorrectType_ForMovement()
    {
        // Arrange
        var position = new GridPosition(4, 4);

        // Act
        _service.EnterMovementMode(Guid.NewGuid(), position, 2);
        var highlights = _service.GetHighlightedCells();

        // Assert
        highlights.Should().NotBeEmpty();
        highlights.Should().OnlyContain(h => h.Type == HighlightType.Movement);
    }

    [Test]
    public void HandleCellHover_UpdatesHoverInfo()
    {
        // Arrange
        var position = new GridPosition(3, 2);
        HoverInfo? hoverInfo = null;
        _service.OnHoverChanged += info => hoverInfo = info;

        // Act
        _service.HandleCellHover(position);

        // Assert
        hoverInfo.Should().NotBeNull();
        hoverInfo!.Position.Should().Be(position);
        hoverInfo.CellLabel.Should().Be("C4");
    }
}
