using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;
using RuneAndRust.Core.Territory;
using RuneAndRust.Core.Factions;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.35.4: Top-level territory control orchestration service
/// Coordinates all territory systems, player actions, and integration points
/// </summary>
public class TerritoryService
{
    private static readonly ILogger _log = Log.ForContext<TerritoryService>();
    private readonly string _connectionString;
    private readonly TerritoryControlService _controlService;
    private readonly FactionWarService _warService;
    private readonly WorldEventService _eventService;
    private readonly ReputationService? _reputationService;
    private readonly HazardDensityModifier _hazardModifier;

    // Caching for frequently accessed data
    private readonly MemoryCache _sectorControlCache;
    private const int CACHE_DURATION_SECONDS = 300; // 5 minutes

    public TerritoryService(
        string connectionString,
        TerritoryControlService controlService,
        FactionWarService warService,
        WorldEventService eventService,
        ReputationService? reputationService = null)
    {
        _connectionString = connectionString;
        _controlService = controlService;
        _warService = warService;
        _eventService = eventService;
        _reputationService = reputationService;
        _hazardModifier = new HazardDensityModifier();

        _sectorControlCache = new MemoryCache(new MemoryCacheOptions());

        _log.Debug("TerritoryService initialized with caching enabled");
    }

    /// <summary>
    /// Record player action that affects territorial influence
    /// </summary>
    public void RecordPlayerAction(
        int characterId,
        int sectorId,
        string actionType,
        string affectedFaction,
        string? notes = null)
    {
        _log.Debug("Recording player territorial action: {ActionType} by character {CharacterId} in sector {SectorId}",
            actionType, characterId, sectorId);

        try
        {
            // Calculate influence delta based on action type and reputation
            double influenceDelta = CalculateInfluenceDelta(
                characterId,
                sectorId,
                actionType,
                affectedFaction);

            // Record action in database
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Player_Territorial_Actions (
                    character_id, world_id, sector_id, action_type,
                    affected_faction, influence_delta, notes
                )
                VALUES (
                    @CharacterId, 1, @SectorId, @ActionType,
                    @AffectedFaction, @InfluenceDelta, @Notes
                )";

            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.Parameters.AddWithValue("@SectorId", sectorId);
            command.Parameters.AddWithValue("@ActionType", actionType);
            command.Parameters.AddWithValue("@AffectedFaction", affectedFaction);
            command.Parameters.AddWithValue("@InfluenceDelta", influenceDelta);
            command.Parameters.AddWithValue("@Notes", notes ?? (object)DBNull.Value);

            command.ExecuteNonQuery();

            // Apply influence shift
            _controlService.ShiftInfluence(
                sectorId,
                affectedFaction,
                influenceDelta,
                $"Player action: {actionType}");

            // Check if action triggers war
            var controlState = _controlService.CalculateSectorControlState(sectorId);
            if (controlState.State == "Contested")
            {
                _warService.CheckWarTrigger(sectorId);
            }

            // Check if action affects active war
            var activeWar = _warService.GetActiveWarForSector(sectorId);
            if (activeWar != null)
            {
                // Determine which side benefits
                double warBalanceShift = CalculateWarBalanceShift(
                    activeWar,
                    affectedFaction,
                    influenceDelta);

                if (warBalanceShift != 0)
                {
                    _warService.AdvanceWar(
                        activeWar.WarId,
                        warBalanceShift,
                        $"Player action: {actionType}");
                }
            }

            // Invalidate cache
            _sectorControlCache.Remove($"control_state_{sectorId}");

            _log.Information(
                "[PLAYER ACTION] Type={ActionType}, Character={CharacterId}, Faction={Faction}, " +
                "Influence={Delta}, Sector={SectorId}",
                actionType, characterId, affectedFaction, influenceDelta, sectorId);
        }
        catch (Exception ex)
        {
            _log.Error(ex,
                "Failed to record player action {ActionType} for character {CharacterId}",
                actionType, characterId);
            throw;
        }
    }

    /// <summary>
    /// Calculate influence delta from player action
    /// </summary>
    private double CalculateInfluenceDelta(
        int characterId,
        int sectorId,
        string actionType,
        string factionName)
    {
        // Base influence values per action type
        double baseInfluence = actionType switch
        {
            "Kill_Enemy" => 0.5,
            "Complete_Quest" => 5.0,
            "Defend_Territory" => 5.0,
            "Sabotage" => -5.0,
            "Escort_Caravan" => 4.0,
            "Destroy_Hazard" => 2.0,
            "Activate_Artifact" => 6.0,
            _ => 1.0
        };

        // Get reputation multiplier (if reputation service available)
        double reputationMultiplier = 1.0;
        if (_reputationService != null)
        {
            // Get faction ID from faction name
            int? factionId = GetFactionIdByName(factionName);
            if (factionId.HasValue)
            {
                var reputation = _reputationService.GetFactionReputation(characterId, factionId.Value);
                if (reputation != null)
                {
                    // -100 to +100 → 0.5x to 1.5x
                    reputationMultiplier = 1.0 + (reputation.ReputationValue / 200.0);
                    reputationMultiplier = Math.Clamp(reputationMultiplier, 0.5, 1.5);
                }
            }
        }

        // Check if sector is player's "home territory" (highest reputation faction controls it)
        double territoryBonus = 1.0;
        var dominantFaction = _controlService.GetDominantFaction(sectorId);
        if (dominantFaction == factionName)
        {
            territoryBonus = 1.2; // +20% bonus in friendly territory
        }

        double finalInfluence = baseInfluence * reputationMultiplier * territoryBonus;

        _log.Debug(
            "Influence calculation: base={Base}, reputation_mult={RepMult}, " +
            "territory_bonus={TerBonus}, final={Final}",
            baseInfluence, reputationMultiplier, territoryBonus, finalInfluence);

        return Math.Clamp(finalInfluence, -10.0, 10.0);
    }

    /// <summary>
    /// Calculate war balance shift from player action
    /// </summary>
    private double CalculateWarBalanceShift(
        FactionWar war,
        string affectedFaction,
        double influenceDelta)
    {
        // Determine which side the action benefits
        if (affectedFaction == war.FactionA)
        {
            return influenceDelta * 2.0; // War actions have double impact
        }
        else if (affectedFaction == war.FactionB)
        {
            return -influenceDelta * 2.0; // Negative for faction_b
        }

        return 0.0; // Action doesn't affect war participants
    }

    /// <summary>
    /// Get complete territory status for a sector (cached)
    /// </summary>
    public TerritoryStatus GetSectorTerritoryStatus(int sectorId)
    {
        string cacheKey = $"control_state_{sectorId}";

        if (_sectorControlCache.TryGetValue(cacheKey, out TerritoryStatus? cachedStatus))
        {
            _log.Debug("Cache hit for sector {SectorId} territory status", sectorId);
            return cachedStatus!;
        }

        _log.Debug("Cache miss - calculating territory status for sector {SectorId}", sectorId);

        try
        {
            var controlState = _controlService.CalculateSectorControlState(sectorId);
            var influences = _controlService.GetSectorInfluences(sectorId);
            var activeWar = _warService.GetActiveWarForSector(sectorId);
            var activeEvents = _eventService.GetActiveSectorEvents(sectorId);

            // Get sector name
            string sectorName = GetSectorName(sectorId);

            var status = new TerritoryStatus
            {
                SectorId = sectorId,
                SectorName = sectorName,
                ControlState = controlState.State,
                DominantFaction = controlState.DominantFaction,
                ContestedFactions = controlState.ContestedFactions,
                FactionInfluences = influences,
                ActiveWar = activeWar,
                ActiveEvents = activeEvents
            };

            // Cache for 5 minutes
            _sectorControlCache.Set(
                cacheKey,
                status,
                TimeSpan.FromSeconds(CACHE_DURATION_SECONDS));

            _log.Debug("Territory status cached for sector {SectorId}: {ControlState}",
                sectorId, status.ControlState);

            return status;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get territory status for sector {SectorId}", sectorId);
            throw;
        }
    }

    /// <summary>
    /// Process daily territory updates (wars, events, influence decay)
    /// </summary>
    public void ProcessDailyTerritoryUpdate()
    {
        _log.Information("[DAILY UPDATE] Starting daily territory update");

        try
        {
            // Process all active wars
            var activeWars = _warService.GetActiveWars();
            _log.Debug("Processing {WarCount} active wars", activeWars.Count);

            foreach (var war in activeWars)
            {
                // Check for time-based resolution
                var warDuration = (DateTime.UtcNow - war.WarStartDate).Days;
                if (warDuration >= 15) // Max duration
                {
                    string victor = war.WarBalance > 0 ? war.FactionA : war.FactionB;
                    _log.Information("Auto-resolving war {WarId} after {Days} days: Victor={Victor}",
                        war.WarId, warDuration, victor);
                    _warService.ResolveWar(war.WarId, victor);
                }
            }

            // Process all sectors for event checks
            var sectors = _controlService.GetSectors(worldId: 1);
            _log.Debug("Processing daily event checks for {SectorCount} sectors", sectors.Count);

            foreach (var sector in sectors)
            {
                _eventService.ProcessDailyEventCheck(sector.SectorId);
            }

            // Check for new war triggers in all contested sectors
            var contestedSectors = GetContestedSectors();
            _log.Debug("Checking war triggers for {ContestedCount} contested sectors", contestedSectors.Count);

            foreach (var sectorId in contestedSectors)
            {
                _warService.CheckWarTrigger(sectorId);
            }

            // Clear cache
            var cacheEntries = _sectorControlCache.Count;
            _sectorControlCache.Compact(1.0); // Remove all entries
            _log.Debug("Cleared {Count} cache entries", cacheEntries);

            _log.Information(
                "[DAILY UPDATE] Complete: {WarCount} wars processed, {SectorCount} sectors checked, " +
                "{ContestedCount} contested sectors",
                activeWars.Count, sectors.Count, contestedSectors.Count);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to process daily territory update");
            throw;
        }
    }

    /// <summary>
    /// Get player's total influence contribution per faction
    /// </summary>
    public Dictionary<string, double> GetPlayerTotalInfluence(int characterId)
    {
        var influences = new Dictionary<string, double>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT affected_faction, SUM(influence_delta) as total
                FROM Player_Territorial_Actions
                WHERE character_id = @CharacterId
                GROUP BY affected_faction
                ORDER BY total DESC";

            command.Parameters.AddWithValue("@CharacterId", characterId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                string faction = reader.GetString(0);
                double total = reader.GetDouble(1);
                influences[faction] = total;
            }

            _log.Debug("Player {CharacterId} total influence: {Count} factions affected",
                characterId, influences.Count);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get player total influence for character {CharacterId}", characterId);
            throw;
        }

        return influences;
    }

    /// <summary>
    /// Get generation parameters influenced by territory control
    /// </summary>
    public SectorGenerationParams GetSectorGenerationParams(int sectorId)
    {
        var status = GetSectorTerritoryStatus(sectorId);
        var baseParams = new SectorGenerationParams();

        // Apply hazard density modifier
        bool isWarZone = status.ControlState == "War";
        baseParams.HazardDensityMultiplier = _hazardModifier.CalculateHazardDensity(
            status.DominantFaction,
            isWarZone);

        // Faction-specific modifications
        switch (status.DominantFaction)
        {
            case "IronBanes":
                baseParams.EnemyFactionFilter = "Undying";
                baseParams.EnemyDensityMultiplier = 1.5; // More Undying patrols
                baseParams.LootTableModifier = "Anti-Undying_Gear";
                baseParams.AmbientDescription = "Iron-Bane patrols maintain vigilance against Undying corruption.";
                break;

            case "JotunReaders":
                baseParams.ArtifactSpawnRate = 1.3; // +30% artifacts
                baseParams.ScholarNPCChance = 0.15; // 15% scholar NPCs
                baseParams.EnvironmentalStorytelling = "Pre-Glitch_History";
                baseParams.AmbientDescription = "Jötun-Reader scholars study ancient Pre-Glitch remnants.";
                break;

            case "GodSleeperCultists":
                baseParams.EnemyFactionFilter = "Jötun-Forged";
                baseParams.EnemyDensityMultiplier = 1.4; // +40% constructs
                baseParams.AmbientDescription = "God-Sleeper rituals disturb dormant constructs.";
                break;

            case "RustClans":
                baseParams.SalvageMaterialRate = 1.5; // +50% salvage
                baseParams.MerchantPriceModifier = 0.85; // -15% prices
                baseParams.ScavengerNPCChance = 0.20; // 20% scavenger NPCs
                baseParams.AmbientDescription = "Rust-Clan trade routes bring prosperity and salvage opportunities.";
                break;

            case "Independents":
                baseParams.EnemyVarietyMultiplier = 1.5; // More diverse encounters
                baseParams.NeutralNPCChance = 0.30; // 30% neutral NPCs
                baseParams.AmbientDescription = "Independent settlements maintain a fragile neutrality.";
                break;
        }

        // War zone modifications
        if (isWarZone && status.ActiveWar != null)
        {
            baseParams.HazardDensityMultiplier *= (1.0 + status.ActiveWar.CollateralDamage / 100.0);
            baseParams.AmbientDescription = $"War-torn sector: {status.ActiveWar.FactionA} vs {status.ActiveWar.FactionB}. Collateral damage evident.";
        }

        _log.Debug("Generation params for sector {SectorId}: Faction={Faction}, War={IsWar}, " +
            "HazardDensity={HazardDensity}x",
            sectorId, status.DominantFaction, isWarZone, baseParams.HazardDensityMultiplier);

        return baseParams;
    }

    /// <summary>
    /// Get all contested sectors
    /// </summary>
    private List<int> GetContestedSectors()
    {
        var sectorIds = new List<int>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT DISTINCT sector_id
                FROM Faction_Territory_Control
                WHERE control_state = 'Contested'";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                sectorIds.Add(reader.GetInt32(0));
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get contested sectors");
            throw;
        }

        return sectorIds;
    }

    /// <summary>
    /// Get sector name by ID
    /// </summary>
    private string GetSectorName(int sectorId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT sector_name
                FROM Sectors
                WHERE sector_id = @SectorId";

            command.Parameters.AddWithValue("@SectorId", sectorId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return reader.GetString(0);
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get sector name for {SectorId}", sectorId);
        }

        return $"Sector {sectorId}";
    }

    /// <summary>
    /// Get faction ID from faction name (helper for reputation lookups)
    /// </summary>
    private int? GetFactionIdByName(string factionName)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT faction_id
                FROM Factions
                WHERE faction_name = @FactionName";

            command.Parameters.AddWithValue("@FactionName", factionName);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return reader.GetInt32(0);
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get faction ID for {FactionName}", factionName);
        }

        return null;
    }

    /// <summary>
    /// Invalidate cache for a specific sector
    /// </summary>
    public void InvalidateSectorCache(int sectorId)
    {
        _sectorControlCache.Remove($"control_state_{sectorId}");
        _log.Debug("Cache invalidated for sector {SectorId}", sectorId);
    }

    /// <summary>
    /// Clear all cached territory status
    /// </summary>
    public void ClearCache()
    {
        var count = _sectorControlCache.Count;
        _sectorControlCache.Compact(1.0);
        _log.Debug("Cleared {Count} territory status cache entries", count);
    }
}
