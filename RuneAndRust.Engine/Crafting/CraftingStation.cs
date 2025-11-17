namespace RuneAndRust.Engine.Crafting;

/// <summary>
/// Represents a crafting station that can be used for crafting
/// </summary>
public class CraftingStation
{
    public int StationId { get; set; }
    public string StationType { get; set; } = string.Empty; // Forge, Workshop, Laboratory, Runic_Altar, Field_Station
    public string StationName { get; set; } = string.Empty;
    public int MaxQualityTier { get; set; } = 1; // 1-5: Maximum quality this station can produce
    public int? LocationSectorId { get; set; } // null for portable stations
    public int? LocationRoomId { get; set; }
    public bool RequiresControlling { get; set; } = false; // Must control the sector to use
    public int UsageCostCredits { get; set; } = 0;
    public string StationDescription { get; set; } = string.Empty;
}
