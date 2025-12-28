using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.ValueObjects;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the DungeonGenerator service (v0.3.24a).
/// Validates template-based dungeon generation and room connectivity.
/// v0.3.24a: Updated to test GenerateDungeonAsync after GenerateTestMapAsync removal.
/// </summary>
public class DungeonGeneratorTests
{
    private readonly Mock<IRoomRepository> _mockRoomRepo;
    private readonly Mock<IEnvironmentPopulator> _mockEnvironmentPopulator;
    private readonly Mock<IRoomTemplateRepository> _mockRoomTemplateRepo;
    private readonly Mock<IBiomeDefinitionRepository> _mockBiomeDefinitionRepo;
    private readonly Mock<ITemplateRendererService> _mockTemplateRendererService;
    private readonly Mock<IDiceService> _mockDiceService;
    private readonly Mock<ILogger<DungeonGenerator>> _mockLogger;
    private readonly DungeonGenerator _generator;
    private List<Room> _addedRooms = new();

    public DungeonGeneratorTests()
    {
        _mockRoomRepo = new Mock<IRoomRepository>();
        _mockEnvironmentPopulator = new Mock<IEnvironmentPopulator>();
        _mockRoomTemplateRepo = new Mock<IRoomTemplateRepository>();
        _mockBiomeDefinitionRepo = new Mock<IBiomeDefinitionRepository>();
        _mockTemplateRendererService = new Mock<ITemplateRendererService>();
        _mockDiceService = new Mock<IDiceService>();
        _mockLogger = new Mock<ILogger<DungeonGenerator>>();
        _generator = new DungeonGenerator(
            _mockRoomRepo.Object,
            _mockEnvironmentPopulator.Object,
            _mockRoomTemplateRepo.Object,
            _mockBiomeDefinitionRepo.Object,
            _mockTemplateRendererService.Object,
            _mockDiceService.Object,
            _mockLogger.Object);

        // Capture rooms when AddRangeAsync is called
        _mockRoomRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Room>>()))
            .Callback<IEnumerable<Room>>(rooms => _addedRooms = rooms.ToList())
            .Returns(Task.CompletedTask);

        _mockRoomRepo.Setup(r => r.ClearAllRoomsAsync())
            .Returns(Task.CompletedTask);

        _mockRoomRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
    }

    /// <summary>
    /// Sets up mock repositories with a valid biome and templates for testing.
    /// </summary>
    private void SetupValidBiomeAndTemplates(int roomCount = 5)
    {
        // Create test biome
        var biome = new BiomeDefinition
        {
            BiomeId = "the_roots",
            Name = "The Roots",
            Description = "Test biome for dungeon generation",
            MinRoomCount = roomCount,
            MaxRoomCount = roomCount,
            AvailableTemplates = new List<string>
            {
                "roots_entry_01",
                "roots_corridor_01",
                "roots_chamber_01",
                "roots_boss_01"
            }
        };

        _mockBiomeDefinitionRepo.Setup(r => r.GetByBiomeIdAsync("the_roots"))
            .ReturnsAsync(biome);

        // Create test templates
        var entryTemplate = new RoomTemplate
        {
            TemplateId = "roots_entry_01",
            Archetype = "EntryHall",
            BiomeId = "the_roots",
            NameTemplates = new List<string> { "Entry Hall" },
            DescriptionTemplates = new List<string> { "A cold, metallic chamber." },
            Difficulty = "Easy"
        };

        var corridorTemplate = new RoomTemplate
        {
            TemplateId = "roots_corridor_01",
            Archetype = "Corridor",
            BiomeId = "the_roots",
            NameTemplates = new List<string> { "Rusted Corridor" },
            DescriptionTemplates = new List<string> { "Corroded pipes line the walls." },
            Difficulty = "Medium"
        };

        var chamberTemplate = new RoomTemplate
        {
            TemplateId = "roots_chamber_01",
            Archetype = "Chamber",
            BiomeId = "the_roots",
            NameTemplates = new List<string> { "Storage Chamber" },
            DescriptionTemplates = new List<string> { "Broken crates litter the floor." },
            Difficulty = "Medium"
        };

        var bossTemplate = new RoomTemplate
        {
            TemplateId = "roots_boss_01",
            Archetype = "BossArena",
            BiomeId = "the_roots",
            NameTemplates = new List<string> { "The Pit" },
            DescriptionTemplates = new List<string> { "A deep shaft descends into darkness." },
            Difficulty = "Hard"
        };

        var allTemplates = new List<RoomTemplate> { entryTemplate, corridorTemplate, chamberTemplate, bossTemplate };
        _mockRoomTemplateRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(allTemplates);

        // Setup template renderer to return first template from list
        _mockTemplateRendererService.Setup(r => r.RenderRoomName(It.IsAny<RoomTemplate>()))
            .Returns<RoomTemplate>(t => t.NameTemplates.FirstOrDefault() ?? "Unknown Room");

        _mockTemplateRendererService.Setup(r => r.RenderRoomDescription(It.IsAny<RoomTemplate>(), It.IsAny<BiomeDefinition>()))
            .Returns<RoomTemplate, BiomeDefinition>((t, b) => t.DescriptionTemplates.FirstOrDefault() ?? "No description.");

        // Setup dice service (deterministic)
        _mockDiceService.Setup(d => d.RollSingle(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(1); // Always returns 1 for deterministic tests
    }

    #region GenerateDungeonAsync Tests

    [Fact]
    public async Task GenerateDungeonAsync_ClearsExistingRooms()
    {
        // Arrange
        SetupValidBiomeAndTemplates();

        // Act
        await _generator.GenerateDungeonAsync("the_roots");

        // Assert
        _mockRoomRepo.Verify(r => r.ClearAllRoomsAsync(), Times.Once);
    }

    [Fact]
    public async Task GenerateDungeonAsync_CreatesRoomsMatchingBiomeCount()
    {
        // Arrange
        SetupValidBiomeAndTemplates(roomCount: 5);

        // Act
        await _generator.GenerateDungeonAsync("the_roots");

        // Assert
        _addedRooms.Should().HaveCount(5);
    }

    [Fact]
    public async Task GenerateDungeonAsync_HasExactlyOneStartingRoom()
    {
        // Arrange
        SetupValidBiomeAndTemplates();

        // Act
        await _generator.GenerateDungeonAsync("the_roots");

        // Assert
        _addedRooms.Count(r => r.IsStartingRoom).Should().Be(1);
    }

    [Fact]
    public async Task GenerateDungeonAsync_StartingRoomIsAtOrigin()
    {
        // Arrange
        SetupValidBiomeAndTemplates();

        // Act
        await _generator.GenerateDungeonAsync("the_roots");

        // Assert
        var startingRoom = _addedRooms.Single(r => r.IsStartingRoom);
        startingRoom.Position.Should().Be(Coordinate.Origin);
    }

    [Fact]
    public async Task GenerateDungeonAsync_ReturnsStartingRoomId()
    {
        // Arrange
        SetupValidBiomeAndTemplates();

        // Act
        var result = await _generator.GenerateDungeonAsync("the_roots");

        // Assert
        var startingRoom = _addedRooms.Single(r => r.IsStartingRoom);
        result.Should().Be(startingRoom.Id);
    }

    [Fact]
    public async Task GenerateDungeonAsync_AllRoomsHaveUniqueIds()
    {
        // Arrange
        SetupValidBiomeAndTemplates();

        // Act
        await _generator.GenerateDungeonAsync("the_roots");

        // Assert
        var ids = _addedRooms.Select(r => r.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task GenerateDungeonAsync_AllRoomsHaveUniquePositions()
    {
        // Arrange
        SetupValidBiomeAndTemplates();

        // Act
        await _generator.GenerateDungeonAsync("the_roots");

        // Assert
        var positions = _addedRooms.Select(r => r.Position).ToList();
        positions.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task GenerateDungeonAsync_AllRoomsHaveNonEmptyNames()
    {
        // Arrange
        SetupValidBiomeAndTemplates();

        // Act
        await _generator.GenerateDungeonAsync("the_roots");

        // Assert
        foreach (var room in _addedRooms)
        {
            room.Name.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task GenerateDungeonAsync_AllRoomsHaveNonEmptyDescriptions()
    {
        // Arrange
        SetupValidBiomeAndTemplates();

        // Act
        await _generator.GenerateDungeonAsync("the_roots");

        // Assert
        foreach (var room in _addedRooms)
        {
            room.Description.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task GenerateDungeonAsync_ExitsAreBidirectional()
    {
        // Arrange
        SetupValidBiomeAndTemplates();

        // Act
        await _generator.GenerateDungeonAsync("the_roots");

        // Assert - verify that for each exit, the target room has a return exit
        foreach (var room in _addedRooms)
        {
            foreach (var (direction, targetId) in room.Exits)
            {
                var targetRoom = _addedRooms.Single(r => r.Id == targetId);
                var oppositeDirection = DungeonGenerator.GetOppositeDirection(direction);
                targetRoom.Exits.Should().ContainKey(oppositeDirection);
                targetRoom.Exits[oppositeDirection].Should().Be(room.Id);
            }
        }
    }

    [Fact]
    public async Task GenerateDungeonAsync_ExitsPointToValidRooms()
    {
        // Arrange
        SetupValidBiomeAndTemplates();

        // Act
        await _generator.GenerateDungeonAsync("the_roots");

        // Assert
        var allIds = _addedRooms.Select(r => r.Id).ToHashSet();

        foreach (var room in _addedRooms)
        {
            foreach (var targetId in room.Exits.Values)
            {
                allIds.Should().Contain(targetId);
            }
        }
    }

    [Fact]
    public async Task GenerateDungeonAsync_SavesChangesToDatabase()
    {
        // Arrange
        SetupValidBiomeAndTemplates();

        // Act
        await _generator.GenerateDungeonAsync("the_roots");

        // Assert
        _mockRoomRepo.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Room>>()), Times.Once);
        _mockRoomRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GenerateDungeonAsync_ThrowsForUnknownBiome()
    {
        // Arrange
        _mockBiomeDefinitionRepo.Setup(r => r.GetByBiomeIdAsync("unknown_biome"))
            .ReturnsAsync((BiomeDefinition?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _generator.GenerateDungeonAsync("unknown_biome"));
    }

    [Fact]
    public async Task GenerateDungeonAsync_PopulatesEachRoom()
    {
        // Arrange
        SetupValidBiomeAndTemplates(roomCount: 3);

        // Act
        await _generator.GenerateDungeonAsync("the_roots");

        // Assert - Environment populator should be called once per room
        _mockEnvironmentPopulator.Verify(
            p => p.PopulateRoomAsync(It.IsAny<Room>()),
            Times.Exactly(3));
    }

    #endregion

    #region GetOppositeDirection Tests

    [Theory]
    [InlineData(Direction.North, Direction.South)]
    [InlineData(Direction.South, Direction.North)]
    [InlineData(Direction.East, Direction.West)]
    [InlineData(Direction.West, Direction.East)]
    [InlineData(Direction.Up, Direction.Down)]
    [InlineData(Direction.Down, Direction.Up)]
    public void GetOppositeDirection_ReturnsCorrectOpposite(Direction input, Direction expected)
    {
        // Act
        var result = DungeonGenerator.GetOppositeDirection(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetOppositeDirection_IsSymmetric()
    {
        // Arrange
        var allDirections = Enum.GetValues<Direction>();

        // Assert
        foreach (var direction in allDirections)
        {
            var opposite = DungeonGenerator.GetOppositeDirection(direction);
            var backToOriginal = DungeonGenerator.GetOppositeDirection(opposite);
            backToOriginal.Should().Be(direction);
        }
    }

    #endregion

    #region Linear Layout Tests

    [Fact]
    public async Task GenerateDungeonAsync_RoomsFormLinearPath()
    {
        // Arrange
        SetupValidBiomeAndTemplates(roomCount: 4);

        // Act
        await _generator.GenerateDungeonAsync("the_roots");

        // Assert - Rooms should be arranged linearly (Y increases)
        var sortedByY = _addedRooms.OrderBy(r => r.Position.Y).ToList();
        for (int i = 0; i < sortedByY.Count; i++)
        {
            sortedByY[i].Position.Y.Should().Be(i);
            sortedByY[i].Position.X.Should().Be(0); // All on same X axis
            sortedByY[i].Position.Z.Should().Be(0); // All on same Z level
        }
    }

    [Fact]
    public async Task GenerateDungeonAsync_FirstRoomIsStartingRoom()
    {
        // Arrange
        SetupValidBiomeAndTemplates();

        // Act
        await _generator.GenerateDungeonAsync("the_roots");

        // Assert
        var roomAtOrigin = _addedRooms.Single(r => r.Position == Coordinate.Origin);
        roomAtOrigin.IsStartingRoom.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateDungeonAsync_RoomsConnectedNorthSouth()
    {
        // Arrange
        SetupValidBiomeAndTemplates(roomCount: 3);

        // Act
        await _generator.GenerateDungeonAsync("the_roots");

        // Assert - Check north/south connectivity
        var startRoom = _addedRooms.Single(r => r.IsStartingRoom);
        startRoom.Exits.Should().ContainKey(Direction.North);

        var middleRoom = _addedRooms.Single(r => r.Position.Y == 1);
        middleRoom.Exits.Should().ContainKey(Direction.South);
        middleRoom.Exits.Should().ContainKey(Direction.North);

        var endRoom = _addedRooms.Single(r => r.Position.Y == 2);
        endRoom.Exits.Should().ContainKey(Direction.South);
    }

    #endregion
}
