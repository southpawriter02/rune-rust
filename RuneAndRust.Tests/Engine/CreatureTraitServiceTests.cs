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
/// Tests for the CreatureTraitService class.
/// Validates trait generation, application, and runtime effect processing.
/// </summary>
public class CreatureTraitServiceTests
{
    private readonly Mock<IDiceService> _mockDice;
    private readonly Mock<ILogger<CreatureTraitService>> _mockLogger;
    private readonly CreatureTraitService _sut;

    public CreatureTraitServiceTests()
    {
        _mockDice = new Mock<IDiceService>();
        _mockLogger = new Mock<ILogger<CreatureTraitService>>();
        _sut = new CreatureTraitService(_mockDice.Object, _mockLogger.Object);
    }

    #region EnhanceEnemy Tests

    [Fact]
    public void EnhanceEnemy_EliteTier_AddsOneTrait()
    {
        // Arrange
        var enemy = CreateEliteEnemy();
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(1); // Select first trait (Armored)

        // Act
        _sut.EnhanceEnemy(enemy);

        // Assert
        enemy.ActiveTraits.Should().HaveCount(1);
    }

    [Fact]
    public void EnhanceEnemy_ChampionTier_AddsTwoTraits()
    {
        // Arrange
        var enemy = CreateChampionEnemy();
        _mockDice.SetupSequence(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(1)  // First trait
            .Returns(1); // Second trait

        // Act
        _sut.EnhanceEnemy(enemy);

        // Assert
        enemy.ActiveTraits.Should().HaveCount(2);
    }

    [Fact]
    public void EnhanceEnemy_BossTier_AddsThreeTraits()
    {
        // Arrange
        var enemy = CreateBossEnemy();
        _mockDice.SetupSequence(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(1)  // First trait
            .Returns(1)  // Second trait
            .Returns(1); // Third trait

        // Act
        _sut.EnhanceEnemy(enemy);

        // Assert
        enemy.ActiveTraits.Should().HaveCount(3);
    }

    [Fact]
    public void EnhanceEnemy_StandardTier_NoTraits()
    {
        // Arrange
        var enemy = CreateStandardEnemy();

        // Act
        _sut.EnhanceEnemy(enemy);

        // Assert
        enemy.ActiveTraits.Should().BeEmpty();
    }

    [Fact]
    public void EnhanceEnemy_Armored_IncreasesArmorSoak()
    {
        // Arrange
        var enemy = CreateEliteEnemy();
        enemy.ArmorSoak = 2;
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(1); // Armored is first in enum order

        // Act
        _sut.EnhanceEnemy(enemy);

        // Assert
        enemy.ArmorSoak.Should().Be(5); // 2 + 3
        enemy.ActiveTraits.Should().Contain(CreatureTraitType.Armored);
    }

    [Fact]
    public void EnhanceEnemy_Relentless_IncreasesHpAndAddsImmunity()
    {
        // Arrange
        var enemy = CreateEliteEnemy();
        enemy.MaxHp = 100;
        enemy.CurrentHp = 80;
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(2); // Relentless is second in enum order

        // Act
        _sut.EnhanceEnemy(enemy);

        // Assert
        enemy.MaxHp.Should().Be(150); // 100 * 1.5
        enemy.CurrentHp.Should().Be(150); // Full heal on enhancement
        enemy.Tags.Should().Contain("ImmuneToStun");
        enemy.ActiveTraits.Should().Contain(CreatureTraitType.Relentless);
    }

    [Fact]
    public void EnhanceEnemy_PrefixesName()
    {
        // Arrange
        var enemy = CreateEliteEnemy();
        enemy.Name = "Test Enemy";
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(1); // Armored

        // Act
        _sut.EnhanceEnemy(enemy);

        // Assert
        enemy.Name.Should().StartWith("Armored ");
    }

    [Fact]
    public void EnhanceEnemy_MechanicalEnemy_CannotBeVampiric()
    {
        // Arrange
        var enemy = CreateEliteEnemy();
        enemy.Tags.Add("Mechanical");

        // Setup dice to always return index for Vampiric - it should be excluded
        var callCount = 0;
        _mockDice.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(() =>
            {
                callCount++;
                return 1; // Always pick first available
            });

        // Act
        _sut.EnhanceEnemy(enemy);

        // Assert
        enemy.ActiveTraits.Should().NotContain(CreatureTraitType.Vampiric);
    }

    #endregion

    #region ProcessTraitTurnStart Tests

    [Fact]
    public void ProcessTraitTurnStart_Regenerating_HealsEnemy()
    {
        // Arrange
        var enemy = CreateEliteEnemy();
        enemy.ActiveTraits.Add(CreatureTraitType.Regenerating);
        var combatant = Combatant.FromEnemy(enemy);
        combatant.MaxHp = 100;
        combatant.CurrentHp = 50;

        // Act
        var healAmount = _sut.ProcessTraitTurnStart(combatant);

        // Assert
        healAmount.Should().Be(10); // 10% of 100
        combatant.CurrentHp.Should().Be(60);
    }

    [Fact]
    public void ProcessTraitTurnStart_NoTrait_ReturnsZero()
    {
        // Arrange
        var enemy = CreateStandardEnemy();
        var combatant = Combatant.FromEnemy(enemy);

        // Act
        var healAmount = _sut.ProcessTraitTurnStart(combatant);

        // Assert
        healAmount.Should().Be(0);
    }

    [Fact]
    public void ProcessTraitTurnStart_AtMaxHp_DoesNotOverheal()
    {
        // Arrange
        var enemy = CreateEliteEnemy();
        enemy.ActiveTraits.Add(CreatureTraitType.Regenerating);
        var combatant = Combatant.FromEnemy(enemy);
        combatant.MaxHp = 100;
        combatant.CurrentHp = 95;

        // Act
        var healAmount = _sut.ProcessTraitTurnStart(combatant);

        // Assert
        healAmount.Should().Be(5); // Only heals to max, not full 10%
        combatant.CurrentHp.Should().Be(100);
    }

    #endregion

    #region ProcessTraitOnDeath Tests

    [Fact]
    public void ProcessTraitOnDeath_Explosive_DamagesAllCombatants()
    {
        // Arrange
        var explosiveEnemy = CreateEliteEnemy();
        explosiveEnemy.ActiveTraits.Add(CreatureTraitType.Explosive);
        var victim = Combatant.FromEnemy(explosiveEnemy);

        var otherEnemy = CreateStandardEnemy();
        var otherCombatant = Combatant.FromEnemy(otherEnemy);

        var player = CreatePlayerCombatant();

        var allCombatants = new List<Combatant> { victim, otherCombatant, player };

        // Act
        var results = _sut.ProcessTraitOnDeath(victim, allCombatants);

        // Assert
        results.Should().HaveCount(2); // Other enemy + player
        results.All(r => r.Damage == 15).Should().BeTrue();
    }

    [Fact]
    public void ProcessTraitOnDeath_Explosive_DoesNotDamageSelf()
    {
        // Arrange
        var explosiveEnemy = CreateEliteEnemy();
        explosiveEnemy.ActiveTraits.Add(CreatureTraitType.Explosive);
        var victim = Combatant.FromEnemy(explosiveEnemy);

        var allCombatants = new List<Combatant> { victim };

        // Act
        var results = _sut.ProcessTraitOnDeath(victim, allCombatants);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void ProcessTraitOnDeath_NoTrait_ReturnsEmpty()
    {
        // Arrange
        var enemy = CreateStandardEnemy();
        var victim = Combatant.FromEnemy(enemy);
        var player = CreatePlayerCombatant();

        var allCombatants = new List<Combatant> { victim, player };

        // Act
        var results = _sut.ProcessTraitOnDeath(victim, allCombatants);

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region ProcessTraitOnDamageDealt Tests

    [Fact]
    public void ProcessTraitOnDamageDealt_Vampiric_HealsAttacker()
    {
        // Arrange
        var enemy = CreateEliteEnemy();
        enemy.ActiveTraits.Add(CreatureTraitType.Vampiric);
        var attacker = Combatant.FromEnemy(enemy);
        attacker.MaxHp = 100;
        attacker.CurrentHp = 50;

        // Act
        var healAmount = _sut.ProcessTraitOnDamageDealt(attacker, 40);

        // Assert
        healAmount.Should().Be(10); // 25% of 40
        attacker.CurrentHp.Should().Be(60);
    }

    [Fact]
    public void ProcessTraitOnDamageDealt_NoTrait_ReturnsZero()
    {
        // Arrange
        var enemy = CreateStandardEnemy();
        var attacker = Combatant.FromEnemy(enemy);

        // Act
        var healAmount = _sut.ProcessTraitOnDamageDealt(attacker, 40);

        // Assert
        healAmount.Should().Be(0);
    }

    [Fact]
    public void ProcessTraitOnDamageDealt_Vampiric_DoesNotOverheal()
    {
        // Arrange
        var enemy = CreateEliteEnemy();
        enemy.ActiveTraits.Add(CreatureTraitType.Vampiric);
        var attacker = Combatant.FromEnemy(enemy);
        attacker.MaxHp = 100;
        attacker.CurrentHp = 95;

        // Act
        var healAmount = _sut.ProcessTraitOnDamageDealt(attacker, 40);

        // Assert
        healAmount.Should().Be(5); // Only heals to max
        attacker.CurrentHp.Should().Be(100);
    }

    #endregion

    #region ProcessTraitOnDamageReceived Tests

    [Fact]
    public void ProcessTraitOnDamageReceived_Thorns_ReturnsDamage()
    {
        // Arrange
        var enemy = CreateEliteEnemy();
        enemy.ActiveTraits.Add(CreatureTraitType.Thorns);
        var defender = Combatant.FromEnemy(enemy);
        var attacker = CreatePlayerCombatant();

        // Act
        var thornsDamage = _sut.ProcessTraitOnDamageReceived(defender, attacker, 40);

        // Assert
        thornsDamage.Should().Be(10); // 25% of 40
    }

    [Fact]
    public void ProcessTraitOnDamageReceived_NoTrait_ReturnsZero()
    {
        // Arrange
        var enemy = CreateStandardEnemy();
        var defender = Combatant.FromEnemy(enemy);
        var attacker = CreatePlayerCombatant();

        // Act
        var thornsDamage = _sut.ProcessTraitOnDamageReceived(defender, attacker, 40);

        // Assert
        thornsDamage.Should().Be(0);
    }

    #endregion

    #region IsImmuneToEffect Tests

    [Fact]
    public void IsImmuneToEffect_Relentless_ImmuneToStun()
    {
        // Arrange
        var enemy = CreateEliteEnemy();
        enemy.ActiveTraits.Add(CreatureTraitType.Relentless);
        var combatant = Combatant.FromEnemy(enemy);

        // Act
        var isImmune = _sut.IsImmuneToEffect(combatant, StatusEffectType.Stunned);

        // Assert
        isImmune.Should().BeTrue();
    }

    [Fact]
    public void IsImmuneToEffect_Relentless_NotImmuneToOtherEffects()
    {
        // Arrange
        var enemy = CreateEliteEnemy();
        enemy.ActiveTraits.Add(CreatureTraitType.Relentless);
        var combatant = Combatant.FromEnemy(enemy);

        // Act & Assert
        _sut.IsImmuneToEffect(combatant, StatusEffectType.Bleeding).Should().BeFalse();
        _sut.IsImmuneToEffect(combatant, StatusEffectType.Poisoned).Should().BeFalse();
        _sut.IsImmuneToEffect(combatant, StatusEffectType.Vulnerable).Should().BeFalse();
    }

    [Fact]
    public void IsImmuneToEffect_NoTrait_NotImmune()
    {
        // Arrange
        var enemy = CreateStandardEnemy();
        var combatant = Combatant.FromEnemy(enemy);

        // Act
        var isImmune = _sut.IsImmuneToEffect(combatant, StatusEffectType.Stunned);

        // Assert
        isImmune.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private static Enemy CreateStandardEnemy()
    {
        return new Enemy
        {
            Name = "Standard Enemy",
            MaxHp = 50,
            CurrentHp = 50,
            MaxStamina = 30,
            CurrentStamina = 30,
            Tags = new List<string>()
        };
    }

    private static Enemy CreateEliteEnemy()
    {
        return new Enemy
        {
            Name = "Elite Enemy",
            MaxHp = 75,
            CurrentHp = 75,
            MaxStamina = 40,
            CurrentStamina = 40,
            Tags = new List<string> { "Elite" }
        };
    }

    private static Enemy CreateChampionEnemy()
    {
        return new Enemy
        {
            Name = "Champion Enemy",
            MaxHp = 100,
            CurrentHp = 100,
            MaxStamina = 50,
            CurrentStamina = 50,
            Tags = new List<string> { "Champion" }
        };
    }

    private static Enemy CreateBossEnemy()
    {
        return new Enemy
        {
            Name = "Boss Enemy",
            MaxHp = 200,
            CurrentHp = 200,
            MaxStamina = 80,
            CurrentStamina = 80,
            Tags = new List<string> { "Boss" }
        };
    }

    private static Combatant CreatePlayerCombatant()
    {
        return new Combatant
        {
            Name = "Test Player",
            IsPlayer = true,
            MaxHp = 100,
            CurrentHp = 100,
            MaxStamina = 50,
            CurrentStamina = 50
        };
    }

    #endregion
}
