namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the output produced by a recipe when successfully crafted.
/// </summary>
/// <remarks>
/// <para>
/// Recipe outputs reference item definitions by their string identifiers.
/// The quantity specifies how many items are produced when the recipe
/// is crafted successfully.
/// </para>
/// <para>
/// The optional <see cref="QualityFormula"/> enables roll-based quality
/// determination for high-tier recipes where exceptional crafting results
/// can produce better quality items.
/// </para>
/// <para>
/// Item IDs are normalized to lowercase for consistent lookups across
/// the item provider system.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Basic output: 1 iron sword
/// var basicOutput = new RecipeOutput("iron-sword", 1);
/// Console.WriteLine(basicOutput.HasQualityScaling); // Output: false
///
/// // Advanced output with quality scaling
/// var advancedOutput = new RecipeOutput(
///     "mithril-blade",
///     1,
///     "roll >= 20 ? 'Legendary' : roll >= 15 ? 'Epic' : 'Rare'");
/// Console.WriteLine(advancedOutput.HasQualityScaling); // Output: true
///
/// // Display the output
/// Console.WriteLine(basicOutput.ToString()); // Output: iron-sword x1
/// Console.WriteLine(advancedOutput.ToString()); // Output: mithril-blade x1 (quality scaling)
/// </code>
/// </example>
public sealed record RecipeOutput
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier of the produced item.
    /// </summary>
    /// <remarks>
    /// References an item definition's ID value.
    /// The ID is normalized to lowercase for consistent lookups.
    /// </remarks>
    public string ItemId { get; }

    /// <summary>
    /// Gets the number of items produced.
    /// </summary>
    /// <remarks>
    /// Must be at least 1. Represents the quantity of items
    /// that will be created when the recipe is crafted successfully.
    /// </remarks>
    public int Quantity { get; }

    /// <summary>
    /// Gets the optional quality determination formula.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When set, the formula is evaluated against the crafting roll to determine
    /// the output item's quality tier. The formula uses a simple expression format
    /// where 'roll' represents the crafting skill check result.
    /// </para>
    /// <para>
    /// Example formula: "roll >= 20 ? 'Legendary' : roll >= 15 ? 'Epic' : 'Rare'"
    /// </para>
    /// <para>
    /// When null or empty, the item is produced at its default quality.
    /// </para>
    /// </remarks>
    public string? QualityFormula { get; }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a value indicating whether this output has quality scaling.
    /// </summary>
    /// <remarks>
    /// Returns true if a <see cref="QualityFormula"/> is set, meaning
    /// the output quality varies based on the crafting roll result.
    /// </remarks>
    public bool HasQualityScaling => !string.IsNullOrEmpty(QualityFormula);

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="RecipeOutput"/> record.
    /// </summary>
    /// <param name="itemId">
    /// The unique identifier of the produced item.
    /// Will be normalized to lowercase.
    /// </param>
    /// <param name="quantity">
    /// The number of items produced. Must be at least 1.
    /// </param>
    /// <param name="qualityFormula">
    /// Optional formula for roll-based quality determination.
    /// When set, the crafting roll determines the output quality tier.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="itemId"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="quantity"/> is less than 1.
    /// </exception>
    /// <example>
    /// <code>
    /// // Basic output without quality scaling
    /// var basic = new RecipeOutput("iron-sword", 1);
    ///
    /// // Output with quality scaling based on crafting roll
    /// var scaled = new RecipeOutput(
    ///     "mithril-blade",
    ///     1,
    ///     "roll >= 20 ? 'Legendary' : roll >= 15 ? 'Epic' : 'Rare'");
    ///
    /// // Invalid - will throw ArgumentException
    /// var invalid1 = new RecipeOutput("", 1); // Empty ID
    ///
    /// // Invalid - will throw ArgumentOutOfRangeException
    /// var invalid2 = new RecipeOutput("iron-sword", 0); // Zero quantity
    /// </code>
    /// </example>
    public RecipeOutput(string itemId, int quantity, string? qualityFormula = null)
    {
        // Validate itemId is not null or whitespace
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId, nameof(itemId));

        // Validate quantity is at least 1
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity, nameof(quantity));

        // Normalize itemId to lowercase for consistent lookups
        ItemId = itemId.ToLowerInvariant();
        Quantity = quantity;
        QualityFormula = qualityFormula;
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a string representation of the output.
    /// </summary>
    /// <returns>
    /// A string describing the output. Includes "(quality scaling)" suffix
    /// if a quality formula is set.
    /// </returns>
    /// <example>
    /// <code>
    /// var basic = new RecipeOutput("iron-sword", 1);
    /// Console.WriteLine(basic.ToString()); // Output: iron-sword x1
    ///
    /// var scaled = new RecipeOutput("mithril-blade", 1, "roll >= 20 ? 'Legendary' : 'Rare'");
    /// Console.WriteLine(scaled.ToString()); // Output: mithril-blade x1 (quality scaling)
    /// </code>
    /// </example>
    public override string ToString()
    {
        var result = $"{ItemId} x{Quantity}";

        if (HasQualityScaling)
        {
            result += " (quality scaling)";
        }

        return result;
    }
}
