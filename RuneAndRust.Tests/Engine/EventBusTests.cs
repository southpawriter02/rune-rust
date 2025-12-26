using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the EventBus class (v0.3.19b).
/// Validates publish/subscribe functionality for game events.
/// </summary>
public class EventBusTests
{
    private readonly Mock<ILogger<EventBus>> _mockLogger;
    private readonly EventBus _sut;

    public EventBusTests()
    {
        _mockLogger = new Mock<ILogger<EventBus>>();
        _sut = new EventBus(_mockLogger.Object);
    }

    #region Subscribe Tests

    [Fact]
    public void Subscribe_SyncHandler_ReceivesPublishedEvents()
    {
        // Arrange
        EntityDamagedEvent? receivedEvent = null;
        _sut.Subscribe<EntityDamagedEvent>(e => receivedEvent = e);

        var testEvent = new EntityDamagedEvent(
            Guid.NewGuid(),
            "Test Target",
            25,
            false,
            DamageType.Physical,
            75);

        // Act
        _sut.Publish(testEvent);

        // Assert
        receivedEvent.Should().NotBeNull();
        receivedEvent!.TargetName.Should().Be("Test Target");
        receivedEvent.Amount.Should().Be(25);
    }

    [Fact]
    public void Subscribe_MultipleHandlers_AllReceiveEvents()
    {
        // Arrange
        var receivedCount = 0;
        _sut.Subscribe<EntityDamagedEvent>(_ => receivedCount++);
        _sut.Subscribe<EntityDamagedEvent>(_ => receivedCount++);
        _sut.Subscribe<EntityDamagedEvent>(_ => receivedCount++);

        var testEvent = new EntityDamagedEvent(
            Guid.NewGuid(),
            "Test",
            10,
            false,
            DamageType.Fire,
            90);

        // Act
        _sut.Publish(testEvent);

        // Assert
        receivedCount.Should().Be(3);
    }

    [Fact]
    public async Task SubscribeAsync_AsyncHandler_ReceivesPublishedEvents()
    {
        // Arrange
        EntityDeathEvent? receivedEvent = null;
        _sut.SubscribeAsync<EntityDeathEvent>(async e =>
        {
            await Task.Delay(1); // Simulate async work
            receivedEvent = e;
        });

        var testEvent = new EntityDeathEvent(
            Guid.NewGuid(),
            "Fallen Enemy",
            false,
            "Player");

        // Act
        await _sut.PublishAsync(testEvent);

        // Assert
        receivedEvent.Should().NotBeNull();
        receivedEvent!.DeceasedName.Should().Be("Fallen Enemy");
        receivedEvent.KilledByName.Should().Be("Player");
    }

    [Fact]
    public async Task SubscribeAsync_MultipleAsyncHandlers_AllReceiveEvents()
    {
        // Arrange
        var receivedCount = 0;
        _sut.SubscribeAsync<ItemLootedEvent>(async _ =>
        {
            await Task.Delay(1);
            Interlocked.Increment(ref receivedCount);
        });
        _sut.SubscribeAsync<ItemLootedEvent>(async _ =>
        {
            await Task.Delay(1);
            Interlocked.Increment(ref receivedCount);
        });

        var testEvent = new ItemLootedEvent(
            Guid.NewGuid(),
            "Gold Coin",
            5,
            100);

        // Act
        await _sut.PublishAsync(testEvent);

        // Assert
        receivedCount.Should().Be(2);
    }

    #endregion

    #region Publish Tests

    [Fact]
    public void Publish_NoSubscribers_DoesNotThrow()
    {
        // Arrange
        var testEvent = new EntityDamagedEvent(
            Guid.NewGuid(),
            "Test",
            10,
            false,
            DamageType.Lightning,
            90);

        // Act
        var action = () => _sut.Publish(testEvent);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Publish_OneHandlerThrows_OtherHandlersStillExecute()
    {
        // Arrange
        var handler1Called = false;
        var handler3Called = false;

        _sut.Subscribe<EntityDamagedEvent>(_ => handler1Called = true);
        _sut.Subscribe<EntityDamagedEvent>(_ => throw new InvalidOperationException("Test exception"));
        _sut.Subscribe<EntityDamagedEvent>(_ => handler3Called = true);

        var testEvent = new EntityDamagedEvent(
            Guid.NewGuid(),
            "Test",
            10,
            false,
            DamageType.Poison,
            90);

        // Act
        var action = () => _sut.Publish(testEvent);

        // Assert
        action.Should().NotThrow();
        handler1Called.Should().BeTrue();
        handler3Called.Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_NoSubscribers_DoesNotThrow()
    {
        // Arrange
        var testEvent = new EntityDeathEvent(
            Guid.NewGuid(),
            "Test",
            true,
            "Enemy");

        // Act
        var action = async () => await _sut.PublishAsync(testEvent);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_OneHandlerThrows_OtherHandlersStillExecute()
    {
        // Arrange
        var handler1Called = false;
        var handler3Called = false;

        _sut.SubscribeAsync<ItemLootedEvent>(async _ =>
        {
            await Task.Delay(1);
            handler1Called = true;
        });
        _sut.SubscribeAsync<ItemLootedEvent>(_ => throw new InvalidOperationException("Test exception"));
        _sut.SubscribeAsync<ItemLootedEvent>(async _ =>
        {
            await Task.Delay(1);
            handler3Called = true;
        });

        var testEvent = new ItemLootedEvent(
            Guid.NewGuid(),
            "Test Item",
            1,
            50);

        // Act
        var action = async () => await _sut.PublishAsync(testEvent);

        // Assert
        await action.Should().NotThrowAsync();
        handler1Called.Should().BeTrue();
        handler3Called.Should().BeTrue();
    }

    #endregion

    #region Unsubscribe Tests

    [Fact]
    public void Unsubscribe_SyncHandler_NoLongerReceivesEvents()
    {
        // Arrange
        var receivedCount = 0;
        Action<EntityDamagedEvent> handler = _ => receivedCount++;

        _sut.Subscribe(handler);
        _sut.Unsubscribe(handler);

        var testEvent = new EntityDamagedEvent(
            Guid.NewGuid(),
            "Test",
            10,
            false,
            DamageType.Physical,
            90);

        // Act
        _sut.Publish(testEvent);

        // Assert
        receivedCount.Should().Be(0);
    }

    [Fact]
    public async Task UnsubscribeAsync_AsyncHandler_NoLongerReceivesEvents()
    {
        // Arrange
        var receivedCount = 0;
        Func<EntityDeathEvent, Task> handler = async _ =>
        {
            await Task.Delay(1);
            receivedCount++;
        };

        _sut.SubscribeAsync(handler);
        _sut.UnsubscribeAsync(handler);

        var testEvent = new EntityDeathEvent(
            Guid.NewGuid(),
            "Test",
            false,
            "Killer");

        // Act
        await _sut.PublishAsync(testEvent);

        // Assert
        receivedCount.Should().Be(0);
    }

    [Fact]
    public void Unsubscribe_NonExistentHandler_DoesNotThrow()
    {
        // Arrange
        Action<EntityDamagedEvent> handler = _ => { };

        // Act
        var action = () => _sut.Unsubscribe(handler);

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region Event Type Isolation Tests

    [Fact]
    public void Publish_DifferentEventTypes_HandlersAreIsolated()
    {
        // Arrange
        var damageHandlerCalled = false;
        var deathHandlerCalled = false;

        _sut.Subscribe<EntityDamagedEvent>(_ => damageHandlerCalled = true);
        _sut.Subscribe<EntityDeathEvent>(_ => deathHandlerCalled = true);

        var damageEvent = new EntityDamagedEvent(
            Guid.NewGuid(),
            "Target",
            10,
            false,
            DamageType.Physical,
            90);

        // Act
        _sut.Publish(damageEvent);

        // Assert
        damageHandlerCalled.Should().BeTrue();
        deathHandlerCalled.Should().BeFalse();
    }

    #endregion

    #region Event Property Tests

    [Fact]
    public void EntityDamagedEvent_IsLethal_ReturnsTrueWhenHpIsZeroOrBelow()
    {
        // Arrange & Act
        var lethalEvent = new EntityDamagedEvent(
            Guid.NewGuid(),
            "Target",
            100,
            true,
            DamageType.Physical,
            0);

        var nonLethalEvent = new EntityDamagedEvent(
            Guid.NewGuid(),
            "Target",
            10,
            false,
            DamageType.Physical,
            50);

        // Assert
        lethalEvent.IsLethal.Should().BeTrue();
        nonLethalEvent.IsLethal.Should().BeFalse();
    }

    [Fact]
    public void ItemLootedEvent_IsValuable_ReturnsTrueWhenValueExceedsThreshold()
    {
        // Arrange & Act
        var valuableEvent = new ItemLootedEvent(
            Guid.NewGuid(),
            "Rare Gem",
            1,
            100);

        var commonEvent = new ItemLootedEvent(
            Guid.NewGuid(),
            "Common Coin",
            1,
            10);

        // Assert
        valuableEvent.IsValuable.Should().BeTrue();
        commonEvent.IsValuable.Should().BeFalse();
    }

    #endregion
}
