namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Configuration for room templates loaded from JSON.
/// </summary>
public class RoomTemplateConfiguration
{
    /// <summary>
    /// Schema version for configuration validation.
    /// </summary>
    public string Version { get; init; } = "1.0";

    /// <summary>
    /// Available room templates keyed by template ID.
    /// </summary>
    public IReadOnlyDictionary<string, RoomTemplate> Templates { get; init; } =
        new Dictionary<string, RoomTemplate>();

    /// <summary>
    /// Gets a template by ID or null if not found.
    /// </summary>
    public RoomTemplate? GetTemplate(string templateId) =>
        Templates.TryGetValue(templateId, out var template) ? template : null;

    /// <summary>
    /// Gets all templates valid for a specific biome and depth.
    /// </summary>
    public IEnumerable<RoomTemplate> GetValidTemplates(string biomeId, int depth) =>
        Templates.Values
            .Where(t => t.Biomes.Contains(biomeId))
            .Where(t => t.IsValidForDepth(depth));
}

/// <summary>
/// Defines a room template for procedural generation.
/// </summary>
/// <remarks>
/// Room templates specify the properties of generated rooms including
/// possible names, descriptions, exit probabilities, and content spawn chances.
/// Templates are associated with biomes and selected via weighted random.
/// </remarks>
public class RoomTemplate
{
    /// <summary>
    /// Unique identifier for this template.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Biomes this template can appear in.
    /// </summary>
    public IReadOnlyList<string> Biomes { get; init; } = [];

    /// <summary>
    /// Possible room names (one selected randomly).
    /// </summary>
    public IReadOnlyList<string> Names { get; init; } = [];

    /// <summary>
    /// Static description templates.
    /// </summary>
    public IReadOnlyList<string> DescriptionTemplates { get; init; } = [];

    /// <summary>
    /// Exit probability per direction (0.0 to 1.0).
    /// Keys are lowercase direction names: north, south, east, west, up, down.
    /// </summary>
    public IReadOnlyDictionary<string, float> ExitProbabilities { get; init; } =
        new Dictionary<string, float>();

    /// <summary>
    /// Probability (0.0 to 1.0) of spawning a monster.
    /// </summary>
    public float MonsterChance { get; init; }

    /// <summary>
    /// Probability (0.0 to 1.0) of spawning an item.
    /// </summary>
    public float ItemChance { get; init; }

    /// <summary>
    /// Selection weight (higher = more likely).
    /// </summary>
    public int Weight { get; init; } = 10;

    /// <summary>
    /// Minimum depth (Z level) for this template.
    /// </summary>
    public int MinDepth { get; init; } = 0;

    /// <summary>
    /// Maximum depth (Z level) for this template (-1 = no limit).
    /// </summary>
    public int MaxDepth { get; init; } = -1;

    /// <summary>
    /// Tags applied to rooms generated from this template.
    /// </summary>
    public IReadOnlyList<string> ImpliedTags { get; init; } = [];

    /// <summary>
    /// Gets the probability for a specific exit direction.
    /// </summary>
    /// <param name="direction">Direction name (e.g., "north", "up").</param>
    /// <returns>Probability from 0.0 to 1.0, defaults to 0.0 if not specified.</returns>
    public float GetExitProbability(string direction) =>
        ExitProbabilities.TryGetValue(direction.ToLowerInvariant(), out var prob) ? prob : 0f;

    /// <summary>
    /// Checks if this template is valid for the given depth.
    /// </summary>
    /// <param name="depth">The Z-level depth.</param>
    /// <returns>True if the template can be used at this depth.</returns>
    public bool IsValidForDepth(int depth)
    {
        if (depth < MinDepth) return false;
        if (MaxDepth >= 0 && depth > MaxDepth) return false;
        return true;
    }
}
