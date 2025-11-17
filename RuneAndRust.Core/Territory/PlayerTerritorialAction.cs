namespace RuneAndRust.Core.Territory;

/// <summary>
/// v0.35.4: Player territorial action record
/// Corresponds to Player_Territorial_Actions database table
/// </summary>
public class PlayerTerritorialAction
{
    public int ActionId { get; set; }
    public int CharacterId { get; set; }
    public int WorldId { get; set; }
    public int SectorId { get; set; }
    public string ActionType { get; set; } = string.Empty; // "Kill_Enemy", "Complete_Quest", etc.
    public string AffectedFaction { get; set; } = string.Empty;
    public double InfluenceDelta { get; set; }
    public DateTime ActionTimestamp { get; set; }
    public string? Notes { get; set; }
}
