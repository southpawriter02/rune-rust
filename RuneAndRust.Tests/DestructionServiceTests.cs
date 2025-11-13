using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Persistence;
using System.Text.Json;
using Population = RuneAndRust.Core.Population;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.13: Unit tests for DestructionService
/// </summary>
[TestClass]
public class DestructionServiceTests
{
    private WorldStateRepository? _repository;
    private DestructionService? _destructionService;
    private DiceService? _diceService;
    private string? _testDbPath;

    [TestInitialize]
    public void Setup()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_runeandrust_{Guid.NewGuid()}.db");
        var testDir = Path.GetDirectoryName(_testDbPath)!;

        _repository = new WorldStateRepository(testDir);
        _diceService = new DiceService();
        _destructionService = new DestructionService(_repository, _diceService);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (_testDbPath != null && File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    [TestMethod]
    public void ApplyWorldStateChanges_TerrainDestroyed_RemovesTerrainAndAddsRubble()
    {
        // Arrange
        var room = CreateTestRoom();
        var pillar = new StaticTerrain
        {
            TerrainId = "pillar_1",
            Name = "Collapsed Pillar",
            Type = StaticTerrainType.CollapsedPillar,
            IsDestructible = true,
            HP = 30
        };
        room.StaticTerrain.Add(pillar);

        var change = CreateTerrainDestroyedChange(
            saveId: 1,
            sectorSeed: "d1",
            roomId: room.RoomId,
            targetId: "pillar_1",
            spawnRubble: true);

        var changes = new List<WorldStateChange> { change };

        // Act
        _destructionService!.ApplyWorldStateChanges(room, changes, saveId: 1);

        // Assert
        Assert.IsFalse(room.StaticTerrain.Any(t => t.TerrainId == "pillar_1"),
            "Original pillar should be removed");
        Assert.IsTrue(room.StaticTerrain.Any(t => t.Type == StaticTerrainType.RubblePile),
            "Rubble pile should be added");
    }

    [TestMethod]
    public void ApplyWorldStateChanges_TerrainDestroyed_NoRubble_RemovesTerrainOnly()
    {
        // Arrange
        var room = CreateTestRoom();
        var grating = new StaticTerrain
        {
            TerrainId = "grating_1",
            Name = "Corroded Grating",
            Type = StaticTerrainType.CorrodedGrating,
            IsDestructible = true,
            HP = 10
        };
        room.StaticTerrain.Add(grating);

        var change = CreateTerrainDestroyedChange(
            saveId: 1,
            sectorSeed: "d1",
            roomId: room.RoomId,
            targetId: "grating_1",
            spawnRubble: false);

        var changes = new List<WorldStateChange> { change };

        // Act
        _destructionService!.ApplyWorldStateChanges(room, changes, saveId: 1);

        // Assert
        Assert.IsFalse(room.StaticTerrain.Any(t => t.TerrainId == "grating_1"));
        Assert.IsFalse(room.StaticTerrain.Any(t => t.Type == StaticTerrainType.RubblePile),
            "No rubble should be added for grating");
    }

    [TestMethod]
    public void ApplyWorldStateChanges_HazardDestroyed_RemovesHazard()
    {
        // Arrange
        var room = CreateTestRoom();
        var hazard = new DynamicHazard
        {
            HazardId = "steam_vent_1",
            Name = "Steam Vent",
            Type = DynamicHazardType.SteamVent,
            IsActive = true
        };
        room.DynamicHazards.Add(hazard);

        var change = CreateHazardDestroyedChange(
            saveId: 1,
            sectorSeed: "d1",
            roomId: room.RoomId,
            targetId: "steam_vent_1");

        var changes = new List<WorldStateChange> { change };

        // Act
        _destructionService!.ApplyWorldStateChanges(room, changes, saveId: 1);

        // Assert
        Assert.IsFalse(room.DynamicHazards.Any(h => h.HazardId == "steam_vent_1"),
            "Hazard should be removed");
    }

    [TestMethod]
    public void ApplyWorldStateChanges_EnemyDefeated_RemovesEnemy()
    {
        // Arrange
        var room = CreateTestRoom();
        var enemy = new Enemy
        {
            Name = "Rusted Servitor",
            Type = EnemyType.CorruptedServitor,
            HP = 10,
            MaxHP = 10
        };
        room.Enemies.Add(enemy);

        var change = CreateEnemyDefeatedChange(
            saveId: 1,
            sectorSeed: "d1",
            roomId: room.RoomId,
            enemyName: "Rusted Servitor");

        var changes = new List<WorldStateChange> { change };

        // Act
        _destructionService!.ApplyWorldStateChanges(room, changes, saveId: 1);

        // Assert
        Assert.IsFalse(room.Enemies.Any(e => e.Name == "Rusted Servitor"),
            "Enemy should be removed");
    }

    [TestMethod]
    public void ApplyWorldStateChanges_MultipleChanges_AppliesInOrder()
    {
        // Arrange
        var room = CreateTestRoom();

        var pillar = new StaticTerrain { TerrainId = "pillar_1", Name = "Pillar", IsDestructible = true, HP = 30 };
        var hazard = new DynamicHazard { HazardId = "steam_vent_1", Name = "Steam Vent", Type = DynamicHazardType.SteamVent };
        var enemy = new Enemy { Name = "Servitor", Type = EnemyType.CorruptedServitor, HP = 10, MaxHP = 10 };

        room.StaticTerrain.Add(pillar);
        room.DynamicHazards.Add(hazard);
        room.Enemies.Add(enemy);

        var changes = new List<WorldStateChange>
        {
            CreateTerrainDestroyedChange(1, "d1", room.RoomId, "pillar_1", true),
            CreateHazardDestroyedChange(1, "d1", room.RoomId, "steam_vent_1"),
            CreateEnemyDefeatedChange(1, "d1", room.RoomId, "Servitor")
        };

        // Set timestamps to ensure order
        changes[0].Timestamp = DateTime.UtcNow.AddMinutes(-2);
        changes[1].Timestamp = DateTime.UtcNow.AddMinutes(-1);
        changes[2].Timestamp = DateTime.UtcNow;

        // Act
        _destructionService!.ApplyWorldStateChanges(room, changes, saveId: 1);

        // Assert
        Assert.IsFalse(room.StaticTerrain.Any(t => t.TerrainId == "pillar_1"));
        Assert.IsTrue(room.StaticTerrain.Any(t => t.Type == StaticTerrainType.RubblePile));
        Assert.IsFalse(room.DynamicHazards.Any(h => h.HazardId == "steam_vent_1"));
        Assert.IsFalse(room.Enemies.Any(e => e.Name == "Servitor"));
    }

    [TestMethod]
    public void AttemptDestroyTerrain_DestructibleTerrain_Succeeds()
    {
        // Arrange
        var room = CreateTestRoom();
        var player = CreateTestPlayer(might: 5);
        var terrain = new StaticTerrain
        {
            TerrainId = "pillar_1",
            Name = "Collapsed Pillar",
            Type = StaticTerrainType.CollapsedPillar,
            IsDestructible = true,
            HP = 5 // Low HP for guaranteed destruction
        };

        // Act
        var result = _destructionService!.AttemptDestroyTerrain(
            room, terrain, player, saveId: 1, turnNumber: 1);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.WasDestroyed);
        Assert.IsFalse(string.IsNullOrEmpty(result.Message));
    }

    [TestMethod]
    public void AttemptDestroyTerrain_NonDestructibleTerrain_Fails()
    {
        // Arrange
        var room = CreateTestRoom();
        var player = CreateTestPlayer();
        var terrain = new StaticTerrain
        {
            TerrainId = "chasm_1",
            Name = "Chasm",
            Type = StaticTerrainType.Chasm,
            IsDestructible = false,
            HP = 0
        };

        // Act
        var result = _destructionService!.AttemptDestroyTerrain(
            room, terrain, player, saveId: 1, turnNumber: 1);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsFalse(result.WasDestroyed);
        Assert.IsTrue(result.Message.Contains("too sturdy"));
    }

    [TestMethod]
    public void AttemptDestroyHazard_DestructibleHazard_Succeeds()
    {
        // Arrange
        var room = CreateTestRoom();
        var player = CreateTestPlayer();
        var hazard = new DynamicHazard
        {
            HazardId = "steam_vent_1",
            Name = "Steam Vent",
            Type = DynamicHazardType.SteamVent,
            IsActive = true
        };

        // Act
        var result = _destructionService!.AttemptDestroyHazard(
            room, hazard, player, saveId: 1, turnNumber: 1);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.WasDestroyed);
        Assert.IsFalse(string.IsNullOrEmpty(result.Message));
    }

    [TestMethod]
    public void RecordEnemyDefeat_ValidEnemy_RecordsToDatabase()
    {
        // Arrange
        var room = CreateTestRoom();
        var enemy = new Enemy
        {
            Name = "Rusted Servitor",
            Type = EnemyType.CorruptedServitor,
            HP = 0,
            MaxHP = 10
        };

        // Act
        _destructionService!.RecordEnemyDefeat(
            room, enemy, saveId: 1, turnNumber: 1, droppedLoot: false);

        // Assert
        var changes = _repository!.GetChangesForRoom(1, "d1", room.RoomId);
        Assert.AreEqual(1, changes.Count);
        Assert.AreEqual(WorldStateChangeType.EnemyDefeated, changes[0].ChangeType);
        Assert.AreEqual("Rusted Servitor", changes[0].TargetId);
    }

    // Helper Methods

    private Room CreateTestRoom()
    {
        return new Room
        {
            RoomId = "room_d1_n5",
            Name = "Test Room",
            Description = "A test room",
            IsProcedurallyGenerated = true,
            StaticTerrain = new List<StaticTerrain>(),
            DynamicHazards = new List<DynamicHazard>(),
            Enemies = new List<Enemy>(),
            LootNodes = new List<LootNode>(),
            AmbientConditions = new List<AmbientCondition>(),
            Exits = new Dictionary<string, string>()
        };
    }

    private PlayerCharacter CreateTestPlayer(int might = 3)
    {
        return new PlayerCharacter
        {
            Name = "Test Player",
            Class = CharacterClass.IronBane,
            Attributes = new Attributes(might, 2, 2, 2, 3),
            HP = 20,
            MaxHP = 20,
            Stamina = 10,
            MaxStamina = 10
        };
    }

    private WorldStateChange CreateTerrainDestroyedChange(
        int saveId, string sectorSeed, string roomId, string targetId, bool spawnRubble)
    {
        var data = new TerrainDestroyedData
        {
            ElementType = "CollapsedPillar",
            SpawnRubble = spawnRubble,
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
            TurnNumber = 1
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
            TurnNumber = 1
        };
    }

    private WorldStateChange CreateEnemyDefeatedChange(
        int saveId, string sectorSeed, string roomId, string enemyName)
    {
        var data = new EnemyDefeatedData
        {
            EnemyType = "CorruptedServitor",
            EnemyName = enemyName,
            DroppedLoot = false
        };

        return new WorldStateChange
        {
            SaveId = saveId,
            SectorSeed = sectorSeed,
            RoomId = roomId,
            ChangeType = WorldStateChangeType.EnemyDefeated,
            TargetId = enemyName,
            ChangeData = JsonSerializer.Serialize(data),
            Timestamp = DateTime.UtcNow,
            TurnNumber = 1
        };
    }
}
