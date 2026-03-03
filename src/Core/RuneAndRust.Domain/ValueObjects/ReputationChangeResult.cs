using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Result of applying a reputation change to a player for a single faction.
/// </summary>
/// <remarks>
/// <para>Returned by <c>IReputationService.ApplyReputationChanges()</c> — one result
/// per affected faction. Contains enough information for the UI to display
/// reputation change notifications and tier transition messages.</para>
///
/// <para>Example usage in a game session:</para>
/// <code>
/// var results = reputationService.ApplyReputationChanges(player, quest.ReputationChanges, WitnessContext.Direct);
/// foreach (var result in results)
/// {
///     session.AddMessage(result.Message);                    // "+25 Iron-Bane Reputation"
///     if (result.TierChanged)
///         session.AddMessage(result.TierTransitionMessage!); // "Your standing with Iron-Banes is now Friendly!"
/// }
/// </code>
/// </remarks>
public record ReputationChangeResult
{
    /// <summary>
    /// Gets the faction ID that was affected.
    /// </summary>
    public string FactionId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name of the affected faction (e.g., "Iron-Banes").
    /// </summary>
    public string FactionName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the raw reputation delta before witness multiplier was applied.
    /// </summary>
    /// <remarks>
    /// This is the delta as specified in the quest reward or game action.
    /// The <see cref="ActualDelta"/> may differ due to witness scaling and clamping.
    /// </remarks>
    public int RawDelta { get; init; }

    /// <summary>
    /// Gets the actual reputation delta applied after witness multiplier and value clamping.
    /// </summary>
    /// <remarks>
    /// <para>Calculated as: <c>(int)(RawDelta * WitnessMultiplier)</c>, then clamped so
    /// the resulting value stays within [-100, +100].</para>
    /// <para>May be 0 if the action was Unwitnessed or if the reputation was already
    /// at a boundary and the delta would push it further.</para>
    /// </remarks>
    public int ActualDelta { get; init; }

    /// <summary>
    /// Gets the new reputation value after the change was applied.
    /// </summary>
    public int NewValue { get; init; }

    /// <summary>
    /// Gets the reputation tier before the change was applied.
    /// </summary>
    public ReputationTier OldTier { get; init; }

    /// <summary>
    /// Gets the reputation tier after the change was applied.
    /// </summary>
    public ReputationTier NewTier { get; init; }

    /// <summary>
    /// Gets whether a tier transition occurred as a result of this change.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="OldTier"/> differs from <see cref="NewTier"/>;
    /// <c>false</c> otherwise.
    /// </value>
    public bool TierChanged => OldTier != NewTier;

    /// <summary>
    /// Gets the human-readable message for the player describing the change.
    /// </summary>
    /// <example>"+25 Iron-Bane Reputation" or "-10 God-Sleeper Cultist Reputation".</example>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional tier transition message, present only when <see cref="TierChanged"/> is <c>true</c>.
    /// </summary>
    /// <example>"Your standing with Iron-Banes is now Friendly!"</example>
    public string? TierTransitionMessage { get; init; }
}
