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

    // v0.13: Persistent World State - Turn tracking for chronological ordering
    public int TurnNumber { get; set; }

    // v0.8: NPC & Quest System
    public NPCService NPCService { get; set; }
    public DialogueService DialogueService { get; set; }
    public QuestService QuestService { get; set; }

    public GameState()
    {
        Player = new PlayerCharacter();
        World = new GameWorld();
        CurrentRoom = World.GetStartRoom();
        CurrentPhase = GamePhase.CharacterCreation;
        Combat = null;
        WorldState = new WorldState();

        // v0.8: Initialize NPC & Quest services
        NPCService = new NPCService();
        DialogueService = new DialogueService();
        QuestService = new QuestService(); // Will be updated with CurrencyService later (v0.9)
    }

    // v0.9: Set CurrencyService after GameState initialization
    public void SetCurrencyService(CurrencyService currencyService)
    {
        // Recreate QuestService with CurrencyService
        QuestService = new QuestService("Data/Quests", currencyService);
    }

    // v0.35: Set TerritoryService after GameState initialization
    public void SetTerritoryService(CurrencyService? currencyService, TerritoryService? territoryService)
    {
        // Recreate QuestService with both CurrencyService and TerritoryService
        QuestService = new QuestService("Data/Quests", currencyService, territoryService);
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
        // [v0.4] Don't auto-trigger combat for rooms with talkable NPCs
        if (CurrentRoom.HasTalkableNPC && !CurrentRoom.HasTalkedToNPC)
        {
            return false;
        }

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

        // [v0.4] Disable environmental hazards if present
        if (CurrentRoom.HasEnvironmentalHazard)
        {
            CurrentRoom.IsHazardActive = false;
        }

        // [v0.4] Room-specific puzzle effects
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
