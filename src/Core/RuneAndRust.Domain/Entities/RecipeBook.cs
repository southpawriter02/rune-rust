using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Tracks recipes known by a player.
/// </summary>
/// <remarks>
/// <para>
/// The recipe book maintains a set of known recipe IDs and timestamps for
/// when each recipe was learned. Players start with default recipes and
/// can learn additional recipes through discovery.
/// </para>
/// <para>
/// Recipe IDs are normalized to lowercase for consistent lookups across
/// the recipe system. All lookups use case-insensitive comparison.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a recipe book for a player
/// var recipeBook = RecipeBook.Create(player.Id);
///
/// // Initialize with default recipes
/// recipeBook.InitializeDefaults(new[] { "iron-sword", "healing-potion" });
///
/// // Learn a new recipe
/// if (recipeBook.Learn("steel-sword"))
/// {
///     Console.WriteLine("Learned Steel Sword recipe!");
/// }
///
/// // Check if a recipe is known
/// if (recipeBook.IsKnown("iron-sword"))
/// {
///     Console.WriteLine($"Known since: {recipeBook.GetLearnedDate("iron-sword")}");
/// }
/// </code>
/// </example>
public sealed class RecipeBook : IEntity
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique entity identifier.
    /// </summary>
    /// <remarks>
    /// This is the primary key for the RecipeBook entity,
    /// satisfying the <see cref="IEntity"/> interface requirement.
    /// </remarks>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the ID of the player who owns this recipe book.
    /// </summary>
    /// <remarks>
    /// Each player has exactly one recipe book. This establishes
    /// a one-to-one relationship between Player and RecipeBook.
    /// </remarks>
    public Guid PlayerId { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // INTERNAL STORAGE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Internal storage for known recipe IDs.
    /// </summary>
    /// <remarks>
    /// Uses case-insensitive comparison to ensure consistent lookups
    /// regardless of how recipe IDs are provided.
    /// </remarks>
    private readonly HashSet<string> _knownRecipeIds = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Internal storage for when recipes were learned.
    /// </summary>
    /// <remarks>
    /// Tracks the UTC DateTime when each recipe was learned,
    /// keyed by the normalized (lowercase) recipe ID.
    /// </remarks>
    private readonly Dictionary<string, DateTime> _learnedAt = new(StringComparer.OrdinalIgnoreCase);

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets all known recipe IDs.
    /// </summary>
    /// <remarks>
    /// Returns a read-only view of the known recipe IDs.
    /// The IDs are normalized to lowercase.
    /// </remarks>
    public IReadOnlySet<string> KnownRecipeIds => _knownRecipeIds;

    /// <summary>
    /// Gets the count of known recipes.
    /// </summary>
    /// <remarks>
    /// This is the total number of recipes the player has learned,
    /// including both default recipes and discovered recipes.
    /// </remarks>
    public int KnownCount => _knownRecipeIds.Count;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for factory pattern and EF Core.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Create(Guid)"/> to create new instances.
    /// </remarks>
    private RecipeBook() { }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new recipe book for a player.
    /// </summary>
    /// <param name="playerId">The ID of the player who owns this book.</param>
    /// <returns>A new RecipeBook instance with no known recipes.</returns>
    /// <remarks>
    /// <para>
    /// The recipe book is created empty. Call <see cref="InitializeDefaults"/>
    /// to populate it with default recipes, or use the RecipeService's
    /// InitializeDefaultRecipes method for proper initialization.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var player = new Player("Hero");
    /// var recipeBook = RecipeBook.Create(player.Id);
    /// // Now call recipeService.InitializeDefaultRecipes(player)
    /// </code>
    /// </example>
    public static RecipeBook Create(Guid playerId)
    {
        return new RecipeBook
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if a recipe is known.
    /// </summary>
    /// <param name="recipeId">The recipe ID to check.</param>
    /// <returns>True if the recipe is known; otherwise, false.</returns>
    /// <remarks>
    /// <para>
    /// The lookup is case-insensitive. Both "iron-sword" and "Iron-Sword"
    /// will match if the recipe is known.
    /// </para>
    /// <para>
    /// Returns false for null or whitespace recipe IDs.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (recipeBook.IsKnown("iron-sword"))
    /// {
    ///     // Player can craft iron swords
    /// }
    /// </code>
    /// </example>
    public bool IsKnown(string recipeId)
    {
        // Handle null or whitespace gracefully
        if (string.IsNullOrWhiteSpace(recipeId))
        {
            return false;
        }

        // Lookup is case-insensitive due to StringComparer.OrdinalIgnoreCase
        return _knownRecipeIds.Contains(recipeId);
    }

    /// <summary>
    /// Gets when a recipe was learned.
    /// </summary>
    /// <param name="recipeId">The recipe ID to query.</param>
    /// <returns>The DateTime when learned (UTC), or null if not known.</returns>
    /// <remarks>
    /// <para>
    /// Returns null if the recipe is not known or if the recipe ID is invalid.
    /// </para>
    /// <para>
    /// The returned DateTime is in UTC format.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var learnedDate = recipeBook.GetLearnedDate("iron-sword");
    /// if (learnedDate.HasValue)
    /// {
    ///     var daysAgo = (DateTime.UtcNow - learnedDate.Value).Days;
    ///     Console.WriteLine($"Learned {daysAgo} days ago");
    /// }
    /// </code>
    /// </example>
    public DateTime? GetLearnedDate(string recipeId)
    {
        // Handle null or whitespace gracefully
        if (string.IsNullOrWhiteSpace(recipeId))
        {
            return null;
        }

        // Lookup is case-insensitive due to StringComparer.OrdinalIgnoreCase
        return _learnedAt.TryGetValue(recipeId, out var date) ? date : null;
    }

    // ═══════════════════════════════════════════════════════════════
    // MUTATION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Learns a recipe. Returns false if already known.
    /// </summary>
    /// <param name="recipeId">The recipe ID to learn.</param>
    /// <returns>True if the recipe was newly learned; false if already known.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="recipeId"/> is null, empty, or whitespace.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The recipe ID is normalized to lowercase before storage.
    /// Learning a recipe records the current UTC time as the learned date.
    /// </para>
    /// <para>
    /// If the recipe is already known, this method returns false without
    /// updating the learned date.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (recipeBook.Learn("steel-sword"))
    /// {
    ///     Console.WriteLine("You learned how to craft a Steel Sword!");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("You already know this recipe.");
    /// }
    /// </code>
    /// </example>
    public bool Learn(string recipeId)
    {
        // Validate input - null/empty/whitespace not allowed
        ArgumentException.ThrowIfNullOrWhiteSpace(recipeId, nameof(recipeId));

        // Normalize to lowercase for consistent storage
        var normalizedId = recipeId.ToLowerInvariant();

        // Check if already known
        if (_knownRecipeIds.Contains(normalizedId))
        {
            return false;
        }

        // Add to known recipes and record the learn time
        _knownRecipeIds.Add(normalizedId);
        _learnedAt[normalizedId] = DateTime.UtcNow;

        return true;
    }

    /// <summary>
    /// Initializes the recipe book with default recipes.
    /// </summary>
    /// <param name="defaultRecipeIds">The IDs of default recipes to learn.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="defaultRecipeIds"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is called during player creation to give new players
    /// access to basic crafting recipes. Null or whitespace recipe IDs
    /// in the collection are silently skipped.
    /// </para>
    /// <para>
    /// If a recipe ID is already known (e.g., from a previous call),
    /// it will be skipped without updating its learned date.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Initialize with default recipes from RecipeProvider
    /// var defaults = recipeProvider.GetDefaultRecipes()
    ///     .Select(r => r.RecipeId);
    /// recipeBook.InitializeDefaults(defaults);
    /// </code>
    /// </example>
    public void InitializeDefaults(IEnumerable<string> defaultRecipeIds)
    {
        ArgumentNullException.ThrowIfNull(defaultRecipeIds, nameof(defaultRecipeIds));

        foreach (var recipeId in defaultRecipeIds)
        {
            // Skip null or whitespace entries silently
            if (!string.IsNullOrWhiteSpace(recipeId))
            {
                Learn(recipeId);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a summary string for display.
    /// </summary>
    /// <returns>A string describing known recipe count.</returns>
    /// <example>
    /// <code>
    /// Console.WriteLine(recipeBook.ToString());
    /// // Output: RecipeBook (9 recipes known)
    /// </code>
    /// </example>
    public override string ToString() => $"RecipeBook ({KnownCount} recipes known)";
}
