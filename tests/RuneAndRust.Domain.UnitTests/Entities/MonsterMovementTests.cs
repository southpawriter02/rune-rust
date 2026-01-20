using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for Monster movement properties (v0.5.0b).
/// </summary>
[TestFixture]
public class MonsterMovementTests
{
    private static Monster CreateMonster() =>
        new Monster("TestMonster", "A test monster", 50, Stats.Default);

    // ===== Default Values Tests =====

    [Test]
    public void Monster_DefaultMovementSpeed_Is3()
    {
        // Arrange & Act
        var monster = CreateMonster();

        // Assert
        monster.MovementSpeed.Should().Be(3);
    }

    [Test]
    public void Monster_CombatGridPosition_InitiallyNull()
    {
        // Arrange & Act
        var monster = CreateMonster();

        // Assert
        monster.CombatGridPosition.Should().BeNull();
    }

    // ===== SetMovementSpeed Tests =====

    [Test]
    public void SetMovementSpeed_ValidSpeed_UpdatesSpeed()
    {
        // Arrange
        var monster = CreateMonster();

        // Act
        monster.SetMovementSpeed(5);

        // Assert
        monster.MovementSpeed.Should().Be(5);
    }

    // ===== ResetMovementPoints Tests =====

    [Test]
    public void ResetMovementPoints_SetsToSpeedTimes2()
    {
        // Arrange
        var monster = CreateMonster();
        // Default speed 3 × 2 = 6

        // Act
        monster.ResetMovementPoints();

        // Assert
        monster.MovementPointsRemaining.Should().Be(6);
    }

    // ===== UseMovementPoints Tests =====

    [Test]
    public void UseMovementPoints_SufficientPoints_ReturnsTrue()
    {
        // Arrange
        var monster = CreateMonster();
        monster.ResetMovementPoints(); // 6 points

        // Act
        var result = monster.UseMovementPoints(3);

        // Assert
        result.Should().BeTrue();
        monster.MovementPointsRemaining.Should().Be(3);
    }

    [Test]
    public void UseMovementPoints_InsufficientPoints_ReturnsFalse()
    {
        // Arrange
        var monster = CreateMonster();
        monster.ResetMovementPoints(); // 6 points

        // Act
        var result = monster.UseMovementPoints(10);

        // Assert
        result.Should().BeFalse();
        monster.MovementPointsRemaining.Should().Be(6); // Unchanged
    }

    // ===== SetCombatGridPosition Tests =====

    [Test]
    public void SetCombatGridPosition_UpdatesPosition()
    {
        // Arrange
        var monster = CreateMonster();
        var position = new GridPosition(5, 2);

        // Act
        monster.SetCombatGridPosition(position);

        // Assert
        monster.CombatGridPosition.Should().Be(position);
    }
}
