using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class AIDecisionTests
{
    [Test]
    public void Attack_ShouldCreateAttackDecision()
    {
        // Arrange
        var monster = Monster.CreateGoblin();
        var combatant = CreateCombatant(monster);

        // Act
        var decision = AIDecision.Attack(combatant, "Testing attack");

        // Assert
        decision.Action.Should().Be(AIAction.Attack);
        decision.Target.Should().Be(combatant);
        decision.AbilityId.Should().BeNull();
        decision.Reasoning.Should().Be("Testing attack");
    }

    [Test]
    public void Defend_ShouldCreateDefendDecision()
    {
        // Act
        var decision = AIDecision.Defend("Testing defend");

        // Assert
        decision.Action.Should().Be(AIAction.Defend);
        decision.Target.Should().BeNull();
        decision.Reasoning.Should().Be("Testing defend");
    }

    [Test]
    public void Heal_ShouldCreateHealDecision()
    {
        // Arrange
        var monster = Monster.CreateGoblinShaman();
        var combatant = CreateCombatant(monster);

        // Act
        var decision = AIDecision.Heal(combatant, "heal_spell", "Testing heal");

        // Assert
        decision.Action.Should().Be(AIAction.Heal);
        decision.Target.Should().Be(combatant);
        decision.AbilityId.Should().Be("heal_spell");
        decision.Reasoning.Should().Be("Testing heal");
    }

    [Test]
    public void Flee_ShouldCreateFleeDecision()
    {
        // Act
        var decision = AIDecision.Flee("Testing flee");

        // Assert
        decision.Action.Should().Be(AIAction.Flee);
        decision.Target.Should().BeNull();
        decision.Reasoning.Should().Be("Testing flee");
    }

    [Test]
    public void Wait_ShouldCreateWaitDecision()
    {
        // Act
        var decision = AIDecision.Wait("Testing wait");

        // Assert
        decision.Action.Should().Be(AIAction.Wait);
        decision.Target.Should().BeNull();
        decision.Reasoning.Should().Be("Testing wait");
    }

    [Test]
    public void ToString_WithTarget_ShouldIncludeTargetName()
    {
        // Arrange
        var monster = Monster.CreateGoblin();
        var combatant = CreateCombatant(monster);
        var decision = AIDecision.Attack(combatant, "Attacking");

        // Act
        var result = decision.ToString();

        // Assert
        result.Should().Contain("Attack");
        result.Should().Contain(combatant.DisplayName);
        result.Should().Contain("Attacking");
    }

    private static Combatant CreateCombatant(Monster monster)
    {
        var pool = DicePool.D10();
        var initiative = new InitiativeRoll(
            new DiceRollResult(pool, [5]),
            monster.InitiativeModifier);
        return Combatant.ForMonster(monster, initiative, 0);
    }
}
