namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents an ingredient required for a recipe.
/// </summary>
/// <remarks>
/// <para>
/// Recipe ingredients reference <see cref="Definitions.ResourceDefinition"/> entities
/// by their string identifiers. The quantity specifies how many units of the
/// resource are consumed when the recipe is crafted.
/// </para>
/// <para>
/// Resource IDs are normalized to lowercase for consistent lookups across
/// the resource provider system.
/// </para>
/// <para>
/// This value object is immutable and validated at construction time.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create an ingredient requiring 5 iron ore
/// var ironIngredient = new RecipeIngredient("iron-ore", 5);
///
/// // IDs are normalized to lowercase
/// var copperIngredient = new RecipeIngredient("Copper-Ore", 3);
/// Console.WriteLine(copperIngredient.ResourceId); // Output: copper-ore
///
/// // Display the ingredient
/// Console.WriteLine(ironIngredient.ToString()); // Output: iron-ore x5
/// </code>
/// </example>
public sealed record RecipeIngredient
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier of the required resource.
    /// </summary>
    /// <remarks>
    /// References a <see cref="Definitions.ResourceDefinition.ResourceId"/> value.
    /// The ID is normalized to lowercase for consistent lookups.
    /// </remarks>
    public string ResourceId { get; }

    /// <summary>
    /// Gets the number of resources required.
    /// </summary>
    /// <remarks>
    /// Must be at least 1. Represents the quantity of the resource
    /// that will be consumed when the recipe is crafted.
    /// </remarks>
    public int Quantity { get; }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="RecipeIngredient"/> record.
    /// </summary>
    /// <param name="resourceId">
    /// The unique identifier of the required resource.
    /// Will be normalized to lowercase.
    /// </param>
    /// <param name="quantity">
    /// The number of resources required. Must be at least 1.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="resourceId"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="quantity"/> is less than 1.
    /// </exception>
    /// <example>
    /// <code>
    /// // Valid construction
    /// var ingredient = new RecipeIngredient("iron-ore", 5);
    ///
    /// // Invalid - will throw ArgumentException
    /// var invalid1 = new RecipeIngredient("", 1); // Empty ID
    ///
    /// // Invalid - will throw ArgumentOutOfRangeException
    /// var invalid2 = new RecipeIngredient("iron-ore", 0); // Zero quantity
    /// </code>
    /// </example>
    public RecipeIngredient(string resourceId, int quantity)
    {
        // Validate resourceId is not null or whitespace
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId, nameof(resourceId));

        // Validate quantity is at least 1
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity, nameof(quantity));

        // Normalize resourceId to lowercase for consistent lookups
        ResourceId = resourceId.ToLowerInvariant();
        Quantity = quantity;
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a string representation of the ingredient.
    /// </summary>
    /// <returns>A string in the format "resourceId xQuantity".</returns>
    /// <example>
    /// <code>
    /// var ingredient = new RecipeIngredient("iron-ore", 5);
    /// Console.WriteLine(ingredient.ToString()); // Output: iron-ore x5
    /// </code>
    /// </example>
    public override string ToString() => $"{ResourceId} x{Quantity}";
}
