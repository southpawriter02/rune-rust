namespace RuneAndRust.Application.DTOs;

/// <summary>
/// The type of result from a learn recipe operation.
/// </summary>
/// <remarks>
/// Used by <see cref="LearnRecipeResult"/> to indicate the outcome
/// of attempting to learn a crafting recipe.
/// </remarks>
public enum LearnResultType
{
    /// <summary>
    /// The recipe was successfully learned.
    /// </summary>
    /// <remarks>
    /// The recipe has been added to the player's recipe book
    /// and they can now craft it (subject to other requirements).
    /// </remarks>
    Success,

    /// <summary>
    /// The recipe was already known by the player.
    /// </summary>
    /// <remarks>
    /// The player already has this recipe in their recipe book.
    /// No changes were made.
    /// </remarks>
    AlreadyKnown,

    /// <summary>
    /// The recipe does not exist in the game.
    /// </summary>
    /// <remarks>
    /// The provided recipe ID does not match any recipe definition
    /// in the recipe provider.
    /// </remarks>
    NotFound
}

/// <summary>
/// Represents the result of attempting to learn a crafting recipe.
/// </summary>
/// <remarks>
/// <para>
/// LearnRecipeResult is an immutable record that captures all information
/// about a recipe learning attempt. Use the static factory methods to create
/// instances rather than the constructor directly.
/// </para>
/// <para>
/// Factory methods:
/// <list type="bullet">
///   <item><description><see cref="Success"/> - Recipe was newly learned</description></item>
///   <item><description><see cref="AlreadyKnown"/> - Recipe was already in the recipe book</description></item>
///   <item><description><see cref="RecipeNotFound"/> - Recipe ID doesn't exist</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Using the result
/// var result = recipeService.LearnRecipe(player, "steel-sword");
///
/// if (result.IsSuccess)
/// {
///     Console.WriteLine($"You learned how to craft {result.RecipeName}!");
/// }
/// else
/// {
///     Console.WriteLine(result.FailureReason);
/// }
///
/// // Pattern matching on result type
/// var message = result.ResultType switch
/// {
///     LearnResultType.Success => $"Learned: {result.RecipeName}",
///     LearnResultType.AlreadyKnown => "You already know this recipe.",
///     LearnResultType.NotFound => "Unknown recipe.",
///     _ => "Unexpected result."
/// };
/// </code>
/// </example>
public sealed record LearnRecipeResult
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the recipe was successfully learned.
    /// </summary>
    /// <remarks>
    /// True only when the recipe was newly added to the player's recipe book.
    /// False for AlreadyKnown or NotFound results.
    /// </remarks>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the type of result indicating the outcome category.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description><see cref="LearnResultType.Success"/> - Recipe learned successfully</description></item>
    ///   <item><description><see cref="LearnResultType.AlreadyKnown"/> - Player already knows this recipe</description></item>
    ///   <item><description><see cref="LearnResultType.NotFound"/> - Recipe doesn't exist</description></item>
    /// </list>
    /// </remarks>
    public LearnResultType ResultType { get; init; }

    /// <summary>
    /// Gets the recipe ID that was attempted to be learned.
    /// </summary>
    /// <remarks>
    /// This is the normalized (lowercase) recipe ID that was provided
    /// to the learn operation.
    /// </remarks>
    public string RecipeId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name of the recipe, if found.
    /// </summary>
    /// <remarks>
    /// Null when the recipe was not found (<see cref="LearnResultType.NotFound"/>).
    /// Contains the recipe's display name for Success and AlreadyKnown results.
    /// </remarks>
    public string? RecipeName { get; init; }

    /// <summary>
    /// Gets the human-readable reason for failure, if applicable.
    /// </summary>
    /// <remarks>
    /// Null for successful operations. Contains a descriptive message
    /// for AlreadyKnown and NotFound results that can be shown to the player.
    /// </remarks>
    public string? FailureReason { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful learn result.
    /// </summary>
    /// <param name="recipeId">The learned recipe ID.</param>
    /// <param name="name">The learned recipe's display name.</param>
    /// <returns>A success result with the recipe details.</returns>
    /// <remarks>
    /// Use this factory when the recipe was newly learned and added
    /// to the player's recipe book.
    /// </remarks>
    /// <example>
    /// <code>
    /// return LearnRecipeResult.Success("steel-sword", "Steel Sword");
    /// </code>
    /// </example>
    public static LearnRecipeResult Success(string recipeId, string name)
        => new()
        {
            IsSuccess = true,
            ResultType = LearnResultType.Success,
            RecipeId = recipeId,
            RecipeName = name,
            FailureReason = null
        };

    /// <summary>
    /// Creates a result indicating the recipe was already known.
    /// </summary>
    /// <param name="recipeId">The recipe ID.</param>
    /// <param name="name">The recipe's display name.</param>
    /// <returns>An already-known result with the recipe details.</returns>
    /// <remarks>
    /// Use this factory when the player already has the recipe in their
    /// recipe book. The recipe book was not modified.
    /// </remarks>
    /// <example>
    /// <code>
    /// return LearnRecipeResult.AlreadyKnown("iron-sword", "Iron Sword");
    /// </code>
    /// </example>
    public static LearnRecipeResult AlreadyKnown(string recipeId, string name)
        => new()
        {
            IsSuccess = false,
            ResultType = LearnResultType.AlreadyKnown,
            RecipeId = recipeId,
            RecipeName = name,
            FailureReason = "You already know this recipe."
        };

    /// <summary>
    /// Creates a result indicating the recipe was not found.
    /// </summary>
    /// <param name="recipeId">The recipe ID that was not found.</param>
    /// <returns>A not-found result.</returns>
    /// <remarks>
    /// Use this factory when the provided recipe ID doesn't match any
    /// recipe definition in the recipe provider.
    /// </remarks>
    /// <example>
    /// <code>
    /// return LearnRecipeResult.RecipeNotFound("nonexistent-recipe");
    /// </code>
    /// </example>
    public static LearnRecipeResult RecipeNotFound(string recipeId)
        => new()
        {
            IsSuccess = false,
            ResultType = LearnResultType.NotFound,
            RecipeId = recipeId,
            RecipeName = null,
            FailureReason = "Recipe not found."
        };
}
