using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Events;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for QuestEventBus service.
/// Verifies event routing to quest objectives and objective watch registration/management.
/// </summary>
[TestFixture]
public class QuestEventBusTests
{
    private Mock<IQuestService> _mockQuestService = null!;
    private Mock<ILogger<QuestEventBus>> _mockLogger = null!;
    private QuestEventBus _eventBus = null!;

    [SetUp]
    public void SetUp()
    {
        _mockQuestService = new Mock<IQuestService>();
        _mockLogger = new Mock<ILogger<QuestEventBus>>();
        _eventBus = new QuestEventBus(_mockQuestService.Object, _mockLogger.Object);
    }

    #region ProcessEvent Event Routing Tests

    [Test]
    public void ProcessEvent_MonsterDefeated_RoutsToKillEnemy()
    {
        // Arrange
        var monsterId = "monster_001";
        var gameEvent = GameEvent.Combat("MonsterDefeated", "Enemy defeated")
            with { Data = new Dictionary<string, object> { { "MonsterId", monsterId } } };

        _mockQuestService.Setup(qs => qs.AdvanceObjective(QuestObjectiveType.KillEnemy, monsterId, 1))
            .Returns(true);
        _mockQuestService.Setup(qs => qs.GetActiveQuests())
            .Returns(new List<Quest>());

        // Act
        var result = _eventBus.ProcessEvent(gameEvent);

        // Assert
        _mockQuestService.Verify(qs => qs.AdvanceObjective(QuestObjectiveType.KillEnemy, monsterId, 1), Times.Once);
        result.Should().NotBeNull();
    }

    [Test]
    public void ProcessEvent_ItemPickedUp_RoutsToCollectItem()
    {
        // Arrange
        var itemId = "item_golden_ring";
        var gameEvent = GameEvent.Inventory("ItemPickedUp", "Item picked up")
            with { Data = new Dictionary<string, object> { { "ItemId", itemId } } };

        _mockQuestService.Setup(qs => qs.AdvanceObjective(QuestObjectiveType.CollectItem, itemId, 1))
            .Returns(true);
        _mockQuestService.Setup(qs => qs.GetActiveQuests())
            .Returns(new List<Quest>());

        // Act
        var result = _eventBus.ProcessEvent(gameEvent);

        // Assert
        _mockQuestService.Verify(qs => qs.AdvanceObjective(QuestObjectiveType.CollectItem, itemId, 1), Times.Once);
        result.Should().NotBeNull();
    }

    [Test]
    public void ProcessEvent_RoomEntered_RoutsToExploreRoom()
    {
        // Arrange
        var roomId = "room_throne_hall";
        var gameEvent = GameEvent.Exploration("RoomEntered", "Room entered")
            with { Data = new Dictionary<string, object> { { "RoomId", roomId } } };

        _mockQuestService.Setup(qs => qs.AdvanceObjective(QuestObjectiveType.ExploreRoom, roomId, 1))
            .Returns(true);
        _mockQuestService.Setup(qs => qs.GetActiveQuests())
            .Returns(new List<Quest>());

        // Act
        var result = _eventBus.ProcessEvent(gameEvent);

        // Assert
        _mockQuestService.Verify(qs => qs.AdvanceObjective(QuestObjectiveType.ExploreRoom, roomId, 1), Times.Once);
        result.Should().NotBeNull();
    }

    [Test]
    public void ProcessEvent_InteractionCompleted_RoutsToInteractWithObject()
    {
        // Arrange
        var objectId = "obj_ancient_lever";
        var gameEvent = GameEvent.Interaction("InteractionCompleted", "Interaction completed")
            with { Data = new Dictionary<string, object> { { "ObjectId", objectId } } };

        _mockQuestService.Setup(qs => qs.AdvanceObjective(QuestObjectiveType.InteractWithObject, objectId, 1))
            .Returns(true);
        _mockQuestService.Setup(qs => qs.GetActiveQuests())
            .Returns(new List<Quest>());

        // Act
        var result = _eventBus.ProcessEvent(gameEvent);

        // Assert
        _mockQuestService.Verify(qs => qs.AdvanceObjective(QuestObjectiveType.InteractWithObject, objectId, 1), Times.Once);
        result.Should().NotBeNull();
    }

    [Test]
    public void ProcessEvent_DialogueChoiceMade_RoutsToMakeChoice()
    {
        // Arrange
        var choiceId = "choice_accept_bargain";
        var gameEvent = GameEvent.Interaction("DialogueChoiceMade", "Dialogue choice made")
            with { Data = new Dictionary<string, object> { { "ChoiceId", choiceId } } };

        _mockQuestService.Setup(qs => qs.AdvanceObjective(QuestObjectiveType.MakeChoice, choiceId, 1))
            .Returns(true);
        _mockQuestService.Setup(qs => qs.GetActiveQuests())
            .Returns(new List<Quest>());

        // Act
        var result = _eventBus.ProcessEvent(gameEvent);

        // Assert
        _mockQuestService.Verify(qs => qs.AdvanceObjective(QuestObjectiveType.MakeChoice, choiceId, 1), Times.Once);
        result.Should().NotBeNull();
    }

    [Test]
    public void ProcessEvent_DialogueStarted_RoutsToTalkToNpc()
    {
        // Arrange
        var npcId = "npc_merchant_borgrim";
        var gameEvent = GameEvent.Interaction("DialogueStarted", "Dialogue started")
            with { Data = new Dictionary<string, object> { { "NpcId", npcId } } };

        _mockQuestService.Setup(qs => qs.AdvanceObjective(QuestObjectiveType.TalkToNpc, npcId, 1))
            .Returns(true);
        _mockQuestService.Setup(qs => qs.GetActiveQuests())
            .Returns(new List<Quest>());

        // Act
        var result = _eventBus.ProcessEvent(gameEvent);

        // Assert
        _mockQuestService.Verify(qs => qs.AdvanceObjective(QuestObjectiveType.TalkToNpc, npcId, 1), Times.Once);
        result.Should().NotBeNull();
    }

    [Test]
    public void ProcessEvent_LevelUp_RoutsToReachLevel()
    {
        // Arrange
        var level = "5";
        var gameEvent = GameEvent.Character("LevelUp", "Level up achieved")
            with { Data = new Dictionary<string, object> { { "Level", level } } };

        _mockQuestService.Setup(qs => qs.AdvanceObjective(QuestObjectiveType.ReachLevel, level, 1))
            .Returns(true);
        _mockQuestService.Setup(qs => qs.GetActiveQuests())
            .Returns(new List<Quest>());

        // Act
        var result = _eventBus.ProcessEvent(gameEvent);

        // Assert
        _mockQuestService.Verify(qs => qs.AdvanceObjective(QuestObjectiveType.ReachLevel, level, 1), Times.Once);
        result.Should().NotBeNull();
    }

    [Test]
    public void ProcessEvent_UnknownEventType_ReturnsEmptyList()
    {
        // Arrange
        var gameEvent = GameEvent.System("UnknownEvent", "Unknown event")
            with { Data = new Dictionary<string, object> { { "SomeKey", "some_value" } } };

        // Act
        var result = _eventBus.ProcessEvent(gameEvent);

        // Assert
        result.Should().BeEmpty();
        _mockQuestService.Verify(qs => qs.AdvanceObjective(It.IsAny<QuestObjectiveType>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void ProcessEvent_NullData_ReturnsEmptyList()
    {
        // Arrange
        var gameEvent = GameEvent.Combat("MonsterDefeated", "Enemy defeated")
            with { Data = null };

        // Act
        var result = _eventBus.ProcessEvent(gameEvent);

        // Assert
        result.Should().BeEmpty();
        _mockQuestService.Verify(qs => qs.AdvanceObjective(It.IsAny<QuestObjectiveType>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void ProcessEvent_MissingDataKey_ReturnsEmptyList()
    {
        // Arrange
        var gameEvent = GameEvent.Combat("MonsterDefeated", "Enemy defeated")
            with { Data = new Dictionary<string, object> { { "WrongKey", "value" } } };

        // Act
        var result = _eventBus.ProcessEvent(gameEvent);

        // Assert
        result.Should().BeEmpty();
        _mockQuestService.Verify(qs => qs.AdvanceObjective(It.IsAny<QuestObjectiveType>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void ProcessEvent_AdvanceObjectiveFalse_ReturnsEmptyList()
    {
        // Arrange
        var monsterId = "monster_001";
        var gameEvent = GameEvent.Combat("MonsterDefeated", "Enemy defeated")
            with { Data = new Dictionary<string, object> { { "MonsterId", monsterId } } };

        _mockQuestService.Setup(qs => qs.AdvanceObjective(QuestObjectiveType.KillEnemy, monsterId, 1))
            .Returns(false);

        // Act
        var result = _eventBus.ProcessEvent(gameEvent);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void ProcessEvent_ThrowsOnNullGameEvent()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _eventBus.ProcessEvent(null!));
    }

    #endregion

    #region ProcessEvent Result Building Tests

    [Test]
    public void ProcessEvent_AdvanceObjectiveTrue_ReturnsProgressResults()
    {
        // Arrange
        var monsterId = "monster_001";
        var gameEvent = GameEvent.Combat("MonsterDefeated", "Enemy defeated")
            with { Data = new Dictionary<string, object> { { "MonsterId", monsterId } } };

        var questObjective = new QuestObjective("Kill the boss", 1);
        var quest = new Quest("quest_defeat_boss", "Defeat the Boss", "Defeat the fearsome boss monster");
        quest.AddObjective(questObjective);

        _mockQuestService.Setup(qs => qs.AdvanceObjective(QuestObjectiveType.KillEnemy, monsterId, 1))
            .Returns(true);
        _mockQuestService.Setup(qs => qs.GetActiveQuests())
            .Returns(new List<Quest> { quest });

        // Act
        var result = _eventBus.ProcessEvent(gameEvent);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result[0].QuestId.Should().Be(quest.Id);
        result[0].QuestName.Should().Be("Defeat the Boss");
        result[0].ObjectiveDescription.Should().Be("Kill the boss");
    }

    [Test]
    public void ProcessEvent_MultipleActiveQuests_ReturnsAllObjectives()
    {
        // Arrange
        var monsterId = "monster_001";
        var gameEvent = GameEvent.Combat("MonsterDefeated", "Enemy defeated")
            with { Data = new Dictionary<string, object> { { "MonsterId", monsterId } } };

        var questObjective1 = new QuestObjective("Kill any monster", 1);
        var questObjective2 = new QuestObjective("Collect items", 1);
        var quest1 = new Quest("quest_1", "Quest 1", "First quest description");
        quest1.AddObjective(questObjective1);
        var quest2 = new Quest("quest_2", "Quest 2", "Second quest description");
        quest2.AddObjective(questObjective2);

        _mockQuestService.Setup(qs => qs.AdvanceObjective(QuestObjectiveType.KillEnemy, monsterId, 1))
            .Returns(true);
        _mockQuestService.Setup(qs => qs.GetActiveQuests())
            .Returns(new List<Quest> { quest1, quest2 });

        // Act
        var result = _eventBus.ProcessEvent(gameEvent);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.QuestId == quest1.Id);
        result.Should().Contain(r => r.QuestId == quest2.Id);
    }

    [Test]
    public void ProcessEvent_ObjectiveWithMultipleCount_PopulatesProgressData()
    {
        // Arrange
        var monsterId = "monster_001";
        var gameEvent = GameEvent.Combat("MonsterDefeated", "Enemy defeated")
            with { Data = new Dictionary<string, object> { { "MonsterId", monsterId } } };

        var questObjective = new QuestObjective("Kill 3 enemies", 3);
        questObjective.AdvanceProgress(1);
        var quest = new Quest("quest_kill_3", "Kill 3 Enemies", "Defeat three enemies in the dungeon");
        quest.AddObjective(questObjective);

        _mockQuestService.Setup(qs => qs.AdvanceObjective(QuestObjectiveType.KillEnemy, monsterId, 1))
            .Returns(true);
        _mockQuestService.Setup(qs => qs.GetActiveQuests())
            .Returns(new List<Quest> { quest });

        // Act
        var result = _eventBus.ProcessEvent(gameEvent);

        // Assert
        result.Should().HaveCount(1);
        result[0].CurrentProgress.Should().Be(1);
        result[0].RequiredCount.Should().Be(3);
        result[0].ObjectiveCompleted.Should().BeFalse();
    }

    #endregion

    #region RegisterObjective Tests

    [Test]
    public void RegisterObjective_AddsWatch()
    {
        // Arrange
        var questId = Guid.NewGuid();
        var objectiveType = QuestObjectiveType.KillEnemy;
        var targetId = "monster_001";

        // Act
        _eventBus.RegisterObjective(questId, objectiveType, targetId);

        // Assert
        var watches = _eventBus.GetActiveWatches();
        watches.Should().HaveCount(1);
        watches[0].QuestId.Should().Be(questId);
        watches[0].ObjectiveType.Should().Be(objectiveType);
        watches[0].TargetId.Should().Be(targetId);
    }

    [Test]
    public void RegisterObjective_MultipleObjectivesForSameQuest_AddsAllWatches()
    {
        // Arrange
        var questId = Guid.NewGuid();

        // Act
        _eventBus.RegisterObjective(questId, QuestObjectiveType.KillEnemy, "monster_001");
        _eventBus.RegisterObjective(questId, QuestObjectiveType.CollectItem, "item_001");
        _eventBus.RegisterObjective(questId, QuestObjectiveType.ExploreRoom, "room_001");

        // Assert
        var watches = _eventBus.GetActiveWatches();
        watches.Should().HaveCount(3);
        watches.Should().AllSatisfy(w => w.QuestId.Should().Be(questId));
    }

    [Test]
    public void RegisterObjective_MultipleQuestsWithObjectives_AddsAllWatches()
    {
        // Arrange
        var questId1 = Guid.NewGuid();
        var questId2 = Guid.NewGuid();

        // Act
        _eventBus.RegisterObjective(questId1, QuestObjectiveType.KillEnemy, "monster_001");
        _eventBus.RegisterObjective(questId2, QuestObjectiveType.CollectItem, "item_001");

        // Assert
        var watches = _eventBus.GetActiveWatches();
        watches.Should().HaveCount(2);
        watches.Should().Contain(w => w.QuestId == questId1);
        watches.Should().Contain(w => w.QuestId == questId2);
    }

    [Test]
    public void RegisterObjective_ThrowsOnNullOrWhiteSpaceTargetId()
    {
        // Arrange
        var questId = Guid.NewGuid();

        // Act & Assert — empty and whitespace throw ArgumentException
        Assert.Throws<ArgumentException>(() =>
            _eventBus.RegisterObjective(questId, QuestObjectiveType.KillEnemy, ""));
        Assert.Throws<ArgumentException>(() =>
            _eventBus.RegisterObjective(questId, QuestObjectiveType.KillEnemy, "   "));
        // null throws ArgumentNullException (subclass of ArgumentException)
        Assert.Throws<ArgumentNullException>(() =>
            _eventBus.RegisterObjective(questId, QuestObjectiveType.KillEnemy, null!));
    }

    #endregion

    #region RegisterQuestObjectives Tests

    [Test]
    public void RegisterQuestObjectives_AddMultipleWatchesFromDtos()
    {
        // Arrange
        var questId = Guid.NewGuid();
        var objectives = new List<QuestObjectiveDto>
        {
            new() { Description = "Kill enemies", Type = "KillEnemy", TargetId = "monster_001", RequiredCount = 3 },
            new() { Description = "Collect items", Type = "CollectItem", TargetId = "item_001", RequiredCount = 2 },
            new() { Description = "Explore room", Type = "ExploreRoom", TargetId = "room_001", RequiredCount = 1 }
        };

        // Act
        _eventBus.RegisterQuestObjectives(questId, objectives);

        // Assert
        var watches = _eventBus.GetActiveWatches();
        watches.Should().HaveCount(3);
        watches.Should().AllSatisfy(w => w.QuestId.Should().Be(questId));
    }

    [Test]
    public void RegisterQuestObjectives_AllObjectiveTypes_ParsesSuccessfully()
    {
        // Arrange
        var questId = Guid.NewGuid();
        var objectives = new List<QuestObjectiveDto>
        {
            new() { Type = "KillEnemy", TargetId = "enemy_1", RequiredCount = 1 },
            new() { Type = "CollectItem", TargetId = "item_1", RequiredCount = 1 },
            new() { Type = "ExploreRoom", TargetId = "room_1", RequiredCount = 1 },
            new() { Type = "InteractWithObject", TargetId = "obj_1", RequiredCount = 1 },
            new() { Type = "MakeChoice", TargetId = "choice_1", RequiredCount = 1 },
            new() { Type = "TalkToNpc", TargetId = "npc_1", RequiredCount = 1 },
            new() { Type = "ReachLevel", TargetId = "level_5", RequiredCount = 1 }
        };

        // Act
        _eventBus.RegisterQuestObjectives(questId, objectives);

        // Assert
        var watches = _eventBus.GetActiveWatches();
        watches.Should().HaveCount(7);
        watches.Select(w => w.ObjectiveType).Should().Contain(QuestObjectiveType.KillEnemy);
        watches.Select(w => w.ObjectiveType).Should().Contain(QuestObjectiveType.CollectItem);
        watches.Select(w => w.ObjectiveType).Should().Contain(QuestObjectiveType.ExploreRoom);
        watches.Select(w => w.ObjectiveType).Should().Contain(QuestObjectiveType.InteractWithObject);
        watches.Select(w => w.ObjectiveType).Should().Contain(QuestObjectiveType.MakeChoice);
        watches.Select(w => w.ObjectiveType).Should().Contain(QuestObjectiveType.TalkToNpc);
        watches.Select(w => w.ObjectiveType).Should().Contain(QuestObjectiveType.ReachLevel);
    }

    [Test]
    public void RegisterQuestObjectives_UnknownObjectiveType_SkipsAndLogsWarning()
    {
        // Arrange
        var questId = Guid.NewGuid();
        var objectives = new List<QuestObjectiveDto>
        {
            new() { Type = "KillEnemy", TargetId = "monster_001", RequiredCount = 1 },
            new() { Type = "UnknownType", TargetId = "unknown_001", RequiredCount = 1 },
            new() { Type = "CollectItem", TargetId = "item_001", RequiredCount = 1 }
        };

        // Act
        _eventBus.RegisterQuestObjectives(questId, objectives);

        // Assert
        var watches = _eventBus.GetActiveWatches();
        watches.Should().HaveCount(2);
        watches.Should().NotContain(w => w.TargetId == "unknown_001");
    }

    [Test]
    public void RegisterQuestObjectives_CaseInsensitiveTypeMatching()
    {
        // Arrange
        var questId = Guid.NewGuid();
        var objectives = new List<QuestObjectiveDto>
        {
            new() { Type = "killEnemy", TargetId = "monster_001", RequiredCount = 1 },
            new() { Type = "COLLECTITEM", TargetId = "item_001", RequiredCount = 1 },
            new() { Type = "explorerOOM", TargetId = "room_001", RequiredCount = 1 }
        };

        // Act
        _eventBus.RegisterQuestObjectives(questId, objectives);

        // Assert
        var watches = _eventBus.GetActiveWatches();
        watches.Should().HaveCount(3);
        watches.Select(w => w.ObjectiveType).Should().Contain(QuestObjectiveType.KillEnemy);
        watches.Select(w => w.ObjectiveType).Should().Contain(QuestObjectiveType.CollectItem);
        watches.Select(w => w.ObjectiveType).Should().Contain(QuestObjectiveType.ExploreRoom);
    }

    [Test]
    public void RegisterQuestObjectives_ThrowsOnNullObjectives()
    {
        // Arrange
        var questId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _eventBus.RegisterQuestObjectives(questId, null!));
    }

    [Test]
    public void RegisterQuestObjectives_EmptyList_NoWatchesAdded()
    {
        // Arrange
        var questId = Guid.NewGuid();
        var objectives = new List<QuestObjectiveDto>();

        // Act
        _eventBus.RegisterQuestObjectives(questId, objectives);

        // Assert
        var watches = _eventBus.GetActiveWatches();
        watches.Should().BeEmpty();
    }

    #endregion

    #region UnregisterQuest Tests

    [Test]
    public void UnregisterQuest_RemovesAllWatchesForQuest()
    {
        // Arrange
        var questId = Guid.NewGuid();
        _eventBus.RegisterObjective(questId, QuestObjectiveType.KillEnemy, "monster_001");
        _eventBus.RegisterObjective(questId, QuestObjectiveType.CollectItem, "item_001");

        // Act
        _eventBus.UnregisterQuest(questId);

        // Assert
        var watches = _eventBus.GetActiveWatches();
        watches.Should().BeEmpty();
    }

    [Test]
    public void UnregisterQuest_OnlyRemovesSpecificQuest()
    {
        // Arrange
        var questId1 = Guid.NewGuid();
        var questId2 = Guid.NewGuid();
        _eventBus.RegisterObjective(questId1, QuestObjectiveType.KillEnemy, "monster_001");
        _eventBus.RegisterObjective(questId2, QuestObjectiveType.CollectItem, "item_001");

        // Act
        _eventBus.UnregisterQuest(questId1);

        // Assert
        var watches = _eventBus.GetActiveWatches();
        watches.Should().HaveCount(1);
        watches[0].QuestId.Should().Be(questId2);
    }

    [Test]
    public void UnregisterQuest_NonexistentQuestId_DoesNotThrow()
    {
        // Arrange
        var questId = Guid.NewGuid();

        // Act & Assert
        FluentActions.Invoking(() => _eventBus.UnregisterQuest(questId))
            .Should().NotThrow();
    }

    [Test]
    public void UnregisterQuest_MultipleTimesForSameId_SafelyHandled()
    {
        // Arrange
        var questId = Guid.NewGuid();
        _eventBus.RegisterObjective(questId, QuestObjectiveType.KillEnemy, "monster_001");

        // Act & Assert
        _eventBus.UnregisterQuest(questId);
        FluentActions.Invoking(() => _eventBus.UnregisterQuest(questId))
            .Should().NotThrow();
        var watches = _eventBus.GetActiveWatches();
        watches.Should().BeEmpty();
    }

    #endregion

    #region GetActiveWatches Tests

    [Test]
    public void GetActiveWatches_EmptyBus_ReturnsEmptyList()
    {
        // Act
        var watches = _eventBus.GetActiveWatches();

        // Assert
        watches.Should().NotBeNull();
        watches.Should().BeEmpty();
    }

    [Test]
    public void GetActiveWatches_SingleQuest_ReturnsSingleWatch()
    {
        // Arrange
        var questId = Guid.NewGuid();
        _eventBus.RegisterObjective(questId, QuestObjectiveType.KillEnemy, "monster_001");

        // Act
        var watches = _eventBus.GetActiveWatches();

        // Assert
        watches.Should().HaveCount(1);
        watches[0].QuestId.Should().Be(questId);
    }

    [Test]
    public void GetActiveWatches_MultipleQuests_ReturnsAllWatches()
    {
        // Arrange
        var questId1 = Guid.NewGuid();
        var questId2 = Guid.NewGuid();
        var questId3 = Guid.NewGuid();

        _eventBus.RegisterObjective(questId1, QuestObjectiveType.KillEnemy, "monster_001");
        _eventBus.RegisterObjective(questId1, QuestObjectiveType.CollectItem, "item_001");
        _eventBus.RegisterObjective(questId2, QuestObjectiveType.ExploreRoom, "room_001");
        _eventBus.RegisterObjective(questId3, QuestObjectiveType.TalkToNpc, "npc_001");

        // Act
        var watches = _eventBus.GetActiveWatches();

        // Assert
        watches.Should().HaveCount(4);
        watches.Should().Contain(w => w.QuestId == questId1 && w.ObjectiveType == QuestObjectiveType.KillEnemy);
        watches.Should().Contain(w => w.QuestId == questId1 && w.ObjectiveType == QuestObjectiveType.CollectItem);
        watches.Should().Contain(w => w.QuestId == questId2 && w.ObjectiveType == QuestObjectiveType.ExploreRoom);
        watches.Should().Contain(w => w.QuestId == questId3 && w.ObjectiveType == QuestObjectiveType.TalkToNpc);
    }

    [Test]
    public void GetActiveWatches_ReturnsReadOnlyList()
    {
        // Arrange
        var questId = Guid.NewGuid();
        _eventBus.RegisterObjective(questId, QuestObjectiveType.KillEnemy, "monster_001");

        // Act
        var watches = _eventBus.GetActiveWatches();

        // Assert
        watches.Should().BeAssignableTo<IReadOnlyList<ObjectiveWatch>>();
    }

    #endregion

    #region Integration Tests

    [Test]
    public void FullWorkflow_RegisterQuestObjectives_ProcessEvent_UnregisterQuest()
    {
        // Arrange
        var questId = Guid.NewGuid();
        var monsterId = "monster_boss";
        var objectives = new List<QuestObjectiveDto>
        {
            new() { Type = "KillEnemy", TargetId = monsterId, RequiredCount = 1 }
        };

        _eventBus.RegisterQuestObjectives(questId, objectives);

        var gameEvent = GameEvent.Combat("MonsterDefeated", "Boss defeated")
            with { Data = new Dictionary<string, object> { { "MonsterId", monsterId } } };

        _mockQuestService.Setup(qs => qs.AdvanceObjective(QuestObjectiveType.KillEnemy, monsterId, 1))
            .Returns(true);
        _mockQuestService.Setup(qs => qs.GetActiveQuests())
            .Returns(new List<Quest>());

        // Act - Verify registration
        var watchesBeforeEvent = _eventBus.GetActiveWatches();
        watchesBeforeEvent.Should().HaveCount(1);

        // Act - Process event
        var result = _eventBus.ProcessEvent(gameEvent);

        // Assert
        result.Should().NotBeNull();
        _mockQuestService.Verify(qs => qs.AdvanceObjective(QuestObjectiveType.KillEnemy, monsterId, 1), Times.Once);

        // Act - Unregister
        _eventBus.UnregisterQuest(questId);
        var watchesAfterUnregister = _eventBus.GetActiveWatches();

        // Assert
        watchesAfterUnregister.Should().BeEmpty();
    }

    [Test]
    public void MultipleQuests_IndependentProcessing()
    {
        // Arrange
        var questId1 = Guid.NewGuid();
        var questId2 = Guid.NewGuid();

        _eventBus.RegisterObjective(questId1, QuestObjectiveType.KillEnemy, "monster_001");
        _eventBus.RegisterObjective(questId2, QuestObjectiveType.CollectItem, "item_001");

        var event1 = GameEvent.Combat("MonsterDefeated", "Enemy defeated")
            with { Data = new Dictionary<string, object> { { "MonsterId", "monster_001" } } };

        _mockQuestService.Setup(qs => qs.AdvanceObjective(QuestObjectiveType.KillEnemy, "monster_001", 1))
            .Returns(true);
        _mockQuestService.Setup(qs => qs.GetActiveQuests())
            .Returns(new List<Quest>());

        // Act
        var result = _eventBus.ProcessEvent(event1);

        // Assert
        _mockQuestService.Verify(qs => qs.AdvanceObjective(QuestObjectiveType.KillEnemy, "monster_001", 1), Times.Once);
        _mockQuestService.Verify(qs => qs.AdvanceObjective(QuestObjectiveType.CollectItem, "item_001", It.IsAny<int>()), Times.Never);
    }

    #endregion
}
