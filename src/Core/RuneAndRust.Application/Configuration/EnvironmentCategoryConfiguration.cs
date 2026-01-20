namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Configuration for environment category definitions loaded from JSON.
/// </summary>
public class EnvironmentCategoryConfiguration
{
    /// <summary>
    /// Schema version for configuration validation.
    /// </summary>
    public string Version { get; init; } = "1.0";

    /// <summary>
    /// Available environment categories keyed by category ID.
    /// </summary>
    public IReadOnlyDictionary<string, EnvironmentCategory> Categories { get; init; } =
        new Dictionary<string, EnvironmentCategory>();

    /// <summary>
    /// Rules defining which category values cannot coexist.
    /// </summary>
    public IReadOnlyList<CategoryExclusionRule> ExclusionRules { get; init; } = [];
}

/// <summary>
/// Defines a category of mutually exclusive environment values.
/// </summary>
/// <remarks>
/// Categories group related environment aspects (e.g., Biome, Climate, Lighting).
/// A room can only have one value per category. Some values within the same or
/// different categories may conflict based on exclusion rules.
/// </remarks>
public class EnvironmentCategory
{
    /// <summary>
    /// Category identifier (e.g., "biome", "climate", "lighting").
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Display name for the category.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description of what this category represents.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Whether this category is required for a valid environment context.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Default value for this category if not specified.
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// Available values within this category.
    /// </summary>
    public IReadOnlyList<CategoryValue> Values { get; init; } = [];
}

/// <summary>
/// A specific value within an environment category.
/// </summary>
public class CategoryValue
{
    /// <summary>
    /// Value identifier (e.g., "cave", "freezing", "dim").
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Display name for this value.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description of what this value represents.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Tags automatically applied when this value is active.
    /// </summary>
    /// <remarks>
    /// These tags are added to descriptor filtering, ensuring descriptors
    /// with matching tags are preferred.
    /// </remarks>
    public IReadOnlyList<string> ImpliedTags { get; init; } = [];
}

/// <summary>
/// Defines a rule preventing specific category values from coexisting.
/// </summary>
/// <remarks>
/// Exclusion rules can be within the same category (implicit) or across
/// different categories (explicit). For example, "volcanic" biome excludes
/// "freezing" climate.
/// </remarks>
public class CategoryExclusionRule
{
    /// <summary>
    /// Unique identifier for this rule.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable description of why this conflict exists.
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// First category in the conflict.
    /// </summary>
    public string Category1 { get; init; } = string.Empty;

    /// <summary>
    /// Values in Category1 that participate in this conflict.
    /// </summary>
    public IReadOnlyList<string> Values1 { get; init; } = [];

    /// <summary>
    /// Second category in the conflict (can be same as Category1).
    /// </summary>
    public string Category2 { get; init; } = string.Empty;

    /// <summary>
    /// Values in Category2 that conflict with Values1.
    /// </summary>
    public IReadOnlyList<string> Values2 { get; init; } = [];

    /// <summary>
    /// Whether to log a warning (soft) or throw an error (hard) on violation.
    /// </summary>
    public bool IsHardRule { get; init; } = true;
}
