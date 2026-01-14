using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for FacingDirection enum and FacingDirectionExtensions.
/// </summary>
[TestFixture]
public class FacingDirectionTests
{
    // ===== GetOpposite Tests =====

    [Test]
    [TestCase(FacingDirection.North, FacingDirection.South)]
    [TestCase(FacingDirection.South, FacingDirection.North)]
    [TestCase(FacingDirection.East, FacingDirection.West)]
    [TestCase(FacingDirection.West, FacingDirection.East)]
    [TestCase(FacingDirection.NorthEast, FacingDirection.SouthWest)]
    [TestCase(FacingDirection.SouthWest, FacingDirection.NorthEast)]
    [TestCase(FacingDirection.NorthWest, FacingDirection.SouthEast)]
    [TestCase(FacingDirection.SouthEast, FacingDirection.NorthWest)]
    public void GetOpposite_ReturnsCorrectOppositeDirection(FacingDirection input, FacingDirection expected)
    {
        // Act
        var result = input.GetOpposite();

        // Assert
        result.Should().Be(expected);
    }

    // ===== GetDirectionTo Tests =====

    [Test]
    public void GetDirectionTo_TargetAbove_ReturnsNorth()
    {
        // Arrange
        var from = new GridPosition(5, 5);
        var to = new GridPosition(5, 3);

        // Act
        var result = FacingDirectionExtensions.GetDirectionTo(from, to);

        // Assert
        result.Should().Be(FacingDirection.North);
    }

    [Test]
    public void GetDirectionTo_TargetBelow_ReturnsSouth()
    {
        // Arrange
        var from = new GridPosition(5, 5);
        var to = new GridPosition(5, 7);

        // Act
        var result = FacingDirectionExtensions.GetDirectionTo(from, to);

        // Assert
        result.Should().Be(FacingDirection.South);
    }

    [Test]
    public void GetDirectionTo_TargetRight_ReturnsEast()
    {
        // Arrange
        var from = new GridPosition(5, 5);
        var to = new GridPosition(7, 5);

        // Act
        var result = FacingDirectionExtensions.GetDirectionTo(from, to);

        // Assert
        result.Should().Be(FacingDirection.East);
    }

    [Test]
    public void GetDirectionTo_TargetLeft_ReturnsWest()
    {
        // Arrange
        var from = new GridPosition(5, 5);
        var to = new GridPosition(3, 5);

        // Act
        var result = FacingDirectionExtensions.GetDirectionTo(from, to);

        // Assert
        result.Should().Be(FacingDirection.West);
    }

    [Test]
    public void GetDirectionTo_TargetUpRight_ReturnsNorthEast()
    {
        // Arrange
        var from = new GridPosition(5, 5);
        var to = new GridPosition(7, 3);

        // Act
        var result = FacingDirectionExtensions.GetDirectionTo(from, to);

        // Assert
        result.Should().Be(FacingDirection.NorthEast);
    }

    [Test]
    public void GetDirectionTo_SamePosition_ReturnsNorth()
    {
        // Arrange
        var from = new GridPosition(5, 5);
        var to = new GridPosition(5, 5);

        // Act
        var result = FacingDirectionExtensions.GetDirectionTo(from, to);

        // Assert
        result.Should().Be(FacingDirection.North);
    }

    // ===== IsOpposite Tests =====

    [Test]
    [TestCase(FacingDirection.North, FacingDirection.South, true)]
    [TestCase(FacingDirection.East, FacingDirection.West, true)]
    [TestCase(FacingDirection.North, FacingDirection.East, false)]
    [TestCase(FacingDirection.North, FacingDirection.NorthEast, false)]
    public void IsOpposite_ReturnsCorrectResult(FacingDirection a, FacingDirection b, bool expected)
    {
        // Act
        var result = a.IsOpposite(b);

        // Assert
        result.Should().Be(expected);
    }

    // ===== IsAdjacent Tests =====

    [Test]
    [TestCase(FacingDirection.North, FacingDirection.NorthEast, true)]
    [TestCase(FacingDirection.North, FacingDirection.NorthWest, true)]
    [TestCase(FacingDirection.North, FacingDirection.East, false)]
    [TestCase(FacingDirection.North, FacingDirection.South, false)]
    public void IsAdjacent_ReturnsCorrectResult(FacingDirection a, FacingDirection b, bool expected)
    {
        // Act
        var result = a.IsAdjacent(b);

        // Assert
        result.Should().Be(expected);
    }

    // ===== IsBehind Tests =====

    [Test]
    public void IsBehind_AttackerDirectlyBehind_ReturnsTrue()
    {
        // Arrange - Target at (5,5) facing North, attacker at (5,6) - behind
        var target = new GridPosition(5, 5);
        var attacker = new GridPosition(5, 6);
        var targetFacing = FacingDirection.North;

        // Act
        var result = FacingDirectionExtensions.IsBehind(attacker, target, targetFacing);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsBehind_AttackerInFront_ReturnsFalse()
    {
        // Arrange - Target at (5,5) facing North, attacker at (5,4) - in front
        var target = new GridPosition(5, 5);
        var attacker = new GridPosition(5, 4);
        var targetFacing = FacingDirection.North;

        // Act
        var result = FacingDirectionExtensions.IsBehind(attacker, target, targetFacing);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void IsBehind_AttackerAtSide_ReturnsFalse()
    {
        // Arrange - Target at (5,5) facing North, attacker at (6,5) - on side
        var target = new GridPosition(5, 5);
        var attacker = new GridPosition(6, 5);
        var targetFacing = FacingDirection.North;

        // Act
        var result = FacingDirectionExtensions.IsBehind(attacker, target, targetFacing);

        // Assert
        result.Should().BeFalse();
    }

    // ===== IsSide Tests =====

    [Test]
    public void IsSide_AttackerOnLeftSide_ReturnsTrue()
    {
        // Arrange - Target at (5,5) facing North, attacker at (4,5) - on left side
        var target = new GridPosition(5, 5);
        var attacker = new GridPosition(4, 5);
        var targetFacing = FacingDirection.North;

        // Act
        var result = FacingDirectionExtensions.IsSide(attacker, target, targetFacing);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsSide_AttackerOnRightSide_ReturnsTrue()
    {
        // Arrange - Target at (5,5) facing North, attacker at (6,5) - on right side
        var target = new GridPosition(5, 5);
        var attacker = new GridPosition(6, 5);
        var targetFacing = FacingDirection.North;

        // Act
        var result = FacingDirectionExtensions.IsSide(attacker, target, targetFacing);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsSide_AttackerInFront_ReturnsFalse()
    {
        // Arrange - Target at (5,5) facing North, attacker at (5,4) - in front
        var target = new GridPosition(5, 5);
        var attacker = new GridPosition(5, 4);
        var targetFacing = FacingDirection.North;

        // Act
        var result = FacingDirectionExtensions.IsSide(attacker, target, targetFacing);

        // Assert
        result.Should().BeFalse();
    }

    // ===== GetArrow Tests =====

    [Test]
    [TestCase(FacingDirection.North, '↑')]
    [TestCase(FacingDirection.South, '↓')]
    [TestCase(FacingDirection.East, '→')]
    [TestCase(FacingDirection.West, '←')]
    [TestCase(FacingDirection.NorthEast, '↗')]
    [TestCase(FacingDirection.SouthWest, '↙')]
    public void GetArrow_ReturnsCorrectArrowCharacter(FacingDirection direction, char expected)
    {
        // Act
        var result = direction.GetArrow();

        // Assert
        result.Should().Be(expected);
    }
}
