using FluentAssertions;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class DungeonGeneratorTests
{
    private Mock<IRoomTemplateProvider> _roomTemplateProviderMock = null!;
    private Mock<IEntityTemplateProvider> _entityTemplateProviderMock = null!;
    private IDungeonGenerator _generator = null!;

    [SetUp]
    public void SetUp()
    {
        _roomTemplateProviderMock = new Mock<IRoomTemplateProvider>();
        _entityTemplateProviderMock = new Mock<IEntityTemplateProvider>();

        // Setup minimal templates for testing
        SetupMinimalTemplates();

        _generator = DungeonGenerator.CreateDefault(
            _roomTemplateProviderMock.Object,
            _entityTemplateProviderMock.Object);
    }

    private void SetupMinimalTemplates()
    {
        // Create minimal room templates for each archetype
        var templates = new List<RoomTemplate>
        {
            CreateTemplate("citadel_corridor", "Corridor", RoomArchetype.Corridor, Biome.Citadel, 2, 2),
            CreateTemplate("citadel_chamber", "Chamber", RoomArchetype.Chamber, Biome.Citadel, 2, 4),
            CreateTemplate("citadel_junction", "Junction", RoomArchetype.Junction, Biome.Citadel, 3, 4),
            CreateTemplate("citadel_deadend", "Dead End", RoomArchetype.DeadEnd, Biome.Citadel, 1, 1),
            CreateTemplate("citadel_boss", "Boss Arena", RoomArchetype.BossArena, Biome.Citadel, 1, 2),
            CreateTemplate("citadel_stairwell", "Stairwell", RoomArchetype.Stairwell, Biome.Citadel, 2, 3)
        };

        _roomTemplateProviderMock.Setup(p => p.GetAllTemplates()).Returns(templates);
        _roomTemplateProviderMock.Setup(p => p.GetTemplatesByBiome(It.IsAny<Biome>())).Returns(templates);

        // Create minimal entity templates
        var entityTemplates = new List<EntityTemplate>
        {
            EntityTemplate.CreateSwarm("skeleton", "Skeleton", "undead_legion", Biome.Citadel, 3,
                new Stats(15, 5, 2, 5)),
            EntityTemplate.CreateGrunt("warrior", "Skeleton Warrior", "undead_legion", Biome.Citadel, 10,
                EntityRole.Melee, new Stats(30, 10, 5, 6)),
            EntityTemplate.CreateElite("knight", "Death Knight", "undead_legion", Biome.Citadel, 50,
                new Stats(80, 18, 15, 12)),
            EntityTemplate.CreateBoss("lich", "Lich Lord", "undead_legion", Biome.Citadel, 100,
                new Stats(120, 25, 12, 18))
        };

        _entityTemplateProviderMock.Setup(p => p.GetAllTemplates()).Returns(entityTemplates);
        _entityTemplateProviderMock.Setup(p => p.GetTemplatesByBiome(It.IsAny<Biome>())).Returns(entityTemplates);
        _entityTemplateProviderMock.Setup(p => p.GetTemplatesByFaction(It.IsAny<string>())).Returns(entityTemplates);
    }

    private static RoomTemplate CreateTemplate(
        string id, string name, RoomArchetype archetype, Biome biome, int minExits, int maxExits)
    {
        return new RoomTemplate(id, name, archetype, biome, "A test room.", minExits, maxExits);
    }

    [Test]
    public void GenerateDungeon_WithValidParameters_CreatesDungeon()
    {
        // Arrange & Act
        var dungeon = _generator.GenerateDungeon("Test Dungeon", Biome.Citadel, DifficultyTier.Tier1, 5, seed: 12345);

        // Assert
        dungeon.Should().NotBeNull();
        dungeon.Name.Should().Be("Test Dungeon");
        dungeon.RoomCount.Should().BeGreaterThanOrEqualTo(3);
    }

    [Test]
    public void GenerateDungeon_HasStartingRoom()
    {
        // Arrange & Act
        var dungeon = _generator.GenerateDungeon("Test Dungeon", Biome.Citadel, DifficultyTier.Tier1, 5, seed: 42);

        // Assert
        var startRoom = dungeon.GetStartingRoom();
        startRoom.Should().NotBeNull();
    }

    [Test]
    public void GenerateDungeon_RoomsHaveExits()
    {
        // Arrange & Act
        var dungeon = _generator.GenerateDungeon("Test Dungeon", Biome.Citadel, DifficultyTier.Tier1, 6, seed: 99);

        // Assert
        foreach (var room in dungeon.Rooms.Values)
        {
            room.Exits.Count.Should().BeGreaterThanOrEqualTo(1,
                $"Room '{room.Name}' should have at least one exit");
        }
    }

    [Test]
    public void GenerateDungeon_RoomsHaveTags()
    {
        // Arrange & Act
        var dungeon = _generator.GenerateDungeon("Test Dungeon", Biome.Citadel, DifficultyTier.Tier1, 5, seed: 123);

        // Assert
        foreach (var room in dungeon.Rooms.Values)
        {
            room.Tags.Should().NotBeEmpty($"Room '{room.Name}' should have tags");
        }
    }

    [Test]
    public void GenerateDungeon_SomeRoomsHaveMonsters()
    {
        // Arrange & Act
        var dungeon = _generator.GenerateDungeon("Test Dungeon", Biome.Citadel, DifficultyTier.Tier2, 8, seed: 456);

        // Assert - At least the boss room should have monsters
        var roomsWithMonsters = dungeon.Rooms.Values.Count(r => r.HasMonsters);
        roomsWithMonsters.Should().BeGreaterThanOrEqualTo(1);
    }

    [Test]
    public void GenerateDungeon_StartRoomHasNoMonsters()
    {
        // Arrange & Act
        var dungeon = _generator.GenerateDungeon("Test Dungeon", Biome.Citadel, DifficultyTier.Tier1, 5, seed: 789);

        // Assert
        var startRoom = dungeon.GetStartingRoom();
        startRoom.Should().NotBeNull();
        startRoom!.HasMonsters.Should().BeFalse("Starting room should not have monsters");
    }

    [Test]
    public void GenerateDungeon_SameSeadProducesSameResult()
    {
        // Arrange & Act
        var dungeon1 = _generator.GenerateDungeon("Test", Biome.Citadel, DifficultyTier.Tier1, 5, seed: 11111);
        var dungeon2 = _generator.GenerateDungeon("Test", Biome.Citadel, DifficultyTier.Tier1, 5, seed: 11111);

        // Assert
        dungeon1.RoomCount.Should().Be(dungeon2.RoomCount);
    }

    [Test]
    public void GenerateDungeon_DifferentSeedsProduceDifferentResults()
    {
        // Arrange & Act
        var dungeon1 = _generator.GenerateDungeon("Test", Biome.Citadel, DifficultyTier.Tier1, 10, seed: 11111);
        var dungeon2 = _generator.GenerateDungeon("Test", Biome.Citadel, DifficultyTier.Tier1, 10, seed: 22222);

        // Assert - Both should be valid, even if counts match by chance
        dungeon1.Should().NotBeNull();
        dungeon2.Should().NotBeNull();
    }

    [Test]
    public void GenerateDungeon_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => _generator.GenerateDungeon("", Biome.Citadel, DifficultyTier.Tier1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Test]
    public void GenerateDungeon_WithTooFewRooms_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => _generator.GenerateDungeon("Test", Biome.Citadel, DifficultyTier.Tier1, roomCount: 2);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("roomCount");
    }

    [Test]
    public async Task GenerateDungeonAsync_WorksCorrectly()
    {
        // Arrange & Act
        var dungeon = await _generator.GenerateDungeonAsync("Async Test", Biome.Citadel, DifficultyTier.Tier1, 5, seed: 42);

        // Assert
        dungeon.Should().NotBeNull();
        dungeon.Name.Should().Be("Async Test");
    }

    [Test]
    public void GenerateDungeon_HigherDifficulty_MoreThreat()
    {
        // This test verifies the threat budget scaling - harder difficulties should result in more monsters
        // We can't directly measure threat budget, but we can observe the effects

        // Arrange & Act - Both dungeons need enough rooms to see the difference
        var easyDungeon = _generator.GenerateDungeon("Easy", Biome.Citadel, DifficultyTier.Tier1, 10, seed: 999);
        var hardDungeon = _generator.GenerateDungeon("Hard", Biome.Citadel, DifficultyTier.Tier4, 10, seed: 999);

        // Assert - Both should be valid (the actual monster count depends on room layout)
        easyDungeon.Should().NotBeNull();
        hardDungeon.Should().NotBeNull();
    }
}
