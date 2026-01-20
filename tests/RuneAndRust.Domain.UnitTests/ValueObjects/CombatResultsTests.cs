using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class CombatResultsTests
{
    [Test]
    public void CombatRoundResult_AttackSuccessLevel_ReturnsCriticalSuccess_OnCriticalHit()
    {
        // Arrange
        var attackRoll = new DiceRollResult(DicePool.D10(), new[] { 10 }, 10);
        var damageRoll = new DiceRollResult(DicePool.D6(), new[] { 5, 5 }, 10);

        // Act
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

        // Assert
        result.AttackSuccessLevel.Should().Be(SuccessLevel.CriticalSuccess);
    }

    [Test]
    public void CombatRoundResult_AttackSuccessLevel_ReturnsCriticalFailure_OnCriticalMiss()
    {
        // Arrange
        var attackRoll = new DiceRollResult(DicePool.D10(), new[] { 1 }, 1);

        // Act
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

        // Assert
        result.AttackSuccessLevel.Should().Be(SuccessLevel.CriticalFailure);
    }

    [Test]
    public void CombatRoundResult_DamageReceived_ReturnsCounterAttackDamage()
    {
        // Arrange
        var attackRoll = new DiceRollResult(DicePool.D10(), new[] { 7 }, 7);
        var damageRoll = new DiceRollResult(DicePool.D6(), new[] { 4 }, 4);
        var counterAttackRoll = new DiceRollResult(DicePool.D10(), new[] { 8 }, 8);
        var counterDamageRoll = new DiceRollResult(DicePool.D6(), new[] { 5 }, 5);

        var counterAttack = new MonsterCounterAttackResult(
            counterAttackRoll, 12, true, false, false, counterDamageRoll, 6, false);

        // Act
        var result = new CombatRoundResult(
            attackRoll: attackRoll,
            attackTotal: 12,
            isHit: true,
            isCriticalHit: false,
            isCriticalMiss: false,
            damageRoll: damageRoll,
            damageDealt: 5,
            monsterCounterAttack: counterAttack,
            monsterDefeated: false,
            playerDefeated: false);

        // Assert
        result.DamageReceived.Should().Be(6);
    }

    [Test]
    public void CombatRoundResult_CombatEnded_True_WhenMonsterDefeated()
    {
        // Arrange
        var attackRoll = new DiceRollResult(DicePool.D10(), new[] { 8 }, 8);
        var damageRoll = new DiceRollResult(DicePool.D6(), new[] { 6 }, 6);

        // Act
        var result = new CombatRoundResult(
            attackRoll: attackRoll,
            attackTotal: 13,
            isHit: true,
            isCriticalHit: false,
            isCriticalMiss: false,
            damageRoll: damageRoll,
            damageDealt: 7,
            monsterCounterAttack: null,
            monsterDefeated: true,
            playerDefeated: false);

        // Assert
        result.CombatEnded.Should().BeTrue();
    }

    [Test]
    public void MonsterCounterAttackResult_AttackSuccessLevel_ReturnsSuccess_OnHit()
    {
        // Arrange
        var attackRoll = new DiceRollResult(DicePool.D10(), new[] { 7 }, 7);
        var damageRoll = new DiceRollResult(DicePool.D6(), new[] { 4 }, 4);

        // Act
        var result = new MonsterCounterAttackResult(
            attackRoll, 11, true, false, false, damageRoll, 5, false);

        // Assert
        result.AttackSuccessLevel.Should().Be(SuccessLevel.Success);
    }

    [Test]
    public void CombatRoundResult_ToString_IncludesAttackResult()
    {
        // Arrange
        var attackRoll = new DiceRollResult(DicePool.D10(), new[] { 10 }, 10);
        var damageRoll = new DiceRollResult(DicePool.D6(), new[] { 6 }, 6);

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
        var str = result.ToString();

        // Assert
        str.Should().Contain("CRITICAL HIT");
        str.Should().Contain("12 damage");
    }
}
