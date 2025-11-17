using Serilog;

namespace RuneAndRust.Engine.Integration;

/// <summary>
/// v0.35.4: Integration helpers for Faction System (v0.33) and Territory Control
/// Provides reputation-based modifiers for territorial influence
/// </summary>
public class FactionTerritoryIntegration
{
    private static readonly ILogger _log = Log.ForContext<FactionTerritoryIntegration>();
    private readonly TerritoryService _territoryService;
    private readonly ReputationService _reputationService;

    public FactionTerritoryIntegration(
        TerritoryService territoryService,
        ReputationService reputationService)
    {
        _territoryService = territoryService;
        _reputationService = reputationService;
    }

    /// <summary>
    /// Calculate reputation gain modified by territory control
    /// Players gain bonus reputation when helping a faction in their home territory
    /// </summary>
    public int CalculateReputationGain(
        int characterId,
        int factionId,
        int baseReputation,
        int sectorId)
    {
        try
        {
            var status = _territoryService.GetSectorTerritoryStatus(sectorId);

            // Get faction name from ID
            string? factionName = GetFactionNameById(factionId);
            if (factionName == null)
            {
                _log.Warning("Could not find faction name for ID {FactionId}", factionId);
                return baseReputation;
            }

            // Bonus reputation if helping faction in their home territory
            double multiplier = status.DominantFaction == factionName ? 1.5 : 1.0;

            int modifiedReputation = (int)(baseReputation * multiplier);

            _log.Debug(
                "Reputation gain calculated: Base={Base}, Multiplier={Mult}x, Final={Final}, " +
                "Faction={Faction}, Sector={Sector}, DominantFaction={Dominant}",
                baseReputation, multiplier, modifiedReputation,
                factionName, sectorId, status.DominantFaction);

            return modifiedReputation;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to calculate modified reputation gain");
            return baseReputation;
        }
    }

    /// <summary>
    /// Get faction influence power multiplier based on reputation
    /// -100 to +100 reputation → 0.5x to 1.5x influence power
    /// </summary>
    public double GetInfluencePowerMultiplier(int reputation)
    {
        double multiplier = Math.Clamp(1.0 + (reputation / 200.0), 0.5, 1.5);

        _log.Debug("Influence power multiplier: Reputation={Rep}, Multiplier={Mult}x",
            reputation, multiplier);

        return multiplier;
    }

    /// <summary>
    /// Check if player should receive territory-based reputation bonus
    /// </summary>
    public bool IsPlayerInFriendlyTerritory(int characterId, int factionId, int sectorId)
    {
        var status = _territoryService.GetSectorTerritoryStatus(sectorId);
        string? factionName = GetFactionNameById(factionId);

        bool isFriendly = factionName != null && status.DominantFaction == factionName;

        _log.Debug("Friendly territory check: Character={CharId}, Faction={FacId}, Sector={Sec}, IsFriendly={IsFriendly}",
            characterId, factionId, sectorId, isFriendly);

        return isFriendly;
    }

    /// <summary>
    /// Helper: Get faction name from ID
    /// </summary>
    private string? GetFactionNameById(int factionId)
    {
        // This is a simplified mapping - in a full implementation,
        // this would query the Factions table
        return factionId switch
        {
            1 => "IronBanes",
            2 => "JotunReaders",
            3 => "GodSleeperCultists",
            4 => "RustClans",
            5 => "Independents",
            _ => null
        };
    }
}
