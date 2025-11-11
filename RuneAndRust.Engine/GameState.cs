using RuneAndRust.Core;

namespace RuneAndRust.Engine;

public enum GamePhase
{
    CharacterCreation,
    Exploration,
    Combat,
    Puzzle,
    Victory,
    GameOver
}

public class GameState
{
    public PlayerCharacter Player { get; set; }
    public GameWorld World { get; set; }
    public Room CurrentRoom { get; set; }
    public GamePhase CurrentPhase { get; set; }
    public CombatState? Combat { get; set; }
    public WorldState WorldState { get; set; }

    public GameState()
    {
        Player = new PlayerCharacter();
        World = new GameWorld();
        CurrentRoom = World.GetStartRoom();
        CurrentPhase = GamePhase.CharacterCreation;
        Combat = null;
        WorldState = new WorldState();
    }

    public void MoveToRoom(string direction)
    {
        if (CurrentRoom.Exits.TryGetValue(direction.ToLower(), out var nextRoomName))
        {
            CurrentRoom = World.GetRoom(nextRoomName);
        }
        else
        {
            throw new InvalidOperationException($"Cannot go {direction} from here.");
        }
    }

    public bool CanMove(string direction)
    {
        return CurrentRoom.Exits.ContainsKey(direction.ToLower());
    }

    public List<string> GetAvailableDirections()
    {
        return CurrentRoom.Exits.Keys.ToList();
    }

    public bool ShouldTriggerCombat()
    {
        return !CurrentRoom.HasBeenCleared &&
               CurrentRoom.Enemies.Count > 0 &&
               CurrentPhase == GamePhase.Exploration;
    }

    public bool ShouldShowPuzzle()
    {
        return CurrentRoom.HasPuzzle &&
               !CurrentRoom.IsPuzzleSolved &&
               CurrentPhase == GamePhase.Exploration;
    }

    public void ClearCurrentRoom()
    {
        CurrentRoom.HasBeenCleared = true;
        WorldState.MarkRoomCleared(CurrentRoom.Id);
    }

    public void SolvePuzzle()
    {
        CurrentRoom.IsPuzzleSolved = true;
        WorldState.PuzzleSolved = true;

        // [v0.4] In the new system, most puzzles just give rewards
        // Only the secret room puzzle unlocks a door
        if (CurrentRoom.Name == "Vault Corridor")
        {
            World.UnlockSecretRoom();
        }
    }

    public void UpdateWorldState()
    {
        // Update world state based on current room
        WorldState.CurrentRoomId = CurrentRoom.Id;

        // Check for boss defeated
        if (CurrentRoom.IsBossRoom && CurrentRoom.HasBeenCleared)
        {
            WorldState.BossDefeated = true;
        }
    }
}
