// ═══════════════════════════════════════════════════════════════════════════════
// StressThreshold.cs
// Represents the psychological stress threshold tiers for a character. Each
// threshold corresponds to a 20-point stress range and imposes escalating
// mechanical penalties. The stress system creates a "death spiral" where high
// stress reduces Defense, making characters more vulnerable to further stress.
// Version: 0.18.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the psychological stress threshold tiers for a character.
/// </summary>
/// <remarks>
/// <para>
/// Each threshold corresponds to a 20-point stress range and imposes
/// escalating mechanical penalties. Higher thresholds indicate greater
/// psychological distress.
/// </para>
/// <para>
/// The stress system creates a "death spiral" where high stress reduces
/// Defense, making characters more vulnerable to further stress accumulation.
/// </para>
/// <para>
/// Threshold tiers and their effects:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="Calm"/> (0-19): No penalties. Baseline operating state.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Uneasy"/> (20-39): Defense -1. Beginning to feel pressure.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Anxious"/> (40-59): Defense -2. Noticeably affected composure.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Panicked"/> (60-79): Defense -3. Approaching breaking point.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Breaking"/> (80-99): Defense -4. Disadvantage on non-combat checks.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="Trauma"/> (100): Defense -5. Disadvantage on ALL checks. Trauma Check triggered.
///     </description>
///   </item>
/// </list>
/// <para>
/// Enum values are explicitly assigned (0-5) matching the defense penalty
/// magnitude for each tier. These integer values must not be changed once
/// persisted.
/// </para>
/// </remarks>
/// <seealso cref="RuneAndRust.Domain.Extensions.StressThresholdExtensions"/>
/// <seealso cref="RuneAndRust.Domain.ValueObjects.StressState"/>
public enum StressThreshold
{
    // ═══════════════════════════════════════════════════════════════════════════
    // STRESS THRESHOLD TIERS (ordered 0-5, matching defense penalty magnitude)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Stress 0-19: Character is calm with no penalties.
    /// </summary>
    /// <remarks>
    /// This is the baseline state. Characters at this threshold
    /// operate at full capacity with no stress-related effects.
    /// </remarks>
    Calm = 0,

    /// <summary>
    /// Stress 20-39: Character is uneasy with minor penalties.
    /// </summary>
    /// <remarks>
    /// Defense penalty: -1. This threshold indicates the character
    /// is beginning to feel the psychological pressure of Aethelgard's horrors.
    /// </remarks>
    Uneasy = 1,

    /// <summary>
    /// Stress 40-59: Character is anxious with moderate penalties.
    /// </summary>
    /// <remarks>
    /// Defense penalty: -2. The character's composure is noticeably
    /// affected, reducing their ability to evade attacks.
    /// </remarks>
    Anxious = 2,

    /// <summary>
    /// Stress 60-79: Character is panicked with significant penalties.
    /// </summary>
    /// <remarks>
    /// Defense penalty: -3. The character is approaching their
    /// psychological breaking point.
    /// </remarks>
    Panicked = 3,

    /// <summary>
    /// Stress 80-99: Character is breaking with severe penalties.
    /// </summary>
    /// <remarks>
    /// Defense penalty: -4. Additionally, the character has
    /// disadvantage on non-combat skill checks due to mental distress.
    /// </remarks>
    Breaking = 4,

    /// <summary>
    /// Stress 100: Trauma Check triggered.
    /// </summary>
    /// <remarks>
    /// Defense penalty: -5. Disadvantage on ALL checks.
    /// This state immediately triggers a Trauma Check. Based on the
    /// check result, the character either:
    /// <list type="bullet">
    ///   <item><description>Passes: Stress resets to 75, no permanent effect</description></item>
    ///   <item><description>Fails: Gains permanent Trauma, stress resets to 50</description></item>
    /// </list>
    /// </remarks>
    Trauma = 5
}
