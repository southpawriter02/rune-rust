namespace RuneAndRust.Application.DTOs;

/// <summary>
/// DTO for resource type information display.
/// </summary>
public record ResourceTypeDto(
    string Id,
    string DisplayName,
    string Abbreviation,
    string Color,
    string Description,
    int DefaultMax,
    int RegenPerTurn,
    int DecayPerTurn,
    bool IsUniversal,
    bool StartsAtZero);
