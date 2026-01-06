using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// DTO for class information display.
/// </summary>
public record ClassDto(
    string Id,
    string Name,
    string Description,
    string ArchetypeId,
    StatModifiers StatModifiers,
    StatModifiers GrowthRates,
    string PrimaryResourceId,
    IReadOnlyList<string> StartingAbilityIds,
    bool HasRequirements,
    int SortOrder);
