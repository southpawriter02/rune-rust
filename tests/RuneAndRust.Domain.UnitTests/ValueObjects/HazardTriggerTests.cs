using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for HazardTrigger value object.
/// </summary>
[TestFixture]
public class HazardTriggerTests
{
    [Test]
    public void OnEnter_CreatesCorrectType()
    {
        // Act
        var trigger = HazardTrigger.OnEnter();

        // Assert
        trigger.Type.Should().Be(HazardTriggerType.OnEnter);
        trigger.Chance.Should().Be(1.0f);
    }

    [Test]
    public void PerTurn_CreatesCorrectType()
    {
        // Act
        var trigger = HazardTrigger.PerTurn(0.5f);

        // Assert
        trigger.Type.Should().Be(HazardTriggerType.PerTurn);
        trigger.Chance.Should().Be(0.5f);
    }

    [Test]
    public void OnEnter_WithAvoidance_SetsAvoidanceProperties()
    {
        // Act
        var trigger = HazardTrigger.OnEnter(avoidanceStat: "athletics", avoidanceDC: 12);

        // Assert
        trigger.AvoidanceStat.Should().Be("athletics");
        trigger.AvoidanceDC.Should().Be(12);
        trigger.CanAvoid.Should().BeTrue();
    }

    [Test]
    public void Ambient_CreatesAlwaysActiveTrigger()
    {
        // Act
        var trigger = HazardTrigger.Ambient();

        // Assert
        trigger.Type.Should().Be(HazardTriggerType.Ambient);
        trigger.Chance.Should().Be(1.0f);
        trigger.CanAvoid.Should().BeFalse();
    }
}
