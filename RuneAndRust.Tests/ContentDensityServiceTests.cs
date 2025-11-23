using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Core.Population;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.39.3: Tests for ContentDensityService
/// Validates global budget calculations with difficulty and biome multipliers
/// </summary>
[TestClass]
public class ContentDensityServiceTests
{
    private ContentDensityService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _service = new ContentDensityService();
    }

    #region Budget Calculation Tests

    [TestMethod]
    public void CalculateGlobalBudget_7RoomsNormal_Returns15Enemies()
    {
        // Arrange: 7 rooms × 2.2 enemies/room = 15.4 → 15
        int roomCount = 7;
        DifficultyTier difficulty = DifficultyTier.Normal;
        string biomeId = "the_roots";

        // Act
        var budget = _service.CalculateGlobalBudget(roomCount, difficulty, biomeId);

        // Assert
        Assert.AreEqual(15, budget.TotalEnemyBudget, "Expected 15 enemies for 7 rooms (7 × 2.2 = 15.4 → 15)");
        Assert.AreEqual(10, budget.TotalHazardBudget, "Expected 10 hazards for 7 rooms (7 × 1.5 = 10.5 → 10)");
        Assert.AreEqual(5, budget.TotalLootBudget, "Expected 5 loot nodes for 7 rooms (7 × 0.8 = 5.6 → 5)");
    }

    [TestMethod]
    public void CalculateGlobalBudget_HardDifficulty_Increases30Percent()
    {
        // Arrange
        int roomCount = 7;
        string biomeId = "the_roots";

        // Act
        var normalBudget = _service.CalculateGlobalBudget(roomCount, DifficultyTier.Normal, biomeId);
        var hardBudget = _service.CalculateGlobalBudget(roomCount, DifficultyTier.Hard, biomeId);

        // Assert - Hard difficulty should be 1.3× Normal
        Assert.AreEqual((int)(normalBudget.TotalEnemyBudget * 1.3f), hardBudget.TotalEnemyBudget,
            "Hard difficulty should increase enemy budget by 30%");
        Assert.AreEqual((int)(normalBudget.TotalHazardBudget * 1.3f), hardBudget.TotalHazardBudget,
            "Hard difficulty should increase hazard budget by 30%");
    }

    [TestMethod]
    public void CalculateGlobalBudget_EasyDifficulty_Decreases20Percent()
    {
        // Arrange
        int roomCount = 10;
        string biomeId = "the_roots";

        // Act
        var normalBudget = _service.CalculateGlobalBudget(roomCount, DifficultyTier.Normal, biomeId);
        var easyBudget = _service.CalculateGlobalBudget(roomCount, DifficultyTier.Easy, biomeId);

        // Assert - Easy difficulty should be 0.8× Normal
        Assert.AreEqual((int)(normalBudget.TotalEnemyBudget * 0.8f), easyBudget.TotalEnemyBudget,
            "Easy difficulty should decrease enemy budget by 20%");
    }

    [TestMethod]
    public void CalculateGlobalBudget_LethalDifficulty_Increases60Percent()
    {
        // Arrange
        int roomCount = 5;
        string biomeId = "the_roots";

        // Act
        var normalBudget = _service.CalculateGlobalBudget(roomCount, DifficultyTier.Normal, biomeId);
        var lethalBudget = _service.CalculateGlobalBudget(roomCount, DifficultyTier.Lethal, biomeId);

        // Assert - Lethal difficulty should be 1.6× Normal
        Assert.AreEqual((int)(normalBudget.TotalEnemyBudget * 1.6f), lethalBudget.TotalEnemyBudget,
            "Lethal difficulty should increase enemy budget by 60%");
    }

    [TestMethod]
    public void CalculateGlobalBudget_Muspelheim_IncreasesBy20Percent()
    {
        // Arrange: Muspelheim has 1.2× multiplier
        int roomCount = 7;
        DifficultyTier difficulty = DifficultyTier.Normal;

        // Act
        var theRootsBudget = _service.CalculateGlobalBudget(roomCount, difficulty, "the_roots");
        var muspelheimBudget = _service.CalculateGlobalBudget(roomCount, difficulty, "muspelheim");

        // Assert
        Assert.AreEqual((int)(theRootsBudget.TotalEnemyBudget * 1.2f), muspelheimBudget.TotalEnemyBudget,
            "Muspelheim should be 20% more dangerous than The Roots");
    }

    [TestMethod]
    public void CalculateGlobalBudget_Jotunheim_IncreasesBy30Percent()
    {
        // Arrange: Jotunheim has 1.3× multiplier (most dangerous)
        int roomCount = 7;
        DifficultyTier difficulty = DifficultyTier.Normal;

        // Act
        var theRootsBudget = _service.CalculateGlobalBudget(roomCount, difficulty, "the_roots");
        var jotunheimBudget = _service.CalculateGlobalBudget(roomCount, difficulty, "jotunheim");

        // Assert
        Assert.AreEqual((int)(theRootsBudget.TotalEnemyBudget * 1.3f), jotunheimBudget.TotalEnemyBudget,
            "Jotunheim should be 30% more dangerous than The Roots");
    }

    [TestMethod]
    public void CalculateGlobalBudget_LootNotScaledByDifficulty()
    {
        // Arrange
        int roomCount = 7;
        string biomeId = "the_roots";

        // Act
        var easyBudget = _service.CalculateGlobalBudget(roomCount, DifficultyTier.Easy, biomeId);
        var lethalBudget = _service.CalculateGlobalBudget(roomCount, DifficultyTier.Lethal, biomeId);

        // Assert - Loot should be the same regardless of difficulty
        Assert.AreEqual(easyBudget.TotalLootBudget, lethalBudget.TotalLootBudget,
            "Loot budget should NOT be scaled by difficulty");
    }

    [TestMethod]
    public void CalculateGlobalBudget_InvalidRoomCount_ReturnsEmptyBudget()
    {
        // Arrange
        int roomCount = 0;
        DifficultyTier difficulty = DifficultyTier.Normal;
        string biomeId = "the_roots";

        // Act
        var budget = _service.CalculateGlobalBudget(roomCount, difficulty, biomeId);

        // Assert
        Assert.AreEqual(0, budget.TotalEnemyBudget);
        Assert.AreEqual(0, budget.TotalHazardBudget);
        Assert.AreEqual(0, budget.TotalLootBudget);
    }

    #endregion

    #region Budget Properties Tests

    [TestMethod]
    public void GlobalBudget_RemainingBudget_UpdatesCorrectly()
    {
        // Arrange
        var budget = new GlobalBudget
        {
            TotalEnemyBudget = 15,
            TotalHazardBudget = 10,
            EnemiesSpawned = 5,
            HazardsSpawned = 3
        };

        // Assert
        Assert.AreEqual(10, budget.RemainingEnemyBudget, "Remaining enemy budget should be 15 - 5 = 10");
        Assert.AreEqual(7, budget.RemainingHazardBudget, "Remaining hazard budget should be 10 - 3 = 7");
        Assert.IsFalse(budget.IsEnemyBudgetExhausted, "Enemy budget should not be exhausted");
        Assert.IsFalse(budget.IsHazardBudgetExhausted, "Hazard budget should not be exhausted");
    }

    [TestMethod]
    public void GlobalBudget_BudgetExhausted_WhenSpawnedEqualsTotal()
    {
        // Arrange
        var budget = new GlobalBudget
        {
            TotalEnemyBudget = 15,
            TotalHazardBudget = 10,
            EnemiesSpawned = 15,
            HazardsSpawned = 10
        };

        // Assert
        Assert.AreEqual(0, budget.RemainingEnemyBudget);
        Assert.AreEqual(0, budget.RemainingHazardBudget);
        Assert.IsTrue(budget.IsEnemyBudgetExhausted, "Enemy budget should be exhausted");
        Assert.IsTrue(budget.IsHazardBudgetExhausted, "Hazard budget should be exhausted");
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void CalculateGlobalBudget_5RoomsHardMuspelheim_CorrectCombinedMultipliers()
    {
        // Arrange: Base 5 rooms, Hard (1.3×), Muspelheim (1.2×)
        // Expected: 5 × 2.2 × 1.3 × 1.2 = 17.16 → 17 enemies
        int roomCount = 5;
        DifficultyTier difficulty = DifficultyTier.Hard;
        string biomeId = "muspelheim";

        // Act
        var budget = _service.CalculateGlobalBudget(roomCount, difficulty, biomeId);

        // Assert
        Assert.AreEqual(17, budget.TotalEnemyBudget, "Expected 17 enemies (5 × 2.2 × 1.3 × 1.2 = 17.16 → 17)");
        Assert.AreEqual(11, budget.TotalHazardBudget, "Expected 11 hazards (5 × 1.5 × 1.3 × 1.2 = 11.7 → 11)");
    }

    [TestMethod]
    public void CalculateGlobalBudget_AverageThreatPerRoom_BelowThreshold()
    {
        // Arrange: Goal is 2.0-2.5 enemies per room average
        int roomCount = 7;
        DifficultyTier difficulty = DifficultyTier.Normal;
        string biomeId = "the_roots";

        // Act
        var budget = _service.CalculateGlobalBudget(roomCount, difficulty, biomeId);
        float avgEnemiesPerRoom = (float)budget.TotalEnemyBudget / roomCount;
        float avgHazardsPerRoom = (float)budget.TotalHazardBudget / roomCount;
        float avgThreatsPerRoom = avgEnemiesPerRoom + avgHazardsPerRoom;

        // Assert - Should be around 3.5-4.0 total threats per room
        Assert.IsTrue(avgEnemiesPerRoom >= 2.0f && avgEnemiesPerRoom <= 2.5f,
            $"Average enemies per room should be 2.0-2.5, got {avgEnemiesPerRoom:F2}");
        Assert.IsTrue(avgThreatsPerRoom >= 3.0f && avgThreatsPerRoom <= 4.5f,
            $"Average total threats per room should be 3.0-4.5, got {avgThreatsPerRoom:F2}");
    }

    #endregion
}
