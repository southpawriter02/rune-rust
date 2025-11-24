using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using System.Text.Json;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.41: Repository for managing cosmetic customization
/// </summary>
public class CosmeticRepository
{
    private static readonly ILogger _log = Log.ForContext<CosmeticRepository>();
    private readonly string _connectionString;

    public CosmeticRepository(string connectionString)
    {
        _connectionString = connectionString;
        _log.Debug("CosmeticRepository initialized");
        InitializeTables();
    }

    #region Table Initialization

    private void InitializeTables()
    {
        _log.Debug("Initializing cosmetic tables");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Cosmetics table (cosmetic definitions)
        var createCosmeticsTable = connection.CreateCommand();
        createCosmeticsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Cosmetics (
                cosmetic_id TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                cosmetic_type TEXT NOT NULL,
                description TEXT NOT NULL,
                preview_image_url TEXT NOT NULL,
                unlock_requirement TEXT NOT NULL,
                parameters_json TEXT DEFAULT '{}'
            )
        ";
        createCosmeticsTable.ExecuteNonQuery();

        // Cosmetic_Progress table (unlock progress per account)
        var createProgressTable = connection.CreateCommand();
        createProgressTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Cosmetic_Progress (
                progress_id INTEGER PRIMARY KEY AUTOINCREMENT,
                account_id INTEGER NOT NULL,
                cosmetic_id TEXT NOT NULL,
                is_unlocked INTEGER DEFAULT 0,
                unlocked_at TEXT,
                FOREIGN KEY (account_id) REFERENCES Account_Progression(account_id),
                FOREIGN KEY (cosmetic_id) REFERENCES Cosmetics(cosmetic_id),
                UNIQUE (account_id, cosmetic_id)
            )
        ";
        createProgressTable.ExecuteNonQuery();

        // Cosmetic_Loadouts table (player's cosmetic loadouts)
        var createLoadoutsTable = connection.CreateCommand();
        createLoadoutsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Cosmetic_Loadouts (
                loadout_id INTEGER PRIMARY KEY AUTOINCREMENT,
                account_id INTEGER NOT NULL,
                loadout_name TEXT NOT NULL,
                selected_title TEXT,
                selected_portrait TEXT,
                selected_ui_theme TEXT,
                selected_frame TEXT,
                selected_emblem TEXT,
                ability_vfx_overrides_json TEXT DEFAULT '{}',
                combat_log_style TEXT,
                is_active INTEGER DEFAULT 0,
                FOREIGN KEY (account_id) REFERENCES Account_Progression(account_id)
            )
        ";
        createLoadoutsTable.ExecuteNonQuery();

        _log.Information("Cosmetic tables initialized");
    }

    #endregion

    #region Cosmetic CRUD

    /// <summary>
    /// Get cosmetic by ID
    /// </summary>
    public Cosmetic? GetById(string cosmeticId)
    {
        _log.Debug("Getting cosmetic: CosmeticID={CosmeticId}", cosmeticId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Cosmetics
            WHERE cosmetic_id = @CosmeticId
        ";
        command.Parameters.AddWithValue("@CosmeticId", cosmeticId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapCosmeticFromReader(reader);
        }

        _log.Warning("Cosmetic not found: {CosmeticId}", cosmeticId);
        return null;
    }

    /// <summary>
    /// Get all cosmetics
    /// </summary>
    public List<Cosmetic> GetAll()
    {
        _log.Debug("Getting all cosmetics");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Cosmetics ORDER BY cosmetic_type, name";

        var cosmetics = new List<Cosmetic>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            cosmetics.Add(MapCosmeticFromReader(reader));
        }

        _log.Information("Retrieved {Count} cosmetics", cosmetics.Count);
        return cosmetics;
    }

    /// <summary>
    /// Get cosmetics by type
    /// </summary>
    public List<Cosmetic> GetByType(CosmeticType type)
    {
        _log.Debug("Getting cosmetics by type: {Type}", type);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Cosmetics
            WHERE cosmetic_type = @Type
            ORDER BY name
        ";
        command.Parameters.AddWithValue("@Type", type.ToString());

        var cosmetics = new List<Cosmetic>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            cosmetics.Add(MapCosmeticFromReader(reader));
        }

        _log.Information("Retrieved {Count} cosmetics of type {Type}",
            cosmetics.Count, type);
        return cosmetics;
    }

    #endregion

    #region Cosmetic Progress

    /// <summary>
    /// Get cosmetic progress for account
    /// </summary>
    public List<CosmeticProgress> GetProgress(int accountId)
    {
        _log.Debug("Getting cosmetic progress: AccountID={AccountId}", accountId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Cosmetic_Progress
            WHERE account_id = @AccountId
        ";
        command.Parameters.AddWithValue("@AccountId", accountId);

        var progressList = new List<CosmeticProgress>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            progressList.Add(MapCosmeticProgressFromReader(reader));
        }

        return progressList;
    }

    /// <summary>
    /// Check if cosmetic is unlocked
    /// </summary>
    public bool IsUnlocked(int accountId, string cosmeticId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT is_unlocked
            FROM Cosmetic_Progress
            WHERE account_id = @AccountId AND cosmetic_id = @CosmeticId
        ";
        command.Parameters.AddWithValue("@AccountId", accountId);
        command.Parameters.AddWithValue("@CosmeticId", cosmeticId);

        var result = command.ExecuteScalar();
        return result != null && Convert.ToInt32(result) == 1;
    }

    /// <summary>
    /// Unlock cosmetic for account
    /// </summary>
    public void UnlockCosmetic(int accountId, string cosmeticId)
    {
        _log.Information("Unlocking cosmetic: AccountID={AccountId}, CosmeticID={CosmeticId}",
            accountId, cosmeticId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO Cosmetic_Progress
                (account_id, cosmetic_id, is_unlocked, unlocked_at)
            VALUES (@AccountId, @CosmeticId, 1, @UnlockedAt)
        ";
        command.Parameters.AddWithValue("@AccountId", accountId);
        command.Parameters.AddWithValue("@CosmeticId", cosmeticId);
        command.Parameters.AddWithValue("@UnlockedAt", DateTime.UtcNow.ToString("O"));

        command.ExecuteNonQuery();

        _log.Information("Cosmetic unlocked successfully");
    }

    /// <summary>
    /// Get unlocked cosmetics for account
    /// </summary>
    public List<Cosmetic> GetUnlockedCosmetics(int accountId)
    {
        _log.Debug("Getting unlocked cosmetics: AccountID={AccountId}", accountId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT c.*
            FROM Cosmetics c
            INNER JOIN Cosmetic_Progress p
                ON c.cosmetic_id = p.cosmetic_id
            WHERE p.account_id = @AccountId AND p.is_unlocked = 1
            ORDER BY c.cosmetic_type, c.name
        ";
        command.Parameters.AddWithValue("@AccountId", accountId);

        var cosmetics = new List<Cosmetic>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            cosmetics.Add(MapCosmeticFromReader(reader));
        }

        _log.Information("Retrieved {Count} unlocked cosmetics for account {AccountId}",
            cosmetics.Count, accountId);
        return cosmetics;
    }

    #endregion

    #region Cosmetic Loadouts

    /// <summary>
    /// Get loadout by ID
    /// </summary>
    public CosmeticLoadout? GetLoadoutById(int loadoutId)
    {
        _log.Debug("Getting cosmetic loadout: LoadoutID={LoadoutId}", loadoutId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Cosmetic_Loadouts
            WHERE loadout_id = @LoadoutId
        ";
        command.Parameters.AddWithValue("@LoadoutId", loadoutId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapLoadoutFromReader(reader);
        }

        return null;
    }

    /// <summary>
    /// Get all loadouts for account
    /// </summary>
    public List<CosmeticLoadout> GetLoadouts(int accountId)
    {
        _log.Debug("Getting cosmetic loadouts: AccountID={AccountId}", accountId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Cosmetic_Loadouts
            WHERE account_id = @AccountId
            ORDER BY is_active DESC, loadout_name
        ";
        command.Parameters.AddWithValue("@AccountId", accountId);

        var loadouts = new List<CosmeticLoadout>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            loadouts.Add(MapLoadoutFromReader(reader));
        }

        return loadouts;
    }

    /// <summary>
    /// Get active loadout for account
    /// </summary>
    public CosmeticLoadout? GetActiveLoadout(int accountId)
    {
        _log.Debug("Getting active cosmetic loadout: AccountID={AccountId}", accountId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Cosmetic_Loadouts
            WHERE account_id = @AccountId AND is_active = 1
            LIMIT 1
        ";
        command.Parameters.AddWithValue("@AccountId", accountId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapLoadoutFromReader(reader);
        }

        return null;
    }

    /// <summary>
    /// Create new cosmetic loadout
    /// </summary>
    public int CreateLoadout(CosmeticLoadout loadout)
    {
        _log.Information("Creating cosmetic loadout: AccountID={AccountId}, Name={Name}",
            loadout.AccountId, loadout.LoadoutName);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var vfxJson = JsonSerializer.Serialize(loadout.AbilityVFXOverrides);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Cosmetic_Loadouts
                (account_id, loadout_name, selected_title, selected_portrait,
                 selected_ui_theme, selected_frame, selected_emblem,
                 ability_vfx_overrides_json, combat_log_style, is_active)
            VALUES (@AccountId, @Name, @Title, @Portrait, @UITheme, @Frame, @Emblem,
                    @VFXJson, @LogStyle, @IsActive);
            SELECT last_insert_rowid();
        ";
        command.Parameters.AddWithValue("@AccountId", loadout.AccountId);
        command.Parameters.AddWithValue("@Name", loadout.LoadoutName);
        command.Parameters.AddWithValue("@Title", (object?)loadout.SelectedTitle ?? DBNull.Value);
        command.Parameters.AddWithValue("@Portrait", (object?)loadout.SelectedPortrait ?? DBNull.Value);
        command.Parameters.AddWithValue("@UITheme", (object?)loadout.SelectedUITheme ?? DBNull.Value);
        command.Parameters.AddWithValue("@Frame", (object?)loadout.SelectedCharacterFrame ?? DBNull.Value);
        command.Parameters.AddWithValue("@Emblem", (object?)loadout.SelectedEmblem ?? DBNull.Value);
        command.Parameters.AddWithValue("@VFXJson", vfxJson);
        command.Parameters.AddWithValue("@LogStyle", (object?)loadout.CombatLogStyle ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", loadout.IsActive ? 1 : 0);

        var loadoutId = Convert.ToInt32(command.ExecuteScalar());

        _log.Information("Cosmetic loadout created: LoadoutID={LoadoutId}", loadoutId);
        return loadoutId;
    }

    /// <summary>
    /// Update cosmetic loadout
    /// </summary>
    public void UpdateLoadout(CosmeticLoadout loadout)
    {
        _log.Information("Updating cosmetic loadout: LoadoutID={LoadoutId}", loadout.LoadoutId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var vfxJson = JsonSerializer.Serialize(loadout.AbilityVFXOverrides);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Cosmetic_Loadouts
            SET loadout_name = @Name,
                selected_title = @Title,
                selected_portrait = @Portrait,
                selected_ui_theme = @UITheme,
                selected_frame = @Frame,
                selected_emblem = @Emblem,
                ability_vfx_overrides_json = @VFXJson,
                combat_log_style = @LogStyle,
                is_active = @IsActive
            WHERE loadout_id = @LoadoutId
        ";
        command.Parameters.AddWithValue("@LoadoutId", loadout.LoadoutId);
        command.Parameters.AddWithValue("@Name", loadout.LoadoutName);
        command.Parameters.AddWithValue("@Title", (object?)loadout.SelectedTitle ?? DBNull.Value);
        command.Parameters.AddWithValue("@Portrait", (object?)loadout.SelectedPortrait ?? DBNull.Value);
        command.Parameters.AddWithValue("@UITheme", (object?)loadout.SelectedUITheme ?? DBNull.Value);
        command.Parameters.AddWithValue("@Frame", (object?)loadout.SelectedCharacterFrame ?? DBNull.Value);
        command.Parameters.AddWithValue("@Emblem", (object?)loadout.SelectedEmblem ?? DBNull.Value);
        command.Parameters.AddWithValue("@VFXJson", vfxJson);
        command.Parameters.AddWithValue("@LogStyle", (object?)loadout.CombatLogStyle ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", loadout.IsActive ? 1 : 0);

        command.ExecuteNonQuery();

        _log.Information("Cosmetic loadout updated");
    }

    /// <summary>
    /// Set active loadout for account
    /// </summary>
    public void SetActiveLoadout(int accountId, int loadoutId)
    {
        _log.Information("Setting active cosmetic loadout: AccountID={AccountId}, LoadoutID={LoadoutId}",
            accountId, loadoutId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Deactivate all loadouts for account
        var deactivateCommand = connection.CreateCommand();
        deactivateCommand.CommandText = @"
            UPDATE Cosmetic_Loadouts
            SET is_active = 0
            WHERE account_id = @AccountId
        ";
        deactivateCommand.Parameters.AddWithValue("@AccountId", accountId);
        deactivateCommand.ExecuteNonQuery();

        // Activate selected loadout
        var activateCommand = connection.CreateCommand();
        activateCommand.CommandText = @"
            UPDATE Cosmetic_Loadouts
            SET is_active = 1
            WHERE loadout_id = @LoadoutId AND account_id = @AccountId
        ";
        activateCommand.Parameters.AddWithValue("@LoadoutId", loadoutId);
        activateCommand.Parameters.AddWithValue("@AccountId", accountId);
        activateCommand.ExecuteNonQuery();

        _log.Information("Active cosmetic loadout set");
    }

    #endregion

    #region Mapping Helpers

    private Cosmetic MapCosmeticFromReader(SqliteDataReader reader)
    {
        var parametersJson = reader.GetString(reader.GetOrdinal("parameters_json"));
        var parameters = JsonSerializer.Deserialize<Dictionary<string, string>>(parametersJson)
            ?? new Dictionary<string, string>();

        return new Cosmetic
        {
            CosmeticId = reader.GetString(reader.GetOrdinal("cosmetic_id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            Type = Enum.Parse<CosmeticType>(reader.GetString(reader.GetOrdinal("cosmetic_type"))),
            Description = reader.GetString(reader.GetOrdinal("description")),
            PreviewImageUrl = reader.GetString(reader.GetOrdinal("preview_image_url")),
            UnlockRequirement = reader.GetString(reader.GetOrdinal("unlock_requirement")),
            Parameters = parameters
        };
    }

    private CosmeticProgress MapCosmeticProgressFromReader(SqliteDataReader reader)
    {
        DateTime? unlockedAt = null;
        if (!reader.IsDBNull(reader.GetOrdinal("unlocked_at")))
        {
            unlockedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("unlocked_at")));
        }

        return new CosmeticProgress
        {
            ProgressId = reader.GetInt32(reader.GetOrdinal("progress_id")),
            AccountId = reader.GetInt32(reader.GetOrdinal("account_id")),
            CosmeticId = reader.GetString(reader.GetOrdinal("cosmetic_id")),
            IsUnlocked = reader.GetInt32(reader.GetOrdinal("is_unlocked")) == 1,
            UnlockedAt = unlockedAt
        };
    }

    private CosmeticLoadout MapLoadoutFromReader(SqliteDataReader reader)
    {
        var vfxJson = reader.GetString(reader.GetOrdinal("ability_vfx_overrides_json"));
        var vfxOverrides = JsonSerializer.Deserialize<Dictionary<string, string>>(vfxJson)
            ?? new Dictionary<string, string>();

        return new CosmeticLoadout
        {
            LoadoutId = reader.GetInt32(reader.GetOrdinal("loadout_id")),
            AccountId = reader.GetInt32(reader.GetOrdinal("account_id")),
            LoadoutName = reader.GetString(reader.GetOrdinal("loadout_name")),
            SelectedTitle = reader.IsDBNull(reader.GetOrdinal("selected_title"))
                ? null : reader.GetString(reader.GetOrdinal("selected_title")),
            SelectedPortrait = reader.IsDBNull(reader.GetOrdinal("selected_portrait"))
                ? null : reader.GetString(reader.GetOrdinal("selected_portrait")),
            SelectedUITheme = reader.IsDBNull(reader.GetOrdinal("selected_ui_theme"))
                ? null : reader.GetString(reader.GetOrdinal("selected_ui_theme")),
            SelectedCharacterFrame = reader.IsDBNull(reader.GetOrdinal("selected_frame"))
                ? null : reader.GetString(reader.GetOrdinal("selected_frame")),
            SelectedEmblem = reader.IsDBNull(reader.GetOrdinal("selected_emblem"))
                ? null : reader.GetString(reader.GetOrdinal("selected_emblem")),
            AbilityVFXOverrides = vfxOverrides,
            CombatLogStyle = reader.IsDBNull(reader.GetOrdinal("combat_log_style"))
                ? null : reader.GetString(reader.GetOrdinal("combat_log_style")),
            IsActive = reader.GetInt32(reader.GetOrdinal("is_active")) == 1
        };
    }

    #endregion
}
