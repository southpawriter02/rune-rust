using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using System.IO;
using System.Text.Json;

namespace RuneAndRust.Tests;

/// <summary>
/// Tests for RoomInstantiator and Dungeon model (v0.10)
/// </summary>
[TestFixture]
public class RoomInstantiationTests
{
    private string _testDataPath = string.Empty;
    private TemplateLibrary _library = null!;
    private DungeonGenerator _generator = null!;

    [SetUp]
    public void Setup()
    {
        // Create test templates
        _testDataPath = Path.Combine(Path.GetTempPath(), $"RuneRustInstTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataPath);
        CreateTestTemplates();

        _library = new TemplateLibrary(_testDataPath);
        _library.LoadTemplates();
        _generator = new DungeonGenerator(_library);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDataPath))
        {
            Directory.Delete(_testDataPath, recursive: true);
        }
    }

    #region Dungeon Model Tests

    [Test]
    public void Dungeon_GetStartRoom_ReturnsCorrectRoom()
    {
        // Arrange
        var dungeon = _generator.GenerateComplete(seed: 42, dungeonId: 1);

        // Act
        var startRoom = dungeon.GetStartRoom();

        // Assert
        Assert.That(startRoom, Is.Not.Null);
        Assert.That(startRoom!.IsStartRoom, Is.True);
        Assert.That(startRoom.GeneratedNodeType, Is.EqualTo(NodeType.Start));
    }

    [Test]
    public void Dungeon_GetBossRoom_ReturnsCorrectRoom()
    {
        // Arrange
        var dungeon = _generator.GenerateComplete(seed: 42, dungeonId: 1);

        // Act
        var bossRoom = dungeon.GetBossRoom();

        // Assert
        Assert.That(bossRoom, Is.Not.Null);
        Assert.That(bossRoom!.IsBossRoom, Is.True);
        Assert.That(bossRoom.GeneratedNodeType, Is.EqualTo(NodeType.Boss));
    }

    [Test]
    public void Dungeon_GetRoom_WithValidId_ReturnsRoom()
    {
        // Arrange
        var dungeon = _generator.GenerateComplete(seed: 42, dungeonId: 1);
        var roomId = dungeon.Rooms.Keys.First();

        // Act
        var room = dungeon.GetRoom(roomId);

        // Assert
        Assert.That(room, Is.Not.Null);
        Assert.That(room!.RoomId, Is.EqualTo(roomId));
    }

    [Test]
    public void Dungeon_GetRoom_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var dungeon = _generator.GenerateComplete(seed: 42, dungeonId: 1);

        // Act
        var room = dungeon.GetRoom("nonexistent_room");

        // Assert
        Assert.That(room, Is.Null);
    }

    [Test]
    public void Dungeon_GetSecretRooms_ReturnsOnlySecretRooms()
    {
        // Arrange - Generate multiple dungeons to find one with secrets
        Dungeon? dungeonWithSecret = null;
        for (int seed = 0; seed < 30; seed++)
        {
            var dungeon = _generator.GenerateComplete(seed, dungeonId: seed);
            if (dungeon.GetSecretRooms().Count > 0)
            {
                dungeonWithSecret = dungeon;
                break;
            }
        }

        // Assert
        if (dungeonWithSecret != null)
        {
            var secretRooms = dungeonWithSecret.GetSecretRooms();
            Assert.That(secretRooms, Is.All.Property("GeneratedNodeType").EqualTo(NodeType.Secret));
        }
    }

    [Test]
    public void Dungeon_Validate_WithValidDungeon_ReturnsTrue()
    {
        // Arrange
        var dungeon = _generator.GenerateComplete(seed: 42, dungeonId: 1);

        // Act
        var (isValid, errors) = dungeon.Validate();

        // Assert
        Assert.That(isValid, Is.True, $"Validation errors: {string.Join(", ", errors)}");
        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void Dungeon_GetStatistics_ReturnsCorrectCounts()
    {
        // Arrange
        var dungeon = _generator.GenerateComplete(seed: 42, dungeonId: 1);

        // Act
        var stats = dungeon.GetStatistics();

        // Assert
        Assert.That(stats["TotalRooms"], Is.EqualTo(dungeon.TotalRoomCount));
        Assert.That(stats["StartRooms"], Is.EqualTo(1));
        Assert.That(stats["BossRooms"], Is.EqualTo(1));

        // All counts should sum to total
        var sum = stats["StartRooms"] + stats["MainRooms"] + stats["BranchRooms"] +
                  stats["SecretRooms"] + stats["BossRooms"];
        Assert.That(sum, Is.EqualTo(stats["TotalRooms"]));
    }

    #endregion

    #region Room Instantiation Tests

    [Test]
    public void GenerateComplete_CreatesRoomsFromGraph()
    {
        // Act
        var dungeon = _generator.GenerateComplete(seed: 42, dungeonId: 1);

        // Assert
        Assert.That(dungeon, Is.Not.Null);
        Assert.That(dungeon.Rooms.Count, Is.GreaterThan(0));
    }

    [Test]
    public void GenerateComplete_AllRoomsHaveNames()
    {
        // Act
        var dungeon = _generator.GenerateComplete(seed: 42, dungeonId: 1);

        // Assert
        foreach (var room in dungeon.Rooms.Values)
        {
            Assert.That(room.Name, Is.Not.Empty,
                $"Room {room.RoomId} has no name");
        }
    }

    [Test]
    public void GenerateComplete_AllRoomsHaveDescriptions()
    {
        // Act
        var dungeon = _generator.GenerateComplete(seed: 42, dungeonId: 1);

        // Assert
        foreach (var room in dungeon.Rooms.Values)
        {
            Assert.That(room.Description, Is.Not.Empty,
                $"Room {room.RoomId} has no description");
        }
    }

    [Test]
    public void GenerateComplete_AllRoomsHaveTemplateIds()
    {
        // Act
        var dungeon = _generator.GenerateComplete(seed: 42, dungeonId: 1);

        // Assert
        foreach (var room in dungeon.Rooms.Values)
        {
            Assert.That(room.TemplateId, Is.Not.Null,
                $"Room {room.RoomId} has no template ID");
            Assert.That(room.IsProcedurallyGenerated, Is.True);
        }
    }

    [Test]
    public void GenerateComplete_AllRoomsHaveNodeTypes()
    {
        // Act
        var dungeon = _generator.GenerateComplete(seed: 42, dungeonId: 1);

        // Assert
        foreach (var room in dungeon.Rooms.Values)
        {
            Assert.That(room.GeneratedNodeType, Is.Not.Null,
                $"Room {room.RoomId} has no node type");
        }
    }

    [Test]
    public void GenerateComplete_RoomExitsConnectToValidRooms()
    {
        // Act
        var dungeon = _generator.GenerateComplete(seed: 42, dungeonId: 1);

        // Assert
        foreach (var room in dungeon.Rooms.Values)
        {
            foreach (var (direction, targetRoomId) in room.Exits)
            {
                Assert.That(dungeon.Rooms.ContainsKey(targetRoomId), Is.True,
                    $"Room {room.RoomId} has exit {direction} to non-existent room {targetRoomId}");
            }
        }
    }

    [Test]
    public void GenerateComplete_ExitsAreBidirectional()
    {
        // Act
        var dungeon = _generator.GenerateComplete(seed: 42, dungeonId: 1);

        // Assert
        foreach (var room in dungeon.Rooms.Values)
        {
            foreach (var (direction, targetRoomId) in room.Exits)
            {
                var targetRoom = dungeon.GetRoom(targetRoomId);
                Assert.That(targetRoom, Is.Not.Null);

                // Check that target room has exit back to this room
                var hasReverseExit = targetRoom!.Exits.Values.Contains(room.RoomId);
                Assert.That(hasReverseExit, Is.True,
                    $"Room {room.RoomId} -> {direction} -> {targetRoomId}, but no reverse exit found");
            }
        }
    }

    [Test]
    public void GenerateComplete_StartRoomHasNoEnemies()
    {
        // Act
        var dungeon = _generator.GenerateComplete(seed: 42, dungeonId: 1);
        var startRoom = dungeon.GetStartRoom();

        // Assert
        Assert.That(startRoom, Is.Not.Null);
        Assert.That(startRoom!.Enemies, Is.Empty);
        Assert.That(startRoom.HasBeenCleared, Is.True);
        Assert.That(startRoom.IsSanctuary, Is.True);
    }

    #endregion

    #region Name/Description Generation Tests

    [Test]
    public void GenerateComplete_NamesContainNoPlaceholders()
    {
        // Act
        var dungeon = _generator.GenerateComplete(seed: 42, dungeonId: 1);

        // Assert
        foreach (var room in dungeon.Rooms.Values)
        {
            Assert.That(room.Name, Does.Not.Contain("{Adjective}"),
                $"Room {room.RoomId} name contains unreplaced placeholder");
            Assert.That(room.Name, Does.Not.Contain("{Detail}"),
                $"Room {room.RoomId} name contains unreplaced placeholder");
        }
    }

    [Test]
    public void GenerateComplete_DescriptionsContainNoPlaceholders()
    {
        // Act
        var dungeon = _generator.GenerateComplete(seed: 42, dungeonId: 1);

        // Assert
        foreach (var room in dungeon.Rooms.Values)
        {
            Assert.That(room.Description, Does.Not.Contain("{Adjective}"),
                $"Room {room.RoomId} description contains unreplaced placeholder");
            Assert.That(room.Description, Does.Not.Contain("{Detail}"),
                $"Room {room.RoomId} description contains unreplaced placeholder");
        }
    }

    [Test]
    public void GenerateComplete_WithSameSeed_GeneratesSameNames()
    {
        // Act
        var dungeon1 = _generator.GenerateComplete(seed: 12345, dungeonId: 1);
        var dungeon2 = _generator.GenerateComplete(seed: 12345, dungeonId: 2);

        // Assert
        var rooms1 = dungeon1.Rooms.Values.OrderBy(r => r.RoomId).ToList();
        var rooms2 = dungeon2.Rooms.Values.OrderBy(r => r.RoomId).ToList();

        for (int i = 0; i < Math.Min(rooms1.Count, rooms2.Count); i++)
        {
            Assert.That(rooms1[i].Name, Is.EqualTo(rooms2[i].Name),
                $"Room names differ at index {i}");
        }
    }

    [Test]
    public void GenerateComplete_WithDifferentSeeds_GeneratesDifferentNames()
    {
        // Act
        var dungeon1 = _generator.GenerateComplete(seed: 111, dungeonId: 1);
        var dungeon2 = _generator.GenerateComplete(seed: 222, dungeonId: 2);

        // Assert
        var names1 = dungeon1.Rooms.Values.Select(r => r.Name).ToHashSet();
        var names2 = dungeon2.Rooms.Values.Select(r => r.Name).ToHashSet();

        // At least some names should be different
        Assert.That(names1, Is.Not.EqualTo(names2));
    }

    #endregion

    #region Reproducibility Tests

    [Test]
    public void GenerateComplete_WithSameSeed_GeneratesIdenticalDungeons()
    {
        // Act
        var dungeon1 = _generator.GenerateComplete(seed: 42, dungeonId: 1);
        var dungeon2 = _generator.GenerateComplete(seed: 42, dungeonId: 2);

        // Assert
        Assert.That(dungeon1.Rooms.Count, Is.EqualTo(dungeon2.Rooms.Count));

        var rooms1 = dungeon1.Rooms.Values.OrderBy(r => r.RoomId).ToList();
        var rooms2 = dungeon2.Rooms.Values.OrderBy(r => r.RoomId).ToList();

        for (int i = 0; i < rooms1.Count; i++)
        {
            Assert.That(rooms1[i].Name, Is.EqualTo(rooms2[i].Name));
            Assert.That(rooms1[i].Description, Is.EqualTo(rooms2[i].Description));
            Assert.That(rooms1[i].TemplateId, Is.EqualTo(rooms2[i].TemplateId));
            Assert.That(rooms1[i].Exits.Count, Is.EqualTo(rooms2[i].Exits.Count));
        }
    }

    #endregion

    #region Helper Methods

    private void CreateTestTemplates()
    {
        SaveTemplate(new RoomTemplate
        {
            TemplateId = "test_entry",
            Archetype = RoomArchetype.EntryHall,
            NameTemplates = new List<string> { "The {Adjective} Entry" },
            Adjectives = new List<string> { "Collapsed", "Shattered" },
            DescriptionTemplates = new List<string> { "A {Adjective} entry hall. {Detail}." },
            Details = new List<string> { "Debris litters the floor", "The ceiling sags" },
            ValidConnections = new List<RoomArchetype> { RoomArchetype.Corridor, RoomArchetype.Chamber },
            MinConnectionPoints = 1,
            MaxConnectionPoints = 2
        }, "test_entry.json");

        SaveTemplate(new RoomTemplate
        {
            TemplateId = "test_corridor",
            Archetype = RoomArchetype.Corridor,
            NameTemplates = new List<string> { "The {Adjective} Corridor" },
            Adjectives = new List<string> { "Rusted", "Twisted" },
            DescriptionTemplates = new List<string> { "A {Adjective} corridor. {Detail}." },
            Details = new List<string> { "Rust covers the walls", "Pipes hiss overhead" },
            ValidConnections = new List<RoomArchetype> { RoomArchetype.Corridor, RoomArchetype.Chamber },
            MinConnectionPoints = 2,
            MaxConnectionPoints = 3
        }, "test_corridor.json");

        SaveTemplate(new RoomTemplate
        {
            TemplateId = "test_chamber",
            Archetype = RoomArchetype.Chamber,
            NameTemplates = new List<string> { "The {Adjective} Chamber" },
            Adjectives = new List<string> { "Empty", "Vast" },
            DescriptionTemplates = new List<string> { "A {Adjective} chamber. {Detail}." },
            Details = new List<string> { "Echoes fill the space", "Shadows dance on walls" },
            ValidConnections = new List<RoomArchetype> { RoomArchetype.Corridor, RoomArchetype.Chamber },
            MinConnectionPoints = 1,
            MaxConnectionPoints = 3
        }, "test_chamber.json");

        SaveTemplate(new RoomTemplate
        {
            TemplateId = "test_boss",
            Archetype = RoomArchetype.BossArena,
            NameTemplates = new List<string> { "The {Adjective} Vault" },
            Adjectives = new List<string> { "Sealed", "Fortified" },
            DescriptionTemplates = new List<string> { "A {Adjective} vault. {Detail}." },
            Details = new List<string> { "Danger awaits", "This is the final chamber" },
            ValidConnections = new List<RoomArchetype> { RoomArchetype.Corridor },
            MinConnectionPoints = 1,
            MaxConnectionPoints = 2
        }, "test_boss.json");

        SaveTemplate(new RoomTemplate
        {
            TemplateId = "test_secret",
            Archetype = RoomArchetype.SecretRoom,
            NameTemplates = new List<string> { "The {Adjective} Cache" },
            Adjectives = new List<string> { "Hidden", "Forgotten" },
            DescriptionTemplates = new List<string> { "A {Adjective} cache. {Detail}." },
            Details = new List<string> { "Few would find this place", "Dust covers everything" },
            ValidConnections = new List<RoomArchetype> { RoomArchetype.Corridor },
            MinConnectionPoints = 1,
            MaxConnectionPoints = 1
        }, "test_secret.json");
    }

    private void SaveTemplate(RoomTemplate template, string fileName)
    {
        var json = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(_testDataPath, fileName), json);
    }

    #endregion
}
