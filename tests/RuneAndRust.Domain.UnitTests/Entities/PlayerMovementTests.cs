using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for Player movement properties (v0.5.0b).
/// </summary>
[TestFixture]
public class PlayerMovementTests
{
    private static Player CreatePlayer() => new("TestPlayer");

    // ===== Default Values Tests =====

    [Test]
    public void Player_DefaultMovementSpeed_Is4()
    {
        // Arrange & Act
        var player = CreatePlayer();

        // Assert
        player.BaseMovementSpeed.Should().Be(4);
        player.MovementSpeed.Should().Be(4);
    }

    [Test]
    public void Player_CombatGridPosition_InitiallyNull()
    {
        // Arrange & Act
        var player = CreatePlayer();

        // Assert
        player.CombatGridPosition.Should().BeNull();
    }

    [Test]
    public void Player_MovementPointsRemaining_InitiallyZero()
    {
        // Arrange & Act
        var player = CreatePlayer();

        // Assert
        player.MovementPointsRemaining.Should().Be(0);
    }

    // ===== SetMovementSpeed Tests =====

    [Test]
    public void SetMovementSpeed_ValidSpeed_UpdatesSpeed()
    {
        // Arrange
        var player = CreatePlayer();

        // Act
        player.SetMovementSpeed(6);

        // Assert
        player.BaseMovementSpeed.Should().Be(6);
        player.MovementSpeed.Should().Be(6);
    }

    [Test]
    public void SetMovementSpeed_ZeroOrNegative_ThrowsException()
    {
        // Arrange
        var player = CreatePlayer();

        // Act
        var act = () => player.SetMovementSpeed(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ===== SetCombatGridPosition Tests =====

    [Test]
    public void SetCombatGridPosition_UpdatesPosition()
    {
        // Arrange
        var player = CreatePlayer();
        var position = new GridPosition(3, 4);

        // Act
        player.SetCombatGridPosition(position);

        // Assert
        player.CombatGridPosition.Should().Be(position);
    }

    [Test]
    public void SetCombatGridPosition_Null_ClearsPosition()
    {
        // Arrange
        var player = CreatePlayer();
        player.SetCombatGridPosition(new GridPosition(3, 4));

        // Act
        player.SetCombatGridPosition(null);

        // Assert
        player.CombatGridPosition.Should().BeNull();
    }

    // ===== ResetMovementPoints Tests =====

    [Test]
    public void ResetMovementPoints_SetsToSpeedTimes2()
    {
        // Arrange
        var player = CreatePlayer();
        player.SetMovementSpeed(4); // 4 × 2 = 8

        // Act
        player.ResetMovementPoints();

        // Assert (Speed 4 × multiplier 2 = 8 points)
        player.MovementPointsRemaining.Should().Be(8);
    }

    // ===== UseMovementPoints Tests =====

    [Test]
    public void UseMovementPoints_SufficientPoints_ReturnsTrue()
    {
        // Arrange
        var player = CreatePlayer();
        player.ResetMovementPoints(); // 8 points

        // Act
        var result = player.UseMovementPoints(2);

        // Assert
        result.Should().BeTrue();
        player.MovementPointsRemaining.Should().Be(6);
    }

    [Test]
    public void UseMovementPoints_InsufficientPoints_ReturnsFalse()
    {
        // Arrange
        var player = CreatePlayer();
        player.ResetMovementPoints(); // 8 points

        // Act
        var result = player.UseMovementPoints(10);

        // Assert
        result.Should().BeFalse();
        player.MovementPointsRemaining.Should().Be(8); // Unchanged
    }

    // ===== GetDisplayMovementRemaining Tests =====

    [Test]
    public void GetDisplayMovementRemaining_ReturnsPointsDividedByMultiplier()
    {
        // Arrange
        var player = CreatePlayer();
        player.ResetMovementPoints(); // 8 points

        // Act
        var display = player.GetDisplayMovementRemaining();

        // Assert
        display.Should().Be(4f);
    }
}
