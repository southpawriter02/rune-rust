using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for InteractiveObject destruction functionality.
/// </summary>
[TestFixture]
public class InteractiveObjectDestructionTests
{
    // ===== SetDestructible Tests =====

    [Test]
    public void SetDestructible_ConfiguresDestructibility()
    {
        // Arrange
        var crate = CreateCrate();
        var props = DestructibleProperties.Weak(10);

        // Act
        crate.SetDestructible(props);

        // Assert
        crate.IsDestructible.Should().BeTrue();
        crate.Destructible.Should().NotBeNull();
        crate.Destructible!.MaxHP.Should().Be(10);
    }

    [Test]
    public void SetDestructible_WithNull_ThrowsException()
    {
        // Arrange
        var crate = CreateCrate();

        // Act
        var act = () => crate.SetDestructible(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void IsDestructible_WhenNotSet_ReturnsFalse()
    {
        // Arrange
        var door = CreateDoor();

        // Assert
        door.IsDestructible.Should().BeFalse();
        door.Destructible.Should().BeNull();
    }

    // ===== TakeDamage Tests =====

    [Test]
    public void TakeDamage_ReducesHP()
    {
        // Arrange
        var crate = CreateDestructibleCrate(15);

        // Act
        var dealt = crate.TakeDamage(5);

        // Assert
        dealt.Should().Be(5);
        crate.Destructible!.CurrentHP.Should().Be(10);
    }

    [Test]
    public void TakeDamage_WhenNotDestructible_ReturnsZero()
    {
        // Arrange
        var door = CreateDoor();

        // Act
        var dealt = door.TakeDamage(10);

        // Assert
        dealt.Should().Be(0);
    }

    [Test]
    public void TakeDamage_WhenDestroyed_ReturnsZero()
    {
        // Arrange
        var crate = CreateDestructibleCrate(5);
        crate.TakeDamage(10); // Destroy

        // Act
        var dealt = crate.TakeDamage(5);

        // Assert
        dealt.Should().Be(0);
    }

    [Test]
    public void TakeDamage_SetsDestroyedState_WhenHPReachesZero()
    {
        // Arrange
        var crate = CreateDestructibleCrate(10);

        // Act
        crate.TakeDamage(10);

        // Assert
        crate.IsDestroyed.Should().BeTrue();
        crate.State.Should().Be(ObjectState.Destroyed);
    }

    [Test]
    public void TakeDamage_WithVulnerability_DealsDoubleDamage()
    {
        // Arrange
        var crate = CreateCrate();
        var props = DestructibleProperties.Create(
            maxHP: 20,
            vulnerabilities: new[] { "fire" });
        crate.SetDestructible(props);

        // Act
        var dealt = crate.TakeDamage(5, "fire");

        // Assert
        dealt.Should().Be(10);
        crate.Destructible!.CurrentHP.Should().Be(10);
    }

    [Test]
    public void TakeDamage_WithImmunity_DealsNoDamage()
    {
        // Arrange
        var crate = CreateCrate();
        var props = DestructibleProperties.Create(
            maxHP: 20,
            immunities: new[] { "poison" });
        crate.SetDestructible(props);

        // Act
        var dealt = crate.TakeDamage(10, "poison");

        // Assert
        dealt.Should().Be(0);
        crate.Destructible!.CurrentHP.Should().Be(20);
    }

    // ===== IsDestroyed Tests =====

    [Test]
    public void IsDestroyed_WhenStateIsDestroyed_ReturnsTrue()
    {
        // Arrange
        var obj = CreateCrate();
        obj.Destroy();

        // Assert
        obj.IsDestroyed.Should().BeTrue();
    }

    [Test]
    public void IsDestroyed_WhenStateIsBroken_ReturnsTrue()
    {
        // Arrange
        var obj = CreateCrate();
        obj.TrySetState(ObjectState.Broken);

        // Assert
        obj.IsDestroyed.Should().BeTrue();
    }

    [Test]
    public void IsDestroyed_WhenDestructibleHPIsZero_ReturnsTrue()
    {
        // Arrange
        var crate = CreateDestructibleCrate(5);
        crate.TakeDamage(5);

        // Assert
        crate.IsDestroyed.Should().BeTrue();
        crate.Destructible!.IsDestroyed.Should().BeTrue();
    }

    // ===== Destroy Tests =====

    [Test]
    public void Destroy_SetsStateToDestroyed()
    {
        // Arrange
        var obj = CreateCrate();

        // Act
        obj.Destroy();

        // Assert
        obj.State.Should().Be(ObjectState.Destroyed);
        obj.IsDestroyed.Should().BeTrue();
    }

    [Test]
    public void Destroy_BypassesDamage()
    {
        // Arrange
        var crate = CreateDestructibleCrate(100);

        // Act
        crate.Destroy();

        // Assert
        crate.State.Should().Be(ObjectState.Destroyed);
        crate.Destructible!.CurrentHP.Should().Be(100); // HP not changed
    }

    // ===== Can Interact Tests =====

    [Test]
    public void CanInteract_WhenDestroyed_ReturnsFalse()
    {
        // Arrange
        var obj = CreateCrate();
        obj.Destroy();

        // Assert
        obj.CanInteract.Should().BeFalse();
    }

    [Test]
    public void Activate_WhenDestroyed_ReturnsFalse()
    {
        // Arrange
        var lever = CreateLever();
        lever.Destroy();

        // Act
        var result = lever.Activate();

        // Assert
        result.Should().BeFalse();
    }

    // ===== Passage Blocking Tests =====

    [Test]
    public void IsCurrentlyBlocking_WhenDestroyed_ReturnsFalse()
    {
        // Arrange
        var barrier = InteractiveObject.Create(
            definitionId: "barrier",
            name: "Barrier",
            description: "A blocking barrier",
            objectType: InteractiveObjectType.Door,
            defaultState: ObjectState.Closed,
            blocksPassage: true,
            blockedDirection: Direction.North);

        // Verify initially blocking
        barrier.IsCurrentlyBlocking.Should().BeTrue();

        // Act
        barrier.Destroy();

        // Assert
        barrier.IsCurrentlyBlocking.Should().BeFalse();
    }

    // ===== Helper Methods =====

    private static InteractiveObject CreateCrate()
    {
        return InteractiveObject.Create(
            definitionId: "wooden-crate",
            name: "Wooden Crate",
            description: "A wooden crate",
            objectType: InteractiveObjectType.Crate,
            defaultState: ObjectState.Closed);
    }

    private static InteractiveObject CreateDestructibleCrate(int maxHP)
    {
        var crate = CreateCrate();
        crate.SetDestructible(DestructibleProperties.Create(maxHP));
        return crate;
    }

    private static InteractiveObject CreateDoor()
    {
        return InteractiveObject.Create(
            definitionId: "iron-door",
            name: "Iron Door",
            description: "A sturdy iron door",
            objectType: InteractiveObjectType.Door,
            defaultState: ObjectState.Closed);
    }

    private static InteractiveObject CreateLever()
    {
        return InteractiveObject.Create(
            definitionId: "lever",
            name: "Lever",
            description: "A lever",
            objectType: InteractiveObjectType.Lever,
            defaultState: ObjectState.Inactive);
    }
}
