// ═══════════════════════════════════════════════════════════════════════════════
// TheWallLivesStateTests.cs
// Unit tests for the TheWallLivesState value object.
// Version: 0.20.1c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

[TestFixture]
public class TheWallLivesStateTests
{
    [Test]
    public void Activate_InitializesWithCorrectDefaults()
    {
        // Arrange & Act
        var state = TheWallLivesState.Activate();

        // Assert
        state.IsActive.Should().BeTrue();
        state.TurnsRemaining.Should().Be(3);
        state.ActivatedAt.Should().NotBeNull();
        state.IsExpired().Should().BeFalse();
    }

    [Test]
    public void Deactivate_CreatesInactiveState()
    {
        // Arrange & Act
        var state = TheWallLivesState.Deactivate();

        // Assert
        state.IsActive.Should().BeFalse();
        state.TurnsRemaining.Should().Be(0);
        state.ActivatedAt.Should().BeNull();
        state.IsExpired().Should().BeTrue();
    }

    [Test]
    public void Tick_DecrementsTurnsRemaining()
    {
        // Arrange
        var state = TheWallLivesState.Activate();

        // Act
        var afterFirstTick = state.Tick();

        // Assert
        afterFirstTick.IsActive.Should().BeTrue();
        afterFirstTick.TurnsRemaining.Should().Be(2);
        afterFirstTick.IsExpired().Should().BeFalse();
    }

    [Test]
    public void Tick_DeactivatesAtZeroTurns()
    {
        // Arrange
        var state = TheWallLivesState.Activate();

        // Act — tick through all 3 turns
        var tick1 = state.Tick();
        var tick2 = tick1.Tick();
        var tick3 = tick2.Tick();

        // Assert — deactivated after 3 ticks
        tick3.IsActive.Should().BeFalse();
        tick3.TurnsRemaining.Should().Be(0);
        tick3.IsExpired().Should().BeTrue();
    }

    [Test]
    public void Tick_WhenInactive_ReturnsSelf()
    {
        // Arrange
        var state = TheWallLivesState.Deactivate();

        // Act
        var ticked = state.Tick();

        // Assert — should be reference-equal (no change)
        ticked.Should().Be(state);
        ticked.IsActive.Should().BeFalse();
    }

    [Test]
    public void PreventLethalDamage_WhenWouldKill_CapsDamage()
    {
        // Arrange
        var state = TheWallLivesState.Activate();

        // Act — 10 damage would kill at 5 HP
        var result = state.PreventLethalDamage(currentHp: 5, incomingDamage: 10);

        // Assert — capped to leave 1 HP (5 - 1 = 4 max damage)
        result.Should().Be(4);
    }

    [Test]
    public void PreventLethalDamage_WhenExactlyLethal_CapsDamage()
    {
        // Arrange
        var state = TheWallLivesState.Activate();

        // Act — 5 damage at 5 HP would kill (result = 0)
        var result = state.PreventLethalDamage(currentHp: 5, incomingDamage: 5);

        // Assert — capped to leave 1 HP (5 - 1 = 4 max damage)
        result.Should().Be(4);
    }

    [Test]
    public void PreventLethalDamage_WhenNonLethal_ReturnsFullDamage()
    {
        // Arrange
        var state = TheWallLivesState.Activate();

        // Act — 5 damage at 20 HP is non-lethal
        var result = state.PreventLethalDamage(currentHp: 20, incomingDamage: 5);

        // Assert — no capping needed
        result.Should().Be(5);
    }

    [Test]
    public void PreventLethalDamage_WhenInactive_ReturnsFullDamage()
    {
        // Arrange
        var state = TheWallLivesState.Deactivate();

        // Act — even lethal damage passes through when inactive
        var result = state.PreventLethalDamage(currentHp: 5, incomingDamage: 10);

        // Assert — no protection
        result.Should().Be(10);
    }

    [Test]
    public void PreventLethalDamage_WhenHpIs1_ReturnsZeroDamage()
    {
        // Arrange
        var state = TheWallLivesState.Activate();

        // Act — any damage at 1 HP would be lethal
        var result = state.PreventLethalDamage(currentHp: 1, incomingDamage: 5);

        // Assert — capped to 0 (cannot go below 1)
        result.Should().Be(0);
    }

    [Test]
    public void GetRemainingDuration_WhenActive_ReturnsTurns()
    {
        // Arrange
        var state = TheWallLivesState.Activate();

        // Act & Assert
        state.GetRemainingDuration().Should().Be(3);
    }

    [Test]
    public void GetRemainingDuration_WhenInactive_ReturnsZero()
    {
        // Arrange
        var state = TheWallLivesState.Deactivate();

        // Act & Assert
        state.GetRemainingDuration().Should().Be(0);
    }

    [Test]
    public void Immutability_OriginalStateUnchangedAfterTick()
    {
        // Arrange
        var original = TheWallLivesState.Activate();

        // Act
        var ticked = original.Tick();

        // Assert — original is unchanged
        original.TurnsRemaining.Should().Be(3);
        original.IsActive.Should().BeTrue();
        ticked.TurnsRemaining.Should().Be(2);
    }

    [Test]
    public void ToString_WhenActive_ContainsActiveAndTurns()
    {
        // Arrange
        var state = TheWallLivesState.Activate();

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Contain("ACTIVE");
        result.Should().Contain("3 turns remaining");
    }

    [Test]
    public void ToString_WhenInactive_ContainsInactive()
    {
        // Arrange
        var state = TheWallLivesState.Deactivate();

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Contain("INACTIVE");
    }

    [Test]
    public void Constants_HaveExpectedValues()
    {
        // Assert
        TheWallLivesState.DefaultDuration.Should().Be(3);
        TheWallLivesState.ActivationCost.Should().Be(4);
        TheWallLivesState.MinimumHp.Should().Be(1);
    }
}
