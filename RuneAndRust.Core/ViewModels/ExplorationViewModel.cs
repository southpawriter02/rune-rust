namespace RuneAndRust.Core.ViewModels;

/// <summary>
/// Immutable view model for the exploration screen HUD (v0.3.5a).
/// Contains all display data for the persistent three-pane interface.
/// </summary>
/// <param name="CharacterName">The player character's display name.</param>
/// <param name="CurrentHp">Current hit points.</param>
/// <param name="MaxHp">Maximum hit points.</param>
/// <param name="CurrentStamina">Current stamina points.</param>
/// <param name="MaxStamina">Maximum stamina points.</param>
/// <param name="CurrentStress">Current psychic stress (0-100).</param>
/// <param name="MaxStress">Maximum stress threshold (always 100).</param>
/// <param name="CurrentCorruption">Current Runic Blight corruption (0-100).</param>
/// <param name="MaxCorruption">Maximum corruption threshold (always 100).</param>
/// <param name="RoomName">Current room's display name.</param>
/// <param name="RoomDescription">Current room's narrative description.</param>
/// <param name="TurnCount">Current exploration turn number.</param>
public record ExplorationViewModel(
    string CharacterName,
    int CurrentHp,
    int MaxHp,
    int CurrentStamina,
    int MaxStamina,
    int CurrentStress,
    int MaxStress,
    int CurrentCorruption,
    int MaxCorruption,
    string RoomName,
    string RoomDescription,
    int TurnCount
);
