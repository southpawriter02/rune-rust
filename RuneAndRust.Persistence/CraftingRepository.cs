using Microsoft.Data.Sqlite;
using RuneAndRust.Engine.Crafting;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.36.1/v0.36.2: Repository for managing crafting system data
/// Provides database access for recipes, components, stations, and player inventory
/// </summary>
public class CraftingRepository
{
    private static readonly ILogger _log = Log.ForContext<CraftingRepository>();
    private readonly string _connectionString;

    public CraftingRepository(string connectionString)
    {
        _connectionString = connectionString;
        _log.Debug("CraftingRepository initialized");
    }

    #region Recipe Queries

    /// <summary>
    /// Get a recipe by ID with its components
    /// </summary>
    public Recipe? GetRecipeById(int recipeId)
    {
        _log.Debug("Getting recipe by ID: {RecipeId}", recipeId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Get recipe
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                recipe_id, recipe_name, recipe_tier, crafted_item_type,
                required_station, quality_bonus, base_value, crafting_time_minutes,
                skill_attribute, skill_check_dc, discovery_method, recipe_description,
                special_effects_json
            FROM Crafting_Recipes
            WHERE recipe_id = @RecipeId
        ";
        command.Parameters.AddWithValue("@RecipeId", recipeId);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            _log.Warning("Recipe not found: {RecipeId}", recipeId);
            return null;
        }

        var recipe = MapRecipeFromReader(reader);
        reader.Close();

        // Get components for this recipe
        recipe.RequiredComponents = GetRecipeComponents(connection, recipeId);

        return recipe;
    }

    /// <summary>
    /// Get all recipes of a specific tier
    /// </summary>
    public List<Recipe> GetRecipesByTier(string tier)
    {
        _log.Debug("Getting recipes by tier: {Tier}", tier);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var recipes = new List<Recipe>();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                recipe_id, recipe_name, recipe_tier, crafted_item_type,
                required_station, quality_bonus, base_value, crafting_time_minutes,
                skill_attribute, skill_check_dc, discovery_method, recipe_description,
                special_effects_json
            FROM Crafting_Recipes
            WHERE recipe_tier = @Tier
            ORDER BY recipe_name
        ";
        command.Parameters.AddWithValue("@Tier", tier);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            recipes.Add(MapRecipeFromReader(reader));
        }

        // Load components for each recipe
        foreach (var recipe in recipes)
        {
            recipe.RequiredComponents = GetRecipeComponents(connection, recipe.RecipeId);
        }

        _log.Information("Found {Count} recipes for tier {Tier}", recipes.Count, tier);
        return recipes;
    }

    /// <summary>
    /// Get recipes learned by a character
    /// </summary>
    public List<Recipe> GetLearnedRecipes(int characterId)
    {
        _log.Debug("Getting learned recipes for character: {CharacterId}", characterId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var recipes = new List<Recipe>();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                r.recipe_id, r.recipe_name, r.recipe_tier, r.crafted_item_type,
                r.required_station, r.quality_bonus, r.base_value, r.crafting_time_minutes,
                r.skill_attribute, r.skill_check_dc, r.discovery_method, r.recipe_description,
                r.special_effects_json
            FROM Crafting_Recipes r
            INNER JOIN Character_Recipes cr ON r.recipe_id = cr.recipe_id
            WHERE cr.character_id = @CharacterId AND cr.is_unlocked = 1
            ORDER BY r.recipe_tier, r.recipe_name
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            recipes.Add(MapRecipeFromReader(reader));
        }

        // Load components for each recipe
        foreach (var recipe in recipes)
        {
            recipe.RequiredComponents = GetRecipeComponents(connection, recipe.RecipeId);
        }

        _log.Information("Found {Count} learned recipes for character {CharacterId}",
            recipes.Count, characterId);
        return recipes;
    }

    /// <summary>
    /// Get components for a specific recipe
    /// </summary>
    private List<RecipeComponent> GetRecipeComponents(SqliteConnection connection, int recipeId)
    {
        var components = new List<RecipeComponent>();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                rc.component_id, rc.recipe_id, rc.component_item_id,
                i.item_name, rc.quantity_required, rc.minimum_quality, rc.is_optional
            FROM Recipe_Components rc
            INNER JOIN Items i ON rc.component_item_id = i.item_id
            WHERE rc.recipe_id = @RecipeId
            ORDER BY rc.component_id
        ";
        command.Parameters.AddWithValue("@RecipeId", recipeId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            components.Add(new RecipeComponent
            {
                ComponentId = reader.GetInt32(0),
                RecipeId = reader.GetInt32(1),
                ComponentItemId = reader.GetInt32(2),
                ComponentName = reader.GetString(3),
                QuantityRequired = reader.GetInt32(4),
                MinimumQuality = reader.GetInt32(5),
                IsOptional = reader.GetBoolean(6)
            });
        }

        return components;
    }

    #endregion

    #region Crafting Station Queries

    /// <summary>
    /// Get a crafting station by ID
    /// </summary>
    public CraftingStation? GetStationById(int stationId)
    {
        _log.Debug("Getting station by ID: {StationId}", stationId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                station_id, station_type, station_name, max_quality_tier,
                location_sector_id, location_room_id, requires_controlling,
                usage_cost_credits, station_description
            FROM Crafting_Stations
            WHERE station_id = @StationId
        ";
        command.Parameters.AddWithValue("@StationId", stationId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapStationFromReader(reader);
        }

        _log.Warning("Station not found: {StationId}", stationId);
        return null;
    }

    /// <summary>
    /// Get all stations of a specific type
    /// </summary>
    public List<CraftingStation> GetStationsByType(string stationType)
    {
        _log.Debug("Getting stations by type: {StationType}", stationType);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var stations = new List<CraftingStation>();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                station_id, station_type, station_name, max_quality_tier,
                location_sector_id, location_room_id, requires_controlling,
                usage_cost_credits, station_description
            FROM Crafting_Stations
            WHERE station_type = @StationType
            ORDER BY max_quality_tier DESC, station_name
        ";
        command.Parameters.AddWithValue("@StationType", stationType);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            stations.Add(MapStationFromReader(reader));
        }

        _log.Information("Found {Count} stations of type {StationType}", stations.Count, stationType);
        return stations;
    }

    /// <summary>
    /// Get stations in a specific sector
    /// </summary>
    public List<CraftingStation> GetStationsBySector(int sectorId)
    {
        _log.Debug("Getting stations in sector: {SectorId}", sectorId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var stations = new List<CraftingStation>();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                station_id, station_type, station_name, max_quality_tier,
                location_sector_id, location_room_id, requires_controlling,
                usage_cost_credits, station_description
            FROM Crafting_Stations
            WHERE location_sector_id = @SectorId
            ORDER BY station_type, station_name
        ";
        command.Parameters.AddWithValue("@SectorId", sectorId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            stations.Add(MapStationFromReader(reader));
        }

        _log.Information("Found {Count} stations in sector {SectorId}", stations.Count, sectorId);
        return stations;
    }

    #endregion

    #region Player Component Inventory

    /// <summary>
    /// Get player's component inventory
    /// </summary>
    public List<PlayerComponent> GetPlayerComponents(int characterId)
    {
        _log.Debug("Getting component inventory for character: {CharacterId}", characterId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var components = new List<PlayerComponent>();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                ci.inventory_id, ci.character_id, ci.item_id,
                i.item_name, ci.quantity, ci.quality_tier, i.item_type
            FROM Character_Inventory ci
            INNER JOIN Items i ON ci.item_id = i.item_id
            WHERE ci.character_id = @CharacterId
              AND i.item_type = 'Component'
              AND ci.quantity > 0
            ORDER BY i.item_name, ci.quality_tier DESC
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            components.Add(new PlayerComponent
            {
                InventoryId = reader.GetInt32(0),
                CharacterId = reader.GetInt32(1),
                ItemId = reader.GetInt32(2),
                ItemName = reader.GetString(3),
                Quantity = reader.GetInt32(4),
                QualityTier = reader.GetInt32(5),
                ItemType = reader.GetString(6)
            });
        }

        _log.Information("Found {Count} component stacks for character {CharacterId}",
            components.Count, characterId);
        return components;
    }

    /// <summary>
    /// Consume components from player inventory
    /// </summary>
    public bool ConsumeComponents(int characterId, List<ConsumedComponent> components)
    {
        _log.Debug("Consuming {Count} component types for character {CharacterId}",
            components.Count, characterId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var component in components)
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    UPDATE Character_Inventory
                    SET quantity = quantity - @Quantity
                    WHERE character_id = @CharacterId
                      AND item_id = @ItemId
                      AND quality_tier = @QualityTier
                      AND quantity >= @Quantity
                ";
                command.Parameters.AddWithValue("@CharacterId", characterId);
                command.Parameters.AddWithValue("@ItemId", component.ItemId);
                command.Parameters.AddWithValue("@QualityTier", component.QualityTier);
                command.Parameters.AddWithValue("@Quantity", component.Quantity);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    _log.Warning("Failed to consume component: {ItemId} x{Quantity} (Quality {Quality})",
                        component.ItemId, component.Quantity, component.QualityTier);
                    transaction.Rollback();
                    return false;
                }
            }

            transaction.Commit();
            _log.Information("Successfully consumed components for character {CharacterId}", characterId);
            return true;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error consuming components for character {CharacterId}", characterId);
            transaction.Rollback();
            return false;
        }
    }

    /// <summary>
    /// Add crafted item to player inventory
    /// </summary>
    public bool AddCraftedItem(int characterId, int itemId, int qualityTier, int quantity = 1)
    {
        _log.Debug("Adding crafted item to inventory: Character={CharacterId}, Item={ItemId}, Quality={Quality}",
            characterId, itemId, qualityTier);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Character_Inventory (character_id, item_id, quantity, quality_tier)
            VALUES (@CharacterId, @ItemId, @Quantity, @QualityTier)
            ON CONFLICT(character_id, item_id, quality_tier)
            DO UPDATE SET quantity = quantity + @Quantity
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);
        command.Parameters.AddWithValue("@ItemId", itemId);
        command.Parameters.AddWithValue("@Quantity", quantity);
        command.Parameters.AddWithValue("@QualityTier", qualityTier);

        try
        {
            command.ExecuteNonQuery();
            _log.Information("Successfully added crafted item to inventory: {ItemId} x{Quantity} (Quality {Quality})",
                itemId, quantity, qualityTier);
            return true;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error adding crafted item to inventory");
            return false;
        }
    }

    #endregion

    #region Helper Methods

    private Recipe MapRecipeFromReader(SqliteDataReader reader)
    {
        return new Recipe
        {
            RecipeId = reader.GetInt32(0),
            RecipeName = reader.GetString(1),
            RecipeTier = reader.GetString(2),
            CraftedItemType = reader.GetString(3),
            RequiredStation = reader.GetString(4),
            QualityBonus = reader.GetInt32(5),
            BaseValue = reader.GetInt32(6),
            CraftingTimeMinutes = reader.GetInt32(7),
            SkillAttribute = reader.GetString(8),
            SkillCheckDC = reader.GetInt32(9),
            DiscoveryMethod = reader.GetString(10),
            RecipeDescription = reader.GetString(11),
            SpecialEffectsJson = reader.IsDBNull(12) ? null : reader.GetString(12)
        };
    }

    private CraftingStation MapStationFromReader(SqliteDataReader reader)
    {
        return new CraftingStation
        {
            StationId = reader.GetInt32(0),
            StationType = reader.GetString(1),
            StationName = reader.GetString(2),
            MaxQualityTier = reader.GetInt32(3),
            LocationSectorId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
            LocationRoomId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
            RequiresControlling = reader.GetBoolean(6),
            UsageCostCredits = reader.GetInt32(7),
            StationDescription = reader.GetString(8)
        };
    }

    #endregion
}
