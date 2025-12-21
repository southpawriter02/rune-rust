using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the HazardService class (v0.3.3a).
/// Validates hazard triggers, cooldowns, state transitions, and effect execution.
/// </summary>
public class HazardServiceTests
{
    private readonly Mock<IInteractableObjectRepository> _mockObjectRepository;
    private readonly Mock<IDiceService> _mockDiceService;
    private readonly Mock<IStatusEffectService> _mockStatusEffectService;
    private readonly Mock<ILogger<HazardService>> _mockLogger;
    private readonly Mock<ILogger<EffectScriptExecutor>> _mockScriptLogger;
    private readonly EffectScriptExecutor _scriptExecutor;
    private readonly HazardService _sut;

    public HazardServiceTests()
    {
        _mockObjectRepository = new Mock<IInteractableObjectRepository>();
        _mockDiceService = new Mock<IDiceService>();
        _mockStatusEffectService = new Mock<IStatusEffectService>();
        _mockLogger = new Mock<ILogger<HazardService>>();
        _mockScriptLogger = new Mock<ILogger<EffectScriptExecutor>>();

        // Create EffectScriptExecutor with mocked dependencies
        _scriptExecutor = new EffectScriptExecutor(
            _mockDiceService.Object,
            _mockStatusEffectService.Object,
            _mockScriptLogger.Object);

        _sut = new HazardService(
            _mockObjectRepository.Object,
            _scriptExecutor,
            _mockLogger.Object);

        // Default dice roll returns 4
        _mockDiceService.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(4);

        // Default status effect modifiers
        _mockStatusEffectService.Setup(s => s.GetDamageMultiplier(It.IsAny<Combatant>()))
            .Returns(1.0f);
        _mockStatusEffectService.Setup(s => s.GetSoakModifier(It.IsAny<Combatant>()))
            .Returns(0);
    }

    #region TriggerOnRoomEnter Tests

    [Fact]
    public async Task TriggerOnRoomEnterAsync_DormantMovementHazard_TriggersAndAppliesDamage()
    {
        // Arrange
        var room = CreateTestRoom();
        var target = CreateTestCombatant(currentHp: 50);
        var hazard = CreateTestHazard(
            roomId: room.Id,
            trigger: TriggerType.Movement,
            state: HazardState.Dormant,
            effectScript: "DAMAGE:Fire:2d6");

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        var results = await _sut.TriggerOnRoomEnterAsync(room, target);

        // Assert
        results.Should().HaveCount(1);
        results[0].WasTriggered.Should().BeTrue();
        results[0].TotalDamage.Should().Be(8); // 2d6 = 2 * 4 = 8
        target.CurrentHp.Should().Be(42); // 50 - 8
    }

    [Fact]
    public async Task TriggerOnRoomEnterAsync_CooldownHazard_DoesNotTrigger()
    {
        // Arrange
        var room = CreateTestRoom();
        var target = CreateTestCombatant();
        var hazard = CreateTestHazard(
            roomId: room.Id,
            trigger: TriggerType.Movement,
            state: HazardState.Cooldown);

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        var results = await _sut.TriggerOnRoomEnterAsync(room, target);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task TriggerOnRoomEnterAsync_DestroyedHazard_NotIncluded()
    {
        // Arrange
        var room = CreateTestRoom();
        var target = CreateTestCombatant();
        var hazard = CreateTestHazard(
            roomId: room.Id,
            trigger: TriggerType.Movement,
            state: HazardState.Destroyed);

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        var results = await _sut.TriggerOnRoomEnterAsync(room, target);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task TriggerOnRoomEnterAsync_DamageTriggerHazard_DoesNotTriggerOnMovement()
    {
        // Arrange
        var room = CreateTestRoom();
        var target = CreateTestCombatant();
        var hazard = CreateTestHazard(
            roomId: room.Id,
            trigger: TriggerType.DamageTaken,
            state: HazardState.Dormant);

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        var results = await _sut.TriggerOnRoomEnterAsync(room, target);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task TriggerOnRoomEnterAsync_NoEntrant_TriggersWithoutApplyingDamage()
    {
        // Arrange
        var room = CreateTestRoom();
        var hazard = CreateTestHazard(
            roomId: room.Id,
            trigger: TriggerType.Movement,
            state: HazardState.Dormant,
            effectScript: "DAMAGE:Fire:2d6");

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        var results = await _sut.TriggerOnRoomEnterAsync(room, entrant: null);

        // Assert
        results.Should().HaveCount(1);
        results[0].WasTriggered.Should().BeTrue();
        results[0].TotalDamage.Should().Be(0); // No target, no damage applied
    }

    #endregion

    #region TriggerOnDamage Tests

    [Fact]
    public async Task TriggerOnDamageAsync_MatchingDamageType_Triggers()
    {
        // Arrange
        var room = CreateTestRoom();
        var target = CreateTestCombatant(currentHp: 50);
        var hazard = CreateTestHazard(
            roomId: room.Id,
            trigger: TriggerType.DamageTaken,
            state: HazardState.Dormant,
            requiredDamageType: DamageType.Fire,
            effectScript: "DAMAGE:Fire:1d6");

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        var results = await _sut.TriggerOnDamageAsync(room, DamageType.Fire, 10, target);

        // Assert
        results.Should().HaveCount(1);
        results[0].WasTriggered.Should().BeTrue();
    }

    [Fact]
    public async Task TriggerOnDamageAsync_WrongDamageType_DoesNotTrigger()
    {
        // Arrange
        var room = CreateTestRoom();
        var target = CreateTestCombatant();
        var hazard = CreateTestHazard(
            roomId: room.Id,
            trigger: TriggerType.DamageTaken,
            state: HazardState.Dormant,
            requiredDamageType: DamageType.Fire);

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        var results = await _sut.TriggerOnDamageAsync(room, DamageType.Physical, 10, target);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task TriggerOnDamageAsync_AnyDamageType_TriggersOnAnyType()
    {
        // Arrange
        var room = CreateTestRoom();
        var target = CreateTestCombatant();
        var hazard = CreateTestHazard(
            roomId: room.Id,
            trigger: TriggerType.DamageTaken,
            state: HazardState.Dormant,
            requiredDamageType: null); // No specific type required

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        var results = await _sut.TriggerOnDamageAsync(room, DamageType.Ice, 10, target);

        // Assert
        results.Should().HaveCount(1);
        results[0].WasTriggered.Should().BeTrue();
    }

    [Fact]
    public async Task TriggerOnDamageAsync_BelowThreshold_DoesNotTrigger()
    {
        // Arrange
        var room = CreateTestRoom();
        var target = CreateTestCombatant();
        var hazard = CreateTestHazard(
            roomId: room.Id,
            trigger: TriggerType.DamageTaken,
            state: HazardState.Dormant,
            damageThreshold: 10);

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        var results = await _sut.TriggerOnDamageAsync(room, DamageType.Physical, 5, target);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task TriggerOnDamageAsync_AtThreshold_Triggers()
    {
        // Arrange
        var room = CreateTestRoom();
        var target = CreateTestCombatant();
        var hazard = CreateTestHazard(
            roomId: room.Id,
            trigger: TriggerType.DamageTaken,
            state: HazardState.Dormant,
            damageThreshold: 10);

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        var results = await _sut.TriggerOnDamageAsync(room, DamageType.Physical, 10, target);

        // Assert
        results.Should().HaveCount(1);
        results[0].WasTriggered.Should().BeTrue();
    }

    #endregion

    #region State Transition Tests

    [Fact]
    public async Task ActivateHazard_OneTimeUse_SetsDestroyedState()
    {
        // Arrange
        var room = CreateTestRoom();
        var target = CreateTestCombatant();
        var hazard = CreateTestHazard(
            roomId: room.Id,
            trigger: TriggerType.Movement,
            state: HazardState.Dormant,
            oneTimeUse: true);

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        var results = await _sut.TriggerOnRoomEnterAsync(room, target);

        // Assert
        results.Should().HaveCount(1);
        results[0].NewState.Should().Be(HazardState.Destroyed);
        hazard.State.Should().Be(HazardState.Destroyed);
    }

    [Fact]
    public async Task ActivateHazard_Reusable_SetsCooldownState()
    {
        // Arrange
        var room = CreateTestRoom();
        var target = CreateTestCombatant();
        var hazard = CreateTestHazard(
            roomId: room.Id,
            trigger: TriggerType.Movement,
            state: HazardState.Dormant,
            oneTimeUse: false,
            maxCooldown: 3);

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        var results = await _sut.TriggerOnRoomEnterAsync(room, target);

        // Assert
        results.Should().HaveCount(1);
        results[0].NewState.Should().Be(HazardState.Cooldown);
        hazard.State.Should().Be(HazardState.Cooldown);
        hazard.CooldownRemaining.Should().Be(3);
    }

    #endregion

    #region Cooldown Tick Tests

    [Fact]
    public async Task TickCooldownsAsync_DecrementsCooldown()
    {
        // Arrange
        var room = CreateTestRoom();
        var hazard = CreateTestHazard(
            roomId: room.Id,
            state: HazardState.Cooldown,
            cooldownRemaining: 2,
            maxCooldown: 3);

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        await _sut.TickCooldownsAsync(room);

        // Assert
        hazard.CooldownRemaining.Should().Be(1);
        hazard.State.Should().Be(HazardState.Cooldown);
    }

    [Fact]
    public async Task TickCooldownsAsync_ZeroCooldown_ReturnsToDormant()
    {
        // Arrange
        var room = CreateTestRoom();
        var hazard = CreateTestHazard(
            roomId: room.Id,
            state: HazardState.Cooldown,
            cooldownRemaining: 1,
            maxCooldown: 3);

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        await _sut.TickCooldownsAsync(room);

        // Assert
        hazard.CooldownRemaining.Should().Be(0);
        hazard.State.Should().Be(HazardState.Dormant);
    }

    [Fact]
    public async Task TickCooldownsAsync_DormantHazard_NoChange()
    {
        // Arrange
        var room = CreateTestRoom();
        var hazard = CreateTestHazard(
            roomId: room.Id,
            state: HazardState.Dormant,
            cooldownRemaining: 0);

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        await _sut.TickCooldownsAsync(room);

        // Assert
        hazard.State.Should().Be(HazardState.Dormant);
        hazard.CooldownRemaining.Should().Be(0);
    }

    [Fact]
    public async Task TickCooldownsAsync_MultipleCooldownHazards_AllTicked()
    {
        // Arrange
        var room = CreateTestRoom();
        var hazard1 = CreateTestHazard(
            roomId: room.Id,
            state: HazardState.Cooldown,
            cooldownRemaining: 2);
        var hazard2 = CreateTestHazard(
            roomId: room.Id,
            state: HazardState.Cooldown,
            cooldownRemaining: 1);

        SetupRepositoryWithHazards(room.Id, hazard1, hazard2);

        // Act
        await _sut.TickCooldownsAsync(room);

        // Assert
        hazard1.CooldownRemaining.Should().Be(1);
        hazard1.State.Should().Be(HazardState.Cooldown);
        hazard2.CooldownRemaining.Should().Be(0);
        hazard2.State.Should().Be(HazardState.Dormant);
    }

    #endregion

    #region ProcessTurnStartHazards Tests

    [Fact]
    public async Task ProcessTurnStartHazardsAsync_ActiveHazard_AppliesEffectToAllCombatants()
    {
        // Arrange
        var room = CreateTestRoom();
        var combatant1 = CreateTestCombatant(name: "Player", currentHp: 50);
        var combatant2 = CreateTestCombatant(name: "Enemy", currentHp: 30);
        var hazard = CreateTestHazard(
            roomId: room.Id,
            trigger: TriggerType.TurnStart,
            state: HazardState.Dormant,
            effectScript: "DAMAGE:Poison:1d4");

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        var results = await _sut.ProcessTurnStartHazardsAsync(room, new List<Combatant> { combatant1, combatant2 });

        // Assert
        results.Should().HaveCount(2); // One result per combatant
        combatant1.CurrentHp.Should().Be(46); // 50 - 4
        combatant2.CurrentHp.Should().Be(26); // 30 - 4
    }

    #endregion

    #region ManualActivate Tests

    [Fact]
    public async Task ManualActivateAsync_ManualTriggerHazard_Activates()
    {
        // Arrange
        var target = CreateTestCombatant(currentHp: 50);
        var hazard = CreateTestHazard(
            trigger: TriggerType.ManualInteraction,
            state: HazardState.Dormant,
            effectScript: "DAMAGE:Physical:1d8");

        _mockObjectRepository.Setup(r => r.UpdateAsync(hazard)).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ManualActivateAsync(hazard, target);

        // Assert
        result.WasTriggered.Should().BeTrue();
        result.TotalDamage.Should().Be(4);
    }

    [Fact]
    public async Task ManualActivateAsync_NotManualTriggerHazard_DoesNotActivate()
    {
        // Arrange
        var target = CreateTestCombatant();
        var hazard = CreateTestHazard(
            trigger: TriggerType.Movement,
            state: HazardState.Dormant);

        // Act
        var result = await _sut.ManualActivateAsync(hazard, target);

        // Assert
        result.WasTriggered.Should().BeFalse();
    }

    [Fact]
    public async Task ManualActivateAsync_NotDormantHazard_DoesNotActivate()
    {
        // Arrange
        var target = CreateTestCombatant();
        var hazard = CreateTestHazard(
            trigger: TriggerType.ManualInteraction,
            state: HazardState.Cooldown);

        // Act
        var result = await _sut.ManualActivateAsync(hazard, target);

        // Assert
        result.WasTriggered.Should().BeFalse();
    }

    #endregion

    #region GetActiveHazards Tests

    [Fact]
    public async Task GetActiveHazardsAsync_ReturnsOnlyNonDestroyedHazards()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var activeHazard = CreateTestHazard(roomId: roomId, state: HazardState.Dormant);
        var cooldownHazard = CreateTestHazard(roomId: roomId, state: HazardState.Cooldown);
        var destroyedHazard = CreateTestHazard(roomId: roomId, state: HazardState.Destroyed);

        SetupRepositoryWithHazards(roomId, activeHazard, cooldownHazard, destroyedHazard);

        // Act
        var result = await _sut.GetActiveHazardsAsync(roomId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(activeHazard);
        result.Should().Contain(cooldownHazard);
        result.Should().NotContain(destroyedHazard);
    }

    #endregion

    #region Default Message Tests

    [Fact]
    public async Task ActivateHazard_MechanicalType_ReturnsGrindingMessage()
    {
        // Arrange
        var room = CreateTestRoom();
        var target = CreateTestCombatant();
        var hazard = CreateTestHazard(
            roomId: room.Id,
            name: "Gear Trap",
            hazardType: HazardType.Mechanical,
            trigger: TriggerType.Movement,
            state: HazardState.Dormant);

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        var results = await _sut.TriggerOnRoomEnterAsync(room, target);

        // Assert
        results[0].Message.Should().Contain("grinding");
    }

    [Fact]
    public async Task ActivateHazard_EnvironmentalType_ReturnsEruptsMessage()
    {
        // Arrange
        var room = CreateTestRoom();
        var target = CreateTestCombatant();
        var hazard = CreateTestHazard(
            roomId: room.Id,
            name: "Steam Vent",
            hazardType: HazardType.Environmental,
            trigger: TriggerType.Movement,
            state: HazardState.Dormant);

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        var results = await _sut.TriggerOnRoomEnterAsync(room, target);

        // Assert
        results[0].Message.Should().Contain("erupts");
    }

    [Fact]
    public async Task ActivateHazard_BiologicalType_ReturnsNoxiousMessage()
    {
        // Arrange
        var room = CreateTestRoom();
        var target = CreateTestCombatant();
        var hazard = CreateTestHazard(
            roomId: room.Id,
            name: "Spore Pod",
            hazardType: HazardType.Biological,
            trigger: TriggerType.Movement,
            state: HazardState.Dormant);

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        var results = await _sut.TriggerOnRoomEnterAsync(room, target);

        // Assert
        results[0].Message.Should().Contain("noxious");
    }

    [Fact]
    public async Task ActivateHazard_CustomTriggerMessage_UsesCustomMessage()
    {
        // Arrange
        var room = CreateTestRoom();
        var target = CreateTestCombatant();
        var hazard = CreateTestHazard(
            roomId: room.Id,
            trigger: TriggerType.Movement,
            state: HazardState.Dormant,
            triggerMessage: "The ancient trap springs to life!");

        SetupRepositoryWithHazards(room.Id, hazard);

        // Act
        var results = await _sut.TriggerOnRoomEnterAsync(room, target);

        // Assert
        results[0].Message.Should().Contain("ancient trap springs to life");
    }

    #endregion

    #region Test Helpers

    private static Room CreateTestRoom()
    {
        return new Room
        {
            Id = Guid.NewGuid(),
            Name = "Test Room",
            Description = "A test room.",
            BiomeType = BiomeType.Ruin
        };
    }

    private static Combatant CreateTestCombatant(
        string name = "Test Target",
        int currentHp = 50,
        int maxHp = 50,
        int armorSoak = 0)
    {
        return new Combatant
        {
            Id = Guid.NewGuid(),
            Name = name,
            CurrentHp = currentHp,
            MaxHp = maxHp,
            ArmorSoak = armorSoak,
            StatusEffects = new List<ActiveStatusEffect>(),
            Cooldowns = new Dictionary<Guid, int>()
        };
    }

    private static DynamicHazard CreateTestHazard(
        Guid? roomId = null,
        string name = "Test Hazard",
        HazardType hazardType = HazardType.Mechanical,
        HazardState state = HazardState.Dormant,
        TriggerType trigger = TriggerType.Movement,
        DamageType? requiredDamageType = null,
        int damageThreshold = 0,
        string effectScript = "",
        string triggerMessage = "",
        bool oneTimeUse = false,
        int maxCooldown = 2,
        int cooldownRemaining = 0)
    {
        return new DynamicHazard
        {
            Id = Guid.NewGuid(),
            RoomId = roomId ?? Guid.NewGuid(),
            Name = name,
            Description = "A test hazard.",
            ObjectType = ObjectType.Hazard,
            HazardType = hazardType,
            State = state,
            Trigger = trigger,
            RequiredDamageType = requiredDamageType,
            DamageThreshold = damageThreshold,
            EffectScript = effectScript,
            TriggerMessage = triggerMessage,
            OneTimeUse = oneTimeUse,
            MaxCooldown = maxCooldown,
            CooldownRemaining = cooldownRemaining
        };
    }

    private void SetupRepositoryWithHazards(Guid roomId, params DynamicHazard[] hazards)
    {
        var allObjects = hazards
            .Where(h => h.State != HazardState.Destroyed)
            .Cast<InteractableObject>()
            .ToList();

        _mockObjectRepository
            .Setup(r => r.GetByRoomIdAsync(roomId))
            .ReturnsAsync(allObjects);

        foreach (var hazard in hazards)
        {
            _mockObjectRepository
                .Setup(r => r.UpdateAsync(hazard))
                .Returns(Task.CompletedTask);
        }
    }

    #endregion
}
