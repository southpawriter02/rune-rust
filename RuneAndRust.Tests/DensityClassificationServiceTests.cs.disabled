using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.39.3: Tests for DensityClassificationService
/// Validates room density distribution and classification logic
/// </summary>
[TestClass]
public class DensityClassificationServiceTests
{
    private DensityClassificationService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _service = new DensityClassificationService();
    }

    #region Classification Tests

    [TestMethod]
    public void ClassifyRooms_BossRoom_AlwaysBossDensity()
    {
        // Arrange
        var rooms = new List<Room>
        {
            new Room { RoomId = "boss1", Archetype = RoomArchetype.BossArena, GeneratedNodeType = NodeType.Boss }
        };
        var rng = new Random(12345);

        // Act
        var classifications = _service.ClassifyRooms(rooms, rng);

        // Assert
        Assert.AreEqual(RoomDensity.Boss, classifications[rooms[0]], "Boss arena should always be Boss density");
    }

    [TestMethod]
    public void ClassifyRooms_EntryHall_AlwaysLightDensity()
    {
        // Arrange
        var rooms = new List<Room>
        {
            new Room { RoomId = "entry1", Archetype = RoomArchetype.EntryHall, IsStartRoom = true }
        };
        var rng = new Random(12345);

        // Act
        var classifications = _service.ClassifyRooms(rooms, rng);

        // Assert
        Assert.AreEqual(RoomDensity.Light, classifications[rooms[0]],
            "Entry hall should always be Light density for safe start");
    }

    [TestMethod]
    public void ClassifyRooms_SecretRoom_AlwaysEmptyDensity()
    {
        // Arrange
        var rooms = new List<Room>
        {
            new Room { RoomId = "secret1", Archetype = RoomArchetype.SecretRoom, GeneratedNodeType = NodeType.Secret }
        };
        var rng = new Random(12345);

        // Act
        var classifications = _service.ClassifyRooms(rooms, rng);

        // Assert
        Assert.AreEqual(RoomDensity.Empty, classifications[rooms[0]],
            "Secret room should always be Empty density to reward exploration");
    }

    [TestMethod]
    public void ClassifyRooms_7Rooms_HasVariety()
    {
        // Arrange
        var rooms = new List<Room>();
        for (int i = 0; i < 7; i++)
        {
            rooms.Add(new Room
            {
                RoomId = $"room_{i}",
                Archetype = RoomArchetype.Chamber,
                GeneratedNodeType = NodeType.Main
            });
        }
        var rng = new Random(12345);

        // Act
        var classifications = _service.ClassifyRooms(rooms, rng);

        // Assert - Should have multiple density types
        var densityCounts = classifications.Values
            .GroupBy(d => d)
            .ToDictionary(g => g.Key, g => g.Count());

        Assert.IsTrue(densityCounts.ContainsKey(RoomDensity.Empty) || densityCounts.ContainsKey(RoomDensity.Light),
            "Should have some Empty or Light rooms");
        Assert.IsTrue(densityCounts.ContainsKey(RoomDensity.Medium),
            "Should have some Medium rooms");
        Assert.IsTrue(densityCounts.Count >= 2, $"Should have variety (at least 2 density types), got {densityCounts.Count}");
    }

    [TestMethod]
    public void ClassifyRooms_10Rooms_FollowsTargetDistribution()
    {
        // Arrange: Target is ~10-15% Empty, 40-50% Light, 25-35% Medium, 10-15% Heavy
        var rooms = new List<Room>();
        for (int i = 0; i < 10; i++)
        {
            rooms.Add(new Room
            {
                RoomId = $"room_{i}",
                Archetype = RoomArchetype.Chamber,
                GeneratedNodeType = NodeType.Main
            });
        }
        var rng = new Random(12345);

        // Act
        var classifications = _service.ClassifyRooms(rooms, rng);

        // Assert
        var densityCounts = classifications.Values
            .GroupBy(d => d)
            .ToDictionary(g => g.Key, g => g.Count());

        var emptyCount = densityCounts.GetValueOrDefault(RoomDensity.Empty, 0);
        var lightCount = densityCounts.GetValueOrDefault(RoomDensity.Light, 0);
        var mediumCount = densityCounts.GetValueOrDefault(RoomDensity.Medium, 0);
        var heavyCount = densityCounts.GetValueOrDefault(RoomDensity.Heavy, 0);

        // Light should be most common (40-50%)
        Assert.IsTrue(lightCount >= 3 && lightCount <= 6,
            $"Light rooms should be 30-60% of total (3-6 out of 10), got {lightCount}");

        // Medium should be 25-35% (2-4 rooms out of 10)
        Assert.IsTrue(mediumCount >= 2 && mediumCount <= 4,
            $"Medium rooms should be 20-40% of total (2-4 out of 10), got {mediumCount}");
    }

    [TestMethod]
    public void ClassifyRooms_MixedArchetypes_CorrectClassifications()
    {
        // Arrange
        var rooms = new List<Room>
        {
            new Room { RoomId = "entry", Archetype = RoomArchetype.EntryHall, IsStartRoom = true },
            new Room { RoomId = "secret", Archetype = RoomArchetype.SecretRoom, GeneratedNodeType = NodeType.Secret },
            new Room { RoomId = "boss", Archetype = RoomArchetype.BossArena, IsBossRoom = true },
            new Room { RoomId = "room1", Archetype = RoomArchetype.Chamber },
            new Room { RoomId = "room2", Archetype = RoomArchetype.Corridor },
            new Room { RoomId = "room3", Archetype = RoomArchetype.LargeChamber }
        };
        var rng = new Random(12345);

        // Act
        var classifications = _service.ClassifyRooms(rooms, rng);

        // Assert
        Assert.AreEqual(RoomDensity.Light, classifications[rooms[0]], "Entry hall should be Light");
        Assert.AreEqual(RoomDensity.Empty, classifications[rooms[1]], "Secret room should be Empty");
        Assert.AreEqual(RoomDensity.Boss, classifications[rooms[2]], "Boss arena should be Boss");

        // Regular rooms should be classified with some variety
        var regularRoomDensities = new[] { classifications[rooms[3]], classifications[rooms[4]], classifications[rooms[5]] };
        Assert.IsTrue(regularRoomDensities.Any(d => d == RoomDensity.Light || d == RoomDensity.Medium || d == RoomDensity.Heavy),
            "Regular rooms should be classified as Light, Medium, or Heavy");
    }

    [TestMethod]
    public void ClassifyRooms_EmptyList_ReturnsEmptyDictionary()
    {
        // Arrange
        var rooms = new List<Room>();
        var rng = new Random(12345);

        // Act
        var classifications = _service.ClassifyRooms(rooms, rng);

        // Assert
        Assert.AreEqual(0, classifications.Count, "Empty room list should return empty dictionary");
    }

    [TestMethod]
    public void ClassifyRooms_DeterministicWithSeed()
    {
        // Arrange
        var rooms = new List<Room>();
        for (int i = 0; i < 5; i++)
        {
            rooms.Add(new Room { RoomId = $"room_{i}", Archetype = RoomArchetype.Chamber });
        }

        var rng1 = new Random(12345);
        var rng2 = new Random(12345);

        // Act
        var classifications1 = _service.ClassifyRooms(rooms, rng1);
        var classifications2 = _service.ClassifyRooms(rooms, rng2);

        // Assert - Same seed should produce same classifications
        foreach (var room in rooms)
        {
            Assert.AreEqual(classifications1[room], classifications2[room],
                $"Room {room.RoomId} should have same classification with same seed");
        }
    }

    #endregion
}
