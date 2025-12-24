using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Factory for creating Enemy entities from templates with scaling and variance.
/// Implements the Prototype pattern for enemy instantiation.
/// </summary>
/// <remarks>See: SPEC-ENEMYFAC-001 for Enemy Factory System design.</remarks>
public interface IEnemyFactory
{
    /// <summary>
    /// Creates an enemy from a template with optional party-level scaling.
    /// Applies variance (+/- 5%) to HP and Stamina for variety.
    /// Hydrates abilities from repository (v0.2.4a).
    /// </summary>
    /// <param name="template">The template blueprint to create from.</param>
    /// <param name="partyLevel">Party level for stat scaling (default 1).</param>
    /// <returns>A new Enemy entity with scaled stats and hydrated abilities.</returns>
    Task<Enemy> CreateFromTemplateAsync(EnemyTemplate template, int partyLevel = 1);

    /// <summary>
    /// Creates an enemy by template ID lookup.
    /// Falls back to a default template if the ID is not found.
    /// Hydrates abilities from repository (v0.2.4a).
    /// </summary>
    /// <param name="templateId">The template ID to look up (e.g., "und_draugr_01").</param>
    /// <param name="partyLevel">Party level for stat scaling (default 1).</param>
    /// <returns>A new Enemy entity with scaled stats and hydrated abilities.</returns>
    Task<Enemy> CreateByIdAsync(string templateId, int partyLevel = 1);

    /// <summary>
    /// Gets all registered template IDs in the factory.
    /// </summary>
    /// <returns>A read-only list of template IDs.</returns>
    IReadOnlyList<string> GetTemplateIds();

    /// <summary>
    /// Gets a template by its ID.
    /// </summary>
    /// <param name="templateId">The template ID to retrieve.</param>
    /// <returns>The template, or null if not found.</returns>
    EnemyTemplate? GetTemplate(string templateId);
}
