using NUnit.Framework;
using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Engine.Crafting;
using RuneAndRust.Persistence;

namespace RuneAndRust.Tests;

[TestFixture]
public class AdvancedCraftingServiceTests
{
    private SqliteConnection _connection = null!;
    private CraftingRepository _repository = null!;
    private AdvancedCraftingService _craftingService = null!;
    private DiceService _diceService = null!;

    [SetUp]
    public void Setup()
    {
        // Create in-memory database
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        // Create schema
        CreateTestSchema();

        // Seed test data
        SeedTestData();

        // Initialize services
        _repository = new CraftingRepository(_connection.ConnectionString);
        _diceService = new DiceService();
        _craftingService = new AdvancedCraftingService(_repository, _diceService);
    }

    [TearDown]
    public void TearDown()
    {
        _connection?.Close();
        _connection?.Dispose();
    }

    #region Test Data Setup

    private void CreateTestSchema()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE Items (
                item_id INTEGER PRIMARY KEY,
                item_name TEXT NOT NULL,
                item_type TEXT NOT NULL,
                quality_tier INTEGER DEFAULT 1
            );

            CREATE TABLE Crafting_Recipes (
                recipe_id INTEGER PRIMARY KEY,
                recipe_name TEXT NOT NULL,
                recipe_tier TEXT NOT NULL,
                crafted_item_type TEXT NOT NULL,
                required_station TEXT NOT NULL,
                quality_bonus INTEGER DEFAULT 0,
                base_value INTEGER DEFAULT 0,
                crafting_time_minutes INTEGER DEFAULT 0,
                skill_attribute TEXT NOT NULL,
                skill_check_dc INTEGER DEFAULT 10,
                discovery_method TEXT DEFAULT 'Default',
                recipe_description TEXT DEFAULT '',
                special_effects_json TEXT NULL
            );

            CREATE TABLE Recipe_Components (
                component_id INTEGER PRIMARY KEY AUTOINCREMENT,
                recipe_id INTEGER NOT NULL,
                component_item_id INTEGER NOT NULL,
                quantity_required INTEGER DEFAULT 1,
                minimum_quality INTEGER DEFAULT 1,
                is_optional INTEGER DEFAULT 0,
                FOREIGN KEY (recipe_id) REFERENCES Crafting_Recipes(recipe_id),
                FOREIGN KEY (component_item_id) REFERENCES Items(item_id)
            );

            CREATE TABLE Crafting_Stations (
                station_id INTEGER PRIMARY KEY,
                station_type TEXT NOT NULL,
                station_name TEXT NOT NULL,
                max_quality_tier INTEGER DEFAULT 1,
                location_sector_id INTEGER NULL,
                location_room_id INTEGER NULL,
                requires_controlling INTEGER DEFAULT 0,
                usage_cost_credits INTEGER DEFAULT 0,
                station_description TEXT DEFAULT ''
            );

            CREATE TABLE Character_Inventory (
                inventory_id INTEGER PRIMARY KEY AUTOINCREMENT,
                character_id INTEGER NOT NULL,
                item_id INTEGER NOT NULL,
                quantity INTEGER DEFAULT 0,
                quality_tier INTEGER DEFAULT 1,
                UNIQUE(character_id, item_id, quality_tier)
            );

            CREATE TABLE Character_Recipes (
                character_id INTEGER NOT NULL,
                recipe_id INTEGER NOT NULL,
                is_unlocked INTEGER DEFAULT 1,
                PRIMARY KEY (character_id, recipe_id)
            );
        ";
        cmd.ExecuteNonQuery();
    }

    private void SeedTestData()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            -- Components
            INSERT INTO Items (item_id, item_name, item_type, quality_tier) VALUES
            (5001, 'Steel Ingot', 'Component', 3),
            (5002, 'Power Core', 'Component', 2),
            (5003, 'Weapon Frame', 'Component', 3),
            (6001, 'Alloy Plate', 'Component', 2);

            -- Recipes
            INSERT INTO Crafting_Recipes VALUES
            (1001, 'Plasma Rifle', 'Basic', 'Weapon', 'Forge', 0, 500, 30, 'WITS', 12, 'Default', 'A basic plasma rifle', NULL),
            (1002, 'Advanced Armor', 'Advanced', 'Armor', 'Workshop', 1, 1000, 60, 'WITS', 15, 'Default', 'Advanced protective armor', NULL),
            (1003, 'Master Sword', 'Expert', 'Weapon', 'Forge', 2, 2000, 120, 'WITS', 18, 'Quest', 'A masterwork blade', NULL);

            -- Recipe Components
            INSERT INTO Recipe_Components (recipe_id, component_item_id, quantity_required, minimum_quality, is_optional) VALUES
            (1001, 5001, 2, 2, 0),  -- Plasma Rifle needs 2 Steel Ingots (min quality 2)
            (1001, 5002, 1, 2, 0),  -- Plasma Rifle needs 1 Power Core (min quality 2)
            (1002, 6001, 3, 2, 0),  -- Advanced Armor needs 3 Alloy Plates (min quality 2)
            (1003, 5001, 3, 3, 0),  -- Master Sword needs 3 Steel Ingots (min quality 3)
            (1003, 5003, 1, 3, 0);  -- Master Sword needs 1 Weapon Frame (min quality 3)

            -- Crafting Stations
            INSERT INTO Crafting_Stations VALUES
            (1, 'Forge', 'Basic Forge', 2, 1, 1, 0, 0, 'A basic forge'),
            (2, 'Forge', 'Master Forge', 5, 1, 2, 0, 100, 'A master-quality forge'),
            (3, 'Workshop', 'Engineering Workshop', 3, 1, 3, 0, 50, 'An engineering workshop');

            -- Test player inventory (Character ID = 1)
            INSERT INTO Character_Inventory (character_id, item_id, quantity, quality_tier) VALUES
            (1, 5001, 5, 3),  -- 5x Steel Ingot (Quality 3)
            (1, 5002, 2, 2),  -- 2x Power Core (Quality 2)
            (1, 5003, 1, 3),  -- 1x Weapon Frame (Quality 3)
            (1, 6001, 4, 2);  -- 4x Alloy Plate (Quality 2)

            -- Learned recipes for test player
            INSERT INTO Character_Recipes (character_id, recipe_id, is_unlocked) VALUES
            (1, 1001, 1),
            (1, 1002, 1),
            (1, 1003, 1);
        ";
        cmd.ExecuteNonQuery();
    }

    private PlayerCharacter CreateTestPlayer(int characterId = 1, int witsValue = 3)
    {
        return new PlayerCharacter
        {
            CharacterID = characterId,
            Name = "Test Character",
            WITS = witsValue,
            BRAWN = 2,
            FINESSE = 2,
            RESOLVE = 2
        };
    }

    #endregion

    #region Successful Crafting Tests

    [Test]
    public void CraftItem_ValidRecipeAndComponents_CraftsSuccessfully()
    {
        // Arrange
        var player = CreateTestPlayer(characterId: 1, witsValue: 5); // High WITS for guaranteed success
        int recipeId = 1001; // Plasma Rifle
        int stationId = 1;   // Basic Forge

        // Act
        var result = _craftingService.CraftItem(player, recipeId, stationId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.CraftedItemName, Is.EqualTo("Plasma Rifle"));
        Assert.That(result.SkillCheckPassed, Is.True);
        Assert.That(result.ConsumedComponents, Has.Count.EqualTo(2)); // Steel Ingot + Power Core
    }

    [Test]
    public void CraftItem_QualityCalculation_MinComponentAndStationQuality()
    {
        // Arrange
        var player = CreateTestPlayer(characterId: 1, witsValue: 5);
        int recipeId = 1001; // Plasma Rifle (quality_bonus = 0)
        int stationId = 1;   // Basic Forge (max_quality = 2)
        // Components: Steel Ingot (quality 3), Power Core (quality 2)
        // Expected: min(2, 2) + 0 = 2

        // Act
        var result = _craftingService.CraftItem(player, recipeId, stationId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.FinalQuality, Is.EqualTo(2));
        Assert.That(result.QualityCalculation, Is.Not.Null);
        Assert.That(result.QualityCalculation!.LowestComponentQuality, Is.EqualTo(2));
        Assert.That(result.QualityCalculation.StationMaxQuality, Is.EqualTo(2));
        Assert.That(result.QualityCalculation.RecipeQualityBonus, Is.EqualTo(0));
    }

    [Test]
    public void CraftItem_WithRecipeBonus_AddsToFinalQuality()
    {
        // Arrange
        var player = CreateTestPlayer(characterId: 1, witsValue: 5);
        int recipeId = 1002; // Advanced Armor (quality_bonus = 1)
        int stationId = 3;   // Engineering Workshop (max_quality = 3)
        // Components: Alloy Plate (quality 2)
        // Expected: min(2, 3) + 1 = 3

        // Act
        var result = _craftingService.CraftItem(player, recipeId, stationId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.FinalQuality, Is.EqualTo(3));
        Assert.That(result.QualityCalculation!.RecipeQualityBonus, Is.EqualTo(1));
    }

    [Test]
    public void CraftItem_MasterRecipe_CanReachQuality4()
    {
        // Arrange
        var player = CreateTestPlayer(characterId: 1, witsValue: 6);
        int recipeId = 1003; // Master Sword (quality_bonus = 2)
        int stationId = 2;   // Master Forge (max_quality = 5)
        // Components: Steel Ingot (quality 3), Weapon Frame (quality 3)
        // Expected: min(3, 5) + 2 = 5, clamped to 4

        // Act
        var result = _craftingService.CraftItem(player, recipeId, stationId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.FinalQuality, Is.EqualTo(4)); // Clamped to max crafted quality
    }

    #endregion

    #region Failure Tests

    [Test]
    public void CraftItem_InsufficientComponents_ReturnsFailed()
    {
        // Arrange
        var player = CreateTestPlayer(characterId: 2, witsValue: 5); // Different character with no components
        int recipeId = 1001;
        int stationId = 1;

        // Act
        var result = _craftingService.CraftItem(player, recipeId, stationId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Insufficient components"));
    }

    [Test]
    public void CraftItem_WrongStationType_ReturnsFailed()
    {
        // Arrange
        var player = CreateTestPlayer(characterId: 1, witsValue: 5);
        int recipeId = 1001; // Plasma Rifle (requires Forge)
        int stationId = 3;   // Engineering Workshop (Workshop type)

        // Act
        var result = _craftingService.CraftItem(player, recipeId, stationId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("requires a Forge"));
    }

    [Test]
    public void CraftItem_InvalidRecipeId_ReturnsFailed()
    {
        // Arrange
        var player = CreateTestPlayer(characterId: 1, witsValue: 5);
        int recipeId = 9999; // Non-existent recipe
        int stationId = 1;

        // Act
        var result = _craftingService.CraftItem(player, recipeId, stationId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Recipe not found."));
    }

    [Test]
    public void CraftItem_InvalidStationId_ReturnsFailed()
    {
        // Arrange
        var player = CreateTestPlayer(characterId: 1, witsValue: 5);
        int recipeId = 1001;
        int stationId = 9999; // Non-existent station

        // Act
        var result = _craftingService.CraftItem(player, recipeId, stationId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Crafting station not found."));
    }

    [Test]
    public void CraftItem_FailedSkillCheck_ConsumesComponentsButNoItem()
    {
        // Arrange
        var player = CreateTestPlayer(characterId: 1, witsValue: 0); // Very low WITS for guaranteed failure
        int recipeId = 1001; // Plasma Rifle (DC 12)
        int stationId = 1;

        // Get initial component count
        var initialComponents = _repository.GetPlayerComponents(1);
        var initialSteelCount = initialComponents.First(c => c.ItemId == 5001).Quantity;

        // Act
        var result = _craftingService.CraftItem(player, recipeId, stationId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.SkillCheckPassed, Is.False);
        Assert.That(result.Message, Does.Contain("Crafting failed"));
        Assert.That(result.Message, Does.Contain("Components were consumed"));
        Assert.That(result.CraftedItemId, Is.Null);

        // Verify components were consumed
        var afterComponents = _repository.GetPlayerComponents(1);
        var afterSteelCount = afterComponents.First(c => c.ItemId == 5001).Quantity;
        Assert.That(afterSteelCount, Is.LessThan(initialSteelCount));
    }

    #endregion

    #region Component Validation Tests

    [Test]
    public void CraftItem_ComponentQualityTooLow_ReturnsFailed()
    {
        // Arrange: Add low-quality components to a new character
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Character_Inventory (character_id, item_id, quantity, quality_tier) VALUES
            (3, 5001, 5, 1);  -- Quality 1 Steel Ingots (recipe requires quality 2)
        ";
        cmd.ExecuteNonQuery();

        var player = CreateTestPlayer(characterId: 3, witsValue: 5);
        int recipeId = 1001; // Requires quality 2 components
        int stationId = 1;

        // Act
        var result = _craftingService.CraftItem(player, recipeId, stationId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Insufficient components"));
    }

    #endregion

    #region Helper Method Tests

    [Test]
    public void GetAvailableRecipes_ReturnsLearnedRecipes()
    {
        // Act
        var recipes = _craftingService.GetAvailableRecipes(characterId: 1);

        // Assert
        Assert.That(recipes, Has.Count.EqualTo(3));
        Assert.That(recipes.Select(r => r.RecipeName), Contains.Item("Plasma Rifle"));
        Assert.That(recipes.Select(r => r.RecipeName), Contains.Item("Advanced Armor"));
    }

    [Test]
    public void GetStationsBySector_ReturnsStationsInSector()
    {
        // Act
        var stations = _craftingService.GetStationsBySector(sectorId: 1);

        // Assert
        Assert.That(stations, Has.Count.EqualTo(3));
        Assert.That(stations.Select(s => s.StationName), Contains.Item("Basic Forge"));
        Assert.That(stations.Select(s => s.StationName), Contains.Item("Master Forge"));
    }

    [Test]
    public void GetPlayerComponents_ReturnsComponentInventory()
    {
        // Act
        var components = _craftingService.GetPlayerComponents(characterId: 1);

        // Assert
        Assert.That(components, Has.Count.EqualTo(4));
        Assert.That(components.Any(c => c.ItemName == "Steel Ingot" && c.Quantity == 5), Is.True);
        Assert.That(components.Any(c => c.ItemName == "Power Core" && c.Quantity == 2), Is.True);
    }

    #endregion

    #region Edge Cases

    [Test]
    public void CraftItem_ExactComponentAmount_CraftsSuccessfully()
    {
        // Arrange: Character with exact components needed
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Character_Inventory (character_id, item_id, quantity, quality_tier) VALUES
            (4, 5001, 2, 3),  -- Exactly 2 Steel Ingots
            (4, 5002, 1, 2);  -- Exactly 1 Power Core

            INSERT INTO Character_Recipes (character_id, recipe_id, is_unlocked) VALUES
            (4, 1001, 1);
        ";
        cmd.ExecuteNonQuery();

        var player = CreateTestPlayer(characterId: 4, witsValue: 5);
        int recipeId = 1001; // Requires 2 Steel + 1 Power Core
        int stationId = 1;

        // Act
        var result = _craftingService.CraftItem(player, recipeId, stationId);

        // Assert
        Assert.That(result.Success, Is.True);

        // Verify all components consumed
        var afterComponents = _repository.GetPlayerComponents(4);
        Assert.That(afterComponents.Sum(c => c.Quantity), Is.EqualTo(0));
    }

    #endregion
}
