using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// An NPC that poses riddles to players.
/// </summary>
/// <remarks>
/// <para>
/// Riddle NPCs block passage until their riddle is solved.
/// Features include:
/// </para>
/// <list type="bullet">
///   <item><description>Wrong answer tracking with max attempts</description></item>
///   <item><description>Configurable failure consequences</description></item>
///   <item><description>Direction-based passage blocking</description></item>
///   <item><description>Reward granting on success</description></item>
/// </list>
/// </remarks>
public class RiddleNpc : IEntity
{
    // ===== Identity Properties =====

    /// <summary>
    /// Gets the unique identifier for this NPC.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the display name of this NPC.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the description of this NPC.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    // ===== Riddle State Properties =====

    /// <summary>
    /// Gets the current riddle definition ID.
    /// </summary>
    public string CurrentRiddleId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets whether the current riddle has been solved.
    /// </summary>
    public bool RiddleSolved { get; private set; }

    /// <summary>
    /// Gets the number of wrong answers given.
    /// </summary>
    public int WrongAnswerCount { get; private set; }

    /// <summary>
    /// Gets the maximum wrong answers before consequences (-1 = unlimited).
    /// </summary>
    public int MaxWrongAnswers { get; private set; } = -1;

    // ===== Consequences & Rewards =====

    /// <summary>
    /// Gets the reward given for solving the riddle.
    /// </summary>
    public string? RewardId { get; private set; }

    /// <summary>
    /// Gets the consequence for too many wrong answers.
    /// </summary>
    public RiddleConsequence? FailureConsequence { get; private set; }

    // ===== Message Properties =====

    /// <summary>
    /// Gets the greeting message when first speaking.
    /// </summary>
    public string GreetingMessage { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the message when riddle is already solved.
    /// </summary>
    public string SolvedMessage { get; private set; } = string.Empty;

    // ===== Passage Blocking =====

    /// <summary>
    /// Gets whether this NPC blocks passage until riddle is solved.
    /// </summary>
    public bool BlocksPassage { get; private set; }

    /// <summary>
    /// Gets the direction blocked (if blocking passage).
    /// </summary>
    public Direction? BlockedDirection { get; private set; }

    // ===== Computed Properties =====

    /// <summary>
    /// Gets whether max failures has been reached.
    /// </summary>
    public bool HasReachedMaxFailures =>
        MaxWrongAnswers > 0 && WrongAnswerCount >= MaxWrongAnswers;

    /// <summary>
    /// Gets whether this NPC has a reward.
    /// </summary>
    public bool HasReward => !string.IsNullOrEmpty(RewardId);

    /// <summary>
    /// Gets whether there are attempts remaining.
    /// </summary>
    public bool HasAttemptsRemaining =>
        MaxWrongAnswers < 0 || WrongAnswerCount < MaxWrongAnswers;

    // ===== Constructors =====

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private RiddleNpc() { }

    // ===== Factory Methods =====

    /// <summary>
    /// Creates a riddle NPC.
    /// </summary>
    /// <param name="name">Display name of the NPC.</param>
    /// <param name="description">Description of the NPC.</param>
    /// <param name="riddleId">ID of the riddle this NPC poses.</param>
    /// <param name="greetingMessage">Message when first spoken to.</param>
    /// <param name="solvedMessage">Message when riddle already solved.</param>
    /// <param name="maxWrongAnswers">Max wrong attempts (-1 = unlimited).</param>
    /// <param name="rewardId">Loot table ID for reward.</param>
    /// <param name="failureConsequence">What happens on max failures.</param>
    /// <param name="blocksPassage">Whether NPC blocks a direction.</param>
    /// <param name="blockedDirection">Which direction is blocked.</param>
    /// <returns>A new RiddleNpc instance.</returns>
    public static RiddleNpc Create(
        string name,
        string description,
        string riddleId,
        string greetingMessage,
        string solvedMessage,
        int maxWrongAnswers = -1,
        string? rewardId = null,
        RiddleConsequence? failureConsequence = null,
        bool blocksPassage = false,
        Direction? blockedDirection = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(riddleId);

        return new RiddleNpc
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description ?? string.Empty,
            CurrentRiddleId = riddleId,
            GreetingMessage = greetingMessage ?? string.Empty,
            SolvedMessage = solvedMessage ?? string.Empty,
            MaxWrongAnswers = maxWrongAnswers,
            RewardId = rewardId,
            FailureConsequence = failureConsequence,
            BlocksPassage = blocksPassage,
            BlockedDirection = blockedDirection
        };
    }

    // ===== State Methods =====

    /// <summary>
    /// Records a wrong answer attempt.
    /// </summary>
    /// <returns>True if max wrong answers reached.</returns>
    public bool RecordWrongAnswer()
    {
        WrongAnswerCount++;
        return MaxWrongAnswers > 0 && WrongAnswerCount >= MaxWrongAnswers;
    }

    /// <summary>
    /// Marks the riddle as solved.
    /// </summary>
    public void MarkSolved()
    {
        RiddleSolved = true;
    }

    /// <summary>
    /// Resets the NPC state (for Reset consequence or retry).
    /// </summary>
    public void Reset()
    {
        WrongAnswerCount = 0;
        RiddleSolved = false;
    }

    // ===== Passage Methods =====

    /// <summary>
    /// Checks if passage in a direction is currently blocked.
    /// </summary>
    /// <param name="direction">The direction to check.</param>
    /// <returns>True if the NPC blocks this direction and riddle is unsolved.</returns>
    public bool IsPassageBlocked(Direction direction)
    {
        return BlocksPassage && !RiddleSolved && BlockedDirection == direction;
    }

    /// <summary>
    /// Gets the remaining attempts before failure (-1 if unlimited).
    /// </summary>
    /// <returns>Remaining attempts or -1.</returns>
    public int GetRemainingAttempts()
    {
        if (MaxWrongAnswers <= 0) return -1;
        return Math.Max(0, MaxWrongAnswers - WrongAnswerCount);
    }

    /// <summary>
    /// Returns a string representation of this NPC.
    /// </summary>
    public override string ToString() =>
        $"RiddleNpc({Name}, Riddle={CurrentRiddleId}, Solved={RiddleSolved})";
}
