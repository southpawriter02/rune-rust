// ═══════════════════════════════════════════════════════════════════════════════
// IDiceHistoryService.cs
// Service interface for tracking and querying dice roll history.
// Version: 0.12.0b
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Application.Models;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Records;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service interface for tracking and querying dice roll history.
/// </summary>
/// <remarks>
/// <para>This service provides methods for:</para>
/// <list type="bullet">
///   <item><description>Recording dice rolls (called by DiceService)</description></item>
///   <item><description>Retrieving roll history for a player</description></item>
///   <item><description>Getting luck rating and streak information</description></item>
///   <item><description>Retrieving recent rolls for display</description></item>
///   <item><description>Getting aggregated dice statistics</description></item>
/// </list>
/// <para>
/// The service maintains dice roll history in the player's DiceHistory entity.
/// It is called by the DiceService when rolls are made to automatically track
/// roll statistics.
/// </para>
/// </remarks>
public interface IDiceHistoryService
{
    /// <summary>
    /// Records a dice roll to the player's history.
    /// </summary>
    /// <param name="player">The player who made the roll.</param>
    /// <param name="roll">The roll record to add.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="player"/> or <paramref name="roll"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is called by the DiceService after each roll. It updates
    /// the player's DiceRollHistory entity with the new roll data, including:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Total roll counts</description></item>
    ///   <item><description>Natural 20/1 counts (for d20 rolls)</description></item>
    ///   <item><description>Streak tracking (for d20 rolls)</description></item>
    ///   <item><description>Recent rolls buffer</description></item>
    /// </list>
    /// </remarks>
    void RecordRoll(Player player, DiceRollRecord roll);

    /// <summary>
    /// Gets the player's dice roll history entity.
    /// </summary>
    /// <param name="player">The player whose history to retrieve.</param>
    /// <returns>The DiceRollHistory entity, creating one if it doesn't exist.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="player"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method ensures the player has a DiceRollHistory entity initialized.
    /// If one doesn't exist, it will be created and associated with the player.
    /// </para>
    /// </remarks>
    DiceRollHistory GetHistory(Player player);

    /// <summary>
    /// Gets the player's current luck rating.
    /// </summary>
    /// <param name="player">The player whose rating to retrieve.</param>
    /// <returns>The calculated luck rating based on roll history.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="player"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Luck rating is calculated from the player's average d20 roll compared
    /// to the expected average of 10.5. See <see cref="LuckRating"/> for
    /// threshold details.
    /// </para>
    /// </remarks>
    LuckRating GetLuckRating(Player player);

    /// <summary>
    /// Gets the player's current streak count.
    /// </summary>
    /// <param name="player">The player whose streak to retrieve.</param>
    /// <returns>
    /// The current streak: positive values indicate lucky streaks (consecutive
    /// rolls >= 11), negative values indicate unlucky streaks (consecutive rolls &lt; 11).
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="player"/> is null.
    /// </exception>
    int GetCurrentStreak(Player player);

    /// <summary>
    /// Gets the player's recent rolls.
    /// </summary>
    /// <param name="player">The player whose rolls to retrieve.</param>
    /// <param name="count">Maximum number of rolls to return (default 10, max 20).</param>
    /// <returns>
    /// The most recent rolls, up to the specified count. Returns rolls in
    /// chronological order (oldest first).
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="player"/> is null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="count"/> is less than or equal to zero.
    /// </exception>
    IReadOnlyList<DiceRollRecord> GetRecentRolls(Player player, int count = 10);

    /// <summary>
    /// Gets aggregated dice statistics for the player.
    /// </summary>
    /// <param name="player">The player whose statistics to retrieve.</param>
    /// <returns>
    /// Aggregated dice statistics including all calculated values for display
    /// in the statistics view.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="player"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method aggregates all statistics from the player's DiceRollHistory
    /// into a single DiceStatistics record suitable for display. It includes:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Roll counts and natural 20/1 counts</description></item>
    ///   <item><description>Average rolls and rates</description></item>
    ///   <item><description>Luck percentage and rating</description></item>
    ///   <item><description>Streak information</description></item>
    /// </list>
    /// </remarks>
    DiceStatistics GetStatistics(Player player);
}
