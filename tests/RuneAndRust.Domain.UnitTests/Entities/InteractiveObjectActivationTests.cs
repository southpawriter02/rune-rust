using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for InteractiveObject activation and button functionality.
/// </summary>
[TestFixture]
public class InteractiveObjectActivationTests
{
    // ===== Effect Tests =====

    [Test]
    public void AddEffect_AddsEffectToList()
    {
        // Arrange
        var lever = CreateLever();
        var effect = ObjectEffect.UnlockTarget("door", ObjectState.Active);

        // Act
        lever.AddEffect(effect);

        // Assert
        lever.Effects.Should().HaveCount(1);
        lever.Effects[0].Type.Should().Be(EffectType.UnlockTarget);
        lever.HasEffects.Should().BeTrue();
    }

    [Test]
    public void SetEffects_ReplacesExistingEffects()
    {
        // Arrange
        var lever = CreateLever();
        lever.AddEffect(ObjectEffect.OpenTarget("door1", ObjectState.Active));

        var newEffects = new[]
        {
            ObjectEffect.UnlockTarget("door2", ObjectState.Active),
            ObjectEffect.OpenTarget("door2", ObjectState.Active)
        };

        // Act
        lever.SetEffects(newEffects);

        // Assert
        lever.Effects.Should().HaveCount(2);
        lever.Effects[0].Type.Should().Be(EffectType.UnlockTarget);
    }

    [Test]
    public void GetTriggeredEffects_FiltersCorrectly()
    {
        // Arrange
        var lever = CreateLever();
        lever.AddEffect(ObjectEffect.UnlockTarget("door", ObjectState.Active));
        lever.AddEffect(ObjectEffect.LockTarget("door", ObjectState.Inactive));

        // Act
        var activeEffects = lever.GetTriggeredEffects(ObjectState.Active).ToList();
        var inactiveEffects = lever.GetTriggeredEffects(ObjectState.Inactive).ToList();

        // Assert
        activeEffects.Should().HaveCount(1);
        activeEffects[0].Type.Should().Be(EffectType.UnlockTarget);
        inactiveEffects.Should().HaveCount(1);
        inactiveEffects[0].Type.Should().Be(EffectType.LockTarget);
    }

    // ===== Activation Tests =====

    [Test]
    public void Activate_ChangesStateToActive()
    {
        // Arrange
        var lever = CreateLever();

        // Act
        var result = lever.Activate();

        // Assert
        result.Should().BeTrue();
        lever.State.Should().Be(ObjectState.Active);
    }

    [Test]
    public void Activate_WhenAlreadyActive_ReturnsFalse()
    {
        // Arrange
        var lever = CreateLever();
        lever.Activate();

        // Act
        var result = lever.Activate();

        // Assert
        result.Should().BeFalse();
        lever.State.Should().Be(ObjectState.Active);
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

    [Test]
    public void Deactivate_ChangesStateToInactive()
    {
        // Arrange
        var lever = CreateLever();
        lever.Activate();

        // Act
        var result = lever.Deactivate();

        // Assert
        result.Should().BeTrue();
        lever.State.Should().Be(ObjectState.Inactive);
    }

    [Test]
    public void Deactivate_WhenNotActive_ReturnsFalse()
    {
        // Arrange
        var lever = CreateLever();

        // Act
        var result = lever.Deactivate();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void Toggle_FromInactiveToActive()
    {
        // Arrange
        var lever = CreateLever();

        // Act
        var newState = lever.Toggle();

        // Assert
        newState.Should().Be(ObjectState.Active);
        lever.State.Should().Be(ObjectState.Active);
    }

    [Test]
    public void Toggle_FromActiveToInactive()
    {
        // Arrange
        var lever = CreateLever();
        lever.Activate();

        // Act
        var newState = lever.Toggle();

        // Assert
        newState.Should().Be(ObjectState.Inactive);
        lever.State.Should().Be(ObjectState.Inactive);
    }

    // ===== Button Tests =====

    [Test]
    public void SetAsButton_ConfiguresButtonBehavior()
    {
        // Arrange
        var button = CreateButton();

        // Act
        button.SetAsButton(5);

        // Assert
        button.IsButton.Should().BeTrue();
        button.ResetDelay.Should().Be(5);
    }

    [Test]
    public void Activate_WhenButton_StartsResetTimer()
    {
        // Arrange
        var button = CreateButton();
        button.SetAsButton(3);

        // Act
        button.Activate();

        // Assert
        button.State.Should().Be(ObjectState.Active);
        button.HasPendingReset.Should().BeTrue();
        button.TurnsUntilReset.Should().Be(3);
    }

    [Test]
    public void Deactivate_WhenButton_ReturnsFalse()
    {
        // Arrange
        var button = CreateButton();
        button.SetAsButton(3);
        button.Activate();

        // Act
        var result = button.Deactivate();

        // Assert
        result.Should().BeFalse(); // Buttons cannot be manually deactivated
        button.State.Should().Be(ObjectState.Active);
    }

    [Test]
    public void Toggle_WhenButtonActive_ReturnsNull()
    {
        // Arrange
        var button = CreateButton();
        button.SetAsButton(3);
        button.Activate();

        // Act
        var newState = button.Toggle();

        // Assert
        newState.Should().BeNull(); // Buttons cannot toggle from active
        button.State.Should().Be(ObjectState.Active);
    }

    [Test]
    public void ProcessTurnTick_DecrementsTimer()
    {
        // Arrange
        var button = CreateButton();
        button.SetAsButton(3);
        button.Activate();

        // Act
        var reset1 = button.ProcessTurnTick();
        var turnsAfter1 = button.TurnsUntilReset;

        var reset2 = button.ProcessTurnTick();
        var turnsAfter2 = button.TurnsUntilReset;

        // Assert
        reset1.Should().BeFalse();
        turnsAfter1.Should().Be(2);
        reset2.Should().BeFalse();
        turnsAfter2.Should().Be(1);
    }

    [Test]
    public void ProcessTurnTick_ResetsButtonWhenTimerExpires()
    {
        // Arrange
        var button = CreateButton();
        button.SetAsButton(1); // Reset after 1 tick
        button.Activate();

        // Act
        button.ProcessTurnTick(); // Decrement to 0
        var didReset = button.ProcessTurnTick(); // Timer expired, should reset

        // Assert
        didReset.Should().BeTrue();
        button.State.Should().Be(ObjectState.Inactive);
        button.HasPendingReset.Should().BeFalse();
    }

    [Test]
    public void ProcessTurnTick_WhenNoReset_ReturnsFalse()
    {
        // Arrange
        var lever = CreateLever();
        lever.Activate();

        // Act
        var result = lever.ProcessTurnTick();

        // Assert
        result.Should().BeFalse();
        lever.State.Should().Be(ObjectState.Active);
    }

    // ===== Helper Methods =====

    private static InteractiveObject CreateLever()
    {
        return InteractiveObject.Create(
            definitionId: "test-lever",
            name: "Test Lever",
            description: "A test lever",
            objectType: InteractiveObjectType.Lever,
            defaultState: ObjectState.Inactive,
            allowedInteractions: new[] { InteractionType.Activate, InteractionType.Deactivate });
    }

    private static InteractiveObject CreateButton()
    {
        return InteractiveObject.Create(
            definitionId: "test-button",
            name: "Test Button",
            description: "A test button",
            objectType: InteractiveObjectType.Button,
            defaultState: ObjectState.Inactive,
            allowedInteractions: new[] { InteractionType.Activate });
    }
}
