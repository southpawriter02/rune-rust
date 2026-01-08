using FluentAssertions;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class MonsterAIServiceTests
{
    private MonsterAIService _service = null!;
    private ILogger<MonsterAIService> _logger = null!;
    private Random _fixedRandom = null!;

    [SetUp]
    public void SetUp()
    {
        _logger = new TestLogger<MonsterAIService>();
        _fixedRandom = new Random(42); // Fixed seed for deterministic tests
        _service = new MonsterAIService(_logger, _fixedRandom);
    }

    [Test]
    public void DecideAction_AggressiveBehavior_ShouldAttackWeakestEnemy()
    {
        // Arrange
        var monster = Monster.CreateOrc(); // Aggressive behavior
        var combatant = CreateMonsterCombatant(monster);
        var player = new Player("TestPlayer");
        var playerCombatant = CreatePlayerCombatant(player);
        var encounter = CombatEncounter.Create(Guid.NewGuid());
        
        var context = new AIContext(combatant, encounter, [], [playerCombatant], 1);

        // Act
        var decision = _service.DecideAction(context);

        // Assert
        decision.Action.Should().Be(AIAction.Attack);
        decision.Target.Should().Be(playerCombatant);
    }

    [Test]
    public void DecideAction_DefensiveBehavior_HighHealth_ShouldAttack()
    {
        // Arrange
        var monster = new Monster("Defender", "A defensive monster", 50, new Stats(50, 10, 5));
        monster.SetBehavior(AIBehavior.Defensive);
        var combatant = CreateMonsterCombatant(monster);
        var player = new Player("TestPlayer");
        var playerCombatant = CreatePlayerCombatant(player);
        var encounter = CombatEncounter.Create(Guid.NewGuid());

        var context = new AIContext(combatant, encounter, [], [playerCombatant], 1);

        // Act
        var decision = _service.DecideAction(context);

        // Assert
        decision.Action.Should().Be(AIAction.Attack);
    }

    [Test]
    public void DecideAction_SupportBehavior_WithHealingAndWoundedAlly_ShouldHealAlly()
    {
        // Arrange
        var shaman = Monster.CreateGoblinShaman(); // Support behavior with healing
        var shamanCombatant = CreateMonsterCombatant(shaman);
        
        var woundedGoblin = Monster.CreateGoblin();
        woundedGoblin.TakeDamage(20); // Wound it below 50%
        var woundedCombatant = CreateMonsterCombatant(woundedGoblin);
        
        var encounter = CombatEncounter.Create(Guid.NewGuid());
        var context = new AIContext(
            shamanCombatant,
            encounter,
            [woundedCombatant], // Wounded ally
            [],
            1);

        // Act
        var decision = _service.DecideAction(context);

        // Assert
        decision.Action.Should().Be(AIAction.Heal);
        decision.Target.Should().Be(woundedCombatant);
    }

    [Test]
    public void DecideAction_NoEnemies_ShouldWait()
    {
        // Arrange
        var monster = Monster.CreateGoblin();
        var combatant = CreateMonsterCombatant(monster);
        var encounter = CombatEncounter.Create(Guid.NewGuid());

        var context = new AIContext(combatant, encounter, [], [], 1);

        // Act
        var decision = _service.DecideAction(context);

        // Assert
        decision.Action.Should().Be(AIAction.Wait);
    }

    [Test]
    public void BuildContext_ShouldReturnValidContext()
    {
        // Arrange
        var monster = Monster.CreateGoblin();
        var combatant = CreateMonsterCombatant(monster);
        var encounter = CombatEncounter.Create(Guid.NewGuid());
        encounter.AddCombatant(combatant);

        // Act
        var context = _service.BuildContext(combatant, encounter);

        // Assert
        context.Self.Should().Be(combatant);
        context.Encounter.Should().Be(encounter);
        context.RoundNumber.Should().Be(0);
    }

    private static Combatant CreateMonsterCombatant(Monster monster)
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

    private class TestLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}
