using System.Text.Json.Serialization;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Holds the mutable state of the current game session.
/// Lives as a Singleton in the DI container to persist across the game loop.
/// </summary>
public class GameState
{
    /// <summary>
    /// Gets or sets the current phase of the game.
    /// </summary>
    public GamePhase Phase { get; set; } = GamePhase.MainMenu;

    /// <summary>
    /// Gets or sets the current player character.
    /// Will be populated when a game session starts.
    /// </summary>
    public Entities.Character? CurrentCharacter { get; set; }

    /// <summary>
    /// Gets or sets the current turn count within a session.
    /// </summary>
    public int TurnCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether a game session is currently active.
    /// </summary>
    public bool IsSessionActive { get; set; } = false;

    /// <summary>
    /// Gets or sets the pending asynchronous action requested by the player.
    /// Used to signal save/load operations from the synchronous command parser.
    /// Not serialized to save files.
    /// </summary>
    [JsonIgnore]
    public PendingGameAction PendingAction { get; set; } = PendingGameAction.None;

    /// <summary>
    /// Gets or sets the ID of the room the player is currently in.
    /// Null when no game session is active.
    /// </summary>
    public Guid? CurrentRoomId { get; set; }

    /// <summary>
    /// Gets or sets the current combat state.
    /// Null when not in combat.
    /// </summary>
    [JsonIgnore]
    public CombatState? CombatState { get; set; }

    /// <summary>
    /// Gets or sets a pending encounter from ambush or other trigger.
    /// Consumed by GameService on Combat phase entry.
    /// Not serialized to save files.
    /// </summary>
    [JsonIgnore]
    public EncounterDefinition? PendingEncounter { get; set; }

    /// <summary>
    /// Resets the game state to initial values for a new session.
    /// </summary>
    public void Reset()
    {
        Phase = GamePhase.MainMenu;
        CurrentCharacter = null;
        TurnCount = 0;
        IsSessionActive = false;
        PendingAction = PendingGameAction.None;
        CurrentRoomId = null;
        CombatState = null;
        PendingEncounter = null;
    }
}
