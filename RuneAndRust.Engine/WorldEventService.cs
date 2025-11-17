using Microsoft.Data.Sqlite;
using RuneAndRust.Core.Territory;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.35.3: Manages dynamic world events and their consequences
/// Generates, processes, and resolves events that affect territorial control
/// </summary>
public class WorldEventService
{
    private static readonly ILogger _log = Log.ForContext<WorldEventService>();
    private readonly string _connectionString;
    private readonly TerritoryControlService _territoryService;

    // Event spawn probability constants
    private const double EVENT_SPAWN_CHANCE_CONTESTED = 0.05; // 5% per day for contested sectors
    private const double EVENT_SPAWN_CHANCE_STABLE = 0.01;    // 1% per day for stable sectors
    private const double EVENT_SPAWN_CHANCE_WAR = 0.10;       // 10% per day for war zones
    private const double EVENT_SPAWN_CHANCE_INDEPENDENT = 0.02; // 2% per day for independent sectors

    public WorldEventService(string connectionString, TerritoryControlService territoryService)
    {
        _connectionString = connectionString;
        _territoryService = territoryService;

        _log.Debug("WorldEventService initialized with connection string");
    }

    /// <summary>
    /// Process daily event checks for a sector
    /// Checks for new event spawns and processes active events
    /// </summary>
    public void ProcessDailyEventCheck(int sectorId)
    {
        _log.Debug("Processing daily event check for sector {SectorId}", sectorId);

        try
        {
            // Calculate sector control state
            var controlState = _territoryService.CalculateSectorControlState(sectorId);

            // Determine spawn chance based on control state
            double spawnChance = controlState.State switch
            {
                "War" => EVENT_SPAWN_CHANCE_WAR,
                "Contested" => EVENT_SPAWN_CHANCE_CONTESTED,
                "Stable" => EVENT_SPAWN_CHANCE_STABLE,
                "Independent" => EVENT_SPAWN_CHANCE_INDEPENDENT,
                _ => EVENT_SPAWN_CHANCE_STABLE
            };

            // Roll for event spawn
            var random = new Random();
            if (random.NextDouble() <= spawnChance)
            {
                SpawnRandomEvent(sectorId, controlState);
            }

            // Process ongoing events
            var activeEvents = GetActiveSectorEvents(sectorId);
            foreach (var evt in activeEvents)
            {
                ProcessEvent(evt);
            }

            _log.Debug("Daily event check complete for sector {SectorId}: {ActiveCount} active events",
                sectorId, activeEvents.Count);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed daily event check for sector {SectorId}", sectorId);
            throw;
        }
    }

    /// <summary>
    /// Spawn a random event based on sector conditions
    /// </summary>
    private void SpawnRandomEvent(int sectorId, SectorControlState controlState)
    {
        // Available event types (excluding Faction_War which is managed by FactionWarService)
        var eventTypes = new List<string>
        {
            "Incursion",
            "Supply_Raid",
            "Catastrophe",
            "Awakening_Ritual",
            "Excavation_Discovery",
            "Purge_Campaign",
            "Scavenger_Caravan"
        };

        var random = new Random();
        string eventType = eventTypes[random.Next(eventTypes.Count)];

        // Faction-specific event filtering
        string? triggeredFaction = FilterFactionSpecificEvent(eventType, controlState);
        if (triggeredFaction == null)
        {
            _log.Debug("Event {EventType} not applicable for sector {SectorId} control state",
                eventType, sectorId);
            return;
        }

        // Determine event duration
        int duration = eventType switch
        {
            "Awakening_Ritual" => 7,
            "Excavation_Discovery" => 5,
            "Purge_Campaign" => 10,
            "Incursion" => 3,
            "Supply_Raid" => 1,
            "Catastrophe" => 2,
            "Scavenger_Caravan" => 2,
            _ => 1
        };

        // Generate event description
        string eventTitle = GenerateEventTitle(eventType, triggeredFaction);
        string eventDescription = GenerateEventDescription(eventType, triggeredFaction);

        // Create event in database
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO World_Events (
                world_id, sector_id, event_type, affected_faction,
                event_title, event_description, event_duration_days,
                is_resolved, player_influenced
            )
            VALUES (
                1, @SectorId, @EventType, @Faction,
                @Title, @Description, @Duration,
                0, 0
            )";

        command.Parameters.AddWithValue("@SectorId", sectorId);
        command.Parameters.AddWithValue("@EventType", eventType);
        command.Parameters.AddWithValue("@Faction", triggeredFaction ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Title", eventTitle);
        command.Parameters.AddWithValue("@Description", eventDescription);
        command.Parameters.AddWithValue("@Duration", duration);

        command.ExecuteNonQuery();

        _log.Information(
            "[EVENT SPAWNED] Type={EventType}, Sector={SectorId}, Faction={Faction}, Duration={Duration} days",
            eventType, sectorId, triggeredFaction, duration);
    }

    /// <summary>
    /// Filter faction-specific events
    /// Returns the faction that should trigger the event, or null if event doesn't apply
    /// </summary>
    private string? FilterFactionSpecificEvent(string eventType, SectorControlState controlState)
    {
        return eventType switch
        {
            "Awakening_Ritual" => controlState.DominantFaction == "GodSleeperCultists" ? "GodSleeperCultists" : null,
            "Excavation_Discovery" => controlState.DominantFaction == "JotunReaders" ? "JotunReaders" : null,
            "Purge_Campaign" => controlState.DominantFaction == "IronBanes" ? "IronBanes" : null,
            "Scavenger_Caravan" => controlState.DominantFaction == "RustClans" ? "RustClans" : null,
            "Incursion" => controlState.DominantFaction,
            "Supply_Raid" => controlState.DominantFaction,
            "Catastrophe" => null, // No faction required
            _ => null
        };
    }

    /// <summary>
    /// Process an active event (advance duration, apply effects)
    /// </summary>
    private void ProcessEvent(WorldEvent evt)
    {
        _log.Debug("Processing event: {EventId}, type {EventType}, sector {SectorId}",
            evt.EventId, evt.EventType, evt.SectorId);

        // Calculate days elapsed
        var daysElapsed = (DateTime.UtcNow - evt.EventStartDate).Days;

        // Check if event should resolve
        if (daysElapsed >= evt.EventDurationDays)
        {
            ResolveEvent(evt);
        }
        else
        {
            _log.Debug("Event {EventId} still active: {DaysElapsed}/{Duration} days elapsed",
                evt.EventId, daysElapsed, evt.EventDurationDays);
        }
    }

    /// <summary>
    /// Resolve a completed event and apply consequences
    /// </summary>
    private void ResolveEvent(WorldEvent evt)
    {
        _log.Information(
            "[EVENT RESOLVING] EventId={EventId}, Type={EventType}, Sector={SectorId}",
            evt.EventId, evt.EventType, evt.SectorId);

        try
        {
            // Apply event-specific consequences
            switch (evt.EventType)
            {
                case "Awakening_Ritual":
                    ResolveAwakeningRitual(evt);
                    break;

                case "Excavation_Discovery":
                    ResolveExcavationDiscovery(evt);
                    break;

                case "Purge_Campaign":
                    ResolvePurgeCampaign(evt);
                    break;

                case "Incursion":
                    ResolveIncursion(evt);
                    break;

                case "Supply_Raid":
                    ResolveSupplyRaid(evt);
                    break;

                case "Catastrophe":
                    ResolveCatastrophe(evt);
                    break;

                case "Scavenger_Caravan":
                    ResolveScavengerCaravan(evt);
                    break;
            }

            // Mark event as resolved
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE World_Events
                SET is_resolved = 1,
                    event_end_date = @EndDate,
                    outcome = @Outcome
                WHERE event_id = @EventId";

            command.Parameters.AddWithValue("@EndDate", DateTime.UtcNow);
            command.Parameters.AddWithValue("@Outcome", "Success");
            command.Parameters.AddWithValue("@EventId", evt.EventId);

            command.ExecuteNonQuery();

            _log.Information("[EVENT RESOLVED] EventId={EventId}, Type={EventType}",
                evt.EventId, evt.EventType);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to resolve event {EventId}", evt.EventId);
            throw;
        }
    }

    /// <summary>
    /// Resolve Awakening Ritual - God-Sleepers awaken dormant constructs
    /// Consequence: Spawn elite enemies, faction influence +5%
    /// </summary>
    private void ResolveAwakeningRitual(WorldEvent evt)
    {
        _log.Information("Awakening Ritual completed in sector {SectorId}: Elite Jötun-Forged awakened",
            evt.SectorId);

        // Apply influence gain for God-Sleepers
        if (evt.SectorId.HasValue && evt.AffectedFaction != null)
        {
            _territoryService.ShiftInfluence(
                evt.SectorId.Value,
                evt.AffectedFaction,
                5.0,
                $"Event: {evt.EventType} completed");
        }

        // Note: Enemy spawning would be handled by encounter generation system
        // Log the consequence for now
        _log.Information("Consequence: Elite Jötun-Forged enemies available in sector {SectorId}",
            evt.SectorId);
    }

    /// <summary>
    /// Resolve Excavation Discovery - Jötun-Readers unearth Pre-Glitch artifacts
    /// Consequence: Rare loot available, faction influence +5%
    /// </summary>
    private void ResolveExcavationDiscovery(WorldEvent evt)
    {
        _log.Information("Excavation Discovery completed in sector {SectorId}: Ancient artifacts unearthed",
            evt.SectorId);

        // Apply influence gain for Jötun-Readers
        if (evt.SectorId.HasValue && evt.AffectedFaction != null)
        {
            _territoryService.ShiftInfluence(
                evt.SectorId.Value,
                evt.AffectedFaction,
                5.0,
                $"Event: {evt.EventType} completed");
        }

        // Note: Artifact loot would be added to merchant inventories
        _log.Information("Consequence: Pre-Glitch artifacts available in sector {SectorId} merchants",
            evt.SectorId);
    }

    /// <summary>
    /// Resolve Purge Campaign - Iron-Banes hunt Undying
    /// Consequence: Reduced Undying spawns, faction influence +5%
    /// </summary>
    private void ResolvePurgeCampaign(WorldEvent evt)
    {
        _log.Information("Purge Campaign completed in sector {SectorId}: Undying threats reduced",
            evt.SectorId);

        // Apply influence gain for Iron-Banes
        if (evt.SectorId.HasValue && evt.AffectedFaction != null)
        {
            _territoryService.ShiftInfluence(
                evt.SectorId.Value,
                evt.AffectedFaction,
                5.0,
                $"Event: {evt.EventType} completed");
        }

        // Note: Enemy spawn rate modifiers would be handled by encounter system
        _log.Information("Consequence: Undying spawn rate reduced by 50% in sector {SectorId} for 10 days",
            evt.SectorId);
    }

    /// <summary>
    /// Resolve Incursion - Faction attempts territorial expansion
    /// Consequence: Faction influence +10%
    /// </summary>
    private void ResolveIncursion(WorldEvent evt)
    {
        _log.Information("Incursion completed in sector {SectorId}: Faction expanded control",
            evt.SectorId);

        // Apply significant influence gain
        if (evt.SectorId.HasValue && evt.AffectedFaction != null)
        {
            _territoryService.ShiftInfluence(
                evt.SectorId.Value,
                evt.AffectedFaction,
                10.0,
                $"Event: {evt.EventType} successful");
        }
    }

    /// <summary>
    /// Resolve Supply Raid - Enemies raid merchant supplies
    /// Consequence: Reduced merchant stock for 3 days
    /// </summary>
    private void ResolveSupplyRaid(WorldEvent evt)
    {
        _log.Information("Supply Raid completed in sector {SectorId}: Merchant stock compromised",
            evt.SectorId);

        // Note: Merchant penalties would be tracked separately
        _log.Information("Consequence: Merchant stock reduced by 30% in sector {SectorId} for 3 days",
            evt.SectorId);
    }

    /// <summary>
    /// Resolve Catastrophe - Reality corruption surge
    /// Consequence: Increased hazard density for 5 days
    /// </summary>
    private void ResolveCatastrophe(WorldEvent evt)
    {
        _log.Information("Catastrophe event completed in sector {SectorId}: Reality corruption surged",
            evt.SectorId);

        // Note: Hazard density modifiers would be tracked separately
        _log.Information("Consequence: Hazard density increased by 50% in sector {SectorId} for 5 days",
            evt.SectorId);
    }

    /// <summary>
    /// Resolve Scavenger Caravan - Rust-Clans establish trade route
    /// Consequence: Better merchant prices for 5 days, faction influence +3%
    /// </summary>
    private void ResolveScavengerCaravan(WorldEvent evt)
    {
        _log.Information("Scavenger Caravan arrived in sector {SectorId}: Trade route established",
            evt.SectorId);

        // Apply small influence gain for Rust-Clans
        if (evt.SectorId.HasValue && evt.AffectedFaction != null)
        {
            _territoryService.ShiftInfluence(
                evt.SectorId.Value,
                evt.AffectedFaction,
                3.0,
                $"Event: {evt.EventType} completed");
        }

        _log.Information("Consequence: Merchant prices reduced by 15% in sector {SectorId} for 5 days",
            evt.SectorId);
    }

    /// <summary>
    /// Get all active events for a specific sector
    /// </summary>
    public List<WorldEvent> GetActiveSectorEvents(int sectorId)
    {
        var events = new List<WorldEvent>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT event_id, world_id, sector_id, event_type, affected_faction,
                       event_title, event_description, event_start_date, event_end_date,
                       event_duration_days, is_resolved, player_influenced, outcome, influence_change
                FROM World_Events
                WHERE sector_id = @SectorId
                AND is_resolved = 0
                ORDER BY event_start_date DESC";

            command.Parameters.AddWithValue("@SectorId", sectorId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                events.Add(new WorldEvent
                {
                    EventId = reader.GetInt32(0),
                    WorldId = reader.GetInt32(1),
                    SectorId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    EventType = reader.GetString(3),
                    AffectedFaction = reader.IsDBNull(4) ? null : reader.GetString(4),
                    EventTitle = reader.GetString(5),
                    EventDescription = reader.GetString(6),
                    EventStartDate = reader.GetDateTime(7),
                    EventEndDate = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                    EventDurationDays = reader.GetInt32(9),
                    IsResolved = reader.GetBoolean(10),
                    PlayerInfluenced = reader.GetBoolean(11),
                    Outcome = reader.IsDBNull(12) ? null : reader.GetString(12),
                    InfluenceChange = reader.GetDouble(13)
                });
            }

            _log.Debug("Retrieved {EventCount} active events for sector {SectorId}",
                events.Count, sectorId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to retrieve active events for sector {SectorId}", sectorId);
            throw;
        }

        return events;
    }

    /// <summary>
    /// Get all active events in the world
    /// </summary>
    public List<WorldEvent> GetAllActiveEvents()
    {
        var events = new List<WorldEvent>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT event_id, world_id, sector_id, event_type, affected_faction,
                       event_title, event_description, event_start_date, event_end_date,
                       event_duration_days, is_resolved, player_influenced, outcome, influence_change
                FROM World_Events
                WHERE is_resolved = 0
                ORDER BY event_start_date DESC";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                events.Add(new WorldEvent
                {
                    EventId = reader.GetInt32(0),
                    WorldId = reader.GetInt32(1),
                    SectorId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    EventType = reader.GetString(3),
                    AffectedFaction = reader.IsDBNull(4) ? null : reader.GetString(4),
                    EventTitle = reader.GetString(5),
                    EventDescription = reader.GetString(6),
                    EventStartDate = reader.GetDateTime(7),
                    EventEndDate = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                    EventDurationDays = reader.GetInt32(9),
                    IsResolved = reader.GetBoolean(10),
                    PlayerInfluenced = reader.GetBoolean(11),
                    Outcome = reader.IsDBNull(12) ? null : reader.GetString(12),
                    InfluenceChange = reader.GetDouble(13)
                });
            }

            _log.Debug("Retrieved {EventCount} active events worldwide", events.Count);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to retrieve active events");
            throw;
        }

        return events;
    }

    /// <summary>
    /// Generate event title
    /// </summary>
    private string GenerateEventTitle(string eventType, string? faction)
    {
        return eventType switch
        {
            "Awakening_Ritual" => "Awakening Ritual in Progress",
            "Excavation_Discovery" => "Major Artifact Cache Discovered",
            "Purge_Campaign" => "Systematic Purge of Undying",
            "Incursion" => $"{faction} Territorial Expansion",
            "Supply_Raid" => "Merchant Supply Raid",
            "Catastrophe" => "Reality Corruption Surge",
            "Scavenger_Caravan" => "Scavenger Caravan Arrival",
            _ => "Unknown Event"
        };
    }

    /// <summary>
    /// Generate event description
    /// </summary>
    private string GenerateEventDescription(string eventType, string? faction)
    {
        return eventType switch
        {
            "Awakening_Ritual" => $"{faction} cultists are performing a ritual to awaken dormant Jötun-Forged constructs. The awakened machines will pose a significant threat to the sector.",
            "Excavation_Discovery" => $"{faction} scholars have unearthed a major Pre-Glitch artifact cache. Rare technology and schematics will be available for a limited time.",
            "Purge_Campaign" => $"{faction} warriors have launched a systematic purge of Undying threats in the sector. Success will significantly reduce corruption presence.",
            "Incursion" => $"{faction} forces are pushing to expand their territorial control. Their influence in the sector is increasing.",
            "Supply_Raid" => "Enemy raiders have struck merchant supply lines. Expect reduced inventory and higher prices until new supplies arrive.",
            "Catastrophe" => "A surge in reality corruption has warped the environment. Hazard density has increased significantly.",
            "Scavenger_Caravan" => $"{faction} has established a trade route through the sector. Merchants will have better stock and prices for a limited time.",
            _ => "An unknown event is occurring."
        };
    }
}
