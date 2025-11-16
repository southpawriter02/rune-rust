namespace RuneAndRust.Core.Territory;

/// <summary>
/// v0.35.2: Represents an active or historical faction war
/// Corresponds to database Faction_Wars table
/// </summary>
public class FactionWar
{
    public int WarId { get; set; }
    public int WorldId { get; set; }
    public int SectorId { get; set; }
    public string FactionA { get; set; } = string.Empty;
    public string FactionB { get; set; } = string.Empty;
    public DateTime WarStartDate { get; set; }
    public DateTime? WarEndDate { get; set; }
    public double WarBalance { get; set; } // -100 to +100 (+ favors faction_a)
    public bool IsActive { get; set; }
    public string? Victor { get; set; }
    public int CollateralDamage { get; set; } // Percentage increase in hazard density
}
