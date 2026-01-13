using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the LockDefinition value object.
/// </summary>
[TestFixture]
public class LockDefinitionTests
{
    #region None Tests

    [Test]
    public void None_HasNoLock()
    {
        // Arrange & Act
        var lockDef = LockDefinition.None;

        // Assert
        lockDef.HasLock.Should().BeFalse();
        lockDef.LockId.Should().BeEmpty();
        lockDef.RequiredKeyId.Should().BeNull();
        lockDef.IsLockpickable.Should().BeFalse();
    }

    #endregion

    #region KeyOnly Tests

    [Test]
    public void KeyOnly_HasCorrectProperties()
    {
        // Act
        var lockDef = LockDefinition.KeyOnly("vault-lock", "vault-key");

        // Assert
        lockDef.HasLock.Should().BeTrue();
        lockDef.LockId.Should().Be("vault-lock");
        lockDef.RequiredKeyId.Should().Be("vault-key");
        lockDef.IsLockpickable.Should().BeFalse();
        lockDef.CanRelock.Should().BeTrue();
        lockDef.KeyConsumedOnUse.Should().BeFalse();
    }

    [Test]
    public void KeyOnly_WithConsumeKey_SetsCorrectly()
    {
        // Act
        var lockDef = LockDefinition.KeyOnly("one-time-lock", "one-time-key", consumeKey: true);

        // Assert
        lockDef.KeyConsumedOnUse.Should().BeTrue();
    }

    [Test]
    public void KeyOnly_WithNoRelock_SetsCorrectly()
    {
        // Act
        var lockDef = LockDefinition.KeyOnly("vault-lock", "vault-key", canRelock: false);

        // Assert
        lockDef.CanRelock.Should().BeFalse();
    }

    #endregion

    #region Pickable Tests

    [Test]
    public void Pickable_HasCorrectDC()
    {
        // Act
        var lockDef = LockDefinition.Pickable("chest-lock", dc: 12);

        // Assert
        lockDef.HasLock.Should().BeTrue();
        lockDef.IsLockpickable.Should().BeTrue();
        lockDef.LockpickDC.Should().Be(12);
        lockDef.RequiredKeyId.Should().BeNull();
    }

    [Test]
    public void Pickable_WithKey_HasBothOptions()
    {
        // Act
        var lockDef = LockDefinition.Pickable("chest-lock", dc: 14, keyId: "chest-key");

        // Assert
        lockDef.IsLockpickable.Should().BeTrue();
        lockDef.LockpickDC.Should().Be(14);
        lockDef.RequiredKeyId.Should().Be("chest-key");
    }

    #endregion

    #region KeyOrPick Tests

    [Test]
    public void KeyOrPick_HasBothKeyAndLockpick()
    {
        // Act
        var lockDef = LockDefinition.KeyOrPick("door-lock", "door-key", dc: 10);

        // Assert
        lockDef.HasLock.Should().BeTrue();
        lockDef.LockId.Should().Be("door-lock");
        lockDef.RequiredKeyId.Should().Be("door-key");
        lockDef.IsLockpickable.Should().BeTrue();
        lockDef.LockpickDC.Should().Be(10);
        lockDef.CanRelock.Should().BeTrue();
    }

    #endregion

    #region KeyMatches Tests

    [Test]
    public void KeyMatches_WithMatchingKey_ReturnsTrue()
    {
        // Arrange
        var lockDef = LockDefinition.KeyOnly("test-lock", "test-key");

        // Act
        var result = lockDef.KeyMatches("test-key");

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void KeyMatches_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var lockDef = LockDefinition.KeyOnly("test-lock", "Test-Key");

        // Act
        var result = lockDef.KeyMatches("TEST-KEY");

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void KeyMatches_WithWrongKey_ReturnsFalse()
    {
        // Arrange
        var lockDef = LockDefinition.KeyOnly("test-lock", "test-key");

        // Act
        var result = lockDef.KeyMatches("other-key");

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void KeyMatches_WithNoRequiredKey_ReturnsFalse()
    {
        // Arrange
        var lockDef = LockDefinition.Pickable("test-lock", 10);

        // Act
        var result = lockDef.KeyMatches("any-key");

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
