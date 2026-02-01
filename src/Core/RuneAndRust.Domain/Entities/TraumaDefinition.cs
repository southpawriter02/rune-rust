namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a specific trauma definition with all mechanical properties.
/// </summary>
/// <remarks>
/// <para>
/// TraumaDefinition is loaded from JSON configuration (traumas.json) and provides
/// the blueprint for creating character traumas. Each trauma is uniquely identified
/// by TraumaId and contains all mechanical rules and effects.
/// </para>
/// <para>
/// Traumas are permanent once acquired and can optionally force character retirement
/// if severe enough. Some traumas stack (acquiring them multiple times) while others
/// are singularities (acquired once, StackCount tracks intensity).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var trauma = TraumaDefinition.Create(
///     traumaId: "survivors-guilt",
///     name: "Survivor's Guilt",
///     type: TraumaType.Emotional,
///     description: "You carry the weight of those who didn't make it.",
///     flavorText: "Why them? Why not me?",
///     isRetirementTrauma: true,
///     retirementCondition: "On acquisition",
///     isStackable: false,
///     acquisitionSources: new[] { "AllyDeath", "PartyWipe" },
///     triggers: new[] { /* triggers */ },
///     effects: new[] { /* effects */ }
/// );
/// </code>
/// </example>
public class TraumaDefinition
{
    /// <summary>
    /// Gets the unique identifier for this TraumaDefinition.
    /// </summary>
    public Guid Id { get; private init; }

    /// <summary>
    /// Gets the string identifier for this trauma (e.g., "survivors-guilt").
    /// </summary>
    /// <remarks>
    /// Used as the key for JSON configuration and database lookups.
    /// Must be unique across all trauma definitions.
    /// </remarks>
    public string TraumaId { get; private init; }

    /// <summary>
    /// Gets the display name of this trauma.
    /// </summary>
    /// <remarks>
    /// Human-readable name shown to players. May include narrative elements
    /// (e.g., "[MACHINE AFFINITY]" for corrupted traumas).
    /// </remarks>
    public string Name { get; private init; }

    /// <summary>
    /// Gets the type category of this trauma.
    /// </summary>
    /// <remarks>
    /// Determines which trauma check categories can result in this trauma.
    /// Affects mechanics and narrative presentation.
    /// </remarks>
    public TraumaType Type { get; private init; }

    /// <summary>
    /// Gets the mechanical description of this trauma's effects.
    /// </summary>
    /// <remarks>
    /// Clear, factual description of what this trauma does mechanically.
    /// Used for UI tooltips and help text.
    /// </remarks>
    public string Description { get; private init; }

    /// <summary>
    /// Gets the narrative flavor text for this trauma.
    /// </summary>
    /// <remarks>
    /// In-character description showing the subjective experience of this trauma.
    /// Often poetic or disturbing to emphasize psychological impact.
    /// </remarks>
    public string FlavorText { get; private init; }

    /// <summary>
    /// Gets the set of triggers that activate this trauma's effects.
    /// </summary>
    /// <remarks>
    /// Empty list if trauma has no special activation triggers.
    /// Used by event systems to determine when to apply effects.
    /// </remarks>
    public IReadOnlyList<TraumaTrigger> Triggers { get; private init; }

    /// <summary>
    /// Gets the mechanical effects this trauma applies.
    /// </summary>
    /// <remarks>
    /// List of effects that activate immediately upon acquisition.
    /// Can include skill penalties, disadvantages, stress increases, etc.
    /// </remarks>
    public IReadOnlyList<TraumaEffect> Effects { get; private init; }

    /// <summary>
    /// Gets whether this trauma forces character retirement if acquired.
    /// </summary>
    /// <remarks>
    /// Terminal traumas (corruption-related, severe psychological breaks)
    /// trigger mandatory or conditional retirement checks.
    /// </remarks>
    public bool IsRetirementTrauma { get; private init; }

    /// <summary>
    /// Gets the condition under which this trauma triggers retirement.
    /// </summary>
    /// <remarks>
    /// Examples:
    /// - "On acquisition" (immediate retirement check)
    /// - "5+ instances" (after 5 stack counts)
    /// - "With 3+ other moderate traumas" (accumulation condition)
    /// Null if IsRetirementTrauma is false.
    /// </remarks>
    public string? RetirementCondition { get; private init; }

    /// <summary>
    /// Gets the list of acquisition sources/triggers for this trauma.
    /// </summary>
    /// <remarks>
    /// Examples: "AllyDeath", "CorruptionThreshold75", "ForlornContact"
    /// Used by trauma check system to determine which traumas can result.
    /// </remarks>
    public IReadOnlyList<string> AcquisitionSources { get; private init; }

    /// <summary>
    /// Gets whether this trauma can be acquired multiple times.
    /// </summary>
    /// <remarks>
    /// Stackable: Can have StackCount > 1 (e.g., Reality Doubt can occur 5+ times)
    /// Non-stackable: Acquired once, but StackCount may track event intensity
    /// </remarks>
    public bool IsStackable { get; private init; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private TraumaDefinition()
    {
        TraumaId = null!;
        Name = null!;
        Description = null!;
        FlavorText = null!;
        Triggers = null!;
        Effects = null!;
        AcquisitionSources = null!;
    }

    /// <summary>
    /// Creates a new TraumaDefinition with the specified properties.
    /// </summary>
    /// <param name="traumaId">String identifier (e.g., "survivors-guilt")</param>
    /// <param name="name">Display name</param>
    /// <param name="type">Trauma category</param>
    /// <param name="description">Mechanical description</param>
    /// <param name="flavorText">Narrative flavor text</param>
    /// <param name="isRetirementTrauma">Whether this trauma forces retirement</param>
    /// <param name="retirementCondition">Retirement condition description</param>
    /// <param name="isStackable">Whether this trauma can stack</param>
    /// <param name="acquisitionSources">List of acquisition trigger types</param>
    /// <param name="triggers">List of TraumaTriggers</param>
    /// <param name="effects">List of TraumaEffects</param>
    /// <returns>A new TraumaDefinition instance</returns>
    /// <exception cref="ArgumentException">If traumaId or name is empty</exception>
    /// <exception cref="ArgumentException">If retirement trauma without condition</exception>
    public static TraumaDefinition Create(
        string traumaId,
        string name,
        TraumaType type,
        string description,
        string flavorText,
        bool isRetirementTrauma,
        string? retirementCondition,
        bool isStackable,
        IReadOnlyList<string> acquisitionSources,
        IReadOnlyList<TraumaTrigger> triggers,
        IReadOnlyList<TraumaEffect> effects)
    {
        if (string.IsNullOrWhiteSpace(traumaId))
            throw new ArgumentException("TraumaId cannot be empty", nameof(traumaId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        if (isRetirementTrauma && string.IsNullOrWhiteSpace(retirementCondition))
            throw new ArgumentException(
                "RetirementCondition must be specified for retirement traumas",
                nameof(retirementCondition));

        return new TraumaDefinition
        {
            Id = Guid.NewGuid(),
            TraumaId = traumaId.ToLowerInvariant(),
            Name = name,
            Type = type,
            Description = description,
            FlavorText = flavorText,
            IsRetirementTrauma = isRetirementTrauma,
            RetirementCondition = retirementCondition,
            IsStackable = isStackable,
            AcquisitionSources = acquisitionSources ?? new List<string>(),
            Triggers = triggers ?? new List<TraumaTrigger>(),
            Effects = effects ?? new List<TraumaEffect>()
        };
    }

    /// <summary>
    /// Gets the display string for this trauma definition.
    /// </summary>
    public override string ToString() =>
        $"{Name} ({Type}) [ID: {TraumaId}]" +
        (IsRetirementTrauma ? " [RETIREMENT]" : "") +
        (IsStackable ? " [STACKABLE]" : "");
}
