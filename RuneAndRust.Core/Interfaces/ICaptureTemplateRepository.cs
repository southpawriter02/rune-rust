using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Repository interface for accessing Data Capture templates.
/// v0.3.25b: Abstracts template storage from service layer.
/// </summary>
/// <remarks>
/// See: SPEC-CODEX-001 for Data Capture System design.
/// Templates are loaded from external JSON files to enable content
/// authoring without code changes.
/// </remarks>
public interface ICaptureTemplateRepository
{
    /// <summary>
    /// Gets all templates for the specified category.
    /// </summary>
    /// <param name="category">Category name (e.g., "rusted-servitor").</param>
    /// <returns>Collection of templates, or empty if category not found.</returns>
    Task<IReadOnlyList<CaptureTemplateDto>> GetByCategoryAsync(string category);

    /// <summary>
    /// Gets a random template from the specified category.
    /// </summary>
    /// <param name="category">Category name.</param>
    /// <returns>Random template, or null if category empty/not found.</returns>
    Task<CaptureTemplateDto?> GetRandomAsync(string category);

    /// <summary>
    /// Gets all available category names.
    /// </summary>
    /// <returns>Collection of category identifiers.</returns>
    Task<IReadOnlyList<string>> GetCategoriesAsync();

    /// <summary>
    /// Gets a specific template by its unique ID.
    /// </summary>
    /// <param name="templateId">Template ID (e.g., "servitor-fungal-infection").</param>
    /// <returns>Template if found, null otherwise.</returns>
    Task<CaptureTemplateDto?> GetByIdAsync(string templateId);

    /// <summary>
    /// Forces a reload of all templates from disk.
    /// Used for hot-reload during development.
    /// </summary>
    Task ReloadAsync();

    /// <summary>
    /// Gets the total count of loaded templates across all categories.
    /// </summary>
    int TotalTemplateCount { get; }
}
