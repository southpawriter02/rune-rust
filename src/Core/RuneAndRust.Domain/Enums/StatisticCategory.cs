// ═══════════════════════════════════════════════════════════════════════════════
// StatisticCategory.cs
// Enum defining categories of tracked statistics.
// Version: 0.12.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categories of tracked statistics.
/// </summary>
/// <remarks>
/// <para>
/// Statistics are grouped into categories for organized display and filtering
/// in the statistics view. Each category represents a distinct aspect of gameplay.
/// </para>
/// <para>Category descriptions:</para>
/// <list type="bullet">
///   <item><description><see cref="Combat"/>: Battle-related metrics like kills and damage.</description></item>
///   <item><description><see cref="Exploration"/>: Discovery metrics like rooms and secrets.</description></item>
///   <item><description><see cref="Progression"/>: Advancement metrics like XP and items.</description></item>
///   <item><description><see cref="Time"/>: Session and playtime tracking.</description></item>
///   <item><description><see cref="Dice"/>: Roll statistics (implemented in v0.12.0b).</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Get statistics for a specific category
/// var combatStats = statisticsService.GetCategoryStatistics(player, StatisticCategory.Combat);
/// foreach (var (statName, value) in combatStats)
/// {
///     Console.WriteLine($"{statName}: {value}");
/// }
/// </code>
/// </example>
public enum StatisticCategory
{
    /// <summary>
    /// Combat-related statistics: kills, damage, critical hits, deaths.
    /// </summary>
    /// <remarks>
    /// Includes:
    /// <list type="bullet">
    ///   <item><description>MonstersKilled - Total monsters defeated</description></item>
    ///   <item><description>BossesKilled - Boss monsters defeated</description></item>
    ///   <item><description>DamageDealt - Total damage inflicted</description></item>
    ///   <item><description>DamageReceived - Total damage taken</description></item>
    ///   <item><description>CriticalHits - Number of critical strikes</description></item>
    ///   <item><description>AttacksMissed - Number of missed attacks</description></item>
    ///   <item><description>AbilitiesUsed - Combat abilities activated</description></item>
    ///   <item><description>DeathCount - Times the player has died</description></item>
    ///   <item><description>TotalAttacks - Total attack attempts made</description></item>
    /// </list>
    /// </remarks>
    Combat,

    /// <summary>
    /// Exploration-related statistics: rooms, secrets, traps, doors.
    /// </summary>
    /// <remarks>
    /// Includes:
    /// <list type="bullet">
    ///   <item><description>RoomsDiscovered - Unique rooms visited</description></item>
    ///   <item><description>SecretsFound - Hidden areas or items discovered</description></item>
    ///   <item><description>TrapsTriggered - Traps that affected the player</description></item>
    ///   <item><description>TrapsAvoided - Traps successfully evaded</description></item>
    ///   <item><description>DoorsOpened - Doors unlocked or opened</description></item>
    ///   <item><description>ChestsOpened - Treasure chests looted</description></item>
    /// </list>
    /// </remarks>
    Exploration,

    /// <summary>
    /// Progression-related statistics: XP, levels, items, gold, quests.
    /// </summary>
    /// <remarks>
    /// Includes:
    /// <list type="bullet">
    ///   <item><description>XPEarned - Total experience points gained</description></item>
    ///   <item><description>LevelsGained - Character levels achieved</description></item>
    ///   <item><description>ItemsFound - Items picked up from world</description></item>
    ///   <item><description>ItemsCrafted - Items created through crafting</description></item>
    ///   <item><description>GoldEarned - Total currency received</description></item>
    ///   <item><description>GoldSpent - Total currency spent</description></item>
    ///   <item><description>QuestsCompleted - Quests finished successfully</description></item>
    ///   <item><description>PuzzlesSolved - Puzzles completed</description></item>
    ///   <item><description>ResourcesGathered - Resources harvested from nodes</description></item>
    /// </list>
    /// </remarks>
    Progression,

    /// <summary>
    /// Time-related statistics: playtime, sessions, dates.
    /// </summary>
    /// <remarks>
    /// Includes:
    /// <list type="bullet">
    ///   <item><description>PlaytimeSeconds - Total time played in seconds</description></item>
    ///   <item><description>SessionCount - Number of play sessions</description></item>
    ///   <item><description>FirstPlayed - Date of first play session</description></item>
    ///   <item><description>LastPlayed - Date of most recent play session</description></item>
    /// </list>
    /// </remarks>
    Time,

    /// <summary>
    /// Dice roll statistics: rolls, natural 20s/1s, streaks.
    /// </summary>
    /// <remarks>
    /// <para>This category is implemented in v0.12.0b (Dice History).</para>
    /// <para>Will include:</para>
    /// <list type="bullet">
    ///   <item><description>TotalRolls - Total dice rolls made</description></item>
    ///   <item><description>Natural20s - Natural 20 rolls</description></item>
    ///   <item><description>Natural1s - Natural 1 rolls (critical failures)</description></item>
    ///   <item><description>LongestStreak - Longest streak of successes</description></item>
    ///   <item><description>AverageRoll - Statistical average of all rolls</description></item>
    /// </list>
    /// </remarks>
    Dice
}
