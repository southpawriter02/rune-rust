namespace RuneAndRust.Engine.Crafting;

/// <summary>
/// Component requirement for inscriptions (parsed from JSON)
/// </summary>
public class ComponentRequirement
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public int MinQuality { get; set; }
}
