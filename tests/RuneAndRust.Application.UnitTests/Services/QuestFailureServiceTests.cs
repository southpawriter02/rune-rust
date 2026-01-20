using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for QuestFailureService.
/// </summary>
[TestFixture]
public class QuestFailureServiceTests
{
    private QuestFailureService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new QuestFailureService();
    }

    [Test]
    public void ProcessQuestTimers_ExpiredQuest_ReturnsFailure()
    {
        // Arrange
        var quests = new List<TimedQuestInfo>
        {
            new("quest-1", "Timed Quest", 0, true)
        };

        // Act
        var results = _service.ProcessQuestTimers(quests);

        // Assert
        results.Should().HaveCount(1);
        results[0].QuestId.Should().Be("quest-1");
        results[0].ConditionType.Should().Be(FailureType.TimeExpired);
    }

    [Test]
    public void ProcessQuestTimers_NonExpiredQuests_ReturnsEmpty()
    {
        // Arrange
        var quests = new List<TimedQuestInfo>
        {
            new("quest-1", "Active Quest", 5, false)
        };

        // Act
        var results = _service.ProcessQuestTimers(quests);

        // Assert
        results.Should().BeEmpty();
    }

    [Test]
    public void CheckFailureConditions_NPCDied_ReturnsCondition()
    {
        // Arrange
        var conditions = new[] { FailureCondition.NPCDied("npc-merchant") };
        var context = new FailureCheckContext(
            DeadNpcIds: new HashSet<string> { "npc-merchant" },
            InventoryItemIds: new HashSet<string>(),
            FactionReputations: new Dictionary<string, int>(),
            CurrentAreaId: "town");

        // Act
        var result = _service.CheckFailureConditions(conditions, context);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Type.Should().Be(FailureType.NPCDied);
    }

    [Test]
    public void CheckFailureConditions_ItemLost_ReturnsCondition()
    {
        // Arrange
        var conditions = new[] { FailureCondition.ItemLost("quest-item") };
        var context = new FailureCheckContext(
            DeadNpcIds: new HashSet<string>(),
            InventoryItemIds: new HashSet<string>(), // Item not in inventory
            FactionReputations: new Dictionary<string, int>(),
            CurrentAreaId: "town");

        // Act
        var result = _service.CheckFailureConditions(conditions, context);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Type.Should().Be(FailureType.ItemLost);
    }

    [Test]
    public void CheckFailureConditions_ReputationDropped_ReturnsCondition()
    {
        // Arrange
        var conditions = new[] { FailureCondition.ReputationDropped("guild", 0) };
        var context = new FailureCheckContext(
            DeadNpcIds: new HashSet<string>(),
            InventoryItemIds: new HashSet<string>(),
            FactionReputations: new Dictionary<string, int> { ["guild"] = -10 },
            CurrentAreaId: "town");

        // Act
        var result = _service.CheckFailureConditions(conditions, context);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Type.Should().Be(FailureType.ReputationDropped);
    }

    [Test]
    public void CheckFailureConditions_LeftArea_ReturnsCondition()
    {
        // Arrange
        var conditions = new[] { FailureCondition.LeftArea("dungeon") };
        var context = new FailureCheckContext(
            DeadNpcIds: new HashSet<string>(),
            InventoryItemIds: new HashSet<string>(),
            FactionReputations: new Dictionary<string, int>(),
            CurrentAreaId: "town"); // Not in dungeon

        // Act
        var result = _service.CheckFailureConditions(conditions, context);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Type.Should().Be(FailureType.LeftArea);
    }

    [Test]
    public void CheckFailureConditions_NoFailure_ReturnsNull()
    {
        // Arrange
        var conditions = new[] { FailureCondition.NPCDied("npc-merchant") };
        var context = new FailureCheckContext(
            DeadNpcIds: new HashSet<string>(), // NPC not dead
            InventoryItemIds: new HashSet<string>(),
            FactionReputations: new Dictionary<string, int>(),
            CurrentAreaId: "town");

        // Act
        var result = _service.CheckFailureConditions(conditions, context);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void CreateFailureResult_CreatesCorrectResult()
    {
        // Arrange
        var condition = FailureCondition.NPCDied("npc-1", "The merchant died!");

        // Act
        var result = _service.CreateFailureResult("quest-1", "Save Merchant", condition);

        // Assert
        result.QuestId.Should().Be("quest-1");
        result.QuestName.Should().Be("Save Merchant");
        result.Reason.Should().Be("The merchant died!");
        result.ConditionType.Should().Be(FailureType.NPCDied);
    }
}
