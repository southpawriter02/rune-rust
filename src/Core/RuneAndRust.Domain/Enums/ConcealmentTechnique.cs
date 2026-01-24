namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the concealment techniques available for counter-tracking.
/// </summary>
/// <remarks>
/// <para>
/// Each technique provides a bonus to the concealment roll that is added to
/// the concealer's Wasteland Survival check result. The sum becomes the DC
/// that any tracker must beat to follow the concealed trail.
/// </para>
/// <para>
/// Technique bonuses stack additively when multiple techniques are combined.
/// Time multipliers compound multiplicatively. For example, BrushTracks (+4, x1.5)
/// combined with Backtracking (+4, x1.25) results in +8 bonus and x1.875 time.
/// </para>
/// <para>
/// Technique summary:
/// <list type="bullet">
///   <item><description>HardSurfaces: +2 bonus, x1.0 time, always available</description></item>
///   <item><description>BrushTracks: +4 bonus, x1.5 time, requires foliage/debris</description></item>
///   <item><description>FalseTrail: +6 bonus, x2.0 time, always available</description></item>
///   <item><description>WaterCrossing: +8 bonus, x1.0 time, requires water nearby</description></item>
///   <item><description>Backtracking: +4 bonus, x1.25 time, always available</description></item>
/// </list>
/// </para>
/// </remarks>
public enum ConcealmentTechnique
{
    /// <summary>
    /// Walking on hard surfaces (stone, metal, packed earth) that
    /// don't hold tracks well.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Always available in any environment. The Jötun-built roads and
    /// pre-Shattering concrete leave no trace of passage.
    /// </para>
    /// <para>
    /// Bonus: +2 to concealment roll.
    /// Time cost: None (x1.0 multiplier).
    /// Requirements: None (always available).
    /// </para>
    /// </remarks>
    HardSurfaces = 0,

    /// <summary>
    /// Using branches, debris, or other materials to brush away
    /// tracks while traveling.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Requires foliage or debris in the area to sweep away footprints.
    /// Common in forested areas, ruins, and settlements with debris.
    /// Each step erased as if you were never there.
    /// </para>
    /// <para>
    /// Bonus: +4 to concealment roll.
    /// Time cost: +50% travel time (x1.5 multiplier).
    /// Requirements: Foliage or debris available.
    /// </para>
    /// </remarks>
    BrushTracks = 1,

    /// <summary>
    /// Creating a deliberate false trail to mislead pursuers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Always available but requires significant time investment.
    /// You deliberately create misleading tracks leading in a different
    /// direction, then double back. Let them chase shadows while you slip away.
    /// </para>
    /// <para>
    /// Bonus: +6 to concealment roll.
    /// Time cost: +100% travel time (x2.0 multiplier).
    /// Requirements: None (always available).
    /// </para>
    /// </remarks>
    FalseTrail = 2,

    /// <summary>
    /// Crossing through water to break the scent and visual trail.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Requires a water body (stream, river, pond, swamp) to wade through.
    /// The stream carries away all evidence of your passage, breaking
    /// both scent and visual trails completely.
    /// </para>
    /// <para>
    /// Bonus: +8 to concealment roll.
    /// Time cost: None (x1.0 multiplier), but requires water.
    /// Requirements: Water body nearby.
    /// </para>
    /// </remarks>
    WaterCrossing = 3,

    /// <summary>
    /// Walking backwards over your own trail to confuse pursuers
    /// about your actual direction of travel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Always available in any environment. Your footprints tell a lie
    /// about where you went, making it difficult for trackers to
    /// determine your true direction.
    /// </para>
    /// <para>
    /// Bonus: +4 to concealment roll.
    /// Time cost: +25% travel time (x1.25 multiplier).
    /// Requirements: None (always available).
    /// </para>
    /// </remarks>
    Backtracking = 4
}
