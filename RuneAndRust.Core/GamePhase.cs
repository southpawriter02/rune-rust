namespace RuneAndRust.Core;

/// <summary>
/// v0.44.1: Represents the current phase of the game.
/// Used by GameStateController to manage state transitions.
/// </summary>
public enum GamePhase
{
    /// <summary>At the main menu, no active game</summary>
    MainMenu,

    /// <summary>Creating a new character</summary>
    CharacterCreation,

    /// <summary>Exploring the dungeon between combat encounters</summary>
    DungeonExploration,

    /// <summary>In active combat</summary>
    Combat,

    /// <summary>Collecting loot after combat victory</summary>
    LootCollection,

    /// <summary>Allocating progression points after level up</summary>
    CharacterProgression,

    /// <summary>Player has died, showing death screen</summary>
    Death,

    /// <summary>Player has won, showing victory screen</summary>
    Victory,

    /// <summary>Selecting endgame mode (NG+, Challenge Sectors, etc.)</summary>
    EndgameMenu
}
