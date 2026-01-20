// ═══════════════════════════════════════════════════════════════════════════════
// CombatRating.cs
// Enum defining combat performance rating tiers.
// Version: 0.12.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Models;

/// <summary>
/// Combat performance rating tiers calculated from player statistics.
/// </summary>
/// <remarks>
/// <para>
/// Combat rating is a derived metric that evaluates overall combat performance
/// based on multiple factors including kill/death ratio, critical hit rate,
/// miss rate, and boss kills. Ratings range from Novice to Legend.
/// </para>
/// <para>Combat rating score calculation:</para>
/// <list type="bullet">
///   <item><description>K/D Ratio contribution: ratio × 10, capped at 40 points</description></item>
///   <item><description>Critical hit rate contribution: rate × 200, capped at 20 points</description></item>
///   <item><description>Miss rate penalty: rate × 100, up to -20 points</description></item>
///   <item><description>Boss kills contribution: kills × 5, capped at 20 points</description></item>
/// </list>
/// <para>Rating thresholds (score range 0-100):</para>
/// <list type="bullet">
///   <item><description><see cref="Novice"/>: Score 0-14</description></item>
///   <item><description><see cref="Apprentice"/>: Score 15-29</description></item>
///   <item><description><see cref="Journeyman"/>: Score 30-44</description></item>
///   <item><description><see cref="Skilled"/>: Score 45-59</description></item>
///   <item><description><see cref="Veteran"/>: Score 60-74</description></item>
///   <item><description><see cref="Master"/>: Score 75-89</description></item>
///   <item><description><see cref="Legend"/>: Score 90-100</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Get combat rating from statistics service
/// var metrics = statisticsService.GetMetrics(player);
/// var rating = metrics.CombatRating;
///
/// if (rating >= CombatRating.Veteran)
/// {
///     // Unlock veteran-only content
/// }
/// </code>
/// </example>
public enum CombatRating
{
    /// <summary>
    /// Novice combat rating (score 0-14).
    /// </summary>
    /// <remarks>
    /// Starting tier for new players or those with limited combat experience.
    /// </remarks>
    Novice,

    /// <summary>
    /// Apprentice combat rating (score 15-29).
    /// </summary>
    /// <remarks>
    /// Indicates basic combat competency with room for improvement.
    /// </remarks>
    Apprentice,

    /// <summary>
    /// Journeyman combat rating (score 30-44).
    /// </summary>
    /// <remarks>
    /// Solid combat performance with developing tactical awareness.
    /// </remarks>
    Journeyman,

    /// <summary>
    /// Skilled combat rating (score 45-59).
    /// </summary>
    /// <remarks>
    /// Above-average combat ability demonstrating consistent performance.
    /// </remarks>
    Skilled,

    /// <summary>
    /// Veteran combat rating (score 60-74).
    /// </summary>
    /// <remarks>
    /// Experienced fighter with proven track record against tough opponents.
    /// </remarks>
    Veteran,

    /// <summary>
    /// Master combat rating (score 75-89).
    /// </summary>
    /// <remarks>
    /// Exceptional combat prowess with high efficiency and boss-killing capability.
    /// </remarks>
    Master,

    /// <summary>
    /// Legend combat rating (score 90-100).
    /// </summary>
    /// <remarks>
    /// Pinnacle of combat excellence. Reserved for players with outstanding
    /// kill/death ratios, critical hit rates, and boss defeats.
    /// </remarks>
    Legend
}
