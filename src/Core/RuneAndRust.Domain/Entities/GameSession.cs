using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a game session, serving as the aggregate root for a player's adventure.
/// </summary>
/// <remarks>
/// A GameSession encapsulates the complete state of a player's game, including
/// the player character, the dungeon being explored, and the current game state.
/// It manages navigation between rooms and item pickup operations.
/// </remarks>
public class GameSession : IEntity
{
    /// <summary>
    /// Gets the unique identifier for this game session.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the player character for this session.
    /// </summary>
    public Player Player { get; private set; }

    /// <summary>
    /// Gets the dungeon being explored in this session.
    /// </summary>
    public Dungeon Dungeon { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the room the player is currently in.
    /// </summary>
    public Guid CurrentRoomId { get; private set; }

    /// <summary>
    /// Gets the current state of the game (e.g., Playing, GameOver, Victory).
    /// </summary>
    public GameState State { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when this session was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when this session was last played or modified.
    /// </summary>
    public DateTime LastPlayedAt { get; private set; }

    /// <summary>
    /// Gets the current turn number.
    /// </summary>
    public int TurnCount { get; private set; }

    /// <summary>
    /// Gets the currently active combat encounter, if any.
    /// </summary>
    public CombatEncounter? ActiveEncounter { get; private set; }

    /// <summary>
    /// Gets whether the player is currently in combat.
    /// </summary>
    public bool IsInCombat => ActiveEncounter?.State == CombatState.Active;

    /// <summary>
    /// Gets the ID of the room the player was in before entering the current room.
    /// </summary>
    /// <remarks>
    /// Used for flee destination in combat.
    /// </remarks>
    public Guid? PreviousRoomId { get; private set; }

    /// <summary>
    /// Set of room IDs that the player has visited.
    /// </summary>
    private readonly HashSet<Guid> _visitedRooms = [];

    /// <summary>
    /// Gets a read-only set of visited room IDs.
    /// </summary>
    public IReadOnlySet<Guid> VisitedRooms => _visitedRooms;

    /// <summary>
    /// Gets the current room the player is in, or null if the room cannot be found.
    /// </summary>
    public Room? CurrentRoom => Dungeon.GetRoom(CurrentRoomId);

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core.
    /// </summary>
    private GameSession()
    {
        Player = null!;
        Dungeon = null!;
    }

    /// <summary>
    /// Creates a new game session with the specified player and dungeon.
    /// </summary>
    /// <param name="player">The player character for this session.</param>
    /// <param name="dungeon">The dungeon to explore.</param>
    /// <exception cref="ArgumentNullException">Thrown when player or dungeon is null.</exception>
    private GameSession(Player player, Dungeon dungeon)
    {
        Id = Guid.NewGuid();
        Player = player ?? throw new ArgumentNullException(nameof(player));
        Dungeon = dungeon ?? throw new ArgumentNullException(nameof(dungeon));
        CurrentRoomId = dungeon.StartingRoomId;
        State = GameState.Playing;
        CreatedAt = DateTime.UtcNow;
        LastPlayedAt = DateTime.UtcNow;
        _visitedRooms = [dungeon.StartingRoomId]; // Mark starting room as visited
    }

    /// <summary>
    /// Factory method to create a new game session with a fresh player and starter dungeon.
    /// </summary>
    /// <param name="playerName">The name for the new player character.</param>
    /// <returns>A new <see cref="GameSession"/> ready for play.</returns>
    public static GameSession CreateNew(string playerName)
    {
        var player = new Player(playerName);
        var dungeon = Dungeon.CreateStarterDungeon();
        return new GameSession(player, dungeon);
    }

    /// <summary>
    /// Starts a combat encounter.
    /// </summary>
    /// <param name="encounter">The encounter to start.</param>
    /// <exception cref="InvalidOperationException">Thrown if already in combat.</exception>
    /// <exception cref="ArgumentNullException">Thrown if encounter is null.</exception>
    public void StartCombat(CombatEncounter encounter)
    {
        ArgumentNullException.ThrowIfNull(encounter);

        if (ActiveEncounter?.State == CombatState.Active)
            throw new InvalidOperationException("Already in combat.");

        ActiveEncounter = encounter;
    }

    /// <summary>
    /// Ends the current combat encounter.
    /// </summary>
    /// <remarks>
    /// Call this after combat resolves (victory, defeat, or flee).
    /// </remarks>
    public void EndCombat()
    {
        ActiveEncounter = null;
    }

    /// <summary>
    /// Attempts to move the player in the specified direction.
    /// </summary>
    /// <param name="direction">The direction to move (North, South, East, or West).</param>
    /// <returns>
    /// <c>true</c> if the move was successful; <c>false</c> if there is no exit
    /// in that direction or the current room cannot be found.
    /// </returns>
    public bool TryMovePlayer(Direction direction)
    {
        var currentRoom = CurrentRoom;
        if (currentRoom == null)
            return false;

        var nextRoomId = currentRoom.GetExit(direction);
        if (nextRoomId == null)
            return false;

        // Track previous room for flee mechanic
        PreviousRoomId = CurrentRoomId;

        CurrentRoomId = nextRoomId.Value;
        var nextRoom = CurrentRoom;
        if (nextRoom != null)
        {
            Player.MoveTo(nextRoom.Position);
        }

        UpdateLastPlayed();
        return true;
    }

    /// <summary>
    /// Attempts to pick up an item from the current room by name.
    /// </summary>
    /// <param name="itemName">The name of the item to pick up (case-insensitive).</param>
    /// <returns>
    /// <c>true</c> if the item was successfully picked up; <c>false</c> if the item
    /// was not found, the player's inventory is full, or the current room cannot be found.
    /// </returns>
    public bool TryPickUpItem(string itemName)
    {
        var currentRoom = CurrentRoom;
        if (currentRoom == null)
            return false;

        var item = currentRoom.GetItemByName(itemName);
        if (item == null)
            return false;

        if (!Player.TryPickUpItem(item))
            return false;

        currentRoom.RemoveItem(item);
        UpdateLastPlayed();
        return true;
    }

    /// <summary>
    /// Marks a room as visited.
    /// </summary>
    /// <param name="roomId">The ID of the room to mark as visited.</param>
    public void MarkRoomVisited(Guid roomId)
    {
        _visitedRooms.Add(roomId);
    }

    /// <summary>
    /// Checks if a room has been visited before.
    /// </summary>
    /// <param name="roomId">The ID of the room to check.</param>
    /// <returns>True if the room has been visited; otherwise, false.</returns>
    public bool HasVisitedRoom(Guid roomId) => _visitedRooms.Contains(roomId);

    /// <summary>
    /// Sets the game state and updates the last played timestamp.
    /// </summary>
    /// <param name="newState">The new game state to set.</param>
    public void SetState(GameState newState)
    {
        State = newState;
        UpdateLastPlayed();
    }

    /// <summary>
    /// Updates the last played timestamp to the current UTC time.
    /// </summary>
    public void UpdateLastPlayed()
    {
        LastPlayedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Advances the turn counter by one.
    /// </summary>
    /// <returns>The new turn count after advancing.</returns>
    public int AdvanceTurn()
    {
        TurnCount++;
        UpdateLastPlayed();
        return TurnCount;
    }

    /// <summary>
    /// Returns a string representation of this game session.
    /// </summary>
    /// <returns>A string containing the player name and game state.</returns>
    public override string ToString() => $"Session: {Player.Name} - {State}";
}
