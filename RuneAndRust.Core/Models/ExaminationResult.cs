namespace RuneAndRust.Core.Models;

/// <summary>
/// Represents the result of a WITS-based examination check.
/// Contains dice roll data and the revealed description text.
/// </summary>
/// <param name="Success">Whether the examination found any information beyond the base description.</param>
/// <param name="NetSuccesses">The net result of the dice roll (successes - botches).</param>
/// <param name="TierRevealed">The highest tier revealed: 0 = base only, 1 = detailed, 2 = expert.</param>
/// <param name="Description">The combined description text based on revealed tiers.</param>
/// <param name="NewInfoRevealed">Whether this examination revealed information not previously seen.</param>
/// <param name="Rolls">The individual dice results for display purposes.</param>
public record ExaminationResult(
    bool Success,
    int NetSuccesses,
    int TierRevealed,
    string Description,
    bool NewInfoRevealed,
    IReadOnlyList<int> Rolls
)
{
    /// <summary>
    /// Gets whether the examination was a fumble (zero successes with botches).
    /// </summary>
    public bool IsFumble => NetSuccesses < 0;

    /// <summary>
    /// Gets whether the expert tier was revealed.
    /// </summary>
    public bool RevealedExpert => TierRevealed >= 2;

    /// <summary>
    /// Gets whether the detailed tier was revealed.
    /// </summary>
    public bool RevealedDetailed => TierRevealed >= 1;

    /// <summary>
    /// Creates a result for when the target object was not found.
    /// </summary>
    /// <param name="targetName">The name of the object that wasn't found.</param>
    /// <returns>An ExaminationResult indicating failure.</returns>
    public static ExaminationResult NotFound(string targetName) => new(
        Success: false,
        NetSuccesses: 0,
        TierRevealed: 0,
        Description: $"You don't see anything called '{targetName}' here.",
        NewInfoRevealed: false,
        Rolls: Array.Empty<int>()
    );
}
