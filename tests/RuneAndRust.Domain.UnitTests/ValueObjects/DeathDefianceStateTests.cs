using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="DeathDefianceState"/> value object.
/// </summary>
[TestFixture]
public class DeathDefianceStateTests
{
    // ===== Factory Tests =====

    [Test]
    public void Create_InitializesCorrectly()
    {
        // Arrange
        var characterId = Guid.NewGuid();

        // Act
        var state = DeathDefianceState.Create(characterId);

        // Assert
        state.CharacterId.Should().Be(characterId);
        state.HasTriggeredThisCombat.Should().BeFalse();
        state.TriggeredAt.Should().BeNull();
        state.DamagePreventedOnTrigger.Should().Be(0);
        state.StateId.Should().NotBeEmpty();
    }

    // ===== CanTrigger Tests =====

    [Test]
    public void CanTrigger_WhenNotTriggered_ReturnsTrue()
    {
        // Arrange
        var state = DeathDefianceState.Create(Guid.NewGuid());

        // Act & Assert
        state.CanTrigger().Should().BeTrue();
    }

    [Test]
    public void CanTrigger_WhenAlreadyTriggered_ReturnsFalse()
    {
        // Arrange
        var state = DeathDefianceState.Create(Guid.NewGuid());
        state.Trigger(25);

        // Act & Assert
        state.CanTrigger().Should().BeFalse();
    }

    // ===== Trigger Tests =====

    [Test]
    public void Trigger_RecordsDamageAndTimestamp()
    {
        // Arrange
        var state = DeathDefianceState.Create(Guid.NewGuid());

        // Act
        state.Trigger(30);

        // Assert
        state.HasTriggeredThisCombat.Should().BeTrue();
        state.DamagePreventedOnTrigger.Should().Be(30);
        state.TriggeredAt.Should().NotBeNull();
        state.TriggeredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Trigger_CannotTriggerTwice()
    {
        // Arrange
        var state = DeathDefianceState.Create(Guid.NewGuid());
        state.Trigger(25);

        // Act — trigger again (state is already marked)
        state.Trigger(50);

        // Assert — the second trigger overwrites values but CanTrigger still returns false
        state.HasTriggeredThisCombat.Should().BeTrue();
        state.CanTrigger().Should().BeFalse();
    }

    // ===== RageGainOnTrigger Tests =====

    [Test]
    public void RageGainOnTrigger_IsTwenty()
    {
        // Assert
        DeathDefianceState.RageGainOnTrigger.Should().Be(20);
    }

    // ===== ResetForNewCombat Tests =====

    [Test]
    public void ResetForNewCombat_ClearsTriggeredState()
    {
        // Arrange
        var state = DeathDefianceState.Create(Guid.NewGuid());
        state.Trigger(30);
        state.HasTriggeredThisCombat.Should().BeTrue();

        // Act
        state.ResetForNewCombat();

        // Assert
        state.HasTriggeredThisCombat.Should().BeFalse();
        state.TriggeredAt.Should().BeNull();
        state.DamagePreventedOnTrigger.Should().Be(0);
    }

    [Test]
    public void ResetForNewCombat_AllowsRetrigger()
    {
        // Arrange
        var state = DeathDefianceState.Create(Guid.NewGuid());
        state.Trigger(25);
        state.CanTrigger().Should().BeFalse();

        // Act
        state.ResetForNewCombat();

        // Assert
        state.CanTrigger().Should().BeTrue();
    }

    // ===== GetDescription Tests =====

    [Test]
    public void GetDescription_WhenReady_ShowsReady()
    {
        // Arrange
        var state = DeathDefianceState.Create(Guid.NewGuid());

        // Act
        var description = state.GetDescription();

        // Assert
        description.Should().Contain("Ready");
        description.Should().Contain("prevent death once");
    }

    [Test]
    public void GetDescription_WhenUsed_ShowsUsed()
    {
        // Arrange
        var state = DeathDefianceState.Create(Guid.NewGuid());
        state.Trigger(30);

        // Act
        var description = state.GetDescription();

        // Assert
        description.Should().Contain("USED");
        description.Should().Contain("prevented 30 damage");
    }
}
