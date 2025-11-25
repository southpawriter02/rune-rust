using RuneAndRust.Core.ChallengeSectors;

namespace RuneAndRust.Core;

/// <summary>
/// v0.44.1: Master state container for an active game session.
/// This is the authoritative source of truth for game state.
/// UI state is derived from this; GameState is always saveable/loadable.
/// </summary>
public class GameState
{
    /// <summary>Unique identifier for this game session</summary>
    public Guid SessionId { get; set; }

    /// <summary>When this session was started</summary>
    public DateTime SessionStarted { get; set; }

    /// <summary>Current game phase</summary>
    public GamePhase CurrentPhase { get; set; }

    /// <summary>Previous game phase (for back navigation)</summary>
    public GamePhase? PreviousPhase { get; set; }

    /// <summary>The player character (null during CharacterCreation until confirmed)</summary>
    public PlayerCharacter? Player { get; set; }

    /// <summary>Current dungeon graph structure</summary>
    public DungeonGraph? CurrentDungeon { get; set; }

    /// <summary>Current room the player is in</summary>
    public Room? CurrentRoom { get; set; }

    /// <summary>Current combat state (null when not in combat)</summary>
    public CombatState? CurrentCombat { get; set; }

    /// <summary>Current NG+ tier (null for first playthrough)</summary>
    public int? CurrentNGPlusTier { get; set; }

    /// <summary>Current Challenge Sector (null if not in challenge mode)</summary>
    public ChallengeSector? CurrentChallengeSector { get; set; }

    /// <summary>Total play time for this session</summary>
    public TimeSpan PlayTime { get; set; }

    /// <summary>Run number (increments each new game)</summary>
    public int RunNumber { get; set; }

    /// <summary>When the game was last saved</summary>
    public DateTime? LastSaved { get; set; }

    /// <summary>Whether auto-save is enabled for this session</summary>
    public bool AutoSaveEnabled { get; set; } = true;

    /// <summary>
    /// Validates that the game state is internally consistent.
    /// Throws InvalidOperationException if validation fails.
    /// </summary>
    public void Validate()
    {
        // Player required after character creation
        if (CurrentPhase != GamePhase.MainMenu &&
            CurrentPhase != GamePhase.CharacterCreation &&
            Player == null)
        {
            throw new InvalidOperationException("GameState must have a Player after character creation");
        }

        // Dungeon required during exploration
        if (CurrentPhase == GamePhase.DungeonExploration && CurrentDungeon == null)
        {
            throw new InvalidOperationException("Cannot be in exploration without a dungeon");
        }

        // Room required during exploration
        if (CurrentPhase == GamePhase.DungeonExploration && CurrentRoom == null)
        {
            throw new InvalidOperationException("Cannot be in exploration without a current room");
        }

        // Combat state required during combat
        if (CurrentPhase == GamePhase.Combat && CurrentCombat == null)
        {
            throw new InvalidOperationException("Cannot be in combat without combat state");
        }
    }

    /// <summary>
    /// Creates a snapshot of the current game state for saving or recovery.
    /// </summary>
    public GameStateSnapshot CreateSnapshot()
    {
        return new GameStateSnapshot
        {
            SessionId = SessionId,
            Timestamp = DateTime.UtcNow,
            Phase = CurrentPhase,
            PlayerSnapshot = Player?.Clone(),
            PlayTime = PlayTime,
            RunNumber = RunNumber,
            CurrentRoomId = CurrentRoom?.RoomId,
            NGPlusTier = CurrentNGPlusTier,
            ChallengeSectorId = CurrentChallengeSector?.SectorId
        };
    }

    /// <summary>
    /// Resets the game state to initial values (for returning to main menu).
    /// </summary>
    public void Reset()
    {
        CurrentPhase = GamePhase.MainMenu;
        PreviousPhase = null;
        Player = null;
        CurrentDungeon = null;
        CurrentRoom = null;
        CurrentCombat = null;
        CurrentNGPlusTier = null;
        CurrentChallengeSector = null;
    }

    /// <summary>
    /// Creates a new game state for starting a new game.
    /// </summary>
    public static GameState CreateNew(int runNumber = 1)
    {
        return new GameState
        {
            SessionId = Guid.NewGuid(),
            SessionStarted = DateTime.UtcNow,
            CurrentPhase = GamePhase.CharacterCreation,
            PlayTime = TimeSpan.Zero,
            RunNumber = runNumber,
            AutoSaveEnabled = true
        };
    }
}

/// <summary>
/// v0.44.1: Snapshot of game state for saving/recovery.
/// Lighter weight than full GameState for quick saves.
/// </summary>
public class GameStateSnapshot
{
    public Guid SessionId { get; set; }
    public DateTime Timestamp { get; set; }
    public GamePhase Phase { get; set; }
    public PlayerCharacter? PlayerSnapshot { get; set; }
    public TimeSpan PlayTime { get; set; }
    public int RunNumber { get; set; }
    public string? CurrentRoomId { get; set; }
    public int? NGPlusTier { get; set; }
    public string? ChallengeSectorId { get; set; }
}
