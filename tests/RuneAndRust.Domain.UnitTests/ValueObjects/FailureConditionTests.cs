using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for FailureCondition value object.
/// </summary>
[TestFixture]
public class FailureConditionTests
{
    [Test]
    public void TimeExpired_CreatesCorrectCondition()
    {
        // Act
        var condition = FailureCondition.TimeExpired();

        // Assert
        condition.Type.Should().Be(FailureType.TimeExpired);
        condition.Message.Should().Be("Time has run out.");
    }

    [Test]
    public void NPCDied_CreatesConditionWithTargetId()
    {
        // Act
        var condition = FailureCondition.NPCDied("npc-merchant");

        // Assert
        condition.Type.Should().Be(FailureType.NPCDied);
        condition.TargetId.Should().Be("npc-merchant");
    }

    [Test]
    public void ItemLost_CreatesConditionWithTargetId()
    {
        // Act
        var condition = FailureCondition.ItemLost("quest-artifact");

        // Assert
        condition.Type.Should().Be(FailureType.ItemLost);
        condition.TargetId.Should().Be("quest-artifact");
    }

    [Test]
    public void ReputationDropped_CreatesConditionWithThreshold()
    {
        // Act
        var condition = FailureCondition.ReputationDropped("guild-thieves", -50);

        // Assert
        condition.Type.Should().Be(FailureType.ReputationDropped);
        condition.TargetId.Should().Be("guild-thieves");
        condition.Threshold.Should().Be(-50);
    }

    [Test]
    public void LeftArea_CreatesConditionWithAreaId()
    {
        // Act
        var condition = FailureCondition.LeftArea("dungeon-level-3");

        // Assert
        condition.Type.Should().Be(FailureType.LeftArea);
        condition.TargetId.Should().Be("dungeon-level-3");
    }

    [Test]
    public void CustomMessage_OverridesDefault()
    {
        // Act
        var condition = FailureCondition.NPCDied("npc-1", "The merchant was slain!");

        // Assert
        condition.Message.Should().Be("The merchant was slain!");
    }
}
