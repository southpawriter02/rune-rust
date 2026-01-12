using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for InteractionResult value object.
/// </summary>
[TestFixture]
public class InteractionResultTests
{
    [Test]
    public void Succeeded_SetsSuccessTrue()
    {
        // Act
        var result = InteractionResult.Succeeded("Action done", InteractionType.Use);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Action done");
    }

    [Test]
    public void Failed_SetsSuccessFalse()
    {
        // Act
        var result = InteractionResult.Failed("Can't do that", InteractionType.Open);

        // Assert
        result.Success.Should().BeFalse();
        result.StateChanged.Should().BeFalse();
    }

    [Test]
    public void Opened_SetsNewStateToOpen()
    {
        // Act
        var result = InteractionResult.Opened("Chest");

        // Assert
        result.Success.Should().BeTrue();
        result.StateChanged.Should().BeTrue();
        result.NewState.Should().Be(ObjectState.Open);
    }

    [Test]
    public void Closed_SetsNewStateToClosed()
    {
        // Act
        var result = InteractionResult.Closed("Door");

        // Assert
        result.NewState.Should().Be(ObjectState.Closed);
    }
}
