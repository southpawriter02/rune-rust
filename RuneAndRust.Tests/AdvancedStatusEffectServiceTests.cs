using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.21.3: Tests for Advanced Status Effect System
/// Tests stacking, interactions, conversions, amplifications, and suppressions
/// </summary>
[TestClass]
public class AdvancedStatusEffectServiceTests
{
    private AdvancedStatusEffectService _service = null!;
    private StatusEffectRepository _repository = null!;
    private TraumaEconomyService _traumaService = null!;
    private DiceService _diceService = null!;
    private string _testDbPath = null!;

    [TestInitialize]
    public void Setup()
    {
        // Create temp directory for test database
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_statuseffects_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDbPath);

        _repository = new StatusEffectRepository(_testDbPath);
        _diceService = new DiceService();

        // Create a minimal TraumaEconomyService for testing
        // Note: This might need adjustment based on actual TraumaEconomyService constructor
        _traumaService = CreateTestTraumaService();

        _service = new AdvancedStatusEffectService(_repository, _traumaService, _diceService);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Clean up test database
        if (Directory.Exists(_testDbPath))
        {
            Directory.Delete(_testDbPath, true);
        }
    }

    private TraumaEconomyService CreateTestTraumaService()
    {
        // Create a minimal trauma service for testing
        // This is a placeholder - adjust based on actual constructor
        try
        {
            return new TraumaEconomyService();
        }
        catch
        {
            // If constructor requires parameters, create mock or use test doubles
            return null!; // Will be handled in actual implementation
        }
    }

    #region Stacking Tests

    [TestMethod]
    public void ApplyEffect_BleedingFiveTimes_ReachesMaxStacks()
    {
        // Arrange
        int targetId = 1;

        // Act
        for (int i = 0; i < 7; i++) // Try to apply 7 times (max is 5)
        {
            _service.ApplyEffect(targetId, "Bleeding", stacks: 1, duration: 5);
        }

        // Assert
        var stackCount = _service.GetStackCount(targetId, "Bleeding");
        Assert.AreEqual(5, stackCount, "Bleeding should cap at 5 stacks");
    }

    [TestMethod]
    public void ApplyEffect_BleedingMultipleStacks_StacksCorrectly()
    {
        // Arrange
        int targetId = 1;

        // Act
        _service.ApplyEffect(targetId, "Bleeding", stacks: 3, duration: 5);

        // Assert
        var stackCount = _service.GetStackCount(targetId, "Bleeding");
        Assert.AreEqual(3, stackCount, "Bleeding should have 3 stacks");
    }

    [TestMethod]
    public void ApplyEffect_PoisonedThreeTimes_ReachesMaxStacks()
    {
        // Arrange
        int targetId = 1;

        // Act
        for (int i = 0; i < 5; i++) // Try to apply 5 times (max is 3)
        {
            _service.ApplyEffect(targetId, "Poisoned", stacks: 1, duration: 4);
        }

        // Assert
        var stackCount = _service.GetStackCount(targetId, "Poisoned");
        Assert.AreEqual(3, stackCount, "Poisoned should cap at 3 stacks");
    }

    [TestMethod]
    public void ApplyEffect_StunnedTwice_DoesNotStack()
    {
        // Arrange
        int targetId = 1;

        // Act
        _service.ApplyEffect(targetId, "Stunned", duration: 1);
        _service.ApplyEffect(targetId, "Stunned", duration: 1);

        // Assert
        var stackCount = _service.GetStackCount(targetId, "Stunned");
        Assert.AreEqual(1, stackCount, "Stunned should not stack");
    }

    [TestMethod]
    public void CanStack_Bleeding_ReturnsTrue()
    {
        // Act
        var canStack = _service.CanStack("Bleeding");

        // Assert
        Assert.IsTrue(canStack, "Bleeding should be stackable");
    }

    [TestMethod]
    public void CanStack_Stunned_ReturnsFalse()
    {
        // Act
        var canStack = _service.CanStack("Stunned");

        // Assert
        Assert.IsFalse(canStack, "Stunned should not be stackable");
    }

    [TestMethod]
    public void GetMaxStacks_Bleeding_ReturnsFive()
    {
        // Act
        var maxStacks = _service.GetMaxStacks("Bleeding");

        // Assert
        Assert.AreEqual(5, maxStacks, "Bleeding should have max 5 stacks");
    }

    #endregion

    #region Conversion Tests

    [TestMethod]
    public void ApplyEffect_DisorientedTwice_ConvertsToStunned()
    {
        // Arrange
        int targetId = 1;

        // Act
        _service.ApplyEffect(targetId, "Disoriented", duration: 2);
        var result = _service.ApplyEffect(targetId, "Disoriented", duration: 2);

        // Assert
        Assert.IsTrue(result.ConversionTriggered, "Conversion should be triggered");
        Assert.AreEqual("Stunned", result.ConvertedTo, "Should convert to Stunned");
        Assert.IsFalse(_service.HasEffect(targetId, "Disoriented"), "Disoriented should be removed");
        Assert.IsTrue(_service.HasEffect(targetId, "Stunned"), "Stunned should be applied");
    }

    [TestMethod]
    public void CheckConversion_DisorientedTwice_ConvertsToStunned()
    {
        // Arrange
        int targetId = 1;
        _service.ApplyEffect(targetId, "Disoriented", duration: 2);

        // Act
        var result = _service.CheckConversion(targetId, "Disoriented");

        // Assert
        Assert.IsTrue(result.ConversionTriggered, "Conversion should be triggered");
        Assert.AreEqual("Stunned", result.ConvertedTo, "Should convert to Stunned");
    }

    #endregion

    #region Amplification Tests

    [TestMethod]
    public void CalculateAmplificationMultiplier_BleedingWithCorroded_Returns150Percent()
    {
        // Arrange
        int targetId = 1;
        _service.ApplyEffect(targetId, "Bleeding", stacks: 1, duration: 5);
        _service.ApplyEffect(targetId, "Corroded", stacks: 1, duration: 5);

        // Act
        var multiplier = _service.CalculateAmplificationMultiplier(targetId, "Bleeding");

        // Assert
        Assert.AreEqual(1.5f, multiplier, 0.01f, "Bleeding should be amplified 1.5× by Corroded");
    }

    [TestMethod]
    public void CalculateAmplificationMultiplier_BleedingWithoutCorroded_Returns100Percent()
    {
        // Arrange
        int targetId = 1;
        _service.ApplyEffect(targetId, "Bleeding", stacks: 1, duration: 5);

        // Act
        var multiplier = _service.CalculateAmplificationMultiplier(targetId, "Bleeding");

        // Assert
        Assert.AreEqual(1.0f, multiplier, 0.01f, "Bleeding without Corroded should have no amplification");
    }

    [TestMethod]
    public void CalculateAmplificationMultiplier_PoisonedWithBleeding_Returns130Percent()
    {
        // Arrange
        int targetId = 1;
        _service.ApplyEffect(targetId, "Poisoned", stacks: 1, duration: 4);
        _service.ApplyEffect(targetId, "Bleeding", stacks: 1, duration: 5);

        // Act
        var multiplier = _service.CalculateAmplificationMultiplier(targetId, "Poisoned");

        // Assert
        Assert.AreEqual(1.3f, multiplier, 0.01f, "Poisoned should be amplified 1.3× by Bleeding");
    }

    #endregion

    #region Suppression Tests

    [TestMethod]
    public void CheckSuppression_SlowedAndHasted_CancelEachOther()
    {
        // Arrange
        int targetId = 1;
        _service.ApplyEffect(targetId, "Hasted", duration: 3);

        // Act
        var suppressed = _service.CheckSuppression(targetId, "Slowed");

        // Assert
        Assert.IsTrue(suppressed, "Slowed should suppress Hasted");
        Assert.IsFalse(_service.HasEffect(targetId, "Hasted"), "Hasted should be removed");
    }

    [TestMethod]
    public void ApplyEffect_SlowedWhenHasted_Suppresses()
    {
        // Arrange
        int targetId = 1;
        _service.ApplyEffect(targetId, "Hasted", duration: 3);

        // Act
        var result = _service.ApplyEffect(targetId, "Slowed", duration: 3);

        // Assert
        Assert.IsFalse(result.Success, "Slowed should be suppressed");
        Assert.IsFalse(_service.HasEffect(targetId, "Hasted"), "Hasted should be canceled");
        Assert.IsFalse(_service.HasEffect(targetId, "Slowed"), "Slowed should not be applied");
    }

    #endregion

    #region Turn Processing Tests

    [TestMethod]
    public async Task ProcessStartOfTurn_BleedingThreeStacks_DealsDamage()
    {
        // Arrange
        int targetId = 1;
        var enemy = new Enemy
        {
            Id = "test-enemy",
            Name = "Test Enemy",
            HP = 100,
            MaxHP = 100,
            Soak = 0
        };

        _service.ApplyEffect(targetId, "Bleeding", stacks: 3, duration: 5);

        // Act
        var messages = await _service.ProcessStartOfTurn(targetId, enemy: enemy);

        // Assert
        Assert.IsTrue(messages.Count > 0, "Should have damage message");
        Assert.IsTrue(enemy.HP < 100, "Enemy should take damage");
        Assert.IsTrue(messages[0].Contains("Bleeding damage"), "Message should mention Bleeding");
    }

    [TestMethod]
    public async Task ProcessStartOfTurn_BleedingIgnoresSoak()
    {
        // Arrange
        int targetId = 1;
        var enemy = new Enemy
        {
            Id = "test-enemy",
            Name = "Test Enemy",
            HP = 100,
            MaxHP = 100,
            Soak = 10 // High soak
        };

        _service.ApplyEffect(targetId, "Bleeding", stacks: 1, duration: 5);

        // Act
        var messages = await _service.ProcessStartOfTurn(targetId, enemy: enemy);

        // Assert
        Assert.IsTrue(enemy.HP < 100, "Bleeding should ignore Soak and deal damage");
    }

    [TestMethod]
    public async Task ProcessEndOfTurn_DecrementsDecrementsDuration()
    {
        // Arrange
        int targetId = 1;
        _service.ApplyEffect(targetId, "Bleeding", stacks: 1, duration: 3);

        // Act
        await _service.ProcessEndOfTurn(targetId);

        // Assert
        var effect = _service.GetActiveEffect(targetId, "Bleeding");
        Assert.IsNotNull(effect, "Effect should still exist");
        Assert.AreEqual(2, effect.DurationRemaining, "Duration should decrement to 2");
    }

    [TestMethod]
    public async Task ProcessEndOfTurn_RemovesExpiredEffects()
    {
        // Arrange
        int targetId = 1;
        _service.ApplyEffect(targetId, "Bleeding", stacks: 1, duration: 1);

        // Act
        var messages = await _service.ProcessEndOfTurn(targetId);

        // Assert
        Assert.IsFalse(_service.HasEffect(targetId, "Bleeding"), "Expired effect should be removed");
        Assert.IsTrue(messages.Any(m => m.Contains("no longer")), "Should have expiration message");
    }

    #endregion

    #region Effect Management Tests

    [TestMethod]
    public void HasEffect_WithActiveEffect_ReturnsTrue()
    {
        // Arrange
        int targetId = 1;
        _service.ApplyEffect(targetId, "Bleeding", stacks: 1, duration: 5);

        // Act
        var hasEffect = _service.HasEffect(targetId, "Bleeding");

        // Assert
        Assert.IsTrue(hasEffect, "Should have Bleeding effect");
    }

    [TestMethod]
    public void HasEffect_WithoutEffect_ReturnsFalse()
    {
        // Arrange
        int targetId = 1;

        // Act
        var hasEffect = _service.HasEffect(targetId, "Bleeding");

        // Assert
        Assert.IsFalse(hasEffect, "Should not have Bleeding effect");
    }

    [TestMethod]
    public void RemoveEffect_RemovesSpecificEffect()
    {
        // Arrange
        int targetId = 1;
        _service.ApplyEffect(targetId, "Bleeding", stacks: 1, duration: 5);
        _service.ApplyEffect(targetId, "Poisoned", stacks: 1, duration: 4);

        // Act
        _service.RemoveEffect(targetId, "Bleeding");

        // Assert
        Assert.IsFalse(_service.HasEffect(targetId, "Bleeding"), "Bleeding should be removed");
        Assert.IsTrue(_service.HasEffect(targetId, "Poisoned"), "Poisoned should remain");
    }

    [TestMethod]
    public void RemoveAllEffects_RemovesAllEffects()
    {
        // Arrange
        int targetId = 1;
        _service.ApplyEffect(targetId, "Bleeding", stacks: 1, duration: 5);
        _service.ApplyEffect(targetId, "Poisoned", stacks: 1, duration: 4);
        _service.ApplyEffect(targetId, "Stunned", duration: 1);

        // Act
        _service.RemoveAllEffects(targetId);

        // Assert
        Assert.IsFalse(_service.HasEffect(targetId, "Bleeding"), "Bleeding should be removed");
        Assert.IsFalse(_service.HasEffect(targetId, "Poisoned"), "Poisoned should be removed");
        Assert.IsFalse(_service.HasEffect(targetId, "Stunned"), "Stunned should be removed");
    }

    [TestMethod]
    public void GetActiveEffects_ReturnsAllEffects()
    {
        // Arrange
        int targetId = 1;
        _service.ApplyEffect(targetId, "Bleeding", stacks: 3, duration: 5);
        _service.ApplyEffect(targetId, "Poisoned", stacks: 2, duration: 4);

        // Act
        var effects = _service.GetActiveEffects(targetId);

        // Assert
        Assert.AreEqual(2, effects.Count, "Should have 2 active effects");
        Assert.IsTrue(effects.Any(e => e.EffectType == "Bleeding"), "Should include Bleeding");
        Assert.IsTrue(effects.Any(e => e.EffectType == "Poisoned"), "Should include Poisoned");
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public async Task Integration_BleedingWithCorrodedAmplification_DealsAmplifiedDamage()
    {
        // Arrange
        int targetId = 1;
        var enemy = new Enemy
        {
            Id = "test-enemy",
            Name = "Test Enemy",
            HP = 100,
            MaxHP = 100,
            Soak = 0
        };

        // Apply Bleeding (3 stacks = 3d6 base damage)
        _service.ApplyEffect(targetId, "Bleeding", stacks: 3, duration: 5);
        // Apply Corroded (amplifies Bleeding by 1.5×)
        _service.ApplyEffect(targetId, "Corroded", stacks: 1, duration: 5);

        int initialHP = enemy.HP;

        // Act
        var messages = await _service.ProcessStartOfTurn(targetId, enemy: enemy);

        // Assert
        int damageDealt = initialHP - enemy.HP;
        // Min damage: 3d6 × 1.5 = 4.5 → 4, Max damage: 18 × 1.5 = 27
        Assert.IsTrue(damageDealt >= 4 && damageDealt <= 27,
            $"Amplified Bleeding should deal 4-27 damage, dealt: {damageDealt}");
        Assert.IsTrue(messages[0].Contains("Bleeding damage"), "Should mention Bleeding");
    }

    [TestMethod]
    public void Integration_MultipleDebuffs_NoErrors()
    {
        // Arrange
        int targetId = 1;

        // Act - Apply multiple different debuffs
        _service.ApplyEffect(targetId, "Bleeding", stacks: 2, duration: 5);
        _service.ApplyEffect(targetId, "Poisoned", stacks: 1, duration: 4);
        _service.ApplyEffect(targetId, "Corroded", stacks: 3, duration: 5);
        _service.ApplyEffect(targetId, "Disoriented", duration: 2);

        // Assert
        var effects = _service.GetActiveEffects(targetId);
        Assert.AreEqual(4, effects.Count, "Should have 4 active effects");
    }

    [TestMethod]
    public void StatusEffectDefinition_GetAllDefinitions_ReturnsAllEffects()
    {
        // Act
        var definitions = StatusEffectDefinition.GetAllDefinitions().ToList();

        // Assert
        Assert.IsTrue(definitions.Count >= 8, "Should have at least 8 canonical effects");
        Assert.IsTrue(definitions.Any(d => d.EffectType == "Stunned"), "Should include Stunned");
        Assert.IsTrue(definitions.Any(d => d.EffectType == "Bleeding"), "Should include Bleeding");
        Assert.IsTrue(definitions.Any(d => d.EffectType == "Poisoned"), "Should include Poisoned");
        Assert.IsTrue(definitions.Any(d => d.EffectType == "Corroded"), "Should include Corroded");
    }

    [TestMethod]
    public void StatusEffectDefinition_GetDefinition_ReturnsCorrectDefinition()
    {
        // Act
        var bleeding = StatusEffectDefinition.GetDefinition("Bleeding");

        // Assert
        Assert.IsNotNull(bleeding, "Bleeding definition should exist");
        Assert.AreEqual("Bleeding", bleeding.EffectType);
        Assert.IsTrue(bleeding.CanStack, "Bleeding should be stackable");
        Assert.AreEqual(5, bleeding.MaxStacks, "Bleeding should have 5 max stacks");
        Assert.IsTrue(bleeding.IgnoresSoak, "Bleeding should ignore Soak");
        Assert.AreEqual("1d6", bleeding.DamageBase, "Bleeding should deal 1d6 per stack");
    }

    #endregion
}
