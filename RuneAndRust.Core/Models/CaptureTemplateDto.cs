using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Data Transfer Object for capture templates loaded from JSON.
/// v0.3.25b: Matches JSON schema structure.
/// </summary>
public record CaptureTemplateDto
{
    /// <summary>
    /// Unique template identifier (e.g., "servitor-fungal-infection").
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Capture type classification.
    /// </summary>
    public required CaptureType Type { get; init; }

    /// <summary>
    /// The lore text content of this fragment.
    /// </summary>
    public required string FragmentContent { get; init; }

    /// <summary>
    /// Description of where/how the fragment was discovered.
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// Keywords for Codex auto-assignment.
    /// </summary>
    public required string[] MatchKeywords { get; init; }

    /// <summary>
    /// Quality value affecting Legend rewards. Default: 15.
    /// </summary>
    public int Quality { get; init; } = 15;

    /// <summary>
    /// Optional metadata tags for filtering.
    /// </summary>
    public string[] Tags { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Category this template belongs to.
    /// </summary>
    public required string Category { get; init; }
}
