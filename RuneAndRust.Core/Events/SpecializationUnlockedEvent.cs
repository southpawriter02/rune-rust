namespace RuneAndRust.Core.Events;

/// <summary>
/// Published when a character unlocks a new specialization.
/// Consumed by UI listeners to display specialization unlock notifications.
/// </summary>
/// <remarks>See: v0.4.1b (The Unlock) for implementation.</remarks>
/// <param name="CharacterId">The unique identifier of the character.</param>
/// <param name="CharacterName">The display name of the character.</param>
/// <param name="SpecializationId">The unique identifier of the unlocked specialization.</param>
/// <param name="SpecializationName">The display name of the unlocked specialization.</param>
/// <param name="ProgressionPointsSpent">The PP spent to unlock this specialization (typically 10).</param>
public record SpecializationUnlockedEvent(
    Guid CharacterId,
    string CharacterName,
    Guid SpecializationId,
    string SpecializationName,
    int ProgressionPointsSpent);
