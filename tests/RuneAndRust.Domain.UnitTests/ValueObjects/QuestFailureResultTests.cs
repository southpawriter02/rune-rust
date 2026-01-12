using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for QuestFailureResult value object.
/// </summary>
[TestFixture]
public class QuestFailureResultTests
{
    [Test]
    public void Create_SetsAllProperties()
    {
        // Act
        var result = QuestFailureResult.Create(
            "quest-1", "Lost Artifact", "You failed!", FailureType.NPCDied);

        // Assert
        result.QuestId.Should().Be("quest-1");
        result.QuestName.Should().Be("Lost Artifact");
        result.Reason.Should().Be("You failed!");
        result.ConditionType.Should().Be(FailureType.NPCDied);
        result.FailedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void TimeExpired_CreatesTimeExpiredResult()
    {
        // Act
        var result = QuestFailureResult.TimeExpired("quest-1", "Timed Quest");

        // Assert
        result.ConditionType.Should().Be(FailureType.TimeExpired);
        result.Reason.Should().Be("Time has run out.");
    }
}
