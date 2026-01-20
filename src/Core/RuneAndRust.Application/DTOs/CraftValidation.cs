namespace RuneAndRust.Application.DTOs;

using RuneAndRust.Domain.Definitions;

/// <summary>
/// Represents a resource that is missing or insufficient for crafting.
/// </summary>
/// <remarks>
/// <para>
/// Used to provide detailed feedback about which resources the player
/// needs to gather before they can craft a recipe.
/// </para>
/// </remarks>
/// <param name="ResourceId">The unique identifier of the missing resource.</param>
/// <param name="ResourceName">The display name of the missing resource.</param>
/// <param name="QuantityNeeded">The additional quantity needed to meet the recipe requirements.</param>
/// <example>
/// <code>
/// // Display missing ingredients
/// foreach (var missing in validation.MissingIngredients)
/// {
///     Console.WriteLine($"Need {missing.QuantityNeeded} more {missing.ResourceName}");
/// }
/// </code>
/// </example>
public sealed record MissingIngredient(
    string ResourceId,
    string ResourceName,
    int QuantityNeeded);

/// <summary>
/// Represents the result of validating whether a player can craft a recipe.
/// </summary>
/// <remarks>
/// <para>
/// CraftValidation performs pre-craft checks without consuming resources.
/// Use this to determine if a player can craft something before attempting
/// the actual crafting operation.
/// </para>
/// <para>
/// Validation checks (in order):
/// <list type="number">
///   <item><description>Recipe exists in the system</description></item>
///   <item><description>Player knows the recipe</description></item>
///   <item><description>Player is at the correct crafting station (if specified)</description></item>
///   <item><description>Player has all required ingredients</description></item>
/// </list>
/// </para>
/// <para>
/// Factory methods:
/// <list type="bullet">
///   <item><description><see cref="Success"/> - All checks passed</description></item>
///   <item><description><see cref="Failed(string)"/> - Validation failed with a reason</description></item>
///   <item><description><see cref="InsufficientResources"/> - Missing ingredients</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var validation = recipeService.CanCraft(player, "iron-sword", "anvil");
///
/// if (validation.IsValid)
/// {
///     Console.WriteLine("Ready to craft!");
///     // Proceed with crafting
/// }
/// else if (validation.IsMissingIngredients)
/// {
///     Console.WriteLine("Missing ingredients:");
///     foreach (var missing in validation.MissingIngredients!)
///     {
///         Console.WriteLine($"  {missing.ResourceName} x{missing.QuantityNeeded}");
///     }
/// }
/// else
/// {
///     Console.WriteLine(validation.FailureReason);
/// }
/// </code>
/// </example>
public sealed record CraftValidation
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the player can craft this recipe.
    /// </summary>
    /// <remarks>
    /// True when all validation checks pass: recipe exists, player knows it,
    /// player is at correct station, and player has all ingredients.
    /// </remarks>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the recipe definition, if validation got far enough to find it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Null when the recipe was not found.
    /// </para>
    /// <para>
    /// Contains the full recipe definition for successful validations
    /// and some failure types, allowing callers to display recipe details.
    /// </para>
    /// </remarks>
    public RecipeDefinition? Recipe { get; init; }

    /// <summary>
    /// Gets the human-readable reason for validation failure.
    /// </summary>
    /// <remarks>
    /// Null for successful validations. Contains a descriptive message
    /// for failed validations that can be shown to the player.
    /// </remarks>
    public string? FailureReason { get; init; }

    /// <summary>
    /// Gets the list of missing ingredients, if any.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Null when validation failed for reasons other than missing resources.
    /// </para>
    /// <para>
    /// Contains a list of <see cref="MissingIngredient"/> records when
    /// the player is missing one or more required resources.
    /// </para>
    /// </remarks>
    public IReadOnlyList<MissingIngredient>? MissingIngredients { get; init; }

    /// <summary>
    /// Gets the crafting station where crafting will occur (v0.11.2b).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Null when validation failed before station lookup, or when station
    /// validation is not applicable to the validation failure type.
    /// </para>
    /// <para>
    /// Contains the station definition for successful validations and
    /// resource-based failures where the station was found.
    /// </para>
    /// </remarks>
    public CraftingStationDefinition? Station { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether the validation failed due to missing ingredients.
    /// </summary>
    /// <remarks>
    /// True when <see cref="MissingIngredients"/> contains one or more entries.
    /// Use this to distinguish ingredient failures from other validation failures.
    /// </remarks>
    public bool IsMissingIngredients => MissingIngredients is { Count: > 0 };

    /// <summary>
    /// Gets whether a valid crafting station was found for the validation.
    /// </summary>
    /// <remarks>
    /// True when <see cref="Station"/> is not null, indicating the player
    /// is at a valid crafting station that supports the recipe category.
    /// </remarks>
    public bool HasStation => Station is not null;

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <param name="recipe">The recipe that can be crafted.</param>
    /// <returns>A success validation containing the recipe definition.</returns>
    /// <remarks>
    /// Use this factory when all validation checks pass and the player
    /// can proceed with crafting. Station is not included in this overload.
    /// </remarks>
    /// <example>
    /// <code>
    /// // After all checks pass (no station context)
    /// return CraftValidation.Success(recipe);
    /// </code>
    /// </example>
    public static CraftValidation Success(RecipeDefinition recipe)
        => new()
        {
            IsValid = true,
            Recipe = recipe,
            FailureReason = null,
            MissingIngredients = null,
            Station = null
        };

    /// <summary>
    /// Creates a successful validation result with station context.
    /// </summary>
    /// <param name="recipe">The recipe that can be crafted.</param>
    /// <param name="station">The crafting station where crafting will occur.</param>
    /// <returns>A success validation containing the recipe definition and station.</returns>
    /// <remarks>
    /// Use this factory when all validation checks pass (including station validation)
    /// and the player can proceed with crafting.
    /// </remarks>
    /// <example>
    /// <code>
    /// // After all checks pass including station
    /// return CraftValidation.Success(recipe, station);
    /// </code>
    /// </example>
    public static CraftValidation Success(RecipeDefinition recipe, CraftingStationDefinition station)
        => new()
        {
            IsValid = true,
            Recipe = recipe,
            FailureReason = null,
            MissingIngredients = null,
            Station = station
        };

    /// <summary>
    /// Creates a failed validation result with a reason.
    /// </summary>
    /// <param name="reason">The human-readable reason for failure.</param>
    /// <returns>A failed validation with the specified reason.</returns>
    /// <remarks>
    /// Use this factory for validation failures that are not related to
    /// missing ingredients (e.g., recipe not found, recipe not known,
    /// wrong crafting station).
    /// </remarks>
    /// <example>
    /// <code>
    /// // Recipe not found
    /// return CraftValidation.Failed("Recipe not found.");
    ///
    /// // Recipe not known
    /// return CraftValidation.Failed("You don't know this recipe.");
    ///
    /// // Wrong station
    /// return CraftValidation.Failed("Requires an Anvil to craft.");
    /// </code>
    /// </example>
    public static CraftValidation Failed(string reason)
        => new()
        {
            IsValid = false,
            Recipe = null,
            FailureReason = reason,
            MissingIngredients = null
        };

    /// <summary>
    /// Creates a failed validation result with a reason and the recipe.
    /// </summary>
    /// <param name="reason">The human-readable reason for failure.</param>
    /// <param name="recipe">The recipe that failed validation.</param>
    /// <returns>A failed validation with the specified reason and recipe.</returns>
    /// <remarks>
    /// Use this when validation failed after the recipe was found,
    /// allowing callers to still access the recipe definition.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Player doesn't know this recipe but we want to show details
    /// return CraftValidation.Failed("You don't know this recipe.", recipe);
    /// </code>
    /// </example>
    public static CraftValidation Failed(string reason, RecipeDefinition recipe)
        => new()
        {
            IsValid = false,
            Recipe = recipe,
            FailureReason = reason,
            MissingIngredients = null
        };

    /// <summary>
    /// Creates a validation result indicating insufficient resources.
    /// </summary>
    /// <param name="recipe">The recipe that requires more resources.</param>
    /// <param name="missing">The list of missing ingredients.</param>
    /// <returns>A validation result with the list of missing ingredients.</returns>
    /// <remarks>
    /// <para>
    /// Use this factory when the player knows the recipe and is at the
    /// correct station, but doesn't have enough of one or more ingredients.
    /// </para>
    /// <para>
    /// The missing ingredients list contains only those resources that are
    /// insufficient or completely absent. Each entry shows how many MORE
    /// of that resource are needed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var missing = new List&lt;MissingIngredient&gt;
    /// {
    ///     new("iron-ore", "Iron Ore", 3),  // Need 3 more
    ///     new("leather", "Leather", 1)      // Need 1 more
    /// };
    /// return CraftValidation.InsufficientResources(recipe, missing);
    /// </code>
    /// </example>
    public static CraftValidation InsufficientResources(
        RecipeDefinition recipe,
        IReadOnlyList<MissingIngredient> missing)
        => new()
        {
            IsValid = false,
            Recipe = recipe,
            FailureReason = "Insufficient resources.",
            MissingIngredients = missing,
            Station = null
        };

    /// <summary>
    /// Creates a validation result indicating insufficient resources with station context.
    /// </summary>
    /// <param name="recipe">The recipe that requires more resources.</param>
    /// <param name="missing">The list of missing ingredients.</param>
    /// <param name="station">The crafting station where crafting was attempted.</param>
    /// <returns>A validation result with the list of missing ingredients and station.</returns>
    /// <remarks>
    /// <para>
    /// Use this factory when the player knows the recipe and is at the
    /// correct station, but doesn't have enough of one or more ingredients.
    /// Including the station allows callers to display station-specific information.
    /// </para>
    /// <para>
    /// The missing ingredients list contains only those resources that are
    /// insufficient or completely absent. Each entry shows how many MORE
    /// of that resource are needed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var missing = new List&lt;MissingIngredient&gt;
    /// {
    ///     new("iron-ore", "Iron Ore", 3),  // Need 3 more
    ///     new("leather", "Leather", 1)      // Need 1 more
    /// };
    /// return CraftValidation.InsufficientResources(recipe, missing, station);
    /// </code>
    /// </example>
    public static CraftValidation InsufficientResources(
        RecipeDefinition recipe,
        IReadOnlyList<MissingIngredient> missing,
        CraftingStationDefinition station)
        => new()
        {
            IsValid = false,
            Recipe = recipe,
            FailureReason = "Insufficient resources.",
            MissingIngredients = missing,
            Station = station
        };
}
