namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

/// <summary>
/// Represents a single entry on a leaderboard.
/// </summary>
/// <remarks>
/// <para>Leaderboard entries capture:</para>
/// <list type="bullet">
///   <item><description>Player identification (name, class)</description></item>
///   <item><description>Performance metrics (level, score)</description></item>
///   <item><description>Category classification</description></item>
///   <item><description>Timestamp for when the score was achieved</description></item>
///   <item><description>Optional completion time for speedrun category</description></item>
/// </list>
/// </remarks>
public class LeaderboardEntry : IEntity
{
    /// <summary>
    /// Gets the unique identifier for this leaderboard entry.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the name of the player who achieved this score.
    /// </summary>
    public string PlayerName { get; private set; } = null!;

    /// <summary>
    /// Gets the class name of the player's character.
    /// </summary>
    public string ClassName { get; private set; } = null!;

    /// <summary>
    /// Gets the level the player reached.
    /// </summary>
    public int Level { get; private set; }

    /// <summary>
    /// Gets the score value for this entry.
    /// </summary>
    /// <remarks>
    /// Interpretation depends on category:
    /// - HighScore: Composite score
    /// - Speedrun: Completion time in seconds (lower is better)
    /// - NoDeath: Level reached before first death
    /// - AchievementPoints: Total achievement points
    /// - BossSlayer: Total bosses killed
    /// </remarks>
    public long Score { get; private set; }

    /// <summary>
    /// Gets the leaderboard category this entry belongs to.
    /// </summary>
    public LeaderboardCategory Category { get; private set; }

    /// <summary>
    /// Gets the timestamp when this score was achieved.
    /// </summary>
    public DateTime AchievedAt { get; private set; }

    /// <summary>
    /// Gets the completion time for speedrun entries.
    /// Null for non-speedrun categories.
    /// </summary>
    public TimeSpan? CompletionTime { get; private set; }

    /// <summary>
    /// Private constructor for EF Core and factory method.
    /// </summary>
    private LeaderboardEntry() { }

    /// <summary>
    /// Creates a new LeaderboardEntry.
    /// </summary>
    /// <param name="playerName">The player's name.</param>
    /// <param name="className">The player's class name.</param>
    /// <param name="level">The level reached.</param>
    /// <param name="score">The score value.</param>
    /// <param name="category">The leaderboard category.</param>
    /// <param name="completionTime">Optional completion time for speedrun.</param>
    /// <returns>A new LeaderboardEntry instance.</returns>
    /// <exception cref="ArgumentException">If playerName or className is null/empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If level or score is negative.</exception>
    public static LeaderboardEntry Create(
        string playerName,
        string className,
        int level,
        long score,
        LeaderboardCategory category,
        TimeSpan? completionTime = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(className);
        ArgumentOutOfRangeException.ThrowIfNegative(level);
        ArgumentOutOfRangeException.ThrowIfNegative(score);

        return new LeaderboardEntry
        {
            Id = Guid.NewGuid(),
            PlayerName = playerName,
            ClassName = className,
            Level = level,
            Score = score,
            Category = category,
            AchievedAt = DateTime.UtcNow,
            CompletionTime = completionTime
        };
    }

    /// <summary>
    /// Gets a display string for the score based on category.
    /// </summary>
    /// <returns>Formatted score string.</returns>
    public string GetScoreDisplay()
    {
        return Category switch
        {
            LeaderboardCategory.Speedrun when CompletionTime.HasValue =>
                $"{CompletionTime.Value:hh\\:mm\\:ss}",
            LeaderboardCategory.HighScore =>
                $"{Score:N0}",
            _ => Score.ToString("N0")
        };
    }

    /// <summary>
    /// Returns a string representation of this leaderboard entry.
    /// </summary>
    /// <returns>A string containing player name, class, level, and score.</returns>
    public override string ToString()
    {
        return $"{PlayerName} ({ClassName}) - Level {Level}: {GetScoreDisplay()} [{Category}]";
    }
}
