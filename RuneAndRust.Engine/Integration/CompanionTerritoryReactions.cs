using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine.Integration;

/// <summary>
/// v0.35.4: Integration helpers for Companion System (v0.34) and Territory Control
/// Companions react to entering faction-controlled territory
/// </summary>
public class CompanionTerritoryReactions
{
    private static readonly ILogger _log = Log.ForContext<CompanionTerritoryReactions>();
    private readonly TerritoryService _territoryService;

    // Faction hostility matrix
    private static readonly Dictionary<string, string[]> HostileFactions = new()
    {
        ["IronBanes"] = new[] { "GodSleeperCultists", "Undying" },
        ["GodSleeperCultists"] = new[] { "IronBanes", "JotunReaders" },
        ["JotunReaders"] = new[] { "GodSleeperCultists" },
        ["RustClans"] = Array.Empty<string>(),
        ["Independents"] = Array.Empty<string>()
    };

    public CompanionTerritoryReactions(TerritoryService territoryService)
    {
        _territoryService = territoryService;
    }

    /// <summary>
    /// Get companion reaction when entering a sector
    /// Returns dialogue line and any buff/debuff to apply
    /// </summary>
    public (string dialogue, string? buffName, int buffDuration, int buffValue) GetCompanionReaction(
        Companion companion,
        int sectorId)
    {
        try
        {
            var status = _territoryService.GetSectorTerritoryStatus(sectorId);

            // Same faction = friendly territory
            if (companion.FactionAffiliation == status.DominantFaction)
            {
                _log.Information("Companion {Name} entered friendly territory: {Faction}",
                    companion.CompanionName, status.DominantFaction);

                return (
                    dialogue: "Good to be among allies. This is our territory.",
                    buffName: "Home_Territory_Morale",
                    buffDuration: 10,
                    buffValue: 10 // +10% to combat effectiveness or morale
                );
            }

            // Hostile faction = enemy territory
            if (IsHostileFaction(companion.FactionAffiliation, status.DominantFaction))
            {
                _log.Information("Companion {Name} entered hostile territory: {Faction} (companion is {CompanionFaction})",
                    companion.CompanionName, status.DominantFaction, companion.FactionAffiliation);

                return (
                    dialogue: "We should be careful here. These aren't our friends.",
                    buffName: "Hostile_Territory_Stress",
                    buffDuration: -1, // Persist while in sector
                    buffValue: 5 // +5 stress or anxiety
                );
            }

            // War zone = general danger
            if (status.ControlState == "War")
            {
                _log.Information("Companion {Name} entered war zone in sector {SectorId}",
                    companion.CompanionName, sectorId);

                return (
                    dialogue: "War zone ahead. Stay sharp.",
                    buffName: "Combat_Readiness",
                    buffDuration: -1,
                    buffValue: 5 // +5 awareness
                );
            }

            // Neutral/Independent territory
            return (
                dialogue: "This sector seems neutral. No strong faction presence.",
                buffName: null,
                buffDuration: 0,
                buffValue: 0
            );
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get companion reaction for sector {SectorId}", sectorId);
            return ("...", null, 0, 0);
        }
    }

    /// <summary>
    /// Check if two factions are hostile
    /// </summary>
    private bool IsHostileFaction(string factionA, string? factionB)
    {
        if (string.IsNullOrEmpty(factionB))
            return false;

        if (HostileFactions.TryGetValue(factionA, out var hostileList))
        {
            return hostileList.Contains(factionB);
        }

        // Check reverse relationship
        if (HostileFactions.TryGetValue(factionB, out var reverseHostileList))
        {
            return reverseHostileList.Contains(factionA);
        }

        return false;
    }

    /// <summary>
    /// Get companion comment on sector control state
    /// </summary>
    public string GetCompanionComment(Companion companion, int sectorId)
    {
        var status = _territoryService.GetSectorTerritoryStatus(sectorId);

        return status.ControlState switch
        {
            "War" when status.ActiveWar != null =>
                $"There's a war here between {status.ActiveWar.FactionA} and {status.ActiveWar.FactionB}. Dangerous.",

            "Contested" when status.ContestedFactions != null && status.ContestedFactions.Length > 0 =>
                $"This sector is contested. {string.Join(" and ", status.ContestedFactions)} are fighting for control.",

            "Stable" when status.DominantFaction != null =>
                $"{status.DominantFaction} controls this sector firmly.",

            "Independent" =>
                "Independent territory. No faction has strong control here.",

            _ => "I'm not sure what's happening in this sector."
        };
    }
}
