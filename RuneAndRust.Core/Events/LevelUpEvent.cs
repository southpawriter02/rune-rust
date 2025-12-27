namespace RuneAndRust.Core.Events;

/// <summary>
/// Published when a character reaches a new level.
/// Consumed by UI listeners to display level-up notifications.
/// </summary>
/// <remarks>See: v0.4.0a (The Legend) for Saga system implementation.</remarks>
/// <param name="CharacterId">The unique identifier of the character.</param>
/// <param name="CharacterName">The display name of the character.</param>
/// <param name="NewLevel">The level achieved.</param>
/// <param name="ProgressionPointsAwarded">The PP awarded for reaching this level.</param>
public record LevelUpEvent(
    Guid CharacterId,
    string CharacterName,
    int NewLevel,
    int ProgressionPointsAwarded);
