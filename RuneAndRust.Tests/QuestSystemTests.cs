using RuneAndRust.Core;
using RuneAndRust.Core.Quests;
using RuneAndRust.Engine;
using Xunit;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.14: Quest System unit tests
/// Tests quest state machine, objective tracking, and dynamic generation
/// </summary>
public class QuestSystemTests
{
    private PlayerCharacter CreateTestPlayer()
    {
        return new PlayerCharacter
        {
            Name = "Test Player",
            CurrentLegend = 100,
            ActiveQuests = new List<Quest>(),
            CompletedQuests = new List<Quest>()
        };
    }

    private Quest CreateTestQuest(string id = "test_quest")
    {
        return new Quest
        {
            Id = id,
            Title = "Test Quest",
            Description = "A test quest",
            GiverNpcId = "test_npc",
            GiverNpcName = "Test NPC",
            Type = QuestType.Side,
            Category = QuestCategory.Combat,
            Status = QuestStatus.NotStarted,
            MinimumLegend = 0,
            Objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    Description = "Kill 5 enemies",
                    Type = ObjectiveType.KillEnemy,
                    TargetId = "CorruptedServitor",
                    Required = 5,
                    Current = 0
                }
            },
            Reward = new QuestReward
            {
                Experience = 100,
                Currency = 50
            }
        };
    }

    [Fact]
    public void OfferQuest_ShouldSucceed_WhenPlayerMeetsRequirements()
    {
        // Arrange
        var questService = new QuestService();
        var player = CreateTestPlayer();
        var quest = CreateTestQuest();

        // Act
        bool canOffer = questService.CanOfferQuest(player, quest);

        // Assert
        Assert.True(canOffer);
    }

    [Fact]
    public void OfferQuest_ShouldFail_WhenPlayerLegendTooLow()
    {
        // Arrange
        var questService = new QuestService();
        var player = CreateTestPlayer();
        player.CurrentLegend = 50;

        var quest = CreateTestQuest();
        quest.MinimumLegend = 100;

        // Act
        bool canOffer = questService.CanOfferQuest(player, quest);

        // Assert
        Assert.False(canOffer);
    }

    [Fact]
    public void OfferQuest_ShouldFail_WhenPrerequisiteNotComplete()
    {
        // Arrange
        var questService = new QuestService();
        var player = CreateTestPlayer();

        var quest = CreateTestQuest();
        quest.PrerequisiteQuests.Add("required_quest");

        // Act
        bool canOffer = questService.CanOfferQuest(player, quest);

        // Assert
        Assert.False(canOffer);
    }

    [Fact]
    public void AcceptQuest_ShouldChangeStatus_ToActive()
    {
        // Arrange
        var questService = new QuestService();
        var player = CreateTestPlayer();
        var quest = CreateTestQuest();
        quest.Status = QuestStatus.Available;
        player.ActiveQuests.Add(quest);

        // Act
        questService.AcceptQuest(quest.Id, player);

        // Assert
        Assert.Equal(QuestStatus.Active, quest.Status);
        Assert.NotNull(quest.AcceptedAt);
    }

    [Fact]
    public void OnEnemyKilled_ShouldUpdateKillObjective()
    {
        // Arrange
        var questService = new QuestService();
        var player = CreateTestPlayer();
        var quest = CreateTestQuest();
        quest.Status = QuestStatus.Active;
        player.ActiveQuests.Add(quest);

        // Act
        var messages = questService.OnEnemyKilled("CorruptedServitor", player);

        // Assert
        var objective = quest.Objectives.First();
        Assert.Equal(1, objective.Current);
        Assert.NotEmpty(messages);
    }

    [Fact]
    public void UpdateQuestProgress_ShouldMarkComplete_WhenAllObjectivesDone()
    {
        // Arrange
        var questService = new QuestService();
        var player = CreateTestPlayer();
        var quest = CreateTestQuest();
        quest.Status = QuestStatus.Active;
        quest.Objectives.First().Current = 5; // Complete all kills
        player.ActiveQuests.Add(quest);

        // Act
        var messages = questService.UpdateQuestProgress(player);

        // Assert
        Assert.Equal(QuestStatus.Complete, quest.Status);
        Assert.NotEmpty(messages);
    }

    [Fact]
    public void TurnInQuest_ShouldGrantRewards_AndMoveToCompleted()
    {
        // Arrange
        var questService = new QuestService(currencyService: new CurrencyService());
        var player = CreateTestPlayer();
        player.Currency = 0;
        var initialLegend = player.CurrentLegend;

        var quest = CreateTestQuest();
        quest.Status = QuestStatus.Complete;
        player.ActiveQuests.Add(quest);

        // Act
        var messages = questService.TurnInQuest(quest.Id, player);

        // Assert
        Assert.Equal(QuestStatus.TurnedIn, quest.Status);
        Assert.NotNull(quest.CompletedAt);
        Assert.Contains(quest, player.CompletedQuests);
        Assert.DoesNotContain(quest, player.ActiveQuests);
        Assert.Equal(initialLegend + 100, player.CurrentLegend);
        Assert.Equal(50, player.Currency);
        Assert.NotEmpty(messages);
    }

    [Fact]
    public void GetActiveQuests_ShouldReturnOnlyActiveQuests()
    {
        // Arrange
        var questService = new QuestService();
        var player = CreateTestPlayer();

        var activeQuest = CreateTestQuest("active_quest");
        activeQuest.Status = QuestStatus.Active;

        var availableQuest = CreateTestQuest("available_quest");
        availableQuest.Status = QuestStatus.Available;

        player.ActiveQuests.Add(activeQuest);
        player.ActiveQuests.Add(availableQuest);

        // Act
        var activeQuests = questService.GetActiveQuests(player);

        // Assert
        Assert.Single(activeQuests);
        Assert.Equal("active_quest", activeQuests[0].Id);
    }

    [Fact]
    public void DynamicQuestGenerator_ShouldCreateClearSectorQuest()
    {
        // Arrange
        var generator = new DynamicQuestGenerator();
        var dungeon = CreateTestDungeon();

        // Act
        var quest = generator.GenerateClearSectorQuest(dungeon, dungeonSeed: 12345);

        // Assert
        Assert.Equal(QuestType.Dynamic, quest.Type);
        Assert.Equal(QuestCategory.Combat, quest.Category);
        Assert.NotEmpty(quest.Objectives);
        Assert.NotNull(quest.Reward);
        Assert.True(quest.Reward.Experience > 0);
    }

    [Fact]
    public void DynamicQuestGenerator_ShouldScaleRewards_ByDungeonSize()
    {
        // Arrange
        var generator = new DynamicQuestGenerator();
        var smallDungeon = CreateTestDungeon(roomCount: 3);
        var largeDungeon = CreateTestDungeon(roomCount: 10);

        // Act
        var smallQuest = generator.GenerateClearSectorQuest(smallDungeon, dungeonSeed: 1);
        var largeQuest = generator.GenerateClearSectorQuest(largeDungeon, dungeonSeed: 2);

        // Assert
        Assert.True(largeQuest.Reward.Experience > smallQuest.Reward.Experience);
        Assert.True(largeQuest.Reward.Currency > smallQuest.Reward.Currency);
    }

    private DungeonGraph CreateTestDungeon(int roomCount = 5)
    {
        var dungeon = new DungeonGraph();

        for (int i = 0; i < roomCount; i++)
        {
            var node = new DungeonNode
            {
                Id = i,
                Name = $"Test Room {i}",
                Type = NodeType.Main,
                Depth = i,
                Template = new RoomTemplate
                {
                    TemplateId = $"test_room_{i}",
                    // Name = $"Test Room {i}", // RoomTemplate doesn't have Name property
                    Archetype = RoomArchetype.Chamber
                }
            };
            dungeon.AddNode(node);
        }

        return dungeon;
    }
}
