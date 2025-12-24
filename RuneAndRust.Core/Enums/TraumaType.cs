using RuneAndRust.Core.Attributes;

namespace RuneAndRust.Core.Enums;

/// <summary>
/// Represents the category of a permanent psychological trauma.
/// Traumas are acquired when a character fails a Breaking Point resolve check.
/// </summary>
/// <remarks>See: SPEC-TRAUMA-001, Section "Core Concepts".</remarks>
public enum TraumaType
{
    /// <summary>
    /// Fear-based trauma. Triggered by specific environmental conditions.
    /// Example: Nyctophobia (fear of darkness).
    /// </summary>
    [GameDocument(
        "Phobia",
        "Fear-based trauma triggered by specific conditions or stimuli. The afflicted experiences overwhelming dread when exposed to their trigger. Common phobias include darkness, enclosed spaces, and specific creature types.")]
    Phobia = 0,

    /// <summary>
    /// Compulsive behavior trauma. Forces specific actions in certain situations.
    /// Example: Pyromaniac (compulsion to start fires).
    /// </summary>
    [GameDocument(
        "Compulsion",
        "Behavioral trauma forcing specific actions in certain situations. The afflicted feels an irresistible urge to perform the compulsive behavior. Resistance causes mounting stress until the compulsion is satisfied.")]
    Compulsion = 1,

    /// <summary>
    /// Distorted perception trauma. Affects how the character perceives reality.
    /// Example: Paranoia (distrust of allies).
    /// </summary>
    [GameDocument(
        "Delusion",
        "Perceptual trauma distorting how the afflicted perceives reality. Paranoia, hallucinations, and false beliefs fall into this category. The afflicted genuinely experiences their distorted perception as truth.")]
    Delusion = 2,

    /// <summary>
    /// Physical manifestation of psychological trauma.
    /// Example: The Shakes (tremors affecting dexterity).
    /// </summary>
    [GameDocument(
        "Somatic",
        "Physical manifestation of psychological damage. Tremors, tics, and stress-induced ailments represent somatic traumas. The body expresses what the mind cannot process, imposing mechanical penalties.")]
    Somatic = 3
}
