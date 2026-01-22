using RuneAndRust.Domain.Constants;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Tracks the state of an extended (multi-round) skill check.
/// </summary>
/// <remarks>
/// <para>
/// Extended checks represent complex tasks that require multiple successful attempts
/// to complete. Characters accumulate net successes over multiple rounds until
/// reaching a target threshold or running out of time.
/// </para>
/// <para>
/// Key mechanics:
/// <list type="bullet">
///   <item><description>Each round, character rolls and adds net successes to total</description></item>
///   <item><description>Fumbles subtract 2 accumulated successes (min 0)</description></item>
///   <item><description>3 consecutive fumbles = catastrophic failure</description></item>
///   <item><description>Success when accumulated ≥ target within max rounds</description></item>
/// </list>
/// </para>
/// </remarks>
public class ExtendedCheckState
{
    // ═══════════════════════════════════════════════════════════════════════════
    // IDENTITY PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Unique identifier for this extended check instance.
    /// </summary>
    public string CheckId { get; init; } = null!;

    /// <summary>
    /// ID of the character performing the check.
    /// </summary>
    public string CharacterId { get; init; } = null!;

    /// <summary>
    /// ID of the skill being used.
    /// </summary>
    public string SkillId { get; init; } = null!;

    // ═══════════════════════════════════════════════════════════════════════════
    // TARGET AND PROGRESS PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Number of accumulated successes required to complete the check.
    /// </summary>
    public int TargetSuccesses { get; init; }

    /// <summary>
    /// Current accumulated net successes.
    /// </summary>
    /// <remarks>
    /// Increases by net successes each round.
    /// Decreases by 2 on fumble (minimum 0).
    /// </remarks>
    public int AccumulatedSuccesses { get; private set; }

    /// <summary>
    /// Number of rounds remaining before failure.
    /// </summary>
    public int RoundsRemaining { get; private set; }

    /// <summary>
    /// Maximum rounds allowed for this check.
    /// </summary>
    public int MaxRounds { get; init; }

    /// <summary>
    /// Number of rounds completed.
    /// </summary>
    public int RoundsCompleted => MaxRounds - RoundsRemaining;

    // ═══════════════════════════════════════════════════════════════════════════
    // ROUND HISTORY AND STATUS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// History of dice roll results for each round.
    /// </summary>
    public List<DiceRollResult> RoundResults { get; } = new();

    /// <summary>
    /// Current status of the extended check.
    /// </summary>
    public ExtendedCheckStatus Status { get; private set; }

    /// <summary>
    /// Number of consecutive fumbles in a row.
    /// </summary>
    /// <remarks>
    /// Resets to 0 when a non-fumble roll occurs.
    /// Triggers catastrophic failure at 3.
    /// </remarks>
    public int ConsecutiveFumbles { get; private set; }

    /// <summary>
    /// Total number of fumbles throughout the check (not necessarily consecutive).
    /// </summary>
    public int TotalFumbles { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // TIMESTAMPS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Timestamp when the check was started.
    /// </summary>
    public DateTime StartedAt { get; init; }

    /// <summary>
    /// Timestamp when the check was completed (or null if in progress).
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the progress toward the target (0.0 to 1.0+).
    /// </summary>
    /// <remarks>
    /// Can exceed 1.0 if accumulated successes exceed the target.
    /// </remarks>
    public double Progress => TargetSuccesses > 0
        ? (double)AccumulatedSuccesses / TargetSuccesses
        : 0;

    /// <summary>
    /// Gets whether the check is still active (in progress).
    /// </summary>
    public bool IsActive => Status == ExtendedCheckStatus.InProgress;

    /// <summary>
    /// Gets whether the check has ended (any terminal state).
    /// </summary>
    public bool IsComplete => Status != ExtendedCheckStatus.InProgress;

    /// <summary>
    /// Gets whether one more fumble will trigger catastrophic failure.
    /// </summary>
    public bool IsAtRisk => ConsecutiveFumbles == ExtendedCheckConstants.CatastrophicFumbleThreshold - 1;

    /// <summary>
    /// Gets the number of successes still needed to complete the check.
    /// </summary>
    public int SuccessesNeeded => Math.Max(0, TargetSuccesses - AccumulatedSuccesses);

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for factory pattern.
    /// </summary>
    private ExtendedCheckState() { }

    /// <summary>
    /// Creates a new extended check state.
    /// </summary>
    /// <param name="checkId">Unique identifier for this check.</param>
    /// <param name="characterId">ID of the character performing the check.</param>
    /// <param name="skillId">ID of the skill being used.</param>
    /// <param name="targetSuccesses">Successes required to complete the check.</param>
    /// <param name="maxRounds">Maximum rounds allowed (default: 10).</param>
    /// <returns>A new ExtendedCheckState in InProgress status.</returns>
    /// <exception cref="ArgumentException">If identifiers are null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If targetSuccesses or maxRounds is invalid.</exception>
    public static ExtendedCheckState Create(
        string checkId,
        string characterId,
        string skillId,
        int targetSuccesses,
        int maxRounds = ExtendedCheckConstants.DefaultMaxRounds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(checkId, nameof(checkId));
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(skillId, nameof(skillId));

        if (targetSuccesses < ExtendedCheckConstants.MinimumTargetSuccesses)
            throw new ArgumentOutOfRangeException(
                nameof(targetSuccesses),
                $"Target successes must be at least {ExtendedCheckConstants.MinimumTargetSuccesses}");

        if (maxRounds < ExtendedCheckConstants.MinimumRounds)
            throw new ArgumentOutOfRangeException(
                nameof(maxRounds),
                $"Max rounds must be at least {ExtendedCheckConstants.MinimumRounds}");

        return new ExtendedCheckState
        {
            CheckId = checkId,
            CharacterId = characterId,
            SkillId = skillId,
            TargetSuccesses = targetSuccesses,
            MaxRounds = maxRounds,
            RoundsRemaining = maxRounds,
            AccumulatedSuccesses = 0,
            ConsecutiveFumbles = 0,
            TotalFumbles = 0,
            Status = ExtendedCheckStatus.InProgress,
            StartedAt = DateTime.UtcNow
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ROUND PROCESSING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Processes a round of the extended check with the given roll result.
    /// </summary>
    /// <param name="rollResult">The dice roll result for this round.</param>
    /// <exception cref="InvalidOperationException">If the check is not in progress.</exception>
    /// <remarks>
    /// <para>
    /// Processing order:
    /// <list type="bullet">
    ///   <item><description>Record the roll result</description></item>
    ///   <item><description>Decrement rounds remaining</description></item>
    ///   <item><description>Handle fumble (penalty + consecutive tracking)</description></item>
    ///   <item><description>Or accumulate net successes</description></item>
    ///   <item><description>Check for completion (success, failure, catastrophic)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public void ProcessRound(DiceRollResult rollResult)
    {
        if (Status != ExtendedCheckStatus.InProgress)
            throw new InvalidOperationException(
                $"Cannot process round when check is {Status}");

        // Record the result
        RoundResults.Add(rollResult);
        RoundsRemaining--;

        if (rollResult.IsFumble)
        {
            ProcessFumble();
        }
        else
        {
            ProcessSuccess(rollResult.NetSuccesses);
        }

        // Check completion conditions (if not already set by ProcessFumble)
        if (Status == ExtendedCheckStatus.InProgress)
        {
            CheckCompletion();
        }
    }

    /// <summary>
    /// Handles fumble processing: penalty and consecutive tracking.
    /// </summary>
    private void ProcessFumble()
    {
        ConsecutiveFumbles++;
        TotalFumbles++;

        // Apply fumble penalty
        AccumulatedSuccesses = Math.Max(0,
            AccumulatedSuccesses - ExtendedCheckConstants.FumblePenalty);

        // Check for catastrophic failure
        if (ConsecutiveFumbles >= ExtendedCheckConstants.CatastrophicFumbleThreshold)
        {
            Status = ExtendedCheckStatus.CatastrophicFailure;
            CompletedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Handles successful (non-fumble) roll processing.
    /// </summary>
    /// <param name="netSuccesses">Net successes from the roll.</param>
    private void ProcessSuccess(int netSuccesses)
    {
        // Reset consecutive fumbles
        ConsecutiveFumbles = 0;

        // Add net successes to accumulated total
        AccumulatedSuccesses += netSuccesses;
    }

    /// <summary>
    /// Checks if the check has reached a completion state.
    /// </summary>
    private void CheckCompletion()
    {
        if (AccumulatedSuccesses >= TargetSuccesses)
        {
            Status = ExtendedCheckStatus.Succeeded;
            CompletedAt = DateTime.UtcNow;
        }
        else if (RoundsRemaining <= 0)
        {
            Status = ExtendedCheckStatus.Failed;
            CompletedAt = DateTime.UtcNow;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STATE CHANGES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Abandons the extended check.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the check is not in progress.</exception>
    public void Abandon()
    {
        if (Status != ExtendedCheckStatus.InProgress)
            throw new InvalidOperationException(
                $"Cannot abandon check when status is {Status}");

        Status = ExtendedCheckStatus.Abandoned;
        CompletedAt = DateTime.UtcNow;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a summary string of the current state.
    /// </summary>
    /// <example>
    /// "ExtendedCheck[abc123]: 4/6 successes, 2/5 rounds remaining, Status: InProgress"
    /// "ExtendedCheck[def456]: 2/8 successes, 0/6 rounds remaining, Status: Failed"
    /// "ExtendedCheck[ghi789]: 0/6 successes, 3/5 rounds remaining, Status: CatastrophicFailure [AT RISK!]"
    /// </example>
    public override string ToString()
    {
        var riskWarning = IsAtRisk ? " [AT RISK!]" : "";
        return $"ExtendedCheck[{CheckId}]: {AccumulatedSuccesses}/{TargetSuccesses} successes, " +
               $"{RoundsRemaining}/{MaxRounds} rounds remaining, " +
               $"Status: {Status}{riskWarning}";
    }
}
