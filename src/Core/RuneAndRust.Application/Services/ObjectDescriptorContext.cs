namespace RuneAndRust.Application.Services;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Context for generating object descriptions.
/// </summary>
public record ObjectDescriptorContext
{
    /// <summary>
    /// The type of object to describe.
    /// </summary>
    public InteractiveObjectType ObjectType { get; init; }

    /// <summary>
    /// The current state of the object.
    /// </summary>
    public ObjectState State { get; init; }

    /// <summary>
    /// The level of examination detail.
    /// </summary>
    public ExaminationDepth Depth { get; init; } = ExaminationDepth.Look;

    /// <summary>
    /// The environment context for coherent descriptors.
    /// </summary>
    public EnvironmentContext? Environment { get; init; }

    /// <summary>
    /// Optional material/composition (wood, iron, stone).
    /// </summary>
    public string? Material { get; init; }

    /// <summary>
    /// Optional age/condition modifier (ancient, new, worn).
    /// </summary>
    public string? Age { get; init; }

    /// <summary>
    /// Additional tags for filtering.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = [];
}
