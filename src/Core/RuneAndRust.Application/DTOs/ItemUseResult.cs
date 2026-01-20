namespace RuneAndRust.Application.DTOs;

/// <summary>
/// The type of result from an item use operation.
/// </summary>
/// <remarks>
/// Used by <see cref="ItemUseResult"/> to indicate the outcome
/// of attempting to use a consumable or special item.
/// </remarks>
public enum ItemUseResultType
{
    /// <summary>
    /// The item was used successfully and consumed.
    /// </summary>
    /// <remarks>
    /// The item should be removed from the player's inventory.
    /// </remarks>
    Consumed,

    /// <summary>
    /// The item was used but preserved (not consumed).
    /// </summary>
    /// <remarks>
    /// The item remains in the player's inventory. This is used
    /// for cases like recipe scrolls when the player already knows
    /// the recipe - the use was successful but the scroll is kept.
    /// </remarks>
    Preserved,

    /// <summary>
    /// The item use failed.
    /// </summary>
    /// <remarks>
    /// The item remains in the player's inventory. The use operation
    /// could not be completed for some reason (e.g., invalid item,
    /// missing prerequisites, or target unavailable).
    /// </remarks>
    Failed
}

/// <summary>
/// Represents the result of attempting to use an item.
/// </summary>
/// <remarks>
/// <para>
/// ItemUseResult is an immutable record that captures all information
/// about an item use attempt. Use the static factory methods to create
/// instances rather than the constructor directly.
/// </para>
/// <para>
/// Factory methods:
/// <list type="bullet">
///   <item><description><see cref="ConsumedWithMessage"/> - Item was used and should be removed from inventory</description></item>
///   <item><description><see cref="Preserved"/> - Item was used but remains in inventory</description></item>
///   <item><description><see cref="Failed"/> - Item could not be used</description></item>
/// </list>
/// </para>
/// <para>
/// Use cases:
/// <list type="bullet">
///   <item><description>Recipe scrolls: Consumed when learning new recipe, Preserved when already known</description></item>
///   <item><description>Health potions: Consumed on use</description></item>
///   <item><description>Buff items: Consumed on successful application</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Using the result
/// var result = itemUseHandler.Handle(player, recipeScroll);
///
/// if (result.IsSuccess)
/// {
///     if (result.WasConsumed)
///     {
///         player.Inventory.RemoveItem(recipeScroll);
///     }
///     Console.WriteLine(result.Message);
/// }
/// else
/// {
///     Console.WriteLine($"Failed: {result.Message}");
/// }
/// </code>
/// </example>
public sealed record ItemUseResult
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the item use was successful.
    /// </summary>
    /// <remarks>
    /// True for both Consumed and Preserved results. False only for Failed results.
    /// </remarks>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets whether the item should be removed from inventory.
    /// </summary>
    /// <remarks>
    /// True only for Consumed results. The caller is responsible for
    /// actually removing the item from inventory.
    /// </remarks>
    public bool WasConsumed { get; init; }

    /// <summary>
    /// Gets a human-readable message describing the result.
    /// </summary>
    /// <remarks>
    /// Contains a descriptive message suitable for display to the player,
    /// explaining what happened when the item was used.
    /// </remarks>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the type of result indicating the outcome category.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description><see cref="ItemUseResultType.Consumed"/> - Item used and should be removed</description></item>
    ///   <item><description><see cref="ItemUseResultType.Preserved"/> - Item used but kept in inventory</description></item>
    ///   <item><description><see cref="ItemUseResultType.Failed"/> - Item could not be used</description></item>
    /// </list>
    /// </remarks>
    public ItemUseResultType ResultType { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful result where the item was consumed.
    /// </summary>
    /// <param name="message">A message describing what happened.</param>
    /// <returns>A consumed result with the specified message.</returns>
    /// <remarks>
    /// Use this factory when the item was successfully used and should
    /// be removed from the player's inventory.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Recipe scroll successfully teaches a recipe
    /// return ItemUseResult.ConsumedWithMessage(
    ///     $"You learned the recipe for {recipeName}!");
    ///
    /// // Health potion used
    /// return ItemUseResult.ConsumedWithMessage(
    ///     $"You recovered {healAmount} health.");
    /// </code>
    /// </example>
    public static ItemUseResult ConsumedWithMessage(string message)
        => new()
        {
            IsSuccess = true,
            WasConsumed = true,
            Message = message,
            ResultType = ItemUseResultType.Consumed
        };

    /// <summary>
    /// Creates a successful result where the item was preserved.
    /// </summary>
    /// <param name="message">A message describing what happened.</param>
    /// <returns>A preserved result with the specified message.</returns>
    /// <remarks>
    /// Use this factory when the item was used successfully but should
    /// remain in the player's inventory. Common for recipe scrolls when
    /// the player already knows the recipe.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Recipe scroll for already-known recipe
    /// return ItemUseResult.Preserved(
    ///     "You already know this recipe. The scroll has been preserved.");
    /// </code>
    /// </example>
    public static ItemUseResult Preserved(string message)
        => new()
        {
            IsSuccess = true,
            WasConsumed = false,
            Message = message,
            ResultType = ItemUseResultType.Preserved
        };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="message">A message describing why the use failed.</param>
    /// <returns>A failed result with the specified message.</returns>
    /// <remarks>
    /// Use this factory when the item could not be used. The item remains
    /// in the player's inventory.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Invalid item passed to handler
    /// return ItemUseResult.Failed(
    ///     "This item cannot be used this way.");
    ///
    /// // Recipe not found
    /// return ItemUseResult.Failed(
    ///     "The recipe on this scroll is illegible.");
    /// </code>
    /// </example>
    public static ItemUseResult Failed(string message)
        => new()
        {
            IsSuccess = false,
            WasConsumed = false,
            Message = message,
            ResultType = ItemUseResultType.Failed
        };
}
