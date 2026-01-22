using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Records;

/// <summary>
/// Represents a complete log entry for a dice roll with full metadata.
/// </summary>
/// <remarks>
/// <para>
/// Immutable record capturing all details of a dice roll for history,
/// debugging, replay, and analytics purposes.
/// </para>
/// <para>
/// Key Features:
/// <list type="bullet">
///   <item><description>Unique RollId for identification</description></item>
///   <item><description>Timestamp for ordering and time-based queries</description></item>
///   <item><description>Seed for deterministic replay</description></item>
///   <item><description>Context string for categorized filtering</description></item>
///   <item><description>Actor/Target IDs for attribution</description></item>
///   <item><description>Fumble/Critical flags for special outcome tracking</description></item>
/// </list>
/// </para>
/// </remarks>
public record DiceRollLog
{
    /// <summary>
    /// Unique identifier for this roll.
    /// </summary>
    /// <remarks>
    /// Generated at log creation time. Used to reference specific rolls.
    /// </remarks>
    public Guid RollId { get; init; }

    /// <summary>
    /// UTC timestamp when the roll was made.
    /// </summary>
    /// <remarks>
    /// Used for chronological ordering and time-based queries.
    /// </remarks>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// RNG seed used for this roll.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Combined with PoolSize, allows deterministic reproduction of the roll.
    /// </para>
    /// <para>
    /// For debugging and replay functionality.
    /// </para>
    /// </remarks>
    public int Seed { get; init; }

    /// <summary>
    /// Number of dice in the pool.
    /// </summary>
    /// <remarks>
    /// Combined with Seed, allows deterministic reproduction.
    /// </remarks>
    public int PoolSize { get; init; }

    /// <summary>
    /// Individual die results.
    /// </summary>
    /// <remarks>
    /// Ordered list of each die's value in the roll.
    /// Includes explosion dice if any occurred.
    /// </remarks>
    public IReadOnlyList<int> Results { get; init; } = Array.Empty<int>();

    /// <summary>
    /// Net successes (successes - botches, minimum 0).
    /// </summary>
    /// <remarks>
    /// The primary success-counting metric for skill checks.
    /// </remarks>
    public int NetSuccesses { get; init; }

    /// <summary>
    /// Context string identifying the type of roll.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses hierarchical format with colon separator:
    /// <list type="bullet">
    ///   <item><description>"Combat:Attack" - Attack roll</description></item>
    ///   <item><description>"Skill:Acrobatics" - Skill check</description></item>
    ///   <item><description>"Dialogue:Persuasion" - Social roll</description></item>
    ///   <item><description>"Crafting:Forge" - Crafting roll</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Use <see cref="Constants.RollContexts"/> constants for standard contexts.
    /// </para>
    /// </remarks>
    public string Context { get; init; } = string.Empty;

    /// <summary>
    /// ID of the actor making the roll (optional).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Typically the player or NPC character ID.
    /// </para>
    /// <para>
    /// Null for rolls not associated with a specific actor (e.g., random events).
    /// </para>
    /// </remarks>
    public Guid? ActorId { get; init; }

    /// <summary>
    /// ID of the target of the roll (optional).
    /// </summary>
    /// <remarks>
    /// <para>
    /// For attack rolls, the target being attacked.
    /// For contested checks, the opposing party.
    /// </para>
    /// <para>
    /// Null for self-targeted or non-targeted rolls.
    /// </para>
    /// </remarks>
    public Guid? TargetId { get; init; }

    /// <summary>
    /// Whether the roll was a fumble.
    /// </summary>
    /// <remarks>
    /// True when 0 successes AND at least 1 botch.
    /// </remarks>
    public bool IsFumble { get; init; }

    /// <summary>
    /// Whether the roll was a critical success.
    /// </summary>
    /// <remarks>
    /// True when net successes >= 5.
    /// </remarks>
    public bool IsCriticalSuccess { get; init; }

    /// <summary>
    /// Total successes before botch subtraction.
    /// </summary>
    /// <remarks>
    /// Count of dice showing 8, 9, or 10.
    /// </remarks>
    public int TotalSuccesses { get; init; }

    /// <summary>
    /// Total botches.
    /// </summary>
    /// <remarks>
    /// Count of dice showing 1.
    /// </remarks>
    public int TotalBotches { get; init; }

    /// <summary>
    /// Raw sum of all dice values.
    /// </summary>
    /// <remarks>
    /// Preserved for damage rolls and sum-based calculations.
    /// </remarks>
    public int RawTotal { get; init; }

    /// <summary>
    /// Creates a log entry from a DiceRollResult and context.
    /// </summary>
    /// <param name="rollResult">The dice roll result to log.</param>
    /// <param name="seed">The RNG seed used for the roll.</param>
    /// <param name="context">The roll context string.</param>
    /// <param name="actorId">Optional actor ID.</param>
    /// <param name="targetId">Optional target ID.</param>
    /// <returns>A new DiceRollLog entry.</returns>
    public static DiceRollLog FromRollResult(
        DiceRollResult rollResult,
        int seed,
        string context,
        Guid? actorId = null,
        Guid? targetId = null)
    {
        return new DiceRollLog
        {
            RollId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Seed = seed,
            PoolSize = rollResult.Pool.Count,
            Results = rollResult.Rolls.Concat(rollResult.ExplosionRolls).ToList().AsReadOnly(),
            NetSuccesses = rollResult.NetSuccesses,
            TotalSuccesses = rollResult.TotalSuccesses,
            TotalBotches = rollResult.TotalBotches,
            RawTotal = rollResult.RawTotal,
            Context = context,
            ActorId = actorId,
            TargetId = targetId,
            IsFumble = rollResult.IsFumble,
            IsCriticalSuccess = rollResult.IsCriticalSuccess
        };
    }

    /// <summary>
    /// Returns a formatted string showing the roll summary.
    /// </summary>
    /// <example>
    /// "[Skill:Acrobatics] 5d10: [8,9,1,4,3] → 1 net (2S-1B)"
    /// "[Combat:Attack] 4d10: [10,8,8,5] → 3 net [CRITICAL!]"
    /// </example>
    public override string ToString()
    {
        var resultsStr = $"[{string.Join(",", Results)}]";
        var outcomeStr = $"{NetSuccesses} net ({TotalSuccesses}S-{TotalBotches}B)";

        var result = $"[{Context}] {PoolSize}d10: {resultsStr} → {outcomeStr}";

        if (IsFumble)
            result += " [FUMBLE!]";
        else if (IsCriticalSuccess)
            result += " [CRITICAL!]";

        return result;
    }

    /// <summary>
    /// Returns a detailed multi-line string for debugging.
    /// </summary>
    public string ToDetailedString()
    {
        return $"""
            Roll ID: {RollId}
            Timestamp: {Timestamp:O}
            Context: {Context}
            Pool Size: {PoolSize}d10
            Seed: {Seed}
            Results: [{string.Join(", ", Results)}]
            Total Successes: {TotalSuccesses}
            Total Botches: {TotalBotches}
            Net Successes: {NetSuccesses}
            Raw Total: {RawTotal}
            Is Fumble: {IsFumble}
            Is Critical: {IsCriticalSuccess}
            Actor ID: {ActorId?.ToString() ?? "None"}
            Target ID: {TargetId?.ToString() ?? "None"}
            """;
    }
}
