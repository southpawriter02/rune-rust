using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Result of examining an object.
/// </summary>
public record ExaminationResultDto(
    Guid ObjectId,
    string ObjectName,
    int WitsRoll,
    int WitsTotal,
    ExaminationLayer HighestLayerUnlocked,
    string CompositeDescription,
    bool RevealedHint,
    string? RevealedSolutionId,
    IReadOnlyList<LayerTextDto> LayerTexts
);

/// <summary>
/// A single layer's examination text.
/// </summary>
public record LayerTextDto(
    ExaminationLayer Layer,
    string Text,
    int RequiredDC
);

/// <summary>
/// Result of passive perception check on room entry.
/// </summary>
public record PassivePerceptionResultDto(
    Guid RoomId,
    int PassivePerception,
    IReadOnlyList<RevealedElementDto> RevealedElements,
    string? PerceptionNarrative
);

/// <summary>
/// Result of an active search.
/// </summary>
public record SearchResultDto(
    Guid RoomId,
    int WitsRoll,
    int WitsTotal,
    IReadOnlyList<RevealedElementDto> RevealedElements,
    string SearchNarrative,
    bool FoundAnything
);

/// <summary>
/// A revealed hidden element.
/// </summary>
public record RevealedElementDto(
    Guid ElementId,
    HiddenElementType ElementType,
    string Name,
    string DiscoveryText,
    int DetectionDC
)
{
    /// <summary>
    /// Additional properties for trap elements.
    /// </summary>
    public int? TrapDamage { get; init; }
    public int? DisarmDC { get; init; }

    /// <summary>
    /// Additional properties for secret door elements.
    /// </summary>
    public Guid? LeadsToRoomId { get; init; }

    /// <summary>
    /// Additional properties for cache elements.
    /// </summary>
    public string? CacheContents { get; init; }
}

/// <summary>
/// Result of getting an ambient flora/fauna observation.
/// </summary>
public record AmbientObservationDto(
    string SpeciesName,
    string? ScientificName,
    FloraFaunaCategory Category,
    ExaminationLayer LayerUnlocked,
    string Observation,
    string? AlchemicalUse,
    bool IsHarvestable,
    int? HarvestDC
);

/// <summary>
/// Extension methods for creating DTOs from domain entities.
/// </summary>
public static class ExaminationDtoExtensions
{
    public static RevealedElementDto ToDto(this Domain.Entities.HiddenElement element)
    {
        return new RevealedElementDto(
            ElementId: element.Id,
            ElementType: element.ElementType,
            Name: element.Name,
            DiscoveryText: element.DiscoveryText,
            DetectionDC: element.DetectionDC
        )
        {
            TrapDamage = element.TrapDamage,
            DisarmDC = element.DisarmDC,
            LeadsToRoomId = element.LeadsToRoomId,
            CacheContents = element.CacheContents
        };
    }

    public static LayerTextDto ToDto(this Domain.Entities.ExaminationDescriptor descriptor)
    {
        return new LayerTextDto(
            Layer: descriptor.Layer,
            Text: descriptor.DescriptorText,
            RequiredDC: descriptor.RequiredDC
        );
    }
}
