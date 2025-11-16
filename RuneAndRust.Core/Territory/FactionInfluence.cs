namespace RuneAndRust.Core.Territory;

/// <summary>
/// v0.35.2: Represents faction influence in a sector
/// Corresponds to database Faction_Territory_Control table
/// </summary>
public class FactionInfluence
{
    public int TerritoryControlId { get; set; }
    public int WorldId { get; set; }
    public int SectorId { get; set; }
    public string FactionName { get; set; } = string.Empty;
    public double InfluenceValue { get; set; } // 0.0 to 100.0
    public string ControlState { get; set; } = string.Empty; // "Stable", "Contested", "War", "Independent", "Ruined"
    public DateTime LastUpdated { get; set; }
}
