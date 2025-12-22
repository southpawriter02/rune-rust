namespace RuneAndRust.Core.ViewModels;

/// <summary>
/// Represents a single entry in the initiative timeline (v0.3.6b).
/// Shows projected turn order for current and upcoming rounds.
/// </summary>
public record TimelineEntryView(
    /// <summary>
    /// The unique identifier of the combatant.
    /// </summary>
    Guid CombatantId,

    /// <summary>
    /// The display name of the combatant.
    /// </summary>
    string Name,

    /// <summary>
    /// Whether this combatant is a player character.
    /// </summary>
    bool IsPlayer,

    /// <summary>
    /// Whether this combatant is currently taking their turn.
    /// </summary>
    bool IsActive,

    /// <summary>
    /// The combatant's initiative value for sorting.
    /// </summary>
    int Initiative,

    /// <summary>
    /// The round number this turn occurs in.
    /// Used to show round transitions in the timeline.
    /// </summary>
    int RoundNumber,

    /// <summary>
    /// Health status indicator: "healthy", "wounded", "critical", or "dead".
    /// </summary>
    string HealthIndicator
);
