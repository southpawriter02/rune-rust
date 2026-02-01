namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the six categories of permanent trauma.
/// </summary>
/// <remarks>
/// <para>
/// Traumas are permanent consequences representing irreversible character changes.
/// Each type represents a different aspect of psychological or physical harm.
/// </para>
/// <para>
/// Trauma Categories:
/// <list type="bullet">
/// <item>Cognitive: Mental processing disorders (flashbacks, paranoia)</item>
/// <item>Emotional: Affective regulation issues (guilt, terror)</item>
/// <item>Physical: Somatic manifestations (pain, injury)</item>
/// <item>Social: Interpersonal difficulties (touch aversion, isolation)</item>
/// <item>Existential: Reality/identity crises (doubt, dissociation)</item>
/// <item>Corruption: Blight-related traumas (machine affinity, corruption)</item>
/// </list>
/// </para>
/// </remarks>
public enum TraumaType
{
    /// <summary>
    /// Mental processing disorders and cognitive dysfunction.
    /// </summary>
    /// <remarks>
    /// Examples: Combat Flashbacks, Paranoid Ideation
    /// Manifestation: Intrusive thoughts, perception distortion, memory issues
    /// </remarks>
    Cognitive = 0,

    /// <summary>
    /// Affective regulation and emotional control issues.
    /// </summary>
    /// <remarks>
    /// Examples: Survivor's Guilt, Night Terrors
    /// Manifestation: Inappropriate emotional responses, mood instability
    /// </remarks>
    Emotional = 1,

    /// <summary>
    /// Somatic manifestations and physical dysfunction.
    /// </summary>
    /// <remarks>
    /// Examples: Chronic Pain, Tremors
    /// Manifestation: Persistent pain, reduced physical capability, fatigue
    /// </remarks>
    Physical = 2,

    /// <summary>
    /// Interpersonal difficulties and social dysfunction.
    /// </summary>
    /// <remarks>
    /// Examples: Touch Aversion, Social Withdrawal
    /// Manifestation: Difficulty with relationships, avoidance behaviors
    /// </remarks>
    Social = 3,

    /// <summary>
    /// Reality/identity crises and existential issues.
    /// </summary>
    /// <remarks>
    /// Examples: Reality Doubt, Dissociation
    /// Manifestation: Questioning reality, identity fragmentation
    /// </remarks>
    Existential = 4,

    /// <summary>
    /// Blight-related traumas from corruption exposure.
    /// </summary>
    /// <remarks>
    /// Examples: Machine Affinity, Hollow Resonance
    /// Manifestation: Affinity for infected entities, reality warping
    /// Terminal traumas that often force retirement
    /// </remarks>
    Corruption = 5
}
