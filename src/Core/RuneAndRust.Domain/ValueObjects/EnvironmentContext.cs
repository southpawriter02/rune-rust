namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the environmental state of a location.
/// </summary>
/// <remarks>
/// EnvironmentContext captures the current environment category values for a room,
/// enabling coherent descriptor selection. Values are stored as a dictionary of
/// category ID to value ID pairs.
/// </remarks>
public readonly record struct EnvironmentContext
{
    /// <summary>
    /// Category values keyed by category ID.
    /// </summary>
    public IReadOnlyDictionary<string, string> CategoryValues { get; init; }

    /// <summary>
    /// Tags derived from category values and biome.
    /// </summary>
    public IReadOnlyList<string> DerivedTags { get; init; }

    /// <summary>
    /// Creates an empty environment context.
    /// </summary>
    public EnvironmentContext()
    {
        CategoryValues = new Dictionary<string, string>();
        DerivedTags = [];
    }

    /// <summary>
    /// Creates an environment context with the specified category values.
    /// </summary>
    /// <param name="categoryValues">Category ID to value ID mappings.</param>
    /// <param name="derivedTags">Tags derived from the environment.</param>
    public EnvironmentContext(
        IReadOnlyDictionary<string, string> categoryValues,
        IReadOnlyList<string> derivedTags)
    {
        CategoryValues = categoryValues ?? new Dictionary<string, string>();
        DerivedTags = derivedTags ?? [];
    }

    /// <summary>
    /// Gets the value for a specific category.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    /// <returns>The value ID if set, otherwise null.</returns>
    public string? GetValue(string categoryId) =>
        CategoryValues.TryGetValue(categoryId, out var value) ? value : null;

    /// <summary>
    /// Gets the biome value if set.
    /// </summary>
    public string? Biome => GetValue("biome");

    /// <summary>
    /// Gets the climate value if set.
    /// </summary>
    public string? Climate => GetValue("climate");

    /// <summary>
    /// Gets the lighting value if set.
    /// </summary>
    public string? Lighting => GetValue("lighting");

    /// <summary>
    /// Gets the era value if set.
    /// </summary>
    public string? Era => GetValue("era");

    /// <summary>
    /// Gets the condition value if set.
    /// </summary>
    public string? Condition => GetValue("condition");

    /// <summary>
    /// Checks if this context has a value for the specified category.
    /// </summary>
    /// <param name="categoryId">The category identifier.</param>
    /// <returns>True if a value is set for this category.</returns>
    public bool HasCategory(string categoryId) =>
        CategoryValues.ContainsKey(categoryId);

    /// <summary>
    /// Creates a new context with an additional or updated category value.
    /// </summary>
    /// <param name="categoryId">The category to set.</param>
    /// <param name="valueId">The value to set.</param>
    /// <returns>A new EnvironmentContext with the updated value.</returns>
    public EnvironmentContext WithValue(string categoryId, string valueId)
    {
        var newValues = new Dictionary<string, string>(CategoryValues)
        {
            [categoryId] = valueId
        };
        return new EnvironmentContext(newValues, DerivedTags);
    }

    /// <summary>
    /// Creates a new context with updated derived tags.
    /// </summary>
    /// <param name="tags">The new derived tags.</param>
    /// <returns>A new EnvironmentContext with the updated tags.</returns>
    public EnvironmentContext WithDerivedTags(IReadOnlyList<string> tags)
    {
        return new EnvironmentContext(CategoryValues, tags);
    }

    /// <summary>
    /// Creates a display string for debugging.
    /// </summary>
    public override string ToString()
    {
        var values = string.Join(", ", CategoryValues.Select(kv => $"{kv.Key}:{kv.Value}"));
        return $"Environment({values})";
    }
}
