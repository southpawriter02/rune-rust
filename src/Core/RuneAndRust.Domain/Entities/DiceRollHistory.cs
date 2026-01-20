// ═══════════════════════════════════════════════════════════════════════════════
// DiceRollHistory.cs
// Entity tracking dice roll history and statistics for a player.
// Version: 0.12.0b
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.Records;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Tracks dice roll history and statistics for a player.
/// </summary>
/// <remarks>
/// <para>This entity maintains comprehensive dice roll statistics including:</para>
/// <list type="bullet">
///   <item><description>Total roll counts and sums for average calculation</description></item>
///   <item><description>Natural 20 and natural 1 counts for critical statistics</description></item>
///   <item><description>Current and longest streak records for luck tracking</description></item>
///   <item><description>A buffer of recent rolls for display in the statistics view</description></item>
/// </list>
/// <para>
/// Streak tracking uses a threshold of 11+ for "above average" on d20 rolls,
/// based on the expected average of 10.5. A lucky streak is consecutive rolls >= 11,
/// while an unlucky streak is consecutive rolls &lt; 11.
/// </para>
/// </remarks>
public class DiceRollHistory : IEntity
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Constants
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The maximum number of recent rolls to retain in the buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The recent rolls buffer maintains the last 20 rolls for display in the
    /// statistics view. When the buffer is full, the oldest roll is removed
    /// when a new roll is recorded.
    /// </para>
    /// </remarks>
    public const int MaxRecentRolls = 20;

    /// <summary>
    /// The expected average for a d20 roll.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The expected average of a fair d20 is (1 + 20) / 2 = 10.5.
    /// This is used to calculate the player's "luck percentage" by comparing
    /// their actual average to the expected average.
    /// </para>
    /// </remarks>
    public const double D20ExpectedAverage = 10.5;

    /// <summary>
    /// The threshold for considering a d20 roll "above average" for streak tracking.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Rolls >= 11 are considered above average (lucky), while rolls &lt; 11
    /// are considered below average (unlucky). This threshold is used to
    /// determine streak continuations and resets.
    /// </para>
    /// </remarks>
    public const int AboveAverageThreshold = 11;

    // ═══════════════════════════════════════════════════════════════════════════
    // Identity Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this history record.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the player ID this history belongs to.
    /// </summary>
    public Guid PlayerId { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Roll Count Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the total number of dice rolls recorded (all dice types).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This counter includes all dice rolls, not just d20 rolls. It provides
    /// an overall measure of player activity.
    /// </para>
    /// </remarks>
    public int TotalRolls { get; private set; }

    /// <summary>
    /// Gets the total number of natural 20s rolled on d20s.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Natural 20s are critical successes in most d20-based systems.
    /// This counter only tracks d20 rolls that resulted in a 20.
    /// </para>
    /// </remarks>
    public int TotalNaturalTwenties { get; private set; }

    /// <summary>
    /// Gets the total number of natural 1s rolled on d20s.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Natural 1s are critical failures in most d20-based systems.
    /// This counter only tracks d20 rolls that resulted in a 1.
    /// </para>
    /// </remarks>
    public int TotalNaturalOnes { get; private set; }

    /// <summary>
    /// Gets the number of d20 rolls (for average calculation).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This counter tracks only d20 rolls, which are used for calculating
    /// the player's luck rating and average roll statistics.
    /// </para>
    /// </remarks>
    public int D20RollCount { get; private set; }

    /// <summary>
    /// Gets the sum of all d20 roll results (for average calculation).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This sum is used with D20RollCount to calculate the player's
    /// average d20 roll. Using a long type prevents overflow for players
    /// with extensive roll histories.
    /// </para>
    /// </remarks>
    public long D20RollSum { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Streak Tracking Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current streak count.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The current streak tracks consecutive above-average or below-average d20 rolls:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Positive values indicate lucky streaks (consecutive rolls >= 11)</description></item>
    ///   <item><description>Negative values indicate unlucky streaks (consecutive rolls &lt; 11)</description></item>
    /// </list>
    /// <para>
    /// The streak resets when the roll category changes (lucky to unlucky or vice versa).
    /// </para>
    /// </remarks>
    public int CurrentStreak { get; private set; }

    /// <summary>
    /// Gets the longest lucky streak recorded.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This tracks the highest value CurrentStreak has ever reached.
    /// A lucky streak is consecutive d20 rolls of 11 or higher.
    /// </para>
    /// </remarks>
    public int LongestLuckyStreak { get; private set; }

    /// <summary>
    /// Gets the longest unlucky streak recorded.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This tracks the lowest (most negative) value CurrentStreak has ever reached,
    /// stored as a positive number. An unlucky streak is consecutive d20 rolls
    /// below 11.
    /// </para>
    /// </remarks>
    public int LongestUnluckyStreak { get; private set; }

    /// <summary>
    /// Gets the longest consecutive natural 20 streak.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This tracks the longest run of consecutive natural 20s.
    /// This is a rare achievement that players enjoy tracking.
    /// </para>
    /// </remarks>
    public int LongestNat20Streak { get; private set; }

    /// <summary>
    /// The current consecutive natural 20 count (for streak tracking).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This private field tracks the ongoing natural 20 streak.
    /// It is reset to 0 whenever a non-natural-20 d20 roll is made.
    /// </para>
    /// </remarks>
    private int _currentNat20Streak;

    // ═══════════════════════════════════════════════════════════════════════════
    // Recent Rolls Buffer
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The queue of recent rolls, limited to MaxRecentRolls entries.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This queue operates as a FIFO buffer. When the buffer exceeds
    /// MaxRecentRolls, the oldest entries are removed.
    /// </para>
    /// </remarks>
    private readonly Queue<DiceRollRecord> _recentRolls = new();

    /// <summary>
    /// Gets the recent rolls as a read-only list.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns a snapshot of the recent rolls buffer. The list is ordered
    /// from oldest to newest (FIFO order).
    /// </para>
    /// </remarks>
    public IReadOnlyList<DiceRollRecord> RecentRolls => _recentRolls.ToList();

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for EF Core and factory method.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use the <see cref="Create"/> factory method to create new instances.
    /// </para>
    /// </remarks>
    private DiceRollHistory()
    {
    }

    /// <summary>
    /// Creates a new DiceRollHistory for a player.
    /// </summary>
    /// <param name="playerId">The ID of the player this history belongs to.</param>
    /// <returns>A new DiceRollHistory instance with default values.</returns>
    /// <example>
    /// <code>
    /// var history = DiceRollHistory.Create(player.Id);
    /// player.InitializeDiceHistory(history);
    /// </code>
    /// </example>
    public static DiceRollHistory Create(Guid playerId)
    {
        return new DiceRollHistory
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Roll Recording Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Records a dice roll to the history.
    /// </summary>
    /// <param name="roll">The roll record to add.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="roll"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method updates all relevant statistics based on the roll:
    /// </para>
    /// <list type="number">
    ///   <item><description>Increments TotalRolls</description></item>
    ///   <item><description>For d20 rolls: updates D20 counts, sums, natural 20/1 counts, and streaks</description></item>
    ///   <item><description>Adds the roll to the recent rolls buffer</description></item>
    /// </list>
    /// </remarks>
    public void RecordRoll(DiceRollRecord roll)
    {
        ArgumentNullException.ThrowIfNull(roll, nameof(roll));

        TotalRolls++;

        // Track d20 rolls specifically for averages and streaks
        if (IsD20Roll(roll.DiceExpression))
        {
            RecordD20Roll(roll);
        }

        // Maintain recent rolls buffer
        AddToRecentRolls(roll);
    }

    /// <summary>
    /// Records a d20 roll, updating all d20-specific statistics.
    /// </summary>
    /// <param name="roll">The d20 roll record.</param>
    private void RecordD20Roll(DiceRollRecord roll)
    {
        D20RollCount++;
        D20RollSum += roll.Result;

        // Natural 20 tracking
        if (roll.HasNatural20)
        {
            TotalNaturalTwenties++;
            _currentNat20Streak++;
            LongestNat20Streak = Math.Max(LongestNat20Streak, _currentNat20Streak);
        }
        else
        {
            _currentNat20Streak = 0;
        }

        // Natural 1 tracking
        if (roll.HasNatural1)
        {
            TotalNaturalOnes++;
        }

        // Streak tracking based on above/below average threshold
        UpdateStreak(roll.Result >= AboveAverageThreshold);
    }

    /// <summary>
    /// Updates the current streak based on whether the roll was above average.
    /// </summary>
    /// <param name="wasAboveAverage">True if the roll was >= 11.</param>
    /// <remarks>
    /// <para>
    /// Streak logic:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>If roll is above average and current streak is positive or zero: increment streak</description></item>
    ///   <item><description>If roll is above average and current streak is negative: reset to +1</description></item>
    ///   <item><description>If roll is below average and current streak is negative or zero: decrement streak</description></item>
    ///   <item><description>If roll is below average and current streak is positive: reset to -1</description></item>
    /// </list>
    /// </remarks>
    private void UpdateStreak(bool wasAboveAverage)
    {
        if (wasAboveAverage)
        {
            // Continue or start lucky streak
            CurrentStreak = CurrentStreak >= 0 ? CurrentStreak + 1 : 1;
            LongestLuckyStreak = Math.Max(LongestLuckyStreak, CurrentStreak);
        }
        else
        {
            // Continue or start unlucky streak
            CurrentStreak = CurrentStreak <= 0 ? CurrentStreak - 1 : -1;
            LongestUnluckyStreak = Math.Max(LongestUnluckyStreak, Math.Abs(CurrentStreak));
        }
    }

    /// <summary>
    /// Adds a roll to the recent rolls buffer, removing old entries if needed.
    /// </summary>
    /// <param name="roll">The roll to add.</param>
    private void AddToRecentRolls(DiceRollRecord roll)
    {
        _recentRolls.Enqueue(roll);
        while (_recentRolls.Count > MaxRecentRolls)
        {
            _recentRolls.Dequeue();
        }
    }

    /// <summary>
    /// Determines if a dice expression is a d20 roll.
    /// </summary>
    /// <param name="expression">The dice expression to check.</param>
    /// <returns>True if the expression represents a d20 roll.</returns>
    /// <remarks>
    /// <para>
    /// Matches expressions like "1d20", "1d20+5", "d20", "D20", etc.
    /// This is used to filter which rolls affect d20-specific statistics.
    /// </para>
    /// </remarks>
    private static bool IsD20Roll(string expression)
    {
        // Match expressions like "1d20", "1d20+5", "d20", etc.
        return expression.StartsWith("1d20", StringComparison.OrdinalIgnoreCase) ||
               expression.StartsWith("d20", StringComparison.OrdinalIgnoreCase);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Statistics Calculation Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the average d20 roll result.
    /// </summary>
    /// <returns>
    /// The actual average d20 roll, or the expected average (10.5) if no d20 rolls recorded.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Returns the expected average when no rolls have been recorded to avoid
    /// displaying misleading statistics for new players.
    /// </para>
    /// </remarks>
    public double GetAverageD20Roll()
    {
        return D20RollCount > 0
            ? (double)D20RollSum / D20RollCount
            : D20ExpectedAverage;
    }

    /// <summary>
    /// Gets the natural 20 rate as a decimal (0.0 to 1.0).
    /// </summary>
    /// <returns>
    /// The actual natural 20 rate, or the expected rate (0.05) if no d20 rolls recorded.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The expected natural 20 rate for a fair d20 is 1/20 = 0.05 (5%).
    /// Returns this expected rate when no rolls have been recorded.
    /// </para>
    /// </remarks>
    public double GetNat20Rate()
    {
        return D20RollCount > 0
            ? (double)TotalNaturalTwenties / D20RollCount
            : 0.05;
    }

    /// <summary>
    /// Gets the natural 1 rate as a decimal (0.0 to 1.0).
    /// </summary>
    /// <returns>
    /// The actual natural 1 rate, or the expected rate (0.05) if no d20 rolls recorded.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The expected natural 1 rate for a fair d20 is 1/20 = 0.05 (5%).
    /// Returns this expected rate when no rolls have been recorded.
    /// </para>
    /// </remarks>
    public double GetNat1Rate()
    {
        return D20RollCount > 0
            ? (double)TotalNaturalOnes / D20RollCount
            : 0.05;
    }

    /// <summary>
    /// Gets the luck percentage, showing deviation from expected average.
    /// </summary>
    /// <returns>
    /// The percentage above or below expected average.
    /// Positive values indicate "lucky", negative values indicate "unlucky".
    /// Returns 0 if no d20 rolls have been recorded.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The formula is: ((ActualAverage - ExpectedAverage) / ExpectedAverage) × 100
    /// </para>
    /// <para>
    /// Examples:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Average of 11.55 → +10% (lucky)</description></item>
    ///   <item><description>Average of 10.5 → 0% (perfectly average)</description></item>
    ///   <item><description>Average of 9.45 → -10% (unlucky)</description></item>
    /// </list>
    /// </remarks>
    public double GetLuckPercentage()
    {
        if (D20RollCount == 0)
        {
            return 0;
        }

        var average = GetAverageD20Roll();
        return ((average - D20ExpectedAverage) / D20ExpectedAverage) * 100;
    }
}
