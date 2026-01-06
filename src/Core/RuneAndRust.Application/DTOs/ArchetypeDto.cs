using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// DTO for archetype information display.
/// </summary>
public record ArchetypeDto(
    string Id,
    string Name,
    string Description,
    string PlaystyleSummary,
    StatTendency StatTendency,
    int SortOrder,
    IReadOnlyList<string> ClassNames);
