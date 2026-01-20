using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for InitiativeService.
/// </summary>
[TestFixture]
public class InitiativeServiceTests
{
    private InitiativeService _service = null!;
    private DiceService _diceService = null!;

    [SetUp]
    public void SetUp()
    {
        // Use a seeded random for deterministic testing
        var random = new Random(42);
        _diceService = new DiceService(NullLogger<DiceService>.Instance, random);
        _service = new InitiativeService(_diceService, NullLogger<InitiativeService>.Instance);
    }

    [Test]
    public void RollForPlayer_ShouldReturnInitiativeWithPlayerFinesse()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var initiative = _service.RollForPlayer(player);

        // Assert
        initiative.Modifier.Should().Be(player.Attributes.Finesse);
        initiative.RollValue.Should().BeInRange(1, 10);
        initiative.Total.Should().Be(initiative.RollValue + initiative.Modifier);
    }

    [Test]
    public void RollForMonster_ShouldReturnInitiativeWithMonsterModifier()
    {
        // Arrange
        var monster = Monster.CreateGoblin();

        // Act
        var initiative = _service.RollForMonster(monster);

        // Assert
        initiative.Modifier.Should().Be(monster.InitiativeModifier);
        initiative.RollValue.Should().BeInRange(1, 10);
    }

    [Test]
    public void CreateEncounter_ShouldCreateEncounterWithAllCombatants()
    {
        // Arrange
        var player = new Player("TestHero");
        var monsters = new[] { Monster.CreateGoblin(), Monster.CreateSkeleton() };
        var roomId = Guid.NewGuid();

        // Act
        var encounter = _service.CreateEncounter(player, monsters, roomId, null);

        // Assert
        encounter.RoomId.Should().Be(roomId);
        encounter.CombatantCount.Should().Be(3); // 1 player + 2 monsters
    }

    [Test]
    public void CreateEncounter_WithDuplicateMonsters_ShouldNumberThem()
    {
        // Arrange
        var player = new Player("TestHero");
        var monsters = new[] { Monster.CreateGoblin(), Monster.CreateGoblin(), Monster.CreateGoblin() };
        var roomId = Guid.NewGuid();

        // Act
        var encounter = _service.CreateEncounter(player, monsters, roomId, null);

        // Assert
        var monsterNames = encounter.Combatants
            .Where(c => c.IsMonster)
            .Select(c => c.DisplayName)
            .ToList();

        monsterNames.Should().Contain("Goblin 1");
        monsterNames.Should().Contain("Goblin 2");
        monsterNames.Should().Contain("Goblin 3");
    }

    [Test]
    public void CreateEncounter_WithUniqueMonster_ShouldNotNumber()
    {
        // Arrange
        var player = new Player("TestHero");
        var monsters = new[] { Monster.CreateGoblin(), Monster.CreateSkeleton(), Monster.CreateOrc() };
        var roomId = Guid.NewGuid();

        // Act
        var encounter = _service.CreateEncounter(player, monsters, roomId, null);

        // Assert
        var monsterNames = encounter.Combatants
            .Where(c => c.IsMonster)
            .Select(c => c.DisplayName)
            .ToList();

        monsterNames.Should().Contain("Goblin");  // No number
        monsterNames.Should().Contain("Skeleton");
        monsterNames.Should().Contain("Orc");
    }

    [Test]
    public void CreateEncounter_WithMixedDuplicates_ShouldNumberCorrectly()
    {
        // Arrange
        var player = new Player("TestHero");
        var monsters = new[] 
        { 
            Monster.CreateGoblin(), 
            Monster.CreateGoblin(), 
            Monster.CreateSkeleton() 
        };
        var roomId = Guid.NewGuid();

        // Act
        var encounter = _service.CreateEncounter(player, monsters, roomId, null);

        // Assert
        var monsterNames = encounter.Combatants
            .Where(c => c.IsMonster)
            .Select(c => c.DisplayName)
            .ToList();

        // Goblins should be numbered
        monsterNames.Should().Contain("Goblin 1");
        monsterNames.Should().Contain("Goblin 2");
        // Skeleton should NOT be numbered (unique)
        monsterNames.Should().Contain("Skeleton");
    }

    [Test]
    public void RollForPlayer_WithNullPlayer_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => _service.RollForPlayer(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void RollForMonster_WithNullMonster_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => _service.RollForMonster(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
