using RuneAndRust.Core.Territory;

namespace RuneAndRust.Core.Territory;

/// <summary>
/// v0.35.4: Complete territory status for a sector
/// Aggregates control state, influences, wars, and events
/// </summary>
public class TerritoryStatus
{
    public int SectorId { get; set; }
    public string SectorName { get; set; } = string.Empty;
    public string ControlState { get; set; } = string.Empty; // "Stable", "Contested", "War", "Independent", "Ruined"
    public string? DominantFaction { get; set; }
    public string[]? ContestedFactions { get; set; }
    public List<FactionInfluence> FactionInfluences { get; set; } = new();
    public FactionWar? ActiveWar { get; set; }
    public List<WorldEvent> ActiveEvents { get; set; } = new();
}
