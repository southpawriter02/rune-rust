namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the types of effects that items can have when used.
/// </summary>
public enum ItemEffect
{
    /// <summary>
    /// No effect. Used for quest items, weapons, etc.
    /// </summary>
    None = 0,

    /// <summary>
    /// Restores player health by the effect value.
    /// </summary>
    Heal = 1,

    /// <summary>
    /// Deals damage to the player (cursed items).
    /// </summary>
    Damage = 2,

    /// <summary>
    /// Temporarily increases attack stat.
    /// </summary>
    BuffAttack = 3,

    /// <summary>
    /// Temporarily increases defense stat.
    /// </summary>
    BuffDefense = 4,

    /// <summary>
    /// Learns a recipe from a recipe scroll when used.
    /// </summary>
    /// <remarks>
    /// This effect is used by recipe scroll items. When an item with this effect
    /// is used, it teaches the player the recipe specified by the item's RecipeId
    /// property. The scroll is consumed on successful learning but preserved if
    /// the player already knows the recipe.
    /// </remarks>
    LearnRecipe = 5
}
