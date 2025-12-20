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
using CharacterEntity = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the AttackResolutionService class.
/// Validates attack mechanics including hit determination, damage calculation, and stamina costs.
/// </summary>
public class AttackResolutionServiceTests
{
    private readonly Mock<IDiceService> _mockDice;
    private readonly Mock<IStatusEffectService> _mockStatusEffects;
    private readonly Mock<ITraumaService> _mockTraumaService;
    private readonly Mock<ILogger<AttackResolutionService>> _mockLogger;
    private readonly AttackResolutionService _sut;

    public AttackResolutionServiceTests()
    {
        _mockDice = new Mock<IDiceService>();
        _mockStatusEffects = new Mock<IStatusEffectService>();
        _mockTraumaService = new Mock<ITraumaService>();
        _mockLogger = new Mock<ILogger<AttackResolutionService>>();

        // Default status effect behavior: no modifiers
        _mockStatusEffects.Setup(s => s.GetDamageMultiplier(It.IsAny<Combatant>())).Returns(1.0f);
        _mockStatusEffects.Setup(s => s.GetSoakModifier(It.IsAny<Combatant>())).Returns(0);

        // Default trauma service behavior: no stress penalty
        _mockTraumaService.Setup(t => t.GetDefensePenalty(It.IsAny<int>())).Returns(0);

        _sut = new AttackResolutionService(
            _mockDice.Object,
            _mockStatusEffects.Object,
            _mockTraumaService.Object,
            _mockLogger.Object);
    }

    #region Helper Methods

    private static Combatant CreatePlayerCombatant(
        int might = 5,
        int finesse = 5,
        int sturdiness = 5,
        int stamina = 60,
        int weaponDamageDie = 6,
        int armorSoak = 0,
        string weaponName = "Sword")
    {
        var character = new CharacterEntity
        {
            Name = "Test Player",
            Might = might,
            Finesse = finesse,
            Sturdiness = sturdiness,
            CurrentStamina = stamina,
            MaxStamina = stamina
        };
        var combatant = Combatant.FromCharacter(character);
        // Override equipment stats for testing (FromCharacter defaults to unarmed d4)
        combatant.WeaponDamageDie = weaponDamageDie;
        combatant.ArmorSoak = armorSoak;
        combatant.WeaponName = weaponName;
        return combatant;
    }

    private static Combatant CreateEnemyCombatant(
        int might = 5,
        int finesse = 3,
        int sturdiness = 5,
        int hp = 50,
        int stamina = 35,
        int weaponDamageDie = 4,
        int armorSoak = 0,
        string weaponName = "Claws")
    {
        var enemy = new Enemy
        {
            Name = "Test Enemy",
            Attributes = new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Might, might },
                { CharacterAttribute.Finesse, finesse },
                { CharacterAttribute.Sturdiness, sturdiness },
                { CharacterAttribute.Wits, 3 },
                { CharacterAttribute.Will, 3 }
            },
            MaxHp = hp,
            CurrentHp = hp,
            MaxStamina = stamina,
            CurrentStamina = stamina,
            WeaponDamageDie = weaponDamageDie,
            ArmorSoak = armorSoak,
            WeaponName = weaponName
        };
        return Combatant.FromEnemy(enemy);
    }

    #endregion

    #region GetStaminaCost Tests

    [Theory]
    [InlineData(AttackType.Light, 15)]
    [InlineData(AttackType.Standard, 25)]
    [InlineData(AttackType.Heavy, 40)]
    public void GetStaminaCost_ReturnsCorrectCost(AttackType type, int expectedCost)
    {
        // Act
        var cost = _sut.GetStaminaCost(type);

        // Assert
        cost.Should().Be(expectedCost);
    }

    #endregion

    #region CanAffordAttack Tests

    [Fact]
    public void CanAffordAttack_WithSufficientStamina_ReturnsTrue()
    {
        // Arrange
        var combatant = CreatePlayerCombatant(stamina: 60);

        // Act
        var canAfford = _sut.CanAffordAttack(combatant, AttackType.Standard);

        // Assert
        canAfford.Should().BeTrue();
    }

    [Fact]
    public void CanAffordAttack_WithExactStamina_ReturnsTrue()
    {
        // Arrange
        var combatant = CreatePlayerCombatant(stamina: 25);

        // Act
        var canAfford = _sut.CanAffordAttack(combatant, AttackType.Standard);

        // Assert
        canAfford.Should().BeTrue();
    }

    [Fact]
    public void CanAffordAttack_WithInsufficientStamina_ReturnsFalse()
    {
        // Arrange
        var combatant = CreatePlayerCombatant(stamina: 24);

        // Act
        var canAfford = _sut.CanAffordAttack(combatant, AttackType.Standard);

        // Assert
        canAfford.Should().BeFalse();
    }

    [Fact]
    public void CanAffordAttack_HeavyAttack_RequiresMoreStamina()
    {
        // Arrange
        var combatant = CreatePlayerCombatant(stamina: 39);

        // Act
        var canAfford = _sut.CanAffordAttack(combatant, AttackType.Heavy);

        // Assert
        canAfford.Should().BeFalse();
    }

    #endregion

    #region CalculateDefenseScore Tests

    [Theory]
    [InlineData(1, 11)]   // 10 + 1 = 11
    [InlineData(5, 15)]   // 10 + 5 = 15
    [InlineData(10, 20)]  // 10 + 10 = 20
    public void CalculateDefenseScore_ReturnsCorrectValue(int finesse, int expectedDefense)
    {
        // Arrange
        var enemy = new Enemy
        {
            Attributes = new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Finesse, finesse },
                { CharacterAttribute.Sturdiness, 5 },
                { CharacterAttribute.Might, 5 },
                { CharacterAttribute.Wits, 3 },
                { CharacterAttribute.Will, 3 }
            }
        };
        var defender = Combatant.FromEnemy(enemy);

        // Act
        var defense = _sut.CalculateDefenseScore(defender);

        // Assert
        defense.Should().Be(expectedDefense);
    }

    [Fact]
    public void CalculateDefenseScore_IncludesStressPenalty()
    {
        // Arrange - defender with Finesse 5 and stress penalty of 3
        var enemy = new Enemy
        {
            Attributes = new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Finesse, 5 },
                { CharacterAttribute.Sturdiness, 5 },
                { CharacterAttribute.Might, 5 },
                { CharacterAttribute.Wits, 3 },
                { CharacterAttribute.Will, 3 }
            }
        };
        var defender = Combatant.FromEnemy(enemy);
        defender.CurrentStress = 60; // Distressed tier

        // Configure mock to return penalty of 3 for stress 60
        _mockTraumaService.Setup(t => t.GetDefensePenalty(60)).Returns(3);

        // Act
        var defense = _sut.CalculateDefenseScore(defender);

        // Assert - Defense = 10 + 5 (Finesse) - 3 (Stress Penalty) = 12
        defense.Should().Be(12);
        _mockTraumaService.Verify(t => t.GetDefensePenalty(60), Times.Once);
    }

    [Fact]
    public void CalculateDefenseScore_WithMaxStressPenalty()
    {
        // Arrange - defender at breaking point (100 stress = max penalty of 5)
        var enemy = new Enemy
        {
            Attributes = new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Finesse, 3 },
                { CharacterAttribute.Sturdiness, 5 },
                { CharacterAttribute.Might, 5 },
                { CharacterAttribute.Wits, 3 },
                { CharacterAttribute.Will, 3 }
            }
        };
        var defender = Combatant.FromEnemy(enemy);
        defender.CurrentStress = 100;

        // Configure mock to return max penalty of 5
        _mockTraumaService.Setup(t => t.GetDefensePenalty(100)).Returns(5);

        // Act
        var defense = _sut.CalculateDefenseScore(defender);

        // Assert - Defense = 10 + 3 (Finesse) - 5 (Max Stress Penalty) = 8
        defense.Should().Be(8);
    }

    #endregion

    #region GetSuccessThreshold Tests

    [Theory]
    [InlineData(10, 2)]   // 10 / 5 = 2
    [InlineData(11, 2)]   // 11 / 5 = 2 (rounds down)
    [InlineData(15, 3)]   // 15 / 5 = 3
    [InlineData(20, 4)]   // 20 / 5 = 4
    [InlineData(5, 1)]    // 5 / 5 = 1
    public void GetSuccessThreshold_ReturnsCorrectValue(int defense, int expectedThreshold)
    {
        // Act
        var threshold = _sut.GetSuccessThreshold(defense);

        // Assert
        threshold.Should().Be(expectedThreshold);
    }

    #endregion

    #region ResolveMeleeAttack - Outcome Tests

    [Fact]
    public void ResolveMeleeAttack_WhenMiss_ReturnsNoDamage()
    {
        // Arrange
        var attacker = CreatePlayerCombatant(might: 5);
        var defender = CreateEnemyCombatant(finesse: 5); // Defense 15, Threshold 3

        // Mock: Roll 2 successes (below threshold of 3)
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(2, 0, new[] { 8, 9, 3, 4, 2 }));

        // Act
        var result = _sut.ResolveMeleeAttack(attacker, defender, AttackType.Standard);

        // Assert
        result.IsHit.Should().BeFalse();
        result.Outcome.Should().Be(AttackOutcome.Miss);
        result.FinalDamage.Should().Be(0);
    }

    [Fact]
    public void ResolveMeleeAttack_WhenFumble_ReturnsNoDamage()
    {
        // Arrange
        var attacker = CreatePlayerCombatant(might: 5);
        var defender = CreateEnemyCombatant();

        // Mock: Roll 0 successes with botches
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(0, 2, new[] { 1, 1, 3, 4, 2 }));

        // Act
        var result = _sut.ResolveMeleeAttack(attacker, defender, AttackType.Standard);

        // Assert
        result.IsHit.Should().BeFalse();
        result.Outcome.Should().Be(AttackOutcome.Fumble);
        result.FinalDamage.Should().Be(0);
    }

    [Fact]
    public void ResolveMeleeAttack_DefenderWinsTies_IsMiss()
    {
        // Arrange
        var attacker = CreatePlayerCombatant(might: 5);
        var defender = CreateEnemyCombatant(finesse: 5); // Defense 15, Threshold 3

        // Mock: Roll exactly 3 successes (equals threshold, net = 0)
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(3, 0, new[] { 8, 9, 10, 4, 2 }));

        // Act
        var result = _sut.ResolveMeleeAttack(attacker, defender, AttackType.Standard);

        // Assert
        result.IsHit.Should().BeFalse();
        result.Outcome.Should().Be(AttackOutcome.Miss);
        result.NetSuccesses.Should().Be(0);
    }

    [Fact]
    public void ResolveMeleeAttack_GlancingBlow_HalvesDamage()
    {
        // Arrange - player with d6 sword vs enemy with no armor
        var attacker = CreatePlayerCombatant(might: 5, weaponDamageDie: 6);
        var defender = CreateEnemyCombatant(finesse: 3, armorSoak: 0); // Defense 13, Threshold 2

        // Mock: Roll 4 successes (net = 4 - 2 = 2, Glancing)
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(4, 0, new[] { 8, 9, 10, 8, 2 }));
        // Weapon damage: 4 (d6)
        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(4);

        // Act
        var result = _sut.ResolveMeleeAttack(attacker, defender, AttackType.Standard);

        // Assert
        result.IsHit.Should().BeTrue();
        result.Outcome.Should().Be(AttackOutcome.Glancing);
        // Raw = Might(5) + Weapon(4) + StandardBonus(2) = 11, halved = 5, no soak = 5
        result.FinalDamage.Should().Be(5);
    }

    [Fact]
    public void ResolveMeleeAttack_SolidHit_FullDamage()
    {
        // Arrange - player with d6 sword vs enemy with 2 armor soak
        var attacker = CreatePlayerCombatant(might: 5, weaponDamageDie: 6);
        var defender = CreateEnemyCombatant(finesse: 3, armorSoak: 2); // Defense 13, Threshold 2

        // Mock: Roll 5 successes (net = 5 - 2 = 3, Solid)
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(5, 0, new[] { 8, 9, 10, 8, 9 }));
        // Weapon damage: 4 (d6)
        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(4);

        // Act
        var result = _sut.ResolveMeleeAttack(attacker, defender, AttackType.Standard);

        // Assert
        result.IsHit.Should().BeTrue();
        result.Outcome.Should().Be(AttackOutcome.Solid);
        // Raw = Might(5) + Weapon(4) + StandardBonus(2) = 11, Soak = 2, Final = 9
        result.FinalDamage.Should().Be(9);
    }

    [Fact]
    public void ResolveMeleeAttack_CriticalHit_DoublesDamage()
    {
        // Arrange - player with d6 sword vs enemy with 2 armor soak
        var attacker = CreatePlayerCombatant(might: 5, weaponDamageDie: 6);
        var defender = CreateEnemyCombatant(finesse: 3, armorSoak: 2); // Defense 13, Threshold 2

        // Mock: Roll 8 successes (net = 8 - 2 = 6, Critical)
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(8, 0, new[] { 8, 9, 10, 8, 9, 10, 8, 9 }));
        // Weapon damage: 4 (d6)
        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(4);

        // Act
        var result = _sut.ResolveMeleeAttack(attacker, defender, AttackType.Standard);

        // Assert
        result.IsHit.Should().BeTrue();
        result.Outcome.Should().Be(AttackOutcome.Critical);
        // Raw = Might(5) + Weapon(4) + StandardBonus(2) = 11, Doubled = 22, Soak = 2, Final = 20
        result.FinalDamage.Should().Be(20);
    }

    #endregion

    #region ResolveMeleeAttack - Attack Type Modifiers

    [Fact]
    public void ResolveMeleeAttack_LightAttack_NoDamageBonus()
    {
        // Arrange - player with d6 sword, light attack has no damage bonus but +1 to hit
        var attacker = CreatePlayerCombatant(might: 5, weaponDamageDie: 6);
        var defender = CreateEnemyCombatant(finesse: 3, armorSoak: 0);

        // Mock: Roll 5 successes
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(5, 0, new[] { 8, 9, 10, 8, 9, 10 }));
        // Weapon damage: 3 (d6)
        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(3);

        // Act
        var result = _sut.ResolveMeleeAttack(attacker, defender, AttackType.Light);

        // Assert
        result.IsHit.Should().BeTrue();
        // Verify combatant's weapon die (d6) was used
        _mockDice.Verify(d => d.RollSingle(6, It.IsAny<string>()), Times.Once);
        // Verify pool was Might + 1 = 6 (light attack bonus to hit)
        _mockDice.Verify(d => d.Roll(6, It.IsAny<string>()), Times.Once);
        // Raw = Might(5) + Weapon(3) + LightBonus(0) = 8, no soak, Final = 8
        result.FinalDamage.Should().Be(8);
    }

    [Fact]
    public void ResolveMeleeAttack_HeavyAttack_AddsDamageBonus()
    {
        // Arrange - player with d6 sword, heavy attack adds +4 damage but -1 to hit
        var attacker = CreatePlayerCombatant(might: 5, weaponDamageDie: 6);
        var defender = CreateEnemyCombatant(finesse: 3, armorSoak: 0);

        // Mock: Roll 5 successes
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(5, 0, new[] { 8, 9, 10, 8 }));
        // Weapon damage: 4 (d6)
        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(4);

        // Act
        var result = _sut.ResolveMeleeAttack(attacker, defender, AttackType.Heavy);

        // Assert
        result.IsHit.Should().BeTrue();
        // Verify combatant's weapon die (d6) was used
        _mockDice.Verify(d => d.RollSingle(6, It.IsAny<string>()), Times.Once);
        // Verify pool was Might - 1 = 4 (heavy attack penalty to hit)
        _mockDice.Verify(d => d.Roll(4, It.IsAny<string>()), Times.Once);
        // Raw = Might(5) + Weapon(4) + HeavyBonus(4) = 13, no soak, Final = 13
        result.FinalDamage.Should().Be(13);
    }

    [Fact]
    public void ResolveMeleeAttack_StandardAttack_UsesWeaponDieAndDamageBonus()
    {
        // Arrange - player with d6 sword, standard attack adds +2 damage
        var attacker = CreatePlayerCombatant(might: 5, weaponDamageDie: 6);
        var defender = CreateEnemyCombatant(finesse: 3, armorSoak: 0);

        // Mock: Roll 5 successes
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(5, 0, new[] { 8, 9, 10, 8, 9 }));
        // Weapon damage: 4 (d6)
        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(4);

        // Act
        var result = _sut.ResolveMeleeAttack(attacker, defender, AttackType.Standard);

        // Assert
        result.IsHit.Should().BeTrue();
        // Verify combatant's weapon die (d6) was used
        _mockDice.Verify(d => d.RollSingle(6, It.IsAny<string>()), Times.Once);
        // Verify pool was exactly Might = 5 (no modifier for standard)
        _mockDice.Verify(d => d.Roll(5, It.IsAny<string>()), Times.Once);
        // Raw = Might(5) + Weapon(4) + StandardBonus(2) = 11, no soak, Final = 11
        result.FinalDamage.Should().Be(11);
    }

    #endregion

    #region ResolveMeleeAttack - Soak and Minimum Damage

    [Fact]
    public void ResolveMeleeAttack_MinimumOneDamage_WhenHit()
    {
        // Arrange - low might vs high armor soak
        var attacker = CreatePlayerCombatant(might: 1, weaponDamageDie: 6);
        var defender = CreateEnemyCombatant(finesse: 1, armorSoak: 20); // High armor soak

        // Mock: Roll 4 successes (net = 4 - 2 = 2, Glancing)
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(4, 0, new[] { 8, 9, 10, 8 }));
        // Minimum weapon damage: 1
        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(1);

        // Act
        var result = _sut.ResolveMeleeAttack(attacker, defender, AttackType.Standard);

        // Assert
        result.IsHit.Should().BeTrue();
        // Raw = 1 + 1 = 2, Glancing = 1, Soak = 10, should be minimum 1
        result.FinalDamage.Should().Be(1);
    }

    [Fact]
    public void ResolveMeleeAttack_ArmorSoakReducesDamage()
    {
        // Arrange - player with d6 sword vs enemy with 5 armor soak
        var attacker = CreatePlayerCombatant(might: 5, weaponDamageDie: 6);
        var defender = CreateEnemyCombatant(finesse: 1, armorSoak: 5);

        // Mock: Roll 5 successes (net = 5 - 2 = 3, Solid)
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(5, 0, new[] { 8, 9, 10, 8, 9 }));
        // Weapon damage: 4
        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(4);

        // Act
        var result = _sut.ResolveMeleeAttack(attacker, defender, AttackType.Standard);

        // Assert
        result.IsHit.Should().BeTrue();
        // Raw = Might(5) + Weapon(4) + StandardBonus(2) = 11, Soak = 5, Final = 6
        result.RawDamage.Should().Be(11);
        result.FinalDamage.Should().Be(6);
    }

    #endregion

    #region ResolveMeleeAttack - Edge Cases

    [Fact]
    public void ResolveMeleeAttack_LowMightWithHeavyPenalty_MinimumOneDie()
    {
        // Arrange - Might 1 with Heavy penalty (-1) should still roll at least 1 die
        var attacker = CreatePlayerCombatant(might: 1, weaponDamageDie: 6);
        var defender = CreateEnemyCombatant(finesse: 1, armorSoak: 0);

        // Mock: Roll 3 successes
        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(3, 0, new[] { 8 }));
        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(4);

        // Act
        var result = _sut.ResolveMeleeAttack(attacker, defender, AttackType.Heavy);

        // Assert
        // Should have rolled at least 1 die even with penalty bringing pool to 0
        _mockDice.Verify(d => d.Roll(1, It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region Equipment Integration Tests

    [Fact]
    public void ResolveMeleeAttack_UsesAttackerWeaponDamageDie()
    {
        // Arrange - attacker with d8 weapon
        var attacker = CreatePlayerCombatant(might: 5, weaponDamageDie: 8, weaponName: "Greataxe");
        var defender = CreateEnemyCombatant(finesse: 3, armorSoak: 0);

        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(5, 0, new[] { 8, 9, 10, 8, 9 }));
        _mockDice.Setup(d => d.RollSingle(8, It.IsAny<string>()))
            .Returns(6);

        // Act
        _sut.ResolveMeleeAttack(attacker, defender, AttackType.Standard);

        // Assert - verify d8 was rolled, not d6
        _mockDice.Verify(d => d.RollSingle(8, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void ResolveMeleeAttack_AppliesDefenderArmorSoak()
    {
        // Arrange - defender with 10 armor soak
        var attacker = CreatePlayerCombatant(might: 5, weaponDamageDie: 6);
        var defender = CreateEnemyCombatant(finesse: 3, armorSoak: 10);

        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(5, 0, new[] { 8, 9, 10, 8, 9 }));
        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>()))
            .Returns(4);

        // Act
        var result = _sut.ResolveMeleeAttack(attacker, defender, AttackType.Standard);

        // Assert
        result.IsHit.Should().BeTrue();
        // Raw = Might(5) + Weapon(4) + StandardBonus(2) = 11, Soak = 10, Final = 1 (minimum)
        result.FinalDamage.Should().Be(1);
    }

    [Fact]
    public void ResolveMeleeAttack_SoakCannotReduceBelowOne()
    {
        // Arrange - massive armor soak vs weak attack
        var attacker = CreatePlayerCombatant(might: 1, weaponDamageDie: 4);
        var defender = CreateEnemyCombatant(finesse: 1, armorSoak: 100);

        _mockDice.Setup(d => d.Roll(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new DiceResult(5, 0, new[] { 8, 9, 10, 8, 9 }));
        _mockDice.Setup(d => d.RollSingle(4, It.IsAny<string>()))
            .Returns(1);

        // Act
        var result = _sut.ResolveMeleeAttack(attacker, defender, AttackType.Light);

        // Assert
        result.IsHit.Should().BeTrue();
        // Even with 100 soak, minimum damage is 1
        result.FinalDamage.Should().Be(1);
    }

    #endregion
}
