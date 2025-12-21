using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the EffectScriptExecutor utility class (v0.3.3a).
/// Validates parsing and execution of effect scripts used by AbilityService and HazardService.
/// </summary>
public class EffectScriptExecutorTests
{
    private readonly Mock<IDiceService> _mockDiceService;
    private readonly Mock<IStatusEffectService> _mockStatusEffectService;
    private readonly Mock<ILogger<EffectScriptExecutor>> _mockLogger;
    private readonly EffectScriptExecutor _sut;

    public EffectScriptExecutorTests()
    {
        _mockDiceService = new Mock<IDiceService>();
        _mockStatusEffectService = new Mock<IStatusEffectService>();
        _mockLogger = new Mock<ILogger<EffectScriptExecutor>>();

        _sut = new EffectScriptExecutor(
            _mockDiceService.Object,
            _mockStatusEffectService.Object,
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

    #region Empty Script Tests

    [Fact]
    public void Execute_EmptyScript_ReturnsEmptyResult()
    {
        // Arrange
        var target = CreateTestCombatant();
        var script = string.Empty;

        // Act
        var result = _sut.Execute(script, target, "Test Source");

        // Assert
        result.TotalDamage.Should().Be(0);
        result.TotalHealing.Should().Be(0);
        result.StatusesApplied.Should().BeEmpty();
        result.Narrative.Should().BeEmpty();
    }

    [Fact]
    public void Execute_NullScript_ReturnsEmptyResult()
    {
        // Arrange
        var target = CreateTestCombatant();
        string? script = null;

        // Act
        var result = _sut.Execute(script!, target, "Test Source");

        // Assert
        result.TotalDamage.Should().Be(0);
        result.TotalHealing.Should().Be(0);
        result.StatusesApplied.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WhitespaceScript_ReturnsEmptyResult()
    {
        // Arrange
        var target = CreateTestCombatant();
        var script = "   ";

        // Act
        var result = _sut.Execute(script, target, "Test Source");

        // Assert
        result.TotalDamage.Should().Be(0);
        result.TotalHealing.Should().Be(0);
        result.StatusesApplied.Should().BeEmpty();
    }

    #endregion

    #region DAMAGE Command Tests

    [Fact]
    public void Execute_DamageCommand_RollsDiceAndAppliesDamage()
    {
        // Arrange
        var target = CreateTestCombatant(currentHp: 50);
        var script = "DAMAGE:Fire:2d6";
        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>())).Returns(4); // 2d6 = 8 total

        // Act
        var result = _sut.Execute(script, target, "Steam Vent");

        // Assert
        result.TotalDamage.Should().Be(8);
        target.CurrentHp.Should().Be(42); // 50 - 8 = 42
        _mockDiceService.Verify(d => d.RollSingle(6, It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public void Execute_PhysicalDamage_AppliesArmorSoak()
    {
        // Arrange
        var target = CreateTestCombatant(currentHp: 50, armorSoak: 3);
        var script = "DAMAGE:Physical:1d8";
        _mockDiceService.Setup(d => d.RollSingle(8, It.IsAny<string>())).Returns(6);

        // Act
        var result = _sut.Execute(script, target, "Pressure Plate");

        // Assert
        result.TotalDamage.Should().Be(3); // 6 - 3 = 3
        target.CurrentHp.Should().Be(47); // 50 - 3 = 47
    }

    [Fact]
    public void Execute_PhysicalDamage_SoakCapsAtZero()
    {
        // Arrange
        var target = CreateTestCombatant(currentHp: 50, armorSoak: 10);
        var script = "DAMAGE:Physical:1d8";
        _mockDiceService.Setup(d => d.RollSingle(8, It.IsAny<string>())).Returns(6);

        // Act
        var result = _sut.Execute(script, target, "Pressure Plate");

        // Assert
        result.TotalDamage.Should().Be(0); // 6 - 10 = capped at 0
        target.CurrentHp.Should().Be(50);
    }

    [Fact]
    public void Execute_FireDamage_BypassesArmorSoak()
    {
        // Arrange
        var target = CreateTestCombatant(currentHp: 50, armorSoak: 10);
        var script = "DAMAGE:Fire:1d6";
        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>())).Returns(5);

        // Act
        var result = _sut.Execute(script, target, "Lava Vent");

        // Assert
        result.TotalDamage.Should().Be(5); // Armor not applied
        target.CurrentHp.Should().Be(45);
    }

    [Fact]
    public void Execute_DamageCommand_AppliesVulnerabilityMultiplier()
    {
        // Arrange
        var target = CreateTestCombatant(currentHp: 50);
        var script = "DAMAGE:Fire:1d6";
        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>())).Returns(4);
        _mockStatusEffectService.Setup(s => s.GetDamageMultiplier(target)).Returns(1.5f); // 50% extra damage

        // Act
        var result = _sut.Execute(script, target, "Fire Trap");

        // Assert
        result.TotalDamage.Should().Be(6); // 4 * 1.5 = 6
        target.CurrentHp.Should().Be(44);
    }

    [Fact]
    public void Execute_DamageCommand_IncludesNarrative()
    {
        // Arrange
        var target = CreateTestCombatant();
        var script = "DAMAGE:Ice:1d4";
        _mockDiceService.Setup(d => d.RollSingle(4, It.IsAny<string>())).Returns(3);

        // Act
        var result = _sut.Execute(script, target, "Frost Vent");

        // Assert
        result.Narrative.Should().Contain("ice damage");
    }

    [Fact]
    public void Execute_DamageCommand_MissingParameters_ReturnsZeroDamage()
    {
        // Arrange
        var target = CreateTestCombatant(currentHp: 50);
        var script = "DAMAGE:Fire"; // Missing dice notation

        // Act
        var result = _sut.Execute(script, target, "Invalid Hazard");

        // Assert
        result.TotalDamage.Should().Be(0);
        target.CurrentHp.Should().Be(50); // No damage applied
    }

    [Fact]
    public void Execute_DamageCommand_InvalidDiceNotation_ReturnsZeroDamage()
    {
        // Arrange
        var target = CreateTestCombatant(currentHp: 50);
        var script = "DAMAGE:Fire:invalid";

        // Act
        var result = _sut.Execute(script, target, "Invalid Hazard");

        // Assert
        result.TotalDamage.Should().Be(0);
        target.CurrentHp.Should().Be(50);
    }

    #endregion

    #region HEAL Command Tests

    [Fact]
    public void Execute_HealCommand_FlatValue_RestoresHP()
    {
        // Arrange
        var target = CreateTestCombatant(currentHp: 30, maxHp: 50);
        var script = "HEAL:15";

        // Act
        var result = _sut.Execute(script, target, "Healing Fountain");

        // Assert
        result.TotalHealing.Should().Be(15);
        target.CurrentHp.Should().Be(45);
    }

    [Fact]
    public void Execute_HealCommand_ClampsToMaxHP()
    {
        // Arrange
        var target = CreateTestCombatant(currentHp: 45, maxHp: 50);
        var script = "HEAL:20";

        // Act
        var result = _sut.Execute(script, target, "Healing Fountain");

        // Assert
        result.TotalHealing.Should().Be(5); // Only heals up to max
        target.CurrentHp.Should().Be(50);
    }

    [Fact]
    public void Execute_HealCommand_AtFullHP_ReturnsZeroHealing()
    {
        // Arrange
        var target = CreateTestCombatant(currentHp: 50, maxHp: 50);
        var script = "HEAL:10";

        // Act
        var result = _sut.Execute(script, target, "Healing Fountain");

        // Assert
        result.TotalHealing.Should().Be(0);
        result.Narrative.Should().Contain("already full");
    }

    [Fact]
    public void Execute_HealCommand_DiceNotation_RollsAndHeals()
    {
        // Arrange
        var target = CreateTestCombatant(currentHp: 30, maxHp: 50);
        var script = "HEAL:2d4";
        _mockDiceService.Setup(d => d.RollSingle(4, It.IsAny<string>())).Returns(3); // 2d4 = 6

        // Act
        var result = _sut.Execute(script, target, "Regeneration Pool");

        // Assert
        result.TotalHealing.Should().Be(6);
        target.CurrentHp.Should().Be(36);
        _mockDiceService.Verify(d => d.RollSingle(4, It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public void Execute_HealCommand_MissingAmount_ReturnsZeroHealing()
    {
        // Arrange
        var target = CreateTestCombatant(currentHp: 30, maxHp: 50);
        var script = "HEAL";

        // Act
        var result = _sut.Execute(script, target, "Invalid Source");

        // Assert
        result.TotalHealing.Should().Be(0);
        target.CurrentHp.Should().Be(30);
    }

    #endregion

    #region STATUS Command Tests

    [Fact]
    public void Execute_StatusCommand_AppliesEffect()
    {
        // Arrange
        var target = CreateTestCombatant();
        var script = "STATUS:Bleeding:3";
        var sourceId = Guid.NewGuid();

        // Act
        var result = _sut.Execute(script, target, "Spike Trap", sourceId);

        // Assert
        result.StatusesApplied.Should().Contain("Bleeding");
        _mockStatusEffectService.Verify(
            s => s.ApplyEffect(target, StatusEffectType.Bleeding, 3, sourceId),
            Times.Once);
    }

    [Fact]
    public void Execute_StatusCommand_WithStacks_AppliesMultipleTimes()
    {
        // Arrange
        var target = CreateTestCombatant();
        var script = "STATUS:Bleeding:2:3"; // 3 stacks, 2 duration each
        var sourceId = Guid.NewGuid();

        // Act
        var result = _sut.Execute(script, target, "Spiked Trap", sourceId);

        // Assert
        result.StatusesApplied.Should().Contain("Bleeding");
        _mockStatusEffectService.Verify(
            s => s.ApplyEffect(target, StatusEffectType.Bleeding, 2, sourceId),
            Times.Exactly(3));
    }

    [Fact]
    public void Execute_StatusCommand_UnknownStatusType_DoesNotApply()
    {
        // Arrange
        var target = CreateTestCombatant();
        var script = "STATUS:NotARealStatus:5";

        // Act
        var result = _sut.Execute(script, target, "Unknown Trap");

        // Assert
        result.StatusesApplied.Should().BeEmpty();
    }

    [Fact]
    public void Execute_StatusCommand_MissingDuration_DoesNotApply()
    {
        // Arrange
        var target = CreateTestCombatant();
        var script = "STATUS:Burning";

        // Act
        var result = _sut.Execute(script, target, "Invalid Source");

        // Assert
        result.StatusesApplied.Should().BeEmpty();
    }

    #endregion

    #region Multiple Commands Tests

    [Fact]
    public void Execute_MultipleCommands_AllExecute()
    {
        // Arrange
        var target = CreateTestCombatant(currentHp: 50, maxHp: 60);
        var script = "DAMAGE:Fire:1d6;STATUS:Poisoned:2;HEAL:5";
        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>())).Returns(4);

        // Act
        var result = _sut.Execute(script, target, "Combo Trap");

        // Assert
        result.TotalDamage.Should().Be(4);
        result.TotalHealing.Should().Be(5);
        result.StatusesApplied.Should().Contain("Poisoned");
        target.CurrentHp.Should().Be(51); // 50 - 4 + 5 = 51
    }

    [Fact]
    public void Execute_MultipleCommands_AggregatesDamage()
    {
        // Arrange
        var target = CreateTestCombatant(currentHp: 50);
        var script = "DAMAGE:Fire:1d6;DAMAGE:Ice:1d4";
        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>())).Returns(4);
        _mockDiceService.Setup(d => d.RollSingle(4, It.IsAny<string>())).Returns(3);

        // Act
        var result = _sut.Execute(script, target, "Elemental Trap");

        // Assert
        result.TotalDamage.Should().Be(7); // 4 + 3
        target.CurrentHp.Should().Be(43); // 50 - 7
    }

    [Fact]
    public void Execute_MultipleCommands_CombinesNarratives()
    {
        // Arrange
        var target = CreateTestCombatant(currentHp: 50);
        var script = "DAMAGE:Fire:1d6;HEAL:5";
        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>())).Returns(4);

        // Act
        var result = _sut.Execute(script, target, "Mixed Trap");

        // Assert
        result.Narrative.Should().Contain("fire damage");
        result.Narrative.Should().Contain("HP");
    }

    #endregion

    #region Unknown Command Tests

    [Fact]
    public void Execute_UnknownCommand_SkippedAndLogged()
    {
        // Arrange
        var target = CreateTestCombatant(currentHp: 40, maxHp: 50);
        var script = "DAMAGE:Fire:1d6;TELEPORT:Room1;HEAL:5";
        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>())).Returns(4);

        // Act
        var result = _sut.Execute(script, target, "Mixed Trap");

        // Assert
        result.TotalDamage.Should().Be(4); // DAMAGE still works
        result.TotalHealing.Should().Be(5); // HEAL still works
        // TELEPORT is skipped silently
    }

    #endregion

    #region Test Helpers

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

    #endregion
}
