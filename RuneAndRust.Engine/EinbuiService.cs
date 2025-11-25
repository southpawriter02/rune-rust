using RuneAndRust.Core;
using Serilog;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.27.2: Service for Einbui specialization-specific abilities and mechanics.
/// Handles field crafting, trap placement, resource location, and Blight Haven creation.
/// The ultimate survivor - master of radical self-reliance.
/// </summary>
public class EinbuiService
{
    private static readonly ILogger _log = Log.ForContext<EinbuiService>();
    private readonly string _connectionString;

    public EinbuiService(string connectionString)
    {
        _connectionString = connectionString;
        _log.Debug("EinbuiService initialized");
    }

    #region Field Crafting Structures

    /// <summary>
    /// Types of concoctions that can be crafted
    /// </summary>
    public enum ConcoctionType
    {
        Poultice,
        Stimulant
    }

    /// <summary>
    /// Result of a field crafting operation
    /// </summary>
    public class CraftResult
    {
        public bool Success { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemEffect { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result of a trap placement operation
    /// </summary>
    public class TrapResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TrapId { get; set; }
    }

    /// <summary>
    /// Held item data
    /// </summary>
    public class HeldItem
    {
        public string Name { get; set; } = string.Empty;
        public string Effect { get; set; } = string.Empty;
    }

    #endregion

    #region Tier 1: Basic Concoction

    /// <summary>
    /// Craft a basic concoction (healing poultice or stamina stimulant)
    /// RANK 1: Crude Poultice (2d6 HP) or Weak Stimulant (15 Stamina)
    /// RANK 2: Refined Poultice (4d6 HP) or Potent Stimulant (30 Stamina)
    /// RANK 3: Superior Poultice (6d6 HP + remove Bleeding) or Exceptional Stimulant (45 Stamina + remove Exhausted)
    /// </summary>
    public CraftResult CraftBasicConcoction(
        int characterId,
        ConcoctionType type,
        int rank = 1,
        int masterImproviserRank = 0)
    {
        try
        {
            _log.Information(
                "Crafting Basic Concoction: CharacterID={CharacterId}, Type={Type}, Rank={Rank}, MasterImproviserRank={MasterImproviserRank}",
                characterId, type, rank, masterImproviserRank);

            // Master Improviser automatically upgrades to Rank 3 effects
            int effectiveRank = masterImproviserRank >= 1 ? 3 : rank;

            // Check if character has required components
            // This would normally query inventory, but for now we'll simulate
            bool hasHerb = CheckInventoryForItem(characterId, "Common Herb");
            bool hasCloth = CheckInventoryForItem(characterId, "Clean Cloth");

            if (!hasHerb || !hasCloth)
            {
                _log.Warning(
                    "Missing crafting components: CharacterID={CharacterId}, HasHerb={HasHerb}, HasCloth={HasCloth}",
                    characterId, hasHerb, hasCloth);
                return new CraftResult
                {
                    Success = false,
                    Message = "Missing components: Need 1 [Common Herb] + 1 [Clean Cloth]"
                };
            }

            // Check held item capacity (max 3)
            var heldItems = GetHeldItems(characterId);
            if (heldItems.Count >= 3)
            {
                _log.Warning("Cannot craft - at max held item capacity: CharacterID={CharacterId}", characterId);
                return new CraftResult
                {
                    Success = false,
                    Message = "Cannot hold more than 3 field-crafted items! Use or discard existing items first."
                };
            }

            // Consume components
            ConsumeItem(characterId, "Common Herb", 1);
            ConsumeItem(characterId, "Clean Cloth", 1);

            // Determine item name and effect based on type and effective rank
            var (itemName, itemEffect) = (type, effectiveRank) switch
            {
                (ConcoctionType.Poultice, 1) => ("Crude Poultice", "Restore 2d6 HP"),
                (ConcoctionType.Poultice, 2) => ("Refined Poultice", "Restore 4d6 HP"),
                (ConcoctionType.Poultice, 3) => ("Superior Poultice", "Restore 6d6 HP + remove [Bleeding]"),
                (ConcoctionType.Stimulant, 1) => ("Weak Stimulant", "Restore 15 Stamina"),
                (ConcoctionType.Stimulant, 2) => ("Potent Stimulant", "Restore 30 Stamina"),
                (ConcoctionType.Stimulant, 3) => ("Exceptional Stimulant", "Restore 45 Stamina + remove [Exhausted]"),
                _ => ("Crude Poultice", "Restore 2d6 HP")
            };

            // Add to held items
            AddHeldItem(characterId, itemName, itemEffect);

            // Update crafting statistics
            IncrementCraftingStats(characterId);

            _log.Information(
                "Basic Concoction crafted successfully: CharacterID={CharacterId}, Item={ItemName}, Effect={Effect}",
                characterId, itemName, itemEffect);

            return new CraftResult
            {
                Success = true,
                ItemName = itemName,
                ItemEffect = itemEffect,
                Message = $"Crafted [{itemName}]: {itemEffect}"
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to craft Basic Concoction: CharacterID={CharacterId}", characterId);
            throw;
        }
    }

    /// <summary>
    /// Use a held field-crafted item (Free Action)
    /// </summary>
    public (bool success, string effect, string message) UseHeldItem(int characterId, string itemName)
    {
        try
        {
            _log.Information("Using held item: CharacterID={CharacterId}, ItemName={ItemName}", characterId, itemName);

            var heldItems = GetHeldItems(characterId);
            var item = heldItems.FirstOrDefault(i => i.Name == itemName);

            if (item == null)
            {
                _log.Warning("Held item not found: CharacterID={CharacterId}, ItemName={ItemName}", characterId, itemName);
                return (false, string.Empty, $"You don't have a [{itemName}] in your field kit!");
            }

            // Remove the used item
            RemoveHeldItem(characterId, itemName);

            _log.Information("Held item used successfully: CharacterID={CharacterId}, ItemName={ItemName}, Effect={Effect}",
                characterId, itemName, item.Effect);

            return (true, item.Effect, $"Used [{itemName}]: {item.Effect}");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to use held item: CharacterID={CharacterId}, ItemName={ItemName}", characterId, itemName);
            throw;
        }
    }

    #endregion

    #region Tier 1: Improvised Trap

    /// <summary>
    /// Place an improvised trap on the battlefield
    /// RANK 1: [Rooted] for 1 turn
    /// RANK 2: [Rooted] for 2 turns
    /// RANK 3: [Rooted] 2 turns + [Bleeding] (1d4 per turn for 3 turns)
    /// </summary>
    public TrapResult PlaceImprovisedTrap(
        int characterId,
        int targetTileX,
        int targetTileY,
        int rank = 1,
        int masterImproviserRank = 0)
    {
        try
        {
            _log.Information(
                "Placing Improvised Trap: CharacterID={CharacterId}, Location=({X},{Y}), Rank={Rank}, MasterImproviserRank={MasterImproviserRank}",
                characterId, targetTileX, targetTileY, rank, masterImproviserRank);

            // Master Improviser automatically upgrades to Rank 3 effects
            int effectiveRank = masterImproviserRank >= 1 ? 3 : rank;

            // Check if character has required components
            bool hasMetal = CheckInventoryForItem(characterId, "Scrap Metal");
            bool hasLeather = CheckInventoryForItem(characterId, "Tough Leather");

            if (!hasMetal || !hasLeather)
            {
                _log.Warning(
                    "Missing trap components: CharacterID={CharacterId}, HasMetal={HasMetal}, HasLeather={HasLeather}",
                    characterId, hasMetal, hasLeather);
                return new TrapResult
                {
                    Success = false,
                    Message = "Missing components: Need 1 [Scrap Metal] + 1 [Tough Leather]"
                };
            }

            // Consume components
            ConsumeItem(characterId, "Scrap Metal", 1);
            ConsumeItem(characterId, "Tough Leather", 1);

            // Create trap in database
            int trapId = CreateBattlefieldTrap(characterId, targetTileX, targetTileY, effectiveRank);

            // Update trap statistics
            IncrementTrapStats(characterId);

            var trapEffect = effectiveRank switch
            {
                1 => "[Rooted] for 1 turn",
                2 => "[Rooted] for 2 turns",
                3 => "[Rooted] for 2 turns + [Bleeding] (1d4 per turn for 3 turns)",
                _ => "[Rooted] for 1 turn"
            };

            _log.Information(
                "Improvised Trap placed successfully: CharacterID={CharacterId}, TrapID={TrapId}, Location=({X},{Y}), Effect={Effect}",
                characterId, trapId, targetTileX, targetTileY, trapEffect);

            return new TrapResult
            {
                Success = true,
                TrapId = trapId,
                Message = $"Improvised Trap set at ({targetTileX},{targetTileY})! First enemy to step here: {trapEffect}"
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to place Improvised Trap: CharacterID={CharacterId}", characterId);
            throw;
        }
    }

    #endregion

    #region Tier 2: Resourceful Eye

    /// <summary>
    /// Reveal hidden resource nodes in current room
    /// RANK 1: DC 12 WITS + Wasteland Survival check
    /// RANK 2: DC 10
    /// RANK 3: DC 10 + also reveals hidden passages and traps
    /// </summary>
    public (bool success, int resourcesFound, bool foundSecrets, string message) ExecuteResourcefulEye(
        int characterId,
        int roomId,
        int rank = 1,
        DiceService? diceService = null)
    {
        try
        {
            _log.Information(
                "Executing Resourceful Eye: CharacterID={CharacterId}, RoomID={RoomId}, Rank={Rank}",
                characterId, roomId, rank);

            // Check if already used in this room
            if (HasUsedResourcefulEyeInRoom(characterId, roomId))
            {
                _log.Warning("Resourceful Eye already used in this room: CharacterID={CharacterId}, RoomID={RoomId}",
                    characterId, roomId);
                return (false, 0, false, "You've already scoured this room thoroughly. Nothing more to find.");
            }

            // Determine DC based on rank
            int dc = rank >= 2 ? 10 : 12;

            // Simulate WITS + Wasteland Survival check (would normally use character's WITS)
            // For now, assume success
            bool checkSuccess = true;

            if (!checkSuccess)
            {
                _log.Information("Resourceful Eye check failed: CharacterID={CharacterId}, RoomID={RoomId}, DC={DC}",
                    characterId, roomId, dc);
                return (false, 0, false, $"Failed to spot hidden resources (DC {dc} check failed).");
            }

            // Reveal hidden resource nodes
            int resourcesFound = RevealHiddenResourceNodes(roomId);

            // Rank 3: Also reveal hidden passages and traps
            bool foundSecrets = false;
            if (rank >= 3)
            {
                foundSecrets = RevealHiddenPassagesAndTraps(roomId);
            }

            // Mark room as searched
            MarkRoomSearched(characterId, roomId);

            _log.Information(
                "Resourceful Eye executed successfully: CharacterID={CharacterId}, RoomID={RoomId}, ResourcesFound={Resources}, SecretsFound={Secrets}",
                characterId, roomId, resourcesFound, foundSecrets);

            string message = $"Found {resourcesFound} hidden resource node(s)!";
            if (foundSecrets)
            {
                message += " Also revealed hidden passages and traps!";
            }

            return (true, resourcesFound, foundSecrets, message);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to execute Resourceful Eye: CharacterID={CharacterId}", characterId);
            throw;
        }
    }

    #endregion

    #region Capstone: Blight Haven

    /// <summary>
    /// Designate a cleared room as a Blight Haven (guaranteed safe rest location)
    /// RANK 1: 0% Ambush, +10 recovery
    /// RANK 2: 0% Ambush, +20 recovery
    /// RANK 3: 0% Ambush, +30 recovery + advanced crafting allowed
    /// </summary>
    public (bool success, string message) DesignateBlightHaven(
        int characterId,
        int roomId,
        int rank = 1)
    {
        try
        {
            _log.Information(
                "Attempting to designate Blight Haven: CharacterID={CharacterId}, RoomID={RoomId}, Rank={Rank}",
                characterId, roomId, rank);

            // Check if already used this expedition
            if (HasUsedBlightHavenThisExpedition(characterId))
            {
                _log.Warning("Blight Haven already used this expedition: CharacterID={CharacterId}", characterId);
                return (false, "You've already created a Blight Haven this expedition. One per expedition only!");
            }

            // Designate room as Blight Haven
            int recoveryBonus = rank switch
            {
                1 => 10,
                2 => 20,
                3 => 30,
                _ => 10
            };

            bool allowsAdvancedCrafting = rank >= 3;

            CreateBlightHaven(characterId, roomId, recoveryBonus, allowsAdvancedCrafting);

            _log.Information(
                "BLIGHT HAVEN DESIGNATED: CharacterID={CharacterId}, RoomID={RoomId}, RecoveryBonus={Bonus}, AllowsCrafting={Crafting}",
                characterId, roomId, recoveryBonus, allowsAdvancedCrafting);

            string message = $"This room is now a [Blight Haven]! (0% Ambush, +{recoveryBonus} recovery, no Psychic Stress gain";
            if (allowsAdvancedCrafting)
            {
                message += ", advanced crafting allowed";
            }
            message += ")";

            return (true, message);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to designate Blight Haven: CharacterID={CharacterId}", characterId);
            throw;
        }
    }

    /// <summary>
    /// Reset Blight Haven usage at the start of a new expedition
    /// </summary>
    public void ResetBlightHavenForNewExpedition(int characterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                UPDATE Characters_FieldCrafting
                SET blight_haven_used_this_expedition = 0,
                    blight_haven_room_id = NULL,
                    resourceful_eye_rooms_this_expedition = '[]',
                    updated_at = @Timestamp
                WHERE character_id = @CharacterId";

            cmd.Parameters.AddWithValue("@CharacterId", characterId);
            cmd.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

            cmd.ExecuteNonQuery();

            _log.Information("Blight Haven usage reset for new expedition: CharacterID={CharacterId}", characterId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to reset Blight Haven: CharacterID={CharacterId}", characterId);
            throw;
        }
    }

    #endregion

    #region Helper Methods - Database Operations

    /// <summary>
    /// Initialize field crafting data for a character
    /// </summary>
    public void InitializeFieldCraftingData(int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT OR IGNORE INTO Characters_FieldCrafting
            (character_id, held_item_1, held_item_2, held_item_3,
             total_items_crafted, total_traps_placed,
             blight_haven_used_this_expedition, blight_haven_room_id,
             resourceful_eye_rooms_this_expedition)
            VALUES (@CharacterId, NULL, NULL, NULL, 0, 0, 0, NULL, '[]')";

        cmd.Parameters.AddWithValue("@CharacterId", characterId);
        cmd.ExecuteNonQuery();

        _log.Debug("Field crafting data initialized: CharacterID={CharacterId}", characterId);
    }

    /// <summary>
    /// Get all held items for a character
    /// </summary>
    private List<HeldItem> GetHeldItems(int characterId)
    {
        var items = new List<HeldItem>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT held_item_1, held_item_2, held_item_3
            FROM Characters_FieldCrafting
            WHERE character_id = @CharacterId";

        cmd.Parameters.AddWithValue("@CharacterId", characterId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            for (int i = 0; i < 3; i++)
            {
                if (!reader.IsDBNull(i))
                {
                    string itemData = reader.GetString(i);
                    var parts = itemData.Split('|');
                    if (parts.Length == 2)
                    {
                        items.Add(new HeldItem { Name = parts[0], Effect = parts[1] });
                    }
                }
            }
        }

        return items;
    }

    /// <summary>
    /// Add a held item to the character's field kit
    /// </summary>
    private void AddHeldItem(int characterId, string itemName, string itemEffect)
    {
        string itemData = $"{itemName}|{itemEffect}";

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Find first empty slot
        using var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = @"
            SELECT held_item_1, held_item_2, held_item_3
            FROM Characters_FieldCrafting
            WHERE character_id = @CharacterId";
        selectCmd.Parameters.AddWithValue("@CharacterId", characterId);

        string? slotToUpdate = null;
        using (var reader = selectCmd.ExecuteReader())
        {
            if (reader.Read())
            {
                if (reader.IsDBNull(0)) slotToUpdate = "held_item_1";
                else if (reader.IsDBNull(1)) slotToUpdate = "held_item_2";
                else if (reader.IsDBNull(2)) slotToUpdate = "held_item_3";
            }
        }

        if (slotToUpdate != null)
        {
            using var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = $@"
                UPDATE Characters_FieldCrafting
                SET {slotToUpdate} = @ItemData,
                    updated_at = @Timestamp
                WHERE character_id = @CharacterId";
            updateCmd.Parameters.AddWithValue("@ItemData", itemData);
            updateCmd.Parameters.AddWithValue("@CharacterId", characterId);
            updateCmd.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            updateCmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Remove a held item from the character's field kit
    /// </summary>
    private void RemoveHeldItem(int characterId, string itemName)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Find and clear the item
        using var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = @"
            SELECT held_item_1, held_item_2, held_item_3
            FROM Characters_FieldCrafting
            WHERE character_id = @CharacterId";
        selectCmd.Parameters.AddWithValue("@CharacterId", characterId);

        string? slotToUpdate = null;
        using (var reader = selectCmd.ExecuteReader())
        {
            if (reader.Read())
            {
                for (int i = 0; i < 3; i++)
                {
                    if (!reader.IsDBNull(i))
                    {
                        string itemData = reader.GetString(i);
                        if (itemData.StartsWith(itemName + "|"))
                        {
                            slotToUpdate = $"held_item_{i + 1}";
                            break;
                        }
                    }
                }
            }
        }

        if (slotToUpdate != null)
        {
            using var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = $@"
                UPDATE Characters_FieldCrafting
                SET {slotToUpdate} = NULL,
                    updated_at = @Timestamp
                WHERE character_id = @CharacterId";
            updateCmd.Parameters.AddWithValue("@CharacterId", characterId);
            updateCmd.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            updateCmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Check if character has an item in inventory (simplified - would query actual inventory table)
    /// </summary>
    private bool CheckInventoryForItem(int characterId, string itemName)
    {
        // Simplified implementation - in real system would query inventory
        // For now, assume player always has components (to be implemented later)
        return true;
    }

    /// <summary>
    /// Consume an item from inventory (simplified)
    /// </summary>
    private void ConsumeItem(int characterId, string itemName, int quantity)
    {
        // Simplified implementation - in real system would update inventory
        _log.Debug("Consuming item: CharacterID={CharacterId}, Item={ItemName}, Quantity={Quantity}",
            characterId, itemName, quantity);
    }

    /// <summary>
    /// Increment crafting statistics
    /// </summary>
    private void IncrementCraftingStats(int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE Characters_FieldCrafting
            SET total_items_crafted = total_items_crafted + 1,
                updated_at = @Timestamp
            WHERE character_id = @CharacterId";

        cmd.Parameters.AddWithValue("@CharacterId", characterId);
        cmd.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Increment trap placement statistics
    /// </summary>
    private void IncrementTrapStats(int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE Characters_FieldCrafting
            SET total_traps_placed = total_traps_placed + 1,
                updated_at = @Timestamp
            WHERE character_id = @CharacterId";

        cmd.Parameters.AddWithValue("@CharacterId", characterId);
        cmd.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Create a battlefield trap (simplified)
    /// </summary>
    private int CreateBattlefieldTrap(int characterId, int tileX, int tileY, int rank)
    {
        // Simplified implementation - in real system would create trap in Battlefield_Traps table
        // Return mock trap ID
        _log.Debug("Creating battlefield trap: CharacterID={CharacterId}, Location=({X},{Y}), Rank={Rank}",
            characterId, tileX, tileY, rank);
        return new Random().Next(1000, 9999);
    }

    /// <summary>
    /// Check if Resourceful Eye has been used in this room
    /// </summary>
    private bool HasUsedResourcefulEyeInRoom(int characterId, int roomId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT resourceful_eye_rooms_this_expedition
            FROM Characters_FieldCrafting
            WHERE character_id = @CharacterId";

        cmd.Parameters.AddWithValue("@CharacterId", characterId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read() && !reader.IsDBNull(0))
        {
            string roomsJson = reader.GetString(0);
            // Simple JSON array parsing
            return roomsJson.Contains($"\"{roomId}\"") || roomsJson.Contains($"{roomId}");
        }

        return false;
    }

    /// <summary>
    /// Mark room as searched with Resourceful Eye
    /// </summary>
    private void MarkRoomSearched(int characterId, int roomId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE Characters_FieldCrafting
            SET resourceful_eye_rooms_this_expedition =
                json_insert(COALESCE(resourceful_eye_rooms_this_expedition, '[]'), '$[#]', @RoomId),
                updated_at = @Timestamp
            WHERE character_id = @CharacterId";

        cmd.Parameters.AddWithValue("@CharacterId", characterId);
        cmd.Parameters.AddWithValue("@RoomId", roomId);
        cmd.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Reveal hidden resource nodes in a room (simplified)
    /// </summary>
    private int RevealHiddenResourceNodes(int roomId)
    {
        // Simplified implementation - in real system would query Rooms/ResourceNodes table
        _log.Debug("Revealing hidden resource nodes: RoomID={RoomId}", roomId);
        return new Random().Next(1, 4); // 1-3 resources found
    }

    /// <summary>
    /// Reveal hidden passages and traps in a room (simplified)
    /// </summary>
    private bool RevealHiddenPassagesAndTraps(int roomId)
    {
        // Simplified implementation
        _log.Debug("Revealing hidden passages and traps: RoomID={RoomId}", roomId);
        return true;
    }

    /// <summary>
    /// Check if Blight Haven has been used this expedition
    /// </summary>
    private bool HasUsedBlightHavenThisExpedition(int characterId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT blight_haven_used_this_expedition
            FROM Characters_FieldCrafting
            WHERE character_id = @CharacterId";

        cmd.Parameters.AddWithValue("@CharacterId", characterId);

        var result = cmd.ExecuteScalar();
        return result != null && Convert.ToInt32(result) == 1;
    }

    /// <summary>
    /// Create Blight Haven in specified room
    /// </summary>
    private void CreateBlightHaven(int characterId, int roomId, int recoveryBonus, bool allowsAdvancedCrafting)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Mark character as having used Blight Haven
        using var charCmd = connection.CreateCommand();
        charCmd.CommandText = @"
            UPDATE Characters_FieldCrafting
            SET blight_haven_used_this_expedition = 1,
                blight_haven_room_id = @RoomId,
                updated_at = @Timestamp
            WHERE character_id = @CharacterId";

        charCmd.Parameters.AddWithValue("@CharacterId", characterId);
        charCmd.Parameters.AddWithValue("@RoomId", roomId);
        charCmd.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

        charCmd.ExecuteNonQuery();

        // In real system, would also update Rooms table to set:
        // - ambush_chance = 0
        // - is_blight_haven = 1
        // - rest_recovery_bonus = recoveryBonus
        // - blocks_ambient_psychic_stress = 1
        // - allows_advanced_crafting = allowsAdvancedCrafting
    }

    #endregion
}
