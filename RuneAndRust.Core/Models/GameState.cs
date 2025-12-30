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
    /// Tracks which rooms the player has visited for Fog of War (v0.3.5b).
    /// Used by MinimapRenderer to determine visibility.
    /// </summary>
    public HashSet<Guid> VisitedRoomIds { get; set; } = new();

    /// <summary>
    /// User-defined notes for rooms, keyed by RoomId (v0.3.20a).
    /// Displayed on minimap with '!' symbol.
    /// </summary>
    public Dictionary<Guid, string> UserNotes { get; set; } = new();

    /// <summary>
    /// Game flags for tracking progress and state (v0.4.2b).
    /// Used by dialogue conditions and quest tracking.
    /// </summary>
    public Dictionary<string, bool> Flags { get; set; } = new();

    /// <summary>
    /// Gets a game flag value (v0.4.2b).
    /// Returns false if the flag doesn't exist.
    /// </summary>
    /// <param name="key">The flag key to check.</param>
    /// <returns>The flag value, or false if not set.</returns>
    public bool GetFlag(string key) => Flags.TryGetValue(key, out var val) && val;

    /// <summary>
    /// Sets a game flag value (v0.4.2b).
    /// </summary>
    /// <param name="key">The flag key to set.</param>
    /// <param name="value">The value to set.</param>
    public void SetFlag(string key, bool value) => Flags[key] = value;

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
    /// Gets or sets whether God Mode is active (v0.3.17b).
    /// When true, player takes no damage, stress, or corruption.
    /// Not serialized to save files.
    /// </summary>
    [JsonIgnore]
    public bool IsGodMode { get; set; } = false;

    /// <summary>
    /// Gets or sets the current dialogue session (v0.4.2c).
    /// Null when not in dialogue.
    /// Not serialized to save files.
    /// </summary>
    [JsonIgnore]
    public DialogueSession? CurrentDialogueSession { get; set; }

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
        VisitedRoomIds.Clear();
        UserNotes.Clear();
        Flags.Clear();
        CombatState = null;
        PendingEncounter = null;
        IsGodMode = false;
        CurrentDialogueSession = null;
    }
}
