using NUnit.Framework;
using Microsoft.Data.Sqlite;
using RuneAndRust.Engine.Crafting;
using RuneAndRust.Persistence;

namespace RuneAndRust.Tests;

[TestFixture]
public class RecipeServiceTests
{
    private SqliteConnection _connection = null!;
    private CraftingRepository _repository = null!;
    private RecipeService _recipeService = null!;

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
        _recipeService = new RecipeService(_repository);
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
                times_crafted INTEGER DEFAULT 0,
                discovered_at TEXT NULL,
                discovery_source TEXT DEFAULT '',
                PRIMARY KEY (character_id, recipe_id)
            );

            CREATE TABLE Characters (
                character_id INTEGER PRIMARY KEY,
                name TEXT NOT NULL,
                credits INTEGER DEFAULT 0
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
        ";
        cmd.ExecuteNonQuery();
    }

    private void SeedTestData()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            -- Items
            INSERT INTO Items (item_id, item_name, item_type, quality_tier) VALUES
            (5001, 'Steel Ingot', 'Component', 3),
            (5002, 'Power Core', 'Component', 2),
            (5003, 'Weapon Frame', 'Component', 3);

            -- Characters
            INSERT INTO Characters (character_id, name, credits) VALUES
            (1, 'Test Character', 500),
            (2, 'Poor Character', 50);

            -- Crafting Recipes
            INSERT INTO Crafting_Recipes VALUES
            (1001, 'Plasma Rifle', 'Basic', 'Weapon', 'Forge', 0, 500, 30, 'WITS', 12, 'Default', 'A basic plasma rifle', NULL),
            (1002, 'Advanced Armor', 'Advanced', 'Armor', 'Workshop', 1, 1000, 60, 'WITS', 15, 'Merchant', 'Advanced protective armor', NULL),
            (1003, 'Master Sword', 'Expert', 'Weapon', 'Forge', 2, 2000, 120, 'WITS', 18, 'Quest', 'A masterwork blade', NULL),
            (1004, 'Basic Stim', 'Basic', 'Consumable', 'Laboratory', 0, 100, 15, 'WITS', 10, 'Default', 'Basic healing', NULL);

            -- Recipe Components
            INSERT INTO Recipe_Components (recipe_id, component_item_id, quantity_required, minimum_quality, is_optional) VALUES
            (1001, 5001, 2, 2, 0),  -- Plasma Rifle needs 2 Steel Ingots
            (1001, 5002, 1, 2, 0),  -- Plasma Rifle needs 1 Power Core
            (1002, 5001, 3, 3, 0),  -- Advanced Armor needs 3 Steel Ingots (high quality)
            (1003, 5001, 3, 3, 0),  -- Master Sword needs 3 Steel Ingots
            (1003, 5003, 1, 3, 0);  -- Master Sword needs 1 Weapon Frame

            -- Character 1 already knows 2 recipes
            INSERT INTO Character_Recipes (character_id, recipe_id, is_unlocked, times_crafted, discovered_at, discovery_source) VALUES
            (1, 1001, 1, 2, '2025-01-01 12:00:00', 'Default'),
            (1, 1004, 1, 0, '2025-01-02 12:00:00', 'Merchant');

            -- Character 1 has components for Plasma Rifle
            INSERT INTO Character_Inventory (character_id, item_id, quantity, quality_tier) VALUES
            (1, 5001, 5, 3),  -- 5x Steel Ingot (Quality 3)
            (1, 5002, 2, 2),  -- 2x Power Core (Quality 2)
            (1, 5003, 1, 3);  -- 1x Weapon Frame (Quality 3)

            -- Crafting Stations
            INSERT INTO Crafting_Stations VALUES
            (1, 'Forge', 'Basic Forge', 2, 1, 1, 0, 0, 'A basic forge'),
            (2, 'Workshop', 'Engineering Workshop', 3, 1, 2, 0, 50, 'An engineering workshop');
        ";
        cmd.ExecuteNonQuery();
    }

    #endregion

    #region Discovery Tests

    [Test]
    public void DiscoverRecipe_NewRecipe_Success()
    {
        // Arrange
        int characterId = 1;
        int recipeId = 1002; // Advanced Armor (not yet known)
        string source = "Quest";

        // Act
        var result = _recipeService.DiscoverRecipe(characterId, recipeId, source);

        // Assert
        Assert.That(result, Is.True);

        // Verify it's now in known recipes
        var knownRecipes = _recipeService.GetKnownRecipes(characterId);
        Assert.That(knownRecipes.Any(r => r.RecipeId == recipeId), Is.True);
    }

    [Test]
    public void DiscoverRecipe_AlreadyKnown_ReturnsFalse()
    {
        // Arrange
        int characterId = 1;
        int recipeId = 1001; // Plasma Rifle (already known)

        // Act
        var result = _recipeService.DiscoverRecipe(characterId, recipeId, "Duplicate");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void GetKnownRecipes_ReturnsKnownRecipes()
    {
        // Arrange
        int characterId = 1;

        // Act
        var recipes = _recipeService.GetKnownRecipes(characterId);

        // Assert
        Assert.That(recipes, Has.Count.EqualTo(2));
        Assert.That(recipes.Any(r => r.RecipeName == "Plasma Rifle"), Is.True);
        Assert.That(recipes.Any(r => r.RecipeName == "Basic Stim"), Is.True);
    }

    [Test]
    public void GetKnownRecipes_IncludesMetadata()
    {
        // Arrange
        int characterId = 1;

        // Act
        var recipes = _recipeService.GetKnownRecipes(characterId);
        var plasmaRifle = recipes.First(r => r.RecipeName == "Plasma Rifle");

        // Assert
        Assert.That(plasmaRifle.TimesCrafted, Is.EqualTo(2));
        Assert.That(plasmaRifle.DiscoverySource, Is.EqualTo("Default"));
        Assert.That(plasmaRifle.DiscoveredAt, Is.Not.Null);
    }

    #endregion

    #region Craftable Recipe Tests

    [Test]
    public void GetCraftableRecipes_ReturnsAllKnown()
    {
        // Arrange
        int characterId = 1;

        // Act
        var craftable = _recipeService.GetCraftableRecipes(characterId);

        // Assert
        Assert.That(craftable, Has.Count.EqualTo(2)); // 2 known recipes
    }

    [Test]
    public void GetCraftableRecipes_IndicatesComponentAvailability()
    {
        // Arrange
        int characterId = 1;

        // Act
        var craftable = _recipeService.GetCraftableRecipes(characterId);

        // Assert
        var plasmaRifle = craftable.First(c => c.Recipe.RecipeName == "Plasma Rifle");
        Assert.That(plasmaRifle.CanCraft, Is.True); // Has components

        var basicStim = craftable.First(c => c.Recipe.RecipeName == "Basic Stim");
        Assert.That(basicStim.CanCraft, Is.False); // Missing components
    }

    [Test]
    public void GetCraftableRecipes_FiltersByStation()
    {
        // Arrange
        int characterId = 1;
        int stationId = 1; // Basic Forge

        // Act
        var craftable = _recipeService.GetCraftableRecipes(characterId, stationId);

        // Assert
        // Should only include recipes that can be made at Forge
        Assert.That(craftable.All(c => c.Recipe.RequiredStation == "Forge" || c.Recipe.RequiredStation == "Any"), Is.True);
    }

    #endregion

    #region Purchase Tests

    [Test]
    public void PurchaseRecipe_SufficientCredits_Success()
    {
        // Arrange
        int characterId = 1; // Has 500 credits
        int recipeId = 1002; // Advanced Armor (costs 225)
        int merchantId = 1;

        // Act
        var result = _recipeService.PurchaseRecipe(characterId, recipeId, merchantId);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Cost, Is.EqualTo(225));
        Assert.That(result.RecipeName, Is.EqualTo("Advanced Armor"));

        // Verify credits deducted
        int remainingCredits = _repository.GetCharacterCredits(characterId);
        Assert.That(remainingCredits, Is.EqualTo(275)); // 500 - 225

        // Verify recipe learned
        var knownRecipes = _recipeService.GetKnownRecipes(characterId);
        Assert.That(knownRecipes.Any(r => r.RecipeId == recipeId), Is.True);
    }

    [Test]
    public void PurchaseRecipe_InsufficientCredits_Fails()
    {
        // Arrange
        int characterId = 2; // Has 50 credits
        int recipeId = 1002; // Advanced Armor (costs 225)
        int merchantId = 1;

        // Act
        var result = _recipeService.PurchaseRecipe(characterId, recipeId, merchantId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Insufficient credits"));

        // Verify no credits deducted
        int remainingCredits = _repository.GetCharacterCredits(characterId);
        Assert.That(remainingCredits, Is.EqualTo(50));
    }

    [Test]
    public void PurchaseRecipe_AlreadyKnown_FailsWithoutCharge()
    {
        // Arrange
        int characterId = 1;
        int recipeId = 1001; // Plasma Rifle (already known)
        int merchantId = 1;

        int creditsBefore = _repository.GetCharacterCredits(characterId);

        // Act
        var result = _recipeService.PurchaseRecipe(characterId, recipeId, merchantId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("already know"));

        // Verify no credits deducted
        int creditsAfter = _repository.GetCharacterCredits(characterId);
        Assert.That(creditsAfter, Is.EqualTo(creditsBefore));
    }

    [Test]
    public void PurchaseRecipe_InvalidRecipe_Fails()
    {
        // Arrange
        int characterId = 1;
        int recipeId = 9999; // Non-existent
        int merchantId = 1;

        // Act
        var result = _recipeService.PurchaseRecipe(characterId, recipeId, merchantId);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Recipe not found"));
    }

    [Test]
    public void CalculateRecipeCost_CorrectTiers()
    {
        // This is tested indirectly through PurchaseRecipe
        // Basic = 75, Advanced = 225, Expert = 525, Master = 1000

        // Arrange & Act
        int characterId = 1;

        // Test Advanced tier (225)
        var result = _recipeService.PurchaseRecipe(characterId, 1002, 1);
        Assert.That(result.Cost, Is.EqualTo(225));
    }

    #endregion

    #region Merchant Recipe Tests

    [Test]
    public void GetMerchantRecipes_ReturnsAvailableRecipes()
    {
        // Act
        var merchantRecipes = _recipeService.GetMerchantRecipes();

        // Assert
        Assert.That(merchantRecipes, Is.Not.Empty);
        Assert.That(merchantRecipes.All(r =>
            r.DiscoveryMethod == "Merchant" || r.DiscoveryMethod == "Default"), Is.True);
    }

    #endregion

    #region Times Crafted Tests

    [Test]
    public void IncrementTimesCrafted_IncrementsCounter()
    {
        // Arrange
        int characterId = 1;
        int recipeId = 1001; // Plasma Rifle (currently crafted 2 times)

        // Act
        _recipeService.IncrementTimesCrafted(characterId, recipeId);

        // Assert
        var recipes = _recipeService.GetKnownRecipes(characterId);
        var plasmaRifle = recipes.First(r => r.RecipeId == recipeId);
        Assert.That(plasmaRifle.TimesCrafted, Is.EqualTo(3)); // 2 + 1
    }

    #endregion
}
