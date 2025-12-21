using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Engine.Services;
using Xunit;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the ConditionService class (v0.3.3b).
/// Validates passive stat modifiers, tick effects, and condition lookups.
/// </summary>
public class ConditionServiceTests
{
    private readonly Mock<IRepository<AmbientCondition>> _mockConditionRepository;
    private readonly Mock<IRoomRepository> _mockRoomRepository;
    private readonly Mock<IDiceService> _mockDiceService;
    private readonly Mock<IStatusEffectService> _mockStatusEffectService;
    private readonly Mock<ILogger<ConditionService>> _mockLogger;
    private readonly Mock<ILogger<EffectScriptExecutor>> _mockScriptLogger;
    private readonly EffectScriptExecutor _scriptExecutor;
    private readonly ConditionService _sut;

    public ConditionServiceTests()
    {
        _mockConditionRepository = new Mock<IRepository<AmbientCondition>>();
        _mockRoomRepository = new Mock<IRoomRepository>();
        _mockDiceService = new Mock<IDiceService>();
        _mockStatusEffectService = new Mock<IStatusEffectService>();
        _mockLogger = new Mock<ILogger<ConditionService>>();
        _mockScriptLogger = new Mock<ILogger<EffectScriptExecutor>>();

        // Create EffectScriptExecutor with mocked dependencies
        _scriptExecutor = new EffectScriptExecutor(
            _mockDiceService.Object,
            _mockStatusEffectService.Object,
            _mockScriptLogger.Object);

        _sut = new ConditionService(
            _mockConditionRepository.Object,
            _mockRoomRepository.Object,
            _scriptExecutor,
            _mockDiceService.Object,
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

    #region GetStatModifiers Tests

    [Fact]
    public void GetStatModifiers_PsychicResonance_ReturnsMinusOneWill()
    {
        // Act
        var result = _sut.GetStatModifiers(ConditionType.PsychicResonance);

        // Assert
        result.Should().ContainKey(CharacterAttribute.Will);
        result[CharacterAttribute.Will].Should().Be(-1);
    }

    [Fact]
    public void GetStatModifiers_LowVisibility_ReturnsMinusTwoWits()
    {
        // Act
        var result = _sut.GetStatModifiers(ConditionType.LowVisibility);

        // Assert
        result.Should().ContainKey(CharacterAttribute.Wits);
        result[CharacterAttribute.Wits].Should().Be(-2);
    }

    [Fact]
    public void GetStatModifiers_BlightedGround_ReturnsBothPenalties()
    {
        // Act
        var result = _sut.GetStatModifiers(ConditionType.BlightedGround);

        // Assert
        result.Should().HaveCount(2);
        result[CharacterAttribute.Will].Should().Be(-1);
        result[CharacterAttribute.Wits].Should().Be(-1);
    }

    [Fact]
    public void GetStatModifiers_ToxicAtmosphere_ReturnsEmpty()
    {
        // Act
        var result = _sut.GetStatModifiers(ConditionType.ToxicAtmosphere);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetStatModifiers_DeepCold_ReturnsMinusOneFinesse()
    {
        // Act
        var result = _sut.GetStatModifiers(ConditionType.DeepCold);

        // Assert
        result.Should().ContainKey(CharacterAttribute.Finesse);
        result[CharacterAttribute.Finesse].Should().Be(-1);
    }

    [Fact]
    public void GetStatModifiers_ScorchingHeat_ReturnsMinusOneSturdiness()
    {
        // Act
        var result = _sut.GetStatModifiers(ConditionType.ScorchingHeat);

        // Assert
        result.Should().ContainKey(CharacterAttribute.Sturdiness);
        result[CharacterAttribute.Sturdiness].Should().Be(-1);
    }

    [Fact]
    public void GetStatModifiers_DreadPresence_ReturnsMinusTwoWill()
    {
        // Act
        var result = _sut.GetStatModifiers(ConditionType.DreadPresence);

        // Assert
        result.Should().ContainKey(CharacterAttribute.Will);
        result[CharacterAttribute.Will].Should().Be(-2);
    }

    #endregion

    #region ApplyPassiveModifiers Tests

    [Fact]
    public void ApplyPassiveModifiers_SetsWillModifier()
    {
        // Arrange
        var combatant = CreateTestCombatant();

        // Act
        _sut.ApplyPassiveModifiers(combatant, ConditionType.PsychicResonance);

        // Assert
        combatant.ConditionWillModifier.Should().Be(-1);
        combatant.ActiveCondition.Should().Be(ConditionType.PsychicResonance);
    }

    [Fact]
    public void ApplyPassiveModifiers_SetsWitsModifier()
    {
        // Arrange
        var combatant = CreateTestCombatant();

        // Act
        _sut.ApplyPassiveModifiers(combatant, ConditionType.LowVisibility);

        // Assert
        combatant.ConditionWitsModifier.Should().Be(-2);
        combatant.ActiveCondition.Should().Be(ConditionType.LowVisibility);
    }

    [Fact]
    public void ApplyPassiveModifiers_SetsFinesseModifier()
    {
        // Arrange
        var combatant = CreateTestCombatant();

        // Act
        _sut.ApplyPassiveModifiers(combatant, ConditionType.DeepCold);

        // Assert
        combatant.ConditionFinesseModifier.Should().Be(-1);
        combatant.ActiveCondition.Should().Be(ConditionType.DeepCold);
    }

    [Fact]
    public void ApplyPassiveModifiers_SetsSturdinessModifier()
    {
        // Arrange
        var combatant = CreateTestCombatant();

        // Act
        _sut.ApplyPassiveModifiers(combatant, ConditionType.ScorchingHeat);

        // Assert
        combatant.ConditionSturdinessModifier.Should().Be(-1);
        combatant.ActiveCondition.Should().Be(ConditionType.ScorchingHeat);
    }

    [Fact]
    public void ApplyPassiveModifiers_BlightedGround_SetsBothModifiers()
    {
        // Arrange
        var combatant = CreateTestCombatant();

        // Act
        _sut.ApplyPassiveModifiers(combatant, ConditionType.BlightedGround);

        // Assert
        combatant.ConditionWillModifier.Should().Be(-1);
        combatant.ConditionWitsModifier.Should().Be(-1);
        combatant.ActiveCondition.Should().Be(ConditionType.BlightedGround);
    }

    [Fact]
    public void ApplyPassiveModifiers_NullCondition_NoEffect()
    {
        // Arrange
        var combatant = CreateTestCombatant();

        // Act
        _sut.ApplyPassiveModifiers(combatant, null);

        // Assert
        combatant.ConditionWillModifier.Should().Be(0);
        combatant.ConditionWitsModifier.Should().Be(0);
        combatant.ConditionFinesseModifier.Should().Be(0);
        combatant.ConditionSturdinessModifier.Should().Be(0);
        combatant.ActiveCondition.Should().BeNull();
    }

    #endregion

    #region ProcessTurnTick Tests

    [Fact]
    public async Task ProcessTurnTick_DamageScript_AppliesDamage()
    {
        // Arrange
        var combatant = CreateTestCombatant(currentHp: 50);
        var condition = CreateTestCondition(
            ConditionType.ToxicAtmosphere,
            tickScript: "DAMAGE:Poison:1d4",
            tickChance: 1.0f);

        // Act
        var result = await _sut.ProcessTurnTickAsync(combatant, condition);

        // Assert
        result.WasApplied.Should().BeTrue();
        result.DamageDealt.Should().Be(4); // Dice mock returns 4
        combatant.CurrentHp.Should().Be(46); // 50 - 4
    }

    [Fact]
    public async Task ProcessTurnTick_StressScript_AppliesStress()
    {
        // Arrange
        var combatant = CreateTestCombatant(currentStress: 10);
        var condition = CreateTestCondition(
            ConditionType.PsychicResonance,
            tickScript: "STRESS:2",
            tickChance: 1.0f);

        // Act
        var result = await _sut.ProcessTurnTickAsync(combatant, condition);

        // Assert
        result.WasApplied.Should().BeTrue();
        result.StressApplied.Should().Be(2);
        combatant.CurrentStress.Should().Be(12);
    }

    [Fact]
    public async Task ProcessTurnTick_CorruptionScript_AppliesCorruption()
    {
        // Arrange
        var combatant = CreateTestCombatant(currentCorruption: 5);
        var condition = CreateTestCondition(
            ConditionType.BlightedGround,
            tickScript: "CORRUPTION:1",
            tickChance: 1.0f);

        // Act
        var result = await _sut.ProcessTurnTickAsync(combatant, condition);

        // Assert
        result.WasApplied.Should().BeTrue();
        result.CorruptionApplied.Should().Be(1);
        combatant.CurrentCorruption.Should().Be(6);
    }

    [Fact]
    public async Task ProcessTurnTick_EmptyScript_ReturnsNone()
    {
        // Arrange
        var combatant = CreateTestCombatant();
        var condition = CreateTestCondition(
            ConditionType.LowVisibility,
            tickScript: string.Empty,
            tickChance: 0f);

        // Act
        var result = await _sut.ProcessTurnTickAsync(combatant, condition);

        // Assert
        result.WasApplied.Should().BeFalse();
        result.Should().Be(ConditionTickResult.None);
    }

    [Fact]
    public async Task ProcessTurnTick_LowChance_SkipsOnFailedRoll()
    {
        // Arrange
        var combatant = CreateTestCombatant(currentHp: 50);
        var condition = CreateTestCondition(
            ConditionType.StaticField,
            tickScript: "DAMAGE:Lightning:1d6",
            tickChance: 0.25f);

        // Setup dice to roll high (above 25 threshold)
        _mockDiceService.Setup(d => d.RollSingle(100, It.IsAny<string>()))
            .Returns(80);

        // Act
        var result = await _sut.ProcessTurnTickAsync(combatant, condition);

        // Assert
        result.WasApplied.Should().BeFalse();
        combatant.CurrentHp.Should().Be(50); // No damage taken
    }

    [Fact]
    public async Task ProcessTurnTick_LowChance_AppliesOnSuccessfulRoll()
    {
        // Arrange
        var combatant = CreateTestCombatant(currentHp: 50);
        var condition = CreateTestCondition(
            ConditionType.StaticField,
            tickScript: "DAMAGE:Lightning:1d6",
            tickChance: 0.25f);

        // Setup dice to roll low (at or below 25 threshold)
        _mockDiceService.Setup(d => d.RollSingle(100, It.IsAny<string>()))
            .Returns(20);
        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(5);

        // Act
        var result = await _sut.ProcessTurnTickAsync(combatant, condition);

        // Assert
        result.WasApplied.Should().BeTrue();
        result.DamageDealt.Should().Be(5);
        combatant.CurrentHp.Should().Be(45);
    }

    [Fact]
    public async Task ProcessTurnTick_StressCappedAtMax()
    {
        // Arrange
        var combatant = CreateTestCombatant(currentStress: 99);
        var condition = CreateTestCondition(
            ConditionType.DreadPresence,
            tickScript: "STRESS:3",
            tickChance: 1.0f);

        // Act
        var result = await _sut.ProcessTurnTickAsync(combatant, condition);

        // Assert
        result.WasApplied.Should().BeTrue();
        result.StressApplied.Should().Be(1); // Only 1 applied, capped at 100
        combatant.CurrentStress.Should().Be(100);
    }

    [Fact]
    public async Task ProcessTurnTick_CorruptionCappedAtMax()
    {
        // Arrange
        var combatant = CreateTestCombatant(currentCorruption: 99);
        var condition = CreateTestCondition(
            ConditionType.BlightedGround,
            tickScript: "CORRUPTION:5",
            tickChance: 1.0f);

        // Act
        var result = await _sut.ProcessTurnTickAsync(combatant, condition);

        // Assert
        result.WasApplied.Should().BeTrue();
        result.CorruptionApplied.Should().Be(1); // Only 1 applied, capped at 100
        combatant.CurrentCorruption.Should().Be(100);
    }

    #endregion

    #region GetRoomCondition Tests

    [Fact]
    public async Task GetRoomCondition_NoCondition_ReturnsNull()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new Room { Id = roomId, ConditionId = null };

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId))
            .ReturnsAsync(room);

        // Act
        var result = await _sut.GetRoomConditionAsync(roomId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRoomCondition_WithCondition_ReturnsCondition()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var conditionId = Guid.NewGuid();
        var room = new Room { Id = roomId, ConditionId = conditionId };
        var condition = CreateTestCondition(ConditionType.ToxicAtmosphere, id: conditionId);

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId))
            .ReturnsAsync(room);
        _mockConditionRepository.Setup(r => r.GetByIdAsync(conditionId))
            .ReturnsAsync(condition);

        // Act
        var result = await _sut.GetRoomConditionAsync(roomId);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(ConditionType.ToxicAtmosphere);
    }

    [Fact]
    public async Task GetRoomCondition_RoomNotFound_ReturnsNull()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        _mockRoomRepository.Setup(r => r.GetByIdAsync(roomId))
            .ReturnsAsync((Room?)null);

        // Act
        var result = await _sut.GetRoomConditionAsync(roomId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Helper Methods

    private static Combatant CreateTestCombatant(
        int currentHp = 100,
        int maxHp = 100,
        int currentStress = 0,
        int currentCorruption = 0)
    {
        return new Combatant
        {
            Id = Guid.NewGuid(),
            Name = "Test Combatant",
            IsPlayer = true,
            CurrentHp = currentHp,
            MaxHp = maxHp,
            CurrentStamina = 50,
            MaxStamina = 50,
            CurrentStress = currentStress,
            MaxStress = 100,
            CurrentCorruption = currentCorruption,
            MaxCorruption = 100,
            ArmorSoak = 0
        };
    }

    private static AmbientCondition CreateTestCondition(
        ConditionType type,
        string tickScript = "",
        float tickChance = 1.0f,
        Guid? id = null)
    {
        return new AmbientCondition
        {
            Id = id ?? Guid.NewGuid(),
            Type = type,
            Name = type.ToString(),
            Description = $"Test {type} condition",
            Color = "grey",
            TickScript = tickScript,
            TickChance = tickChance
        };
    }

    #endregion
}
