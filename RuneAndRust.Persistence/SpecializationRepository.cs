using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using System.Text.Json;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.19: Repository for managing specialization data
/// Provides CRUD operations for specializations and character specialization tracking
/// </summary>
public class SpecializationRepository
{
    private static readonly ILogger _log = Log.ForContext<SpecializationRepository>();
    private readonly string _connectionString;

    public SpecializationRepository(string connectionString)
    {
        _connectionString = connectionString;
        _log.Debug("SpecializationRepository initialized");
    }

    #region Specialization CRUD

    /// <summary>
    /// Get a specialization by ID
    /// </summary>
    public SpecializationData? GetById(int specializationId)
    {
        _log.Debug("Getting specialization by ID: {SpecializationID}", specializationId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Specializations
            WHERE SpecializationID = @Id AND IsActive = 1
        ";
        command.Parameters.AddWithValue("@Id", specializationId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapSpecializationFromReader(reader);
        }

        _log.Warning("Specialization not found: {SpecializationID}", specializationId);
        return null;
    }

    /// <summary>
    /// Get all specializations for a specific archetype
    /// </summary>
    public List<SpecializationData> GetByArchetype(int archetypeId)
    {
        _log.Debug("Getting specializations for archetype: {ArchetypeID}", archetypeId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Specializations
            WHERE ArchetypeID = @ArchetypeId AND IsActive = 1
            ORDER BY SpecializationID
        ";
        command.Parameters.AddWithValue("@ArchetypeId", archetypeId);

        var specializations = new List<SpecializationData>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            specializations.Add(MapSpecializationFromReader(reader));
        }

        _log.Information("Found {Count} specializations for archetype {ArchetypeID}", specializations.Count, archetypeId);
        return specializations;
    }

    /// <summary>
    /// Get all active specializations
    /// </summary>
    public List<SpecializationData> GetAll()
    {
        _log.Debug("Getting all active specializations");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Specializations
            WHERE IsActive = 1
            ORDER BY ArchetypeID, SpecializationID
        ";

        var specializations = new List<SpecializationData>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            specializations.Add(MapSpecializationFromReader(reader));
        }

        _log.Information("Found {Count} total specializations", specializations.Count);
        return specializations;
    }

    /// <summary>
    /// Insert a new specialization
    /// </summary>
    public void Insert(SpecializationData specialization)
    {
        _log.Information("Inserting specialization: {SpecializationName}", specialization.Name);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Specializations (
                SpecializationID, Name, ArchetypeID, PathType, MechanicalRole,
                PrimaryAttribute, SecondaryAttribute, Description, Tagline,
                UnlockRequirementsJson, ResourceSystem, TraumaRisk, IconEmoji,
                PPCostToUnlock, IsActive
            ) VALUES (
                @Id, @Name, @ArchetypeId, @PathType, @MechanicalRole,
                @PrimaryAttribute, @SecondaryAttribute, @Description, @Tagline,
                @UnlockRequirementsJson, @ResourceSystem, @TraumaRisk, @IconEmoji,
                @PPCostToUnlock, @IsActive
            )
        ";

        command.Parameters.AddWithValue("@Id", specialization.SpecializationID);
        command.Parameters.AddWithValue("@Name", specialization.Name);
        command.Parameters.AddWithValue("@ArchetypeId", specialization.ArchetypeID);
        command.Parameters.AddWithValue("@PathType", specialization.PathType);
        command.Parameters.AddWithValue("@MechanicalRole", specialization.MechanicalRole);
        command.Parameters.AddWithValue("@PrimaryAttribute", specialization.PrimaryAttribute);
        command.Parameters.AddWithValue("@SecondaryAttribute", specialization.SecondaryAttribute ?? string.Empty);
        command.Parameters.AddWithValue("@Description", specialization.Description);
        command.Parameters.AddWithValue("@Tagline", specialization.Tagline);
        command.Parameters.AddWithValue("@UnlockRequirementsJson", JsonSerializer.Serialize(specialization.UnlockRequirements));
        command.Parameters.AddWithValue("@ResourceSystem", specialization.ResourceSystem);
        command.Parameters.AddWithValue("@TraumaRisk", specialization.TraumaRisk);
        command.Parameters.AddWithValue("@IconEmoji", specialization.IconEmoji);
        command.Parameters.AddWithValue("@PPCostToUnlock", specialization.PPCostToUnlock);
        command.Parameters.AddWithValue("@IsActive", specialization.IsActive ? 1 : 0);

        command.ExecuteNonQuery();

        _log.Information("Specialization inserted: {SpecializationID} - {SpecializationName}",
            specialization.SpecializationID, specialization.Name);
    }

    #endregion

    #region Character Specialization Tracking

    /// <summary>
    /// Unlock a specialization for a character
    /// </summary>
    public void UnlockForCharacter(int characterId, int specializationId)
    {
        _log.Information("Unlocking specialization {SpecializationID} for character {CharacterID}",
            specializationId, characterId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO CharacterSpecializations (
                CharacterID, SpecializationID, UnlockedAt, IsActive, PPSpentInTree
            ) VALUES (
                @CharacterId, @SpecializationId, @UnlockedAt, 1, 0
            )
        ";

        command.Parameters.AddWithValue("@CharacterId", characterId);
        command.Parameters.AddWithValue("@SpecializationId", specializationId);
        command.Parameters.AddWithValue("@UnlockedAt", DateTime.UtcNow.ToString("O"));

        command.ExecuteNonQuery();

        _log.Information("Specialization unlocked successfully");
    }

    /// <summary>
    /// Get all specializations unlocked by a character
    /// </summary>
    public List<CharacterSpecialization> GetUnlockedSpecializations(int characterId)
    {
        _log.Debug("Getting unlocked specializations for character {CharacterID}", characterId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM CharacterSpecializations
            WHERE CharacterID = @CharacterId AND IsActive = 1
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);

        var unlockedSpecs = new List<CharacterSpecialization>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            unlockedSpecs.Add(new CharacterSpecialization
            {
                CharacterID = reader.GetInt32(0),
                SpecializationID = reader.GetInt32(1),
                UnlockedAt = DateTime.Parse(reader.GetString(2)),
                IsActive = reader.GetInt32(3) == 1,
                PPSpentInTree = reader.GetInt32(4)
            });
        }

        _log.Information("Character {CharacterID} has {Count} unlocked specializations",
            characterId, unlockedSpecs.Count);
        return unlockedSpecs;
    }

    /// <summary>
    /// Check if character has unlocked a specific specialization
    /// </summary>
    public bool HasUnlocked(int characterId, int specializationId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(*) FROM CharacterSpecializations
            WHERE CharacterID = @CharacterId
              AND SpecializationID = @SpecializationId
              AND IsActive = 1
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);
        command.Parameters.AddWithValue("@SpecializationId", specializationId);

        var count = (long)(command.ExecuteScalar() ?? 0L);
        return count > 0;
    }

    /// <summary>
    /// Get PP spent in a specific specialization tree
    /// </summary>
    public int GetPPSpentInTree(int characterId, int specializationId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT PPSpentInTree FROM CharacterSpecializations
            WHERE CharacterID = @CharacterId
              AND SpecializationID = @SpecializationId
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);
        command.Parameters.AddWithValue("@SpecializationId", specializationId);

        var result = command.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    /// <summary>
    /// Update PP spent in a specialization tree
    /// </summary>
    public void UpdatePPSpentInTree(int characterId, int specializationId, int ppAmount)
    {
        _log.Debug("Updating PP spent in tree: Character={CharacterID}, Spec={SpecializationID}, PP={PP}",
            characterId, specializationId, ppAmount);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE CharacterSpecializations
            SET PPSpentInTree = PPSpentInTree + @PPAmount
            WHERE CharacterID = @CharacterId
              AND SpecializationID = @SpecializationId
        ";
        command.Parameters.AddWithValue("@CharacterId", characterId);
        command.Parameters.AddWithValue("@SpecializationId", specializationId);
        command.Parameters.AddWithValue("@PPAmount", ppAmount);

        command.ExecuteNonQuery();
    }

    #endregion

    #region Helper Methods

    private SpecializationData MapSpecializationFromReader(SqliteDataReader reader)
    {
        return new SpecializationData
        {
            SpecializationID = reader.GetInt32(reader.GetOrdinal("SpecializationID")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            ArchetypeID = reader.GetInt32(reader.GetOrdinal("ArchetypeID")),
            PathType = reader.GetString(reader.GetOrdinal("PathType")),
            MechanicalRole = reader.GetString(reader.GetOrdinal("MechanicalRole")),
            PrimaryAttribute = reader.GetString(reader.GetOrdinal("PrimaryAttribute")),
            SecondaryAttribute = reader.GetString(reader.GetOrdinal("SecondaryAttribute")),
            Description = reader.GetString(reader.GetOrdinal("Description")),
            Tagline = reader.GetString(reader.GetOrdinal("Tagline")),
            UnlockRequirements = JsonSerializer.Deserialize<UnlockRequirements>(
                reader.GetString(reader.GetOrdinal("UnlockRequirementsJson"))) ?? new UnlockRequirements(),
            ResourceSystem = reader.GetString(reader.GetOrdinal("ResourceSystem")),
            TraumaRisk = reader.GetString(reader.GetOrdinal("TraumaRisk")),
            IconEmoji = reader.GetString(reader.GetOrdinal("IconEmoji")),
            PPCostToUnlock = reader.GetInt32(reader.GetOrdinal("PPCostToUnlock")),
            IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1
        };
    }

    #endregion
}
