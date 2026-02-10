using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="TheWallLivesState"/> capstone ability state.
/// </summary>
[TestFixture]
public class TheWallLivesStateTests
{
    [Test]
    public void Activate_SetsActiveAndThreeTurns()
    {
        // Arrange
        var state = new TheWallLivesState();

        // Act
        state.Activate();

        // Assert
        state.IsActive.Should().BeTrue();
        state.TurnsRemaining.Should().Be(3);
        state.ActivatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Tick_DecrementsAndExpiresAfterThreeTicks()
    {
        // Arrange
        var state = new TheWallLivesState();
        state.Activate();

        // Act & Assert
        state.IsActive.Should().BeTrue();
        state.TurnsRemaining.Should().Be(3);

        state.Tick();
        state.TurnsRemaining.Should().Be(2);
        state.IsActive.Should().BeTrue();

        state.Tick();
        state.TurnsRemaining.Should().Be(1);
        state.IsActive.Should().BeTrue();

        state.Tick();
        state.TurnsRemaining.Should().Be(0);
        state.IsActive.Should().BeFalse();
    }

    [Test]
    public void Tick_WhenInactive_DoesNothing()
    {
        // Arrange
        var state = new TheWallLivesState();

        // Act
        state.Tick();

        // Assert
        state.IsActive.Should().BeFalse();
        state.TurnsRemaining.Should().Be(0);
    }

    [Test]
    public void PreventLethalDamage_WhenActive_CapsDamageToPreserveOneHp()
    {
        // Arrange
        var state = new TheWallLivesState();
        state.Activate();

        // Act — 10 damage to 5 HP would kill, should cap at 4
        var cappedDamage = state.PreventLethalDamage(currentHp: 5, incomingDamage: 10);

        // Assert
        cappedDamage.Should().Be(4); // 5 - 4 = 1 HP remaining
    }

    [Test]
    public void PreventLethalDamage_WhenActive_NonLethalDamage_ReturnsFullDamage()
    {
        // Arrange
        var state = new TheWallLivesState();
        state.Activate();

        // Act — 3 damage to 10 HP is not lethal
        var result = state.PreventLethalDamage(currentHp: 10, incomingDamage: 3);

        // Assert
        result.Should().Be(3);
    }

    [Test]
    public void PreventLethalDamage_WhenInactive_ReturnsFullDamage()
    {
        // Arrange
        var state = new TheWallLivesState();

        // Act
        var result = state.PreventLethalDamage(currentHp: 5, incomingDamage: 10);

        // Assert
        result.Should().Be(10);
    }

    [Test]
    public void PreventLethalDamage_WhenActive_AtOneHp_ReturnsZeroDamage()
    {
        // Arrange
        var state = new TheWallLivesState();
        state.Activate();

        // Act — already at 1 HP, any damage would be lethal
        var result = state.PreventLethalDamage(currentHp: 1, incomingDamage: 50);

        // Assert
        result.Should().Be(0); // Cannot take any damage
    }

    [Test]
    public void Deactivate_SetsInactiveAndZeroTurns()
    {
        // Arrange
        var state = new TheWallLivesState();
        state.Activate();

        // Act
        state.Deactivate();

        // Assert
        state.IsActive.Should().BeFalse();
        state.TurnsRemaining.Should().Be(0);
    }

    [Test]
    public void IsExpired_WhenActive_ReturnsFalse()
    {
        // Arrange
        var state = new TheWallLivesState();
        state.Activate();

        // Assert
        state.IsExpired().Should().BeFalse();
    }

    [Test]
    public void IsExpired_AfterAllTicks_ReturnsTrue()
    {
        // Arrange
        var state = new TheWallLivesState();
        state.Activate();
        state.Tick();
        state.Tick();
        state.Tick();

        // Assert
        state.IsExpired().Should().BeTrue();
    }

    [Test]
    public void GetRemainingDuration_WhenActive_ReturnsTurnsRemaining()
    {
        // Arrange
        var state = new TheWallLivesState();
        state.Activate();
        state.Tick();

        // Assert
        state.GetRemainingDuration().Should().Be(2);
    }

    [Test]
    public void GetRemainingDuration_WhenInactive_ReturnsZero()
    {
        // Arrange
        var state = new TheWallLivesState();

        // Assert
        state.GetRemainingDuration().Should().Be(0);
    }
}
