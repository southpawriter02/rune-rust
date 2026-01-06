using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

namespace RuneAndRust.Domain.Entities;

public class GameSession : IEntity
{
    public Guid Id { get; private set; }
    public Player Player { get; private set; }
    public Dungeon Dungeon { get; private set; }
    public Guid CurrentRoomId { get; private set; }
    public GameState State { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastPlayedAt { get; private set; }

    private readonly HashSet<string> _revealedSolutionIds = [];

    public Room? CurrentRoom => Dungeon.GetRoom(CurrentRoomId);
    public IReadOnlySet<string> RevealedSolutions => _revealedSolutionIds;

    private GameSession()
    {
        Player = null!;
        Dungeon = null!;
    } // For EF Core

    private GameSession(Player player, Dungeon dungeon)
    {
        Id = Guid.NewGuid();
        Player = player ?? throw new ArgumentNullException(nameof(player));
        Dungeon = dungeon ?? throw new ArgumentNullException(nameof(dungeon));
        CurrentRoomId = dungeon.StartingRoomId;
        State = GameState.Playing;
        CreatedAt = DateTime.UtcNow;
        LastPlayedAt = DateTime.UtcNow;
    }

    public static GameSession CreateNew(string playerName)
    {
        var player = new Player(playerName);
        var dungeon = Dungeon.CreateStarterDungeon();
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

        CurrentRoomId = nextRoomId.Value;
        var nextRoom = CurrentRoom;
        if (nextRoom != null)
        {
            Player.MoveTo(nextRoom.Position);
        }

        UpdateLastPlayed();
        return true;
    }

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

    public void SetState(GameState newState)
    {
        State = newState;
        UpdateLastPlayed();
    }

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

    public override string ToString() => $"Session: {Player.Name} - {State}";
}
