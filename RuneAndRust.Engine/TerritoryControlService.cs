using RuneAndRust.Core.Territory;
using Microsoft.Data.Sqlite;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.35.2: Territory Control Service
/// Manages faction influence and calculates sector control states
/// </summary>
public class TerritoryControlService
{
    private static readonly ILogger _log = Log.ForContext<TerritoryControlService>();
    private readonly string _connectionString;

    public TerritoryControlService(string connectionString)
    {
        _connectionString = connectionString;
        _log.Debug("TerritoryControlService initialized with connection string");
    }

    /// <summary>
    /// Calculate control state for a sector based on faction influence distribution.
    /// Control states: Stable (60%+ influence), Contested (2+ factions 40-60%), War (active war), Independent (no faction > 40%)
    /// </summary>
    public SectorControlState CalculateSectorControlState(int sectorId)
    {
        _log.Debug("Calculating sector control state for sector {SectorId}", sectorId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        try
        {
            // Get all faction influences for sector, ordered by influence descending
            var influences = new List<FactionInfluence>();
            var queryCommand = connection.CreateCommand();
            queryCommand.CommandText = @"
                SELECT faction_name, influence_value, control_state
                FROM Faction_Territory_Control
                WHERE sector_id = @SectorId
                ORDER BY influence_value DESC";
            queryCommand.Parameters.AddWithValue("@SectorId", sectorId);

            using var reader = queryCommand.ExecuteReader();
            while (reader.Read())
            {
                influences.Add(new FactionInfluence
                {
                    FactionName = reader.GetString(0),
                    InfluenceValue = reader.GetDouble(1),
                    ControlState = reader.GetString(2)
                });
            }

            if (influences.Count == 0)
            {
                _log.Warning("No faction influence data for sector {SectorId}", sectorId);
                return new SectorControlState
                {
                    State = "Independent",
                    DominantFaction = "Independents"
                };
            }

            var topInfluence = influences[0];
            var secondInfluence = influences.Count > 1 ? influences[1] : null;

            // Check for active war
            var warCommand = connection.CreateCommand();
            warCommand.CommandText = @"
                SELECT faction_a, faction_b FROM Faction_Wars
                WHERE sector_id = @SectorId AND is_active = 1";
            warCommand.Parameters.AddWithValue("@SectorId", sectorId);

            using var warReader = warCommand.ExecuteReader();
            if (warReader.Read())
            {
                string factionA = warReader.GetString(0);
                string factionB = warReader.GetString(1);

                _log.Information(
                    "Sector {SectorId} in active war between {FactionA} and {FactionB}",
                    sectorId, factionA, factionB);

                return new SectorControlState
                {
                    State = "War",
                    DominantFaction = null,
                    ContestedFactions = new[] { factionA, factionB }
                };
            }

            // Stable: One faction has 60%+ influence
            if (topInfluence.InfluenceValue >= 60.0)
            {
                _log.Debug(
                    "Sector {SectorId} stable under {Faction} control ({Influence}%)",
                    sectorId, topInfluence.FactionName, topInfluence.InfluenceValue);

                return new SectorControlState
                {
                    State = "Stable",
                    DominantFaction = topInfluence.FactionName
                };
            }

            // Contested: Two or more factions have 40-60% influence
            if (secondInfluence != null &&
                topInfluence.InfluenceValue >= 40.0 &&
                secondInfluence.InfluenceValue >= 40.0)
            {
                _log.Information(
                    "Sector {SectorId} contested between {Faction1} ({Influence1}%) and {Faction2} ({Influence2}%)",
                    sectorId, topInfluence.FactionName, topInfluence.InfluenceValue,
                    secondInfluence.FactionName, secondInfluence.InfluenceValue);

                return new SectorControlState
                {
                    State = "Contested",
                    DominantFaction = null,
                    ContestedFactions = new[]
                    {
                        topInfluence.FactionName,
                        secondInfluence.FactionName
                    }
                };
            }

            // Independent: No faction exceeds 40% influence
            _log.Debug(
                "Sector {SectorId} independent (top faction {Faction} at {Influence}%)",
                sectorId, topInfluence.FactionName, topInfluence.InfluenceValue);

            return new SectorControlState
            {
                State = "Independent",
                DominantFaction = "Independents"
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to calculate control state for sector {SectorId}", sectorId);
            throw;
        }
    }

    /// <summary>
    /// Get the dominant faction for a sector (faction with highest influence).
    /// </summary>
    public string GetDominantFaction(int sectorId)
    {
        _log.Debug("Getting dominant faction for sector {SectorId}", sectorId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT faction_name
            FROM Faction_Territory_Control
            WHERE sector_id = @SectorId
            ORDER BY influence_value DESC
            LIMIT 1";
        command.Parameters.AddWithValue("@SectorId", sectorId);

        var result = command.ExecuteScalar();
        var dominantFaction = result?.ToString() ?? "Independents";

        _log.Debug("Dominant faction in sector {SectorId}: {Faction}", sectorId, dominantFaction);
        return dominantFaction;
    }

    /// <summary>
    /// Shift faction influence in a sector by a delta amount.
    /// Normalizes influences if sum exceeds 100%, then recalculates control state.
    /// </summary>
    public void ShiftInfluence(
        int sectorId,
        string factionName,
        double influenceDelta,
        string reason)
    {
        _log.Information(
            "Shifting influence for {Faction} in sector {SectorId} by {Delta} ({Reason})",
            factionName, sectorId, influenceDelta, reason);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        try
        {
            // Apply influence change
            var updateCommand = connection.CreateCommand();
            updateCommand.CommandText = @"
                UPDATE Faction_Territory_Control
                SET influence_value = influence_value + @Delta,
                    last_updated = CURRENT_TIMESTAMP
                WHERE sector_id = @SectorId AND faction_name = @FactionName";
            updateCommand.Parameters.AddWithValue("@Delta", influenceDelta);
            updateCommand.Parameters.AddWithValue("@SectorId", sectorId);
            updateCommand.Parameters.AddWithValue("@FactionName", factionName);
            updateCommand.ExecuteNonQuery();

            // Normalize influences if sum exceeds 100
            NormalizeInfluences(sectorId, connection);

            // Recalculate control state
            var newState = CalculateSectorControlState(sectorId);

            // Update control_state for all factions in sector
            var stateUpdateCommand = connection.CreateCommand();
            stateUpdateCommand.CommandText = @"
                UPDATE Faction_Territory_Control
                SET control_state = @State
                WHERE sector_id = @SectorId";
            stateUpdateCommand.Parameters.AddWithValue("@State", newState.State);
            stateUpdateCommand.Parameters.AddWithValue("@SectorId", sectorId);
            stateUpdateCommand.ExecuteNonQuery();

            _log.Information(
                "Influence shifted: {Faction} in sector {SectorId} by {Delta} ({Reason}). New state: {State}",
                factionName, sectorId, influenceDelta, reason, newState.State);
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "Failed to shift influence for {Faction} in sector {SectorId}",
                factionName, sectorId);
            throw;
        }
    }

    /// <summary>
    /// Normalize influences in a sector so sum doesn't exceed 100%.
    /// Scales all influences proportionally if total exceeds 100%.
    /// </summary>
    private void NormalizeInfluences(int sectorId, SqliteConnection connection)
    {
        // Get all influences for sector
        var influences = new List<FactionInfluence>();
        var queryCommand = connection.CreateCommand();
        queryCommand.CommandText = @"
            SELECT faction_name, influence_value
            FROM Faction_Territory_Control
            WHERE sector_id = @SectorId";
        queryCommand.Parameters.AddWithValue("@SectorId", sectorId);

        using var reader = queryCommand.ExecuteReader();
        while (reader.Read())
        {
            influences.Add(new FactionInfluence
            {
                FactionName = reader.GetString(0),
                InfluenceValue = reader.GetDouble(1)
            });
        }
        reader.Close();

        double totalInfluence = influences.Sum(f => f.InfluenceValue);

        if (totalInfluence > 100.0)
        {
            _log.Debug(
                "Normalizing influences in sector {SectorId} (total: {Total}%)",
                sectorId, totalInfluence);

            // Scale all influences proportionally
            foreach (var faction in influences)
            {
                double normalized = (faction.InfluenceValue / totalInfluence) * 100.0;

                var updateCommand = connection.CreateCommand();
                updateCommand.CommandText = @"
                    UPDATE Faction_Territory_Control
                    SET influence_value = @Normalized
                    WHERE sector_id = @SectorId AND faction_name = @FactionName";
                updateCommand.Parameters.AddWithValue("@Normalized", normalized);
                updateCommand.Parameters.AddWithValue("@SectorId", sectorId);
                updateCommand.Parameters.AddWithValue("@FactionName", faction.FactionName);
                updateCommand.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Get all faction influences for a sector.
    /// Returns list ordered by influence descending.
    /// </summary>
    public List<FactionInfluence> GetSectorInfluences(int sectorId)
    {
        _log.Debug("Getting all influences for sector {SectorId}", sectorId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var influences = new List<FactionInfluence>();
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT territory_control_id, world_id, sector_id, faction_name,
                   influence_value, control_state, last_updated
            FROM Faction_Territory_Control
            WHERE sector_id = @SectorId
            ORDER BY influence_value DESC";
        command.Parameters.AddWithValue("@SectorId", sectorId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            influences.Add(new FactionInfluence
            {
                TerritoryControlId = reader.GetInt32(0),
                WorldId = reader.GetInt32(1),
                SectorId = reader.GetInt32(2),
                FactionName = reader.GetString(3),
                InfluenceValue = reader.GetDouble(4),
                ControlState = reader.GetString(5),
                LastUpdated = DateTime.Parse(reader.GetString(6))
            });
        }

        _log.Debug("Found {Count} faction influences for sector {SectorId}", influences.Count, sectorId);
        return influences;
    }

    /// <summary>
    /// Get all sectors in a world.
    /// </summary>
    public List<Sector> GetSectors(int worldId)
    {
        _log.Debug("Getting sectors for world {WorldId}", worldId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var sectors = new List<Sector>();
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT sector_id, world_id, sector_name, sector_description,
                   biome_id, z_level, created_at
            FROM Sectors
            WHERE world_id = @WorldId
            ORDER BY sector_id";
        command.Parameters.AddWithValue("@WorldId", worldId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            sectors.Add(new Sector
            {
                SectorId = reader.GetInt32(0),
                WorldId = reader.GetInt32(1),
                SectorName = reader.GetString(2),
                SectorDescription = reader.GetString(3),
                BiomeId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                ZLevel = reader.GetString(5),
                CreatedAt = DateTime.Parse(reader.GetString(6))
            });
        }

        _log.Information("Found {Count} sectors for world {WorldId}", sectors.Count, worldId);
        return sectors;
    }
}
