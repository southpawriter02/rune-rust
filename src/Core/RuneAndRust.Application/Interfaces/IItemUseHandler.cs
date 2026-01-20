using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Handler interface for processing item use actions.
/// </summary>
/// <remarks>
/// <para>
/// IItemUseHandler follows the strategy pattern, where each handler is responsible
/// for processing a specific type of item effect. Handlers are selected based on
/// their <see cref="HandledEffect"/> property matching the item's Effect.
/// </para>
/// <para>
/// Implementation responsibilities:
/// <list type="bullet">
///   <item><description>Validate the item is appropriate for this handler</description></item>
///   <item><description>Perform the item's effect on the player</description></item>
///   <item><description>Return an appropriate result indicating success/failure and consumption</description></item>
///   <item><description>Log relevant information at appropriate levels</description></item>
/// </list>
/// </para>
/// <para>
/// The handler does NOT remove items from inventory - it returns an <see cref="ItemUseResult"/>
/// indicating whether the item should be consumed. The caller is responsible for
/// managing inventory updates.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Handler registration in DI
/// services.AddScoped&lt;IItemUseHandler, RecipeScrollUseHandler&gt;();
/// services.AddScoped&lt;IItemUseHandler, HealingItemUseHandler&gt;();
///
/// // Handler selection and use
/// var handler = handlers.FirstOrDefault(h => h.HandledEffect == item.Effect);
/// if (handler != null)
/// {
///     var result = handler.Handle(player, item);
///     if (result.WasConsumed)
///     {
///         player.Inventory.RemoveItem(item);
///     }
///     DisplayMessage(result.Message);
/// }
/// </code>
/// </example>
public interface IItemUseHandler
{
    /// <summary>
    /// Gets the item effect this handler processes.
    /// </summary>
    /// <remarks>
    /// Used for handler selection. When a player uses an item, the item's
    /// <see cref="Item.Effect"/> is matched against available handlers'
    /// HandledEffect properties.
    /// </remarks>
    /// <example>
    /// <code>
    /// // RecipeScrollUseHandler
    /// public ItemEffect HandledEffect => ItemEffect.LearnRecipe;
    ///
    /// // HealingItemUseHandler
    /// public ItemEffect HandledEffect => ItemEffect.Heal;
    /// </code>
    /// </example>
    ItemEffect HandledEffect { get; }

    /// <summary>
    /// Handles the use of an item on a player.
    /// </summary>
    /// <param name="player">The player using the item.</param>
    /// <param name="item">The item being used.</param>
    /// <returns>
    /// An <see cref="ItemUseResult"/> indicating:
    /// <list type="bullet">
    ///   <item><description><see cref="ItemUseResult.IsSuccess"/> - Whether the use was successful</description></item>
    ///   <item><description><see cref="ItemUseResult.WasConsumed"/> - Whether the item should be removed from inventory</description></item>
    ///   <item><description><see cref="ItemUseResult.Message"/> - A message describing the result</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// Implementations should:
    /// <list type="number">
    ///   <item><description>Validate the item has the expected effect type</description></item>
    ///   <item><description>Validate any item-specific requirements (e.g., RecipeId for scrolls)</description></item>
    ///   <item><description>Perform the item's action on the player</description></item>
    ///   <item><description>Return an appropriate result</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This method should NOT throw exceptions for expected failure conditions.
    /// Instead, return <see cref="ItemUseResult.Failed"/> with an appropriate message.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when player or item is null.</exception>
    /// <example>
    /// <code>
    /// // Recipe scroll handler
    /// public ItemUseResult Handle(Player player, Item item)
    /// {
    ///     if (item.Effect != HandledEffect)
    ///         return ItemUseResult.Failed("Invalid item for this handler.");
    ///
    ///     if (string.IsNullOrWhiteSpace(item.RecipeId))
    ///         return ItemUseResult.Failed("This scroll has no recipe.");
    ///
    ///     if (_recipeService.IsRecipeKnown(player, item.RecipeId))
    ///         return ItemUseResult.Preserved("You already know this recipe.");
    ///
    ///     var learnResult = _recipeService.LearnRecipe(player, item.RecipeId);
    ///     if (learnResult.IsSuccess)
    ///         return ItemUseResult.ConsumedWithMessage($"You learned {learnResult.RecipeName}!");
    ///
    ///     return ItemUseResult.Failed(learnResult.FailureReason ?? "Failed to learn recipe.");
    /// }
    /// </code>
    /// </example>
    ItemUseResult Handle(Player player, Item item);
}
