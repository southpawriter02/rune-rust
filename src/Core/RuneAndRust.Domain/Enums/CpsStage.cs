// ═══════════════════════════════════════════════════════════════════════════════
// CpsStage.cs
// Represents the progressive stages of Cognitive Paradox Syndrome (CPS).
// Each stage corresponds to a stress threshold range and indicates escalating
// cognitive deterioration from processing reality-bending paradoxes.
// Unlike Corruption (physical taint), CPS tracks the mind's inability to
// reconcile impossible experiences with normal cognition.
// Version: 0.18.2a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the five progressive stages of Cognitive Paradox Syndrome (CPS).
/// </summary>
/// <remarks>
/// <para>
/// CPS is the mental deterioration from processing reality-bending paradoxes.
/// Unlike corruption (physical taint), CPS represents the mind's inability
/// to reconcile impossible experiences with normal cognition.
/// </para>
/// <para>
/// Stage Thresholds (based on Psychic Stress):
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="None"/> (0-19 stress): Clear-minded, no symptoms.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="WeightOfKnowing"/> (20-39 stress): Reality feels "off".
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="GlimmerMadness"/> (40-59 stress): Reality flickers and distorts.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="RuinMadness"/> (60-79 stress): Panic Table triggers on stress events.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="HollowShell"/> (80+ stress): Terminal state, survival check required.
///     </description>
///   </item>
/// </list>
/// </para>
/// <para>
/// Unlike corruption stages, CPS is directly derived from Psychic Stress
/// and can recover when stress is reduced through rest or abilities.
/// </para>
/// <para>
/// Enum values are explicitly assigned (0-4) for stable serialization and
/// persistence. These integer values must not be changed once persisted.
/// </para>
/// </remarks>
/// <seealso cref="RuneAndRust.Domain.ValueObjects.CpsState"/>
public enum CpsStage
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CPS STAGE TIERS (ordered 0-4, matching progression severity)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// No cognitive impairment (0-19 stress).
    /// </summary>
    /// <remarks>
    /// The character's mind is clear and unaffected by paradox exposure.
    /// No mechanical penalties or special behaviors.
    /// </remarks>
    None = 0,

    /// <summary>
    /// Initial stage of cognitive strain (20-39 stress).
    /// </summary>
    /// <remarks>
    /// <para>
    /// "The Weight of Knowing" — reality feels subtly wrong.
    /// The character perceives minor distortions at the edge of vision.
    /// </para>
    /// <para>
    /// UI Effects: Occasional text flickers, subtle color shifts.
    /// </para>
    /// </remarks>
    WeightOfKnowing = 1,

    /// <summary>
    /// Moderate cognitive deterioration (40-59 stress).
    /// </summary>
    /// <remarks>
    /// <para>
    /// "Glimmer Madness" — reality actively flickers.
    /// Objects and entities may appear duplicated or distorted.
    /// </para>
    /// <para>
    /// UI Effects: More frequent distortions, unreliable perception hints.
    /// </para>
    /// </remarks>
    GlimmerMadness = 2,

    /// <summary>
    /// Severe cognitive impairment (60-79 stress).
    /// </summary>
    /// <remarks>
    /// <para>
    /// "Ruin Madness" — the mind fractures under paradox weight.
    /// Panic Table rolls triggered on stress-inducing events.
    /// </para>
    /// <para>
    /// UI Effects: Major distortions, unreliable information.
    /// Combat Effects: d10 Panic Table on combat/horror triggers.
    /// </para>
    /// </remarks>
    RuinMadness = 3,

    /// <summary>
    /// Terminal cognitive collapse (80+ stress).
    /// </summary>
    /// <remarks>
    /// <para>
    /// "Hollow Shell" — the mind has shattered.
    /// Character must pass survival check or be lost permanently.
    /// </para>
    /// <para>
    /// Terminal State: WILL check vs DC 4 to survive.
    /// Failure: Character becomes unplayable NPC.
    /// Success: Stress reduced to 79, character survives but scarred.
    /// </para>
    /// </remarks>
    HollowShell = 4
}
