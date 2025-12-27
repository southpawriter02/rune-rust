using System.Text.Json.Serialization;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Represents a category file loaded from JSON.
/// Matches the JSON schema structure for deserialization.
/// v0.3.25b: Data-driven template system.
/// </summary>
public record CaptureTemplateCollection
{
    /// <summary>
    /// Reference to the JSON schema file.
    /// </summary>
    [JsonPropertyName("$schema")]
    public string? Schema { get; init; }

    /// <summary>
    /// Category identifier matching the filename (kebab-case).
    /// </summary>
    [JsonPropertyName("category")]
    public required string Category { get; init; }

    /// <summary>
    /// Schema version for migration tracking (semver format).
    /// </summary>
    [JsonPropertyName("version")]
    public required string Version { get; init; }

    /// <summary>
    /// Human-readable description of this category.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// Default keywords for all templates in this category.
    /// Can be overridden per-template.
    /// </summary>
    [JsonPropertyName("matchKeywords")]
    public string[] MatchKeywords { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Array of capture templates in this category.
    /// </summary>
    [JsonPropertyName("templates")]
    public required CaptureTemplateJson[] Templates { get; init; }
}

/// <summary>
/// Individual template from JSON, before conversion to DTO.
/// v0.3.25b: Intermediate model for JSON deserialization.
/// </summary>
public record CaptureTemplateJson
{
    /// <summary>
    /// Unique template identifier (kebab-case).
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// CaptureType enum value as string.
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// The lore text content of this fragment.
    /// </summary>
    [JsonPropertyName("fragmentContent")]
    public required string FragmentContent { get; init; }

    /// <summary>
    /// Description of where/how the fragment was discovered.
    /// </summary>
    [JsonPropertyName("source")]
    public required string Source { get; init; }

    /// <summary>
    /// Keywords for Codex auto-assignment (overrides category default).
    /// </summary>
    [JsonPropertyName("matchKeywords")]
    public string[]? MatchKeywords { get; init; }

    /// <summary>
    /// Quality value affecting Legend rewards. Default: 15.
    /// </summary>
    [JsonPropertyName("quality")]
    public int Quality { get; init; } = 15;

    /// <summary>
    /// Optional metadata tags for filtering.
    /// </summary>
    [JsonPropertyName("tags")]
    public string[]? Tags { get; init; }
}
