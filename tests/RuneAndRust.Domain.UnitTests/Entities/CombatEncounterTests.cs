using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Tests for CombatEncounter aggregate root.
/// </summary>
[TestFixture]
public class CombatEncounterTests
{
    private static InitiativeRoll CreateInitiativeRoll(int rollValue, int modifier)
    {
        var pool = DicePool.D10();
        var diceResult = new DiceRollResult(pool, [rollValue], rollValue);
        return new InitiativeRoll(diceResult, modifier);
    }

    private static Combatant CreatePlayerCombatant(int initiative = 10)
    {
        var player = new Player("TestHero");
        return Combatant.ForPlayer(player, CreateInitiativeRoll(initiative, 0));
    }

    private static Combatant CreateMonsterCombatant(int initiative = 5, int displayNumber = 0)
    {
        var monster = Monster.CreateGoblin();
        return Combatant.ForMonster(monster, CreateInitiativeRoll(initiative, 0), displayNumber);
    }

    [Test]
    public void Create_ShouldInitializeEncounterInNotStartedState()
    {
        // Arrange & Act
        var roomId = Guid.NewGuid();
        var encounter = CombatEncounter.Create(roomId);

        // Assert
        encounter.RoomId.Should().Be(roomId);
        encounter.State.Should().Be(CombatState.NotStarted);
        encounter.RoundNumber.Should().Be(0);
        encounter.Combatants.Should().BeEmpty();
    }

    [Test]
    public void Create_WithPreviousRoomId_ShouldStorePreviousRoom()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var prevRoomId = Guid.NewGuid();

        // Act
        var encounter = CombatEncounter.Create(roomId, prevRoomId);

        // Assert
        encounter.PreviousRoomId.Should().Be(prevRoomId);
    }

    [Test]
    public void AddCombatant_ShouldAddToCollection()
    {
        // Arrange
        var encounter = CombatEncounter.Create(Guid.NewGuid());
        var combatant = CreatePlayerCombatant();

        // Act
        encounter.AddCombatant(combatant);

        // Assert
        encounter.Combatants.Should().Contain(combatant);
        encounter.CombatantCount.Should().Be(1);
    }

    [Test]
    public void AddCombatant_AfterStart_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var encounter = CombatEncounter.Create(Guid.NewGuid());
        encounter.AddCombatant(CreatePlayerCombatant());
        encounter.AddCombatant(CreateMonsterCombatant());
        encounter.Start();

        // Act
        var act = () => encounter.AddCombatant(CreateMonsterCombatant());

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Start_ShouldSortCombatantsByInitiativeDescending()
    {
        // Arrange
        var encounter = CombatEncounter.Create(Guid.NewGuid());
        encounter.AddCombatant(CreatePlayerCombatant(initiative: 5));
        encounter.AddCombatant(CreateMonsterCombatant(initiative: 10));

        // Act
        encounter.Start();

        // Assert
        encounter.State.Should().Be(CombatState.Active);
        encounter.RoundNumber.Should().Be(1);
        encounter.Combatants[0].Initiative.Should().Be(10); // Monster first (higher)
        encounter.Combatants[1].Initiative.Should().Be(5);  // Player second
    }

    [Test]
    public void Start_WithNoCombatants_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var encounter = CombatEncounter.Create(Guid.NewGuid());

        // Act
        var act = () => encounter.Start();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void CurrentCombatant_WhenActive_ShouldReturnFirstInOrder()
    {
        // Arrange
        var encounter = CombatEncounter.Create(Guid.NewGuid());
        var player = CreatePlayerCombatant(initiative: 15);
        var monster = CreateMonsterCombatant(initiative: 5);
        encounter.AddCombatant(player);
        encounter.AddCombatant(monster);
        encounter.Start();

        // Assert
        encounter.CurrentCombatant.Should().Be(player); // Player has higher initiative
        encounter.IsPlayerTurn.Should().BeTrue();
    }

    [Test]
    public void AdvanceTurn_ShouldMoveToNextCombatant()
    {
        // Arrange
        var encounter = CombatEncounter.Create(Guid.NewGuid());
        encounter.AddCombatant(CreatePlayerCombatant(initiative: 15));
        encounter.AddCombatant(CreateMonsterCombatant(initiative: 5));
        encounter.Start();

        // Act
        var next = encounter.AdvanceTurn();

        // Assert
        next.Should().NotBeNull();
        next!.IsMonster.Should().BeTrue();
        encounter.IsPlayerTurn.Should().BeFalse();
    }

    [Test]
    public void AdvanceTurn_AtEndOfRound_ShouldStartNewRound()
    {
        // Arrange
        var encounter = CombatEncounter.Create(Guid.NewGuid());
        encounter.AddCombatant(CreatePlayerCombatant(initiative: 15));
        encounter.AddCombatant(CreateMonsterCombatant(initiative: 5));
        encounter.Start();
        encounter.RoundNumber.Should().Be(1);

        // Act
        encounter.AdvanceTurn(); // End player turn
        encounter.AdvanceTurn(); // End monster turn, wrap to new round

        // Assert  
        encounter.RoundNumber.Should().Be(2);
    }

    [Test]
    public void CheckForResolution_WhenAllMonstersDefeated_ShouldSetVictory()
    {
        // Arrange
        var encounter = CombatEncounter.Create(Guid.NewGuid());
        var player = CreatePlayerCombatant();
        var monster = Monster.CreateGoblin();
        monster.TakeDamage(1000); // Kill the monster
        encounter.AddCombatant(player);
        encounter.AddCombatant(Combatant.ForMonster(monster, CreateInitiativeRoll(5, 0), 0));
        encounter.Start();

        // Act
        encounter.CheckForResolution();

        // Assert
        encounter.State.Should().Be(CombatState.Victory);
        encounter.IsEnded.Should().BeTrue();
    }

    [Test]
    public void EndByFlee_ShouldSetFledState()
    {
        // Arrange
        var encounter = CombatEncounter.Create(Guid.NewGuid());
        encounter.AddCombatant(CreatePlayerCombatant());
        encounter.AddCombatant(CreateMonsterCombatant());
        encounter.Start();

        // Act
        encounter.EndByFlee();

        // Assert
        encounter.State.Should().Be(CombatState.Fled);
        encounter.IsEnded.Should().BeTrue();
    }

    [Test]
    public void GetActiveMonsters_ShouldReturnOnlyAliveMonsters()
    {
        // Arrange
        var encounter = CombatEncounter.Create(Guid.NewGuid());
        encounter.AddCombatant(CreatePlayerCombatant());
        encounter.AddCombatant(CreateMonsterCombatant(initiative: 5, displayNumber: 1));

        var deadMonster = Monster.CreateGoblin();
        deadMonster.TakeDamage(1000);
        encounter.AddCombatant(Combatant.ForMonster(deadMonster, CreateInitiativeRoll(3, 0), 2));
        encounter.Start();

        // Act
        var activeMonsters = encounter.GetActiveMonsters().ToList();

        // Assert
        activeMonsters.Should().HaveCount(1);
    }

    [Test]
    public void GetMonsterByNumber_ShouldReturnCorrectMonster()
    {
        // Arrange
        var encounter = CombatEncounter.Create(Guid.NewGuid());
        encounter.AddCombatant(CreatePlayerCombatant());
        encounter.AddCombatant(CreateMonsterCombatant(initiative: 5, displayNumber: 1));
        encounter.AddCombatant(CreateMonsterCombatant(initiative: 4, displayNumber: 2));
        encounter.Start();

        // Act
        var first = encounter.GetMonsterByNumber(1);
        var second = encounter.GetMonsterByNumber(2);
        var invalid = encounter.GetMonsterByNumber(3);

        // Assert
        first.Should().NotBeNull();
        second.Should().NotBeNull();
        invalid.Should().BeNull();
    }

    [Test]
    public void ActiveMonsterCount_ShouldReturnCountOfAliveMonsters()
    {
        // Arrange
        var encounter = CombatEncounter.Create(Guid.NewGuid());
        encounter.AddCombatant(CreatePlayerCombatant());
        encounter.AddCombatant(CreateMonsterCombatant());
        encounter.AddCombatant(CreateMonsterCombatant());

        // Act & Assert
        encounter.ActiveMonsterCount.Should().Be(2);
    }
}
