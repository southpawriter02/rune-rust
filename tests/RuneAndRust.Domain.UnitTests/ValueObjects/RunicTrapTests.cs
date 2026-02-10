// ═══════════════════════════════════════════════════════════════════════════════
// RunicTrapTests.cs
// Unit tests for the RunicTrap value object, validating creation, trigger
// mechanics, expiry, visibility, and display methods.
// Version: 0.20.2b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tests for <see cref="RunicTrap"/>.
/// </summary>
[TestFixture]
public class RunicTrapTests
{
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly Guid _enemyId = Guid.NewGuid();
    private readonly (int X, int Y) _position = (5, 3);

    // ─────────────────────────────────────────────────────────────────────────
    // Creation Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void Create_WithValidInputs_CreatesActiveTrap()
    {
        // Arrange & Act
        var trap = RunicTrap.Create(_ownerId, _position);

        // Assert
        trap.OwnerId.Should().Be(_ownerId);
        trap.Position.Should().Be(_position);
        trap.DamageDice.Should().Be("3d6");
        trap.DetectionDc.Should().Be(14);
        trap.TriggerType.Should().Be(TrapTriggerType.Movement);
        trap.IsTriggered.Should().BeFalse();
        trap.IsActive.Should().BeTrue();
        trap.TriggeredByCharacterId.Should().BeNull();
        trap.TriggeredAt.Should().BeNull();
        trap.TrapId.Should().NotBeEmpty();
    }

    [Test]
    public void Create_SetsExpirationOneHourInFuture()
    {
        // Arrange & Act
        var before = DateTime.UtcNow;
        var trap = RunicTrap.Create(_ownerId, _position);
        var after = DateTime.UtcNow;

        // Assert — ExpiresAt should be approximately 1 hour after creation
        trap.ExpiresAt.Should().BeAfter(before.AddHours(1).AddSeconds(-1));
        trap.ExpiresAt.Should().BeBefore(after.AddHours(1).AddSeconds(1));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Trigger Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void Trigger_ActiveTrap_SucceedsWithDamage()
    {
        // Arrange
        var trap = RunicTrap.Create(_ownerId, _position);
        var seededRandom = new Random(42);

        // Act
        var (result, updatedTrap) = trap.Trigger(_enemyId, seededRandom);

        // Assert
        result.Success.Should().BeTrue();
        result.DamageRoll.Should().BeGreaterThan(0);
        result.DamageRoll.Should().BeLessThanOrEqualTo(18); // 3d6 max
        result.TargetId.Should().Be(_enemyId);
        result.TrapId.Should().Be(trap.TrapId);
        updatedTrap.IsTriggered.Should().BeTrue();
        updatedTrap.TriggeredByCharacterId.Should().Be(_enemyId);
        updatedTrap.TriggeredAt.Should().NotBeNull();
    }

    [Test]
    public void Trigger_AlreadyTriggeredTrap_ReturnsFailed()
    {
        // Arrange
        var trap = RunicTrap.Create(_ownerId, _position);
        var (_, triggeredTrap) = trap.Trigger(_enemyId, new Random(42));

        // Act — try to trigger again
        var (result, secondUpdate) = triggeredTrap.Trigger(Guid.NewGuid(), new Random(42));

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already been triggered");
        result.DamageRoll.Should().Be(0);
    }

    [Test]
    public void Trigger_ExpiredTrap_ReturnsFailed()
    {
        // Arrange — create a trap that expires in the past
        var trap = RunicTrap.CreateWithExpiration(
            _ownerId, _position, DateTime.UtcNow.AddHours(-1));

        // Act
        var (result, _) = trap.Trigger(_enemyId, new Random(42));

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("expired");
    }

    [Test]
    public void Trigger_DoesNotModifyOriginalInstance()
    {
        // Arrange
        var trap = RunicTrap.Create(_ownerId, _position);

        // Act
        _ = trap.Trigger(_enemyId, new Random(42));

        // Assert — original unchanged
        trap.IsTriggered.Should().BeFalse();
        trap.TriggeredByCharacterId.Should().BeNull();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Visibility Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void IsVisibleTo_Owner_ReturnsTrue()
    {
        // Arrange
        var trap = RunicTrap.Create(_ownerId, _position);

        // Act & Assert
        trap.IsVisibleTo(_ownerId).Should().BeTrue();
    }

    [Test]
    public void IsVisibleTo_NonOwner_ReturnsFalse()
    {
        // Arrange
        var trap = RunicTrap.Create(_ownerId, _position);

        // Act & Assert
        trap.IsVisibleTo(_enemyId).Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Detection & Expiry Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void CanBeDetected_ActiveTrap_ReturnsTrue()
    {
        // Arrange
        var trap = RunicTrap.Create(_ownerId, _position);

        // Act & Assert
        trap.CanBeDetected().Should().BeTrue();
    }

    [Test]
    public void CanBeDetected_TriggeredTrap_ReturnsFalse()
    {
        // Arrange
        var trap = RunicTrap.Create(_ownerId, _position);
        var (_, triggered) = trap.Trigger(_enemyId, new Random(42));

        // Act & Assert
        triggered.CanBeDetected().Should().BeFalse();
    }

    [Test]
    public void IsExpired_FutureTrap_ReturnsFalse()
    {
        // Arrange
        var trap = RunicTrap.Create(_ownerId, _position);

        // Act & Assert
        trap.IsExpired.Should().BeFalse();
    }

    [Test]
    public void IsExpired_PastTrap_ReturnsTrue()
    {
        // Arrange
        var trap = RunicTrap.CreateWithExpiration(
            _ownerId, _position, DateTime.UtcNow.AddHours(-1));

        // Act & Assert
        trap.IsExpired.Should().BeTrue();
        trap.IsActive.Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Constants Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void Constants_HaveCorrectValues()
    {
        // Assert
        RunicTrap.MaxActiveTraps.Should().Be(3);
        RunicTrap.DefaultDetectionDc.Should().Be(14);
        RunicTrap.DefaultDurationHours.Should().Be(1);
        RunicTrap.DefaultDamageDice.Should().Be("3d6");
        RunicTrap.DamageDiceCount.Should().Be(3);
        RunicTrap.DamageDiceSides.Should().Be(6);
    }
}
