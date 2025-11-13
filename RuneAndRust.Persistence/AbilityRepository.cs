using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using System.Text.Json;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.19: Repository for managing ability data
/// Provides CRUD operations for abilities and character ability tracking
/// </summary>
public class AbilityRepository
{
    private static readonly ILogger _log = Log.ForContext<AbilityRepository>();
    private readonly string _connectionString;

    public AbilityRepository(string connectionString)
    {
        _connectionString = connectionString;
        _log.Debug("AbilityRepository initialized");
    }

    #region Ability CRUD

    /// <summary>
    /// Get an ability by ID
    /// </summary>
    public AbilityData? GetById(int abilityId)
    {
        _log.Debug("Getting ability by ID: {AbilityID}", abilityId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Abilities
            WHERE AbilityID = @Id AND IsActive = 1
        ";
        command.Parameters.AddWithValue("@Id", abilityId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapAbilityFromReader(reader);
        }

        _log.Warning("Ability not found: {AbilityID}", abilityId);
        return null;
    }

    /// <summary>
    /// Get all abilities for a specific specialization
    /// </summary>
    public List<AbilityData> GetBySpecialization(int specializationId)
    {
        _log.Debug("Getting abilities for specialization: {SpecializationID}", specializationId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Abilities
            WHERE SpecializationID = @SpecializationId AND IsActive = 1
            ORDER BY TierLevel, AbilityID
        ";
        command.Parameters.AddWithValue("@SpecializationId", specializationId);

        var abilities = new List<AbilityData>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            abilities.Add(MapAbilityFromReader(reader));
        }

        _log.Information("Found {Count} abilities for specialization {SpecializationID}",
            abilities.Count, specializationId);
        return abilities;
    }

    /// <summary>
    /// Get abilities by tier for a specialization
    /// </summary>
    public List<AbilityData> GetBySpecializationAndTier(int specializationId, int tierLevel)
    {
        _log.Debug("Getting Tier {TierLevel} abilities for specialization: {SpecializationID}",
            tierLevel, specializationId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Abilities
            WHERE SpecializationID = @SpecializationId
              AND TierLevel = @TierLevel
              AND IsActive = 1
            ORDER BY AbilityID
        ";
        command.Parameters.AddWithValue("@SpecializationId", specializationId);
        command.Parameters.AddWithValue("@TierLevel", tierLevel);

        var abilities = new List<AbilityData>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            abilities.Add(MapAbilityFromReader(reader));
        }

        _log.Information("Found {Count} Tier {TierLevel} abilities", abilities.Count, tierLevel);
        return abilities;
    }

    /// <summary>
    /// Insert a new ability
    /// </summary>
    public void Insert(AbilityData ability)
    {
        _log.Information("Inserting ability: {AbilityName}", ability.Name);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Abilities (
                AbilityID, SpecializationID, Name, Description, TierLevel, PPCost,
                PrerequisitesJson, AbilityType, ActionType, TargetType, ResourceCostJson,
                AttributeUsed, BonusDice, SuccessThreshold, MechanicalSummary,
                DamageDice, IgnoresArmor, HealingDice, StatusEffectsAppliedJson,
                StatusEffectsRemovedJson, MaxRank, CostToRank2, CostToRank3,
                CooldownTurns, CooldownType, IsActive, Notes
            ) VALUES (
                @Id, @SpecializationId, @Name, @Description, @TierLevel, @PPCost,
                @PrerequisitesJson, @AbilityType, @ActionType, @TargetType, @ResourceCostJson,
                @AttributeUsed, @BonusDice, @SuccessThreshold, @MechanicalSummary,
                @DamageDice, @IgnoresArmor, @HealingDice, @StatusEffectsAppliedJson,
                @StatusEffectsRemovedJson, @MaxRank, @CostToRank2, @CostToRank3,
                @CooldownTurns, @CooldownType, @IsActive, @Notes
            )
        ";

        command.Parameters.AddWithValue("@Id", ability.AbilityID);
        command.Parameters.AddWithValue("@SpecializationId", ability.SpecializationID);
        command.Parameters.AddWithValue("@Name", ability.Name);
        command.Parameters.AddWithValue("@Description", ability.Description);
        command.Parameters.AddWithValue("@TierLevel", ability.TierLevel);
        command.Parameters.AddWithValue("@PPCost", ability.PPCost);
        command.Parameters.AddWithValue("@PrerequisitesJson", JsonSerializer.Serialize(ability.Prerequisites));
        command.Parameters.AddWithValue("@AbilityType", ability.AbilityType);
        command.Parameters.AddWithValue("@ActionType", ability.ActionType);
        command.Parameters.AddWithValue("@TargetType", ability.TargetType);
        command.Parameters.AddWithValue("@ResourceCostJson", JsonSerializer.Serialize(ability.ResourceCost));
        command.Parameters.AddWithValue("@AttributeUsed", ability.AttributeUsed ?? string.Empty);
        command.Parameters.AddWithValue("@BonusDice", ability.BonusDice);
        command.Parameters.AddWithValue("@SuccessThreshold", ability.SuccessThreshold);
        command.Parameters.AddWithValue("@MechanicalSummary", ability.MechanicalSummary ?? string.Empty);
        command.Parameters.AddWithValue("@DamageDice", ability.DamageDice);
        command.Parameters.AddWithValue("@IgnoresArmor", ability.IgnoresArmor ? 1 : 0);
        command.Parameters.AddWithValue("@HealingDice", ability.HealingDice);
        command.Parameters.AddWithValue("@StatusEffectsAppliedJson", JsonSerializer.Serialize(ability.StatusEffectsApplied));
        command.Parameters.AddWithValue("@StatusEffectsRemovedJson", JsonSerializer.Serialize(ability.StatusEffectsRemoved));
        command.Parameters.AddWithValue("@MaxRank", ability.MaxRank);
        command.Parameters.AddWithValue("@CostToRank2", ability.CostToRank2);
        command.Parameters.AddWithValue("@CostToRank3", ability.CostToRank3);
        command.Parameters.AddWithValue("@CooldownTurns", ability.CooldownTurns);
        command.Parameters.AddWithValue("@CooldownType", ability.CooldownType);
        command.Parameters.AddWithValue("@IsActive", ability.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("@Notes", ability.Notes ?? string.Empty);

        command.ExecuteNonQuery();

        _log.Information("Ability inserted: {AbilityID} - {AbilityName}",
            ability.AbilityID, ability.Name);
    }

    #endregion

    #region Character Ability Tracking

    /// <summary>
    /// Learn an ability for a character
    /// </summary>
    public void LearnForCharacter(int characterId, int abilityId)
    {
        _log.Information("Learning ability {AbilityID} for character {CharacterID}",
            abilityId, characterId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO CharacterAbilities (
                CharacterID, AbilityID, UnlockedAt, CurrentRank, TimesUsed
            ) VALUES (
                @CharacterId, @AbilityId, @UnlockedAt, 1, 0
            )
        ";

        command.Parameters.AddWithValue("@CharacterId", characterId);
        command.Parameters.AddWithValue("@AbilityId", abilityId);
        command.Parameters.AddWithValue("@UnlockedAt", DateTime.UtcNow.ToString("O"));

        command.ExecuteNonQuery();

        _log.Information("Ability learned successfully");
    }

    /// <summary>
    /// Get all abilities learned by a character
    /// </summary>
    public List<CharacterAbility> GetLearnedAbilities(int characterId)
    {
        _log.Debug("Getting learned abilities for character {CharacterID}", characterId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM CharacterAbilities
            WHERE CharacterID = @CharacterId
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);

        var learnedAbilities = new List<CharacterAbility>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            learnedAbilities.Add(new CharacterAbility
            {
                CharacterID = reader.GetInt32(0),
                AbilityID = reader.GetInt32(1),
                UnlockedAt = DateTime.Parse(reader.GetString(2)),
                CurrentRank = reader.GetInt32(3),
                TimesUsed = reader.GetInt32(4)
            });
        }

        _log.Information("Character {CharacterID} has learned {Count} abilities",
            characterId, learnedAbilities.Count);
        return learnedAbilities;
    }

    /// <summary>
    /// Check if character has learned a specific ability
    /// </summary>
    public bool HasLearned(int characterId, int abilityId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(*) FROM CharacterAbilities
            WHERE CharacterID = @CharacterId
              AND AbilityID = @AbilityId
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);
        command.Parameters.AddWithValue("@AbilityId", abilityId);

        var count = (long)(command.ExecuteScalar() ?? 0L);
        return count > 0;
    }

    /// <summary>
    /// Rank up an ability
    /// </summary>
    public void RankUp(int characterId, int abilityId)
    {
        _log.Information("Ranking up ability {AbilityID} for character {CharacterID}",
            abilityId, characterId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE CharacterAbilities
            SET CurrentRank = CurrentRank + 1
            WHERE CharacterID = @CharacterId
              AND AbilityID = @AbilityId
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);
        command.Parameters.AddWithValue("@AbilityId", abilityId);

        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Increment usage count for an ability
    /// </summary>
    public void IncrementUsage(int characterId, int abilityId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE CharacterAbilities
            SET TimesUsed = TimesUsed + 1
            WHERE CharacterID = @CharacterId
              AND AbilityID = @AbilityId
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);
        command.Parameters.AddWithValue("@AbilityId", abilityId);

        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Get the current rank of an ability for a character
    /// </summary>
    public int GetCurrentRank(int characterId, int abilityId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT CurrentRank FROM CharacterAbilities
            WHERE CharacterID = @CharacterId
              AND AbilityID = @AbilityId
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);
        command.Parameters.AddWithValue("@AbilityId", abilityId);

        var result = command.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    #endregion

    #region Helper Methods

    private AbilityData MapAbilityFromReader(SqliteDataReader reader)
    {
        return new AbilityData
        {
            AbilityID = reader.GetInt32(reader.GetOrdinal("AbilityID")),
            SpecializationID = reader.GetInt32(reader.GetOrdinal("SpecializationID")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Description = reader.GetString(reader.GetOrdinal("Description")),
            TierLevel = reader.GetInt32(reader.GetOrdinal("TierLevel")),
            PPCost = reader.GetInt32(reader.GetOrdinal("PPCost")),
            Prerequisites = JsonSerializer.Deserialize<AbilityPrerequisites>(
                reader.GetString(reader.GetOrdinal("PrerequisitesJson"))) ?? new AbilityPrerequisites(),
            AbilityType = reader.GetString(reader.GetOrdinal("AbilityType")),
            ActionType = reader.GetString(reader.GetOrdinal("ActionType")),
            TargetType = reader.GetString(reader.GetOrdinal("TargetType")),
            ResourceCost = JsonSerializer.Deserialize<AbilityResourceCost>(
                reader.GetString(reader.GetOrdinal("ResourceCostJson"))) ?? new AbilityResourceCost(),
            AttributeUsed = reader.GetString(reader.GetOrdinal("AttributeUsed")),
            BonusDice = reader.GetInt32(reader.GetOrdinal("BonusDice")),
            SuccessThreshold = reader.GetInt32(reader.GetOrdinal("SuccessThreshold")),
            MechanicalSummary = reader.GetString(reader.GetOrdinal("MechanicalSummary")),
            DamageDice = reader.GetInt32(reader.GetOrdinal("DamageDice")),
            IgnoresArmor = reader.GetInt32(reader.GetOrdinal("IgnoresArmor")) == 1,
            HealingDice = reader.GetInt32(reader.GetOrdinal("HealingDice")),
            StatusEffectsApplied = JsonSerializer.Deserialize<List<string>>(
                reader.GetString(reader.GetOrdinal("StatusEffectsAppliedJson"))) ?? new List<string>(),
            StatusEffectsRemoved = JsonSerializer.Deserialize<List<string>>(
                reader.GetString(reader.GetOrdinal("StatusEffectsRemovedJson"))) ?? new List<string>(),
            MaxRank = reader.GetInt32(reader.GetOrdinal("MaxRank")),
            CostToRank2 = reader.GetInt32(reader.GetOrdinal("CostToRank2")),
            CostToRank3 = reader.GetInt32(reader.GetOrdinal("CostToRank3")),
            CooldownTurns = reader.GetInt32(reader.GetOrdinal("CooldownTurns")),
            CooldownType = reader.GetString(reader.GetOrdinal("CooldownType")),
            IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1,
            Notes = reader.GetString(reader.GetOrdinal("Notes"))
        };
    }

    #endregion
}
