namespace RuneAndRust.Engine.Crafting;

/// <summary>
/// Extended recipe information including discovery metadata
/// </summary>
public class RecipeDetails : Recipe
{
    public int TimesCrafted { get; set; }
    public DateTime? DiscoveredAt { get; set; }
    public string DiscoverySource { get; set; } = string.Empty;
}
