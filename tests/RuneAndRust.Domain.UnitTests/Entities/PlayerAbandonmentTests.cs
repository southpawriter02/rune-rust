using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Player quest abandonment functionality.
/// </summary>
[TestFixture]
public class PlayerAbandonmentTests
{
    [Test]
    public void AbandonQuest_RemovesFromActiveQuests()
    {
        // Arrange
        var player = new Player("TestHero");
        var quest = CreateActiveQuest("test-quest");
        quest.SetCategory(QuestCategory.Side);
        player.AddQuest(quest);

        // Act
        player.AbandonQuest(quest);

        // Assert
        player.ActiveQuests.Should().NotContain(quest);
    }

    [Test]
    public void AbandonQuest_AddsToAbandonedList()
    {
        // Arrange
        var player = new Player("TestHero");
        var quest = CreateActiveQuest("test-quest");
        quest.SetCategory(QuestCategory.Side);
        player.AddQuest(quest);

        // Act
        player.AbandonQuest(quest);

        // Assert
        player.AbandonedQuestIds.Should().Contain("test-quest");
    }

    [Test]
    public void HasAbandonedQuest_ReturnsTrueForAbandonedQuest()
    {
        // Arrange
        var player = new Player("TestHero");
        var quest = CreateActiveQuest("test-quest");
        quest.SetCategory(QuestCategory.Side);
        player.AddQuest(quest);
        player.AbandonQuest(quest);

        // Act & Assert
        player.HasAbandonedQuest("test-quest").Should().BeTrue();
    }

    [Test]
    public void GetQuestsByCategory_ReturnsCorrectQuests()
    {
        // Arrange
        var player = new Player("TestHero");
        var mainQuest = CreateActiveQuest("main-1");
        mainQuest.SetCategory(QuestCategory.Main);
        var sideQuest = CreateActiveQuest("side-1");
        sideQuest.SetCategory(QuestCategory.Side);
        player.AddQuest(mainQuest);
        player.AddQuest(sideQuest);

        // Act
        var mainQuests = player.GetQuestsByCategory(QuestCategory.Main).ToList();

        // Assert
        mainQuests.Should().ContainSingle();
        mainQuests[0].DefinitionId.Should().Be("main-1");
    }

    private static Quest CreateActiveQuest(string definitionId)
    {
        var quest = new Quest(definitionId, "Test Quest", "A test description");
        quest.AddObjective(new QuestObjective("Complete task", 1));
        quest.Activate();
        return quest;
    }
}
