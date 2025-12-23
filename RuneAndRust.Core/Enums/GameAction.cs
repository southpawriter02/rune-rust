namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines bindable game actions for input configuration (v0.3.9c).
/// Used by InputConfigurationService to map keys to commands.
/// </summary>
public enum GameAction
{
    #region Movement Actions

    /// <summary>
    /// Move north (default: N).
    /// </summary>
    MoveNorth = 0,

    /// <summary>
    /// Move south (default: S).
    /// </summary>
    MoveSouth = 1,

    /// <summary>
    /// Move east (default: E).
    /// </summary>
    MoveEast = 2,

    /// <summary>
    /// Move west (default: W).
    /// </summary>
    MoveWest = 3,

    /// <summary>
    /// Move up (default: U).
    /// </summary>
    MoveUp = 4,

    /// <summary>
    /// Move down (default: D).
    /// </summary>
    MoveDown = 5,

    #endregion

    #region Core Actions

    /// <summary>
    /// Confirm selection (default: Enter).
    /// </summary>
    Confirm = 10,

    /// <summary>
    /// Cancel/back (default: Escape).
    /// </summary>
    Cancel = 11,

    /// <summary>
    /// Open main menu (default: M).
    /// </summary>
    Menu = 12,

    /// <summary>
    /// Display help (default: H or ?).
    /// </summary>
    Help = 13,

    #endregion

    #region Screen Navigation

    /// <summary>
    /// Open inventory screen (default: I).
    /// </summary>
    Inventory = 20,

    /// <summary>
    /// Open character screen (default: C).
    /// </summary>
    Character = 21,

    /// <summary>
    /// Open journal screen (default: J).
    /// </summary>
    Journal = 22,

    /// <summary>
    /// Open crafting screen (default: B for Bench).
    /// </summary>
    Crafting = 23,

    #endregion

    #region Gameplay Actions

    /// <summary>
    /// Interact with object/NPC (default: F).
    /// </summary>
    Interact = 30,

    /// <summary>
    /// Look at surroundings (default: L).
    /// </summary>
    Look = 31,

    /// <summary>
    /// Search the room (default: X).
    /// </summary>
    Search = 32,

    /// <summary>
    /// Wait/pass turn (default: Spacebar).
    /// </summary>
    Wait = 33,

    #endregion

    #region Combat Actions

    /// <summary>
    /// Standard attack (default: A).
    /// </summary>
    Attack = 40,

    /// <summary>
    /// Light/quick attack (default: Q).
    /// </summary>
    LightAttack = 41,

    /// <summary>
    /// Heavy/power attack (default: R).
    /// </summary>
    HeavyAttack = 42,

    /// <summary>
    /// Use ability (default: 1-9 for hotkeys).
    /// </summary>
    UseAbility = 43,

    /// <summary>
    /// Flee from combat (default: F).
    /// </summary>
    Flee = 44

    #endregion
}
