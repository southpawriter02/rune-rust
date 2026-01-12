using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Quest abandonment functionality.
/// </summary>
[TestFixture]
public class QuestAbandonmentTests
{
    [Test]
    public void Abandon_SetsStatusToAvailable()
    {
        // Arrange
        var quest = CreateActiveQuest();
        quest.SetCategory(QuestCategory.Side);

        // Act
        quest.Abandon();

        // Assert
        quest.Status.Should().Be(QuestStatus.Available);
    }

    [Test]
    public void Abandon_ResetsObjectiveProgress()
    {
        // Arrange
        var quest = CreateActiveQuestWithProgress();
        quest.SetCategory(QuestCategory.Side);

        // Act
        quest.Abandon();

        // Assert
        quest.Objectives.Should().AllSatisfy(o =>
            o.CurrentProgress.Should().Be(0));
    }

    [Test]
    public void Abandon_ThrowsForMainQuests()
    {
        // Arrange
        var quest = CreateActiveQuest();
        quest.SetCategory(QuestCategory.Main);

        // Act
        var act = () => quest.Abandon();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*main*");
    }

    [Test]
    public void Abandon_ThrowsForNonActiveQuests()
    {
        // Arrange
        var quest = new Quest("test-quest", "Test Quest", "Description");
        quest.SetCategory(QuestCategory.Side);
        // Quest is in Available status, not Active

        // Act
        var act = () => quest.Abandon();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*active*");
    }

    [Test]
    public void Abandon_ResetsTimedQuestTimer()
    {
        // Arrange
        var quest = CreateActiveTimedQuest(timeLimit: 20);
        quest.SetCategory(QuestCategory.Side);
        quest.TickTime(15); // Reduce to 5 turns

        // Act
        quest.Abandon();

        // Assert
        quest.TurnsRemaining.Should().Be(20);
    }

    private static Quest CreateActiveQuest()
    {
        var quest = new Quest("test-quest", "Test Quest", "A test description");
        quest.AddObjective(new QuestObjective("Kill 5 enemies", 5));
        quest.Activate();
        return quest;
    }

    private static Quest CreateActiveQuestWithProgress()
    {
        var quest = new Quest("test-quest", "Test Quest", "A test description");
        var objective = new QuestObjective("Kill 5 enemies", 5);
        objective.AdvanceProgress(3);
        quest.AddObjective(objective);
        quest.Activate();
        return quest;
    }

    private static Quest CreateActiveTimedQuest(int timeLimit)
    {
        var quest = new Quest("test-quest", "Test Quest", "A test description");
        quest.AddObjective(new QuestObjective("Complete task", 1));
        quest.SetTimeLimit(timeLimit);
        quest.Activate();
        return quest;
    }
}
