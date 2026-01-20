using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for the Exit value object.
/// </summary>
[TestFixture]
public class ExitTests
{
    [Test]
    public void Standard_CreatesVisibleNonHiddenExit()
    {
        // Arrange
        var targetId = Guid.NewGuid();

        // Act
        var exit = Exit.Standard(targetId);

        // Assert
        exit.TargetRoomId.Should().Be(targetId);
        exit.IsHidden.Should().BeFalse();
        exit.IsDiscovered.Should().BeTrue();
        exit.IsVisible.Should().BeTrue();
        exit.DiscoveryDC.Should().Be(0);
        exit.StairType.Should().BeNull();
    }

    [Test]
    public void Hidden_CreatesHiddenUndiscoveredExit()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var hint = "a slight draft";

        // Act
        var exit = Exit.Hidden(targetId, 15, hint);

        // Assert
        exit.TargetRoomId.Should().Be(targetId);
        exit.IsHidden.Should().BeTrue();
        exit.IsDiscovered.Should().BeFalse();
        exit.IsVisible.Should().BeFalse();
        exit.DiscoveryDC.Should().Be(15);
        exit.HiddenHint.Should().Be(hint);
        exit.StairType.Should().BeNull();
    }

    [Test]
    public void Vertical_CreatesExitWithStairType()
    {
        // Arrange
        var targetId = Guid.NewGuid();

        // Act
        var exit = Exit.Vertical(targetId, StairType.Ladder);

        // Assert
        exit.TargetRoomId.Should().Be(targetId);
        exit.IsVisible.Should().BeTrue();
        exit.StairType.Should().Be(StairType.Ladder);
    }

    [Test]
    public void HiddenVertical_CreatesHiddenExitWithStairType()
    {
        // Arrange
        var targetId = Guid.NewGuid();

        // Act
        var exit = Exit.HiddenVertical(targetId, StairType.Shaft, 18, "sounds of distant water");

        // Assert
        exit.IsHidden.Should().BeTrue();
        exit.IsDiscovered.Should().BeFalse();
        exit.IsVisible.Should().BeFalse();
        exit.DiscoveryDC.Should().Be(18);
        exit.StairType.Should().Be(StairType.Shaft);
    }

    [Test]
    public void AsDiscovered_ReturnsNewExitWithIsDiscoveredTrue()
    {
        // Arrange
        var exit = Exit.Hidden(Guid.NewGuid(), 12);

        // Act
        var discovered = exit.AsDiscovered();

        // Assert
        discovered.IsHidden.Should().BeTrue();
        discovered.IsDiscovered.Should().BeTrue();
        discovered.IsVisible.Should().BeTrue();
        exit.IsDiscovered.Should().BeFalse(); // Original unchanged
    }

    [Test]
    public void IsVisible_TrueWhenNotHidden()
    {
        // Arrange & Act
        var exit = Exit.Standard(Guid.NewGuid());

        // Assert
        exit.IsVisible.Should().BeTrue();
    }

    [Test]
    public void IsVisible_FalseWhenHiddenAndNotDiscovered()
    {
        // Arrange & Act
        var exit = Exit.Hidden(Guid.NewGuid(), 10);

        // Assert
        exit.IsVisible.Should().BeFalse();
    }
}
