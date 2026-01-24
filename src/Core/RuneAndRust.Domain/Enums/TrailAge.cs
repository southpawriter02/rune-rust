namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Classification of trail ages with associated base difficulty classes for tracking.
/// </summary>
/// <remarks>
/// <para>
/// Trail age represents how old the signs of passage are, directly affecting the
/// difficulty of following the trail. Older trails have degraded more, with
/// footprints blown over, scent dissipated, and disturbances settled.
/// </para>
/// <para>
/// Base DCs assume normal conditions. Additional modifiers apply for:
/// <list type="bullet">
///   <item><description>Environmental conditions (rain +4, blood trail -4)</description></item>
///   <item><description>Target behavior (hiding +2, multiple targets -2)</description></item>
///   <item><description>Time elapsed since last check (+1 DC per hour)</description></item>
/// </list>
/// </para>
/// <para>
/// Cold trails (24-48 hours) can only be attempted by Master rank (Rank 5) trackers.
/// </para>
/// </remarks>
public enum TrailAge
{
    /// <summary>
    /// Obvious trail such as a caravan through ash or large group passage.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Unmistakable signs: deep ruts, scattered debris, obvious disturbance.
    /// Even untrained observers might notice this trail.
    /// </para>
    /// <para>
    /// Base DC: 8 (requires ~1-2 net successes).
    /// </para>
    /// </remarks>
    Obvious = 8,

    /// <summary>
    /// Fresh trail less than 1 hour old.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Clear footprints, undisturbed dust patterns, fresh scent.
    /// Standard tracking difficulty for recent passage.
    /// </para>
    /// <para>
    /// Base DC: 12 (requires ~2 net successes).
    /// </para>
    /// </remarks>
    Fresh = 12,

    /// <summary>
    /// Standard trail 2-8 hours old.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Footprints partially obscured, dust settling, scent fading.
    /// Most common tracking scenario in the wasteland.
    /// </para>
    /// <para>
    /// Base DC: 16 (requires ~3 net successes).
    /// </para>
    /// </remarks>
    Standard = 16,

    /// <summary>
    /// Old trail 12-24 hours old.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Significantly degraded signs, requires careful examination.
    /// Wind, weather, and other passage have obscured much evidence.
    /// </para>
    /// <para>
    /// Base DC: 20 (requires ~4 net successes).
    /// </para>
    /// </remarks>
    Old = 20,

    /// <summary>
    /// Deliberately obscured trail where target has taken counter-tracking measures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Target has actively hidden their passage: brushed tracks, false trails,
    /// walked on hard surfaces, or used water crossings.
    /// </para>
    /// <para>
    /// Base DC: 24 (requires ~5 net successes).
    /// </para>
    /// </remarks>
    Obscured = 24,

    /// <summary>
    /// Cold trail 24-48 hours old.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Extremely degraded, only the most subtle signs remain.
    /// Requires Master rank (Rank 5) in Wasteland Survival to attempt.
    /// </para>
    /// <para>
    /// Base DC: 28 (requires ~5+ net successes with high skill).
    /// </para>
    /// </remarks>
    Cold = 28
}
