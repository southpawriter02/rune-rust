namespace RuneAndRust.Core.Territory;

/// <summary>
/// v0.35.2: Represents the calculated control state of a sector
/// Used by TerritoryControlService to return control state information
/// </summary>
public class SectorControlState
{
    /// <summary>
    /// Control state: "Stable", "Contested", "War", "Independent", or "Ruined"
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Faction with highest influence (null if contested or war)
    /// </summary>
    public string? DominantFaction { get; set; }

    /// <summary>
    /// Factions competing for control (for contested/war states)
    /// </summary>
    public string[]? ContestedFactions { get; set; }
}
