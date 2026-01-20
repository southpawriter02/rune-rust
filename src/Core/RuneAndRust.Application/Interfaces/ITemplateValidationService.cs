using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for validating room template configurations.
/// </summary>
public interface ITemplateValidationService
{
    /// <summary>
    /// Validates a single template.
    /// </summary>
    /// <param name="template">The template to validate.</param>
    /// <returns>Validation result with any errors found.</returns>
    TemplateValidationResult ValidateTemplate(RoomTemplate template);

    /// <summary>
    /// Validates all loaded templates.
    /// </summary>
    /// <returns>Validation results for all templates.</returns>
    IReadOnlyList<TemplateValidationResult> ValidateAllTemplates();

    /// <summary>
    /// Validates a single slot definition.
    /// </summary>
    /// <param name="slot">The slot to validate.</param>
    /// <param name="templateId">The parent template ID for error reporting.</param>
    /// <returns>List of validation errors.</returns>
    IReadOnlyList<string> ValidateSlot(TemplateSlot slot, string templateId);
}

/// <summary>
/// Result of template validation.
/// </summary>
public record TemplateValidationResult
{
    /// <summary>
    /// The template ID that was validated.
    /// </summary>
    public required string TemplateId { get; init; }

    /// <summary>
    /// Whether the template is valid.
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// List of validation errors.
    /// </summary>
    public IReadOnlyList<string> Errors { get; init; } = [];

    /// <summary>
    /// List of validation warnings (non-fatal issues).
    /// </summary>
    public IReadOnlyList<string> Warnings { get; init; } = [];
}
