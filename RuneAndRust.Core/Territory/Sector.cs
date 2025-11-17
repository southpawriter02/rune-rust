namespace RuneAndRust.Core.Territory;

/// <summary>
/// v0.35.1: Represents a territorial sector within a world
/// Corresponds to database Sectors table
/// </summary>
public class Sector
{
    public int SectorId { get; set; }
    public int WorldId { get; set; }
    public string SectorName { get; set; } = string.Empty;
    public string SectorDescription { get; set; } = string.Empty;
    public int? BiomeId { get; set; } // Optional link to Biomes table
    public string ZLevel { get; set; } = string.Empty; // "Trunk", "Roots", "Canopy"
    public DateTime CreatedAt { get; set; }
}
