using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.21.4: Unit tests for CounterAttackService
/// Tests parry mechanics, riposte execution, and specialization bonuses
/// </summary>
[TestClass]
public class CounterAttackServiceTests
{
    private const string TestDbPath = "./test_counterattack.db";
    private CounterAttackService _service = null!;
    private CounterAttackRepository _repository = null!;
    private DiceService _diceService = null!;
    private TraumaEconomyService _traumaService = null!;

    [TestInitialize]
    public void Setup()
    {
        // Delete test database if it exists
        if (File.Exists(TestDbPath))
        {
            File.Delete(TestDbPath);
        }

        // Create test services
        _diceService = new DiceService(seed: 12345); // Fixed seed for deterministic tests
        _traumaService = new TraumaEconomyService(new Random(12345));
        _repository = new CounterAttackRepository(Path.GetDirectoryName(TestDbPath));
        _service = new CounterAttackService(_diceService, _repository, _traumaService);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Clean up test database
        if (File.Exists(TestDbPath))
        {
            File.Delete(TestDbPath);
        }
    }

    #region Parry Outcome Tests

    [TestMethod]
    public void DetermineParryOutcome_ParryLessThanAccuracy_ReturnsFailed()
    {
        // Arrange
        int parryRoll = 15;
        int accuracyRoll = 18;

        // Act
        var outcome = _service.DetermineParryOutcome(parryRoll, accuracyRoll);

        // Assert
        Assert.AreEqual(ParryOutcome.Failed, outcome, "Parry < Accuracy should result in Failed");
    }

    [TestMethod]
    public void DetermineParryOutcome_ParryEqualsAccuracy_ReturnsStandard()
    {
        // Arrange
        int parryRoll = 18;
        int accuracyRoll = 18;

        // Act
        var outcome = _service.DetermineParryOutcome(parryRoll, accuracyRoll);

        // Assert
        Assert.AreEqual(ParryOutcome.Standard, outcome, "Parry = Accuracy should result in Standard");
    }

    [TestMethod]
    public void DetermineParryOutcome_ParryExceedsBy1to4_ReturnsSuperior()
    {
        // Arrange & Act & Assert
        for (int diff = 1; diff <= 4; diff++)
        {
            var outcome = _service.DetermineParryOutcome(18 + diff, 18);
            Assert.AreEqual(ParryOutcome.Superior, outcome,
                $"Parry exceeding by {diff} should result in Superior");
        }
    }

    [TestMethod]
    public void DetermineParryOutcome_ParryExceedsBy5Plus_ReturnsCritical()
    {
        // Arrange
        int parryRoll = 24;
        int accuracyRoll = 18;

        // Act
        var outcome = _service.DetermineParryOutcome(parryRoll, accuracyRoll);

        // Assert
        Assert.AreEqual(ParryOutcome.Critical, outcome,
            "Parry exceeding by 5+ should result in Critical");
    }

    #endregion

    #region Parry Pool Calculation Tests

    [TestMethod]
    public void CalculateParryPool_BaseCharacter_ReturnsCorrectValue()
    {
        // Arrange
        var character = CreateTestCharacter(finesse: 6);

        // Act
        var parryPool = _service.CalculateParryPool(character);

        // Assert
        // Expected: FINESSE (6) + WeaponSkill (3) = 9
        Assert.IsTrue(parryPool >= 9, "Base parry pool should be at least FINESSE + WeaponSkill");
    }

    [TestMethod]
    public void CalculateParryPool_WithHolmgangrBonus_AddsBonus()
    {
        // Arrange
        var character = CreateTestCharacter(finesse: 6);
        _service.ApplyHolmgangrReactiveParry(character, rank: 1);

        // Act
        var parryPool = _service.CalculateParryPool(character);

        // Assert
        // Expected: Base (9) + 1d10 (at least 1, at most 10)
        Assert.IsTrue(parryPool >= 10 && parryPool <= 19,
            "Hólmgangr Rank 1 should add +1d10 to base pool of 9");
    }

    #endregion

    #region Riposte Tests

    [TestMethod]
    public void CanRiposte_CriticalParry_AlwaysReturnsTrue()
    {
        // Arrange
        var character = CreateTestCharacter();

        // Act
        var canRiposte = _service.CanRiposte(character, ParryOutcome.Critical);

        // Assert
        Assert.IsTrue(canRiposte, "All characters should be able to riposte on Critical Parry");
    }

    [TestMethod]
    public void CanRiposte_SuperiorParry_WithoutHolmgangr_ReturnsFalse()
    {
        // Arrange
        var character = CreateTestCharacter();

        // Act
        var canRiposte = _service.CanRiposte(character, ParryOutcome.Superior);

        // Assert
        Assert.IsFalse(canRiposte,
            "Non-Hólmgangr characters should NOT riposte on Superior Parry");
    }

    [TestMethod]
    public void CanRiposte_SuperiorParry_WithHolmgangr_ReturnsTrue()
    {
        // Arrange
        var character = CreateTestCharacter();
        _service.ApplyHolmgangrReactiveParry(character, rank: 1);

        // Act
        var canRiposte = _service.CanRiposte(character, ParryOutcome.Superior);

        // Assert
        Assert.IsTrue(canRiposte,
            "Hólmgangr should be able to riposte on Superior Parry");
    }

    [TestMethod]
    public void ExecuteRiposte_ValidTarget_DealsPositiveDamage()
    {
        // Arrange
        var attacker = CreateTestCharacter(finesse: 7, might: 5);
        var target = CreateTestEnemy(hp: 50, defense: 10, soak: 2);

        // Act
        var result = _service.ExecuteRiposte(attacker, target);

        // Assert
        if (result.Hit)
        {
            Assert.IsTrue(result.DamageDealt > 0, "Riposte hit should deal positive damage");
            Assert.IsTrue(target.HP < 50, "Target HP should be reduced");
        }
    }

    #endregion

    #region Full Parry Execution Tests

    [TestMethod]
    public void ExecuteParry_FailedParry_ReturnsFailedOutcome()
    {
        // Arrange
        var defender = CreateTestCharacter(finesse: 3); // Low finesse
        var attacker = CreateTestEnemy();

        // Act
        var result = _service.ExecuteParry(defender, attacker, attackAccuracy: 20);

        // Assert
        Assert.IsFalse(result.Success, "Low parry roll should fail against high accuracy");
        Assert.AreEqual(ParryOutcome.Failed, result.Outcome);
    }

    [TestMethod]
    public void ExecuteParry_CriticalParry_TriggersRiposte()
    {
        // Arrange
        var defender = CreateTestCharacter(finesse: 10);
        var attacker = CreateTestEnemy(hp: 30, defense: 5);
        _service.ApplyHolmgangrReactiveParry(defender, rank: 1); // Ensure high parry

        // Act
        var result = _service.ExecuteParry(defender, attacker, attackAccuracy: 5);

        // Assert
        Assert.IsTrue(result.Success, "High parry vs low accuracy should succeed");

        // Note: Due to dice randomness, we can't guarantee Critical,
        // but we can test the logic
        if (result.Outcome == ParryOutcome.Critical)
        {
            Assert.IsTrue(result.RiposteTriggered, "Critical Parry should trigger Riposte");
        }
    }

    #endregion

    #region Specialization Bonus Tests

    [TestMethod]
    public void ApplyHolmgangrReactiveParry_Rank1_AppliesCorrectBonus()
    {
        // Arrange
        var character = CreateTestCharacter();

        // Act
        _service.ApplyHolmgangrReactiveParry(character, rank: 1);

        // Assert
        Assert.AreEqual(1, _service.GetParryBonusDice(character),
            "Hólmgangr Rank 1 should grant +1 bonus die");
        Assert.IsTrue(_service.HasSuperiorRiposte(character),
            "Hólmgangr should have Superior Riposte");
        Assert.AreEqual(1, _service.GetParriesPerRound(character),
            "Hólmgangr Rank 1 should have 1 parry per round");
    }

    [TestMethod]
    public void ApplyHolmgangrReactiveParry_Rank2_AppliesCorrectBonus()
    {
        // Arrange
        var character = CreateTestCharacter();

        // Act
        _service.ApplyHolmgangrReactiveParry(character, rank: 2);

        // Assert
        Assert.AreEqual(2, _service.GetParryBonusDice(character),
            "Hólmgangr Rank 2 should grant +2 bonus dice");
        Assert.IsTrue(_service.HasSuperiorRiposte(character),
            "Hólmgangr should have Superior Riposte");
        Assert.AreEqual(1, _service.GetParriesPerRound(character),
            "Hólmgangr Rank 2 should still have 1 parry per round");
    }

    [TestMethod]
    public void ApplyHolmgangrReactiveParry_Rank3_AppliesDoubleParry()
    {
        // Arrange
        var character = CreateTestCharacter();

        // Act
        _service.ApplyHolmgangrReactiveParry(character, rank: 3);

        // Assert
        Assert.AreEqual(2, _service.GetParryBonusDice(character),
            "Hólmgangr Rank 3 should grant +2 bonus dice");
        Assert.IsTrue(_service.HasSuperiorRiposte(character),
            "Hólmgangr should have Superior Riposte");
        Assert.AreEqual(2, _service.GetParriesPerRound(character),
            "Hólmgangr Rank 3 should have 2 parries per round");
    }

    [TestMethod]
    public void ApplyAtgeirWielderParryBonus_AppliesCorrectBonus()
    {
        // Arrange
        var character = CreateTestCharacter();

        // Act
        _service.ApplyAtgeirWielderParryBonus(character);

        // Assert
        Assert.AreEqual(1, _service.GetParryBonusDice(character),
            "Atgeir-wielder should grant +1 bonus die");
        Assert.IsFalse(_service.HasSuperiorRiposte(character),
            "Atgeir-wielder should NOT have Superior Riposte");
        Assert.AreEqual(1, _service.GetParriesPerRound(character),
            "Atgeir-wielder should have 1 parry per round");
    }

    [TestMethod]
    public void RemoveAllParryBonuses_RemovesAllBonuses()
    {
        // Arrange
        var character = CreateTestCharacter();
        _service.ApplyHolmgangrReactiveParry(character, rank: 1);
        _service.ApplyAtgeirWielderParryBonus(character);

        // Act
        _service.RemoveAllParryBonuses(character);

        // Assert
        Assert.AreEqual(0, _service.GetParryBonusDice(character),
            "All parry bonuses should be removed");
        Assert.IsFalse(_service.HasSuperiorRiposte(character),
            "Superior Riposte should be removed");
    }

    #endregion

    #region Turn Management Tests

    [TestMethod]
    public void ResetParriesForNewTurn_ResetsParryCount()
    {
        // Arrange
        var character = CreateTestCharacter();
        _service.ApplyHolmgangrReactiveParry(character, rank: 3); // 2 parries per round
        character.ParriesRemainingThisTurn = 0; // Simulate used parries

        // Act
        _service.ResetParriesForNewTurn(character);

        // Assert
        Assert.AreEqual(2, character.ParriesRemainingThisTurn,
            "Parries should be reset to maximum per round");
    }

    [TestMethod]
    public void CanParryThisTurn_WithRemainingParries_ReturnsTrue()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.ParriesRemainingThisTurn = 1;

        // Act
        var canParry = _service.CanParryThisTurn(character);

        // Assert
        Assert.IsTrue(canParry, "Should be able to parry when parries remain");
    }

    [TestMethod]
    public void CanParryThisTurn_WithNoParriesRemaining_ReturnsFalse()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.ParriesRemainingThisTurn = 0;

        // Act
        var canParry = _service.CanParryThisTurn(character);

        // Assert
        Assert.IsFalse(canParry, "Should NOT be able to parry when no parries remain");
    }

    [TestMethod]
    public void ConsumeParryAttempt_ReducesParryCount()
    {
        // Arrange
        var character = CreateTestCharacter();
        character.ParriesRemainingThisTurn = 2;

        // Act
        _service.ConsumeParryAttempt(character);

        // Assert
        Assert.AreEqual(1, character.ParriesRemainingThisTurn,
            "Parry count should be reduced by 1");
    }

    #endregion

    #region Statistics Tests

    [TestMethod]
    public void GetParryStatistics_NewCharacter_ReturnsEmptyStats()
    {
        // Arrange
        var character = CreateTestCharacter();

        // Act
        var stats = _service.GetParryStatistics(character);

        // Assert
        Assert.AreEqual(0, stats.TotalParryAttempts, "New character should have no attempts");
        Assert.AreEqual(0, stats.SuccessfulParries, "New character should have no successes");
    }

    [TestMethod]
    public void ExecuteParry_UpdatesStatistics()
    {
        // Arrange
        var defender = CreateTestCharacter(finesse: 10);
        var attacker = CreateTestEnemy();

        // Act
        var result = _service.ExecuteParry(defender, attacker, attackAccuracy: 10);
        var stats = _service.GetParryStatistics(defender);

        // Assert
        Assert.AreEqual(1, stats.TotalParryAttempts, "Should record 1 attempt");

        if (result.Success)
        {
            Assert.IsTrue(stats.SuccessfulParries > 0, "Success should be recorded");
        }
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestCharacter(int finesse = 6, int might = 5)
    {
        return new PlayerCharacter
        {
            Name = "TestCharacter" + Guid.NewGuid().ToString(),
            Attributes = new Attributes
            {
                Finesse = finesse,
                Might = might,
                Wits = 5,
                Will = 5,
                Sturdiness = 5
            },
            Class = CharacterClass.Warrior,
            HP = 50,
            MaxHP = 50,
            EquippedWeapon = new Equipment
            {
                Name = "Test Sword",
                Type = EquipmentType.Weapon,
                DamageDice = 2
            },
            ParriesRemainingThisTurn = 1
        };
    }

    private Enemy CreateTestEnemy(int hp = 30, int defense = 10, int soak = 2)
    {
        return new Enemy
        {
            Id = "test_enemy_" + Guid.NewGuid().ToString(),
            Name = "Test Enemy",
            HP = hp,
            MaxHP = hp,
            Defense = defense,
            Soak = soak,
            AttackDice = 3,
            DamageDice = 2
        };
    }

    #endregion
}
