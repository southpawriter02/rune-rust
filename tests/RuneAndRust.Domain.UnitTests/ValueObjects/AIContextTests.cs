using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class AIContextTests
{
    [Test]
    public void HealthPercentage_ShouldCalculateCorrectly()
    {
        // Arrange
        var monster = Monster.CreateGoblin(); // 30 HP
        var combatant = CreateCombatant(monster);
        var context = CreateContext(combatant);

        // Act & Assert
        context.HealthPercentage.Should().Be(1.0f);
    }

    [Test]
    public void IsLowHealth_WhenAbove50Percent_ShouldBeFalse()
    {
        // Arrange
        var monster = Monster.CreateGoblin();
        var combatant = CreateCombatant(monster);
        var context = CreateContext(combatant);

        // Assert
        context.IsLowHealth.Should().BeFalse();
    }

    [Test]
    public void IsCriticalHealth_WhenAbove30Percent_ShouldBeFalse()
    {
        // Arrange
        var monster = Monster.CreateGoblin();
        var combatant = CreateCombatant(monster);
        var context = CreateContext(combatant);

        // Assert
        context.IsCriticalHealth.Should().BeFalse();
    }

    [Test]
    public void HasAllies_WhenNoAllies_ShouldBeFalse()
    {
        // Arrange
        var monster = Monster.CreateGoblin();
        var combatant = CreateCombatant(monster);
        var context = CreateContext(combatant);

        // Assert
        context.HasAllies.Should().BeFalse();
    }

    [Test]
    public void WeakestEnemy_ShouldReturnLowestHealthEnemy()
    {
        // Arrange
        var monster = Monster.CreateGoblin();
        var combatant = CreateCombatant(monster);
        var player = new Player("TestPlayer");
        var playerCombatant = CreatePlayerCombatant(player);
        var enemies = new List<Combatant> { playerCombatant };
        var encounter = CombatEncounter.Create(Guid.NewGuid());
        
        var context = new AIContext(combatant, encounter, [], enemies, 1);

        // Act
        var weakest = context.WeakestEnemy;

        // Assert
        weakest.Should().Be(playerCombatant);
    }

    private static Combatant CreateCombatant(Monster monster)
    {
        var pool = DicePool.D10();
        var initiative = new InitiativeRoll(
            new DiceRollResult(pool, [5], 5),
            monster.InitiativeModifier);
        return Combatant.ForMonster(monster, initiative, 0);
    }

    private static Combatant CreatePlayerCombatant(Player player)
    {
        var pool = DicePool.D10();
        var initiative = new InitiativeRoll(
            new DiceRollResult(pool, [5], 5),
            player.Attributes.Finesse);
        return Combatant.ForPlayer(player, initiative);
    }

    private static AIContext CreateContext(Combatant self)
    {
        var encounter = CombatEncounter.Create(Guid.NewGuid());
        return new AIContext(self, encounter, [], [], 1);
    }
}
