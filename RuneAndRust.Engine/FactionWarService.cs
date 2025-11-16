using RuneAndRust.Core.Territory;
using Microsoft.Data.Sqlite;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.35.2: Faction War Service
/// Manages faction wars including triggering, advancement, and resolution
/// </summary>
public class FactionWarService
{
    private static readonly ILogger _log = Log.ForContext<FactionWarService>();
    private readonly string _connectionString;
    private readonly TerritoryControlService _territoryService;

    // War configuration constants
    private const double WAR_TRIGGER_THRESHOLD = 45.0; // Both factions need 45%+ influence
    private const int WAR_TRIGGER_DURATION_DAYS = 10; // Contested for 10+ days
    private const double WAR_VICTORY_THRESHOLD = 50.0; // ±50 war_balance
    private const int WAR_MAX_DURATION_DAYS = 15; // Maximum war duration before forced resolution
    private const double WAR_VICTOR_INFLUENCE_GAIN = 20.0; // Victor gains +20% influence
    private const double WAR_LOSER_INFLUENCE_LOSS = 20.0; // Loser loses -20% influence
    private const int WAR_COLLATERAL_DAMAGE_PERCENT = 25; // Hazard density increase

    public FactionWarService(string connectionString, TerritoryControlService territoryService)
    {
        _connectionString = connectionString;
        _territoryService = territoryService;
        _log.Debug("FactionWarService initialized");
    }

    /// <summary>
    /// Check if contested sector should escalate to war.
    /// Wars trigger when two factions both exceed 45% influence.
    /// </summary>
    public bool CheckWarTrigger(int sectorId)
    {
        _log.Debug("Checking war trigger for sector {SectorId}", sectorId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        try
        {
            // Check if already at war
            var existingWarCommand = connection.CreateCommand();
            existingWarCommand.CommandText = @"
                SELECT war_id FROM Faction_Wars
                WHERE sector_id = @SectorId AND is_active = 1";
            existingWarCommand.Parameters.AddWithValue("@SectorId", sectorId);

            if (existingWarCommand.ExecuteScalar() != null)
            {
                _log.Debug("Sector {SectorId} already at war", sectorId);
                return false;
            }

            // Get top two factions
            var influences = _territoryService.GetSectorInfluences(sectorId);

            if (influences.Count < 2)
            {
                _log.Debug("Sector {SectorId} has fewer than 2 factions, cannot trigger war", sectorId);
                return false;
            }

            var faction1 = influences[0];
            var faction2 = influences[1];

            // Check trigger threshold (both factions >= 45%)
            if (faction1.InfluenceValue >= WAR_TRIGGER_THRESHOLD &&
                faction2.InfluenceValue >= WAR_TRIGGER_THRESHOLD)
            {
                _log.Warning(
                    "War trigger threshold met in sector {SectorId}: {Faction1} ({Influence1}%) vs {Faction2} ({Influence2}%)",
                    sectorId, faction1.FactionName, faction1.InfluenceValue,
                    faction2.FactionName, faction2.InfluenceValue);

                // Trigger war
                InitiateWar(sectorId, faction1.FactionName, faction2.FactionName, connection);
                return true;
            }

            _log.Debug(
                "War threshold not met in sector {SectorId}: {Faction1} ({Influence1}%) vs {Faction2} ({Influence2}%)",
                sectorId, faction1.FactionName, faction1.InfluenceValue,
                faction2.FactionName, faction2.InfluenceValue);

            return false;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to check war trigger for sector {SectorId}", sectorId);
            throw;
        }
    }

    /// <summary>
    /// Initiate a new faction war in a sector.
    /// </summary>
    private void InitiateWar(int sectorId, string factionA, string factionB, SqliteConnection connection)
    {
        _log.Information(
            "Initiating war in sector {SectorId} between {FactionA} and {FactionB}",
            sectorId, factionA, factionB);

        try
        {
            // Create war record
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT INTO Faction_Wars (
                    world_id, sector_id, faction_a, faction_b,
                    war_balance, is_active, collateral_damage
                )
                VALUES (1, @SectorId, @FactionA, @FactionB, 0.0, 1, 0)";
            insertCommand.Parameters.AddWithValue("@SectorId", sectorId);
            insertCommand.Parameters.AddWithValue("@FactionA", factionA);
            insertCommand.Parameters.AddWithValue("@FactionB", factionB);
            insertCommand.ExecuteNonQuery();

            // Get the war_id of the newly created war
            var warIdCommand = connection.CreateCommand();
            warIdCommand.CommandText = "SELECT last_insert_rowid()";
            var warId = Convert.ToInt32(warIdCommand.ExecuteScalar());

            // Update control state to 'War'
            var updateStateCommand = connection.CreateCommand();
            updateStateCommand.CommandText = @"
                UPDATE Faction_Territory_Control
                SET control_state = 'War'
                WHERE sector_id = @SectorId";
            updateStateCommand.Parameters.AddWithValue("@SectorId", sectorId);
            updateStateCommand.ExecuteNonQuery();

            _log.Warning(
                "[WAR INITIATED] War {WarId} started in sector {SectorId}: {FactionA} vs {FactionB}",
                warId, sectorId, factionA, factionB);
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "Failed to initiate war in sector {SectorId}",
                sectorId);
            throw;
        }
    }

    /// <summary>
    /// Advance an active war by processing a balance shift.
    /// Checks for victory condition or time limit after each shift.
    /// </summary>
    public void AdvanceWar(int warId, double balanceShift, string reason)
    {
        _log.Information(
            "Advancing war {WarId} by {Shift} ({Reason})",
            warId, balanceShift, reason);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        try
        {
            // Get current war state
            var war = GetWar(warId, connection);

            if (war == null || !war.IsActive)
            {
                _log.Warning("War {WarId} not found or not active", warId);
                return;
            }

            // Update war balance (clamped to -100 to +100)
            double newBalance = Math.Clamp(war.WarBalance + balanceShift, -100.0, 100.0);

            var updateCommand = connection.CreateCommand();
            updateCommand.CommandText = @"
                UPDATE Faction_Wars
                SET war_balance = @Balance
                WHERE war_id = @WarId";
            updateCommand.Parameters.AddWithValue("@Balance", newBalance);
            updateCommand.Parameters.AddWithValue("@WarId", warId);
            updateCommand.ExecuteNonQuery();

            _log.Information(
                "War {WarId} balance shifted from {OldBalance} to {NewBalance} ({Reason})",
                warId, war.WarBalance, newBalance, reason);

            // Check for victory condition (±50 threshold)
            if (Math.Abs(newBalance) >= WAR_VICTORY_THRESHOLD)
            {
                string victor = newBalance > 0 ? war.FactionA : war.FactionB;
                _log.Warning(
                    "War {WarId} victory threshold reached (balance: {Balance}). Victor: {Victor}",
                    warId, newBalance, victor);
                ResolveWar(warId, victor, connection);
            }
            else
            {
                // Check for time limit
                var warDuration = (DateTime.Now - war.WarStartDate).Days;
                if (warDuration >= WAR_MAX_DURATION_DAYS)
                {
                    _log.Warning(
                        "War {WarId} reached max duration ({Days} days), forcing resolution",
                        warId, WAR_MAX_DURATION_DAYS);

                    // Victor is faction with positive balance (or faction_a if tied)
                    string victor = newBalance >= 0 ? war.FactionA : war.FactionB;
                    ResolveWar(warId, victor, connection);
                }
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to advance war {WarId}", warId);
            throw;
        }
    }

    /// <summary>
    /// Resolve a war and apply consequences.
    /// Victor gains +20% influence, loser loses -20%, collateral damage increases hazards.
    /// </summary>
    public void ResolveWar(int warId, string victor)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        ResolveWar(warId, victor, connection);
    }

    private void ResolveWar(int warId, string victor, SqliteConnection connection)
    {
        _log.Information("Resolving war {WarId} with victor {Victor}", warId, victor);

        try
        {
            var war = GetWar(warId, connection);

            if (war == null)
            {
                _log.Error("War {WarId} not found", warId);
                return;
            }

            string loser = victor == war.FactionA ? war.FactionB : war.FactionA;

            // Update war record
            var updateCommand = connection.CreateCommand();
            updateCommand.CommandText = @"
                UPDATE Faction_Wars
                SET is_active = 0,
                    war_end_date = CURRENT_TIMESTAMP,
                    victor = @Victor,
                    collateral_damage = @Damage
                WHERE war_id = @WarId";
            updateCommand.Parameters.AddWithValue("@Victor", victor);
            updateCommand.Parameters.AddWithValue("@Damage", WAR_COLLATERAL_DAMAGE_PERCENT);
            updateCommand.Parameters.AddWithValue("@WarId", warId);
            updateCommand.ExecuteNonQuery();

            // Apply influence changes
            _territoryService.ShiftInfluence(
                war.SectorId,
                victor,
                WAR_VICTOR_INFLUENCE_GAIN,
                $"Victory in war {warId}");

            _territoryService.ShiftInfluence(
                war.SectorId,
                loser,
                -WAR_LOSER_INFLUENCE_LOSS,
                $"Defeat in war {warId}");

            // Update control state (war -> new state)
            var newState = _territoryService.CalculateSectorControlState(war.SectorId);
            var stateUpdateCommand = connection.CreateCommand();
            stateUpdateCommand.CommandText = @"
                UPDATE Faction_Territory_Control
                SET control_state = @State
                WHERE sector_id = @SectorId";
            stateUpdateCommand.Parameters.AddWithValue("@State", newState.State);
            stateUpdateCommand.Parameters.AddWithValue("@SectorId", war.SectorId);
            stateUpdateCommand.ExecuteNonQuery();

            _log.Warning(
                "[WAR RESOLVED] War {WarId} in sector {SectorId} won by {Victor}. " +
                "Loser: {Loser}. New state: {State}. Collateral damage: {Damage}%",
                warId, war.SectorId, victor, loser, newState.State, WAR_COLLATERAL_DAMAGE_PERCENT);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to resolve war {WarId}", warId);
            throw;
        }
    }

    /// <summary>
    /// Get all active wars across all sectors.
    /// </summary>
    public List<FactionWar> GetActiveWars()
    {
        _log.Debug("Getting all active wars");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var wars = new List<FactionWar>();
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT war_id, world_id, sector_id, faction_a, faction_b,
                   war_start_date, war_end_date, war_balance, is_active,
                   victor, collateral_damage
            FROM Faction_Wars
            WHERE is_active = 1
            ORDER BY war_start_date DESC";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            wars.Add(ReadFactionWar(reader));
        }

        _log.Information("Found {Count} active wars", wars.Count);
        return wars;
    }

    /// <summary>
    /// Get active war for a specific sector.
    /// Returns null if no active war.
    /// </summary>
    public FactionWar? GetActiveWarForSector(int sectorId)
    {
        _log.Debug("Getting active war for sector {SectorId}", sectorId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT war_id, world_id, sector_id, faction_a, faction_b,
                   war_start_date, war_end_date, war_balance, is_active,
                   victor, collateral_damage
            FROM Faction_Wars
            WHERE sector_id = @SectorId AND is_active = 1";
        command.Parameters.AddWithValue("@SectorId", sectorId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var war = ReadFactionWar(reader);
            _log.Debug("Found active war {WarId} in sector {SectorId}", war.WarId, sectorId);
            return war;
        }

        _log.Debug("No active war in sector {SectorId}", sectorId);
        return null;
    }

    /// <summary>
    /// Get a specific war by ID.
    /// </summary>
    private FactionWar? GetWar(int warId, SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT war_id, world_id, sector_id, faction_a, faction_b,
                   war_start_date, war_end_date, war_balance, is_active,
                   victor, collateral_damage
            FROM Faction_Wars
            WHERE war_id = @WarId";
        command.Parameters.AddWithValue("@WarId", warId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return ReadFactionWar(reader);
        }

        return null;
    }

    /// <summary>
    /// Helper method to read FactionWar from database reader.
    /// </summary>
    private FactionWar ReadFactionWar(SqliteDataReader reader)
    {
        return new FactionWar
        {
            WarId = reader.GetInt32(0),
            WorldId = reader.GetInt32(1),
            SectorId = reader.GetInt32(2),
            FactionA = reader.GetString(3),
            FactionB = reader.GetString(4),
            WarStartDate = DateTime.Parse(reader.GetString(5)),
            WarEndDate = reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6)),
            WarBalance = reader.GetDouble(7),
            IsActive = reader.GetInt32(8) == 1,
            Victor = reader.IsDBNull(9) ? null : reader.GetString(9),
            CollateralDamage = reader.GetInt32(10)
        };
    }
}
