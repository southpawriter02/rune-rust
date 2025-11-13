using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.13: Service for handling environmental destruction and world state recording
/// </summary>
public class DestructionService
{
    private static readonly ILogger _log = Log.ForContext<DestructionService>();
    private readonly WorldStateRepository _worldStateRepository;
    private readonly DiceService _diceService;

    // Track current HP for destructible elements (keyed by ElementId)
    private readonly Dictionary<string, int> _elementCurrentHP = new();

    public DestructionService(WorldStateRepository worldStateRepository, DiceService diceService)
    {
        _worldStateRepository = worldStateRepository;
        _diceService = diceService;
    }

    /// <summary>
    /// Initialize HP tracking for a room's destructible elements
    /// </summary>
    public void InitializeRoomElements(Room room)
    {
        _log.Debug("Initializing destructible elements for room: {RoomId}", room.RoomId);

        // Initialize static terrain
        foreach (var terrain in room.StaticTerrain.Where(t => t.IsDestructible && t.HP > 0))
        {
            if (!_elementCurrentHP.ContainsKey(terrain.TerrainId))
            {
                _elementCurrentHP[terrain.TerrainId] = terrain.HP;
                _log.Debug("Initialized terrain HP: {TerrainId} = {HP}", terrain.TerrainId, terrain.HP);
            }
        }

        // Initialize dynamic hazards (if they have HP in future)
        // Currently hazards use IsActive flag rather than HP
    }

    /// <summary>
    /// Apply world state changes to a room after generation
    /// This restores previously destroyed elements
    /// </summary>
    public void ApplyWorldStateChanges(Room room, List<WorldStateChange> changes, int saveId)
    {
        if (changes == null || changes.Count == 0)
        {
            _log.Debug("No world state changes to apply for room: {RoomId}", room.RoomId);
            return;
        }

        _log.Information("Applying {ChangeCount} world state changes to room: {RoomId}",
            changes.Count, room.RoomId);

        foreach (var change in changes.OrderBy(c => c.Timestamp))
        {
            ApplyWorldStateChange(room, change);
        }
    }

    private void ApplyWorldStateChange(Room room, WorldStateChange change)
    {
        switch (change.ChangeType)
        {
            case WorldStateChangeType.TerrainDestroyed:
                ApplyTerrainDestroyed(room, change);
                break;

            case WorldStateChangeType.HazardDestroyed:
                ApplyHazardDestroyed(room, change);
                break;

            case WorldStateChangeType.EnemyDefeated:
                ApplyEnemyDefeated(room, change);
                break;

            case WorldStateChangeType.LootCollected:
                ApplyLootCollected(room, change);
                break;

            default:
                _log.Warning("Unknown change type: {ChangeType}", change.ChangeType);
                break;
        }
    }

    private void ApplyTerrainDestroyed(Room room, WorldStateChange change)
    {
        try
        {
            var data = JsonSerializer.Deserialize<TerrainDestroyedData>(change.ChangeData);
            if (data == null)
            {
                _log.Warning("Failed to deserialize TerrainDestroyedData for change: {ChangeId}", change.Id);
                return;
            }

            // Remove the destroyed terrain element
            var element = room.StaticTerrain.FirstOrDefault(t => t.TerrainId == change.TargetId);
            if (element != null)
            {
                room.StaticTerrain.Remove(element);
                _log.Debug("Removed destroyed terrain: {ElementId}", change.TargetId);

                // Add rubble if specified
                if (data.SpawnRubble)
                {
                    room.StaticTerrain.Add(new StaticTerrain
                    {
                        TerrainId = $"rubble_from_{change.TargetId}",
                        Name = "Rubble Pile",
                        Description = $"Debris from the destroyed {element.Name.ToLower()}.",
                        Type = StaticTerrainType.RubblePile,
                        CoverProvided = CoverType.Partial,
                        AccuracyModifier = -2,
                        IsDifficultTerrain = true,
                        MovementCostModifier = 2,
                        IsDestructible = true,
                        HP = 15
                    });
                    _log.Debug("Added rubble pile from destroyed terrain");
                }
            }
            else
            {
                _log.Warning("Terrain element not found for destruction change: {ElementId}", change.TargetId);
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to apply terrain destroyed change: {ChangeId}", change.Id);
        }
    }

    private void ApplyHazardDestroyed(Room room, WorldStateChange change)
    {
        try
        {
            var data = JsonSerializer.Deserialize<HazardDestroyedData>(change.ChangeData);
            if (data == null)
            {
                _log.Warning("Failed to deserialize HazardDestroyedData for change: {ChangeId}", change.Id);
                return;
            }

            // Remove the destroyed hazard
            var hazard = room.DynamicHazards.FirstOrDefault(h => h.HazardId == change.TargetId);
            if (hazard != null)
            {
                room.DynamicHazards.Remove(hazard);
                _log.Debug("Removed destroyed hazard: {HazardId}", change.TargetId);
            }
            else
            {
                _log.Warning("Hazard not found for destruction change: {HazardId}", change.TargetId);
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to apply hazard destroyed change: {ChangeId}", change.Id);
        }
    }

    private void ApplyEnemyDefeated(Room room, WorldStateChange change)
    {
        try
        {
            var data = JsonSerializer.Deserialize<EnemyDefeatedData>(change.ChangeData);
            if (data == null)
            {
                _log.Warning("Failed to deserialize EnemyDefeatedData for change: {ChangeId}", change.Id);
                return;
            }

            // Remove the defeated enemy
            var enemy = room.Enemies.FirstOrDefault(e => e.Name == change.TargetId);
            if (enemy != null)
            {
                room.Enemies.Remove(enemy);
                _log.Debug("Removed defeated enemy: {EnemyName}", change.TargetId);
            }
            else
            {
                _log.Debug("Enemy not found for defeat change (may have been removed already): {EnemyName}",
                    change.TargetId);
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to apply enemy defeated change: {ChangeId}", change.Id);
        }
    }

    private void ApplyLootCollected(Room room, WorldStateChange change)
    {
        try
        {
            var data = JsonSerializer.Deserialize<LootCollectedData>(change.ChangeData);
            if (data == null)
            {
                _log.Warning("Failed to deserialize LootCollectedData for change: {ChangeId}", change.Id);
                return;
            }

            // Remove the collected loot node
            var lootNode = room.LootNodes.FirstOrDefault(l => l.NodeId == change.TargetId);
            if (lootNode != null)
            {
                room.LootNodes.Remove(lootNode);
                _log.Debug("Removed collected loot node: {NodeId}", change.TargetId);
            }
            else
            {
                _log.Debug("Loot node not found for collection change: {NodeId}", change.TargetId);
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to apply loot collected change: {ChangeId}", change.Id);
        }
    }

    /// <summary>
    /// Attempt to destroy static terrain
    /// </summary>
    public DestructionResult AttemptDestroyTerrain(
        Room room,
        StaticTerrain terrain,
        PlayerCharacter player,
        int saveId,
        int turnNumber)
    {
        if (!terrain.IsDestructible || terrain.HP <= 0)
        {
            return new DestructionResult
            {
                Success = false,
                Message = $"The {terrain.Name} is too sturdy to destroy."
            };
        }

        // Calculate damage based on player MIGHT
        int damage = CalculateDestructionDamage(player);

        // Get or initialize current HP
        if (!_elementCurrentHP.ContainsKey(terrain.TerrainId))
        {
            _elementCurrentHP[terrain.TerrainId] = terrain.HP;
        }

        int currentHP = _elementCurrentHP[terrain.TerrainId];
        int newHP = Math.Max(0, currentHP - damage);
        _elementCurrentHP[terrain.TerrainId] = newHP;

        bool wasDestroyed = newHP <= 0;

        _log.Information("Terrain damage: Element={TerrainId}, Damage={Damage}, HP={NewHP}/{MaxHP}, Destroyed={Destroyed}",
            terrain.TerrainId, damage, newHP, terrain.HP, wasDestroyed);

        if (wasDestroyed)
        {
            // Record world state change
            var changeData = new TerrainDestroyedData
            {
                ElementType = terrain.Type.ToString(),
                SpawnRubble = terrain.Type.ShouldSpawnRubble(),
                DestroyedBy = player.Name
            };

            var change = new WorldStateChange
            {
                SaveId = saveId,
                SectorSeed = GetSectorSeedFromRoomId(room.RoomId),
                RoomId = room.RoomId,
                ChangeType = WorldStateChangeType.TerrainDestroyed,
                TargetId = terrain.TerrainId,
                ChangeData = JsonSerializer.Serialize(changeData),
                Timestamp = DateTime.UtcNow,
                TurnNumber = turnNumber
            };

            _worldStateRepository.RecordChange(change);

            return new DestructionResult
            {
                Success = true,
                WasDestroyed = true,
                Message = terrain.Type.GetDestructionNarrative(),
                SpawnRubble = changeData.SpawnRubble
            };
        }
        else
        {
            return new DestructionResult
            {
                Success = true,
                WasDestroyed = false,
                Message = $"You strike the {terrain.Name}. It shows signs of damage but holds together. " +
                         $"({newHP}/{terrain.HP} HP remaining)"
            };
        }
    }

    /// <summary>
    /// Attempt to destroy a dynamic hazard
    /// </summary>
    public DestructionResult AttemptDestroyHazard(
        Room room,
        DynamicHazard hazard,
        PlayerCharacter player,
        int saveId,
        int turnNumber)
    {
        if (!hazard.Type.IsDestructibleHazard())
        {
            return new DestructionResult
            {
                Success = false,
                Message = $"The {hazard.Name} cannot be destroyed."
            };
        }

        // Hazards are destroyed in one action (simplified)
        var changeData = new HazardDestroyedData
        {
            HazardType = hazard.Type.ToString(),
            CausedSecondaryEffect = hazard.Type.CausesSecondaryEffect()
        };

        var change = new WorldStateChange
        {
            SaveId = saveId,
            SectorSeed = GetSectorSeedFromRoomId(room.RoomId),
            RoomId = room.RoomId,
            ChangeType = WorldStateChangeType.HazardDestroyed,
            TargetId = hazard.HazardId,
            ChangeData = JsonSerializer.Serialize(changeData),
            Timestamp = DateTime.UtcNow,
            TurnNumber = turnNumber
        };

        _worldStateRepository.RecordChange(change);

        _log.Information("Hazard destroyed: HazardId={HazardId}, Type={Type}",
            hazard.HazardId, hazard.Type);

        return new DestructionResult
        {
            Success = true,
            WasDestroyed = true,
            Message = hazard.Type.GetDestructionNarrative(),
            CausedSecondaryEffect = changeData.CausedSecondaryEffect
        };
    }

    /// <summary>
    /// Record enemy defeat to world state
    /// </summary>
    public void RecordEnemyDefeat(
        Room room,
        Enemy enemy,
        int saveId,
        int turnNumber,
        bool droppedLoot = false,
        string? lootDropId = null)
    {
        var changeData = new EnemyDefeatedData
        {
            EnemyType = enemy.Type.ToString(),
            EnemyName = enemy.Name,
            DroppedLoot = droppedLoot,
            LootDropId = lootDropId
        };

        var change = new WorldStateChange
        {
            SaveId = saveId,
            SectorSeed = GetSectorSeedFromRoomId(room.RoomId),
            RoomId = room.RoomId,
            ChangeType = WorldStateChangeType.EnemyDefeated,
            TargetId = enemy.Name, // Use name as unique identifier
            ChangeData = JsonSerializer.Serialize(changeData),
            Timestamp = DateTime.UtcNow,
            TurnNumber = turnNumber
        };

        _worldStateRepository.RecordChange(change);

        _log.Information("Enemy defeat recorded: Enemy={EnemyName}, Type={Type}, Room={RoomId}",
            enemy.Name, enemy.Type, room.RoomId);
    }

    /// <summary>
    /// Calculate destruction damage based on player MIGHT
    /// </summary>
    private int CalculateDestructionDamage(PlayerCharacter player)
    {
        // Base damage from MIGHT attribute
        int baseDamage = player.Attributes.Might * 2;

        // Weapon bonus (if weapon has destruction properties)
        int weaponBonus = player.EquippedWeapon?.DamageDice ?? 0;

        // Random variance
        int variance = _diceService.RollD6() - 3; // -2 to +3

        int totalDamage = Math.Max(1, baseDamage + weaponBonus + variance);

        _log.Debug("Calculated destruction damage: Base={Base}, Weapon={Weapon}, Variance={Variance}, Total={Total}",
            baseDamage, weaponBonus, variance, totalDamage);

        return totalDamage;
    }

    /// <summary>
    /// Extract sector seed from room ID
    /// Room IDs are in format: room_d{dungeonId}_n{nodeId} or room_sector{seed}_node{id}
    /// </summary>
    private string GetSectorSeedFromRoomId(string roomId)
    {
        // For procedural rooms: room_d1_n5 -> extract dungeon ID
        if (roomId.StartsWith("room_d"))
        {
            var parts = roomId.Split('_');
            if (parts.Length >= 2)
            {
                return parts[1]; // "d1"
            }
        }

        // For sector-based rooms: room_sector12345_node3 -> extract seed
        if (roomId.Contains("sector"))
        {
            var parts = roomId.Split('_');
            foreach (var part in parts)
            {
                if (part.StartsWith("sector"))
                {
                    return part.Replace("sector", "");
                }
            }
        }

        // Fallback: use room ID as seed
        return roomId;
    }
}

/// <summary>
/// v0.13: Result of a destruction attempt
/// </summary>
public class DestructionResult
{
    public bool Success { get; set; }
    public bool WasDestroyed { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool SpawnRubble { get; set; }
    public bool CausedSecondaryEffect { get; set; }
}
