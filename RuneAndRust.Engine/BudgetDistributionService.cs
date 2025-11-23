using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.39.3: Budget distribution service
/// Distributes global sector budget across rooms based on density classification
/// Priority: Boss rooms → Heavy rooms → Medium rooms → Light rooms → Empty rooms
/// </summary>
public class BudgetDistributionService
{
    private static readonly ILogger _log = Log.ForContext<BudgetDistributionService>();

    /// <summary>
    /// Distributes global budget across rooms based on density classification
    /// </summary>
    /// <param name="budget">Global budget to distribute</param>
    /// <param name="densityMap">Room density classifications</param>
    /// <param name="rng">Random number generator for variance</param>
    /// <returns>Complete population plan with room allocations</returns>
    public SectorPopulationPlan DistributeBudget(
        GlobalBudget budget,
        Dictionary<Room, RoomDensity> densityMap,
        Random rng)
    {
        if (densityMap == null || densityMap.Count == 0)
        {
            _log.Warning("No rooms in density map, returning empty plan");
            return new SectorPopulationPlan();
        }

        var plan = new SectorPopulationPlan();

        _log.Information(
            "Starting budget distribution: Total budget = {Enemies} enemies, {Hazards} hazards, {Loot} loot",
            budget.TotalEnemyBudget, budget.TotalHazardBudget, budget.TotalLootBudget);

        // Step 1: Allocate to Boss rooms first (20-30% of budget)
        AllocateToBossRooms(budget, densityMap, plan);

        // Step 2: Allocate to Heavy rooms (15-20% of remaining budget)
        AllocateToHeavyRooms(budget, densityMap, plan);

        // Step 3: Allocate to Medium rooms (30-40% of remaining budget)
        AllocateToMediumRooms(budget, densityMap, plan);

        // Step 4: Allocate to Light rooms (remaining budget)
        AllocateToLightRooms(budget, densityMap, plan);

        // Step 5: Empty rooms get loot, not threats
        AllocateLootToEmptyRooms(budget, densityMap, plan);

        _log.Information(
            "Budget distribution complete: Allocated {Enemies}/{TotalEnemies} enemies, " +
            "{Hazards}/{TotalHazards} hazards, {Loot}/{TotalLoot} loot across {Rooms} rooms",
            plan.TotalEnemiesAllocated, budget.TotalEnemyBudget,
            plan.TotalHazardsAllocated, budget.TotalHazardBudget,
            plan.TotalLootAllocated, budget.TotalLootBudget,
            plan.RoomAllocations.Count);

        return plan;
    }

    /// <summary>
    /// Allocates 20-30% of budget to boss rooms
    /// </summary>
    private void AllocateToBossRooms(
        GlobalBudget budget,
        Dictionary<Room, RoomDensity> densityMap,
        SectorPopulationPlan plan)
    {
        var bossRooms = densityMap
            .Where(kvp => kvp.Value == RoomDensity.Boss)
            .Select(kvp => kvp.Key)
            .ToList();

        if (bossRooms.Count == 0)
        {
            _log.Debug("No boss rooms to allocate");
            return;
        }

        // Boss rooms get 25% of total budget
        var bossEnemyBudget = (int)(budget.TotalEnemyBudget * 0.25f);
        var bossHazardBudget = (int)(budget.TotalHazardBudget * 0.25f);

        foreach (var room in bossRooms)
        {
            var enemyAllocation = bossEnemyBudget / bossRooms.Count;
            var hazardAllocation = bossHazardBudget / bossRooms.Count;

            plan.RoomAllocations[room.RoomId] = new RoomAllocation
            {
                RoomId = room.RoomId,
                AllocatedEnemies = enemyAllocation,
                AllocatedHazards = hazardAllocation,
                AllocatedLoot = 2, // Boss rooms always have good loot
                Density = RoomDensity.Boss
            };

            budget.EnemiesSpawned += enemyAllocation;
            budget.HazardsSpawned += hazardAllocation;
            budget.LootSpawned += 2;

            _log.Debug(
                "Boss room {RoomId}: Allocated {Enemies} enemies, {Hazards} hazards",
                room.RoomId, enemyAllocation, hazardAllocation);
        }
    }

    /// <summary>
    /// Allocates to Heavy rooms (5-7 threats each)
    /// </summary>
    private void AllocateToHeavyRooms(
        GlobalBudget budget,
        Dictionary<Room, RoomDensity> densityMap,
        SectorPopulationPlan plan)
    {
        var heavyRooms = densityMap
            .Where(kvp => kvp.Value == RoomDensity.Heavy)
            .Select(kvp => kvp.Key)
            .ToList();

        if (heavyRooms.Count == 0)
        {
            _log.Debug("No heavy rooms to allocate");
            return;
        }

        foreach (var room in heavyRooms)
        {
            // Heavy rooms: 5-7 threats (target 5-6 enemies, 1-2 hazards)
            var enemyAllocation = Math.Min(5, budget.RemainingEnemyBudget);
            var hazardAllocation = Math.Min(2, budget.RemainingHazardBudget);

            plan.RoomAllocations[room.RoomId] = new RoomAllocation
            {
                RoomId = room.RoomId,
                AllocatedEnemies = enemyAllocation,
                AllocatedHazards = hazardAllocation,
                AllocatedLoot = 1,
                Density = RoomDensity.Heavy
            };

            budget.EnemiesSpawned += enemyAllocation;
            budget.HazardsSpawned += hazardAllocation;
            budget.LootSpawned += 1;

            _log.Debug(
                "Heavy room {RoomId}: Allocated {Enemies} enemies, {Hazards} hazards",
                room.RoomId, enemyAllocation, hazardAllocation);
        }
    }

    /// <summary>
    /// Allocates to Medium rooms (3-4 threats each)
    /// </summary>
    private void AllocateToMediumRooms(
        GlobalBudget budget,
        Dictionary<Room, RoomDensity> densityMap,
        SectorPopulationPlan plan)
    {
        var mediumRooms = densityMap
            .Where(kvp => kvp.Value == RoomDensity.Medium)
            .Select(kvp => kvp.Key)
            .ToList();

        if (mediumRooms.Count == 0)
        {
            _log.Debug("No medium rooms to allocate");
            return;
        }

        foreach (var room in mediumRooms)
        {
            // Medium rooms: 3-4 threats (target 3 enemies, 1 hazard)
            var enemyAllocation = Math.Min(3, budget.RemainingEnemyBudget);
            var hazardAllocation = Math.Min(1, budget.RemainingHazardBudget);

            plan.RoomAllocations[room.RoomId] = new RoomAllocation
            {
                RoomId = room.RoomId,
                AllocatedEnemies = enemyAllocation,
                AllocatedHazards = hazardAllocation,
                AllocatedLoot = 1,
                Density = RoomDensity.Medium
            };

            budget.EnemiesSpawned += enemyAllocation;
            budget.HazardsSpawned += hazardAllocation;
            budget.LootSpawned += 1;

            _log.Debug(
                "Medium room {RoomId}: Allocated {Enemies} enemies, {Hazards} hazards",
                room.RoomId, enemyAllocation, hazardAllocation);
        }
    }

    /// <summary>
    /// Allocates to Light rooms (1-2 threats each)
    /// </summary>
    private void AllocateToLightRooms(
        GlobalBudget budget,
        Dictionary<Room, RoomDensity> densityMap,
        SectorPopulationPlan plan)
    {
        var lightRooms = densityMap
            .Where(kvp => kvp.Value == RoomDensity.Light)
            .Select(kvp => kvp.Key)
            .ToList();

        if (lightRooms.Count == 0)
        {
            _log.Debug("No light rooms to allocate");
            return;
        }

        foreach (var room in lightRooms)
        {
            // Light rooms: 1-2 threats (typically 1-2 enemies OR 1 enemy + 1 hazard)
            var enemyAllocation = Math.Min(1, budget.RemainingEnemyBudget);
            var hazardAllocation = 0;

            // Randomly decide if we want a hazard instead of second enemy
            if (budget.RemainingHazardBudget > 0 && budget.RemainingEnemyBudget > 0)
            {
                hazardAllocation = 1;
                budget.HazardsSpawned += 1;
            }
            else if (budget.RemainingEnemyBudget > 1)
            {
                enemyAllocation = 2;
            }

            plan.RoomAllocations[room.RoomId] = new RoomAllocation
            {
                RoomId = room.RoomId,
                AllocatedEnemies = enemyAllocation,
                AllocatedHazards = hazardAllocation,
                AllocatedLoot = 0, // Light rooms don't always have loot
                Density = RoomDensity.Light
            };

            budget.EnemiesSpawned += enemyAllocation;

            _log.Debug(
                "Light room {RoomId}: Allocated {Enemies} enemies, {Hazards} hazards",
                room.RoomId, enemyAllocation, hazardAllocation);
        }
    }

    /// <summary>
    /// Allocates loot to Empty rooms (no threats)
    /// </summary>
    private void AllocateLootToEmptyRooms(
        GlobalBudget budget,
        Dictionary<Room, RoomDensity> densityMap,
        SectorPopulationPlan plan)
    {
        var emptyRooms = densityMap
            .Where(kvp => kvp.Value == RoomDensity.Empty)
            .Select(kvp => kvp.Key)
            .ToList();

        if (emptyRooms.Count == 0)
        {
            _log.Debug("No empty rooms to allocate");
            return;
        }

        foreach (var room in emptyRooms)
        {
            // Empty rooms: 0 threats, but 1-2 loot nodes
            var lootAllocation = Math.Min(1, budget.RemainingLootBudget);

            plan.RoomAllocations[room.RoomId] = new RoomAllocation
            {
                RoomId = room.RoomId,
                AllocatedEnemies = 0,
                AllocatedHazards = 0,
                AllocatedLoot = lootAllocation,
                Density = RoomDensity.Empty
            };

            budget.LootSpawned += lootAllocation;

            _log.Debug(
                "Empty room {RoomId}: Allocated {Loot} loot nodes (breather room)",
                room.RoomId, lootAllocation);
        }
    }
}
