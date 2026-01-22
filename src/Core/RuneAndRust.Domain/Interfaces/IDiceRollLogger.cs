using RuneAndRust.Domain.Records;

namespace RuneAndRust.Domain.Interfaces;

/// <summary>
/// Interface for logging and querying dice rolls.
/// </summary>
/// <remarks>
/// <para>
/// Provides methods for:
/// <list type="bullet">
///   <item><description>Logging rolls with full metadata</description></item>
///   <item><description>Retrieving recent roll history</description></item>
///   <item><description>Filtering rolls by context prefix</description></item>
///   <item><description>Clearing roll history</description></item>
/// </list>
/// </para>
/// <para>
/// Implementations may store rolls in memory, database, or external systems.
/// </para>
/// </remarks>
public interface IDiceRollLogger
{
    /// <summary>
    /// Logs a dice roll with full metadata.
    /// </summary>
    /// <param name="log">The roll log entry to store.</param>
    /// <remarks>
    /// <para>
    /// Implementations should:
    /// <list type="bullet">
    ///   <item><description>Store the log for later retrieval</description></item>
    ///   <item><description>Enforce any storage limits (e.g., max history size)</description></item>
    ///   <item><description>Be thread-safe if used in concurrent contexts</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    void LogRoll(DiceRollLog log);

    /// <summary>
    /// Retrieves the most recent rolls from history.
    /// </summary>
    /// <param name="count">Maximum number of rolls to return (default 100).</param>
    /// <returns>
    /// A read-only list of roll logs, ordered from oldest to newest.
    /// Returns fewer than <paramref name="count"/> if history is smaller.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The returned list is ordered chronologically (oldest first).
    /// </para>
    /// <para>
    /// If count exceeds available history, returns all available rolls.
    /// </para>
    /// </remarks>
    IReadOnlyList<DiceRollLog> GetRollHistory(int count = 100);

    /// <summary>
    /// Retrieves rolls matching a specific context prefix.
    /// </summary>
    /// <param name="contextPrefix">
    /// The context prefix to filter by (e.g., "Combat:", "Skill:Acrobatics").
    /// </param>
    /// <returns>
    /// A read-only list of matching roll logs, ordered from oldest to newest.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Context matching is case-insensitive.
    /// </para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    ///   <item><description>"Combat:" matches "Combat:Attack", "Combat:Damage", etc.</description></item>
    ///   <item><description>"Skill:Stealth" matches only "Skill:Stealth" rolls</description></item>
    ///   <item><description>"" (empty) matches all rolls</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    IReadOnlyList<DiceRollLog> GetRollsByContext(string contextPrefix);

    /// <summary>
    /// Clears all stored roll history.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use with caution - this permanently removes all logged rolls.
    /// </para>
    /// <para>
    /// Useful for:
    /// <list type="bullet">
    ///   <item><description>Starting a new game session</description></item>
    ///   <item><description>Memory management</description></item>
    ///   <item><description>Testing scenarios</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    void ClearHistory();

    /// <summary>
    /// Gets the current number of rolls in history.
    /// </summary>
    int HistoryCount { get; }

    /// <summary>
    /// Gets the maximum number of rolls stored before oldest are discarded.
    /// </summary>
    int MaxHistorySize { get; }
}
