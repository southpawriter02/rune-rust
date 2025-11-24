using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using System.Text.Json;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.41: Repository for managing account-wide progression data
/// Handles account progression, unlocks, milestones, and statistics
/// </summary>
public class AccountProgressionRepository
{
    private static readonly ILogger _log = Log.ForContext<AccountProgressionRepository>();
    private readonly string _connectionString;

    public AccountProgressionRepository(string connectionString)
    {
        _connectionString = connectionString;
        _log.Debug("AccountProgressionRepository initialized");
        InitializeTables();
    }

    #region Table Initialization

    /// <summary>
    /// Initialize all account progression tables
    /// </summary>
    private void InitializeTables()
    {
        _log.Debug("Initializing account progression tables");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Account_Progression table
        var createProgressionTable = connection.CreateCommand();
        createProgressionTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Account_Progression (
                account_id INTEGER PRIMARY KEY AUTOINCREMENT,
                total_achievement_points INTEGER DEFAULT 0,
                current_milestone_tier INTEGER DEFAULT 1,
                created_at TEXT NOT NULL,
                updated_at TEXT NOT NULL,
                total_characters_created INTEGER DEFAULT 0,
                total_campaigns_completed INTEGER DEFAULT 0,
                total_bosses_defeated INTEGER DEFAULT 0,
                total_achievements_unlocked INTEGER DEFAULT 0,
                highest_new_game_plus_tier INTEGER DEFAULT 0,
                highest_endless_wave INTEGER DEFAULT 0
            )
        ";
        createProgressionTable.ExecuteNonQuery();

        // Account_Unlocks table (unlock definitions)
        var createUnlocksTable = connection.CreateCommand();
        createUnlocksTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Account_Unlocks (
                unlock_id TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                unlock_type TEXT NOT NULL,
                description TEXT NOT NULL,
                requirement_description TEXT NOT NULL,
                parameters_json TEXT DEFAULT '{}'
            )
        ";
        createUnlocksTable.ExecuteNonQuery();

        // Account_Unlock_Progress table (unlock progress per account)
        var createUnlockProgressTable = connection.CreateCommand();
        createUnlockProgressTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Account_Unlock_Progress (
                progress_id INTEGER PRIMARY KEY AUTOINCREMENT,
                account_id INTEGER NOT NULL,
                unlock_id TEXT NOT NULL,
                is_unlocked INTEGER DEFAULT 0,
                unlocked_at TEXT,
                FOREIGN KEY (account_id) REFERENCES Account_Progression(account_id),
                FOREIGN KEY (unlock_id) REFERENCES Account_Unlocks(unlock_id),
                UNIQUE (account_id, unlock_id)
            )
        ";
        createUnlockProgressTable.ExecuteNonQuery();

        // Milestone_Tiers table
        var createMilestoneTiersTable = connection.CreateCommand();
        createMilestoneTiersTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Milestone_Tiers (
                tier_number INTEGER PRIMARY KEY,
                tier_name TEXT NOT NULL,
                description TEXT NOT NULL,
                required_achievement_points INTEGER NOT NULL,
                unlock_rewards_json TEXT DEFAULT '[]',
                cosmetic_rewards_json TEXT DEFAULT '[]',
                alternative_start_unlock TEXT
            )
        ";
        createMilestoneTiersTable.ExecuteNonQuery();

        // Account_Milestone_Progress table
        var createMilestoneProgressTable = connection.CreateCommand();
        createMilestoneProgressTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Account_Milestone_Progress (
                account_id INTEGER PRIMARY KEY,
                current_tier_number INTEGER DEFAULT 1,
                last_tier_reached_at TEXT,
                FOREIGN KEY (account_id) REFERENCES Account_Progression(account_id),
                FOREIGN KEY (current_tier_number) REFERENCES Milestone_Tiers(tier_number)
            )
        ";
        createMilestoneProgressTable.ExecuteNonQuery();

        _log.Information("Account progression tables initialized");
    }

    #endregion

    #region Account Progression CRUD

    /// <summary>
    /// Create a new account progression record
    /// </summary>
    public int CreateAccount()
    {
        _log.Information("Creating new account");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Account_Progression (created_at, updated_at)
            VALUES (@CreatedAt, @UpdatedAt);
            SELECT last_insert_rowid();
        ";
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow.ToString("O"));

        var accountId = Convert.ToInt32(command.ExecuteScalar());

        // Initialize milestone progress
        var milestoneCommand = connection.CreateCommand();
        milestoneCommand.CommandText = @"
            INSERT INTO Account_Milestone_Progress (account_id, current_tier_number)
            VALUES (@AccountId, 1)
        ";
        milestoneCommand.Parameters.AddWithValue("@AccountId", accountId);
        milestoneCommand.ExecuteNonQuery();

        _log.Information("Account created: AccountID={AccountId}", accountId);
        return accountId;
    }

    /// <summary>
    /// Get account progression by ID
    /// </summary>
    public AccountProgression? GetById(int accountId)
    {
        _log.Debug("Getting account progression: AccountID={AccountId}", accountId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Account_Progression
            WHERE account_id = @AccountId
        ";
        command.Parameters.AddWithValue("@AccountId", accountId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapAccountProgressionFromReader(reader);
        }

        _log.Warning("Account not found: AccountID={AccountId}", accountId);
        return null;
    }

    /// <summary>
    /// Update account progression
    /// </summary>
    public void Update(AccountProgression account)
    {
        _log.Debug("Updating account progression: AccountID={AccountId}", account.AccountId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Account_Progression
            SET total_achievement_points = @Points,
                current_milestone_tier = @Tier,
                updated_at = @UpdatedAt,
                total_characters_created = @CharactersCreated,
                total_campaigns_completed = @CampaignsCompleted,
                total_bosses_defeated = @BossesDefeated,
                total_achievements_unlocked = @AchievementsUnlocked,
                highest_new_game_plus_tier = @HighestNGPlus,
                highest_endless_wave = @HighestWave
            WHERE account_id = @AccountId
        ";
        command.Parameters.AddWithValue("@AccountId", account.AccountId);
        command.Parameters.AddWithValue("@Points", account.TotalAchievementPoints);
        command.Parameters.AddWithValue("@Tier", account.CurrentMilestoneTier);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("@CharactersCreated", account.TotalCharactersCreated);
        command.Parameters.AddWithValue("@CampaignsCompleted", account.TotalCampaignsCompleted);
        command.Parameters.AddWithValue("@BossesDefeated", account.TotalBossesDefeated);
        command.Parameters.AddWithValue("@AchievementsUnlocked", account.TotalAchievementsUnlocked);
        command.Parameters.AddWithValue("@HighestNGPlus", account.HighestNewGamePlusTier);
        command.Parameters.AddWithValue("@HighestWave", account.HighestEndlessWave);

        command.ExecuteNonQuery();

        _log.Information("Account updated: AccountID={AccountId}, Points={Points}, Tier={Tier}",
            account.AccountId, account.TotalAchievementPoints, account.CurrentMilestoneTier);
    }

    #endregion

    #region Account Unlocks

    /// <summary>
    /// Get all unlocks for an account
    /// </summary>
    public List<AccountUnlock> GetUnlocks(int accountId)
    {
        _log.Debug("Getting account unlocks: AccountID={AccountId}", accountId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                u.unlock_id,
                u.name,
                u.unlock_type,
                u.description,
                u.requirement_description,
                u.parameters_json,
                COALESCE(p.is_unlocked, 0) as is_unlocked,
                p.unlocked_at
            FROM Account_Unlocks u
            LEFT JOIN Account_Unlock_Progress p
                ON u.unlock_id = p.unlock_id AND p.account_id = @AccountId
        ";
        command.Parameters.AddWithValue("@AccountId", accountId);

        var unlocks = new List<AccountUnlock>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            unlocks.Add(MapAccountUnlockFromReader(reader));
        }

        _log.Information("Retrieved {Count} unlocks for account {AccountId}",
            unlocks.Count, accountId);
        return unlocks;
    }

    /// <summary>
    /// Get only unlocked benefits for an account
    /// </summary>
    public List<AccountUnlock> GetUnlockedBenefits(int accountId)
    {
        return GetUnlocks(accountId).Where(u => u.IsUnlocked).ToList();
    }

    /// <summary>
    /// Unlock a specific unlock for an account
    /// </summary>
    public void UnlockBenefit(int accountId, string unlockId)
    {
        _log.Information("Unlocking benefit: AccountID={AccountId}, UnlockID={UnlockId}",
            accountId, unlockId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO Account_Unlock_Progress
                (account_id, unlock_id, is_unlocked, unlocked_at)
            VALUES (@AccountId, @UnlockId, 1, @UnlockedAt)
        ";
        command.Parameters.AddWithValue("@AccountId", accountId);
        command.Parameters.AddWithValue("@UnlockId", unlockId);
        command.Parameters.AddWithValue("@UnlockedAt", DateTime.UtcNow.ToString("O"));

        command.ExecuteNonQuery();

        _log.Information("Benefit unlocked successfully");
    }

    /// <summary>
    /// Check if a specific unlock is unlocked
    /// </summary>
    public bool IsUnlocked(int accountId, string unlockId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT is_unlocked
            FROM Account_Unlock_Progress
            WHERE account_id = @AccountId AND unlock_id = @UnlockId
        ";
        command.Parameters.AddWithValue("@AccountId", accountId);
        command.Parameters.AddWithValue("@UnlockId", unlockId);

        var result = command.ExecuteScalar();
        return result != null && Convert.ToInt32(result) == 1;
    }

    #endregion

    #region Milestone Tiers

    /// <summary>
    /// Get milestone tier by tier number
    /// </summary>
    public MilestoneTier? GetTier(int tierNumber)
    {
        _log.Debug("Getting milestone tier: Tier={TierNumber}", tierNumber);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Milestone_Tiers
            WHERE tier_number = @TierNumber
        ";
        command.Parameters.AddWithValue("@TierNumber", tierNumber);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapMilestoneTierFromReader(reader);
        }

        return null;
    }

    /// <summary>
    /// Get all milestone tiers
    /// </summary>
    public List<MilestoneTier> GetAllTiers()
    {
        _log.Debug("Getting all milestone tiers");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Milestone_Tiers
            ORDER BY tier_number
        ";

        var tiers = new List<MilestoneTier>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tiers.Add(MapMilestoneTierFromReader(reader));
        }

        _log.Information("Retrieved {Count} milestone tiers", tiers.Count);
        return tiers;
    }

    /// <summary>
    /// Get current milestone tier for account
    /// </summary>
    public MilestoneTier? GetCurrentTier(int accountId)
    {
        var account = GetById(accountId);
        if (account == null) return null;

        return GetTier(account.CurrentMilestoneTier);
    }

    /// <summary>
    /// Update account milestone tier
    /// </summary>
    public void UpdateMilestoneTier(int accountId, int tierNumber)
    {
        _log.Information("Updating milestone tier: AccountID={AccountId}, Tier={TierNumber}",
            accountId, tierNumber);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Account_Milestone_Progress
            SET current_tier_number = @TierNumber,
                last_tier_reached_at = @ReachedAt
            WHERE account_id = @AccountId
        ";
        command.Parameters.AddWithValue("@AccountId", accountId);
        command.Parameters.AddWithValue("@TierNumber", tierNumber);
        command.Parameters.AddWithValue("@ReachedAt", DateTime.UtcNow.ToString("O"));

        command.ExecuteNonQuery();

        // Also update the account progression tier
        var updateAccount = connection.CreateCommand();
        updateAccount.CommandText = @"
            UPDATE Account_Progression
            SET current_milestone_tier = @TierNumber,
                updated_at = @UpdatedAt
            WHERE account_id = @AccountId
        ";
        updateAccount.Parameters.AddWithValue("@AccountId", accountId);
        updateAccount.Parameters.AddWithValue("@TierNumber", tierNumber);
        updateAccount.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow.ToString("O"));
        updateAccount.ExecuteNonQuery();

        _log.Information("Milestone tier updated successfully");
    }

    #endregion

    #region Mapping Helpers

    private AccountProgression MapAccountProgressionFromReader(SqliteDataReader reader)
    {
        return new AccountProgression
        {
            AccountId = reader.GetInt32(reader.GetOrdinal("account_id")),
            TotalAchievementPoints = reader.GetInt32(reader.GetOrdinal("total_achievement_points")),
            CurrentMilestoneTier = reader.GetInt32(reader.GetOrdinal("current_milestone_tier")),
            CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("created_at"))),
            UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("updated_at"))),
            TotalCharactersCreated = reader.GetInt32(reader.GetOrdinal("total_characters_created")),
            TotalCampaignsCompleted = reader.GetInt32(reader.GetOrdinal("total_campaigns_completed")),
            TotalBossesDefeated = reader.GetInt32(reader.GetOrdinal("total_bosses_defeated")),
            TotalAchievementsUnlocked = reader.GetInt32(reader.GetOrdinal("total_achievements_unlocked")),
            HighestNewGamePlusTier = reader.GetInt32(reader.GetOrdinal("highest_new_game_plus_tier")),
            HighestEndlessWave = reader.GetInt32(reader.GetOrdinal("highest_endless_wave"))
        };
    }

    private AccountUnlock MapAccountUnlockFromReader(SqliteDataReader reader)
    {
        var parametersJson = reader.GetString(reader.GetOrdinal("parameters_json"));
        var parameters = JsonSerializer.Deserialize<Dictionary<string, string>>(parametersJson)
            ?? new Dictionary<string, string>();

        var isUnlocked = reader.GetInt32(reader.GetOrdinal("is_unlocked")) == 1;
        DateTime? unlockedAt = null;
        if (!reader.IsDBNull(reader.GetOrdinal("unlocked_at")))
        {
            unlockedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("unlocked_at")));
        }

        return new AccountUnlock
        {
            UnlockId = reader.GetString(reader.GetOrdinal("unlock_id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            Type = Enum.Parse<AccountUnlockType>(reader.GetString(reader.GetOrdinal("unlock_type"))),
            Description = reader.GetString(reader.GetOrdinal("description")),
            RequirementDescription = reader.GetString(reader.GetOrdinal("requirement_description")),
            Parameters = parameters,
            IsUnlocked = isUnlocked,
            UnlockedAt = unlockedAt
        };
    }

    private MilestoneTier MapMilestoneTierFromReader(SqliteDataReader reader)
    {
        var unlockRewardsJson = reader.GetString(reader.GetOrdinal("unlock_rewards_json"));
        var cosmeticRewardsJson = reader.GetString(reader.GetOrdinal("cosmetic_rewards_json"));

        var unlockRewards = JsonSerializer.Deserialize<List<string>>(unlockRewardsJson)
            ?? new List<string>();
        var cosmeticRewards = JsonSerializer.Deserialize<List<string>>(cosmeticRewardsJson)
            ?? new List<string>();

        string? alternativeStartUnlock = null;
        if (!reader.IsDBNull(reader.GetOrdinal("alternative_start_unlock")))
        {
            alternativeStartUnlock = reader.GetString(reader.GetOrdinal("alternative_start_unlock"));
        }

        return new MilestoneTier
        {
            TierNumber = reader.GetInt32(reader.GetOrdinal("tier_number")),
            TierName = reader.GetString(reader.GetOrdinal("tier_name")),
            Description = reader.GetString(reader.GetOrdinal("description")),
            RequiredAchievementPoints = reader.GetInt32(reader.GetOrdinal("required_achievement_points")),
            UnlockRewards = unlockRewards,
            CosmeticRewards = cosmeticRewards,
            AlternativeStartUnlock = alternativeStartUnlock
        };
    }

    #endregion
}
