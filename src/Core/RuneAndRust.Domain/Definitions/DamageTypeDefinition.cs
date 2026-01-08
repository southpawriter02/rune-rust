namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Defines a type of damage that can be dealt in combat.
/// </summary>
/// <remarks>
/// <para>Damage types are loaded from configuration and determine how
/// attacks interact with monster resistances and vulnerabilities.</para>
/// <para>The default damage type is "physical" for backward compatibility.</para>
/// </remarks>
public class DamageTypeDefinition
{
    /// <summary>
    /// Gets the unique identifier for this damage type.
    /// </summary>
    /// <example>"physical", "fire", "ice", "lightning"</example>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name of this damage type.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the description shown to players.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the color used for displaying this damage type.
    /// </summary>
    /// <remarks>
    /// Should be a valid Spectre.Console color name.
    /// </remarks>
    public string Color { get; init; } = "white";

    /// <summary>
    /// Gets the optional icon/emoji for this damage type.
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    /// Gets the display sort order.
    /// </summary>
    public int SortOrder { get; init; } = 0;

    /// <summary>
    /// Private parameterless constructor for JSON deserialization.
    /// </summary>
    private DamageTypeDefinition()
    {
    }

    /// <summary>
    /// Creates a validated DamageTypeDefinition.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="name">The display name.</param>
    /// <param name="description">The description.</param>
    /// <param name="color">The Spectre.Console color name.</param>
    /// <param name="icon">Optional icon/emoji.</param>
    /// <param name="sortOrder">Display sort order.</param>
    /// <returns>A new DamageTypeDefinition.</returns>
    /// <exception cref="ArgumentException">Thrown when id or name is null/whitespace.</exception>
    public static DamageTypeDefinition Create(
        string id,
        string name,
        string description,
        string color = "white",
        string? icon = null,
        int sortOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new DamageTypeDefinition
        {
            Id = id.ToLowerInvariant(),
            Name = name,
            Description = description ?? string.Empty,
            Color = string.IsNullOrWhiteSpace(color) ? "white" : color,
            Icon = icon,
            SortOrder = sortOrder
        };
    }

    /// <summary>
    /// Gets the default Physical damage type.
    /// </summary>
    public static DamageTypeDefinition Physical => Create(
        "physical",
        "Physical",
        "Standard physical damage from weapons and natural attacks.",
        "white",
        null,
        0);
}
