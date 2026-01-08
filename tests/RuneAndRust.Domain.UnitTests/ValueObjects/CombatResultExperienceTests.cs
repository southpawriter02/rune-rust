using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for CombatRoundResult experience-related functionality (v0.0.8a).
/// </summary>
[TestFixture]
public class CombatResultExperienceTests
{
    [Test]
    public void CombatRoundResult_DefaultExperienceGained_IsZero()
    {
        // Arrange & Act
        var result = new CombatRoundResult(
            attackRoll: new DiceRollResult(DicePool.D10(), new[] { 5 }, 5),
            attackTotal: 10,
            isHit: true,
            isCriticalHit: false,
            isCriticalMiss: false,
            damageRoll: new DiceRollResult(DicePool.D6(), new[] { 3 }, 3),
            damageDealt: 5,
            monsterCounterAttack: null,
            monsterDefeated: false,
            playerDefeated: false);

        // Assert
        result.ExperienceGained.Should().Be(0);
    }

    [Test]
    public void CombatRoundResult_MonsterDefeated_IncludesXp()
    {
        // Arrange & Act
        var result = new CombatRoundResult(
            attackRoll: new DiceRollResult(DicePool.D10(), new[] { 8 }, 8),
            attackTotal: 15,
            isHit: true,
            isCriticalHit: false,
            isCriticalMiss: false,
            damageRoll: new DiceRollResult(DicePool.D6(), new[] { 6, 6 }, 12),
            damageDealt: 12,
            monsterCounterAttack: null,
            monsterDefeated: true,
            playerDefeated: false,
            experienceGained: 25);

        // Assert
        result.ExperienceGained.Should().Be(25);
        result.MonsterDefeated.Should().BeTrue();
    }

    [Test]
    public void CombatRoundResult_MonsterAlive_ZeroXp()
    {
        // Arrange & Act
        var result = new CombatRoundResult(
            attackRoll: new DiceRollResult(DicePool.D10(), new[] { 7 }, 7),
            attackTotal: 12,
            isHit: true,
            isCriticalHit: false,
            isCriticalMiss: false,
            damageRoll: new DiceRollResult(DicePool.D6(), new[] { 4 }, 4),
            damageDealt: 4,
            monsterCounterAttack: null,
            monsterDefeated: false,
            playerDefeated: false,
            experienceGained: 0);

        // Assert
        result.ExperienceGained.Should().Be(0);
        result.MonsterDefeated.Should().BeFalse();
    }

    [Test]
    public void CombatRoundResult_PlayerDefeated_HasNoXp()
    {
        // Arrange & Act
        var result = new CombatRoundResult(
            attackRoll: new DiceRollResult(DicePool.D10(), new[] { 1 }, 1),
            attackTotal: 6,
            isHit: false,
            isCriticalHit: false,
            isCriticalMiss: true,
            damageRoll: null,
            damageDealt: 0,
            monsterCounterAttack: null,
            monsterDefeated: false,
            playerDefeated: true,
            experienceGained: 0);

        // Assert
        result.ExperienceGained.Should().Be(0);
        result.PlayerDefeated.Should().BeTrue();
        result.AttackSuccessLevel.Should().Be(SuccessLevel.CriticalFailure);
    }

    [Test]
    public void CombatRoundResult_WithCounterAttack_PreservesXp()
    {
        // Arrange
        var counterAttack = new MonsterCounterAttackResult(
            attackRoll: new DiceRollResult(DicePool.D10(), new[] { 6 }, 6),
            attackTotal: 8,
            isHit: true,
            isCriticalHit: false,
            isCriticalMiss: false,
            damageRoll: new DiceRollResult(DicePool.D4(), new[] { 3 }, 3),
            damageDealt: 3,
            playerDefeated: false);

        // Act
        var result = new CombatRoundResult(
            attackRoll: new DiceRollResult(DicePool.D10(), new[] { 10 }, 10),
            attackTotal: 17,
            isHit: true,
            isCriticalHit: true,
            isCriticalMiss: false,
            damageRoll: new DiceRollResult(DicePool.D6(), new[] { 6, 5 }, 22),
            damageDealt: 22,
            monsterCounterAttack: counterAttack,
            monsterDefeated: true,
            playerDefeated: false,
            experienceGained: 40);

        // Assert
        result.ExperienceGained.Should().Be(40);
        result.MonsterDefeated.Should().BeTrue();
        result.MonsterCounterAttack.Should().NotBeNull();
        result.AttackSuccessLevel.Should().Be(SuccessLevel.CriticalSuccess);
    }
}
