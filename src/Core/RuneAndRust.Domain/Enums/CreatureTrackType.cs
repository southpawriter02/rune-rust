namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Classification of creature track freshness for the Veiðimaðr's Read the Signs ability.
/// Determines the Difficulty Class (DC) of the investigation skill check.
/// </summary>
/// <remarks>
/// <para>Track quality is assessed by the Veiðimaðr when investigating an area for creature signs.
/// Fresher tracks are easier to read, while degraded tracks require greater skill:</para>
/// <list type="bullet">
/// <item><see cref="Fresh"/> — 0–1 hours old, DC 10 (75% success with average bonuses)</item>
/// <item><see cref="Recent"/> — 1–6 hours old, DC 12 (65% success)</item>
/// <item><see cref="Worn"/> — 6–24 hours old, DC 15 (50% success)</item>
/// <item><see cref="Ancient"/> — 24+ hours old, DC 18 (35% success)</item>
/// <item><see cref="Unclear"/> — Degraded or indecipherable, DC 20 (25% success)</item>
/// </list>
/// <para>The Veiðimaðr's Read the Signs ability provides +4 to the investigation check,
/// and Keen Senses adds an additional +1, for a total of +5 from specialization abilities alone.</para>
/// <para>Introduced in v0.20.7a as part of the Veiðimaðr specialization framework.</para>
/// </remarks>
public enum CreatureTrackType
{
    /// <summary>Tracks are 0–1 hours old. Clear, well-defined prints. DC 10.</summary>
    Fresh = 0,

    /// <summary>Tracks are 1–6 hours old. Slightly weathered but readable. DC 12.</summary>
    Recent = 1,

    /// <summary>Tracks are 6–24 hours old. Partially obscured by weather or traffic. DC 15.</summary>
    Worn = 2,

    /// <summary>Tracks are 24+ hours old. Heavily degraded, only faint impressions remain. DC 18.</summary>
    Ancient = 3,

    /// <summary>Tracks are degraded beyond normal age — trampled, washed out, or deliberately obscured. DC 20.</summary>
    Unclear = 4
}
