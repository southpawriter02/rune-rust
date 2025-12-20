using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Entities;

/// <summary>
/// Defines a crafting recipe with its requirements and outputs.
/// Recipes are stored in the RecipeRegistry and referenced by ID.
/// </summary>
public class Recipe
{
    /// <summary>
    /// Unique identifier for the recipe (e.g., "RCP_ALC_STIM").
    /// </summary>
    public string RecipeId { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the recipe.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Narrative description of the crafting process and result.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The crafting trade this recipe belongs to.
    /// </summary>
    public CraftingTrade Trade { get; set; }

    /// <summary>
    /// Base difficulty class for the crafting roll.
    /// Success requires net successes >= BaseDc.
    /// Masterwork requires net successes >= BaseDc + 5.
    /// </summary>
    public int BaseDc { get; set; }

    /// <summary>
    /// Required ingredients as ItemId -> Quantity pairs.
    /// All ingredients must be present in the crafter's inventory.
    /// </summary>
    public Dictionary<string, int> Ingredients { get; set; } = new();

    /// <summary>
    /// The ItemId of the item produced on successful craft.
    /// </summary>
    public string OutputItemId { get; set; } = string.Empty;

    /// <summary>
    /// Number of output items produced on success.
    /// </summary>
    public int OutputQuantity { get; set; } = 1;

    /// <summary>
    /// Optional keywords required to unlock this recipe.
    /// Empty list means the recipe is always available.
    /// </summary>
    public List<string> RequiredKeywords { get; set; } = new();
}
