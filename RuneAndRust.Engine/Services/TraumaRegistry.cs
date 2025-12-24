using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models.Combat;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Static registry of all available trauma definitions.
/// Provides trauma lookup and random selection for Breaking Point events.
/// </summary>
/// <remarks>See: SPEC-TRAUMA-001, Section "TraumaRegistry Definitions".</remarks>
public static class TraumaRegistry
{
    /// <summary>
    /// All registered trauma definitions.
    /// </summary>
    public static readonly IReadOnlyList<TraumaDefinition> All = new List<TraumaDefinition>
    {
        // Phobias - Fear-based, triggered by environmental conditions
        new TraumaDefinition(
            DefinitionId: "TRM_NYCTO",
            Name: "Nyctophobia",
            Description: "An irrational terror of darkness. The shadows seem to writhe with malevolent intent.",
            Type: TraumaType.Phobia,
            AttributePenalties: new Dictionary<CharacterAttribute, int>(),
            StressPerTurnInCondition: 2,
            TriggerCondition: "In dark or dimly lit areas"
        ),

        new TraumaDefinition(
            DefinitionId: "TRM_CLAUSTRO",
            Name: "Claustrophobia",
            Description: "Confined spaces trigger panic. The walls always feel like they're closing in.",
            Type: TraumaType.Phobia,
            AttributePenalties: new Dictionary<CharacterAttribute, int>(),
            StressPerTurnInCondition: 2,
            TriggerCondition: "In small rooms or tunnels"
        ),

        new TraumaDefinition(
            DefinitionId: "TRM_HEMATO",
            Name: "Hemophobia",
            Description: "The sight of blood induces overwhelming dread. Combat becomes a psychological ordeal.",
            Type: TraumaType.Phobia,
            AttributePenalties: new Dictionary<CharacterAttribute, int>(),
            StressPerTurnInCondition: 1,
            TriggerCondition: "When any combatant takes damage"
        ),

        // Somatic - Physical manifestations of psychological damage
        new TraumaDefinition(
            DefinitionId: "TRM_SHAKES",
            Name: "The Shakes",
            Description: "Persistent tremors that never quite stop. Fine motor control is permanently compromised.",
            Type: TraumaType.Somatic,
            AttributePenalties: new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Finesse, -1 }
            },
            StressPerTurnInCondition: 0,
            TriggerCondition: "Always active"
        ),

        new TraumaDefinition(
            DefinitionId: "TRM_MIGRAINES",
            Name: "Chronic Migraines",
            Description: "Splitting headaches that cloud thought. Mental acuity suffers.",
            Type: TraumaType.Somatic,
            AttributePenalties: new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Wits, -1 }
            },
            StressPerTurnInCondition: 0,
            TriggerCondition: "Always active"
        ),

        new TraumaDefinition(
            DefinitionId: "TRM_FATIGUE",
            Name: "Bone-Deep Exhaustion",
            Description: "A weariness that sleep cannot cure. The body moves as if through water.",
            Type: TraumaType.Somatic,
            AttributePenalties: new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Sturdiness, -1 }
            },
            StressPerTurnInCondition: 0,
            TriggerCondition: "Always active"
        ),

        // Delusions - Distorted perception
        new TraumaDefinition(
            DefinitionId: "TRM_PARANOIA",
            Name: "Paranoia",
            Description: "Trust no one. Every ally could be an enemy in disguise. Help feels like a trap.",
            Type: TraumaType.Delusion,
            AttributePenalties: new Dictionary<CharacterAttribute, int>(),
            StressPerTurnInCondition: 1,
            TriggerCondition: "When receiving beneficial effects from allies"
        ),

        new TraumaDefinition(
            DefinitionId: "TRM_VOICES",
            Name: "The Whispers",
            Description: "Voices that aren't there. They distract, confuse, and sometimes advise poorly.",
            Type: TraumaType.Delusion,
            AttributePenalties: new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Will, -1 }
            },
            StressPerTurnInCondition: 0,
            TriggerCondition: "Always active"
        ),

        // Compulsions - Forced behaviors
        new TraumaDefinition(
            DefinitionId: "TRM_SELFHARM",
            Name: "Self-Destructive Urges",
            Description: "An unhealthy relationship with pain. Sometimes hurting feels like the only control.",
            Type: TraumaType.Compulsion,
            AttributePenalties: new Dictionary<CharacterAttribute, int>(),
            StressPerTurnInCondition: 0,
            TriggerCondition: "When stress reaches Fractured tier"
        ),

        new TraumaDefinition(
            DefinitionId: "TRM_HOARDER",
            Name: "Compulsive Hoarding",
            Description: "Nothing can be discarded. Everything might be needed. The weight is a comfort.",
            Type: TraumaType.Compulsion,
            AttributePenalties: new Dictionary<CharacterAttribute, int>(),
            StressPerTurnInCondition: 1,
            TriggerCondition: "When discarding items"
        )
    };

    private static readonly Random _random = new();

    /// <summary>
    /// Gets a random trauma definition from the registry.
    /// Used when a character fails a Breaking Point resolve check.
    /// </summary>
    /// <returns>A randomly selected trauma definition.</returns>
    public static TraumaDefinition GetRandom()
    {
        return All[_random.Next(All.Count)];
    }

    /// <summary>
    /// Gets a trauma definition by its ID.
    /// </summary>
    /// <param name="definitionId">The definition ID to look up.</param>
    /// <returns>The trauma definition, or null if not found.</returns>
    public static TraumaDefinition? GetById(string definitionId)
    {
        return All.FirstOrDefault(t => t.DefinitionId == definitionId);
    }

    /// <summary>
    /// Gets all trauma definitions of a specific type.
    /// </summary>
    /// <param name="type">The trauma type to filter by.</param>
    /// <returns>All matching trauma definitions.</returns>
    public static IEnumerable<TraumaDefinition> GetByType(TraumaType type)
    {
        return All.Where(t => t.Type == type);
    }
}
