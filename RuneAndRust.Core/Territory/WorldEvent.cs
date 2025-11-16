namespace RuneAndRust.Core.Territory;

/// <summary>
/// v0.35.3: Represents a dynamic world event
/// Corresponds to database World_Events table
/// </summary>
public class WorldEvent
{
    public int EventId { get; set; }
    public int WorldId { get; set; }
    public int? SectorId { get; set; } // Null for world-wide events
    public string EventType { get; set; } = string.Empty; // "Faction_War", "Incursion", etc.
    public string? AffectedFaction { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public string EventDescription { get; set; } = string.Empty;
    public DateTime EventStartDate { get; set; }
    public DateTime? EventEndDate { get; set; }
    public int EventDurationDays { get; set; }
    public bool IsResolved { get; set; }
    public bool PlayerInfluenced { get; set; }
    public string? Outcome { get; set; }
    public double InfluenceChange { get; set; }
}
