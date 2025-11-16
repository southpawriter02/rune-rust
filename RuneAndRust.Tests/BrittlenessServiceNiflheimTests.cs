using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.30.2: Tests for Brittleness Service (Niflheim variant)
/// Tests [Brittle] debuff mechanic for Niflheim biome
/// - Enemy has Ice Resistance > 0%
/// - Enemy takes Ice damage
/// - Apply [Brittle] debuff for 1 turn (Niflheim-specific duration)
/// - While [Brittle]: Physical damage +50%
/// </summary>
[TestClass]
public class BrittlenessServiceNiflheimTests
{
    private BrittlenessService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _service = new BrittlenessService();
    }

    #region Ice Resistance Eligibility Tests

    [TestMethod]
    public void IsBrittleEligibleNiflheim_EnemyWithIceResistance_ReturnsTrue()
    {
        // Arrange
        var enemy = CreateTestEnemy("Ice-Resistant Enemy", 50, 50);
        _service.SetEnemyResistance(enemy.EnemyID, "Ice", 50);

        // Act
        bool eligible = _service.IsBrittleEligibleNiflheim(enemy);

        // Assert
        Assert.IsTrue(eligible, "Enemy with Ice Resistance > 0% should be eligible for [Brittle]");
    }

    [TestMethod]
    public void IsBrittleEligibleNiflheim_EnemyWithoutIceResistance_ReturnsFalse()
    {
        // Arrange
        var enemy = CreateTestEnemy("No Ice Resistance", 50, 50);
        _service.SetEnemyResistance(enemy.EnemyID, "Ice", 0);

        // Act
        bool eligible = _service.IsBrittleEligibleNiflheim(enemy);

        // Assert
        Assert.IsFalse(eligible, "Enemy without Ice Resistance should NOT be eligible");
    }

    [TestMethod]
    public void IsBrittleEligibleNiflheim_EnemyWithNegativeIceResistance_ReturnsFalse()
    {
        // Arrange
        var enemy = CreateTestEnemy("Ice-Vulnerable Enemy", 50, 50);
        _service.SetEnemyResistance(enemy.EnemyID, "Ice", -25); // Vulnerable

        // Act
        bool eligible = _service.IsBrittleEligibleNiflheim(enemy);

        // Assert
        Assert.IsFalse(eligible, "Enemy with negative Ice Resistance should NOT be eligible");
    }

    #endregion

    #region Apply Brittle Tests

    [TestMethod]
    public void ApplyBrittle_NewBrittle_AddsStatusEffect()
    {
        // Arrange
        var enemy = CreateTestEnemy("Test Enemy", 50, 50);

        // Act
        _service.ApplyBrittle(enemy);

        // Assert
        Assert.AreEqual(1, enemy.StatusEffects.Count, "Should add [Brittle] status effect");
        var brittle = enemy.StatusEffects.First();
        Assert.AreEqual("Brittle", brittle.EffectType);
        Assert.AreEqual(1, brittle.DurationRemaining, "Niflheim Brittle should last 1 turn");
        Assert.AreEqual(1, brittle.StackCount);
    }

    [TestMethod]
    public void ApplyBrittle_WithCustomDuration_UsesThatDuration()
    {
        // Arrange
        var enemy = CreateTestEnemy("Test Enemy", 50, 50);
        int customDuration = 3;

        // Act
        _service.ApplyBrittle(enemy, customDuration);

        // Assert
        var brittle = enemy.StatusEffects.First();
        Assert.AreEqual(3, brittle.DurationRemaining, "Should use custom duration");
    }

    [TestMethod]
    public void ApplyBrittle_AlreadyBrittle_RefreshesDuration()
    {
        // Arrange
        var enemy = CreateTestEnemy("Test Enemy", 50, 50);
        _service.ApplyBrittle(enemy);

        // Simulate duration decay
        enemy.StatusEffects.First().DurationRemaining = 0;

        // Act
        _service.ApplyBrittle(enemy);

        // Assert
        Assert.AreEqual(1, enemy.StatusEffects.Count, "Should not add duplicate [Brittle]");
        var brittle = enemy.StatusEffects.First();
        Assert.AreEqual(1, brittle.DurationRemaining, "Should refresh duration to 1 turn");
    }

    #endregion

    #region Has Brittle Tests

    [TestMethod]
    public void HasBrittle_EnemyWithBrittle_ReturnsTrue()
    {
        // Arrange
        var enemy = CreateTestEnemy("Test Enemy", 50, 50);
        _service.ApplyBrittle(enemy);

        // Act
        bool hasBrittle = _service.HasBrittle(enemy);

        // Assert
        Assert.IsTrue(hasBrittle, "Enemy should have [Brittle]");
    }

    [TestMethod]
    public void HasBrittle_EnemyWithoutBrittle_ReturnsFalse()
    {
        // Arrange
        var enemy = CreateTestEnemy("Test Enemy", 50, 50);

        // Act
        bool hasBrittle = _service.HasBrittle(enemy);

        // Assert
        Assert.IsFalse(hasBrittle, "Enemy without [Brittle] should return false");
    }

    [TestMethod]
    public void HasBrittle_BrittleExpired_ReturnsFalse()
    {
        // Arrange
        var enemy = CreateTestEnemy("Test Enemy", 50, 50);
        _service.ApplyBrittle(enemy);

        // Expire the effect
        enemy.StatusEffects.First().DurationRemaining = 0;

        // Act
        bool hasBrittle = _service.HasBrittle(enemy);

        // Assert
        Assert.IsFalse(hasBrittle, "Expired [Brittle] should return false");
    }

    #endregion

    #region Try Apply Brittle Niflheim Tests

    [TestMethod]
    public void TryApplyBrittleNiflheim_EligibleEnemy_AppliesBrittle()
    {
        // Arrange
        var enemy = CreateTestEnemy("Ice-Resistant Enemy", 50, 50);
        _service.SetEnemyResistance(enemy.EnemyID, "Ice", 50);
        int iceDamage = 10;

        // Act
        _service.TryApplyBrittleNiflheim(enemy, iceDamage);

        // Assert
        Assert.AreEqual(1, enemy.StatusEffects.Count, "Should apply [Brittle]");
        Assert.AreEqual("Brittle", enemy.StatusEffects.First().EffectType);
    }

    [TestMethod]
    public void TryApplyBrittleNiflheim_NotEligibleEnemy_DoesNotApplyBrittle()
    {
        // Arrange
        var enemy = CreateTestEnemy("No Ice Resistance", 50, 50);
        _service.SetEnemyResistance(enemy.EnemyID, "Ice", 0);
        int iceDamage = 10;

        // Act
        _service.TryApplyBrittleNiflheim(enemy, iceDamage);

        // Assert
        Assert.AreEqual(0, enemy.StatusEffects.Count, "Should NOT apply [Brittle]");
    }

    [TestMethod]
    public void TryApplyBrittleNiflheim_ZeroIceDamage_DoesNotApplyBrittle()
    {
        // Arrange
        var enemy = CreateTestEnemy("Ice-Resistant Enemy", 50, 50);
        _service.SetEnemyResistance(enemy.EnemyID, "Ice", 50);
        int iceDamage = 0;

        // Act
        _service.TryApplyBrittleNiflheim(enemy, iceDamage);

        // Assert
        Assert.AreEqual(0, enemy.StatusEffects.Count, "Should NOT apply [Brittle] with 0 damage");
    }

    [TestMethod]
    public void TryApplyBrittleNiflheim_NegativeIceDamage_DoesNotApplyBrittle()
    {
        // Arrange
        var enemy = CreateTestEnemy("Ice-Resistant Enemy", 50, 50);
        _service.SetEnemyResistance(enemy.EnemyID, "Ice", 50);
        int iceDamage = -5;

        // Act
        _service.TryApplyBrittleNiflheim(enemy, iceDamage);

        // Assert
        Assert.AreEqual(0, enemy.StatusEffects.Count, "Should NOT apply [Brittle] with negative damage");
    }

    #endregion

    #region Apply Brittle Bonus Tests

    [TestMethod]
    public void ApplyBrittleBonus_BrittleEnemy_Amplifies50Percent()
    {
        // Arrange
        var enemy = CreateTestEnemy("Brittle Enemy", 50, 50);
        _service.ApplyBrittle(enemy);
        int basePhysicalDamage = 20;

        // Act
        int amplifiedDamage = _service.ApplyBrittleBonus(enemy, basePhysicalDamage);

        // Assert
        Assert.AreEqual(30, amplifiedDamage, "Physical damage should be amplified by +50%");
    }

    [TestMethod]
    public void ApplyBrittleBonus_NotBrittleEnemy_NoAmplification()
    {
        // Arrange
        var enemy = CreateTestEnemy("Normal Enemy", 50, 50);
        int basePhysicalDamage = 20;

        // Act
        int amplifiedDamage = _service.ApplyBrittleBonus(enemy, basePhysicalDamage);

        // Assert
        Assert.AreEqual(20, amplifiedDamage, "Damage should not be amplified without [Brittle]");
    }

    [TestMethod]
    public void ApplyBrittleBonus_OddBaseDamage_RoundsCorrectly()
    {
        // Arrange
        var enemy = CreateTestEnemy("Brittle Enemy", 50, 50);
        _service.ApplyBrittle(enemy);
        int basePhysicalDamage = 7; // 7 * 1.5 = 10.5 → should be 7 + 3 = 10

        // Act
        int amplifiedDamage = _service.ApplyBrittleBonus(enemy, basePhysicalDamage);

        // Assert
        Assert.AreEqual(10, amplifiedDamage, "Odd damage should calculate bonus correctly (7 + 3 = 10)");
    }

    [TestMethod]
    public void ApplyBrittleBonus_ZeroDamage_RemainsZero()
    {
        // Arrange
        var enemy = CreateTestEnemy("Brittle Enemy", 50, 50);
        _service.ApplyBrittle(enemy);
        int basePhysicalDamage = 0;

        // Act
        int amplifiedDamage = _service.ApplyBrittleBonus(enemy, basePhysicalDamage);

        // Assert
        Assert.AreEqual(0, amplifiedDamage, "Zero damage should remain zero");
    }

    #endregion

    #region Resistance Management Tests

    [TestMethod]
    public void SetEnemyResistance_StoresCorrectly()
    {
        // Arrange
        var enemy = CreateTestEnemy("Test Enemy", 50, 50);

        // Act
        _service.SetEnemyResistance(enemy.EnemyID, "Ice", 50);
        int resistance = _service.GetEnemyResistance(enemy.EnemyID, "Ice");

        // Assert
        Assert.AreEqual(50, resistance, "Should store and retrieve Ice Resistance correctly");
    }

    [TestMethod]
    public void GetEnemyResistance_NoResistanceSet_ReturnsZero()
    {
        // Arrange
        var enemy = CreateTestEnemy("Test Enemy", 50, 50);

        // Act
        int resistance = _service.GetEnemyResistance(enemy.EnemyID, "Ice");

        // Assert
        Assert.AreEqual(0, resistance, "Should return 0 when no resistance is set");
    }

    [TestMethod]
    public void SetEnemyResistance_MultipleDamageTypes_StoresSeparately()
    {
        // Arrange
        var enemy = CreateTestEnemy("Test Enemy", 50, 50);

        // Act
        _service.SetEnemyResistance(enemy.EnemyID, "Ice", 50);
        _service.SetEnemyResistance(enemy.EnemyID, "Fire", 25);

        // Assert
        Assert.AreEqual(50, _service.GetEnemyResistance(enemy.EnemyID, "Ice"));
        Assert.AreEqual(25, _service.GetEnemyResistance(enemy.EnemyID, "Fire"));
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void FullWorkflow_IceDamageToIceResistantEnemy_AppliesBrittleThenAmplifies()
    {
        // Arrange
        var enemy = CreateTestEnemy("Frost-Rimed Undying", 50, 50);
        _service.SetEnemyResistance(enemy.EnemyID, "Ice", 50);

        int iceDamage = 10;
        int physicalDamage = 20;

        // Act - Phase 1: Ice damage applies Brittle
        _service.TryApplyBrittleNiflheim(enemy, iceDamage);

        // Act - Phase 2: Physical damage amplified
        int amplifiedPhysicalDamage = _service.ApplyBrittleBonus(enemy, physicalDamage);

        // Assert
        Assert.IsTrue(_service.HasBrittle(enemy), "Enemy should have [Brittle]");
        Assert.AreEqual(30, amplifiedPhysicalDamage, "Physical damage should be amplified by +50%");
    }

    [TestMethod]
    public void DurationComparison_NiflheimVsMuspelheim()
    {
        // Arrange
        var niflheimEnemy = CreateTestEnemy("Niflheim Enemy", 50, 50);

        // Act - Apply Niflheim variant (1 turn)
        _service.ApplyBrittle(niflheimEnemy);

        // Assert
        var niflheimBrittle = niflheimEnemy.StatusEffects.First();
        Assert.AreEqual(1, niflheimBrittle.DurationRemaining,
            "Niflheim [Brittle] should last 1 turn (vs Muspelheim's 2 turns)");
    }

    #endregion

    #region Helper Methods

    private Enemy CreateTestEnemy(string name, int hp, int maxHp)
    {
        return new Enemy
        {
            EnemyID = new Random().Next(1000, 9999), // Random ID to avoid conflicts
            Name = name,
            HP = hp,
            MaxHP = maxHp,
            StatusEffects = new List<StatusEffect>()
        };
    }

    #endregion
}
