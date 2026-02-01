// ═══════════════════════════════════════════════════════════════════════════════
// CorruptionStage.cs
// Represents the progressive stages of Runic Blight Corruption for a character.
// Each stage corresponds to a 20-point corruption range and indicates escalating
// physical and spiritual contamination from exposure to corrupted runic energies.
// Unlike Psychic Stress (mental strain), Corruption tracks the body's absorption
// of malevolent magical energy and its progressive mutation effects.
// Version: 0.18.1a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the five progressive stages of Runic Blight Corruption.
/// </summary>
/// <remarks>
/// <para>
/// Corruption tracks physical and spiritual contamination from exposure to
/// corrupted runic energies in Aethelgard. Unlike Psychic Stress (mental strain),
/// Corruption represents the body's absorption of malevolent magical energy
/// and its progressive mutation effects.
/// </para>
/// <para>
/// Stage thresholds and their effects:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="Uncorrupted"/> (0-19): No effects. The character's essence is pure.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Tainted"/> (20-39): Minor cosmetic signs. Pure entities sense unease.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Infected"/> (40-59): Visible corruption marks. -1 social checks.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Blighted"/> (60-79): Physical mutations begin. -2 social, +1 horror.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Corrupted"/> (80-99): Severe mutations. -3 social, +2 horror. Mutation risk.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Consumed"/> (100): Mutation Check required. Character transformation imminent.
///     </description>
///   </item>
/// </list>
/// <para>
/// Enum values are explicitly assigned (0-5) for stable serialization and
/// persistence. These integer values must not be changed once persisted.
/// </para>
/// </remarks>
/// <seealso cref="RuneAndRust.Domain.ValueObjects.CorruptionState"/>
public enum CorruptionStage
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CORRUPTION STAGE TIERS (ordered 0-5, matching progression severity)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Corruption 0-19: No corruption present.
    /// </summary>
    /// <remarks>
    /// The character's essence is pure and untouched by Runic Blight.
    /// No mechanical effects or visible signs of corruption.
    /// </remarks>
    Uncorrupted = 0,

    /// <summary>
    /// Corruption 20-39: Minor corruption exposure.
    /// </summary>
    /// <remarks>
    /// The character has been exposed to Runic Blight but shows only
    /// minor cosmetic signs. Pure entities may sense unease around them.
    /// No mechanical penalties, but narrative implications begin.
    /// </remarks>
    Tainted = 1,

    /// <summary>
    /// Corruption 40-59: Moderate corruption infection.
    /// </summary>
    /// <remarks>
    /// Visible corruption marks appear on the character's body.
    /// Mechanical effect: -1 penalty to social checks with pure entities.
    /// The corruption is actively growing and cannot be hidden.
    /// </remarks>
    Infected = 2,

    /// <summary>
    /// Corruption 60-79: Significant corruption spread.
    /// </summary>
    /// <remarks>
    /// Physical mutations begin manifesting.
    /// Mechanical effects:
    /// <list type="bullet">
    ///   <item><description>-2 penalty to social checks with pure entities</description></item>
    ///   <item><description>+1 to horror/intimidation against susceptible targets</description></item>
    /// </list>
    /// The character's humanity is visibly compromised.
    /// </remarks>
    Blighted = 3,

    /// <summary>
    /// Corruption 80-99: Severe corruption.
    /// </summary>
    /// <remarks>
    /// Severe mutations are present and growing.
    /// Mechanical effects:
    /// <list type="bullet">
    ///   <item><description>-3 penalty to social checks with pure entities</description></item>
    ///   <item><description>+2 to horror/intimidation against susceptible targets</description></item>
    ///   <item><description>May gain corrupted abilities at cost</description></item>
    /// </list>
    /// The character is at high risk of being Consumed.
    /// </remarks>
    Corrupted = 4,

    /// <summary>
    /// Corruption 100: Maximum corruption reached.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The character's essence is overwhelmed by Runic Blight.
    /// A <b>Mutation Check</b> is required immediately.
    /// </para>
    /// <para>
    /// Mutation Check outcomes:
    /// <list type="bullet">
    ///   <item><description>Success: Gain a permanent mutation, corruption resets to 75</description></item>
    ///   <item><description>Failure: Gain a severe mutation, corruption resets to 50</description></item>
    ///   <item><description>Critical Failure: Character may become an NPC (GM discretion)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Consumed = 5
}
