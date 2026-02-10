using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="TrapTriggerResult"/> value object.
/// Tests factory methods and default values.
/// </summary>
[TestFixture]
public class TrapTriggerResultTests
{
    private static readonly Guid TestTrapId = Guid.NewGuid();
    private static readonly Guid TestTargetId = Guid.NewGuid();

    [Test]
    public void Triggered_FactoryMethod_SetsCorrectValues()
    {
        // Arrange & Act
        var result = TrapTriggerResult.Triggered(TestTrapId, TestTargetId, 15);

        // Assert
        result.Success.Should().BeTrue();
        result.DamageDealt.Should().Be(15);
        result.TargetId.Should().Be(TestTargetId);
        result.TrapId.Should().Be(TestTrapId);
        result.Message.Should().Contain("15");
    }

    [Test]
    public void Failed_FactoryMethod_SetsCorrectValues()
    {
        // Arrange & Act
        var result = TrapTriggerResult.Failed("Trap already triggered");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Trap already triggered");
        result.DamageDealt.Should().Be(0);
        result.TargetId.Should().BeNull();
        result.TrapId.Should().BeNull();
    }

    [Test]
    public void DefaultDamage_IsZero()
    {
        // Arrange & Act
        var result = new TrapTriggerResult { Success = false, Message = "test" };

        // Assert
        result.DamageDealt.Should().Be(0);
    }
}
