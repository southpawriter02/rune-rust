using FluentAssertions;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.DTOs;

/// <summary>
/// Unit tests for <see cref="CombatStartResult"/> (v0.5.0c).
/// </summary>
[TestFixture]
public class CombatStartResultTests
{
    [Test]
    public void Succeed_CreatesSuccessResult()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();
        var playerPos = new GridPosition(3, 6);
        var monsterPositions = new Dictionary<Guid, GridPosition>
        {
            { Guid.NewGuid(), new GridPosition(2, 1) }
        };

        // Act
        var result = CombatStartResult.Succeed(grid, playerPos, monsterPositions);

        // Assert
        result.Success.Should().BeTrue();
        result.Grid.Should().Be(grid);
        result.PlayerPosition.Should().Be(playerPos);
        result.MonsterPositions.Should().HaveCount(1);
        result.Message.Should().Be("Combat begins!");
    }

    [Test]
    public void Succeed_WithCustomMessage_UsesMessage()
    {
        // Arrange
        var grid = CombatGrid.CreateDefault();

        // Act
        var result = CombatStartResult.Succeed(
            grid,
            new GridPosition(0, 0),
            new Dictionary<Guid, GridPosition>(),
            "Battle commences!");

        // Assert
        result.Message.Should().Be("Battle commences!");
    }

    [Test]
    public void Fail_CreatesFailureResult()
    {
        // Act
        var result = CombatStartResult.Fail("Grid initialization failed");

        // Assert
        result.Success.Should().BeFalse();
        result.Grid.Should().BeNull();
        result.MonsterPositions.Should().BeEmpty();
        result.Message.Should().Be("Grid initialization failed");
    }
}
