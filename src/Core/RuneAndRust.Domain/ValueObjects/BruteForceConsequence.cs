// ------------------------------------------------------------------------------
// <copyright file="BruteForceConsequence.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents a potential consequence of a brute force bypass attempt.
// Consequences balance the power of brute force with appropriate trade-offs.
// Part of v0.15.4h Alternative Bypass Methods implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

// =============================================================================
// CONSEQUENCE TYPE ENUM
// =============================================================================

/// <summary>
/// Types of consequences that can result from brute force attempts.
/// </summary>
/// <remarks>
/// <para>
/// Consequences represent the trade-offs of using brute force instead of
/// finesse-based bypass methods. Each consequence type has different
/// severity and implications for stealth, resources, and character safety.
/// </para>
/// </remarks>
public enum ConsequenceType
{
    /// <summary>
    /// Noise that may alert nearby creatures.
    /// </summary>
    /// <remarks>
    /// Noise consequences have an associated <see cref="NoiseLevel"/> that
    /// determines the alert radius and potential guard response.
    /// </remarks>
    Noise,

    /// <summary>
    /// Damage to items behind or inside the obstacle.
    /// </summary>
    /// <remarks>
    /// Content damage applies a probability-based chance to damage items
    /// within containers or rooms behind forced doors.
    /// </remarks>
    ContentDamage,

    /// <summary>
    /// Physical exhaustion from the effort.
    /// </summary>
    /// <remarks>
    /// Exhaustion consequences affect stamina and may impose penalties
    /// on subsequent physical actions.
    /// </remarks>
    Exhaustion,

    /// <summary>
    /// Damage to the character attempting the brute force.
    /// </summary>
    /// <remarks>
    /// Self-damage typically occurs on fumbles from things like splinters,
    /// strained muscles, or mechanism backlash.
    /// </remarks>
    SelfDamage,

    /// <summary>
    /// Maximum noise level triggered (entire area alerted).
    /// </summary>
    /// <remarks>
    /// MaxNoise is an escalated version of Noise that always triggers
    /// at Extreme level, typically from fumbles.
    /// </remarks>
    MaxNoise,

    /// <summary>
    /// Security system alerted.
    /// </summary>
    /// <remarks>
    /// Security alerts may trigger alarms, summon guards, or activate
    /// automated defense systems.
    /// </remarks>
    SecurityAlert,

    /// <summary>
    /// Structural damage that may affect surroundings.
    /// </summary>
    /// <remarks>
    /// Structural damage can cause cave-ins, weaken floors, or create
    /// new hazards in the environment.
    /// </remarks>
    StructuralDamage
}

// =============================================================================
// BRUTE FORCE CONSEQUENCE VALUE OBJECT
// =============================================================================

/// <summary>
/// Represents a potential consequence of a brute force bypass attempt.
/// </summary>
/// <remarks>
/// <para>
/// Consequences balance the power of brute force with appropriate trade-offs.
/// Each consequence defines:
/// <list type="bullet">
///   <item><description>Type: Category of consequence (noise, damage, etc.)</description></item>
///   <item><description>Noise Level: Alert radius if noise-based</description></item>
///   <item><description>Probability: Chance the consequence applies (0.0-1.0)</description></item>
///   <item><description>Avoidability: Whether critical success negates it</description></item>
///   <item><description>Severity: Impact rating for UI/AI decision-making</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Design Principle:</b> Brute force should always be an option, but the
/// consequences should make players consider whether finesse-based approaches
/// are worth the extra effort.
/// </para>
/// </remarks>
/// <param name="Type">Category of consequence.</param>
/// <param name="NoiseLevel">Noise level if this is a noise consequence.</param>
/// <param name="Probability">Chance this consequence applies (0.0-1.0).</param>
/// <param name="CanBeAvoidedOnCritical">Whether critical success avoids this consequence.</param>
/// <param name="SeverityRating">Impact rating from 1 (minor) to 5 (severe).</param>
public readonly record struct BruteForceConsequence(
    ConsequenceType Type,
    NoiseLevel NoiseLevel,
    float Probability,
    bool CanBeAvoidedOnCritical,
    int SeverityRating)
{
    // =========================================================================
    // STATIC FACTORY PROPERTIES - Standard Consequences
    // =========================================================================

    /// <summary>
    /// Loud noise that alerts nearby creatures (60 ft radius).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Standard consequence for simple doors. Guards within 60 feet will
    /// likely hear and investigate.
    /// </para>
    /// <para>
    /// <b>Avoidable on Critical:</b> Yes - a precise strike minimizes noise.
    /// </para>
    /// </remarks>
    public static BruteForceConsequence LoudNoise => new(
        Type: ConsequenceType.Noise,
        NoiseLevel: NoiseLevel.Loud,
        Probability: 1.0f,
        CanBeAvoidedOnCritical: true,
        SeverityRating: 2);

    /// <summary>
    /// Very loud noise that alerts guards throughout the area (120 ft radius).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Consequence for reinforced doors and vaults. The tremendous crash
    /// can be heard throughout the building.
    /// </para>
    /// <para>
    /// <b>Avoidable on Critical:</b> Yes - reduced to Loud on critical success.
    /// </para>
    /// </remarks>
    public static BruteForceConsequence VeryLoudNoise => new(
        Type: ConsequenceType.Noise,
        NoiseLevel: NoiseLevel.VeryLoud,
        Probability: 1.0f,
        CanBeAvoidedOnCritical: true,
        SeverityRating: 3);

    /// <summary>
    /// Risk of damaging contents within the obstacle (50% chance).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applies to containers and reinforced doors. Delicate items behind
    /// the obstacle may be damaged by the force of entry.
    /// </para>
    /// <para>
    /// <b>Probability:</b> 50% chance of occurring.
    /// </para>
    /// <para>
    /// <b>Avoidable on Critical:</b> Yes - precise entry protects contents.
    /// </para>
    /// </remarks>
    public static BruteForceConsequence ContentDamageRisk => new(
        Type: ConsequenceType.ContentDamage,
        NoiseLevel: NoiseLevel.Silent,
        Probability: 0.5f,
        CanBeAvoidedOnCritical: true,
        SeverityRating: 3);

    /// <summary>
    /// Physical exhaustion from tremendous effort.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Primarily applies to vault forcing. The sustained effort required
    /// to breach heavy obstacles leaves the character winded.
    /// </para>
    /// <para>
    /// <b>Avoidable on Critical:</b> Yes - efficient technique reduces strain.
    /// </para>
    /// </remarks>
    public static BruteForceConsequence Exhausting => new(
        Type: ConsequenceType.Exhaustion,
        NoiseLevel: NoiseLevel.Silent,
        Probability: 1.0f,
        CanBeAvoidedOnCritical: true,
        SeverityRating: 2);

    /// <summary>
    /// Self-damage from fumbled attempt (1d6 damage).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Occurs on fumble results. The character hurts themselves through
    /// splinters, strained muscles, or mechanism backlash.
    /// </para>
    /// <para>
    /// <b>Avoidable on Critical:</b> No - only applies on fumble, never on success.
    /// </para>
    /// </remarks>
    public static BruteForceConsequence SelfDamageOnFumble => new(
        Type: ConsequenceType.SelfDamage,
        NoiseLevel: NoiseLevel.Silent,
        Probability: 1.0f,
        CanBeAvoidedOnCritical: false,
        SeverityRating: 3);

    /// <summary>
    /// Maximum noise from catastrophic fumble (entire area alerted).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Occurs on fumble results. The failed attempt creates extreme noise
    /// that alerts everyone in the vicinity.
    /// </para>
    /// <para>
    /// <b>Avoidable on Critical:</b> No - only applies on fumble, never on success.
    /// </para>
    /// </remarks>
    public static BruteForceConsequence MaxNoiseOnFumble => new(
        Type: ConsequenceType.MaxNoise,
        NoiseLevel: NoiseLevel.Extreme,
        Probability: 1.0f,
        CanBeAvoidedOnCritical: false,
        SeverityRating: 5);

    /// <summary>
    /// Security alert triggered by forcing secured obstacles.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applies to obstacles with integrated security systems. Breaking
    /// through may trigger alarms or summon automated defenses.
    /// </para>
    /// <para>
    /// <b>Avoidable on Critical:</b> Yes - careful entry bypasses triggers.
    /// </para>
    /// </remarks>
    public static BruteForceConsequence SecurityAlertRisk => new(
        Type: ConsequenceType.SecurityAlert,
        NoiseLevel: NoiseLevel.Silent,
        Probability: 0.3f,
        CanBeAvoidedOnCritical: true,
        SeverityRating: 4);

    // =========================================================================
    // INSTANCE METHODS
    // =========================================================================

    /// <summary>
    /// Gets a narrative description of this consequence for display.
    /// </summary>
    /// <returns>A descriptive string suitable for player-facing text.</returns>
    /// <remarks>
    /// Returns context-appropriate descriptions based on the consequence type
    /// and noise level.
    /// </remarks>
    public string GetNarrativeDescription() => Type switch
    {
        ConsequenceType.Noise when NoiseLevel == NoiseLevel.Loud =>
            "The noise echoes through nearby corridors.",
        ConsequenceType.Noise when NoiseLevel == NoiseLevel.VeryLoud =>
            "The tremendous crash could be heard throughout the building.",
        ConsequenceType.Noise when NoiseLevel == NoiseLevel.Extreme =>
            "The catastrophic noise alerts everyone in the area!",
        ConsequenceType.ContentDamage =>
            "Some items behind the obstacle may have been damaged.",
        ConsequenceType.Exhaustion =>
            "The effort leaves you winded.",
        ConsequenceType.SelfDamage =>
            "You hurt yourself in the attempt.",
        ConsequenceType.MaxNoise =>
            "The failed attempt creates a tremendous racket!",
        ConsequenceType.SecurityAlert =>
            "A security system has been triggered.",
        ConsequenceType.StructuralDamage =>
            "The surrounding structure shows signs of stress.",
        _ => "An unexpected consequence occurred."
    };

    /// <summary>
    /// Creates a display string showing consequence details for UI.
    /// </summary>
    /// <returns>
    /// A formatted string including type, probability, and avoidability.
    /// </returns>
    /// <example>
    /// <code>
    /// var consequence = BruteForceConsequence.ContentDamageRisk;
    /// Console.WriteLine(consequence.ToDisplayString());
    /// // Output: "Content damage risk (50% chance) (avoided on critical)"
    /// </code>
    /// </example>
    public string ToDisplayString()
    {
        var avoidable = CanBeAvoidedOnCritical ? " (avoided on critical)" : string.Empty;
        var probability = Probability < 1.0f ? $" ({Probability:P0} chance)" : string.Empty;

        return Type switch
        {
            ConsequenceType.Noise => $"{NoiseLevel} noise{avoidable}",
            ConsequenceType.ContentDamage => $"Content damage risk{probability}{avoidable}",
            ConsequenceType.Exhaustion => $"Exhausting{avoidable}",
            ConsequenceType.SelfDamage => $"Self-damage{probability}",
            ConsequenceType.MaxNoise => $"Maximum noise alert",
            ConsequenceType.SecurityAlert => $"Security alert risk{probability}{avoidable}",
            ConsequenceType.StructuralDamage => $"Structural damage risk{probability}{avoidable}",
            _ => $"{Type}{probability}{avoidable}"
        };
    }

    /// <summary>
    /// Gets the alert radius in feet for noise-based consequences.
    /// </summary>
    /// <returns>
    /// The radius in feet that the noise can be heard, or 0 for non-noise consequences.
    /// </returns>
    public int GetAlertRadiusFeet() => Type switch
    {
        ConsequenceType.Noise or ConsequenceType.MaxNoise => NoiseLevel switch
        {
            NoiseLevel.Silent => 0,
            NoiseLevel.Quiet => 5,
            NoiseLevel.Moderate => 30,
            NoiseLevel.Loud => 60,
            NoiseLevel.VeryLoud => 120,
            NoiseLevel.Extreme => int.MaxValue, // Entire area
            _ => 0
        },
        _ => 0
    };

    /// <summary>
    /// Determines if this consequence should be applied based on probability.
    /// </summary>
    /// <param name="random">Random instance for probability check.</param>
    /// <returns>True if the consequence should be applied.</returns>
    /// <remarks>
    /// Consequences with Probability of 1.0 always apply. Lower probabilities
    /// are checked against a random roll.
    /// </remarks>
    public bool ShouldApply(Random random)
    {
        if (Probability >= 1.0f)
        {
            return true;
        }

        return random.NextSingle() < Probability;
    }
}
