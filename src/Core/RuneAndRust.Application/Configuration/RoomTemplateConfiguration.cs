using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Configuration for room templates loaded from JSON.
/// </summary>
public class RoomTemplateConfiguration
{
    /// <summary>
    /// Schema version for configuration validation.
    /// </summary>
    public string Version { get; init; } = "1.1";

    /// <summary>
    /// Available room templates keyed by template ID.
    /// </summary>
    public Dictionary<string, RoomTemplate> Templates { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a template by ID or null if not found.
    /// </summary>
    public RoomTemplate? GetTemplate(string templateId) =>
        Templates.TryGetValue(templateId, out var template) ? template : null;

    /// <summary>
    /// Adds a template to the configuration.
    /// </summary>
    public void AddTemplate(RoomTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);
        Templates[template.TemplateId] = template;
    }

    /// <summary>
    /// Gets all template IDs.
    /// </summary>
    public IEnumerable<string> GetTemplateIds() => Templates.Keys;
}
