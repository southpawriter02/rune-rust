using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the ObjectEffect value object.
/// </summary>
[TestFixture]
public class ObjectEffectTests
{
    // ===== Factory Method Tests =====

    [Test]
    public void OpenTarget_CreatesCorrectEffect()
    {
        // Arrange & Act
        var effect = ObjectEffect.OpenTarget("iron-gate", ObjectState.Active, "The gate swings open.");

        // Assert
        effect.Type.Should().Be(EffectType.OpenTarget);
        effect.TargetObjectId.Should().Be("iron-gate");
        effect.TriggerOnState.Should().Be(ObjectState.Active);
        effect.EffectMessage.Should().Be("The gate swings open.");
        effect.DelayTurns.Should().Be(0);
        effect.IsImmediate.Should().BeTrue();
    }

    [Test]
    public void CloseTarget_CreatesCorrectEffect()
    {
        // Arrange & Act
        var effect = ObjectEffect.CloseTarget("secret-door", ObjectState.Inactive);

        // Assert
        effect.Type.Should().Be(EffectType.CloseTarget);
        effect.TargetObjectId.Should().Be("secret-door");
        effect.TriggerOnState.Should().Be(ObjectState.Inactive);
        effect.IsImmediate.Should().BeTrue();
    }

    [Test]
    public void UnlockTarget_CreatesCorrectEffect()
    {
        // Arrange & Act
        var effect = ObjectEffect.UnlockTarget("Chest-Lock", ObjectState.Active);

        // Assert
        effect.Type.Should().Be(EffectType.UnlockTarget);
        effect.TargetObjectId.Should().Be("chest-lock"); // normalized to lowercase
    }

    [Test]
    public void LockTarget_CreatesCorrectEffect()
    {
        // Arrange & Act
        var effect = ObjectEffect.LockTarget("cell-door", ObjectState.Inactive);

        // Assert
        effect.Type.Should().Be(EffectType.LockTarget);
        effect.TargetObjectId.Should().Be("cell-door");
    }

    [Test]
    public void ToggleTarget_CreatesCorrectEffect()
    {
        // Arrange & Act
        var effect = ObjectEffect.ToggleTarget("pressure-plate", ObjectState.Active);

        // Assert
        effect.Type.Should().Be(EffectType.ToggleTarget);
    }

    [Test]
    public void ActivateTarget_CreatesCorrectEffect()
    {
        // Arrange & Act
        var effect = ObjectEffect.ActivateTarget("trap-mechanism", ObjectState.Active);

        // Assert
        effect.Type.Should().Be(EffectType.ActivateTarget);
    }

    [Test]
    public void DeactivateTarget_CreatesCorrectEffect()
    {
        // Arrange & Act
        var effect = ObjectEffect.DeactivateTarget("trap-mechanism", ObjectState.Inactive);

        // Assert
        effect.Type.Should().Be(EffectType.DeactivateTarget);
    }

    [Test]
    public void DestroyTarget_CreatesCorrectEffect()
    {
        // Arrange & Act
        var effect = ObjectEffect.DestroyTarget("support-beam", ObjectState.Active, "The beam crumbles!");

        // Assert
        effect.Type.Should().Be(EffectType.DestroyTarget);
        effect.EffectMessage.Should().Be("The beam crumbles!");
    }

    [Test]
    public void RevealTarget_CreatesCorrectEffect()
    {
        // Arrange & Act
        var effect = ObjectEffect.RevealTarget("hidden-passage", ObjectState.Active);

        // Assert
        effect.Type.Should().Be(EffectType.RevealTarget);
    }

    [Test]
    public void MessageOnly_CreatesEffectWithNoTarget()
    {
        // Arrange & Act
        var effect = ObjectEffect.MessageOnly("You hear a distant rumbling.", ObjectState.Active);

        // Assert
        effect.Type.Should().Be(EffectType.Message);
        effect.TargetObjectId.Should().BeEmpty();
        effect.HasTarget.Should().BeFalse();
        effect.EffectMessage.Should().Be("You hear a distant rumbling.");
    }

    // ===== Delay Tests =====

    [Test]
    public void Delayed_CreatesDelayedEffect()
    {
        // Arrange
        var baseEffect = ObjectEffect.OpenTarget("portcullis", ObjectState.Active);

        // Act
        var delayedEffect = ObjectEffect.Delayed(baseEffect, 3);

        // Assert
        delayedEffect.DelayTurns.Should().Be(3);
        delayedEffect.IsImmediate.Should().BeFalse();
        delayedEffect.Type.Should().Be(EffectType.OpenTarget);
    }

    [Test]
    public void Delayed_WithZeroDelay_IsImmediate()
    {
        // Arrange
        var baseEffect = ObjectEffect.CloseTarget("door", ObjectState.Inactive);

        // Act
        var effect = ObjectEffect.Delayed(baseEffect, 0);

        // Assert
        effect.DelayTurns.Should().Be(0);
        effect.IsImmediate.Should().BeTrue();
    }

    [Test]
    public void Delayed_WithNegativeDelay_ThrowsException()
    {
        // Arrange
        var baseEffect = ObjectEffect.OpenTarget("door", ObjectState.Active);

        // Act
        var act = () => ObjectEffect.Delayed(baseEffect, -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ===== Property Tests =====

    [Test]
    public void HasTarget_WithEmptyTarget_ReturnsFalse()
    {
        // Arrange
        var effect = ObjectEffect.MessageOnly("Message", ObjectState.Active);

        // Assert
        effect.HasTarget.Should().BeFalse();
    }

    [Test]
    public void HasTarget_WithValidTarget_ReturnsTrue()
    {
        // Arrange
        var effect = ObjectEffect.OpenTarget("door", ObjectState.Active);

        // Assert
        effect.HasTarget.Should().BeTrue();
    }

    [Test]
    public void ToString_ReturnsDescriptiveString()
    {
        // Arrange
        var effect = ObjectEffect.UnlockTarget("chest", ObjectState.Active);

        // Act
        var result = effect.ToString();

        // Assert
        result.Should().Contain("UnlockTarget");
        result.Should().Contain("chest");
        result.Should().Contain("Active");
    }

    [Test]
    public void ToString_WithDelay_IncludesDelayInfo()
    {
        // Arrange
        var effect = ObjectEffect.Delayed(ObjectEffect.OpenTarget("gate", ObjectState.Active), 2);

        // Act
        var result = effect.ToString();

        // Assert
        result.Should().Contain("delayed");
        result.Should().Contain("2");
    }

    [Test]
    public void TargetId_NormalizesToLowercase()
    {
        // Arrange & Act
        var effect = ObjectEffect.OpenTarget("My-DOOR-ID", ObjectState.Active);

        // Assert
        effect.TargetObjectId.Should().Be("my-door-id");
    }
}
