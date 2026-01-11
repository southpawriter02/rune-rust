using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.TestUtilities.Builders;

namespace RuneAndRust.Application.UnitTests.TestUtilities;

/// <summary>
/// Tests for the fluent entity builders.
/// </summary>
[TestFixture]
public class BuilderTests
{
    [Test]
    public void PlayerBuilder_Build_CreatesValidPlayer()
    {
        // Arrange & Act
        var player = PlayerBuilder.Create().Build();

        // Assert
        player.Should().NotBeNull();
        player.Name.Should().Be("TestPlayer");
        player.IsAlive.Should().BeTrue();
        player.Health.Should().Be(player.Stats.MaxHealth);
    }

    [Test]
    public void PlayerBuilder_WithStats_SetsCorrectValues()
    {
        // Arrange & Act
        var player = PlayerBuilder.Create()
            .WithStats(200, 20, 15)
            .Build();

        // Assert
        player.Stats.MaxHealth.Should().Be(200);
        player.Stats.Attack.Should().Be(20);
        player.Stats.Defense.Should().Be(15);
        player.Health.Should().Be(200);
    }

    [Test]
    public void PlayerBuilder_WithResource_InitializesPool()
    {
        // Arrange & Act
        var player = PlayerBuilder.Create()
            .WithResource("mana", 100, 50)
            .Build();

        // Assert
        player.HasResource("mana").Should().BeTrue();
        var resource = player.GetResource("mana");
        resource.Should().NotBeNull();
        resource!.Maximum.Should().Be(100);
        resource.Current.Should().Be(50);
    }

    [Test]
    public void PlayerBuilder_WithClass_SetsArchetypeAndClass()
    {
        // Arrange & Act
        var player = PlayerBuilder.Create()
            .WithClass("martial", "warrior")
            .Build();

        // Assert
        player.HasClass.Should().BeTrue();
        player.ArchetypeId.Should().Be("martial");
        player.ClassId.Should().Be("warrior");
    }

    [Test]
    public void PlayerBuilder_WithCurrentHealth_DamagesPlayerCorrectly()
    {
        // Arrange & Act
        var player = PlayerBuilder.Create()
            .WithStats(100, 10, 5)
            .WithCurrentHealth(50)
            .Build();

        // Assert
        player.Health.Should().Be(50);
        player.IsAlive.Should().BeTrue();
    }

    [Test]
    public void MonsterBuilder_Goblin_CreatesGoblinPreset()
    {
        // Arrange & Act
        var monster = MonsterBuilder.Goblin().Build();

        // Assert
        monster.Name.Should().Be("Goblin");
        monster.MonsterDefinitionId.Should().Be("goblin");
        monster.Stats.MaxHealth.Should().Be(30);
        monster.Stats.Attack.Should().Be(8);
        monster.Stats.Defense.Should().Be(2);
        monster.ExperienceValue.Should().Be(25);
        monster.Behavior.Should().Be(AIBehavior.Cowardly);
    }

    [Test]
    public void MonsterBuilder_WithCurrentHealth_DamagesMonsterCorrectly()
    {
        // Arrange & Act
        var monster = MonsterBuilder.Create()
            .WithStats(50, 10, 5)
            .WithCurrentHealth(25)
            .Build();

        // Assert
        monster.Health.Should().Be(25);
        monster.IsAlive.Should().BeTrue();
    }

    [Test]
    public void MonsterBuilder_Orc_CreatesOrcPreset()
    {
        // Arrange & Act
        var monster = MonsterBuilder.Orc().Build();

        // Assert
        monster.Name.Should().Be("Orc");
        monster.MonsterDefinitionId.Should().Be("orc");
        monster.Stats.MaxHealth.Should().Be(50);
        monster.ExperienceValue.Should().Be(50);
        monster.Behavior.Should().Be(AIBehavior.Aggressive);
    }

    [Test]
    public void RoomBuilder_Build_CreatesValidRoom()
    {
        // Arrange & Act
        var room = RoomBuilder.Create()
            .WithName("Dungeon Entrance")
            .WithDescription("A dark entrance to the dungeon.")
            .AtPosition(0, 0)
            .Build();

        // Assert
        room.Should().NotBeNull();
        room.Name.Should().Be("Dungeon Entrance");
        room.Description.Should().Be("A dark entrance to the dungeon.");
        room.Position.Should().Be(new Position3D(0, 0, 0));
    }

    [Test]
    public void RoomBuilder_WithMonster_AddsMonsterToRoom()
    {
        // Arrange
        var monster = MonsterBuilder.Goblin().Build();

        // Act
        var room = RoomBuilder.Create()
            .WithMonster(monster)
            .Build();

        // Assert
        room.HasMonsters.Should().BeTrue();
        room.Monsters.Should().HaveCount(1);
        room.Monsters.First().Name.Should().Be("Goblin");
    }

    [Test]
    public void GameSessionBuilder_Build_CreatesValidSession()
    {
        // Arrange & Act
        var session = GameSessionBuilder.Create().Build();

        // Assert
        session.Should().NotBeNull();
        session.Player.Should().NotBeNull();
        session.Dungeon.Should().NotBeNull();
        session.State.Should().Be(GameState.Playing);
    }

    [Test]
    public void GameSessionBuilder_WithState_SetsGameState()
    {
        // Arrange & Act
        var session = GameSessionBuilder.Create()
            .WithState(GameState.GameOver)
            .Build();

        // Assert
        session.State.Should().Be(GameState.GameOver);
    }
}
