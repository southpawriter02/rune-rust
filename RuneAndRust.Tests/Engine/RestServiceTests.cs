using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Engine.Services;
using Xunit;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for the RestService class.
/// Validates rest mechanics including supply consumption, recovery formulas, and status effects.
/// </summary>
public class RestServiceTests
{
    private readonly Mock<IInventoryService> _mockInventoryService;
    private readonly Mock<IAmbushService> _mockAmbushService;
    private readonly Mock<ILogger<RestService>> _mockLogger;
    private readonly RestService _sut;

    public RestServiceTests()
    {
        _mockInventoryService = new Mock<IInventoryService>();
        _mockAmbushService = new Mock<IAmbushService>();
        _mockLogger = new Mock<ILogger<RestService>>();
        _sut = new RestService(_mockInventoryService.Object, _mockAmbushService.Object, _mockLogger.Object);
    }

    private Character CreateTestCharacter(
        int currentHp = 50,
        int maxHp = 100,
        int currentStamina = 30,
        int maxStamina = 60,
        int psychicStress = 50,
        int sturdiness = 5,
        int will = 5)
    {
        return new Character
        {
            Name = "Test Hero",
            CurrentHP = currentHp,
            MaxHP = maxHp,
            CurrentStamina = currentStamina,
            MaxStamina = maxStamina,
            PsychicStress = psychicStress,
            Sturdiness = sturdiness,
            Will = will
        };
    }

    private InventoryItem CreateRationItem()
    {
        var item = new Item
        {
            Name = "Field Rations",
            Tags = new List<string> { "Ration" }
        };
        return new InventoryItem { Item = item, Quantity = 1 };
    }

    private InventoryItem CreateWaterItem()
    {
        var item = new Item
        {
            Name = "Waterskin",
            Tags = new List<string> { "Water" }
        };
        return new InventoryItem { Item = item, Quantity = 1 };
    }

    private Room CreateTestRoom(DangerLevel dangerLevel = DangerLevel.Hostile, BiomeType biome = BiomeType.Ruin)
    {
        return new Room
        {
            Name = "Test Room",
            DangerLevel = dangerLevel,
            BiomeType = biome
        };
    }

    #region Sanctuary Rest Tests

    [Fact]
    public async Task Sanctuary_FullRecovery_RestoresAllStats()
    {
        // Arrange
        var character = CreateTestCharacter(
            currentHp: 20,
            maxHp: 100,
            currentStamina: 10,
            maxStamina: 60,
            psychicStress: 80);

        // Act
        var result = await _sut.PerformRestAsync(character, RestType.Sanctuary);

        // Assert
        character.CurrentHP.Should().Be(100);
        character.CurrentStamina.Should().Be(60);
        character.PsychicStress.Should().Be(0);
        result.HpRecovered.Should().Be(80);
        result.StaminaRecovered.Should().Be(50);
        result.StressRecovered.Should().Be(80);
        result.SuppliesConsumed.Should().BeFalse();
        result.IsExhausted.Should().BeFalse();
    }

    [Fact]
    public async Task Sanctuary_RemovesExhausted_WhenPresent()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.AddStatusEffect(StatusEffectType.Exhausted);

        // Act
        await _sut.PerformRestAsync(character, RestType.Sanctuary);

        // Assert
        character.HasStatusEffect(StatusEffectType.Exhausted).Should().BeFalse();
    }

    [Fact]
    public async Task Sanctuary_DoesNotConsumeSupplies()
    {
        // Arrange
        var character = CreateTestCharacter();

        // Act
        var result = await _sut.PerformRestAsync(character, RestType.Sanctuary);

        // Assert
        result.SuppliesConsumed.Should().BeFalse();
        _mockInventoryService.Verify(
            x => x.RemoveItemAsync(It.IsAny<Character>(), It.IsAny<string>(), It.IsAny<int>()),
            Times.Never);
    }

    #endregion

    #region Wilderness Rest - With Supplies

    [Fact]
    public async Task Wilderness_WithSupplies_ConsumesRationAndWater()
    {
        // Arrange
        var character = CreateTestCharacter();
        var ration = CreateRationItem();
        var water = CreateWaterItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync(ration);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync(water);

        // Act
        var result = await _sut.PerformRestAsync(character, RestType.Wilderness);

        // Assert
        result.SuppliesConsumed.Should().BeTrue();
        _mockInventoryService.Verify(
            x => x.RemoveItemAsync(character, "Field Rations", 1), Times.Once);
        _mockInventoryService.Verify(
            x => x.RemoveItemAsync(character, "Waterskin", 1), Times.Once);
    }

    [Fact]
    public async Task Wilderness_WithSupplies_HpRecovery_UsesSturdiness()
    {
        // Arrange - Sturdiness 5: Recovery = 10 + (5*2) = 20
        var character = CreateTestCharacter(
            currentHp: 50,
            maxHp: 100,
            sturdiness: 5);
        var ration = CreateRationItem();
        var water = CreateWaterItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync(ration);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync(water);

        // Act
        var result = await _sut.PerformRestAsync(character, RestType.Wilderness);

        // Assert - Should recover 20 HP (10 base + 5*2)
        character.CurrentHP.Should().Be(70);
        result.HpRecovered.Should().Be(20);
    }

    [Fact]
    public async Task Wilderness_WithSupplies_HpRecovery_CapsAtMaxHp()
    {
        // Arrange - Nearly full HP, shouldn't exceed max
        var character = CreateTestCharacter(
            currentHp: 95,
            maxHp: 100,
            sturdiness: 5);
        var ration = CreateRationItem();
        var water = CreateWaterItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync(ration);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync(water);

        // Act
        var result = await _sut.PerformRestAsync(character, RestType.Wilderness);

        // Assert - Should cap at 100, not 115
        character.CurrentHP.Should().Be(100);
        result.HpRecovered.Should().Be(5);
    }

    [Fact]
    public async Task Wilderness_WithSupplies_StressRecovery_UsesWill()
    {
        // Arrange - Will 5: Stress reduction = 5 * 5 = 25
        var character = CreateTestCharacter(
            psychicStress: 50,
            will: 5);
        var ration = CreateRationItem();
        var water = CreateWaterItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync(ration);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync(water);

        // Act
        var result = await _sut.PerformRestAsync(character, RestType.Wilderness);

        // Assert - Should reduce stress by 25 (5*5)
        character.PsychicStress.Should().Be(25);
        result.StressRecovered.Should().Be(25);
    }

    [Fact]
    public async Task Wilderness_WithSupplies_StaminaRecovery_Full()
    {
        // Arrange
        var character = CreateTestCharacter(
            currentStamina: 10,
            maxStamina: 60);
        var ration = CreateRationItem();
        var water = CreateWaterItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync(ration);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync(water);

        // Act
        await _sut.PerformRestAsync(character, RestType.Wilderness);

        // Assert - Stamina should be fully restored
        character.CurrentStamina.Should().Be(60);
    }

    #endregion

    #region Wilderness Rest - Without Supplies

    [Fact]
    public async Task Wilderness_WithoutSupplies_AppliesExhausted()
    {
        // Arrange
        var character = CreateTestCharacter();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync((InventoryItem?)null);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _sut.PerformRestAsync(character, RestType.Wilderness);

        // Assert
        result.IsExhausted.Should().BeTrue();
        character.HasStatusEffect(StatusEffectType.Exhausted).Should().BeTrue();
    }

    [Fact]
    public async Task Wilderness_WithoutSupplies_HalvesHpRecovery()
    {
        // Arrange - Sturdiness 5: Normal = 20, Exhausted = 10
        var character = CreateTestCharacter(
            currentHp: 50,
            maxHp: 100,
            sturdiness: 5);

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync((InventoryItem?)null);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _sut.PerformRestAsync(character, RestType.Wilderness);

        // Assert - Should recover 10 HP (20 / 2)
        character.CurrentHP.Should().Be(60);
        result.HpRecovered.Should().Be(10);
    }

    [Fact]
    public async Task Wilderness_WithoutSupplies_HalvesStaminaRecovery()
    {
        // Arrange
        var character = CreateTestCharacter(
            currentStamina: 10,
            maxStamina: 60);

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync((InventoryItem?)null);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        await _sut.PerformRestAsync(character, RestType.Wilderness);

        // Assert - Stamina should be half of max
        character.CurrentStamina.Should().Be(30);
    }

    [Fact]
    public async Task Wilderness_WithoutSupplies_NoStressRecovery()
    {
        // Arrange
        var character = CreateTestCharacter(
            psychicStress: 50,
            will: 5);

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync((InventoryItem?)null);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _sut.PerformRestAsync(character, RestType.Wilderness);

        // Assert - No stress recovery when exhausted
        character.PsychicStress.Should().Be(50);
        result.StressRecovered.Should().Be(0);
    }

    [Fact]
    public async Task Wilderness_MissingRationOnly_AppliesExhausted()
    {
        // Arrange - Has water but no ration
        var character = CreateTestCharacter();
        var water = CreateWaterItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync((InventoryItem?)null);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync(water);

        // Act
        var result = await _sut.PerformRestAsync(character, RestType.Wilderness);

        // Assert
        result.IsExhausted.Should().BeTrue();
        character.HasStatusEffect(StatusEffectType.Exhausted).Should().BeTrue();
    }

    [Fact]
    public async Task Wilderness_MissingWaterOnly_AppliesExhausted()
    {
        // Arrange - Has ration but no water
        var character = CreateTestCharacter();
        var ration = CreateRationItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync(ration);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _sut.PerformRestAsync(character, RestType.Wilderness);

        // Assert
        result.IsExhausted.Should().BeTrue();
        character.HasStatusEffect(StatusEffectType.Exhausted).Should().BeTrue();
    }

    #endregion

    #region HasRequiredSuppliesAsync Tests

    [Fact]
    public async Task HasRequiredSupplies_BothPresent_ReturnsTrue()
    {
        // Arrange
        var character = CreateTestCharacter();
        var ration = CreateRationItem();
        var water = CreateWaterItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync(ration);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync(water);

        // Act
        var result = await _sut.HasRequiredSuppliesAsync(character);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasRequiredSupplies_MissingRation_ReturnsFalse()
    {
        // Arrange
        var character = CreateTestCharacter();
        var water = CreateWaterItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync((InventoryItem?)null);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync(water);

        // Act
        var result = await _sut.HasRequiredSuppliesAsync(character);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasRequiredSupplies_MissingWater_ReturnsFalse()
    {
        // Arrange
        var character = CreateTestCharacter();
        var ration = CreateRationItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync(ration);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _sut.HasRequiredSuppliesAsync(character);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Wilderness_HighSturdiness_LargeHpRecovery()
    {
        // Arrange - Sturdiness 10: Recovery = 10 + (10*2) = 30
        var character = CreateTestCharacter(
            currentHp: 50,
            maxHp: 100,
            sturdiness: 10);
        var ration = CreateRationItem();
        var water = CreateWaterItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync(ration);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync(water);

        // Act
        var result = await _sut.PerformRestAsync(character, RestType.Wilderness);

        // Assert
        character.CurrentHP.Should().Be(80);
        result.HpRecovered.Should().Be(30);
    }

    [Fact]
    public async Task Wilderness_StressAtZero_StaysAtZero()
    {
        // Arrange
        var character = CreateTestCharacter(
            psychicStress: 0,
            will: 5);
        var ration = CreateRationItem();
        var water = CreateWaterItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync(ration);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync(water);

        // Act
        var result = await _sut.PerformRestAsync(character, RestType.Wilderness);

        // Assert - Stress should stay at 0, not go negative
        character.PsychicStress.Should().Be(0);
        result.StressRecovered.Should().Be(0);
    }

    [Fact]
    public async Task Wilderness_AlreadyExhausted_StillAppliesPenalties()
    {
        // Arrange - Character already exhausted
        var character = CreateTestCharacter(
            currentHp: 50,
            maxHp: 100,
            sturdiness: 5);
        character.AddStatusEffect(StatusEffectType.Exhausted);
        var ration = CreateRationItem();
        var water = CreateWaterItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync(ration);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync(water);

        // Act
        var result = await _sut.PerformRestAsync(character, RestType.Wilderness);

        // Assert - Still gets halved recovery due to pre-existing exhaustion
        character.CurrentHP.Should().Be(60); // 10 HP recovery (halved from 20)
        result.IsExhausted.Should().BeTrue();
    }

    #endregion

    #region Ambush Integration Tests (v0.3.2b)

    [Fact]
    public async Task Wilderness_WithRoom_CallsAmbushService()
    {
        // Arrange
        var character = CreateTestCharacter();
        var room = CreateTestRoom(DangerLevel.Hostile);
        var ration = CreateRationItem();
        var water = CreateWaterItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync(ration);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync(water);
        _mockAmbushService
            .Setup(x => x.CalculateAmbushAsync(character, room))
            .ReturnsAsync(new AmbushResult(
                IsAmbush: false,
                Message: "Safe passage.",
                BaseRiskPercent: 30,
                MitigationPercent: 10,
                FinalRiskPercent: 20,
                RollValue: 50,
                MitigationSuccesses: 2));

        // Act
        await _sut.PerformRestAsync(character, RestType.Wilderness, room);

        // Assert
        _mockAmbushService.Verify(
            x => x.CalculateAmbushAsync(character, room), Times.Once);
    }

    [Fact]
    public async Task Wilderness_AmbushTriggered_ReturnsWasAmbushed()
    {
        // Arrange
        var character = CreateTestCharacter();
        var room = CreateTestRoom(DangerLevel.Hostile);
        var ration = CreateRationItem();
        var water = CreateWaterItem();
        var encounter = new EncounterDefinition(
            TemplateIds: new List<string> { "bst_vargr_01" },
            Budget: 80f,
            IsAmbush: true);

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync(ration);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync(water);
        _mockAmbushService
            .Setup(x => x.CalculateAmbushAsync(character, room))
            .ReturnsAsync(new AmbushResult(
                IsAmbush: true,
                Message: "You are not alone!",
                BaseRiskPercent: 30,
                MitigationPercent: 0,
                FinalRiskPercent: 30,
                RollValue: 15,
                MitigationSuccesses: 0,
                Encounter: encounter));

        // Act
        var result = await _sut.PerformRestAsync(character, RestType.Wilderness, room);

        // Assert
        result.WasAmbushed.Should().BeTrue();
        result.AmbushDetails.Should().NotBeNull();
        result.AmbushDetails!.Encounter.Should().NotBeNull();
        result.AmbushDetails.Encounter!.TemplateIds.Should().Contain("bst_vargr_01");
    }

    [Fact]
    public async Task Wilderness_AmbushTriggered_ConsumesSupplies()
    {
        // Arrange
        var character = CreateTestCharacter();
        var room = CreateTestRoom(DangerLevel.Hostile);
        var ration = CreateRationItem();
        var water = CreateWaterItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync(ration);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync(water);
        _mockAmbushService
            .Setup(x => x.CalculateAmbushAsync(character, room))
            .ReturnsAsync(new AmbushResult(
                IsAmbush: true,
                Message: "You are not alone!",
                BaseRiskPercent: 30,
                MitigationPercent: 0,
                FinalRiskPercent: 30,
                RollValue: 15,
                MitigationSuccesses: 0,
                Encounter: new EncounterDefinition(
                    TemplateIds: new List<string> { "bst_vargr_01" },
                    Budget: 80f,
                    IsAmbush: true)));

        // Act
        var result = await _sut.PerformRestAsync(character, RestType.Wilderness, room);

        // Assert - Supplies still consumed even when ambushed
        result.SuppliesConsumed.Should().BeTrue();
        _mockInventoryService.Verify(
            x => x.RemoveItemAsync(character, "Field Rations", 1), Times.Once);
        _mockInventoryService.Verify(
            x => x.RemoveItemAsync(character, "Waterskin", 1), Times.Once);
    }

    [Fact]
    public async Task Wilderness_AmbushTriggered_AppliesDisoriented()
    {
        // Arrange
        var character = CreateTestCharacter();
        var room = CreateTestRoom(DangerLevel.Hostile);
        var ration = CreateRationItem();
        var water = CreateWaterItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync(ration);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync(water);
        _mockAmbushService
            .Setup(x => x.CalculateAmbushAsync(character, room))
            .ReturnsAsync(new AmbushResult(
                IsAmbush: true,
                Message: "You are not alone!",
                BaseRiskPercent: 30,
                MitigationPercent: 0,
                FinalRiskPercent: 30,
                RollValue: 15,
                MitigationSuccesses: 0,
                Encounter: new EncounterDefinition(
                    TemplateIds: new List<string> { "bst_vargr_01" },
                    Budget: 80f,
                    IsAmbush: true)));

        // Act
        await _sut.PerformRestAsync(character, RestType.Wilderness, room);

        // Assert
        character.HasStatusEffect(StatusEffectType.Disoriented).Should().BeTrue();
    }

    [Fact]
    public async Task Wilderness_NoAmbush_NoDisoriented()
    {
        // Arrange
        var character = CreateTestCharacter();
        var room = CreateTestRoom(DangerLevel.Hostile);
        var ration = CreateRationItem();
        var water = CreateWaterItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync(ration);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync(water);
        _mockAmbushService
            .Setup(x => x.CalculateAmbushAsync(character, room))
            .ReturnsAsync(new AmbushResult(
                IsAmbush: false,
                Message: "Safe passage.",
                BaseRiskPercent: 30,
                MitigationPercent: 20,
                FinalRiskPercent: 10,
                RollValue: 50,
                MitigationSuccesses: 4));

        // Act
        await _sut.PerformRestAsync(character, RestType.Wilderness, room);

        // Assert
        character.HasStatusEffect(StatusEffectType.Disoriented).Should().BeFalse();
    }

    [Fact]
    public async Task Wilderness_AmbushTriggered_NoRecovery()
    {
        // Arrange
        var character = CreateTestCharacter(
            currentHp: 50,
            maxHp: 100,
            currentStamina: 30,
            maxStamina: 60);
        var room = CreateTestRoom(DangerLevel.Hostile);
        var ration = CreateRationItem();
        var water = CreateWaterItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync(ration);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync(water);
        _mockAmbushService
            .Setup(x => x.CalculateAmbushAsync(character, room))
            .ReturnsAsync(new AmbushResult(
                IsAmbush: true,
                Message: "You are not alone!",
                BaseRiskPercent: 30,
                MitigationPercent: 0,
                FinalRiskPercent: 30,
                RollValue: 15,
                MitigationSuccesses: 0,
                Encounter: new EncounterDefinition(
                    TemplateIds: new List<string> { "bst_vargr_01" },
                    Budget: 80f,
                    IsAmbush: true)));

        // Act
        var result = await _sut.PerformRestAsync(character, RestType.Wilderness, room);

        // Assert - No HP or Stamina recovery when ambushed
        result.HpRecovered.Should().Be(0);
        result.StaminaRecovered.Should().Be(0);
        character.CurrentHP.Should().Be(50);
        character.CurrentStamina.Should().Be(30);
    }

    [Fact]
    public async Task Wilderness_AmbushTriggered_StressNotReduced()
    {
        // Arrange
        var character = CreateTestCharacter(
            psychicStress: 50,
            will: 5);
        var room = CreateTestRoom(DangerLevel.Hostile);
        var ration = CreateRationItem();
        var water = CreateWaterItem();

        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Ration"))
            .ReturnsAsync(ration);
        _mockInventoryService
            .Setup(x => x.FindItemByTagAsync(character, "Water"))
            .ReturnsAsync(water);
        _mockAmbushService
            .Setup(x => x.CalculateAmbushAsync(character, room))
            .ReturnsAsync(new AmbushResult(
                IsAmbush: true,
                Message: "You are not alone!",
                BaseRiskPercent: 30,
                MitigationPercent: 0,
                FinalRiskPercent: 30,
                RollValue: 15,
                MitigationSuccesses: 0,
                Encounter: new EncounterDefinition(
                    TemplateIds: new List<string> { "bst_vargr_01" },
                    Budget: 80f,
                    IsAmbush: true)));

        // Act
        var result = await _sut.PerformRestAsync(character, RestType.Wilderness, room);

        // Assert - No stress recovery when ambushed
        result.StressRecovered.Should().Be(0);
        character.PsychicStress.Should().Be(50);
    }

    [Fact]
    public async Task Sanctuary_SkipsAmbushCheck()
    {
        // Arrange
        var character = CreateTestCharacter();
        var room = CreateTestRoom(DangerLevel.Hostile);

        // Act
        await _sut.PerformRestAsync(character, RestType.Sanctuary, room);

        // Assert - Sanctuary rest should not call ambush service
        _mockAmbushService.Verify(
            x => x.CalculateAmbushAsync(It.IsAny<Character>(), It.IsAny<Room>()),
            Times.Never);
    }

    #endregion
}
