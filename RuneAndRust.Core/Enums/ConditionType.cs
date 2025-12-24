using RuneAndRust.Core.Attributes;

namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines the types of ambient conditions in Rune &amp; Rust (v0.3.3b).
/// Conditions represent room-wide environmental effects that apply persistent
/// stat penalties and/or turn-based damage to all combatants.
/// </summary>
public enum ConditionType
{
    /// <summary>
    /// A low psychic hum that erodes sanity.
    /// Passive: -1 WILL
    /// Active: +2 Stress per turn
    /// </summary>
    [GameDocument(
        "Psychic Resonance",
        "A low hum at the edge of perception that erodes mental stability. Those exposed find their willpower diminished as stress accumulates with each passing moment. The source remains unknown.")]
    PsychicResonance = 0,

    /// <summary>
    /// Corrosive air that damages lungs and tissue.
    /// Passive: None
    /// Active: 1d4 Poison damage per turn
    /// </summary>
    [GameDocument(
        "Toxic Atmosphere",
        "Corrosive vapors that damage lungs and tissue with each breath. No passive impairment, but the damage accumulates rapidly. Gas masks or breath-holding offer brief respite.")]
    ToxicAtmosphere = 1,

    /// <summary>
    /// Numbing frost that slows reactions.
    /// Passive: -1 FINESSE
    /// Active: 1 Ice damage per turn
    /// </summary>
    [GameDocument(
        "Deep Cold",
        "Numbing frost that penetrates to the bone. Reactions slow as extremities grow clumsy, while the constant chill inflicts steady harm. Warm clothing and fire provide relief.")]
    DeepCold = 2,

    /// <summary>
    /// Oppressive heat that saps strength.
    /// Passive: -1 STURDINESS
    /// Active: 1 Fire damage per turn
    /// </summary>
    [GameDocument(
        "Scorching Heat",
        "Oppressive heat that saps physical endurance. The body weakens as constant damage accumulates from the environment. Hydration and shade become precious commodities.")]
    ScorchingHeat = 3,

    /// <summary>
    /// Thick dust or fog that obscures vision.
    /// Passive: -2 WITS (for perception-based checks)
    /// Active: None
    /// </summary>
    [GameDocument(
        "Low Visibility",
        "Thick dust, fog, or darkness that obscures perception. The environment itself offers no harm, but the inability to perceive threats makes danger far more lethal.")]
    LowVisibility = 4,

    /// <summary>
    /// Runic corruption that seeps from the walls.
    /// Passive: -1 WILL, -1 WITS
    /// Active: +1 Corruption per turn
    /// </summary>
    [GameDocument(
        "Blighted Ground",
        "Runic corruption seeping from surfaces, walls, and floor alike. Mental faculties dim as the Blight works its influence. Corruption accumulates with exposure.")]
    BlightedGround = 5,

    /// <summary>
    /// Electrical discharge crackling through the air.
    /// Passive: -1 FINESSE
    /// Active: 1d6 Lightning damage (25% chance per turn)
    /// </summary>
    [GameDocument(
        "Static Field",
        "Electrical discharge crackling through the air. Precision suffers as muscles twitch involuntarily. Periodic lightning strikes threaten anyone in the affected area.")]
    StaticField = 6,

    /// <summary>
    /// The presence of something ancient and terrible.
    /// Passive: -2 WILL
    /// Active: +3 Stress per turn
    /// </summary>
    [GameDocument(
        "Dread Presence",
        "The weight of something ancient and terrible pressing upon the psyche. Willpower crumbles under the oppressive aura while stress mounts with terrible speed. Leaving is the only cure.")]
    DreadPresence = 7
}

/// <summary>
/// Extension methods for ConditionType providing passive penalty lookups.
/// </summary>
public static class ConditionTypeExtensions
{
    /// <summary>
    /// Gets the passive attribute penalties for a condition type.
    /// Penalties are returned as negative values.
    /// </summary>
    /// <param name="type">The condition type.</param>
    /// <returns>Dictionary mapping attributes to their penalty values.</returns>
    public static Dictionary<Attribute, int> GetPassivePenalties(this ConditionType type) => type switch
    {
        ConditionType.PsychicResonance => new Dictionary<Attribute, int> { { Attribute.Will, -1 } },
        ConditionType.DeepCold => new Dictionary<Attribute, int> { { Attribute.Finesse, -1 } },
        ConditionType.ScorchingHeat => new Dictionary<Attribute, int> { { Attribute.Sturdiness, -1 } },
        ConditionType.LowVisibility => new Dictionary<Attribute, int> { { Attribute.Wits, -2 } },
        ConditionType.BlightedGround => new Dictionary<Attribute, int>
        {
            { Attribute.Will, -1 },
            { Attribute.Wits, -1 }
        },
        ConditionType.StaticField => new Dictionary<Attribute, int> { { Attribute.Finesse, -1 } },
        ConditionType.DreadPresence => new Dictionary<Attribute, int> { { Attribute.Will, -2 } },
        ConditionType.ToxicAtmosphere => new Dictionary<Attribute, int>(), // No passive penalties
        _ => new Dictionary<Attribute, int>()
    };
}
