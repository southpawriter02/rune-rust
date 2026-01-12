using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Events;

namespace RuneAndRust.Domain.UnitTests.Events;

/// <summary>
/// Tests for GameEvent base record.
/// </summary>
[TestFixture]
public class GameEventTests
{
    [Test]
    public void System_CreatesSystemEvent()
    {
        // Act
        var evt = GameEvent.System("ServiceStarted", "Service started");

        // Assert
        evt.Category.Should().Be(EventCategory.System);
        evt.EventType.Should().Be("ServiceStarted");
        evt.Message.Should().Be("Service started");
    }

    [Test]
    public void Combat_CreatesCombatEvent()
    {
        // Arrange
        var correlationId = Guid.NewGuid();

        // Act
        var evt = GameEvent.Combat("Attack", "Player attacks", correlationId);

        // Assert
        evt.Category.Should().Be(EventCategory.Combat);
        evt.CorrelationId.Should().Be(correlationId);
    }

    [Test]
    public void Exploration_CreatesExplorationEvent()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        // Act
        var evt = GameEvent.Exploration("RoomEntered", "Entered room", roomId);

        // Assert
        evt.Category.Should().Be(EventCategory.Exploration);
        evt.RoomId.Should().Be(roomId);
    }

    [Test]
    public void Quest_CreatesQuestEvent()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        // Act
        var evt = GameEvent.Quest("QuestStarted", "Started quest", playerId);

        // Assert
        evt.Category.Should().Be(EventCategory.Quest);
        evt.PlayerId.Should().Be(playerId);
    }

    [Test]
    public void EventId_IsUnique()
    {
        // Act
        var evt1 = GameEvent.System("Test", "Test");
        var evt2 = GameEvent.System("Test", "Test");

        // Assert
        evt1.EventId.Should().NotBe(evt2.EventId);
    }

    [Test]
    public void Timestamp_IsSetAutomatically()
    {
        // Act
        var before = DateTime.UtcNow;
        var evt = GameEvent.System("Test", "Test");
        var after = DateTime.UtcNow;

        // Assert
        evt.Timestamp.Should().BeOnOrAfter(before);
        evt.Timestamp.Should().BeOnOrBefore(after);
    }
}
