namespace RuneAndRust.Application.DTOs;

/// <summary>
/// DTO for player resource pool information display.
/// </summary>
public record ResourcePoolDto(
    string ResourceTypeId,
    string DisplayName,
    string Abbreviation,
    string Color,
    int Current,
    int Maximum,
    float Percentage);
