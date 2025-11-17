namespace RuneAndRust.Engine.Crafting;

/// <summary>
/// Recipe with component availability information
/// </summary>
public class CraftableRecipe
{
    public RecipeDetails Recipe { get; set; } = null!;
    public List<RecipeComponent> Components { get; set; } = new();
    public bool CanCraft { get; set; }
}
