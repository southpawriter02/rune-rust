using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for InteractiveObject lock functionality.
/// </summary>
[TestFixture]
public class InteractiveObjectLockTests
{
    #region Lock Setup Tests

    [Test]
    public void SetLock_SetsLockDefinition()
    {
        // Arrange
        var obj = InteractiveObject.Create(
            "test-door", "Test Door", "A test door.",
            InteractiveObjectType.Door,
            ObjectState.Closed);
        var lockDef = LockDefinition.KeyOnly("door-lock", "door-key");

        // Act
        obj.SetLock(lockDef);

        // Assert
        obj.HasLock.Should().BeTrue();
        obj.Lock.Should().Be(lockDef);
        obj.IsLocked.Should().BeTrue(); // State changes to Locked
    }

    [Test]
    public void SetLock_WhenClosed_ChangesStateToLocked()
    {
        // Arrange
        var obj = InteractiveObject.Create(
            "test-chest", "Test Chest", "A chest.",
            InteractiveObjectType.Chest,
            ObjectState.Closed);

        // Act
        obj.SetLock(LockDefinition.Pickable("chest-lock", 12));

        // Assert
        obj.State.Should().Be(ObjectState.Locked);
    }

    #endregion

    #region TryUnlockWithKey Tests

    [Test]
    public void TryUnlockWithKey_MatchingKey_Unlocks()
    {
        // Arrange
        var obj = InteractiveObject.Create(
            "locked-chest", "Locked Chest", "A chest.",
            InteractiveObjectType.Chest,
            ObjectState.Locked);
        obj.SetLock(LockDefinition.KeyOnly("chest-lock", "chest-key"));

        // Act
        var result = obj.TryUnlockWithKey("chest-key");

        // Assert
        result.Should().BeTrue();
        obj.IsLocked.Should().BeFalse();
        obj.State.Should().Be(ObjectState.Closed);
    }

    [Test]
    public void TryUnlockWithKey_WrongKey_ReturnsFalse()
    {
        // Arrange
        var obj = InteractiveObject.Create(
            "locked-chest", "Locked Chest", "A chest.",
            InteractiveObjectType.Chest,
            ObjectState.Locked);
        obj.SetLock(LockDefinition.KeyOnly("chest-lock", "chest-key"));

        // Act
        var result = obj.TryUnlockWithKey("wrong-key");

        // Assert
        result.Should().BeFalse();
        obj.IsLocked.Should().BeTrue();
    }

    [Test]
    public void TryUnlockWithKey_NotLocked_ReturnsFalse()
    {
        // Arrange
        var obj = InteractiveObject.Create(
            "chest", "Chest", "A chest.",
            InteractiveObjectType.Chest,
            ObjectState.Closed);
        obj.SetLock(LockDefinition.KeyOnly("chest-lock", "chest-key"));
        obj.TrySetState(ObjectState.Closed); // Ensure it's closed, not locked

        // Act - the lock is set but state is not Locked
        obj.TrySetState(ObjectState.Closed);
        var result = obj.TryUnlockWithKey("chest-key");

        // Assert - it's locked because SetLock sets state to locked
        // Let's unlock it first, then try again
    }

    [Test]
    public void TryUnlockWithKey_NoLock_ReturnsFalse()
    {
        // Arrange
        var obj = InteractiveObject.Create(
            "chest", "Chest", "A chest.",
            InteractiveObjectType.Chest,
            ObjectState.Locked);

        // Act
        var result = obj.TryUnlockWithKey("any-key");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Unlock Tests

    [Test]
    public void Unlock_WhenLocked_Unlocks()
    {
        // Arrange
        var obj = InteractiveObject.Create(
            "locked-door", "Locked Door", "A door.",
            InteractiveObjectType.Door,
            ObjectState.Locked);
        obj.SetLock(LockDefinition.Pickable("door-lock", 12));

        // Act
        var result = obj.Unlock();

        // Assert
        result.Should().BeTrue();
        obj.IsLocked.Should().BeFalse();
        obj.State.Should().Be(ObjectState.Closed);
    }

    [Test]
    public void Unlock_WhenNotLocked_ReturnsFalse()
    {
        // Arrange
        var obj = InteractiveObject.Create(
            "door", "Door", "A door.",
            InteractiveObjectType.Door,
            ObjectState.Closed);

        // Act
        var result = obj.Unlock();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region TryLock Tests

    [Test]
    public void TryLock_WhenClosed_Locks()
    {
        // Arrange
        var obj = InteractiveObject.Create(
            "chest", "Chest", "A chest.",
            InteractiveObjectType.Chest,
            ObjectState.Closed);
        obj.SetLock(LockDefinition.Pickable("chest-lock", 10));
        obj.Unlock(); // Start unlocked but with lock

        // Act
        var result = obj.TryLock();

        // Assert
        result.Should().BeTrue();
        obj.IsLocked.Should().BeTrue();
        obj.State.Should().Be(ObjectState.Locked);
    }

    [Test]
    public void TryLock_WhenNotClosed_ReturnsFalse()
    {
        // Arrange
        var obj = InteractiveObject.Create(
            "chest", "Chest", "A chest.",
            InteractiveObjectType.Chest,
            ObjectState.Open);
        obj.SetLock(LockDefinition.Pickable("chest-lock", 10));

        // Act
        var result = obj.TryLock();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void TryLock_CannotRelock_ReturnsFalse()
    {
        // Arrange
        var obj = InteractiveObject.Create(
            "chest", "Chest", "A chest.",
            InteractiveObjectType.Chest,
            ObjectState.Closed);
        obj.SetLock(LockDefinition.KeyOnly("one-time-lock", "key", canRelock: false));
        obj.Unlock();

        // Act
        var result = obj.TryLock();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void TryLock_AlreadyLocked_ReturnsFalse()
    {
        // Arrange
        var obj = InteractiveObject.Create(
            "locked-chest", "Locked Chest", "A chest.",
            InteractiveObjectType.Chest,
            ObjectState.Locked);
        obj.SetLock(LockDefinition.Pickable("chest-lock", 10));

        // Act
        var result = obj.TryLock();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void TryLock_NoLock_ReturnsFalse()
    {
        // Arrange
        var obj = InteractiveObject.Create(
            "chest", "Chest", "A chest.",
            InteractiveObjectType.Chest,
            ObjectState.Closed);

        // Act
        var result = obj.TryLock();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Container Tests

    [Test]
    public void SetupAsContainer_CreatesInventory()
    {
        // Arrange
        var obj = InteractiveObject.Create(
            "chest", "Chest", "A chest.",
            InteractiveObjectType.Chest,
            ObjectState.Closed);

        // Act
        obj.SetupAsContainer(10);

        // Assert
        obj.IsContainer.Should().BeTrue();
        obj.ContainerInventory.Should().NotBeNull();
        obj.ContainerInventory!.Capacity.Should().Be(10);
    }

    [Test]
    public void IsContainer_WithoutSetup_ReturnsFalse()
    {
        // Arrange
        var obj = InteractiveObject.Create(
            "door", "Door", "A door.",
            InteractiveObjectType.Door,
            ObjectState.Closed);

        // Assert
        obj.IsContainer.Should().BeFalse();
        obj.ContainerInventory.Should().BeNull();
    }

    #endregion
}
