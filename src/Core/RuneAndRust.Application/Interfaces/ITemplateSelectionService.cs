using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for selecting room templates based on context.
/// </summary>
public interface ITemplateSelectionService
{
    /// <summary>
    /// Selects a room template based on the provided context.
    /// </summary>
    /// <param name="context">The selection context (biome, depth, tags).</param>
    /// <returns>The selected template, or null if no valid templates exist.</returns>
    RoomTemplate? SelectTemplate(TemplateSelectionContext context);

    /// <summary>
    /// Gets all templates valid for the specified context.
    /// </summary>
    /// <param name="context">The selection context.</param>
    /// <returns>All templates matching the context criteria.</returns>
    IReadOnlyList<RoomTemplate> GetValidTemplates(TemplateSelectionContext context);

    /// <summary>
    /// Gets the fallback template for when no matches are found.
    /// </summary>
    /// <param name="biome">The biome to get a fallback for.</param>
    /// <returns>A generic fallback template.</returns>
    RoomTemplate GetFallbackTemplate(string biome);
}

/// <summary>
/// Context for template selection decisions.
/// </summary>
public record TemplateSelectionContext
{
    /// <summary>
    /// The current biome for filtering.
    /// </summary>
    public required string Biome { get; init; }

    /// <summary>
    /// The current depth (Z-level) for filtering.
    /// </summary>
    public required int Depth { get; init; }

    /// <summary>
    /// Tags that must be present on the template.
    /// </summary>
    public IReadOnlyList<string> RequiredTags { get; init; } = [];

    /// <summary>
    /// Tags that must not be present on the template.
    /// </summary>
    public IReadOnlyList<string> ExcludedTags { get; init; } = [];

    /// <summary>
    /// Optional seed for deterministic selection.
    /// </summary>
    public int? Seed { get; init; }
}
