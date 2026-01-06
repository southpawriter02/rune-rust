namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Represents detailed examination information about a target.
/// </summary>
public record ExamineResultDto(
    string Name,
    string Type,
    string Description,
    IReadOnlyDictionary<string, string> Properties
);
