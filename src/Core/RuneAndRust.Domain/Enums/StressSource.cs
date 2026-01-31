// ═══════════════════════════════════════════════════════════════════════════════
// StressSource.cs
// Categorizes the sources of psychic stress accumulation in the Trauma Economy.
// Each source type enables contextual resistance modifiers, stress history
// tracking for analytics, and descriptive logging for debugging and playtesting.
// Specific stress values and resistance DCs are defined in the stress-sources.json
// configuration file (v0.18.0e).
// Version: 0.18.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes the sources of psychic stress accumulation.
/// </summary>
/// <remarks>
/// <para>
/// Stress sources are used to:
/// <list type="bullet">
///   <item>
///     <description>
///       Apply contextual resistance modifiers (e.g., gear that reduces combat stress).
///     </description>
///   </item>
///   <item>
///     <description>
///       Track stress history for analytics and achievements.
///     </description>
///   </item>
///   <item>
///     <description>
///       Provide descriptive logging for debugging and playtesting.
///     </description>
///   </item>
/// </list>
/// </para>
/// <para>
/// Specific stress values and resistance DCs are defined in the
/// <c>stress-sources.json</c> configuration file (v0.18.0e).
/// </para>
/// <para>
/// This enum replaces the v0.15.1e StressSource which had narrow skill-check-specific
/// values (None, Corruption, Fumble, CorruptedObject, ExtendedExposure). The v0.18.0a
/// version provides broader system-wide categories for the full Trauma Economy.
/// </para>
/// </remarks>
/// <seealso cref="RuneAndRust.Domain.Extensions.StressSourceExtensions"/>
public enum StressSource
{
    // ═══════════════════════════════════════════════════════════════════════════
    // STRESS SOURCE CATEGORIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Stress from combat encounters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Includes: enemy fear auras, psychic attacks, witnessing ally harm,
    /// facing Forlorn, fumbles during combat skill checks, and other
    /// combat-related psychological trauma.
    /// </para>
    /// <para>
    /// Combat stress is typically resistable via WILL-based resistance checks.
    /// Characters with high stress resistance or anti-fear equipment can
    /// mitigate combat stress accumulation.
    /// </para>
    /// </remarks>
    Combat,

    /// <summary>
    /// Stress from exploration and discovery.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Includes: disturbing discoveries, corpse-filled rooms, paradox
    /// encounters, reading forbidden texts, interacting with corrupted
    /// objects and artifacts, and other exploration-related stress.
    /// </para>
    /// <para>
    /// Exploration stress is typically resistable. Characters trained in
    /// investigation or with relevant backgrounds may have contextual
    /// resistance bonuses.
    /// </para>
    /// </remarks>
    Exploration,

    /// <summary>
    /// Stress from narrative events and story developments.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Includes: traumatic revelations, NPC betrayals, moral compromises,
    /// and other story-driven psychological impacts.
    /// </para>
    /// <para>
    /// Narrative stress is typically <strong>not</strong> resistable — these
    /// are unavoidable story beats that affect all characters equally
    /// regardless of their WILL attribute or resistance modifiers.
    /// </para>
    /// </remarks>
    Narrative,

    /// <summary>
    /// Stress from heretical or forbidden activities.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Includes: using Blót-Priest abilities, invoking forbidden runes,
    /// and other actions that violate the Combine's orthodoxy.
    /// Often linked to Corruption gain.
    /// </para>
    /// <para>
    /// Heretical stress is typically resistable, but characters who
    /// frequently engage in heretical activities may accumulate
    /// Corruption alongside stress.
    /// </para>
    /// </remarks>
    Heretical,

    /// <summary>
    /// Stress from environmental hazards.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Includes: exposure to Reality Bleed zones, prolonged darkness,
    /// extreme temperatures, extended exposure during multi-stage
    /// procedures in corrupted areas, and other environmental dangers.
    /// </para>
    /// <para>
    /// Environmental stress is typically resistable. Characters with
    /// appropriate gear or survival training may have reduced
    /// environmental stress accumulation.
    /// </para>
    /// </remarks>
    Environmental,

    /// <summary>
    /// Stress resulting from Corruption accumulation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Failing Corruption resistance checks causes stress as the
    /// character's mind struggles against the Runic Blight infection.
    /// This creates a feedback loop between the Corruption and Stress
    /// systems in the Trauma Economy.
    /// </para>
    /// <para>
    /// Corruption-sourced stress is typically <strong>not</strong> resistable —
    /// it is an automatic consequence of failing a Corruption check.
    /// </para>
    /// </remarks>
    Corruption
}
