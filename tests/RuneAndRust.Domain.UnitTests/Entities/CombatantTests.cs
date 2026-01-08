using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for Combatant entity.
/// </summary>
[TestFixture]
public class CombatantTests
{
    private static InitiativeRoll CreateInitiativeRoll(int rollValue, int modifier)
    {
        var pool = DicePool.D10();
        var diceResult = new DiceRollResult(pool, [rollValue], rollValue);
        return new InitiativeRoll(diceResult, modifier);
    }

    [Test]
    public void ForPlayer_ShouldCreatePlayerCombatant()
    {
        // Arrange
        var player = new Player("TestHero");
        var initiative = CreateInitiativeRoll(7, 5);

        // Act
        var combatant = Combatant.ForPlayer(player, initiative);

        // Assert
        combatant.IsPlayer.Should().BeTrue();
        combatant.IsMonster.Should().BeFalse();
        combatant.Player.Should().Be(player);
        combatant.Monster.Should().BeNull();
        combatant.DisplayName.Should().Be("TestHero");
        combatant.DisplayNumber.Should().BeNull();
        combatant.Initiative.Should().Be(12); // 7 + 5
        combatant.Finesse.Should().Be(player.Attributes.Finesse);
    }

    [Test]
    public void ForMonster_WithNumber_ShouldCreateNumberedCombatant()
    {
        // Arrange
        var monster = Monster.CreateGoblin();
        var initiative = CreateInitiativeRoll(4, 1);

        // Act
        var combatant = Combatant.ForMonster(monster, initiative, displayNumber: 2);

        // Assert
        combatant.IsMonster.Should().BeTrue();
        combatant.IsPlayer.Should().BeFalse();
        combatant.Monster.Should().Be(monster);
        combatant.Player.Should().BeNull();
        combatant.DisplayName.Should().Be("Goblin 2");
        combatant.DisplayNumber.Should().Be(2);
    }

    [Test]
    public void ForMonster_WithZeroNumber_ShouldNotAddNumberToName()
    {
        // Arrange
        var monster = Monster.CreateGoblin();
        var initiative = CreateInitiativeRoll(5, 1);

        // Act
        var combatant = Combatant.ForMonster(monster, initiative, displayNumber: 0);

        // Assert
        combatant.DisplayName.Should().Be("Goblin");
        combatant.DisplayNumber.Should().BeNull();
    }

    [Test]
    public void ForPlayer_WithNullPlayer_ShouldThrowArgumentNullException()
    {
        // Arrange
        var initiative = CreateInitiativeRoll(5, 0);

        // Act
        var act = () => Combatant.ForPlayer(null!, initiative);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void IsActive_WhenPlayerAlive_ShouldReturnTrue()
    {
        // Arrange
        var player = new Player("TestHero");
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll(5, 0));

        // Assert
        combatant.IsActive.Should().BeTrue();
    }

    [Test]
    public void IsActive_WhenMonsterDefeated_ShouldReturnFalse()
    {
        // Arrange
        var monster = Monster.CreateGoblin();
        monster.TakeDamage(1000); // Kill the monster
        var combatant = Combatant.ForMonster(monster, CreateInitiativeRoll(5, 0), 0);

        // Assert
        combatant.IsActive.Should().BeFalse();
    }

    [Test]
    public void MarkActed_ShouldSetHasActedThisRound()
    {
        // Arrange
        var player = new Player("TestHero");
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll(5, 0));
        combatant.HasActedThisRound.Should().BeFalse();

        // Act
        combatant.MarkActed();

        // Assert
        combatant.HasActedThisRound.Should().BeTrue();
    }

    [Test]
    public void ResetActed_ShouldClearHasActedThisRound()
    {
        // Arrange
        var player = new Player("TestHero");
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll(5, 0));
        combatant.MarkActed();
        combatant.HasActedThisRound.Should().BeTrue();

        // Act
        combatant.ResetActed();

        // Assert
        combatant.HasActedThisRound.Should().BeFalse();
    }

    [Test]
    public void CurrentHealth_ShouldReturnPlayerHealth()
    {
        // Arrange
        var player = new Player("TestHero", new Stats(100, 10, 5));
        var combatant = Combatant.ForPlayer(player, CreateInitiativeRoll(5, 0));

        // Assert
        combatant.CurrentHealth.Should().Be(100);
        combatant.MaxHealth.Should().Be(100);
    }

    [Test]
    public void CurrentHealth_ShouldReturnMonsterHealth()
    {
        // Arrange
        var monster = Monster.CreateGoblin();
        var combatant = Combatant.ForMonster(monster, CreateInitiativeRoll(5, 0), 0);

        // Assert
        combatant.CurrentHealth.Should().Be(monster.Health);
        combatant.MaxHealth.Should().Be(monster.MaxHealth);
    }
}
