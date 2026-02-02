namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the types of ambient environmental hazards in realms.
/// </summary>
/// <remarks>
/// <para>
/// Each realm has a primary environmental condition that poses ongoing
/// hazards to characters. Conditions trigger periodic resistance checks
/// and deal damage on failure.
/// </para>
/// <para>
/// Standard mechanics: Base DC 12, STURDINESS check, damage varies by type.
/// Zone modifiers can adjust DC from -6 (very safe) to +8 (extremely dangerous).
/// </para>
/// </remarks>
public enum EnvironmentalConditionType
{
    /// <summary>
    /// No environmental hazard.
    /// </summary>
    /// <remarks>
    /// Safe zones like Midgard's civilized areas have no ambient threats.
    /// </remarks>
    None = 0,

    /// <summary>
    /// Extreme heat causing burns and exhaustion.
    /// </summary>
    /// <remarks>
    /// Primary hazard in Muspelheim.
    /// Check: STURDINESS DC 12, Damage: 2d6 Fire per turn.
    /// Mitigations: Fire Resistance, Cooling equipment, Hearth shelter.
    /// </remarks>
    IntenseHeat = 1,

    /// <summary>
    /// Freezing cold causing frostbite and hypothermia.
    /// </summary>
    /// <remarks>
    /// Primary hazard in Niflheim.
    /// Check: STURDINESS DC 12, Damage: 2d6 Cold per turn.
    /// Mitigations: Cold Resistance, Heating equipment, Warm clothing.
    /// </remarks>
    ExtremeCold = 2,

    /// <summary>
    /// Poisonous air causing respiratory damage.
    /// </summary>
    /// <remarks>
    /// Primary hazard in Helheim.
    /// Check: STURDINESS DC 12, Damage: 2d4 Poison per turn.
    /// Mitigations: Poison Resistance, Respirator, Environmental seal.
    /// </remarks>
    ToxicAtmosphere = 3,

    /// <summary>
    /// Mutagenic fungal spores causing biological corruption.
    /// </summary>
    /// <remarks>
    /// Primary hazard in Vanaheim.
    /// Check: STURDINESS DC 12, Damage: 1d6 Poison + Mutation risk per hour.
    /// Mitigations: Bio-filter, Sealed suit, Antifungal treatment.
    /// </remarks>
    MutagenicSpores = 4,

    /// <summary>
    /// Reality distortion causing psychic damage.
    /// </summary>
    /// <remarks>
    /// Primary hazard in Asgard.
    /// Check: WILL DC 12, Damage: 1d8 Psychic per turn.
    /// Mitigations: Psi-shield, Mental training, Reality anchor.
    /// </remarks>
    RealityFlux = 5,

    /// <summary>
    /// Psychic radiation causing Cognitive Paradox Syndrome progression.
    /// </summary>
    /// <remarks>
    /// Primary hazard in Alfheim.
    /// Check: WILL DC 12, Effect: +1d4 Stress per hour.
    /// Mitigations: Psi-dampener, Mental fortitude, Limited exposure.
    /// </remarks>
    CpsExposure = 6,

    /// <summary>
    /// Complete absence of light.
    /// </summary>
    /// <remarks>
    /// Primary hazard in Svartalfheim.
    /// Effect: Blindness without light source, penalties to actions.
    /// Mitigations: Darkvision, Light source, Echolocation.
    /// </remarks>
    TotalDarkness = 7,

    /// <summary>
    /// Macro-scale environment where everything is enormous.
    /// </summary>
    /// <remarks>
    /// Primary hazard in Jotunheim.
    /// Effect: Increased fall distances, oversized obstacles, massive creatures.
    /// Mitigations: Climbing gear, Size-altering equipment, Flight.
    /// </remarks>
    GiantScale = 8
}
