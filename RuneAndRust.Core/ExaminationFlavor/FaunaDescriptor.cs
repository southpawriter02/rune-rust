// ==============================================================================
// v0.38.9: Perception & Examination Descriptors
// FaunaDescriptor.cs
// ==============================================================================
// Purpose: Ambient creatures, critters, non-combat animals
// Usage: Living world atmosphere, environmental storytelling
// ==============================================================================

namespace RuneAndRust.Core.ExaminationFlavor;

/// <summary>
/// Represents a fauna descriptor (ambient creatures, non-hostile animals).
/// Adds living world atmosphere through creature observations.
/// </summary>
public class FaunaDescriptor
{
    // ==================== IDENTITY ====================

    /// <summary>
    /// Unique identifier for this descriptor.
    /// </summary>
    public int DescriptorId { get; set; }

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// Display name of the creature.
    /// Example: "Cave Rat", "Rust Beetle", "Blight-Moth"
    /// </summary>
    public string CreatureName { get; set; } = string.Empty;

    /// <summary>
    /// Type of creature.
    /// Values: Rodent, Insect, Bird, Reptile, Amphibian, AmbientCreature,
    ///         BlightCreature, Construct
    /// </summary>
    public string CreatureType { get; set; } = string.Empty;

    /// <summary>
    /// Type of observation.
    /// Values: Sighting, Sound, Traces, ExpertObservation
    /// </summary>
    public string ObservationType { get; set; } = string.Empty;

    // ==================== BIOME ====================

    /// <summary>
    /// Where this fauna appears.
    /// NULL for multiple biomes.
    /// </summary>
    public string? BiomeName { get; set; }

    // ==================== BEHAVIOR ====================

    /// <summary>
    /// Non-hostile by default (ambient creatures).
    /// </summary>
    public bool IsHostile { get; set; } = false;

    /// <summary>
    /// Ecological role.
    /// Values: Predator, Scavenger, Herbivore, Decomposer, BlightAdapted, NULL
    /// </summary>
    public string? EcologicalRole { get; set; }

    /// <summary>
    /// Additional context for expert observation.
    /// NULL for basic sightings.
    /// </summary>
    public string? ExpertInsight { get; set; }

    // ==================== TEMPLATE TEXT ====================

    /// <summary>
    /// Creature observation narrative.
    ///
    /// Available variables:
    /// - {Species}, {Behavior}, {Location}
    /// - {Biome}, {Player}
    ///
    /// Example: "A rat scurries across the floor, disappearing into a crack..."
    /// </summary>
    public string DescriptorText { get; set; } = string.Empty;

    // ==================== METADATA ====================

    /// <summary>
    /// Probability weight for random selection.
    /// </summary>
    public float Weight { get; set; } = 1.0f;

    /// <summary>
    /// Whether this descriptor is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON array of tags for categorization.
    /// </summary>
    public string? Tags { get; set; }

    // ==================== ENUMERATIONS ====================

    /// <summary>
    /// Valid creature types.
    /// </summary>
    public static class CreatureTypes
    {
        public const string Rodent = "Rodent";
        public const string Insect = "Insect";
        public const string Bird = "Bird";
        public const string Reptile = "Reptile";
        public const string Amphibian = "Amphibian";
        public const string AmbientCreature = "AmbientCreature";
        public const string BlightCreature = "BlightCreature";
        public const string Construct = "Construct";
    }

    /// <summary>
    /// Valid observation types.
    /// </summary>
    public static class ObservationTypes
    {
        public const string Sighting = "Sighting";
        public const string Sound = "Sound";
        public const string Traces = "Traces";
        public const string ExpertObservation = "ExpertObservation";
    }

    /// <summary>
    /// Ecological roles.
    /// </summary>
    public static class EcologicalRoles
    {
        public const string Predator = "Predator";
        public const string Scavenger = "Scavenger";
        public const string Herbivore = "Herbivore";
        public const string Decomposer = "Decomposer";
        public const string BlightAdapted = "BlightAdapted";
    }
}
