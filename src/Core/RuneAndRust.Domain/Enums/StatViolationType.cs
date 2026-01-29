namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes the type of stat violation detected during item verification.
/// </summary>
/// <remarks>
/// <para>
/// Used by <see cref="ValueObjects.StatViolation"/> to indicate which stat
/// category failed validation against tier expectations.
/// </para>
/// <para>
/// Each type corresponds to different validation rules:
/// <list type="bullet">
///   <item><description>Damage - Weapon damage dice/bonus out of range</description></item>
///   <item><description>Defense - Armor defense value out of range</description></item>
///   <item><description>Attribute - Attribute bonus (Might, Finesse, etc.) out of range</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="ValueObjects.StatViolation"/>
/// <seealso cref="ValueObjects.StatVerificationResult"/>
public enum StatViolationType
{
    /// <summary>
    /// Weapon damage stat is outside the expected range for the tier.
    /// </summary>
    /// <remarks>
    /// Damage violations occur when weapon min/max damage values fall outside
    /// tier expectations (e.g., Tier 0 expects 1d6 = 1-6, Tier 4 expects 2d6+4 = 6-16).
    /// </remarks>
    Damage,

    /// <summary>
    /// Armor defense stat is outside the expected range for the tier.
    /// </summary>
    /// <remarks>
    /// Defense violations occur when armor defense values fall outside
    /// tier expectations (e.g., Tier 0 expects 1-2, Tier 4 expects 7-10).
    /// </remarks>
    Defense,

    /// <summary>
    /// Attribute bonus is outside the expected range for the tier.
    /// </summary>
    /// <remarks>
    /// Attribute violations occur when item attribute bonuses (Might, Finesse,
    /// Will, Wits) fall outside tier expectations (e.g., Tier 4 expects +4).
    /// </remarks>
    Attribute
}
