using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Tests for SlotType enum.
/// </summary>
[TestFixture]
public class SlotTypeTests
{
    [Test]
    public void SlotType_HasAllExpectedValues()
    {
        // Assert
        Enum.GetNames<SlotType>().Should().HaveCount(7);
        Enum.IsDefined(SlotType.Monster).Should().BeTrue();
        Enum.IsDefined(SlotType.Item).Should().BeTrue();
        Enum.IsDefined(SlotType.Feature).Should().BeTrue();
        Enum.IsDefined(SlotType.Exit).Should().BeTrue();
        Enum.IsDefined(SlotType.Description).Should().BeTrue();
        Enum.IsDefined(SlotType.Hazard).Should().BeTrue();
        Enum.IsDefined(SlotType.Container).Should().BeTrue();
    }

    [Test]
    public void SlotType_ValuesAreSequential()
    {
        // Assert - Values should be 0-6
        ((int)SlotType.Monster).Should().Be(0);
        ((int)SlotType.Item).Should().Be(1);
        ((int)SlotType.Feature).Should().Be(2);
        ((int)SlotType.Exit).Should().Be(3);
        ((int)SlotType.Description).Should().Be(4);
        ((int)SlotType.Hazard).Should().Be(5);
        ((int)SlotType.Container).Should().Be(6);
    }
}
