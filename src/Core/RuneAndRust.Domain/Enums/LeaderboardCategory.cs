namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categories for leaderboard rankings.
/// </summary>
/// <remarks>
/// <para>Each category tracks different aspects of player performance:</para>
/// <list type="bullet">
///   <item><description>HighScore: Overall composite score based on all activities</description></item>
///   <item><description>Speedrun: Fastest game completion time</description></item>
///   <item><description>NoDeath: Highest level reached without dying</description></item>
///   <item><description>AchievementPoints: Most achievement points earned</description></item>
///   <item><description>BossSlayer: Most bosses defeated</description></item>
/// </list>
/// </remarks>
public enum LeaderboardCategory
{
    /// <summary>
    /// Total composite score based on all game activities.
    /// Sorted descending (highest score first).
    /// </summary>
    HighScore,

    /// <summary>
    /// Fastest game completion time.
    /// Sorted ascending (fastest time first).
    /// Requires game completion to qualify.
    /// </summary>
    Speedrun,

    /// <summary>
    /// Highest level reached without dying.
    /// Sorted descending (highest level first).
    /// Only counts levels before first death.
    /// </summary>
    NoDeath,

    /// <summary>
    /// Total achievement points earned.
    /// Sorted descending (most points first).
    /// </summary>
    AchievementPoints,

    /// <summary>
    /// Total bosses defeated.
    /// Sorted descending (most bosses first).
    /// </summary>
    BossSlayer
}
