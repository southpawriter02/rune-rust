using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.11 Dormant Process (Enemy) Spawner
/// Implements weighted enemy placement with spawn budget system
/// </summary>
public class DormantProcessSpawner
{
    private static readonly ILogger _log = Log.ForContext<DormantProcessSpawner>();
    private readonly EnemyFactory _enemyFactory;

    public DormantProcessSpawner(EnemyFactory enemyFactory)
    {
        _enemyFactory = enemyFactory;
    }

    /// <summary>
    /// Populates a room with enemies based on biome elements and spawn budget
    /// </summary>
    public void PopulateRoom(Room room, BiomeDefinition biome, Random rng)
    {
        // Skip if handcrafted (Quest Anchor)
        if (room.IsHandcrafted)
        {
            _log.Debug("Skipping enemy population for handcrafted room {RoomId}", room.RoomId);
            return;
        }

        // Step 1: Handle Boss Arena rooms (special case)
        if (room.IsBossRoom)
        {
            SpawnBoss(room, biome, rng);
            return;
        }

        // Step 2: Calculate spawn budget
        int spawnBudget = CalculateSpawnBudget(room);
        if (spawnBudget <= 0)
        {
            _log.Debug("Room {RoomId} has 0 spawn budget (Entry Hall or other)", room.RoomId);
            return;
        }

        _log.Debug("Room {RoomId}: Spawn budget = {Budget}", room.RoomId, spawnBudget);

        // Step 3: Get eligible enemy elements from biome
        if (biome.Elements == null)
        {
            _log.Warning("Biome {BiomeName} has no Elements table, skipping enemy population", biome.Name);
            return;
        }

        var availableEnemies = biome.Elements.GetEligibleElements(
            BiomeElementType.DormantProcess, room, rng);

        if (availableEnemies.Count == 0)
        {
            _log.Debug("No eligible enemies for room {RoomId}", room.RoomId);
            return;
        }

        // Step 4: Weighted random selection until budget exhausted
        int spawnedCount = 0;
        while (spawnBudget > 0 && availableEnemies.Count > 0)
        {
            var selected = biome.Elements.WeightedRandomSelection(availableEnemies, rng);
            if (selected == null) break;

            // Check if we can afford this enemy
            if (selected.SpawnCost > spawnBudget)
            {
                // Remove from available pool if too expensive
                availableEnemies = availableEnemies.Where(e => e.SpawnCost <= spawnBudget).ToList();
                continue;
            }

            // Create enemy
            var enemy = CreateEnemyFromElement(selected, room, rng);
            if (enemy != null)
            {
                room.Enemies.Add(enemy);
                spawnBudget -= selected.SpawnCost;
                spawnedCount++;

                _log.Debug("Spawned {EnemyName} (cost {Cost}) in room {RoomId}, budget remaining: {Budget}",
                    enemy.Name, selected.SpawnCost, room.RoomId, spawnBudget);
            }
        }

        _log.Information("Room {RoomId}: Spawned {Count} enemies with {ElementTypes} types",
            room.RoomId, spawnedCount, room.Enemies.Select(e => e.Type).Distinct().Count());
    }

    /// <summary>
    /// Calculates spawn budget based on room size, archetype, and depth
    /// v0.39.3: Now uses allocated budget from density system when available
    /// </summary>
    private int CalculateSpawnBudget(Room room)
    {
        // v0.39.3: Use allocated budget if available
        if (room.AllocatedEnemyBudget > 0)
        {
            _log.Debug("Using v0.39.3 allocated enemy budget: {Budget}", room.AllocatedEnemyBudget);
            return room.AllocatedEnemyBudget;
        }

        // Fallback to v0.11 budget calculation for backward compatibility
        _log.Debug("Using v0.11 fallback budget calculation");

        // Base budget by room type (heuristic based on node type)
        int baseBudget = room.GeneratedNodeType switch
        {
            NodeType.Start => 1,      // Entry Hall - safer
            NodeType.Main => 5,        // Main path rooms - medium
            NodeType.Branch => 4,      // Branch rooms - slightly lower
            NodeType.Secret => 2,      // Secret rooms - reward, not gauntlet
            NodeType.Boss => 0,        // Handled separately
            _ => 5
        };

        // Modify by room properties
        if (room.IsStartRoom)
        {
            baseBudget = Math.Max(1, baseBudget - 2); // Entry halls are safer
        }

        return baseBudget;
    }

    /// <summary>
    /// Spawns a boss in a Boss Arena room
    /// </summary>
    private void SpawnBoss(Room room, BiomeDefinition biome, Random rng)
    {
        _log.Debug("Spawning boss for room {RoomId}", room.RoomId);

        if (biome.Elements == null)
        {
            _log.Warning("Biome {BiomeName} has no Elements table, cannot spawn boss", biome.Name);
            return;
        }

        // Get boss-specific elements
        var bossElements = biome.Elements.GetEligibleElements(
            BiomeElementType.DormantProcess, room, rng)
            .Where(e => e.SpawnRules?.RequiredArchetype == "BossArena" ||
                       e.ElementName.Contains("Overseer") ||
                       e.ElementName.Contains("Boss"))
            .ToList();

        if (bossElements.Count == 0)
        {
            _log.Warning("No boss elements found for biome {BiomeName}, using fallback", biome.Name);
            // Fallback: spawn a champion-tier enemy
            bossElements = biome.Elements.GetEligibleElements(
                BiomeElementType.DormantProcess, room, rng)
                .Where(e => e.SpawnCost >= 3)
                .ToList();
        }

        if (bossElements.Count > 0)
        {
            var bossElement = biome.Elements.WeightedRandomSelection(bossElements, rng);
            if (bossElement != null)
            {
                var boss = CreateEnemyFromElement(bossElement, room, rng, isBoss: true);
                if (boss != null)
                {
                    room.Enemies.Add(boss);
                    _log.Information("Spawned boss {BossName} in room {RoomId}", boss.Name, room.RoomId);
                }
            }
        }
    }

    /// <summary>
    /// Creates an enemy from a BiomeElement
    /// </summary>
    private Enemy? CreateEnemyFromElement(BiomeElement element, Room room, Random rng, bool isBoss = false)
    {
        // Map AssociatedDataId to EnemyType
        var enemyType = MapElementToEnemyType(element.AssociatedDataId);
        if (enemyType == null)
        {
            _log.Warning("Could not map element {ElementName} to enemy type", element.ElementName);
            return null;
        }

        // Use EnemyFactory to create the enemy
        var enemy = EnemyFactory.CreateEnemy(enemyType.Value);
        enemy.IsBoss = isBoss;

        // Boss stat multipliers
        if (isBoss)
        {
            enemy.MaxHP = (int)(enemy.MaxHP * 2.5f);
            enemy.HP = enemy.MaxHP;
            enemy.BaseDamageDice = (int)(enemy.BaseDamageDice * 1.5f);
        }

        return enemy;
    }

    /// <summary>
    /// Maps BiomeElement AssociatedDataId to EnemyType enum
    /// TODO: This mapping should ideally be data-driven (e.g., in a config file)
    /// </summary>
    private EnemyType? MapElementToEnemyType(string? dataId)
    {
        return dataId switch
        {
            "rust_horror" => EnemyType.CorruptedServitor, // Placeholder - need new enemy types
            "rusted_servitor" => EnemyType.CorruptedServitor,
            "corroded_drone" => EnemyType.BlightDrone,
            "blight_rat_swarm" => EnemyType.ScrapHound,
            "construction_hauler" => EnemyType.WarFrame,
            "husk_enforcer" => EnemyType.WarFrame,
            "servitor_overseer" => EnemyType.RuinWarden, // Boss
            _ => null
        };
    }
}
