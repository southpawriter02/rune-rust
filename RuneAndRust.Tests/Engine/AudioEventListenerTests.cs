using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Data;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the AudioEventListener class (v0.3.19b).
/// Validates event subscription and sound cue triggering.
/// </summary>
public class AudioEventListenerTests
{
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly Mock<IAudioService> _mockAudioService;
    private readonly Mock<ILogger<AudioEventListener>> _mockLogger;
    private readonly AudioEventListener _sut;

    // Captured handlers for testing
    private Func<EntityDamagedEvent, Task>? _capturedDamageHandler;
    private Func<EntityDeathEvent, Task>? _capturedDeathHandler;
    private Func<ItemLootedEvent, Task>? _capturedLootHandler;

    public AudioEventListenerTests()
    {
        _mockEventBus = new Mock<IEventBus>();
        _mockAudioService = new Mock<IAudioService>();
        _mockLogger = new Mock<ILogger<AudioEventListener>>();

        // Capture handlers when SubscribeAsync is called
        _mockEventBus
            .Setup(e => e.SubscribeAsync(It.IsAny<Func<EntityDamagedEvent, Task>>()))
            .Callback<Func<EntityDamagedEvent, Task>>(h => _capturedDamageHandler = h);

        _mockEventBus
            .Setup(e => e.SubscribeAsync(It.IsAny<Func<EntityDeathEvent, Task>>()))
            .Callback<Func<EntityDeathEvent, Task>>(h => _capturedDeathHandler = h);

        _mockEventBus
            .Setup(e => e.SubscribeAsync(It.IsAny<Func<ItemLootedEvent, Task>>()))
            .Callback<Func<ItemLootedEvent, Task>>(h => _capturedLootHandler = h);

        _sut = new AudioEventListener(
            _mockEventBus.Object,
            _mockAudioService.Object,
            _mockLogger.Object);
    }

    #region SubscribeAll Tests

    [Fact]
    public void SubscribeAll_SubscribesToAllEventTypes()
    {
        // Act
        _sut.SubscribeAll();

        // Assert
        _mockEventBus.Verify(e => e.SubscribeAsync(It.IsAny<Func<EntityDamagedEvent, Task>>()), Times.Once);
        _mockEventBus.Verify(e => e.SubscribeAsync(It.IsAny<Func<EntityDeathEvent, Task>>()), Times.Once);
        _mockEventBus.Verify(e => e.SubscribeAsync(It.IsAny<Func<ItemLootedEvent, Task>>()), Times.Once);
    }

    #endregion

    #region Damage Event Tests

    [Fact]
    public async Task OnEntityDamaged_LightHit_PlaysLightHitCue()
    {
        // Arrange
        _sut.SubscribeAll();

        var damageEvent = new EntityDamagedEvent(
            Guid.NewGuid(),
            "Enemy",
            5, // Light damage
            false,
            DamageType.Physical,
            95);

        // Act
        await _capturedDamageHandler!(damageEvent);

        // Assert
        _mockAudioService.Verify(
            a => a.PlayAsync(It.Is<SoundCue>(c => c.Id == SoundCue.CombatHitLight.Id)),
            Times.Once);
    }

    [Fact]
    public async Task OnEntityDamaged_HeavyHit_PlaysHeavyHitCue()
    {
        // Arrange
        _sut.SubscribeAll();

        var damageEvent = new EntityDamagedEvent(
            Guid.NewGuid(),
            "Enemy",
            25, // Heavy damage (>10)
            false,
            DamageType.Physical,
            75);

        // Act
        await _capturedDamageHandler!(damageEvent);

        // Assert
        _mockAudioService.Verify(
            a => a.PlayAsync(It.Is<SoundCue>(c => c.Id == SoundCue.CombatHitHeavy.Id)),
            Times.Once);
    }

    [Fact]
    public async Task OnEntityDamaged_CriticalHit_PlaysCriticalCue()
    {
        // Arrange
        _sut.SubscribeAll();

        var damageEvent = new EntityDamagedEvent(
            Guid.NewGuid(),
            "Enemy",
            5, // Even light damage
            true, // But critical!
            DamageType.Physical,
            95);

        // Act
        await _capturedDamageHandler!(damageEvent);

        // Assert
        _mockAudioService.Verify(
            a => a.PlayAsync(It.Is<SoundCue>(c => c.Id == SoundCue.CombatCritical.Id)),
            Times.Once);
    }

    #endregion

    #region Death Event Tests

    [Fact]
    public async Task OnEntityDeath_EnemyDeath_PlaysHeavyHitCue()
    {
        // Arrange
        _sut.SubscribeAll();

        var deathEvent = new EntityDeathEvent(
            Guid.NewGuid(),
            "Goblin",
            false, // Not a player
            "Hero");

        // Act
        await _capturedDeathHandler!(deathEvent);

        // Assert
        _mockAudioService.Verify(
            a => a.PlayAsync(It.Is<SoundCue>(c => c.Id == SoundCue.CombatHitHeavy.Id)),
            Times.Once);
    }

    [Fact]
    public async Task OnEntityDeath_PlayerDeath_PlaysErrorCue()
    {
        // Arrange
        _sut.SubscribeAll();

        var deathEvent = new EntityDeathEvent(
            Guid.NewGuid(),
            "Hero",
            true, // Player death
            "Boss");

        // Act
        await _capturedDeathHandler!(deathEvent);

        // Assert
        _mockAudioService.Verify(
            a => a.PlayAsync(It.Is<SoundCue>(c => c.Id == SoundCue.UiError.Id)),
            Times.Once);
    }

    #endregion

    #region Loot Event Tests

    [Fact]
    public async Task OnItemLooted_CommonItem_PlaysClickCue()
    {
        // Arrange
        _sut.SubscribeAll();

        var lootEvent = new ItemLootedEvent(
            Guid.NewGuid(),
            "Iron Scrap",
            1,
            10); // Low value

        // Act
        await _capturedLootHandler!(lootEvent);

        // Assert
        _mockAudioService.Verify(
            a => a.PlayAsync(It.Is<SoundCue>(c => c.Id == SoundCue.UiClick.Id)),
            Times.Once);
    }

    [Fact]
    public async Task OnItemLooted_ValuableItem_PlaysSelectCue()
    {
        // Arrange
        _sut.SubscribeAll();

        var lootEvent = new ItemLootedEvent(
            Guid.NewGuid(),
            "Ancient Relic",
            1,
            100); // High value (>50)

        // Act
        await _capturedLootHandler!(lootEvent);

        // Assert
        _mockAudioService.Verify(
            a => a.PlayAsync(It.Is<SoundCue>(c => c.Id == SoundCue.UiSelect.Id)),
            Times.Once);
    }

    #endregion

    #region SoundRegistry Tests

    [Theory]
    [InlineData(true, 5, "combat_critical")]  // Critical always wins
    [InlineData(true, 25, "combat_critical")] // Critical always wins
    [InlineData(false, 5, "combat_hit_light")]  // Light hit
    [InlineData(false, 10, "combat_hit_light")] // Boundary (<=10 is light)
    [InlineData(false, 11, "combat_hit_heavy")] // Heavy hit
    [InlineData(false, 50, "combat_hit_heavy")] // Heavy hit
    public void GetDamageCue_ReturnsCorrectCue(bool isCritical, int damage, string expectedCueId)
    {
        // Act
        var cue = SoundRegistry.GetDamageCue(isCritical, damage);

        // Assert
        cue.Id.Should().Be(expectedCueId);
    }

    [Theory]
    [InlineData(true, "ui_error")]   // Player death
    [InlineData(false, "combat_hit_heavy")] // Enemy death
    public void GetDeathCue_ReturnsCorrectCue(bool isPlayer, string expectedCueId)
    {
        // Act
        var cue = SoundRegistry.GetDeathCue(isPlayer);

        // Assert
        cue.Id.Should().Be(expectedCueId);
    }

    [Theory]
    [InlineData(10, "ui_click")]  // Low value
    [InlineData(50, "ui_click")]  // Boundary (<=50 is common)
    [InlineData(51, "ui_select")] // Valuable
    [InlineData(100, "ui_select")] // Valuable
    public void GetLootCue_ReturnsCorrectCue(int itemValue, string expectedCueId)
    {
        // Act
        var cue = SoundRegistry.GetLootCue(itemValue);

        // Assert
        cue.Id.Should().Be(expectedCueId);
    }

    [Theory]
    [InlineData(UiActionType.Click, "ui_click")]
    [InlineData(UiActionType.Select, "ui_select")]
    [InlineData(UiActionType.Error, "ui_error")]
    [InlineData(UiActionType.Confirm, "ui_select")]
    [InlineData(UiActionType.Cancel, "ui_click")]
    public void GetUiCue_ReturnsCorrectCue(UiActionType actionType, string expectedCueId)
    {
        // Act
        var cue = SoundRegistry.GetUiCue(actionType);

        // Assert
        cue.Id.Should().Be(expectedCueId);
    }

    #endregion
}
