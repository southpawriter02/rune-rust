using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Core.Models.Combat;

/// <summary>
/// Defines a trauma template with its effects and behaviors.
/// Used by the TraumaRegistry to create Trauma instances.
/// </summary>
/// <param name="DefinitionId">Unique identifier for the trauma definition (e.g., "TRM_NYCTO").</param>
/// <param name="Name">Display name of the trauma.</param>
/// <param name="Description">Narrative description of the trauma's effects.</param>
/// <param name="Type">The category of trauma (Phobia, Compulsion, Delusion, Somatic).</param>
/// <param name="AttributePenalties">Permanent attribute penalties applied when acquired.</param>
/// <param name="StressPerTurnInCondition">Stress gained per turn when trigger condition is met.</param>
/// <param name="TriggerCondition">Description of when this trauma's effects activate.</param>
public record TraumaDefinition(
    string DefinitionId,
    string Name,
    string Description,
    TraumaType Type,
    Dictionary<CharacterAttribute, int> AttributePenalties,
    int StressPerTurnInCondition,
    string TriggerCondition
)
{
    /// <summary>
    /// Creates a Trauma entity instance from this definition.
    /// </summary>
    /// <param name="source">The source event that caused the trauma.</param>
    /// <returns>A new Trauma entity.</returns>
    public Trauma CreateInstance(string source) => new()
    {
        DefinitionId = DefinitionId,
        Name = Name,
        Description = Description,
        Type = Type,
        Source = source,
        AcquiredAt = DateTime.UtcNow,
        IsActive = true
    };
}
