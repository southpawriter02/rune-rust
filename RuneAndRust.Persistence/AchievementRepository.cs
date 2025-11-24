using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using System.Text.Json;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.41: Repository for managing achievements and achievement progress
/// </summary>
public class AchievementRepository
{
    private static readonly ILogger _log = Log.ForContext<AchievementRepository>();
    private readonly string _connectionString;

    public AchievementRepository(string connectionString)
    {
        _connectionString = connectionString;
        _log.Debug("AchievementRepository initialized");
        InitializeTables();
    }

    #region Table Initialization

    private void InitializeTables()
    {
        _log.Debug("Initializing achievement tables");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Achievements table (achievement definitions)
        var createAchievementsTable = connection.CreateCommand();
        createAchievementsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Achievements (
                achievement_id TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                category TEXT NOT NULL,
                description TEXT NOT NULL,
                flavor_text TEXT NOT NULL,
                achievement_points INTEGER NOT NULL,
                is_secret INTEGER DEFAULT 0,
                icon_id TEXT NOT NULL,
                required_progress INTEGER DEFAULT 1
            )
        ";
        createAchievementsTable.ExecuteNonQuery();

        // Achievement_Progress table (progress per account)
        var createProgressTable = connection.CreateCommand();
        createProgressTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Achievement_Progress (
                progress_id INTEGER PRIMARY KEY AUTOINCREMENT,
                account_id INTEGER NOT NULL,
                achievement_id TEXT NOT NULL,
                current_progress INTEGER DEFAULT 0,
                is_unlocked INTEGER DEFAULT 0,
                unlocked_at TEXT,
                FOREIGN KEY (account_id) REFERENCES Account_Progression(account_id),
                FOREIGN KEY (achievement_id) REFERENCES Achievements(achievement_id),
                UNIQUE (account_id, achievement_id)
            )
        ";
        createProgressTable.ExecuteNonQuery();

        // Achievement_Rewards table (rewards for achievements)
        var createRewardsTable = connection.CreateCommand();
        createRewardsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Achievement_Rewards (
                reward_id INTEGER PRIMARY KEY AUTOINCREMENT,
                achievement_id TEXT NOT NULL,
                reward_type TEXT NOT NULL,
                reward_item_id TEXT NOT NULL,
                FOREIGN KEY (achievement_id) REFERENCES Achievements(achievement_id)
            )
        ";
        createRewardsTable.ExecuteNonQuery();

        _log.Information("Achievement tables initialized");
    }

    #endregion

    #region Achievement CRUD

    /// <summary>
    /// Get achievement by ID
    /// </summary>
    public Achievement? GetById(string achievementId)
    {
        _log.Debug("Getting achievement: AchievementID={AchievementId}", achievementId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Achievements
            WHERE achievement_id = @AchievementId
        ";
        command.Parameters.AddWithValue("@AchievementId", achievementId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var achievement = MapAchievementFromReader(reader);

            // Load rewards
            achievement.RewardIds = GetRewardIds(achievementId);

            return achievement;
        }

        _log.Warning("Achievement not found: {AchievementId}", achievementId);
        return null;
    }

    /// <summary>
    /// Get all achievements
    /// </summary>
    public List<Achievement> GetAll()
    {
        _log.Debug("Getting all achievements");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Achievements ORDER BY category, achievement_points";

        var achievements = new List<Achievement>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var achievement = MapAchievementFromReader(reader);
            achievement.RewardIds = GetRewardIds(achievement.AchievementId);
            achievements.Add(achievement);
        }

        _log.Information("Retrieved {Count} achievements", achievements.Count);
        return achievements;
    }

    /// <summary>
    /// Get achievements by category
    /// </summary>
    public List<Achievement> GetByCategory(AchievementCategory category)
    {
        _log.Debug("Getting achievements by category: {Category}", category);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Achievements
            WHERE category = @Category
            ORDER BY achievement_points
        ";
        command.Parameters.AddWithValue("@Category", category.ToString());

        var achievements = new List<Achievement>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var achievement = MapAchievementFromReader(reader);
            achievement.RewardIds = GetRewardIds(achievement.AchievementId);
            achievements.Add(achievement);
        }

        _log.Information("Retrieved {Count} achievements for category {Category}",
            achievements.Count, category);
        return achievements;
    }

    #endregion

    #region Achievement Progress

    /// <summary>
    /// Get achievement progress for an account
    /// </summary>
    public AchievementProgress? GetProgress(int accountId, string achievementId)
    {
        _log.Debug("Getting achievement progress: AccountID={AccountId}, AchievementID={AchievementId}",
            accountId, achievementId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Achievement_Progress
            WHERE account_id = @AccountId AND achievement_id = @AchievementId
        ";
        command.Parameters.AddWithValue("@AccountId", accountId);
        command.Parameters.AddWithValue("@AchievementId", achievementId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapAchievementProgressFromReader(reader);
        }

        // If no progress exists, create initial entry
        return CreateInitialProgress(accountId, achievementId);
    }

    /// <summary>
    /// Get all achievement progress for an account
    /// </summary>
    public List<AchievementProgress> GetAllProgress(int accountId)
    {
        _log.Debug("Getting all achievement progress: AccountID={AccountId}", accountId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Achievement_Progress
            WHERE account_id = @AccountId
        ";
        command.Parameters.AddWithValue("@AccountId", accountId);

        var progressList = new List<AchievementProgress>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            progressList.Add(MapAchievementProgressFromReader(reader));
        }

        _log.Information("Retrieved {Count} achievement progress entries for account {AccountId}",
            progressList.Count, accountId);
        return progressList;
    }

    /// <summary>
    /// Update achievement progress
    /// </summary>
    public void UpdateProgress(AchievementProgress progress)
    {
        _log.Debug("Updating achievement progress: AccountID={AccountId}, AchievementID={AchievementId}, Progress={Progress}",
            progress.AccountId, progress.AchievementId, progress.CurrentProgress);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO Achievement_Progress
                (account_id, achievement_id, current_progress, is_unlocked, unlocked_at)
            VALUES (@AccountId, @AchievementId, @Progress, @IsUnlocked, @UnlockedAt)
        ";
        command.Parameters.AddWithValue("@AccountId", progress.AccountId);
        command.Parameters.AddWithValue("@AchievementId", progress.AchievementId);
        command.Parameters.AddWithValue("@Progress", progress.CurrentProgress);
        command.Parameters.AddWithValue("@IsUnlocked", progress.IsUnlocked ? 1 : 0);
        command.Parameters.AddWithValue("@UnlockedAt",
            progress.UnlockedAt?.ToString("O") ?? (object)DBNull.Value);

        command.ExecuteNonQuery();

        _log.Information("Achievement progress updated");
    }

    /// <summary>
    /// Unlock achievement for account
    /// </summary>
    public void UnlockAchievement(int accountId, string achievementId)
    {
        _log.Information("Unlocking achievement: AccountID={AccountId}, AchievementID={AchievementId}",
            accountId, achievementId);

        var progress = GetProgress(accountId, achievementId);
        if (progress == null)
        {
            progress = CreateInitialProgress(accountId, achievementId);
        }

        progress.IsUnlocked = true;
        progress.UnlockedAt = DateTime.UtcNow;

        UpdateProgress(progress);

        _log.Information("Achievement unlocked successfully");
    }

    /// <summary>
    /// Increment achievement progress
    /// </summary>
    public AchievementProgress IncrementProgress(int accountId, string achievementId, int increment = 1)
    {
        _log.Debug("Incrementing achievement progress: AccountID={AccountId}, AchievementID={AchievementId}, Increment={Increment}",
            accountId, achievementId, increment);

        var progress = GetProgress(accountId, achievementId);
        if (progress == null)
        {
            progress = CreateInitialProgress(accountId, achievementId);
        }

        progress.CurrentProgress += increment;
        UpdateProgress(progress);

        return progress;
    }

    /// <summary>
    /// Check if achievement is unlocked
    /// </summary>
    public bool IsUnlocked(int accountId, string achievementId)
    {
        var progress = GetProgress(accountId, achievementId);
        return progress?.IsUnlocked ?? false;
    }

    /// <summary>
    /// Get unlocked achievements for account
    /// </summary>
    public List<Achievement> GetUnlockedAchievements(int accountId)
    {
        _log.Debug("Getting unlocked achievements: AccountID={AccountId}", accountId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT a.*
            FROM Achievements a
            INNER JOIN Achievement_Progress p
                ON a.achievement_id = p.achievement_id
            WHERE p.account_id = @AccountId AND p.is_unlocked = 1
            ORDER BY p.unlocked_at DESC
        ";
        command.Parameters.AddWithValue("@AccountId", accountId);

        var achievements = new List<Achievement>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var achievement = MapAchievementFromReader(reader);
            achievement.RewardIds = GetRewardIds(achievement.AchievementId);
            achievements.Add(achievement);
        }

        _log.Information("Retrieved {Count} unlocked achievements for account {AccountId}",
            achievements.Count, accountId);
        return achievements;
    }

    #endregion

    #region Achievement Rewards

    /// <summary>
    /// Get reward IDs for an achievement
    /// </summary>
    public List<string> GetRewardIds(string achievementId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT reward_item_id FROM Achievement_Rewards
            WHERE achievement_id = @AchievementId
        ";
        command.Parameters.AddWithValue("@AchievementId", achievementId);

        var rewardIds = new List<string>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rewardIds.Add(reader.GetString(0));
        }

        return rewardIds;
    }

    /// <summary>
    /// Add reward to achievement
    /// </summary>
    public void AddReward(string achievementId, string rewardType, string rewardItemId)
    {
        _log.Debug("Adding reward to achievement: AchievementID={AchievementId}, Type={RewardType}, ItemID={ItemId}",
            achievementId, rewardType, rewardItemId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Achievement_Rewards (achievement_id, reward_type, reward_item_id)
            VALUES (@AchievementId, @RewardType, @RewardItemId)
        ";
        command.Parameters.AddWithValue("@AchievementId", achievementId);
        command.Parameters.AddWithValue("@RewardType", rewardType);
        command.Parameters.AddWithValue("@RewardItemId", rewardItemId);

        command.ExecuteNonQuery();

        _log.Information("Reward added to achievement");
    }

    #endregion

    #region Helper Methods

    private AchievementProgress CreateInitialProgress(int accountId, string achievementId)
    {
        var progress = new AchievementProgress
        {
            AccountId = accountId,
            AchievementId = achievementId,
            CurrentProgress = 0,
            IsUnlocked = false
        };

        UpdateProgress(progress);

        return progress;
    }

    private Achievement MapAchievementFromReader(SqliteDataReader reader)
    {
        return new Achievement
        {
            AchievementId = reader.GetString(reader.GetOrdinal("achievement_id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            Category = Enum.Parse<AchievementCategory>(reader.GetString(reader.GetOrdinal("category"))),
            Description = reader.GetString(reader.GetOrdinal("description")),
            FlavorText = reader.GetString(reader.GetOrdinal("flavor_text")),
            AchievementPoints = reader.GetInt32(reader.GetOrdinal("achievement_points")),
            IsSecret = reader.GetInt32(reader.GetOrdinal("is_secret")) == 1,
            IconId = reader.GetString(reader.GetOrdinal("icon_id")),
            RequiredProgress = reader.GetInt32(reader.GetOrdinal("required_progress"))
        };
    }

    private AchievementProgress MapAchievementProgressFromReader(SqliteDataReader reader)
    {
        DateTime? unlockedAt = null;
        if (!reader.IsDBNull(reader.GetOrdinal("unlocked_at")))
        {
            unlockedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("unlocked_at")));
        }

        return new AchievementProgress
        {
            ProgressId = reader.GetInt32(reader.GetOrdinal("progress_id")),
            AccountId = reader.GetInt32(reader.GetOrdinal("account_id")),
            AchievementId = reader.GetString(reader.GetOrdinal("achievement_id")),
            CurrentProgress = reader.GetInt32(reader.GetOrdinal("current_progress")),
            IsUnlocked = reader.GetInt32(reader.GetOrdinal("is_unlocked")) == 1,
            UnlockedAt = unlockedAt
        };
    }

    #endregion
}
