using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.39.3: Tests for BudgetDistributionService
/// Validates budget distribution logic across density types
/// </summary>
[TestClass]
public class BudgetDistributionServiceTests
{
    private BudgetDistributionService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _service = new BudgetDistributionService();
    }

    #region Budget Distribution Tests

    [TestMethod]
    public void DistributeBudget_BossRoomGets25PercentOfBudget()
    {
        // Arrange
        var budget = new GlobalBudget
        {
            TotalEnemyBudget = 20,
            TotalHazardBudget = 10
        };

        var room1 = new Room { RoomId = "boss", Archetype = RoomArchetype.BossArena };
        var room2 = new Room { RoomId = "room1", Archetype = RoomArchetype.Chamber };

        var densityMap = new Dictionary<Room, RoomDensity>
        {
            { room1, RoomDensity.Boss },
            { room2, RoomDensity.Light }
        };

        var rng = new Random(12345);

        // Act
        var plan = _service.DistributeBudget(budget, densityMap, rng);

        // Assert
        var bossAllocation = plan.RoomAllocations["boss"];
        Assert.IsTrue(bossAllocation.AllocatedEnemies >= 4 && bossAllocation.AllocatedEnemies <= 6,
            $"Boss room should get ~25% of 20 enemies (4-6), got {bossAllocation.AllocatedEnemies}");
    }

    [TestMethod]
    public void DistributeBudget_HeavyRoomGets5to7Threats()
    {
        // Arrange
        var budget = new GlobalBudget
        {
            TotalEnemyBudget = 20,
            TotalHazardBudget = 15
        };

        var room = new Room { RoomId = "heavy1", Archetype = RoomArchetype.LargeChamber };
        var densityMap = new Dictionary<Room, RoomDensity>
        {
            { room, RoomDensity.Heavy }
        };

        var rng = new Random(12345);

        // Act
        var plan = _service.DistributeBudget(budget, densityMap, rng);

        // Assert
        var allocation = plan.RoomAllocations["heavy1"];
        var totalThreats = allocation.AllocatedEnemies + allocation.AllocatedHazards;

        Assert.IsTrue(totalThreats >= 5 && totalThreats <= 7,
            $"Heavy room should get 5-7 threats, got {totalThreats}");
    }

    [TestMethod]
    public void DistributeBudget_MediumRoomGets3to4Threats()
    {
        // Arrange
        var budget = new GlobalBudget
        {
            TotalEnemyBudget = 15,
            TotalHazardBudget = 10
        };

        var room = new Room { RoomId = "medium1", Archetype = RoomArchetype.Chamber };
        var densityMap = new Dictionary<Room, RoomDensity>
        {
            { room, RoomDensity.Medium }
        };

        var rng = new Random(12345);

        // Act
        var plan = _service.DistributeBudget(budget, densityMap, rng);

        // Assert
        var allocation = plan.RoomAllocations["medium1"];
        var totalThreats = allocation.AllocatedEnemies + allocation.AllocatedHazards;

        Assert.IsTrue(totalThreats >= 3 && totalThreats <= 4,
            $"Medium room should get 3-4 threats, got {totalThreats}");
    }

    [TestMethod]
    public void DistributeBudget_LightRoomGets1to2Threats()
    {
        // Arrange
        var budget = new GlobalBudget
        {
            TotalEnemyBudget = 10,
            TotalHazardBudget = 8
        };

        var room = new Room { RoomId = "light1", Archetype = RoomArchetype.Corridor };
        var densityMap = new Dictionary<Room, RoomDensity>
        {
            { room, RoomDensity.Light }
        };

        var rng = new Random(12345);

        // Act
        var plan = _service.DistributeBudget(budget, densityMap, rng);

        // Assert
        var allocation = plan.RoomAllocations["light1"];
        var totalThreats = allocation.AllocatedEnemies + allocation.AllocatedHazards;

        Assert.IsTrue(totalThreats >= 1 && totalThreats <= 2,
            $"Light room should get 1-2 threats, got {totalThreats}");
    }

    [TestMethod]
    public void DistributeBudget_EmptyRoomGetsLootNotThreats()
    {
        // Arrange
        var budget = new GlobalBudget
        {
            TotalEnemyBudget = 15,
            TotalHazardBudget = 10,
            TotalLootBudget = 5
        };

        var room = new Room { RoomId = "empty1", Archetype = RoomArchetype.SecretRoom };
        var densityMap = new Dictionary<Room, RoomDensity>
        {
            { room, RoomDensity.Empty }
        };

        var rng = new Random(12345);

        // Act
        var plan = _service.DistributeBudget(budget, densityMap, rng);

        // Assert
        var allocation = plan.RoomAllocations["empty1"];

        Assert.AreEqual(0, allocation.AllocatedEnemies, "Empty room should have 0 enemies");
        Assert.AreEqual(0, allocation.AllocatedHazards, "Empty room should have 0 hazards");
        Assert.IsTrue(allocation.AllocatedLoot > 0, "Empty room should have loot");
    }

    [TestMethod]
    public void DistributeBudget_TotalAllocatedDoesNotExceedBudget()
    {
        // Arrange
        var budget = new GlobalBudget
        {
            TotalEnemyBudget = 15,
            TotalHazardBudget = 10,
            TotalLootBudget = 5
        };

        var rooms = new List<Room>();
        for (int i = 0; i < 7; i++)
        {
            rooms.Add(new Room { RoomId = $"room_{i}", Archetype = RoomArchetype.Chamber });
        }

        var densityMap = new Dictionary<Room, RoomDensity>
        {
            { rooms[0], RoomDensity.Boss },
            { rooms[1], RoomDensity.Heavy },
            { rooms[2], RoomDensity.Medium },
            { rooms[3], RoomDensity.Medium },
            { rooms[4], RoomDensity.Light },
            { rooms[5], RoomDensity.Light },
            { rooms[6], RoomDensity.Empty }
        };

        var rng = new Random(12345);

        // Act
        var plan = _service.DistributeBudget(budget, densityMap, rng);

        // Assert
        Assert.IsTrue(plan.TotalEnemiesAllocated <= budget.TotalEnemyBudget,
            $"Allocated enemies ({plan.TotalEnemiesAllocated}) should not exceed budget ({budget.TotalEnemyBudget})");
        Assert.IsTrue(plan.TotalHazardsAllocated <= budget.TotalHazardBudget,
            $"Allocated hazards ({plan.TotalHazardsAllocated}) should not exceed budget ({budget.TotalHazardBudget})");
        Assert.IsTrue(plan.TotalLootAllocated <= budget.TotalLootBudget,
            $"Allocated loot ({plan.TotalLootAllocated}) should not exceed budget ({budget.TotalLootBudget})");
    }

    [TestMethod]
    public void DistributeBudget_EmptyDensityMap_ReturnsEmptyPlan()
    {
        // Arrange
        var budget = new GlobalBudget
        {
            TotalEnemyBudget = 15,
            TotalHazardBudget = 10
        };

        var densityMap = new Dictionary<Room, RoomDensity>();
        var rng = new Random(12345);

        // Act
        var plan = _service.DistributeBudget(budget, densityMap, rng);

        // Assert
        Assert.AreEqual(0, plan.RoomAllocations.Count, "Empty density map should return empty plan");
        Assert.AreEqual(0, plan.TotalEnemiesAllocated);
        Assert.AreEqual(0, plan.TotalHazardsAllocated);
    }

    #endregion
}
