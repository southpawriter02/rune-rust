using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Persistence;
using System.Text.Json;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.13: Unit tests for WorldStateRepository
/// </summary>
[TestClass]
public class WorldStateRepositoryTests
{
    private WorldStateRepository? _repository;
    private string? _testDbPath;

    [TestInitialize]
    public void Setup()
    {
        // Create temporary test database
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_runeandrust_{Guid.NewGuid()}.db");
        var testDir = Path.GetDirectoryName(_testDbPath)!;

        _repository = new WorldStateRepository(testDir);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Delete test database
        if (_testDbPath != null && File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    [TestMethod]
    public void RecordChange_ValidTerrainDestroyed_SavedToDatabase()
    {
        // Arrange
        var change = CreateTerrainDestroyedChange(saveId: 1, "d1", "room_d1_n5", "pillar_1");

        // Act
        _repository!.RecordChange(change);

        // Assert
        var retrieved = _repository.GetChangesForRoom(1, "d1", "room_d1_n5");
        Assert.AreEqual(1, retrieved.Count);
        Assert.AreEqual(WorldStateChangeType.TerrainDestroyed, retrieved[0].ChangeType);
        Assert.AreEqual("pillar_1", retrieved[0].TargetId);
    }

    [TestMethod]
    public void RecordChange_ValidHazardDestroyed_SavedToDatabase()
    {
        // Arrange
        var change = CreateHazardDestroyedChange(saveId: 1, "d1", "room_d1_n5", "steam_vent_1");

        // Act
        _repository!.RecordChange(change);

        // Assert
        var retrieved = _repository.GetChangesForRoom(1, "d1", "room_d1_n5");
        Assert.AreEqual(1, retrieved.Count);
        Assert.AreEqual(WorldStateChangeType.HazardDestroyed, retrieved[0].ChangeType);
    }

    [TestMethod]
    public void RecordChange_ValidEnemyDefeated_SavedToDatabase()
    {
        // Arrange
        var change = CreateEnemyDefeatedChange(saveId: 1, "d1", "room_d1_n5", "Rusted Servitor");

        // Act
        _repository!.RecordChange(change);

        // Assert
        var retrieved = _repository.GetChangesForRoom(1, "d1", "room_d1_n5");
        Assert.AreEqual(1, retrieved.Count);
        Assert.AreEqual(WorldStateChangeType.EnemyDefeated, retrieved[0].ChangeType);
        Assert.AreEqual("Rusted Servitor", retrieved[0].TargetId);
    }

    [TestMethod]
    public void GetChangesForRoom_MultipleChanges_ReturnsInChronologicalOrder()
    {
        // Arrange
        var change1 = CreateTerrainDestroyedChange(1, "d1", "room_d1_n5", "pillar_1");
        change1.Timestamp = DateTime.UtcNow.AddMinutes(-10);

        var change2 = CreateHazardDestroyedChange(1, "d1", "room_d1_n5", "steam_vent_1");
        change2.Timestamp = DateTime.UtcNow.AddMinutes(-5);

        var change3 = CreateEnemyDefeatedChange(1, "d1", "room_d1_n5", "Servitor");
        change3.Timestamp = DateTime.UtcNow;

        // Act
        _repository!.RecordChange(change1);
        _repository.RecordChange(change2);
        _repository.RecordChange(change3);

        // Assert
        var retrieved = _repository.GetChangesForRoom(1, "d1", "room_d1_n5");
        Assert.AreEqual(3, retrieved.Count);

        // Verify chronological order
        Assert.IsTrue(retrieved[0].Timestamp < retrieved[1].Timestamp);
        Assert.IsTrue(retrieved[1].Timestamp < retrieved[2].Timestamp);
    }

    [TestMethod]
    public void GetChangesForRoom_DifferentRooms_ReturnsOnlyMatchingRoom()
    {
        // Arrange
        _repository!.RecordChange(CreateTerrainDestroyedChange(1, "d1", "room_d1_n5", "pillar_1"));
        _repository.RecordChange(CreateTerrainDestroyedChange(1, "d1", "room_d1_n6", "pillar_2"));
        _repository.RecordChange(CreateTerrainDestroyedChange(1, "d1", "room_d1_n5", "bulkhead_1"));

        // Act
        var room5Changes = _repository.GetChangesForRoom(1, "d1", "room_d1_n5");
        var room6Changes = _repository.GetChangesForRoom(1, "d1", "room_d1_n6");

        // Assert
        Assert.AreEqual(2, room5Changes.Count);
        Assert.AreEqual(1, room6Changes.Count);
    }

    [TestMethod]
    public void GetChangesForSector_MultipleSectors_ReturnsOnlyMatchingSector()
    {
        // Arrange
        _repository!.RecordChange(CreateTerrainDestroyedChange(1, "d1", "room_d1_n5", "pillar_1"));
        _repository.RecordChange(CreateTerrainDestroyedChange(1, "d1", "room_d1_n6", "pillar_2"));
        _repository.RecordChange(CreateTerrainDestroyedChange(1, "d2", "room_d2_n5", "pillar_3"));

        // Act
        var sector1Changes = _repository.GetChangesForSector(1, "d1");
        var sector2Changes = _repository.GetChangesForSector(1, "d2");

        // Assert
        Assert.AreEqual(2, sector1Changes.Count);
        Assert.AreEqual(1, sector2Changes.Count);
    }

    [TestMethod]
    public void GetChangesForSave_MultipleSaves_ReturnsOnlyMatchingSave()
    {
        // Arrange
        _repository!.RecordChange(CreateTerrainDestroyedChange(saveId: 1, "d1", "room_d1_n5", "pillar_1"));
        _repository.RecordChange(CreateTerrainDestroyedChange(saveId: 1, "d1", "room_d1_n6", "pillar_2"));
        _repository.RecordChange(CreateTerrainDestroyedChange(saveId: 2, "d1", "room_d1_n5", "pillar_3"));

        // Act
        var save1Changes = _repository.GetChangesForSave(1);
        var save2Changes = _repository.GetChangesForSave(2);

        // Assert
        Assert.AreEqual(2, save1Changes.Count);
        Assert.AreEqual(1, save2Changes.Count);
    }

    [TestMethod]
    public void GetChangesForRoom_NoChanges_ReturnsEmptyList()
    {
        // Act
        var changes = _repository!.GetChangesForRoom(1, "d1", "room_d1_n5");

        // Assert
        Assert.AreEqual(0, changes.Count);
    }

    [TestMethod]
    public void GetChangeCountForRoom_MultipleChanges_ReturnsCorrectCount()
    {
        // Arrange
        _repository!.RecordChange(CreateTerrainDestroyedChange(1, "d1", "room_d1_n5", "pillar_1"));
        _repository.RecordChange(CreateHazardDestroyedChange(1, "d1", "room_d1_n5", "steam_vent_1"));
        _repository.RecordChange(CreateEnemyDefeatedChange(1, "d1", "room_d1_n5", "Servitor"));

        // Act
        var count = _repository.GetChangeCountForRoom(1, "room_d1_n5");

        // Assert
        Assert.AreEqual(3, count);
    }

    [TestMethod]
    public void DeleteChangesForSave_RemovesAllChanges()
    {
        // Arrange
        _repository!.RecordChange(CreateTerrainDestroyedChange(1, "d1", "room_d1_n5", "pillar_1"));
        _repository.RecordChange(CreateHazardDestroyedChange(1, "d1", "room_d1_n6", "steam_vent_1"));

        // Act
        _repository.DeleteChangesForSave(1);

        // Assert
        var changes = _repository.GetChangesForSave(1);
        Assert.AreEqual(0, changes.Count);
    }

    // Helper Methods

    private WorldStateChange CreateTerrainDestroyedChange(
        int saveId, string sectorSeed, string roomId, string targetId)
    {
        var data = new TerrainDestroyedData
        {
            ElementType = "CollapsedPillar",
            SpawnRubble = true,
            DestroyedBy = "Test Player"
        };

        return new WorldStateChange
        {
            SaveId = saveId,
            SectorSeed = sectorSeed,
            RoomId = roomId,
            ChangeType = WorldStateChangeType.TerrainDestroyed,
            TargetId = targetId,
            ChangeData = JsonSerializer.Serialize(data),
            Timestamp = DateTime.UtcNow,
            TurnNumber = 42,
            SchemaVersion = 1
        };
    }

    private WorldStateChange CreateHazardDestroyedChange(
        int saveId, string sectorSeed, string roomId, string targetId)
    {
        var data = new HazardDestroyedData
        {
            HazardType = "SteamVent",
            CausedSecondaryEffect = false
        };

        return new WorldStateChange
        {
            SaveId = saveId,
            SectorSeed = sectorSeed,
            RoomId = roomId,
            ChangeType = WorldStateChangeType.HazardDestroyed,
            TargetId = targetId,
            ChangeData = JsonSerializer.Serialize(data),
            Timestamp = DateTime.UtcNow,
            TurnNumber = 43,
            SchemaVersion = 1
        };
    }

    private WorldStateChange CreateEnemyDefeatedChange(
        int saveId, string sectorSeed, string roomId, string targetId)
    {
        var data = new EnemyDefeatedData
        {
            EnemyType = "CorruptedServitor",
            EnemyName = targetId,
            DroppedLoot = false
        };

        return new WorldStateChange
        {
            SaveId = saveId,
            SectorSeed = sectorSeed,
            RoomId = roomId,
            ChangeType = WorldStateChangeType.EnemyDefeated,
            TargetId = targetId,
            ChangeData = JsonSerializer.Serialize(data),
            Timestamp = DateTime.UtcNow,
            TurnNumber = 44,
            SchemaVersion = 1
        };
    }
}
