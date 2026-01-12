using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Events;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for GameEventLogger service.
/// </summary>
[TestFixture]
public class GameEventLoggerTests
{
    private GameEventLogger _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _logger = new GameEventLogger();
    }

    [Test]
    public void Log_StoresEvent()
    {
        // Arrange
        var evt = GameEvent.System("Test", "Test message");

        // Act
        _logger.Log(evt);
        var events = _logger.GetEventsByCategory(EventCategory.System);

        // Assert
        events.Should().HaveCount(1);
        events[0].Message.Should().Be("Test message");
    }

    [Test]
    public void LogCombat_StoresCombatEvent()
    {
        // Act
        _logger.LogCombat("Attack", "Player attacks enemy");

        // Assert
        var events = _logger.GetEventsByCategory(EventCategory.Combat);
        events.Should().HaveCount(1);
        events[0].EventType.Should().Be("Attack");
    }

    [Test]
    public void LogExploration_StoresExplorationEvent()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        // Act
        _logger.LogExploration("RoomEntered", "Entered room", roomId);

        // Assert
        var events = _logger.GetEventsByCategory(EventCategory.Exploration);
        events.Should().HaveCount(1);
        events[0].RoomId.Should().Be(roomId);
    }

    [Test]
    public void SetSession_EnrichesEvents()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        // Act
        _logger.SetSession(sessionId, playerId);
        _logger.LogCombat("Attack", "Player attacks");

        // Assert
        var events = _logger.GetSessionEvents();
        events.Should().Contain(e => e.SessionId == sessionId && e.PlayerId == playerId);
    }

    [Test]
    public void GetSessionEvents_ReturnsOnlySessionEvents()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _logger.LogCombat("PreSession", "Before session");
        _logger.SetSession(sessionId);
        _logger.LogCombat("InSession", "During session");

        // Act
        var sessionEvents = _logger.GetSessionEvents();

        // Assert
        sessionEvents.Should().OnlyContain(e => e.SessionId == sessionId);
    }

    [Test]
    public void ClearSession_ResetsContext()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _logger.SetSession(sessionId);

        // Act
        _logger.ClearSession();
        _logger.LogCombat("PostSession", "After session");

        // Assert
        var events = _logger.GetEventsByCategory(EventCategory.Combat);
        events.Last().SessionId.Should().BeNull();
    }

    [Test]
    public void GetEventsByCategory_FiltersCorrectly()
    {
        // Arrange
        _logger.LogCombat("Attack", "Combat event");
        _logger.LogExploration("Move", "Exploration event");
        _logger.LogQuest("Start", "Quest event");

        // Act
        var combatEvents = _logger.GetEventsByCategory(EventCategory.Combat);
        var explorationEvents = _logger.GetEventsByCategory(EventCategory.Exploration);

        // Assert
        combatEvents.Should().HaveCount(1);
        explorationEvents.Should().HaveCount(1);
    }

    [Test]
    public void LogSystem_SetsSeverity()
    {
        // Act
        _logger.LogSystem("Error", "Something failed", EventSeverity.Error);

        // Assert
        var events = _logger.GetEventsByCategory(EventCategory.System);
        events.Should().Contain(e => e.Severity == EventSeverity.Error);
    }

    [Test]
    public void Log_WithData_StoresData()
    {
        // Act
        _logger.LogCombat("Attack", "Hit", data: new Dictionary<string, object> { ["damage"] = 25 });

        // Assert
        var events = _logger.GetEventsByCategory(EventCategory.Combat);
        events[0].Data.Should().ContainKey("damage");
    }
}
