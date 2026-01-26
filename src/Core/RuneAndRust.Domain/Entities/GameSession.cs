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
    /// Gets the ID of the previous room the player was in.
    /// </summary>
    public Guid? PreviousRoomId { get; private set; }

    /// <summary>
    /// Gets the current turn count.
    /// </summary>
    public int TurnCount { get; private set; }

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

    private readonly HashSet<string> _revealedSolutionIds = [];
    private readonly HashSet<Guid> _visitedRooms = [];

    public Room? CurrentRoom => Dungeon.GetRoom(CurrentRoomId);
    public IReadOnlySet<string> RevealedSolutions => _revealedSolutionIds;

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
    /// Creates a new game session with a procedurally generated dungeon.
    /// </summary>
    public static GameSession CreateWithDungeon(string playerName, Dungeon dungeon)
    {
        var player = new Player(playerName);
        return new GameSession(player, dungeon);
    }

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
    /// Records that a puzzle solution has been revealed to the player.
    /// </summary>
    public void RevealSolution(string solutionId)
    {
        if (!string.IsNullOrWhiteSpace(solutionId))
        {
            _revealedSolutionIds.Add(solutionId);
            UpdateLastPlayed();
        }
    }

    /// <summary>
    /// Checks if a particular puzzle solution has been revealed.
    /// </summary>
    public bool HasRevealedSolution(string solutionId) =>
        !string.IsNullOrWhiteSpace(solutionId) && _revealedSolutionIds.Contains(solutionId);

    /// <summary>
    /// Advances the game turn count.
    /// </summary>
    /// <returns>The new turn count.</returns>
    public int AdvanceTurn()
    {
        TurnCount++;
        UpdateLastPlayed();
        return TurnCount;
    }

    public override string ToString() => $"Session: {Player.Name} - {State}";
}
