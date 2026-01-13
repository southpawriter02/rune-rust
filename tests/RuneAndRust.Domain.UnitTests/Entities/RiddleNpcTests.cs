using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for the RiddleNpc entity.
/// </summary>
[TestFixture]
public class RiddleNpcTests
{
    [Test]
    public void Create_WithValidParameters_CreatesInstance()
    {
        // Act
        var npc = RiddleNpc.Create("Sphinx", "A majestic creature", "riddle-1", "Greetings!", "Well done");

        // Assert
        npc.Name.Should().Be("Sphinx");
        npc.CurrentRiddleId.Should().Be("riddle-1");
        npc.RiddleSolved.Should().BeFalse();
    }

    [Test]
    public void RecordWrongAnswer_IncrementsCount()
    {
        // Arrange
        var npc = RiddleNpc.Create("Sphinx", "", "riddle-1", "", "", maxWrongAnswers: 3);

        // Act
        npc.RecordWrongAnswer();

        // Assert
        npc.WrongAnswerCount.Should().Be(1);
        npc.GetRemainingAttempts().Should().Be(2);
    }

    [Test]
    public void RecordWrongAnswer_AtMax_ReturnsTrue()
    {
        // Arrange
        var npc = RiddleNpc.Create("Sphinx", "", "riddle-1", "", "", maxWrongAnswers: 2);
        npc.RecordWrongAnswer();

        // Act
        var reachedMax = npc.RecordWrongAnswer();

        // Assert
        reachedMax.Should().BeTrue();
        npc.HasReachedMaxFailures.Should().BeTrue();
    }

    [Test]
    public void MarkSolved_SetsRiddleSolvedTrue()
    {
        // Arrange
        var npc = RiddleNpc.Create("Sphinx", "", "riddle-1", "", "");

        // Act
        npc.MarkSolved();

        // Assert
        npc.RiddleSolved.Should().BeTrue();
    }

    [Test]
    public void IsPassageBlocked_WhenBlockingAndUnsolved_ReturnsTrue()
    {
        // Arrange
        var npc = RiddleNpc.Create("Sphinx", "", "riddle-1", "", "",
            blocksPassage: true, blockedDirection: Direction.North);

        // Act & Assert
        npc.IsPassageBlocked(Direction.North).Should().BeTrue();
        npc.IsPassageBlocked(Direction.South).Should().BeFalse();
    }

    [Test]
    public void IsPassageBlocked_WhenSolved_ReturnsFalse()
    {
        // Arrange
        var npc = RiddleNpc.Create("Sphinx", "", "riddle-1", "", "",
            blocksPassage: true, blockedDirection: Direction.North);
        npc.MarkSolved();

        // Act & Assert
        npc.IsPassageBlocked(Direction.North).Should().BeFalse();
    }
}
