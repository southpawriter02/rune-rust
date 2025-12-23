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
/// Tests for the AbilityService class.
/// Validates ability execution, cooldown management, and EffectScript parsing.
/// </summary>
public class AbilityServiceTests
{
    private readonly Mock<IResourceService> _mockResourceService;
    private readonly Mock<IStatusEffectService> _mockStatusEffectService;
    private readonly Mock<IDiceService> _mockDiceService;
    private readonly Mock<ILogger<AbilityService>> _mockLogger;
    private readonly Mock<ILogger<EffectScriptExecutor>> _mockScriptLogger;
    private readonly EffectScriptExecutor _scriptExecutor;
    private readonly AbilityService _sut;

    public AbilityServiceTests()
    {
        _mockResourceService = new Mock<IResourceService>();
        _mockStatusEffectService = new Mock<IStatusEffectService>();
        _mockDiceService = new Mock<IDiceService>();
        _mockLogger = new Mock<ILogger<AbilityService>>();
        _mockScriptLogger = new Mock<ILogger<EffectScriptExecutor>>();

        // Create EffectScriptExecutor with mocked dependencies (v0.3.3a refactor)
        _scriptExecutor = new EffectScriptExecutor(
            _mockDiceService.Object,
            _mockStatusEffectService.Object,
            _mockScriptLogger.Object);

        _sut = new AbilityService(
            _mockResourceService.Object,
            _mockStatusEffectService.Object,
            _scriptExecutor,
            _mockLogger.Object);

        // Default setup: resources are always affordable
        _mockResourceService.Setup(r => r.CanAfford(It.IsAny<Combatant>(), It.IsAny<ResourceType>(), It.IsAny<int>()))
            .Returns(true);
        _mockResourceService.Setup(r => r.GetCurrent(It.IsAny<Combatant>(), It.IsAny<ResourceType>()))
            .Returns(100);

        // Default dice roll
        _mockDiceService.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(4);

        // Default status effect modifiers
        _mockStatusEffectService.Setup(s => s.GetDamageMultiplier(It.IsAny<Combatant>()))
            .Returns(1.0f);
        _mockStatusEffectService.Setup(s => s.GetSoakModifier(It.IsAny<Combatant>()))
            .Returns(0);
    }

    #region CanUse Tests

    [Fact]
    public void CanUse_OnCooldown_ReturnsFalse()
    {
        // Arrange
        var user = CreateTestCombatant();
        var ability = CreateTestAbility();
        user.Cooldowns[ability.Id] = 2; // 2 turns remaining

        // Act
        var result = _sut.CanUse(user, ability);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanUse_InsufficientStamina_ReturnsFalse()
    {
        // Arrange
        var user = CreateTestCombatant();
        var ability = CreateTestAbility(staminaCost: 50);

        _mockResourceService.Setup(r => r.CanAfford(user, ResourceType.Stamina, 50))
            .Returns(false);

        // Act
        var result = _sut.CanUse(user, ability);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanUse_InsufficientAether_ReturnsFalse()
    {
        // Arrange
        var user = CreateTestCombatant();
        var ability = CreateTestAbility(aetherCost: 30);

        _mockResourceService.Setup(r => r.CanAfford(user, ResourceType.Aether, 30))
            .Returns(false);

        // Act
        var result = _sut.CanUse(user, ability);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanUse_AllConditionsMet_ReturnsTrue()
    {
        // Arrange
        var user = CreateTestCombatant();
        var ability = CreateTestAbility(staminaCost: 25, aetherCost: 10);

        // Act
        var result = _sut.CanUse(user, ability);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanUse_ZeroCooldownRemaining_ReturnsTrue()
    {
        // Arrange
        var user = CreateTestCombatant();
        var ability = CreateTestAbility();
        user.Cooldowns[ability.Id] = 0; // Expired cooldown

        // Act
        var result = _sut.CanUse(user, ability);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanUse_NoCosts_ReturnsTrue()
    {
        // Arrange
        var user = CreateTestCombatant();
        var ability = CreateTestAbility(staminaCost: 0, aetherCost: 0);

        // Act
        var result = _sut.CanUse(user, ability);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Execute Tests - Resource Handling

    [Fact]
    public void Execute_DeductsStaminaCost()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        var ability = CreateTestAbility(staminaCost: 25);

        // Act
        _sut.Execute(user, target, ability);

        // Assert
        _mockResourceService.Verify(r => r.Deduct(user, ResourceType.Stamina, 25), Times.Once);
    }

    [Fact]
    public void Execute_DeductsAetherCost()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        var ability = CreateTestAbility(aetherCost: 15);

        // Act
        _sut.Execute(user, target, ability);

        // Assert
        _mockResourceService.Verify(r => r.Deduct(user, ResourceType.Aether, 15), Times.Once);
    }

    [Fact]
    public void Execute_SetsCooldown()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        var ability = CreateTestAbility(cooldownTurns: 3);

        // Act
        _sut.Execute(user, target, ability);

        // Assert
        user.Cooldowns.Should().ContainKey(ability.Id);
        user.Cooldowns[ability.Id].Should().Be(3);
    }

    [Fact]
    public void Execute_CannotUse_ReturnsFailure()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        var ability = CreateTestAbility(staminaCost: 50);
        _mockResourceService.Setup(r => r.CanAfford(user, ResourceType.Stamina, 50))
            .Returns(false);

        // Act
        var result = _sut.Execute(user, target, ability);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("stamina");
    }

    #endregion

    #region Execute Tests - DAMAGE Command

    [Fact]
    public void Execute_DamageScript_RollsDice()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        var ability = CreateTestAbility(effectScript: "DAMAGE:Physical:2d6");

        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(4);

        // Act
        _sut.Execute(user, target, ability);

        // Assert - 2d6 means RollSingle called twice
        _mockDiceService.Verify(d => d.RollSingle(6, It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public void Execute_DamageScript_AppliesDamageToTarget()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        target.CurrentHp = 50;
        var ability = CreateTestAbility(effectScript: "DAMAGE:Physical:2d6");

        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(5); // 2d6 = 10 total

        // Act
        _sut.Execute(user, target, ability);

        // Assert - 10 damage applied (no soak for this test)
        target.CurrentHp.Should().Be(40);
    }

    [Fact]
    public void Execute_PhysicalDamage_AppliesArmorSoak()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        target.CurrentHp = 50;
        target.ArmorSoak = 4;
        var ability = CreateTestAbility(effectScript: "DAMAGE:Physical:2d6");

        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(5); // 2d6 = 10 total, minus 4 soak = 6 damage

        // Act
        _sut.Execute(user, target, ability);

        // Assert
        target.CurrentHp.Should().Be(44); // 50 - 6 = 44
    }

    [Fact]
    public void Execute_NonPhysicalDamage_IgnoresArmorSoak()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        target.CurrentHp = 50;
        target.ArmorSoak = 4;
        var ability = CreateTestAbility(effectScript: "DAMAGE:Fire:2d6");

        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(5); // 2d6 = 10 total

        // Act
        _sut.Execute(user, target, ability);

        // Assert - Full 10 damage (no soak applied)
        target.CurrentHp.Should().Be(40);
    }

    [Fact]
    public void Execute_DamageScript_AppliesVulnerabilityMultiplier()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        target.CurrentHp = 100;
        var ability = CreateTestAbility(effectScript: "DAMAGE:Fire:2d6");

        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(5); // 2d6 = 10 total

        _mockStatusEffectService.Setup(s => s.GetDamageMultiplier(target))
            .Returns(1.5f); // Vulnerable = 50% extra damage

        // Act
        _sut.Execute(user, target, ability);

        // Assert - 10 * 1.5 = 15 damage
        target.CurrentHp.Should().Be(85);
    }

    [Fact]
    public void Execute_DamageScript_ReturnsDamageAmount()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        var ability = CreateTestAbility(effectScript: "DAMAGE:Physical:2d6");

        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(5); // 2d6 = 10 total

        // Act
        var result = _sut.Execute(user, target, ability);

        // Assert
        result.TotalDamage.Should().Be(10);
    }

    #endregion

    #region Execute Tests - HEAL Command

    [Fact]
    public void Execute_HealScript_RestoresHp()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        target.CurrentHp = 50;
        target.MaxHp = 100;
        var ability = CreateTestAbility(effectScript: "HEAL:20");

        // Act
        _sut.Execute(user, target, ability);

        // Assert
        target.CurrentHp.Should().Be(70);
    }

    [Fact]
    public void Execute_HealScript_ClampsToMaxHp()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        target.CurrentHp = 90;
        target.MaxHp = 100;
        var ability = CreateTestAbility(effectScript: "HEAL:25");

        // Act
        _sut.Execute(user, target, ability);

        // Assert
        target.CurrentHp.Should().Be(100); // Clamped to max
    }

    [Fact]
    public void Execute_HealScript_ReturnsHealingAmount()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        target.CurrentHp = 50;
        target.MaxHp = 100;
        var ability = CreateTestAbility(effectScript: "HEAL:20");

        // Act
        var result = _sut.Execute(user, target, ability);

        // Assert
        result.TotalHealing.Should().Be(20);
    }

    [Fact]
    public void Execute_HealScript_ReturnsActualHealingWhenClamped()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        target.CurrentHp = 95;
        target.MaxHp = 100;
        var ability = CreateTestAbility(effectScript: "HEAL:20");

        // Act
        var result = _sut.Execute(user, target, ability);

        // Assert
        result.TotalHealing.Should().Be(5); // Only 5 was actually healed
    }

    #endregion

    #region Execute Tests - STATUS Command

    [Fact]
    public void Execute_StatusScript_AppliesEffect()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        var ability = CreateTestAbility(effectScript: "STATUS:Bleeding:3:1");

        // Act
        _sut.Execute(user, target, ability);

        // Assert
        _mockStatusEffectService.Verify(
            s => s.ApplyEffect(target, StatusEffectType.Bleeding, 3, user.Id),
            Times.Once);
    }

    [Fact]
    public void Execute_StatusScript_AppliesMultipleStacks()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        var ability = CreateTestAbility(effectScript: "STATUS:Bleeding:3:3");

        // Act
        _sut.Execute(user, target, ability);

        // Assert - ApplyEffect called 3 times for 3 stacks
        _mockStatusEffectService.Verify(
            s => s.ApplyEffect(target, StatusEffectType.Bleeding, 3, user.Id),
            Times.Exactly(3));
    }

    [Fact]
    public void Execute_StatusScript_ReturnsStatusApplied()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        var ability = CreateTestAbility(effectScript: "STATUS:Poisoned:2:1");

        // Act
        var result = _sut.Execute(user, target, ability);

        // Assert
        result.StatusesApplied.Should().NotBeNull();
        result.StatusesApplied.Should().Contain("Poisoned");
    }

    [Fact]
    public void Execute_StatusScript_DefaultsToOneStack()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        var ability = CreateTestAbility(effectScript: "STATUS:Stunned:2"); // No stack parameter

        // Act
        _sut.Execute(user, target, ability);

        // Assert - Applied once
        _mockStatusEffectService.Verify(
            s => s.ApplyEffect(target, StatusEffectType.Stunned, 2, user.Id),
            Times.Once);
    }

    #endregion

    #region Execute Tests - Combined Scripts

    [Fact]
    public void Execute_CombinedScript_AppliesAllEffects()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        target.CurrentHp = 100;
        target.MaxHp = 100;
        var ability = CreateTestAbility(effectScript: "DAMAGE:Physical:2d6;STATUS:Bleeding:3:1");

        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(4); // 2d6 = 8 damage

        // Act
        var result = _sut.Execute(user, target, ability);

        // Assert
        target.CurrentHp.Should().Be(92); // 100 - 8 = 92
        _mockStatusEffectService.Verify(
            s => s.ApplyEffect(target, StatusEffectType.Bleeding, 3, user.Id),
            Times.Once);
        result.TotalDamage.Should().Be(8);
        result.StatusesApplied.Should().Contain("Bleeding");
    }

    [Fact]
    public void Execute_CombinedScript_DamageAndHeal()
    {
        // Arrange - Self-harm then heal ability (self-target)
        var user = CreateTestCombatant();
        user.CurrentHp = 50;  // Start at 50/100 so heal isn't clamped
        user.MaxHp = 100;
        var ability = CreateTestAbility(effectScript: "DAMAGE:Physical:1d6;HEAL:10");

        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(3); // 1d6 = 3 damage

        // Act
        var result = _sut.Execute(user, user, ability);

        // Assert - Damage then heal: 50 - 3 + 10 = 57
        result.TotalDamage.Should().Be(3);
        result.TotalHealing.Should().Be(10);
        user.CurrentHp.Should().Be(57);
    }

    [Fact]
    public void Execute_ReturnsSuccessWithMessage()
    {
        // Arrange
        var user = CreateTestCombatant();
        user.Name = "Hero";
        var target = CreateTestCombatant();
        target.Name = "Goblin";
        var ability = CreateTestAbility(effectScript: "DAMAGE:Physical:1d6");
        ability.Name = "Power Strike";

        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(4);

        // Act
        var result = _sut.Execute(user, target, ability);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Hero");
        result.Message.Should().Contain("Power Strike");
        result.Message.Should().Contain("Goblin");
    }

    #endregion

    #region ProcessCooldowns Tests

    [Fact]
    public void ProcessCooldowns_DecrementsValues()
    {
        // Arrange
        var abilityId = Guid.NewGuid();
        var combatant = CreateTestCombatant();
        combatant.Cooldowns[abilityId] = 3;

        // Act
        _sut.ProcessCooldowns(combatant);

        // Assert
        combatant.Cooldowns[abilityId].Should().Be(2);
    }

    [Fact]
    public void ProcessCooldowns_RemovesExpired()
    {
        // Arrange
        var abilityId = Guid.NewGuid();
        var combatant = CreateTestCombatant();
        combatant.Cooldowns[abilityId] = 1;

        // Act
        _sut.ProcessCooldowns(combatant);

        // Assert
        combatant.Cooldowns.Should().NotContainKey(abilityId);
    }

    [Fact]
    public void ProcessCooldowns_HandlesMultipleCooldowns()
    {
        // Arrange
        var ability1 = Guid.NewGuid();
        var ability2 = Guid.NewGuid();
        var ability3 = Guid.NewGuid();
        var combatant = CreateTestCombatant();
        combatant.Cooldowns[ability1] = 1; // Will be removed
        combatant.Cooldowns[ability2] = 3; // Will become 2
        combatant.Cooldowns[ability3] = 2; // Will become 1

        // Act
        _sut.ProcessCooldowns(combatant);

        // Assert
        combatant.Cooldowns.Should().NotContainKey(ability1);
        combatant.Cooldowns[ability2].Should().Be(2);
        combatant.Cooldowns[ability3].Should().Be(1);
    }

    [Fact]
    public void ProcessCooldowns_EmptyCooldowns_DoesNothing()
    {
        // Arrange
        var combatant = CreateTestCombatant();

        // Act - Should not throw
        var act = () => _sut.ProcessCooldowns(combatant);

        // Assert
        act.Should().NotThrow();
        combatant.Cooldowns.Should().BeEmpty();
    }

    #endregion

    #region GetCooldownRemaining Tests

    [Fact]
    public void GetCooldownRemaining_ReturnsRemainingTurns()
    {
        // Arrange
        var abilityId = Guid.NewGuid();
        var combatant = CreateTestCombatant();
        combatant.Cooldowns[abilityId] = 5;

        // Act
        var result = _sut.GetCooldownRemaining(combatant, abilityId);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void GetCooldownRemaining_NotOnCooldown_ReturnsZero()
    {
        // Arrange
        var abilityId = Guid.NewGuid();
        var combatant = CreateTestCombatant();

        // Act
        var result = _sut.GetCooldownRemaining(combatant, abilityId);

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Execute_EmptyEffectScript_ReturnsSuccessWithMessage()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        var ability = CreateTestAbility(effectScript: "");

        // Act
        var result = _sut.Execute(user, target, ability);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("nothing happens");
    }

    [Fact]
    public void Execute_InvalidDiceNotation_HandleGracefully()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        target.CurrentHp = 100;
        var ability = CreateTestAbility(effectScript: "DAMAGE:Physical:invalid");

        // Act
        var result = _sut.Execute(user, target, ability);

        // Assert
        result.Success.Should().BeTrue();
        target.CurrentHp.Should().Be(100); // No damage applied
    }

    [Fact]
    public void Execute_UnknownCommand_IgnoresAndContinues()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        target.CurrentHp = 100;
        var ability = CreateTestAbility(effectScript: "UNKNOWN:Param1;DAMAGE:Physical:1d6");

        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(4);

        // Act
        var result = _sut.Execute(user, target, ability);

        // Assert - Should still apply damage despite unknown command
        result.Success.Should().BeTrue();
        target.CurrentHp.Should().Be(96);
    }

    [Fact]
    public void Execute_UnknownStatusEffect_DoesNotApply()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        var ability = CreateTestAbility(effectScript: "STATUS:FakeEffect:3:1");

        // Act
        var result = _sut.Execute(user, target, ability);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusesApplied.Should().BeNullOrEmpty();
        _mockStatusEffectService.Verify(
            s => s.ApplyEffect(It.IsAny<Combatant>(), It.IsAny<StatusEffectType>(), It.IsAny<int>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public void Execute_SelfTarget_Works()
    {
        // Arrange
        var user = CreateTestCombatant();
        user.CurrentHp = 50;
        user.MaxHp = 100;
        var ability = CreateTestAbility(effectScript: "HEAL:25");

        // Act
        var result = _sut.Execute(user, user, ability);

        // Assert
        user.CurrentHp.Should().Be(75);
        result.Message.Should().Contain(user.Name);
        result.Message.Should().NotContain(" on "); // Self-target message format
    }

    [Fact]
    public void Execute_DamageCannotGoNegative()
    {
        // Arrange
        var user = CreateTestCombatant();
        var target = CreateTestCombatant();
        target.CurrentHp = 100;
        target.ArmorSoak = 50; // Very high soak
        var ability = CreateTestAbility(effectScript: "DAMAGE:Physical:1d6");

        _mockDiceService.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(3); // Only 3 damage, but 50 soak

        // Act
        _sut.Execute(user, target, ability);

        // Assert - Damage clamped to 0
        target.CurrentHp.Should().Be(100);
    }

    #endregion

    #region Helper Methods

    private static Combatant CreateTestCombatant()
    {
        return new Combatant
        {
            Id = Guid.NewGuid(),
            Name = "TestCombatant",
            CurrentHp = 100,
            MaxHp = 100,
            CurrentStamina = 60,
            MaxStamina = 60,
            CurrentAp = 30,
            MaxAp = 30,
            ArmorSoak = 0
        };
    }

    private static ActiveAbility CreateTestAbility(
        int staminaCost = 0,
        int aetherCost = 0,
        int cooldownTurns = 0,
        string effectScript = "")
    {
        return new ActiveAbility
        {
            Id = Guid.NewGuid(),
            Name = "Test Ability",
            Description = "A test ability",
            StaminaCost = staminaCost,
            AetherCost = aetherCost,
            CooldownTurns = cooldownTurns,
            EffectScript = effectScript
        };
    }

    #endregion
}
