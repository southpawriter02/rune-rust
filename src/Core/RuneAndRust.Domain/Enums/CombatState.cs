namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the current state of a combat encounter.
/// </summary>
/// <remarks>
/// <para>Combat state follows a simple state machine:</para>
/// <code>
/// NotStarted -> Active -> Victory | PlayerDefeated | Fled
/// </code>
/// <para>Transitions are managed by <see cref="Entities.CombatEncounter"/>.</para>
/// </remarks>
public enum CombatState
{
    /// <summary>
    /// Combat has not yet started (setup phase).
    /// Combatants can be added during this state.
    /// </summary>
    NotStarted,

    /// <summary>
    /// Combat is actively in progress.
    /// Turns are being processed in initiative order.
    /// </summary>
    Active,

    /// <summary>
    /// Combat ended with player victory.
    /// All monsters have been defeated.
    /// </summary>
    Victory,

    /// <summary>
    /// Combat ended with player defeat.
    /// The player's health reached zero.
    /// </summary>
    PlayerDefeated,

    /// <summary>
    /// Combat ended because player fled.
    /// Player successfully escaped to previous room.
    /// </summary>
    Fled
}
