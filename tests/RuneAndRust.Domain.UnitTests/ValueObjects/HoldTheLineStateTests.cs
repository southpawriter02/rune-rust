// ═══════════════════════════════════════════════════════════════════════════════
// HoldTheLineStateTests.cs
// Unit tests for the HoldTheLineState value object.
// Version: 0.20.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

[TestFixture]
public class HoldTheLineStateTests
{
    [Test]
    public void Activate_InitializesWithCorrectDefaults()
    {
        // Arrange & Act
        var state = HoldTheLineState.Activate((5, 5));

        // Assert
        state.IsActive.Should().BeTrue();
        state.TurnsRemaining.Should().Be(2);
        state.BlockedPosition.Should().Be((5, 5));
        state.ActivatedAt.Should().NotBeNull();
        state.IsExpired().Should().BeFalse();
    }

    [Test]
    public void Tick_DecrementsTurnsAndDeactivatesAtZero()
    {
        // Arrange
        var state = HoldTheLineState.Activate((3, 4));

        // Act — first tick
        var afterFirstTick = state.Tick();

        // Assert — still active with 1 turn remaining
        afterFirstTick.IsActive.Should().BeTrue();
        afterFirstTick.TurnsRemaining.Should().Be(1);
        afterFirstTick.IsExpired().Should().BeFalse();

        // Act — second tick
        var afterSecondTick = afterFirstTick.Tick();

        // Assert — deactivated at 0 turns
        afterSecondTick.IsActive.Should().BeFalse();
        afterSecondTick.TurnsRemaining.Should().Be(0);
        afterSecondTick.IsExpired().Should().BeTrue();
    }

    [Test]
    public void Tick_WhenInactive_ReturnsUnchanged()
    {
        // Arrange
        var state = HoldTheLineState.Deactivate();

        // Act
        var ticked = state.Tick();

        // Assert — should be reference-equal (no change)
        ticked.Should().Be(state);
        ticked.IsActive.Should().BeFalse();
    }

    [Test]
    public void Deactivate_CreatesInactiveState()
    {
        // Arrange & Act
        var state = HoldTheLineState.Deactivate();

        // Assert
        state.IsActive.Should().BeFalse();
        state.TurnsRemaining.Should().Be(0);
        state.ActivatedAt.Should().BeNull();
        state.IsExpired().Should().BeTrue();
    }

    [Test]
    public void IsMovementBlocked_WhenActive_BlocksMatchingPosition()
    {
        // Arrange
        var state = HoldTheLineState.Activate((5, 5));

        // Act & Assert
        state.IsMovementBlocked((5, 5)).Should().BeTrue();
        state.IsMovementBlocked((6, 5)).Should().BeFalse();
        state.IsMovementBlocked((5, 6)).Should().BeFalse();
    }

    [Test]
    public void IsMovementBlocked_WhenInactive_NeverBlocks()
    {
        // Arrange
        var state = HoldTheLineState.Deactivate();

        // Act & Assert
        state.IsMovementBlocked((5, 5)).Should().BeFalse();
    }

    [Test]
    public void GetBlockedPositions_WhenActive_ReturnsSinglePosition()
    {
        // Arrange
        var state = HoldTheLineState.Activate((7, 3));

        // Act
        var blocked = state.GetBlockedPositions();

        // Assert
        blocked.Should().ContainSingle().Which.Should().Be((7, 3));
    }

    [Test]
    public void GetBlockedPositions_WhenInactive_ReturnsEmpty()
    {
        // Arrange
        var state = HoldTheLineState.Deactivate();

        // Act
        var blocked = state.GetBlockedPositions();

        // Assert
        blocked.Should().BeEmpty();
    }

    [Test]
    public void ShouldTerminateEffect_WhenSkjaldmaerMoves_ReturnsTrue()
    {
        // Arrange
        var state = HoldTheLineState.Activate((5, 5));

        // Act & Assert — moved to a different position
        state.ShouldTerminateEffect((6, 5)).Should().BeTrue();
        state.ShouldTerminateEffect((5, 6)).Should().BeTrue();

        // Still at blocked position — should not terminate
        state.ShouldTerminateEffect((5, 5)).Should().BeFalse();
    }

    [Test]
    public void Immutability_OriginalStateUnchangedAfterTick()
    {
        // Arrange
        var original = HoldTheLineState.Activate((5, 5));

        // Act
        var ticked = original.Tick();

        // Assert — original is unchanged
        original.TurnsRemaining.Should().Be(2);
        original.IsActive.Should().BeTrue();
        ticked.TurnsRemaining.Should().Be(1);
    }
}
