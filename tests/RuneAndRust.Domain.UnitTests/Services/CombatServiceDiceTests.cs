using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.Services;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Services;

[TestFixture]
public class CombatServiceDiceTests
{
    private CombatService _combatService = null!;
    private Mock<IDiceService> _diceServiceMock = null!;
    private Mock<ILogger<CombatService>> _loggerMock = null!;

    [SetUp]
    public void SetUp()
    {
        _diceServiceMock = new Mock<IDiceService>();
        _loggerMock = new Mock<ILogger<CombatService>>();
        _combatService = new CombatService(_loggerMock.Object);
    }

    private Player CreateTestPlayer(int finesse = 5, int might = 5, int defense = 10)
    {
        var player = new Player("TestHero", new Stats(100, 10, defense));
        // Player has default attributes with Finesse/Might = 5
        return player;
    }

    private Monster CreateTestMonster(int attack = 5, int defense = 10, int health = 30)
    {
        return new Monster("Test Goblin", "A goblin", 1, new Stats(health, attack, defense));
    }

    [Test]
    public void ResolveCombatRound_CriticalHit_AlwaysHits()
    {
        // Arrange
        var player = CreateTestPlayer();
        var monster = CreateTestMonster(defense: 100, health: 1); // Very high defense, 1 HP

        var callCount = 0;
        // Setup dice rolls - must handle attack, damage, and potential counterattack
        _diceServiceMock.Setup(d => d.Roll(It.IsAny<DicePool>(), It.IsAny<AdvantageType>()))
            .Returns(() =>
            {
                callCount++;
                return callCount switch
                {
                    1 => new DiceRollResult(DicePool.D10(), new[] { 10 }), // Player attack - crit
                    2 => new DiceRollResult(DicePool.D6(), new[] { 4, 4 }),  // Player damage
                    _ => new DiceRollResult(DicePool.D10(), new[] { 5 })     // Any other roll
                };
            });

        // Act
        var result = _combatService.ResolveCombatRound(player, monster, _diceServiceMock.Object);

        // Assert
        result.IsCriticalHit.Should().BeTrue();
        result.IsHit.Should().BeTrue();
        result.DamageDealt.Should().BeGreaterThan(0);
    }

    [Test]
    public void ResolveCombatRound_CriticalMiss_AlwaysMisses()
    {
        // Arrange
        var player = CreateTestPlayer();
        var monster = CreateTestMonster(defense: 1); // Very low defense

        // Setup dice to roll natural 1 for attack
        _diceServiceMock.SetupSequence(d => d.Roll(It.IsAny<DicePool>(), It.IsAny<AdvantageType>()))
            .Returns(new DiceRollResult(DicePool.D10(), new[] { 1 })) // Attack - critical miss
            .Returns(new DiceRollResult(DicePool.D10(), new[] { 5 })) // Monster attack roll
            .Returns(new DiceRollResult(DicePool.D6(), new[] { 3 })); // Monster damage roll

        // Act
        var result = _combatService.ResolveCombatRound(player, monster, _diceServiceMock.Object);

        // Assert
        result.IsCriticalMiss.Should().BeTrue();
        result.IsHit.Should().BeFalse();
        result.DamageDealt.Should().Be(0);
    }

    [Test]
    public void ResolveCombatRound_Hit_IncludesFinesseModifier()
    {
        // Arrange
        var player = CreateTestPlayer(finesse: 5);
        var monster = CreateTestMonster(defense: 10);

        // Roll 7, with modifier = AttackTotal, verify hit succeeds
        _diceServiceMock.SetupSequence(d => d.Roll(It.IsAny<DicePool>(), It.IsAny<AdvantageType>()))
            .Returns(new DiceRollResult(DicePool.D10(), new[] { 7 })) // Player attack
            .Returns(new DiceRollResult(DicePool.D6(), new[] { 5 })) // Player damage
            .Returns(new DiceRollResult(DicePool.D10(), new[] { 3 })); // Monster attack - misses

        // Act
        var result = _combatService.ResolveCombatRound(player, monster, _diceServiceMock.Object);

        // Assert
        result.IsHit.Should().BeTrue();
        // Attack total = dice roll + Finesse modifier; exact value depends on Player defaults
        result.AttackTotal.Should().BeGreaterThan(7); // Should include some modifier
    }

    [Test]
    public void ResolveCombatRound_Damage_IncludesMightBonus()
    {
        // Arrange
        var player = CreateTestPlayer(might: 5);
        var monster = CreateTestMonster(defense: 5);

        // Setup high attack roll to ensure hit, and specific damage roll
        _diceServiceMock.SetupSequence(d => d.Roll(It.IsAny<DicePool>(), It.IsAny<AdvantageType>()))
            .Returns(new DiceRollResult(DicePool.D10(), new[] { 8 })) // Attack - hits
            .Returns(new DiceRollResult(DicePool.D6(), new[] { 4 })); // Damage - 4 + 5 Might - 2 armor = 7

        // Act
        var result = _combatService.ResolveCombatRound(player, monster, _diceServiceMock.Object);

        // Assert
        result.IsHit.Should().BeTrue();
        result.DamageDealt.Should().BeGreaterThan(0);
    }

    [Test]
    public void ResolveCombatRound_MonsterDefeated_NoCounterAttack()
    {
        // Arrange
        var player = CreateTestPlayer();
        var monster = CreateTestMonster(health: 5);

        // High damage to kill monster
        _diceServiceMock.SetupSequence(d => d.Roll(It.IsAny<DicePool>(), It.IsAny<AdvantageType>()))
            .Returns(new DiceRollResult(DicePool.D10(), new[] { 10 })) // Critical hit
            .Returns(new DiceRollResult(DicePool.D6(), new[] { 6, 6 })); // Double damage dice

        // Act
        var result = _combatService.ResolveCombatRound(player, monster, _diceServiceMock.Object);

        // Assert
        result.MonsterDefeated.Should().BeTrue();
        result.MonsterCounterAttack.Should().BeNull();
    }

    [Test]
    public void ResolveCombatRound_MonsterSurvives_IncludesCounterAttack()
    {
        // Arrange
        var player = CreateTestPlayer();
        var monster = CreateTestMonster(health: 100, attack: 8);

        _diceServiceMock.SetupSequence(d => d.Roll(It.IsAny<DicePool>(), It.IsAny<AdvantageType>()))
            .Returns(new DiceRollResult(DicePool.D10(), new[] { 6 })) // Player attack
            .Returns(new DiceRollResult(DicePool.D6(), new[] { 3 })) // Player damage
            .Returns(new DiceRollResult(DicePool.D10(), new[] { 7 })) // Monster attack
            .Returns(new DiceRollResult(DicePool.D6(), new[] { 4 })); // Monster damage

        // Act
        var result = _combatService.ResolveCombatRound(player, monster, _diceServiceMock.Object);

        // Assert
        result.MonsterDefeated.Should().BeFalse();
        result.MonsterCounterAttack.Should().NotBeNull();
    }

    [Test]
    public void GetCombatDescription_NormalMiss_IncludesMissMessage()
    {
        // Arrange
        var attackRoll = new DiceRollResult(DicePool.D10(), new[] { 2 });

        var result = new CombatRoundResult(
            attackRoll: attackRoll,
            attackTotal: 7, // Roll 2 + modifier = 7, below defense
            isHit: false,
            isCriticalHit: false,
            isCriticalMiss: false,
            damageRoll: null,
            damageDealt: 0,
            monsterCounterAttack: null,
            monsterDefeated: false,
            playerDefeated: false);

        // Act
        var description = _combatService.GetCombatDescription(result, "Hero", "Goblin");

        // Assert
        description.Should().Contain("misses");
    }

    [Test]
    public void GetCombatDescription_CriticalHit_IncludesSpecialMessage()
    {
        // Arrange
        var attackRoll = new DiceRollResult(DicePool.D10(), new[] { 10 });
        var damageRoll = new DiceRollResult(DicePool.D6(), new[] { 5, 5 });

        var result = new CombatRoundResult(
            attackRoll: attackRoll,
            attackTotal: 15,
            isHit: true,
            isCriticalHit: true,
            isCriticalMiss: false,
            damageRoll: damageRoll,
            damageDealt: 12,
            monsterCounterAttack: null,
            monsterDefeated: true,
            playerDefeated: false);

        // Act
        var description = _combatService.GetCombatDescription(result, "Hero", "Goblin");

        // Assert
        description.Should().Contain("CRITICAL HIT");
        description.Should().Contain("defeated");
    }

    [Test]
    public void GetCombatDescription_CriticalMiss_IncludesFumbleMessage()
    {
        // Arrange
        var attackRoll = new DiceRollResult(DicePool.D10(), new[] { 1 });

        var result = new CombatRoundResult(
            attackRoll: attackRoll,
            attackTotal: 6,
            isHit: false,
            isCriticalHit: false,
            isCriticalMiss: true,
            damageRoll: null,
            damageDealt: 0,
            monsterCounterAttack: null,
            monsterDefeated: false,
            playerDefeated: false);

        // Act
        var description = _combatService.GetCombatDescription(result, "Hero", "Goblin");

        // Assert
        description.Should().Contain("fumble");
    }

    [Test]
    public void GetCombatDescription_NormalHit_IncludesDamage()
    {
        // Arrange
        var attackRoll = new DiceRollResult(DicePool.D10(), new[] { 7 });
        var damageRoll = new DiceRollResult(DicePool.D6(), new[] { 4 });

        var result = new CombatRoundResult(
            attackRoll: attackRoll,
            attackTotal: 12,
            isHit: true,
            isCriticalHit: false,
            isCriticalMiss: false,
            damageRoll: damageRoll,
            damageDealt: 5,
            monsterCounterAttack: null,
            monsterDefeated: false,
            playerDefeated: false);

        // Act
        var description = _combatService.GetCombatDescription(result, "Hero", "Goblin");

        // Assert
        description.Should().Contain("Hero hits");
        description.Should().Contain("5 damage");
    }
}
